namespace ConfirmCommentLogic;

using Npgsql;

//* utils
using DBConnection;

public class Confirm_Comment_Service
{
    public static async Task<(int StatusCode, string Message)> Confirm_Comment_Logic(int mediaId, int userId, Rating ratingData)
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
        }
        catch (Exception)
        {
            return (500, "Error during rating verification.");
        }


        try
        {
            string ConfirmQuery = @"
                UPDATE ratings
                SET
                    comment_published = @status
                WHERE media_id = @mId AND user_id = @uId;
            ";

            using var CMD = new NpgsqlCommand(ConfirmQuery, conn);
            CMD.Parameters.AddWithValue("mId", mediaId);
            CMD.Parameters.AddWithValue("uId", userId);
            CMD.Parameters.AddWithValue("status", ratingData.CommentPublished);

            int result = await CMD.ExecuteNonQueryAsync();
            if (result == 0) return (404, "Rating not found.");

            //###############//
            // Update rating //
            //###############//
            string recalcQuery = @"
                UPDATE media_entries
                SET avg_score = COALESCE((
                    SELECT ROUND(AVG(stars), 2)
                    FROM ratings
                    WHERE media_id = @mId AND comment_published = TRUE
                ), 0)
                WHERE id = @mId;
            ";

            using var recalcCmd = new NpgsqlCommand(recalcQuery, conn);
            recalcCmd.Parameters.AddWithValue("mId", mediaId);
            await recalcCmd.ExecuteNonQueryAsync();

            string action = ratingData.CommentPublished ? "public" : "private";
            string msg = $"Comment is now {action}.";

            return (200, msg);
        }        
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
            return (500, "Database error during confirmation.");
        }
    }
}