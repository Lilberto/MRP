using System;
using MySql.Data.MySqlClient;

public static class Register_Check
{
    private const string ConnectionString = "Server=localhost;Database=my_mrp;User Id=root;Password=root;";

    public static bool RegisterUser(string username, string password)
    {
        try
        {
            using var conn = new MySqlConnection(ConnectionString);
            conn.Open();

            // Prüfen, ob Username schon existiert
            string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username;";
            using var checkCmd = new MySqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@username", username);

            var count = Convert.ToInt32(checkCmd.ExecuteScalar());
            if (count > 0)
            {
                Console.WriteLine("❌ Benutzername existiert bereits!");
                return false;
            }

            // Benutzer einfügen
            string insertQuery = "INSERT INTO users (username, password_hash) VALUES (@username, @password);";
            using var insertCmd = new MySqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@username", username);
            insertCmd.Parameters.AddWithValue("@password", password); 

            insertCmd.ExecuteNonQuery();

            Console.WriteLine("✅ Benutzer erfolgreich registriert!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Fehler bei der Registrierung:");
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}
