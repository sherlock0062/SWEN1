using System.Text.Json;
using Npgsql;
using SWEN1.Helpers;
using SWEN1.Models;

namespace SWEN1.Controllers;

public static class UserController
{
    public static (int, string, string) RegisterUser(string body, string connString)
    {
        var userData = JsonSerializer.Deserialize<UserCredentials>(body);
        if (string.IsNullOrEmpty(userData?.Username) || string.IsNullOrEmpty(userData.Password))
            return ResponseHelper.ErrorResponse(400, "Username and password are required");
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        int count = Convert.ToInt32(DbHelpers.ExecuteScalar(
            conn,
            "SELECT COUNT(*) FROM users WHERE username=@u",
            cmd => cmd.Parameters.AddWithValue("u", userData.Username)
        ) ?? 0);
        if (count > 0) return ResponseHelper.ErrorResponse(409, "User already exists");
        DbHelpers.ExecuteNonQuery(
            conn,
            "INSERT INTO users (username, password, coins, elo, wins, losses) VALUES(@u, @p, 20, 100, 0, 0)",
            cmd =>
            {
                cmd.Parameters.AddWithValue("u", userData.Username);
                cmd.Parameters.AddWithValue("p", userData.Password);
            }
        );
        return ResponseHelper.JsonResponse(201, new { message = "User successfully created" });
    }

    public static (int, string, string) LoginUser(string body, string connString)
    {
        var userData = JsonSerializer.Deserialize<UserCredentials>(body);
        if (string.IsNullOrEmpty(userData?.Username) || string.IsNullOrEmpty(userData.Password))
            return ResponseHelper.ErrorResponse(400, "Username and password are required");
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        int count = Convert.ToInt32(DbHelpers.ExecuteScalar(
            conn,
            "SELECT COUNT(*) FROM users WHERE username=@u AND password=@p",
            cmd =>
            {
                cmd.Parameters.AddWithValue("u", userData.Username);
                cmd.Parameters.AddWithValue("p", userData.Password);
            }
        ) ?? 0);
        if (count == 0) return ResponseHelper.ErrorResponse(401, "Invalid username/password");
        string token = $"{userData.Username}-mtcgToken";
        DbHelpers.ExecuteNonQuery(
            conn,
            "UPDATE users SET token= @t WHERE username=@u",
            cmd =>
            {
                cmd.Parameters.AddWithValue("t", token);
                cmd.Parameters.AddWithValue("u", userData.Username);
            }
        );
        return (200, "application/json", JsonSerializer.Serialize(token));
    }

    public static (int, string, string) GetUserProfile(string currentUser, string requestedUser, string connString)
    {
        if (currentUser != requestedUser && currentUser != "admin")
            return ResponseHelper.ErrorResponse(403, "Access denied");
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        var sql = "SELECT username, name, bio, image, coins, elo FROM users WHERE username=@u";
        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("u", requestedUser);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return ResponseHelper.ErrorResponse(404, "User not found");
        var profile = new
        {
            Username = reader.GetString(0),
            Name = reader.IsDBNull(1) ? null : reader.GetString(1),
            Bio = reader.IsDBNull(2) ? null : reader.GetString(2),
            Image = reader.IsDBNull(3) ? null : reader.GetString(3),
            Coins = reader.GetInt32(4),
            ELO = reader.GetInt32(5)
        };
        return ResponseHelper.JsonResponse(200, profile);
    }

    public static (int, string, string) UpdateUserProfile(string currentUser, string requestedUser, string body, string connString)
    {
        if (currentUser != requestedUser)
            return ResponseHelper.ErrorResponse(403, "Access denied");
        var profileData = JsonSerializer.Deserialize<UserProfile>(body) ?? new UserProfile();
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        int rows = DbHelpers.ExecuteNonQuery(
            conn,
            "UPDATE users SET name= @n, bio= @b, image= @i WHERE username=@u",
            cmd =>
            {
                cmd.Parameters.AddWithValue("n", (object?)profileData.Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("b", (object?)profileData.Bio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("i", (object?)profileData.Image ?? DBNull.Value);
                cmd.Parameters.AddWithValue("u", requestedUser);
            }
        );
        if (rows == 0) return ResponseHelper.ErrorResponse(404, "User not found");
        return ResponseHelper.JsonResponse(200, new { message = "User updated" });
    }

    public static string? GetUsernameFromToken(string token, string connString)
    {
        if (string.IsNullOrEmpty(token)) return null;
        using var conn = new NpgsqlConnection(connString);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT username FROM users WHERE token=@t", conn);
        cmd.Parameters.AddWithValue("t", token);
        return cmd.ExecuteScalar() as string;
    }
}