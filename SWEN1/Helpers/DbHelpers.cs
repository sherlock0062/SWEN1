using Npgsql;

namespace SWEN1.Helpers;

public static class DbHelpers
{
    public static object? ExecuteScalar(
        NpgsqlConnection conn,
        string sql,
        Action<NpgsqlCommand>? param = null,
        NpgsqlTransaction? tx = null)
    {
        using var cmd = new NpgsqlCommand(sql, conn, tx);
        param?.Invoke(cmd);
        return cmd.ExecuteScalar();
    }

    public static int ExecuteNonQuery(
        NpgsqlConnection conn,
        string sql,
        Action<NpgsqlCommand>? param = null,
        NpgsqlTransaction? tx = null)
    {
        using var cmd = new NpgsqlCommand(sql, conn, tx);
        param?.Invoke(cmd);
        return cmd.ExecuteNonQuery();
    }
}