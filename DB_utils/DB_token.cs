namespace DBTokenValid;

using Npgsql;
using DBConnection;

public class DB_token
{
    public static async Task<bool> ValidateTokenDB(string token)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        using var checkCmd = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM users WHERE token = @token);", conn);
        checkCmd.Parameters.AddWithValue("token", token);

        bool exists = (bool?)await checkCmd.ExecuteScalarAsync() ?? false;

        Console.WriteLine($"Token validation in DB: {exists}"); 
        Console.WriteLine($"Token checked: {token}");

        if (!exists)
        {
            return false;
        }

        return true;
    }   
}