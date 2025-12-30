namespace RatingHistoryService;

public static class Rating_History_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static async Task<(int StatusCode, List<RatingHistoryDto> Data)> Rating_History_Logic(int userId)
    {
        var history = new List<RatingHistoryDto>();

        try
        {
            using var conn = new Npgsql.NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    m.title, 
                    m.media_type,
                    COALESCE(STRING_AGG(g.genre, ', '), 'No Genres') as genres, 
                    r.stars, 
                    r.comment, 
                    r.created_at
                FROM ratings r
                JOIN media_entries m ON r.media_id = m.id
                LEFT JOIN media_genres g ON m.id = g.media_id
                WHERE r.user_id = @user_Id
                GROUP BY m.id, m.title, m.media_type, r.id, r.stars, r.comment, r.created_at
                ORDER BY r.created_at DESC;"  
            ;

            using var cmd = new Npgsql.NpgsqlCommand(query, conn);
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
                    CreatedAt = reader.GetDateTime(5)
                };
                history.Add(entry);
            }
            return (200, history);
        }
            catch (Exception)
        {
            return (500, history);
        }
    }
}