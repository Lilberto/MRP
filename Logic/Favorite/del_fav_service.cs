namespace DeleteFavoriteMediaService;

using Npgsql;

public class Delete_Favorite_Media_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int Delete_Favorite_Media_Logic(int mediaId, int userId)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        // checks if the media exists
        using var checkCmd = new NpgsqlCommand("SELECT id FROM media_entries WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", mediaId);
        var ownerId = checkCmd.ExecuteScalar();

        if (ownerId == null) return 404; // Media entry not found

        Console.WriteLine($"Before: \n--OwnerID: {ownerId}\n--UserID: {userId}");

        try
        {
            string query = "DELETE FROM favorites WHERE user_id = @user_id AND media_id = @media_id;";

            using var CMD = new NpgsqlCommand(query, conn);
            CMD.Parameters.AddWithValue("media_id", mediaId);
            CMD.Parameters.AddWithValue("user_id", userId);
            
            int result = CMD.ExecuteNonQuery();

            return result > 0 ? 200 : 404;
        }
        catch (Exception)
        {
            return 500;
        }
    }
}