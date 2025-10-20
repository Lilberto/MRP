using MySql.Data.MySqlClient;

public static class UserValidator
{
    private const string ConnectionString = "Server=localhost;Database=my_mrp;User Id=root;Password=root;";

    public static bool ValidateCredentials(string username, string password, string token)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        // SQL Select for identity and token verification
        string checkQuery = @"
                SELECT u.id
                FROM users u
                LEFT JOIN tokens t ON t.user_id = u.id
                WHERE u.username = @username
                  AND u.password_hash = @password
                  AND t.token = @token;";

        using var checkCmd = new MySqlCommand(checkQuery, conn);
        checkCmd.Parameters.AddWithValue("@username", username);
        checkCmd.Parameters.AddWithValue("@password", password);
        checkCmd.Parameters.AddWithValue("@token", token);

        var result = checkCmd.ExecuteScalar();

        if (result == null)
        {
            Console.WriteLine("🔴 Access denied: invalid token or login");
            return false;
        }

        int userId = Convert.ToInt32(result);
        Console.WriteLine("🟢 Token valid, loading profile...");
        return true;
    }
}