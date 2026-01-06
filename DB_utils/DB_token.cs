namespace DBTokenValid;

using Npgsql;
using DBConnection;

public class DB_token
{
    public static async Task<bool> ValidateTokenDB(string token)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        using var checkCmd = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM users WHERE token = @token AND token_created_at > NOW() - INTERVAL '72 hours');", conn);
        checkCmd.Parameters.AddWithValue("token", token);

        bool exists = (bool?)await checkCmd.ExecuteScalarAsync() ?? false;

        Console.WriteLine($"Token validation in DB: {exists}"); 
        Console.WriteLine($"Token checked: {token}");

        if (!exists)
        {   
            string updateQuery = "UPDATE users SET token_created_At = NULL, token = NULL WHERE token = @token;";
            using var Cmd = new NpgsqlCommand(updateQuery, conn);
            Cmd.Parameters.AddWithValue("@token", token);

            await Cmd.ExecuteNonQueryAsync();
            return false;
        }

        return true;
    }   
}