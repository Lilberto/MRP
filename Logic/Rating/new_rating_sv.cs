namespace NewRatingLogic;

using Npgsql;

//* utils
using DBConnection;

public class New_Rating_Service
{
public static async Task<(int StatusCode, string Message)> New_Rating_Logic(int mediaId, int userId, Rating RatingData)
{
    using var conn = DbFactory.GetConnection();
    await conn.OpenAsync();

    try
    {
        //####################################################//
        // Check if media exists and is not owned by the user //
        //####################################################//
        using var checkCmd = new NpgsqlCommand("SELECT user_id FROM media_entries WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", mediaId);
        var mediaOwnerIdObj = await checkCmd.ExecuteScalarAsync();
        
        if (mediaOwnerIdObj == null) 
            return (404, "Media not found");

        //###########################################//
        // Prevent users from rating their own media //
        //###########################################//
        if (Convert.ToInt32(mediaOwnerIdObj) == userId)
                return (403, "You cannot rate your own media entries.");

        string createRatingQuery = @"
            INSERT INTO ratings (user_id, media_id, stars, comment, created_at, updated_at)
            VALUES (@uId, @mId, @stars, @comment, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            RETURNING id;
        ";

        using var cmd = new NpgsqlCommand(createRatingQuery, conn);
        cmd.Parameters.AddWithValue("uId", userId);
        cmd.Parameters.AddWithValue("mId", mediaId);
        cmd.Parameters.AddWithValue("stars", RatingData.Stars);
        cmd.Parameters.AddWithValue("comment", (object?)RatingData.Comment ?? DBNull.Value);

        var result = await cmd.ExecuteScalarAsync();

        return result != null 
            ? (201, "Rating created successfully") 
            : (500, "Failed to create rating");
    }
    catch (PostgresException ex) when (ex.SqlState == "23505")
    {
        return (409, "You have already rated this media. Use update instead.");
    }
    catch (Exception ex)
    {
        return (500, $"Internal Server Error: {ex.Message}");
    }
}
}