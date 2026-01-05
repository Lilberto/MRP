namespace LikeRatingLogic;

using Npgsql;

//* utils
using DBConnection;

public class Like_Rating_Service
{
    public static async Task<(int StatusCode, string Message)> Like_Rating_Logic(int ratingId, int userId)
    {
        try
        {
            using var conn = DbFactory.GetConnection();
            await conn.OpenAsync();

            //#######################################################//
            // Check if rating exists and does not belong to the user//
            //#######################################################//
            using var checkCmd = new NpgsqlCommand("SELECT user_id FROM ratings WHERE id = @rId", conn);
            checkCmd.Parameters.AddWithValue("rId", ratingId);
            var result = await checkCmd.ExecuteScalarAsync();

            if (result == null) return (404, "Rating not found.");
            if (Convert.ToInt32(result) == userId) return (403, "Cannot like own rating.");

            //######################################//
            // Check if the user already liked this //
            //######################################//
            using var existsCmd = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM rating_likes WHERE rating_id = @rId AND user_id = @uId)", conn);
            existsCmd.Parameters.AddWithValue("rId", ratingId);
            existsCmd.Parameters.AddWithValue("uId", userId);
            bool alreadyLiked = (bool)(await existsCmd.ExecuteScalarAsync() ?? false);

            using var transaction = await conn.BeginTransactionAsync();
            try
            {
                if (alreadyLiked)
                {
                    //#########//
                    // UNLIKE  //
                    //#########//
                    string deleteLike = "DELETE FROM rating_likes WHERE rating_id = @rId AND user_id = @uId;";
                    using var cmdDel = new NpgsqlCommand(deleteLike, conn, transaction);
                    cmdDel.Parameters.AddWithValue("rId", ratingId);
                    cmdDel.Parameters.AddWithValue("uId", userId);
                    await cmdDel.ExecuteNonQueryAsync();

                    string subUpdate = "UPDATE ratings SET likes_count = GREATEST(0, likes_count - 1) WHERE id = @rId;";
                    using var cmdSub = new NpgsqlCommand(subUpdate, conn, transaction);
                    cmdSub.Parameters.AddWithValue("rId", ratingId);
                    await cmdSub.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                    return (200, "Unliked.");
                }
                else
                {
                    //#######//
                    // LIKE  //
                    //#######//
                    string insertLike = "INSERT INTO rating_likes (rating_id, user_id) VALUES (@rId, @uId);";
                    using var cmdIns = new NpgsqlCommand(insertLike, conn, transaction);
                    cmdIns.Parameters.AddWithValue("rId", ratingId);
                    cmdIns.Parameters.AddWithValue("uId", userId);
                    await cmdIns.ExecuteNonQueryAsync();

                    string addUpdate = "UPDATE ratings SET likes_count = likes_count + 1 WHERE id = @rId;";
                    using var cmdAdd = new NpgsqlCommand(addUpdate, conn, transaction);
                    cmdAdd.Parameters.AddWithValue("rId", ratingId);
                    await cmdAdd.ExecuteNonQueryAsync();

                    await transaction.CommitAsync();
                    return (201, "Liked.");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (500, $"Transaction error: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            return (500, $"Database error: {ex.Message}");
        }
    }
}