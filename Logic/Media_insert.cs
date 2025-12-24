using Npgsql;

namespace Media_insert;
public class Media_insert_service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int Media_insert(int userid, string title, string description, string mediatype, int release_year, string age_restriction, List<string> genres)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();
        using var transaction = conn.BeginTransaction();

        string MediaInsertQuery = @"
            INSERT INTO media_entries (user_id, title, description, media_type, release_year, age_restriction) 
            VALUES (@userId, @title, @description, @mediaType, @releaseYear, @ageRestriction)
            RETURNING id;";
        using var insertCMD = new NpgsqlCommand(MediaInsertQuery, conn);

        insertCMD.Parameters.AddWithValue("@userId", userid);
        insertCMD.Parameters.AddWithValue("@title", title);
        insertCMD.Parameters.AddWithValue("@description", (object)description ?? DBNull.Value);
        insertCMD.Parameters.AddWithValue("@mediaType", mediatype);
        insertCMD.Parameters.AddWithValue("@releaseYear", release_year);
        insertCMD.Parameters.AddWithValue("@ageRestriction", age_restriction);

        var newMediaID = insertCMD.ExecuteScalar();


        if (genres != null && genres.Count > 0)
        {
            foreach (var genre in genres)
            {
                string sqlGenre = "INSERT INTO media_genres (media_id, genre) VALUES (@mid, @g);";
                using var cmdGenre = new NpgsqlCommand(sqlGenre, conn);
                cmdGenre.Parameters.AddWithValue("mid", newMediaID!);
                cmdGenre.Parameters.AddWithValue("g", genre);
                cmdGenre.ExecuteNonQuery();
            }
        }
        transaction.Commit();

        Console.WriteLine($"New Media: {newMediaID}");

        return Convert.ToInt32(newMediaID);
    }
}