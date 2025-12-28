using Npgsql;

namespace NewRatingLogic;

public class New_Rating_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int New_Rating_Logic(int mediaId, int userId, Rating RatingData)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        // checks if the media exists
        using var checkCmd = new NpgsqlCommand("SELECT id FROM media_entries WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", mediaId);
        if (checkCmd.ExecuteScalar() == null) return 404;

        try
        {
            string CreateRatingQuery = @"
                INSERT INTO ratings (user_id, media_id, stars, comment, created_at, updated_at)
                VALUES (@user_id, @media_id, @stars, @comment, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
                RETURNING id;
            ";

            using var CMD = new NpgsqlCommand(CreateRatingQuery, conn);
            CMD.Parameters.AddWithValue("user_id", userId);
            CMD.Parameters.AddWithValue("media_id", mediaId);
            CMD.Parameters.AddWithValue("stars", RatingData.Stars!);
            CMD.Parameters.AddWithValue("comment", (object)RatingData.Comment! ?? DBNull.Value);

            var result = CMD.ExecuteScalar();

            Console.WriteLine($"Stars: {RatingData.Stars}, \nComment: {RatingData.Comment}");

            return result != null ? 201 : 500;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation in db
        {
            // User already rated this media
            return 409;
        }
        catch (Exception)
        {
            return 500;
        }
    }
}