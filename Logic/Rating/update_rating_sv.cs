namespace UpdateRatingLogic;

using Npgsql;

//* utils
using DBConnection;

public class Update_Rating_Service
{
    public static async Task<(int StatusCode, string Message)> Update_Rating_Logic(int mediaId, int userId, Rating RatingData)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        try
        {
            using var checkCmd = new NpgsqlCommand("SELECT user_id FROM ratings WHERE media_id = @mId AND user_id = @uId", conn);
            checkCmd.Parameters.AddWithValue("mId", mediaId);
            checkCmd.Parameters.AddWithValue("uId", userId);
            var ownerIdObj = await checkCmd.ExecuteScalarAsync();

            if (ownerIdObj == null) return (404, "Rating not found.");

            Console.WriteLine($"Owner ID: {ownerIdObj}, User ID: {userId}");
            if (Convert.ToInt32(ownerIdObj) != userId) return (403, "Not your rating.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Check Error: {ex.Message}");
            return (500, "Error during rating verification.");
        }

        try
        {
            string CreateRatingQuery = @"
                UPDATE ratings 
                SET stars = @stars, comment = @comment, updated_at = CURRENT_TIMESTAMP
                WHERE media_id = @mId AND user_id = @uId;
            ";

            using var CMD = new NpgsqlCommand(CreateRatingQuery, conn);
            CMD.Parameters.AddWithValue("uId", userId);
            CMD.Parameters.AddWithValue("mId", mediaId);
            CMD.Parameters.AddWithValue("stars", RatingData.Stars!);
            CMD.Parameters.AddWithValue("comment", (object)RatingData.Comment! ?? DBNull.Value);

            var rows = await CMD.ExecuteNonQueryAsync();

            return rows > 0 ? (200, "Rating updated.") : (500, "Update failed.");
        }
        catch (Exception)
        {
            return (500, "Database error during update.");
        }
    }
}