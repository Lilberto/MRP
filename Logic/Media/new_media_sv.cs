namespace Media_insert;

using Npgsql;

//* utils
using DBConnection;

public class Media_insert_service
{
    public static async Task<(int StatusCode, string Message, Media)> Media_insert(Media media, int User_ID)
    {
        try
        {
            using var conn = DbFactory.GetConnection();
            await conn.OpenAsync();

            string MediaInsertQuery = @"
                WITH inserted AS (
                    INSERT INTO media_entries (user_id, title, description, media_type, release_year, age_restriction) 
                    VALUES (@userId, @title, @description, @mediaType, @releaseYear, @ageRestriction)
                    RETURNING *
                )
                SELECT i.*, u.username 
                FROM inserted i
                JOIN users u ON i.user_id = u.id;
            ";

            using var insertCMD = new NpgsqlCommand(MediaInsertQuery, conn);
            insertCMD.Parameters.AddWithValue("@userId", User_ID);
            insertCMD.Parameters.AddWithValue("@title", media.title);
            insertCMD.Parameters.AddWithValue("@description", (object)media.description ?? DBNull.Value);
            insertCMD.Parameters.AddWithValue("@mediaType", media.type);
            insertCMD.Parameters.AddWithValue("@releaseYear", media.year);
            insertCMD.Parameters.AddWithValue("@ageRestriction", media.agerating);

            using var reader = await insertCMD.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                media.id = reader.GetInt32(reader.GetOrdinal("id"));
                media.userid = reader.GetInt32(reader.GetOrdinal("user_id"));
                media.username = reader.GetString(reader.GetOrdinal("username"));
                media.title = reader.GetString(reader.GetOrdinal("title"));
                media.description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString(reader.GetOrdinal("description"));
                media.type = reader.GetString(reader.GetOrdinal("media_type"));
                media.year = reader.GetInt32(reader.GetOrdinal("release_year"));
                media.agerating = reader.GetString(reader.GetOrdinal("age_restriction"));

                return (201, "Media created.", media);
            }

            return (500, "Insert failed.", media);
        }
        catch (Exception ex)
        {
            return (500, $"Error: {ex.Message}", media);
        }
    }
}