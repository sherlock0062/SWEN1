using System.Text.Json;
using Npgsql;

namespace SWEN1.Controllers;

public static class StatsController
{
    public static (int, string, string) GetStats(string username, string connString)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        var sql = "SELECT elo,wins,losses FROM users WHERE username=@u";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("u", username);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return ErrorResponse(404, "User not found");
        var stats = new
        {
            Username = username,
            ELO = r.GetInt32(0),
            Wins = r.GetInt32(1),
            Losses = r.GetInt32(2)
        };
        return JsonResponse(200, stats);
    }

    public static (int, string, string) GetScoreboard(string connString)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        var sql = "SELECT username,elo,wins,losses FROM users ORDER BY elo DESC";
        using var cmd = new NpgsqlCommand(sql, conn);
        var result = new List<object>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            result.Add(new
            {
                Username = r.GetString(0),
                ELO = r.GetInt32(1),
                Wins = r.GetInt32(2),
                Losses = r.GetInt32(3)
            });
        }
        return JsonResponse(200, result);
    }

    static (int, string, string) ErrorResponse(int code, string message)
    {
        return (code, "application/json", $"{{\"HTTP:\":\"{code}\":\"error\":\"{message}\"}}");
    }

    static (int, string, string) JsonResponse(int code, object data)
    {
        return (code, "application/json", JsonSerializer.Serialize(data));
    }
}