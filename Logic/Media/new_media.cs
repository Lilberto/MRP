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
                INSERT INTO media_entries (user_id, title, description, media_type, release_year, age_restriction) 
                VALUES (@userId, @title, @description, @mediaType, @releaseYear, @ageRestriction)
                RETURNING id;
            ";
            using var insertCMD = new NpgsqlCommand(MediaInsertQuery, conn);

            insertCMD.Parameters.AddWithValue("@userId", User_ID);
            insertCMD.Parameters.AddWithValue("@title", media.title);
            insertCMD.Parameters.AddWithValue("@description", (object)media.description ?? DBNull.Value);
            insertCMD.Parameters.AddWithValue("@mediaType", media.type);
            insertCMD.Parameters.AddWithValue("@releaseYear", media.year);
            insertCMD.Parameters.AddWithValue("@ageRestriction", media.agerating);

            var result = await insertCMD.ExecuteScalarAsync();

            if (result != null)
            {
                int newId = Convert.ToInt32(result);
                
                media.id = newId; 
                media.userid = User_ID;

                return (201, "Media successfully created", media);
            }

            return (500, "Failed to retrieve new Media ID", media);

        }
        catch (NpgsqlException ex)
        {
            return (409, $"Database conflict: {ex.Message}", media);
        }
        catch (Exception ex)
        {
            return (500, $"Internal Server Error {ex.Message}", media);
        }
    }
}