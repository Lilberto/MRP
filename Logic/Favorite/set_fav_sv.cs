namespace SetFavoriteMediaService;

using Npgsql;

//* utils
using DBConnection;

public class Set_Favorite_Media_Service
{

    public static async Task <(int StatusCode, string Message)> Set_Favorite_Media_Logic(int mediaId, int userId)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        //###############//
        // Media exisits //
        //###############//
        using var checkCmd = new NpgsqlCommand("SELECT id FROM media_entries WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", mediaId);
        var exists = await checkCmd.ExecuteScalarAsync();

        if (exists == null) return (404, "Media not found."); 

        try
        {
            //###########################################//
            // Link user and media in favorites table    //
            //###########################################//
            string query = "INSERT INTO favorites (user_id, media_id) VALUES (@user_id, @media_id);";

            using var CMD = new NpgsqlCommand(query, conn);
            CMD.Parameters.AddWithValue("media_id", mediaId);
            CMD.Parameters.AddWithValue("user_id", userId);
            
            await CMD.ExecuteNonQueryAsync();

            return (201, "Media marked as favorite.");
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") 
        {
            return (409, "Media is already in favorites.");
        }
        catch (Exception ex)
        {
            return (500, $"Internal server error: {ex.Message}");
        }
    }
}