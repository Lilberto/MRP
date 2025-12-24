using Hash_util;
using Npgsql;

namespace Login_Service;

public static class LoginService
{
    private const string ConnectionString = @"
        Host=localhost;
        Port=5432;
        Database=mrp_db;
        Username=admin;
        Password=mrp123;"
    ;

    public static bool LoginUser(string username, string password)
    {
        try
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();

            string checkQuery = "SELECT salt, password_hash FROM users WHERE username = @username;";
            using var checkCmd = new NpgsqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@username", username);
            checkCmd.Parameters.AddWithValue("@password", password);

            using var reader = checkCmd.ExecuteReader();
            
            if(!reader.Read())
            {
                return false;
            }

            string dbSalt = reader.GetString(0);
            string dbHash = reader.GetString(1);
            reader.Close();

            string computedHash = Hash.HashPassword(password, dbSalt);

            Console.WriteLine($"dbHash: {dbHash}, salt: {dbSalt}, computedHash: {computedHash}");

            if (computedHash == dbHash)
            {
                string token = $"{username}-mrptoken"; 

                string updateQuery = "UPDATE users SET token = @token, token_created_At = CURRENT_TIMESTAMP WHERE username = @username";
                using var updateCmd = new NpgsqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@token", token);
                updateCmd.Parameters.AddWithValue("@username", username);

                var insertToken = updateCmd.ExecuteNonQuery();
                Console.WriteLine($"User '{username}' successfully registered token: {token}!");

                return true;

            } else {
                Console.WriteLine("Wrong login");
                return false;
            } 
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine("Database error during registration:");
            Console.WriteLine($"PostgreSQL Error [{ex.SqlState}]: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("General error during registration:");
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}