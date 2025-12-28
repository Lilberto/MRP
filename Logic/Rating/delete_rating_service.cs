namespace DeleteRatingLogic;

using Npgsql;

public class delete_rating_service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int delete_rating_logic(int ratingId, int userId)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        // checks if the rating exists
        using var checkCmd = new NpgsqlCommand("SELECT user_id FROM ratings WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", ratingId);
        var ownerId = checkCmd.ExecuteScalar();

        if (ownerId == null) return 404; // not found any userid
        if ((int)ownerId != userId) return 403; // forbidden: user is NOT the owner of this rating

        try
        {
            using var cmd = new NpgsqlCommand("DELETE FROM ratings WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", ratingId);

            int result = cmd.ExecuteNonQuery();

            return result > 0 ? 200 : 500;
        }
        catch (PostgresException ex)
        {
            // Database specific error
            Console.WriteLine($"DB Error: {ex.Message} (Code: {ex.SqlState})");
            return 500;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General Error: {ex.Message}");
            return 500;
        }
    }
}