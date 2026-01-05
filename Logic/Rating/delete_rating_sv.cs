namespace DeleteRatingLogic;

using Npgsql;

//* utils
using DBConnection;
using DeleteRatingEP;

public class delete_rating_service
{
    public static async Task<(int StatusCode, string Message)> delete_rating_logic(int mediaId, int userId)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        using var checkCmd = new NpgsqlCommand("SELECT user_id FROM ratings WHERE media_id = @mId AND user_id = @uId", conn);
        checkCmd.Parameters.AddWithValue("mId", mediaId);
        checkCmd.Parameters.AddWithValue("uId", userId);
        var ownerIdObj = await checkCmd.ExecuteScalarAsync();

        if (ownerIdObj == null) return (404, "Rating not found.");
        if (Convert.ToInt32(ownerIdObj) != userId) return (403, "Not your rating.");

        try
        {
            using var cmd = new NpgsqlCommand("DELETE FROM ratings WHERE media_id = @mId AND user_id = @uId", conn);
            cmd.Parameters.AddWithValue("mId", mediaId);
            cmd.Parameters.AddWithValue("uId", userId);

            // Update rating
            int result = await cmd.ExecuteNonQueryAsync();
            if (result == 0) return (404, "Rating not found.");
            
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

            return (200, "Rating deleted.");
        }
        catch (PostgresException)
        {
            return (500, "Database error during deletion.");
        }
        catch (Exception)
        {
            return (500, "Unexpected server error.");
        }
    }
}