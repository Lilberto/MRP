namespace LeaderboardService;

public class Leaderboard_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static async Task<(int StatusCode, List<Media> Data)> Leaderboard_Logic()
    {
        var media = new List<Media>();

        try
        {
            using var conn = new Npgsql.NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            string query = @"
                SELECT 
                    m.id,
                    m.user_id,
                    m.title, 
                    m.description,
                    m.media_type, 
                    m.release_year,
                    COALESCE(STRING_AGG(DISTINCT g.genre, ', '), 'No Genres') as genres,
                    m.age_restriction,
                    COALESCE(AVG(r.stars), 0) as average_rating, 
                    
                    m.created_at
                FROM media_entries m
                LEFT JOIN ratings r ON m.id = r.media_id
                LEFT JOIN media_genres g ON m.id = g.media_id
                GROUP BY m.id, m.user_id, m.title, m.description, m.media_type, m.release_year, m.age_restriction, m.created_at
                ORDER BY average_rating DESC
                LIMIT 10;
            "; //LIMIT 10

            using var cmd = new Npgsql.NpgsqlCommand(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var entry = new Media
                {
                    id = reader.GetInt32(0),
                    userid = reader.GetInt32(1),
                    title = reader.GetString(2),
                    description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    type = reader.GetString(4),
                    year = reader.GetInt32(5),
                    genres = reader.GetString(6).Split(", ").ToList(),
                    agerating = reader.GetString(7),
                    score = Convert.ToDouble(reader.GetValue(8)), // Sicherer Cast
                    //creator = reader.IsDBNull(9) ? "Unknown" : reader.GetString(9),
                    created = reader.GetDateTime(9)
                };
                media.Add(entry);
            }
            return (200, media);
        }
        catch (Exception)
        {
            return (500, media);
        }
    }
}