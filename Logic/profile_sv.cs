namespace Profile_Service;

using Npgsql;
using System;

//* utils
using DBConnection;

public static class ProfileService
{
    public static async Task<(int StatusCode, string Message, List<UserProfileDTO> Data)> Profile_User(int user_id, string username)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        try
        {
            //#########################//
            // User has access to list //
            //#########################//
            using var checkCmd = new NpgsqlCommand("SELECT username FROM users WHERE id = @id", conn);
            checkCmd.Parameters.AddWithValue("id", user_id);
            string? actualUsername = await checkCmd.ExecuteScalarAsync() as string;

            if (actualUsername == null) return (404, "User profile not found.", new List<UserProfileDTO>());

            if (!string.Equals(actualUsername, username, StringComparison.OrdinalIgnoreCase))
            {
                return (403, "Forbidden: Access restricted to profile owner.", new List<UserProfileDTO>());
            }

            //###########################################################//
            // 1. Retrieves user profile data and activity statistics    //
            // 2. Computes total average score from all received ratings //
            // 3. Identifies the creator's most common media genre       //
            //###########################################################//
            string query = @"
                SELECT 
                    u.id, u.username, u.created_at,
                    (SELECT COUNT(*) FROM media_entries WHERE user_id = u.id) as media_count,
                    (SELECT COUNT(*) FROM ratings WHERE user_id = u.id) as ratings_given,
                    
                    COALESCE((
                        SELECT AVG(r.stars)::float8 
                        FROM ratings r
                        WHERE r.media_id IN (SELECT id FROM media_entries WHERE user_id = u.id)
                    ), 0) as avg_received,

                    (SELECT g.genre FROM media_genres g 
                    JOIN media_entries m ON g.media_id = m.id 
                    WHERE m.user_id = u.id 
                    GROUP BY g.genre ORDER BY COUNT(*) DESC LIMIT 1) as fav_genre
                FROM users u
                WHERE u.id = @user_id;
            ";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("user_id", user_id);
            using var reader = await cmd.ExecuteReaderAsync();

            var profiles = new List<UserProfileDTO>();

            while (await reader.ReadAsync())
            {
                var profile = new UserProfileDTO
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    MemberSince = reader.GetDateTime(2),
                    TotalMediaEntries = (int)reader.GetInt64(3), 
                    TotalRatingsGiven = (int)reader.GetInt64(4),
                    AvgRatingReceived = Convert.ToDouble(reader.GetValue(5)),
                    FavoriteGenre = reader.IsDBNull(6) ? "No values" : reader.GetString(6)
                };

                profiles.Add(profile);
            }

            return (200, $"Profile data for {username} retrieved successfully.", profiles);
        }
        catch (Exception)
        {
            return (500, "Internal Server Error. Please contact support.", new List<UserProfileDTO>());
        }
    }
}