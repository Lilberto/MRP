namespace Profile_Service;

using System;

using Auth_util;

public static class ProfileService
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static async Task<(int StatusCode, List<UserProfileDTO> Data)> Profile_User(int user_id)
    {
        try
        {
            using var conn = new Npgsql.NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    u.id, u.username, u.created_at,
                    (SELECT COUNT(*) FROM media_entries WHERE user_id = u.id) as media_count,
                    (SELECT COUNT(*) FROM ratings WHERE user_id = u.id) as ratings_given,
                    COALESCE((SELECT AVG(r.stars) FROM ratings r 
                            JOIN media_entries m ON r.media_id = m.id 
                            WHERE m.user_id = u.id), 0) as avg_received,

                    (SELECT g.genre FROM media_genres g 
                    JOIN media_entries m ON g.media_id = m.id 
                    WHERE m.user_id = u.id 
                    GROUP BY g.genre ORDER BY COUNT(*) DESC LIMIT 1) as fav_genre
                FROM users u
                WHERE u.id = @user_id;
            ";

            using var cmd = new Npgsql.NpgsqlCommand(query, conn);
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

            return (200, profiles);
        }
        catch (Exception)
        {
            return (500, new List<UserProfileDTO>());
        }
    }
}