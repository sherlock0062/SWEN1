using System.Text;
using System.Text.Json;
using Npgsql;
using SWEN1.Helpers;
using SWEN1.Models;

namespace SWEN1.Controllers;

public static class CardController
{
    public static (int, string, string) CreatePackage(string token, string body, string connString)
    {
        if (token != "admin-mtcgToken") return ResponseHelper.ErrorResponse(403, "Only admin can create packages");
        var cards = JsonSerializer.Deserialize<List<Card>>(body);
        if (cards == null || cards.Count != 5) return ResponseHelper.ErrorResponse(400, "Package must contain exactly 5 cards");
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            int packageId = Convert.ToInt32(DbHelpers.ExecuteScalar(
                conn,
                "INSERT INTO packages(isacquired) VALUES(false) RETURNING id",
                null,
                tran
            ) ?? 0);
            foreach (var card in cards)
            {
                string elementType = "Normal";
                if (card.Name.Contains("Water")) elementType = "Water";
                else if (card.Name.Contains("Fire")) elementType = "Fire";
                string cardType = card.Name.Contains("Spell") ? "Spell" : "Monster";
                DbHelpers.ExecuteNonQuery(conn, "DELETE FROM cards WHERE cardid=@cid", cmd =>
                {
                    cmd.Parameters.AddWithValue("cid", Guid.Parse(card.Id));
                }, tran);
                DbHelpers.ExecuteNonQuery(conn,
                    "INSERT INTO cards (cardid, name, damage, elementtype, cardtype) VALUES(@cid, @n, @d, @e, @ct)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("cid", Guid.Parse(card.Id));
                        cmd.Parameters.AddWithValue("n", card.Name);
                        cmd.Parameters.AddWithValue("d", card.Damage);
                        cmd.Parameters.AddWithValue("e", elementType);
                        cmd.Parameters.AddWithValue("ct", cardType);
                    },
                    tran
                );
                DbHelpers.ExecuteNonQuery(conn,
                    "INSERT INTO package_cards(package_id, cardid) VALUES(@pid, @cid)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("pid", packageId);
                        cmd.Parameters.AddWithValue("cid", Guid.Parse(card.Id));
                    },
                    tran
                );
            }
            tran.Commit();
            return ResponseHelper.JsonResponse(201, new { message = "Package created successfully" });
        }
        catch
        {
            tran.Rollback();
            return ResponseHelper.ErrorResponse(400, "Invalid package data");
        }
    }

    public static (int, string, string) AcquirePackage(string username, string connString)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            int coins = Convert.ToInt32(DbHelpers.ExecuteScalar(
                conn,
                "SELECT coins FROM users WHERE username=@u",
                cmd => cmd.Parameters.AddWithValue("u", username),
                tran
            ) ?? 0);
            if (coins < 5) return ResponseHelper.ErrorResponse(403, "Not enough coins");
            var pidObj = DbHelpers.ExecuteScalar(
                conn,
                "SELECT id FROM packages WHERE isacquired=false ORDER BY id LIMIT 1",
                null,
                tran
            );
            if (pidObj == null) return ResponseHelper.ErrorResponse(404, "No packages available");
            int packageId = Convert.ToInt32(pidObj);
            DbHelpers.ExecuteNonQuery(
                conn,
                "UPDATE packages SET isacquired=true WHERE id=@p",
                cmd => cmd.Parameters.AddWithValue("p", packageId),
                tran
            );
            DbHelpers.ExecuteNonQuery(
                conn,
                "UPDATE cards SET ownerid= @u WHERE cardid IN (SELECT cardid FROM package_cards WHERE package_id=@p)",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("u", username);
                    cmd.Parameters.AddWithValue("p", packageId);
                },
                tran
            );
            DbHelpers.ExecuteNonQuery(
                conn,
                "UPDATE users SET coins=coins-5 WHERE username=@u",
                cmd => cmd.Parameters.AddWithValue("u", username),
                tran
            );
            tran.Commit();
            return ResponseHelper.JsonResponse(201, new { message = "Package acquired" });
        }
        catch
        {
            tran.Rollback();
            return ResponseHelper.ErrorResponse(500, "Failed to acquire package");
        }
    }

    public static (int, string, string) GetUserCards(string username, string connString)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        var sql = "SELECT cardid,name,damage FROM cards WHERE ownerid=@u";
        var cards = new List<object>();
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("u", username);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            cards.Add(new
            {
                Id = r.GetGuid(0).ToString(),
                Name = r.GetString(1),
                Damage = r.GetDecimal(2)
            });
        }
        return ResponseHelper.JsonResponse(200, cards);
    }

    public static (int, string, string) GetDeck(string username, string query, string connString)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        var cards = new List<object>();
        using var cmd = new NpgsqlCommand(
            @"SELECT c.cardid,c.name,c.damage
                  FROM cards c
                  JOIN deck_cards d ON c.cardid=d.cardid
                  WHERE d.username=@u", conn);
        cmd.Parameters.AddWithValue("u", username);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            cards.Add(new
            {
                Id = r.GetGuid(0).ToString(),
                Name = r.GetString(1),
                Damage = r.GetDecimal(2)
            });
        }
        if (query.Contains("format=plain"))
        {
            var sb = new StringBuilder();
            foreach (var c in cards)
            {
                sb.AppendLine(JsonSerializer.Serialize(c));
            }
            return (200, "text/plain", sb.ToString());
        }
        return ResponseHelper.JsonResponse(200, cards);
    }

    public static (int, string, string) ConfigureDeck(string username, string body, string connString)
    {
        var cardIds = JsonSerializer.Deserialize<List<string>>(body);
        if (cardIds == null || cardIds.Count != 4) return ResponseHelper.ErrorResponse(400, "Deck must contain exactly 4 cards");
        var guids = cardIds.Select(Guid.Parse).ToArray();
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            int count = Convert.ToInt32(DbHelpers.ExecuteScalar(
                conn,
                "SELECT COUNT(*) FROM cards WHERE cardid=ANY(@ids) AND ownerid=@u",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("ids", guids);
                    cmd.Parameters.AddWithValue("u", username);
                },
                tran
            ) ?? 0);
            if (count != 4) return ResponseHelper.ErrorResponse(403, "At least one card is not owned by the user");
            DbHelpers.ExecuteNonQuery(
                conn,
                "DELETE FROM deck_cards WHERE username=@u",
                cmd => cmd.Parameters.AddWithValue("u", username),
                tran
            );
            foreach (var cid in guids)
            {
                DbHelpers.ExecuteNonQuery(
                    conn,
                    "INSERT INTO deck_cards (username, cardid) VALUES (@u,@c)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("u", username);
                        cmd.Parameters.AddWithValue("c", cid);
                    },
                    tran
                );
            }
            tran.Commit();
            return ResponseHelper.JsonResponse(200, new { message = "Deck configured successfully" });
        }
        catch
        {
            tran.Rollback();
            return ResponseHelper.ErrorResponse(400, "Invalid card data");
        }
    }
}