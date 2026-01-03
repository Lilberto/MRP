namespace All_Media_extract;

using Npgsql;

//* utils
using DBConnection;

public class All_Media_extract_service
{
    public static async Task<(int StatusCode, string Message, List<Media> Data)> All_Media_extract(MediaSearchFilter filter)
    {
        try
        {
            using var conn = DbFactory.GetConnection();
            await conn.OpenAsync();

            //#############################################################################
            // 1. avg score calculation
            // - if no ratings, avg score = 0.0

            // 2. JOINS: all media with genres and ratings
            // - include media without genres or ratings 

            // 3. Filters
            // - even if a filter param is null, the query works correctly

            // 4. Sorting
            // - by title, rating (best/worst), release year (newest/oldest), id (asc/desc)
            //#############################################################################
            string query = @"
                SELECT 
                    m.id, m.user_id, m.title, m.description, m.media_type, 
                    m.release_year, m.age_restriction, 
                    COALESCE(AVG(r.stars), 0.0) as avg_score,
                    ARRAY_AGG(DISTINCT g.genre) FILTER (WHERE g.genre IS NOT NULL) as genres,
                    m.created_at  

                FROM media_entries m
                LEFT JOIN media_genres g ON m.id = g.media_id
                LEFT JOIN ratings r ON m.id = r.media_id

                WHERE (@title IS NULL OR m.title ILIKE @title)
                AND (@type IS NULL OR m.media_type = @type)
                AND (@year IS NULL OR m.release_year = @year)
                AND (@age IS NULL OR m.age_restriction = @age)
                AND (@genre IS NULL OR EXISTS (
                        SELECT 1 FROM media_genres g2 
                        WHERE g2.media_id = m.id AND g2.genre ILIKE @genre))

                GROUP BY m.id, m.user_id, m.title, m.description, m.media_type, m.release_year, m.age_restriction, m.created_at
                HAVING (@rating IS NULL OR AVG(r.stars) >= @rating)
            ";

            //##########################//
            // Sorting
            // Default: by id ascending
            //##########################//
            query += filter.SortBy switch
            {
                "title" => " ORDER BY m.title ASC",
                "rating_best" => " ORDER BY avg_score DESC",
                "rating_worst" => " ORDER BY avg_score ASC",
                "year_newest" => " ORDER BY m.release_year DESC",
                "year_oldest" => " ORDER BY m.release_year ASC",
                "asc" => " ORDER BY m.id ASC",
                "desc" => " ORDER BY m.id DESC",
                _ => " ORDER BY m.id ASC"
            };

            using var extractCMD = new NpgsqlCommand(query, conn);

            // Genre filter uses ILIKE with wildcards for partial matching (%...%)
            extractCMD.Parameters.Add(new NpgsqlParameter("title", NpgsqlTypes.NpgsqlDbType.Text) { Value = string.IsNullOrWhiteSpace(filter.Title) ? DBNull.Value : $"%{filter.Title}%" });

            // Genre filter uses ILIKE with wildcards for partial matching (%...%)
            extractCMD.Parameters.Add(new NpgsqlParameter("genre", NpgsqlTypes.NpgsqlDbType.Text)
            {
                Value = string.IsNullOrWhiteSpace(filter.Genre) ? DBNull.Value : $"%{filter.Genre}%"
            });
            extractCMD.Parameters.Add(new NpgsqlParameter("type", NpgsqlTypes.NpgsqlDbType.Text) { Value = string.IsNullOrWhiteSpace(filter.MediaType) ? DBNull.Value : filter.MediaType });
            extractCMD.Parameters.Add(new NpgsqlParameter("year", NpgsqlTypes.NpgsqlDbType.Integer) { Value = filter.ReleaseYear ?? (object)DBNull.Value });
            extractCMD.Parameters.Add(new NpgsqlParameter("age", NpgsqlTypes.NpgsqlDbType.Text) { Value = string.IsNullOrWhiteSpace(filter.AgeRestriction) ? DBNull.Value : filter.AgeRestriction });
            extractCMD.Parameters.Add(new NpgsqlParameter("rating", NpgsqlTypes.NpgsqlDbType.Numeric) { Value = filter.MinRating ?? (object)DBNull.Value });

            using var reader = await extractCMD.ExecuteReaderAsync();

            var MediaList = new List<Media>();
            while (await reader.ReadAsync())
            {
                MediaList.Add(new Media
                {
                    id = reader.GetInt32(0),
                    userid = reader.GetInt32(1),
                    title = reader.GetString(2),
                    description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    type = reader.GetString(4),
                    year = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    agerating = reader.GetString(6),
                    score = (double)reader.GetDecimal(7),
                    genres = reader.IsDBNull(8) ? new List<string>() : reader.GetFieldValue<string[]>(8).ToList(),
                    created = reader.GetDateTime(9)
                });
            }

            return (200, "Media successfully retrieved", MediaList);
        }
        catch (NpgsqlException ex)
        {
            return (409, $"Database conflict: {ex.Message}", null!);
        }
        catch (Exception ex)
        {
            return (500, $"Internal Server Error {ex.Message}", null!);
        }
    }
}