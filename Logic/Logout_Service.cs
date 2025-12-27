using Npgsql;

using Hash_util;

namespace Logout_Service;

public static class LogoutService
{
    private const string ConnectionString = @"
        Host=localhost;
        Port=5432;
        Database=mrp_db;
        Username=admin;
        Password=mrp123;"
    ;
    public static bool LogoutUserService(string token)
    {
        try
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();

            string updateQuery = "UPDATE users SET token_created_At = NULL WHERE token = @token;";
            using var Cmd = new NpgsqlCommand(updateQuery, conn);
            Cmd.Parameters.AddWithValue("@token", token);

            Cmd.ExecuteNonQuery();

            return true;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine("Database error during logout:");
            Console.WriteLine($"PostgreSQL Error [{ex.SqlState}]: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("General error during logout:");
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}