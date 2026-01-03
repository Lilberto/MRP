namespace ConfirmCommentLogic;

using Npgsql;

public class Confirm_Comment_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int Confirm_Comment_Logic(int ratingId, int userId, Rating ratingData)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        // Console.WriteLine($"Opened DB connection for confirming comment on rating ID: {ratingId} by user ID: {userId}");
        
        // // checks if the rating exists
        // using var checkCmd = new NpgsqlCommand("SELECT user_id FROM ratings WHERE id = @id;", conn);
        // checkCmd.Parameters.AddWithValue("id", ratingId);
        // var ownerId = checkCmd.ExecuteScalar();

        // Console.WriteLine($"Owner ID from DB: {ownerId}");

        // if (ownerId == null) return 404; // not found any userid
        // if ((int)ownerId != userId) return 403; // forbidden: user is NOT the owner of this rating

        // Console.WriteLine($"User {userId} is the owner of rating {ratingId}, proceeding to confirm comment.");

        try
        {
            string ConfirmQuery = @"
                UPDATE ratings
                SET
                    comment_published = TRUE
                WHERE id = @id;
            ";

            using var CMD = new NpgsqlCommand(ConfirmQuery, conn);
            CMD.Parameters.AddWithValue("id", ratingId);

            int result = CMD.ExecuteNonQuery();

            if(result == 0) return 404;

            return 200;
        }        
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
            return 500;
        }
    }
}