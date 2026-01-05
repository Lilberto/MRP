namespace Media_update_logic;

using Npgsql;

//* utils
using DBConnection;
using System.Diagnostics.CodeAnalysis;

public class Media_update_service
{
    public static async Task<(int StatusCode, string Message, MediaUpdateDto Media)> Media_update(int mediaID, int userID, MediaUpdateDto data)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();

        try
        {
            using var checkCmd = new NpgsqlCommand("SELECT user_id FROM media_entries WHERE id = @id", conn);
            checkCmd.Parameters.AddWithValue("id", mediaID);
            var ownerIdObj = await checkCmd.ExecuteScalarAsync();

            if (ownerIdObj == null) return (404, "Media not found", null!);
            if (Convert.ToInt32(ownerIdObj) != userID) return (403, "Forbidden: Not owner", null!);
        }
        catch (Exception ex)
        {
            return (500, $"Check failed: {ex.Message}", null!);
        }

        using var transaction = await conn.BeginTransactionAsync();
        try
        {
            using var checkCmd = new NpgsqlCommand("SELECT user_id FROM media_entries WHERE id = @id", conn);
            checkCmd.Parameters.AddWithValue("id", mediaID);
            var ownerIdObj = await checkCmd.ExecuteScalarAsync();

            if (ownerIdObj == null) return (404, "Media not found", null!);
            if (Convert.ToInt32(ownerIdObj) != userID) return (403, "Forbidden: You are not the owner", null!); 


            string MediaExtractSingleQuery = @"
                UPDATE media_entries
                SET title = @title, description = @description, media_type = @media_type,
                    release_year = @release_year, age_restriction = @age_restriction,
                    updated_at = CURRENT_TIMESTAMP

                WHERE id = @id AND user_id = @user_id
                RETURNING id;
            ";

            using var cmd = new NpgsqlCommand(MediaExtractSingleQuery, conn);
            cmd.Parameters.AddWithValue("id", mediaID);
            cmd.Parameters.AddWithValue("user_id", userID);
            cmd.Parameters.AddWithValue("title", data.title);
            cmd.Parameters.AddWithValue("description", (object?)data.description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("media_type", data.type);
            cmd.Parameters.AddWithValue("release_year", data.year);
            cmd.Parameters.AddWithValue("age_restriction", data.agerating);

            await cmd.ExecuteScalarAsync();

            using (var delCmd = new NpgsqlCommand("DELETE FROM media_genres WHERE media_id = @id", conn))
            {
                delCmd.Parameters.AddWithValue("id", mediaID);
                await delCmd.ExecuteNonQueryAsync();
            }

            foreach (var genre in data.genres)
            {
                using var insCmd = new NpgsqlCommand("INSERT INTO media_genres (media_id, genre) VALUES (@id, @genre)", conn);
                insCmd.Parameters.AddWithValue("id", mediaID);
                insCmd.Parameters.AddWithValue("genre", genre);
                await insCmd.ExecuteNonQueryAsync();
            }

            data.id = mediaID; 
            data.userid = userID;
            await transaction.CommitAsync();
            return (200, "Media updated successfully", data);

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (500, $"Internal server error: {ex.Message}", null!);
        }

    }
}