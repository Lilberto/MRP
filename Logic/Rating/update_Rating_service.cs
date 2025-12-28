using Npgsql;

namespace UpdateRatingLogic;

public class Update_Rating_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int Update_Rating_Logic(int ratingId, int userId, Rating RatingData)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        // checks if the rating exists
        using var checkCmd = new NpgsqlCommand("SELECT user_id FROM ratings WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", ratingId);
        var ownerId = checkCmd.ExecuteScalar();

        Console.WriteLine($"OwnerID: {ownerId}");

        if (ownerId == null) return 404; // does not exisits
        if ((int)ownerId != userId) return 403; // another users rating

        try
        {
            string CreateRatingQuery = @"
                UPDATE ratings 
                SET 
                    stars = @stars, 
                    comment = @comment, 
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @rating_id AND user_id = @user_id;
            ";

            using var CMD = new NpgsqlCommand(CreateRatingQuery, conn);
            CMD.Parameters.AddWithValue("user_id", userId);
            CMD.Parameters.AddWithValue("rating_id", ratingId);
            CMD.Parameters.AddWithValue("stars", RatingData.Stars!);
            CMD.Parameters.AddWithValue("comment", (object)RatingData.Comment! ?? DBNull.Value);

            var result = CMD.ExecuteNonQuery();

            Console.WriteLine($"Stars: {RatingData.Stars}, \nComment: {RatingData.Comment}");

            return result > 0 ? 200 : 500;
        }
        catch (Exception)
        {
            return 500;
        }
    }
}