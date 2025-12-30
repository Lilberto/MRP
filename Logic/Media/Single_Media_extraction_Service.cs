using System.Data.Common;
using Npgsql;

namespace Single_Media_extract;
public class Single_Media_extract_service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static Media? Single_Media_extract(int id)
    {
        var MediaList = new List<Media>();

        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

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

        using var extractCMD = new NpgsqlCommand(MediaExtractSingleQuery, conn);
        extractCMD.Parameters.AddWithValue("id", id);
        using var reader = extractCMD.ExecuteReader();

        if (reader.Read())
        {
            return new Media
            {
                id = reader.GetInt32(0), 
                userid = reader.GetInt32(1),
                title = reader.GetString(2),
                description = reader.IsDBNull(3) ? "" : reader.GetString(3),
                type = reader.GetString(4),
                year = reader.GetInt32(5),
                agerating = reader.GetString(6),
                score = (double)reader.GetDecimal(7),
                genres = reader.IsDBNull(8) ? new List<string>() : new List<string>(reader.GetFieldValue<string[]>(8))
            };
        }

        return null;
    }
}