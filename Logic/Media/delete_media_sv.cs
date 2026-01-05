namespace Media_Delete_Logic;

using Npgsql;

//* utils 
using DBConnection;

public class MediaDeleteLogic
{
    public static async Task<(int StatusCode, string Message)> DeleteMedia(int mediaID, int userID)
    {
        using var conn = DbFactory.GetConnection();

        try
        {
            await conn.OpenAsync();

            using var checkCmd = new NpgsqlCommand("SELECT user_id FROM media_entries WHERE id = @id", conn);
            checkCmd.Parameters.AddWithValue("id", mediaID);
            var ownerIdObj = await checkCmd.ExecuteScalarAsync();

            if (ownerIdObj == null) return (404, "Media not found");
            if (Convert.ToInt32(ownerIdObj) != userID) return (403, "Forbidden: Not the owner");

        }
        catch (Exception ex)
        {
            return (500, $"Connection or Auth error: {ex.Message}");
        }

        using var transaction = await conn.BeginTransactionAsync();
        try
        {
            //#######################//
            // Delete related genres //
            using var delGenres = new NpgsqlCommand("DELETE FROM media_genres WHERE media_id = @id", conn);
            delGenres.Parameters.AddWithValue("id", mediaID);
            await delGenres.ExecuteNonQueryAsync();

            //#################################//
            // Delete related comments/ratings //
            using var delRatings = new NpgsqlCommand("DELETE FROM ratings WHERE media_id = @id", conn);
            delRatings.Parameters.AddWithValue("id", mediaID);
            await delRatings.ExecuteNonQueryAsync();

            //##########################
            // Delete the media entry //
            string deleteQuery = "DELETE FROM media_entries WHERE id = @id;";
            using var cmd = new NpgsqlCommand(deleteQuery, conn);
            cmd.Parameters.AddWithValue("id", mediaID);
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            return rowsAffected > 0 ? (200, "Deleted") : (404, "Not found during delete");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (500, $"Database error during deletion: {ex.Message}");
        }
    }
}