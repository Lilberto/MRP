namespace LeaderboardService;

using Npgsql;

//* utils
using DBConnection;

public class Leaderboard_Service
{
    public static async Task<(int StatusCode, List<Media> Data)> Leaderboard_Logic()
    {
        var media = new List<Media>();

        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        try
        {
            //####################################################//
            // 1. Fetches media details and pre-calculated scores //
            // 2. Joins creator names and aggregates genres       //
            // 3. Orders by highest score and limits results      //
            //####################################################//
            string query = @"
                SELECT 
                    m.id, m.user_id, u.username, m.title, m.description, 
                    m.media_type, m.release_year, 
                    COALESCE(STRING_AGG(DISTINCT g.genre, ', '), 'No Genres') as genres,
                    m.age_restriction, m.avg_score, m.created_at

                FROM media_entries m
                JOIN users u ON m.user_id = u.id
                LEFT JOIN media_genres g ON m.id = g.media_id

                GROUP BY 
                    m.id, m.user_id, u.username, m.title, m.description, 
                    m.media_type, m.release_year, m.age_restriction, m.avg_score, m.created_at
                ORDER BY m.avg_score DESC
                LIMIT 10;
            ";

            using var cmd = new NpgsqlCommand(query, conn);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var entry = new Media
                {
                    id = reader.GetInt32(0),
                    userid = reader.GetInt32(1),
                    username = reader.GetString(2), 
                    title = reader.GetString(3),
                    description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    type = reader.GetString(5),
                    year = reader.GetInt32(6),
                    genres = reader.GetString(7).Split(", ").ToList(),
                    agerating = reader.GetString(8),
                    score = Convert.ToDouble(reader.GetValue(9)), 
                    created = reader.GetDateTime(10)
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