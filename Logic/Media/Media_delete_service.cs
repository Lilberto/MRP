using System.Data.Common;
using System.Runtime.InteropServices;
using Npgsql;
namespace Media_Delete_Logic;

public class MediaDeleteLogic
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static int DeleteMedia(int mediaID, int userID)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        using var checkCmd = new NpgsqlCommand("SELECT user_id FROM media_entries WHERE id = @id", conn);
        checkCmd.Parameters.AddWithValue("id", mediaID);
        var ownerId = checkCmd.ExecuteScalar();

        if (ownerId == null) return 404; // Does not exist
        if ((int)ownerId != userID) return 403; // Another user created the media

        using var transaction = conn.BeginTransaction();

        try
        {
            string MediaExtractSingleQuery = @"DELETE FROM media_entries WHERE id = @id;";
            using var CMD = new NpgsqlCommand(MediaExtractSingleQuery, conn);
            CMD.Parameters.AddWithValue("id", mediaID);

            int result = CMD.ExecuteNonQuery();

            transaction.Commit();

            return result > 0 ? 200 : 500;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update Error: {ex.Message}");
            transaction.Rollback();
            return 500;
        }
    }
}