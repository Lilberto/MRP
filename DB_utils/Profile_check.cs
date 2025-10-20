using System;
using MySql.Data.MySqlClient;

public static class Profile_Check
{
    private const string ConnectionString = "Server=localhost;Database=my_mrp;User Id=root;Password=root;";

    public static User? ProfileUser(string username, string password, string token)
    {
        try
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
                return null;
            }

            int userId = Convert.ToInt32(result);
            Console.WriteLine("🟢 Token valid, loading profile...");

            // Profile data retrieval
            string profileQuery = @"
                SELECT 
                    u.id,
                    u.username,
                    u.password_hash,
                    u.created_at,
                    u.total_ratings,
                    u.avg_score,
                    u.favorite_genre
                FROM users u
                WHERE u.id = @userId;";

            using var profileCmd = new MySqlCommand(profileQuery, conn);
            profileCmd.Parameters.AddWithValue("@userId", userId);

            using var reader = profileCmd.ExecuteReader();

            // If user found, map to User object
            if (reader.Read())
            {
                var user = new User
                {
                    Id = reader.GetInt32("id"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password_hash"),
                    Token = token,
                    CreatedAt = reader.GetDateTime("created_at"),
                    TotalRatings = reader.GetInt32("total_ratings"),
                    AverageScore = Convert.ToDouble(reader.GetDecimal("avg_score")),
                    FavoriteGenre = reader.IsDBNull(6) ? null : reader.GetString(6),
                };

                return user;
            }

            Console.WriteLine("🟡 No Profile found.");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine("🔴 Error when loading Profile:");
            Console.WriteLine(ex.Message);
            return null;
        }
    }
}
