namespace SetFavoriteMediaService;

using Npgsql;

public class Set_Favorite_Media_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int Set_Favorite_Media_Logic(int mediaId, int userId)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        // checks if the media exists
        using var checkCmd = new NpgsqlCommand("SELECT id FROM media_entries WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", mediaId);
        var ownerId = checkCmd.ExecuteScalar();

        Console.WriteLine($"--OwnerID: {ownerId}\n--UserID: {userId}");

        if (ownerId == null) return 404; // Media entry not found

        try
        {
            string query = "INSERT INTO favorites (user_id, media_id) VALUES (@user_id, @media_id);";

            using var CMD = new NpgsqlCommand(query, conn);
            CMD.Parameters.AddWithValue("media_id", mediaId);
            CMD.Parameters.AddWithValue("user_id", userId);
            
            CMD.ExecuteNonQuery();

            return 201;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") 
        {
            // if the user already has this media in favorites
            return 409;
        }
        catch (Exception)
        {
            return 500;
        }
    }
}