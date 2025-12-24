using Npgsql;

using Hash_util;

namespace Register_Service;

public static class RegisterService
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";
    public static bool RegisterUser(string username, string password)
    {
        try
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();

            string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username;";
            using var checkCmd = new NpgsqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@username", username);

            var count = Convert.ToInt64(checkCmd.ExecuteScalar());
            if (count > 0)
            {
                Console.WriteLine("User exists!");
                return false;
            }

            // 2. Salt generieren und Password hashen (SICHERHEIT!)
            string salt = Hash.GenerateSalt();
            string passwordHash = Hash.HashPassword(password, salt);

            // 3. Benutzer mit ALLEN benötigten Feldern einfügen
            string insertQuery = @"
                INSERT INTO users 
                    (username, password_hash, salt, created_at) 
                VALUES 
                    (@username, @password_hash, @salt, @created_at)
                RETURNING id;";

            using var insertCmd = new NpgsqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@username", username);
            insertCmd.Parameters.AddWithValue("@password_hash", passwordHash);
            insertCmd.Parameters.AddWithValue("@salt", salt);
            insertCmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);

            var newUserId = insertCmd.ExecuteScalar();
            Console.WriteLine($"User '{username}' successfully registered (ID: {newUserId})!");

            return true;
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