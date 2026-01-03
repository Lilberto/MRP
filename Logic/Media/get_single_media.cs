namespace Single_Media_extract;

using Npgsql;

//* utils
using DBConnection;

public class Single_Media_extract_service
{
    public static async Task<(int StatusCode, string Message, Media? Data, List<Rating>? Rating_Data)> Single_Media_extract(int media_id)
    {
        try
        {
            using var conn = DbFactory.GetConnection();
            await conn.OpenAsync();

            string MediaExtractSingleQuery = @"
                SELECT 
                    m.id, 
                    m.user_id, 
                    m.title, 
                    m.description, 
                    m.media_type, 
                    m.release_year, 
                    m.age_restriction, 
                    COALESCE(AVG(r.stars), 0.0) as avg_score,
                    ARRAY_AGG(DISTINCT g.genre) FILTER (WHERE g.genre IS NOT NULL) as genres
                FROM media_entries m
                LEFT JOIN media_genres g ON m.id = g.media_id
                LEFT JOIN ratings r ON m.id = r.media_id
                WHERE m.id = @id
                GROUP BY m.id, m.user_id, m.title, m.description, m.media_type, m.release_year, m.age_restriction;
            ";

            using var extractCMD1 = new NpgsqlCommand(MediaExtractSingleQuery, conn);
            extractCMD1.Parameters.AddWithValue("id", media_id);
            using var reader1 = await extractCMD1.ExecuteReaderAsync();

            if (!await reader1.ReadAsync()) 
                return (404, "Media not found.", null, null);

            var media = new Media {
                id = reader1.GetInt32(0),
                userid = reader1.GetInt32(1),
                title = reader1.GetString(2),
                description = reader1.IsDBNull(3) ? "" : reader1.GetString(3),
                type = reader1.GetString(4),
                year = reader1.GetInt32(5),
                agerating = reader1.GetString(6),
                score = (double)reader1.GetDecimal(7),
                genres = reader1.IsDBNull(8) ? new() : new(reader1.GetFieldValue<string[]>(8))
            };
            reader1.Close();

            //#################//
            // Extract Ratings //
            //#################//
            string RatingExtractQuery = @"
                SELECT id, user_id, stars, comment, created_at 
                FROM ratings 
                WHERE media_id = @id 
                AND comment_published = true 
                ORDER BY created_at DESC;
            ";
            var ratings = new List<Rating>();

            using var extractCMD2 = new NpgsqlCommand(RatingExtractQuery, conn);
            extractCMD2.Parameters.AddWithValue("id", media_id);
            using var reader2 = await extractCMD2.ExecuteReaderAsync();

            while (await reader2.ReadAsync()) {
                ratings.Add(new Rating {
                    Id = reader2.GetInt32(0),
                    UserId = reader2.GetInt32(1),
                    Stars = reader2.GetInt32(2),
                    Comment = reader2.IsDBNull(3) ? "" : reader2.GetString(3),
                    CreatedAt = reader2.GetDateTime(4)
                });
            }

            return (200, "Media retrieved successfully.", media, ratings);
        }
        catch (NpgsqlException ex)
        {
            return (409, $"Database conflict: {ex.Message}", null!, null!);
        }
        catch (Exception ex)
        {
            return (500, $"Internal server error: {ex.Message}", null, null);
        }
    }
}