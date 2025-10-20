using System;
using MySql.Data.MySqlClient;

public static class Login_Check
{
    private const string ConnectionString = "Server=localhost;Database=my_mrp;User Id=root;Password=root;";

    public static bool LoginUser(string username, string password)
    {
        try
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            string checkQuery = "SELECT id FROM users WHERE username = @username AND password_hash = @password;";
            using var checkCmd = new MySqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@username", username);
            checkCmd.Parameters.AddWithValue("@password", password);

            object? result = checkCmd.ExecuteScalar();

            if (result == null)
            {
                Console.WriteLine("🔴 Login not possible – incorrect user name or password.");
                return false;
            }

            int userId = Convert.ToInt32(result);
            Console.WriteLine($"🟢Login successfull (User-ID: {userId})");

            string token = $"{username}-mrpToken";

            // SQL Select for identity and token creation and identity verification
            string checkTokenQuery = "SELECT COUNT(*) FROM tokens WHERE user_id = @user_id AND token = @token;";
            using var checkTokenCmd = new MySqlCommand(checkTokenQuery, conn);
            checkTokenCmd.Parameters.AddWithValue("@user_id", userId);
            checkTokenCmd.Parameters.AddWithValue("@token", token);

            bool tokenExists = Convert.ToInt32(checkTokenCmd.ExecuteScalar()) > 0;

            if (!tokenExists)
            {
                // SQL Insert to save new token
                string insertTokenQuery = "INSERT INTO tokens (user_id, token, created_at) VALUES (@user_id, @token, NOW());";
                using var insertCmd = new MySqlCommand(insertTokenQuery, conn);
                insertCmd.Parameters.AddWithValue("@user_id", userId);
                insertCmd.Parameters.AddWithValue("@token", token);
                insertCmd.ExecuteNonQuery();

                Console.WriteLine($"New Token saved: {token}");
            }
            else
            {
                Console.WriteLine($"Token allready exists: {token}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Fehler beim Login:");
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}
