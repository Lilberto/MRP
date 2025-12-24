using Npgsql;

namespace Media_extract;
public class Media_extract_service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static List<Media> Media_extract()
    {
        var MediaList = new List<Media>();

        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        string MediaExtractAllQuery = @"SELECT m.id, m.user_id, m.title, m.description, m.media_type, 
                m.release_year, m.age_restriction, m.avg_score,
                ARRAY_AGG(g.genre) FILTER (WHERE g.genre IS NOT NULL) as genres
                FROM media_entries m
                LEFT JOIN media_genres g ON m.id = g.media_id
                GROUP BY m.id;";
        using var extractCMD = new NpgsqlCommand(MediaExtractAllQuery, conn);
        using var reader = extractCMD.ExecuteReader();

        while (reader.Read())
        {
            MediaList.Add(new Media
            {
                id = reader.GetInt32(0), 
                userid = reader.GetInt32(1),
                title = reader.GetString(2),
                description = reader.IsDBNull(3) ? "" : reader.GetString(3), // input = empty / db desc = empty string
                type = reader.GetString(4),
                year = reader.GetInt32(5),
                agerating = reader.GetString(6),
                score = (double)reader.GetDecimal(7),
                genres = reader.IsDBNull(8) ? new List<string>() : new List<string>(reader.GetFieldValue<string[]>(8))
            });
        }

        return MediaList;
    }
}