using System.Data.Common;
using System.Runtime.InteropServices;
using Npgsql;

namespace Media_update_logic;

public class Media_update_service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int Media_update(int mediaID, int userID, MediaUpdateDto data)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        using var checkCmd = new NpgsqlCommand("SELECT user_id FROM media_entries WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", mediaID);
        var ownerId = checkCmd.ExecuteScalar();

        if (ownerId == null) return 404; // Does not exist
        if ((int)ownerId != userID) return 403; // Another user created the media

        using var transaction = conn.BeginTransaction();

        try
        {
            string MediaExtractSingleQuery = @"
                UPDATE media_entries
                SET 
                    title = @title,
                    description = @description,
                    media_type = @media_type,
                    release_year = @release_year,
                    age_restriction = @age_restriction,
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @id AND user_id = @user_id
                RETURNING id;
            ";

            using var CMD = new NpgsqlCommand(MediaExtractSingleQuery, conn);
            CMD.Parameters.AddWithValue("user_id", userID);
            CMD.Parameters.AddWithValue("id", mediaID);
            CMD.Parameters.AddWithValue("title", data.title);
            CMD.Parameters.AddWithValue("description", (object)data.description ?? DBNull.Value);
            CMD.Parameters.AddWithValue("media_type", data.type);
            CMD.Parameters.AddWithValue("release_year", data.year);
            CMD.Parameters.AddWithValue("age_restriction", data.agerating);

           var result = CMD.ExecuteScalar();

            if (result == null)
            {
                transaction.Rollback();
                return 404; 
            }

            using (var delCmd = new NpgsqlCommand("DELETE FROM media_genres WHERE media_id=@id", conn))
            {
                delCmd.Parameters.AddWithValue("id", mediaID);
                delCmd.ExecuteNonQuery();
            }

            foreach (var genre in data.genres)
            {
                using var insCmd = new NpgsqlCommand("INSERT INTO media_genres (media_id, genre) VALUES (@id, @genre)", conn);
                insCmd.Parameters.AddWithValue("id", mediaID);
                insCmd.Parameters.AddWithValue("genre", genre);
                insCmd.ExecuteNonQuery();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update Error: {ex.Message}");
            transaction.Rollback();
            return 500;
        }
        transaction.Commit();
        return 200;
    }
}