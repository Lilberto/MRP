namespace DeleteFavoriteMediaService;

using Npgsql;

//* utils
using DBConnection;

public class Delete_Favorite_Media_Service
{
    public static async Task <(int StatusCode, string Message)> Delete_Favorite_Media_Logic(int mediaId, int userId)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        //#############################//
        // Media exisits in favorites? //
        //#############################//
        using var checkCmd = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM favorites WHERE user_id = @uId AND media_id = @mId)", conn);
        checkCmd.Parameters.AddWithValue("uId", userId);
        checkCmd.Parameters.AddWithValue("mId", mediaId);
        var ownerId = await checkCmd.ExecuteScalarAsync();

        var result = await checkCmd.ExecuteScalarAsync();
        bool isFavorite = (result as bool?) ?? false;

        if (!isFavorite) 
        {
            return (404, "This media is not in your favorites.");
        }

        try
        {
            string query = "DELETE FROM favorites WHERE user_id = @user_id AND media_id = @media_id;";

            using var CMD = new NpgsqlCommand(query, conn);
            CMD.Parameters.AddWithValue("media_id", mediaId);
            CMD.Parameters.AddWithValue("user_id", userId);
            
            await CMD.ExecuteNonQueryAsync();

            return (200, "Successfully removed from favorites.");
        }
        catch (Exception)
        {
            return (500, $"Delete failed.");
        }
    }
}