namespace RatingHistoryService;

using Npgsql;

//* utils
using DBConnection;

public static class Rating_History_Service
{
    public static async Task<(int StatusCode, string Message, List<RatingHistoryDto> Data)> Rating_History_Logic(int userId, string username, string token)
    {
        var history = new List<RatingHistoryDto>();

        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        //#########################//
        // User has access to list //
        //#########################//
        using var checkCmd = new NpgsqlCommand("SELECT username FROM users WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", userId);
        string? actualUsername = await checkCmd.ExecuteScalarAsync() as string;

        if (actualUsername == null) return (404, "User not found.", history);

        if (!string.Equals(actualUsername, username, StringComparison.OrdinalIgnoreCase))
        {
            return (403, "Access denied: This is not your favorite list.", history);
        }

        try
        {
            //###########################################################//
            // 1. Selects media details and aggregates associated genres //
            // 2. Joins ratings with media and creator information       //
            // 3. Filters by specific user with chronological sorting    //
            //###########################################################//
            string query = @"
                SELECT 
                    m.title, 
                    m.media_type,
                    COALESCE(STRING_AGG(DISTINCT g.genre, ', '), 'No Genres') as genres, 
                    r.stars, 
                    r.comment, 
                    r.created_at,
                    u.username

                FROM ratings r
                JOIN media_entries m ON r.media_id = m.id
                LEFT JOIN media_genres g ON m.id = g.media_id
                JOIN users u ON m.user_id = u.id 

                WHERE r.user_id = @user_Id
                GROUP BY 
                    m.id, m.title, m.media_type, r.id, r.stars, 
                    r.comment, r.created_at, u.username
                ORDER BY r.created_at DESC;
            ";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("user_Id", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var entry = new RatingHistoryDto
                {
                    MediaTitle = reader.GetString(0),
                    MediaType = reader.GetString(1),
                    MediaGenre = reader.IsDBNull(2) ? "No Genres" : reader.GetString(2),
                    Stars = reader.GetInt32(3),
                    Comment = reader.IsDBNull(4) ? null : reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5),
                    Username = reader.GetString(6)
                };
                history.Add(entry);
            }
            return (200, "History retrieved", history);
        }
            catch (Exception)
        {
            return (500, "Internal Error", history);
        }
    }
}