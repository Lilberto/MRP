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
        using var checkCmd = new NpgsqlCommand("SELECT id FROM media_entries WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", mediaId);
        var mediaExists = await checkCmd.ExecuteScalarAsync();
        
        if (mediaExists == null) 
            return (404, "Media not found");

        string createRatingQuery = @"
            INSERT INTO ratings (user_id, media_id, stars, comment, created_at, updated_at)
            VALUES (@user_id, @media_id, @stars, @comment, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            RETURNING id;";

        using var cmd = new NpgsqlCommand(createRatingQuery, conn);
        cmd.Parameters.AddWithValue("user_id", userId);
        cmd.Parameters.AddWithValue("media_id", mediaId);
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