using Npgsql;

using Hash_util;
using System.Runtime.InteropServices;

namespace Auth_util;

public static class Auth
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static bool Auth_User(string token)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        string checkQuery = "SELECT EXISTS(SELECT 1 FROM users WHERE token = @token)"; // AND token_created_at > NOW() - INTERVAL '168 hours'
        using var checkCmd = new NpgsqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@token", token);

        var result = checkCmd.ExecuteScalar();
        bool AuthValid = result != null && (bool)result;

        Console.WriteLine("Auth func: Login successfull");
        return AuthValid;
    }
}
