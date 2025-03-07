using System.Text;
using Npgsql;
using SWEN1.Helpers;
using SWEN1.Models;

namespace SWEN1.Controllers;

public static class BattleController
{
    public static (int, string, string) StartBattle(
        string username,
        string connString,
        Queue<string> battleQueue,
        object battleQueueLock)
    {
        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();
            int count = Convert.ToInt32(DbHelpers.ExecuteScalar(
                conn,
                "SELECT COUNT(*) FROM deck_cards WHERE username=@u",
                cmd => cmd.Parameters.AddWithValue("u", username)
            ) ?? 0);
            if (count != 4) return ResponseHelper.ErrorResponse(400, "You need a 4-card deck to battle");
        }
        lock (battleQueueLock)
        {
            if (battleQueue.Count == 0)
            {
                battleQueue.Enqueue(username);
                return ResponseHelper.JsonResponse(200, new { message = "Waiting for opponent" });
            }
            else
            {
                var opponent = battleQueue.Dequeue();
                if (opponent == username)
                {
                    battleQueue.Enqueue(username);
                    return ResponseHelper.JsonResponse(200, new { message = "Waiting for opponent" });
                }
                var battleLog = ExecuteBattle(username, opponent, connString);
                return (200, "application/json", battleLog);
            }
        }
    }

    static string ExecuteBattle(string p1, string p2, string connString)
    {
        var deck1 = LoadPlayerDeck(p1, connString);
        var deck2 = LoadPlayerDeck(p2, connString);
        var log = new StringBuilder();
        log.AppendLine($"Battle: {p1} vs {p2}");
        int rounds = 0;
        string? winner = null;
        var rnd = new Random();
        while (deck1.Count > 0 && deck2.Count > 0 && rounds < 100)
        {
            rounds++;
            var c1Index = rnd.Next(deck1.Count);
            var c2Index = rnd.Next(deck2.Count);
            var card1 = deck1[c1Index];
            var card2 = deck2[c2Index];
            log.AppendLine($"Round {rounds}: {p1} plays {card1.Name} vs {p2} plays {card2.Name}");
            double dmg1 = CalculateDamage(card1, card2);
            double dmg2 = CalculateDamage(card2, card1);
            log.AppendLine($"Effective: {card1.Name}={dmg1}, {card2.Name}={dmg2}");
            if (dmg1 > dmg2)
            {
                log.AppendLine($"{p1} wins round");
                deck1.Add(card2);
                deck2.RemoveAt(c2Index);
            }
            else if (dmg2 > dmg1)
            {
                log.AppendLine($"{p2} wins round");
                deck2.Add(card1);
                deck1.RemoveAt(c1Index);
            }
            else
            {
                log.AppendLine("Round draw");
            }
            log.AppendLine($"Deck sizes: {p1}={deck1.Count}, {p2}={deck2.Count}");
        }
        if (deck1.Count > deck2.Count)
        {
            winner = p1;
            log.AppendLine($"{p1} wins the battle");
        }
        else if (deck2.Count > deck1.Count)
        {
            winner = p2;
            log.AppendLine($"{p2} wins the battle");
        }
        else
        {
            log.AppendLine("Battle ended in a draw");
        }
        UpdateBattleStats(p1, p2, winner, connString);
        return log.ToString();
    }

    static List<Card> LoadPlayerDeck(string username, string connString)
    {
        var deck = new List<Card>();
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        using var cmd = new NpgsqlCommand(
            @"SELECT c.cardid,c.name,c.damage,c.elementtype,c.cardtype
                  FROM cards c
                  JOIN deck_cards d ON c.cardid=d.cardid
                  WHERE d.username=@u", conn);
        cmd.Parameters.AddWithValue("u", username);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            deck.Add(new Card
            {
                Id = r.GetGuid(0).ToString(),
                Name = r.GetString(1),
                Damage = r.GetDecimal(2),
                ElementType = r.GetString(3),
                CardType = r.GetString(4)
            });
        }
        return deck;
    }

    public static double CalculateDamage(Card attacker, Card defender) // das einzige was whichtig is wer mehr schaden macht
    {
        double dmg = (double)attacker.Damage;
        if (attacker.Name.Contains("Goblin") && defender.Name.Contains("Dragon")) return 0;
        if (attacker.Name.Contains("Ork") && defender.Name.Contains("Wizzard")) return 0;
        if (attacker.Name.Contains("WaterSpell") && defender.Name.Contains("Knight")) return double.MaxValue; // instakill
        if (attacker.CardType == "Spell" && defender.Name.Contains("Kraken")) return 0;        
        if (attacker.Name.Contains("Dragon") && defender.Name.Contains("FireElf")) return 0;

        if (attacker.CardType == "Spell" || defender.CardType == "Spell")
        {
            if (attacker.ElementType == "Water" && defender.ElementType == "Fire") dmg *= 2;
            else if (attacker.ElementType == "Fire" && defender.ElementType == "Normal") dmg *= 2;
            else if (attacker.ElementType == "Normal" && defender.ElementType == "Water") dmg *= 2;
            if (attacker.ElementType == "Fire" && defender.ElementType == "Water") dmg *= 0.5;
            else if (attacker.ElementType == "Normal" && defender.ElementType == "Fire") dmg *= 0.5;
            else if (attacker.ElementType == "Water" && defender.ElementType == "Normal") dmg *= 0.5;
        }
        return dmg;
    }

    static void UpdateBattleStats(string p1, string p2, string? winner, string connString)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        using var tran = conn.BeginTransaction();
        DbHelpers.ExecuteNonQuery(
            conn,
            "INSERT INTO battles(user1id,user2id,winnerid) VALUES(@p1,@p2,@w)",
            cmd =>
            {
                cmd.Parameters.AddWithValue("p1", p1);
                cmd.Parameters.AddWithValue("p2", p2);
                cmd.Parameters.AddWithValue("w", (object?)winner ?? DBNull.Value);
            },
            tran
        );
        if (!string.IsNullOrEmpty(winner))
        {
            DbHelpers.ExecuteNonQuery(
                conn,
                "UPDATE users SET wins=wins+1, elo=elo+3 WHERE username=@u",
                cmd => cmd.Parameters.AddWithValue("u", winner),
                tran
            );
            var loser = winner == p1 ? p2 : p1;
            DbHelpers.ExecuteNonQuery(
                conn,
                "UPDATE users SET losses=losses+1, elo=GREATEST(elo-5,0) WHERE username=@u",
                cmd => cmd.Parameters.AddWithValue("u", loser),
                tran
            );
        }
        tran.Commit();
    }
}