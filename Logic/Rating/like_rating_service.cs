namespace LikeRatingLogic;

using Npgsql;


public class Like_Rating_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int Like_Rating_Logic(int ratingId, int userId)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        // checks if the rating exists & is not from the current user
        using var checkCmd = new NpgsqlCommand("SELECT user_id FROM ratings WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", ratingId);
        var ownerId = checkCmd.ExecuteScalar();

        Console.WriteLine($"Before: \n--OwnerID: {ownerId}\n--UserID: {userId}");

        if (ownerId == null) return 404; // rating not found
        if ((int)ownerId == userId) return 400; // forbidden: user is not allowed to like his own rating

        Console.WriteLine("After");
        try
        {
            string LikeQuery = "INSERT INTO rating_likes (rating_id, user_id) VALUES (@rating_id, @user_id);";

            using var CMD = new NpgsqlCommand(LikeQuery, conn);
            CMD.Parameters.AddWithValue("rating_id", ratingId);
            CMD.Parameters.AddWithValue("user_id", userId);

            CMD.ExecuteNonQuery();

            return 201;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Unique Violation
        {
            return 409; // Conflict: Already liked
        }
        catch (Exception)
        {
            return 500;
        }
    }
}