namespace FavListService;

using System.Threading.Tasks;
using Npgsql;

//* utils
using DBConnection;

public class Fav_List_Service
{
    public static async Task<(int StatusCode, string Message, List<Media> Data)> Fav_List_Logic(int userId, string username)
    {
        var favs = new List<Media>();
        try
        {
            using var conn = DbFactory.GetConnection();
            await conn.OpenAsync();

            //#########################//
            // User has access to list //
            //#########################//
            using var checkCmd = new NpgsqlCommand("SELECT username FROM users WHERE id = @id", conn);
            checkCmd.Parameters.AddWithValue("id", userId);
            string? actualUsername = await checkCmd.ExecuteScalarAsync() as string;

            if (actualUsername == null) return (404, "User not found.", favs);

            if (!string.Equals(actualUsername, username, StringComparison.OrdinalIgnoreCase))
            {
                return (403, "Access denied: This is not your favorite list.", favs);
            }

            //###############################################//
            // 1. Fetch media details with aggregated genres //
            // 2. Link favorites to media and genres         //
            // 3. Filter by user and organize results        //
            //###############################################//
            string sql = @"
                SELECT 
                    m.id, m.user_id, u.username, m.title, m.description, 
                    m.media_type, m.release_year, m.age_restriction, m.avg_score, m.created_at,
                    STRING_AGG(mg.genre, ',') as genres
                
                FROM favorites f
                JOIN media_entries m ON f.media_id = m.id
                JOIN users u ON m.user_id = u.id
                LEFT JOIN media_genres mg ON m.id = mg.media_id
                
                WHERE f.user_id = @userId
                GROUP BY m.id, u.username, f.created_at
                ORDER BY f.created_at DESC
            ";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("userId", userId);

            using var reader = await cmd.ExecuteReaderAsync();

            //#######################################//
            // Convert query results to media objects//
            //#######################################//
            while (await reader.ReadAsync())
            {
                string rawGenres = reader.IsDBNull(10) ? "" : reader.GetString(10);

                favs.Add(new Media
                {
                    id = reader.GetInt32(0),
                    userid = reader.GetInt32(1),
                    username = reader.GetString(2),
                    title = reader.GetString(3),
                    description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    type = reader.GetString(5),
                    year = reader.GetInt32(6),
                    agerating = reader.GetString(7),
                    score = (double)reader.GetDecimal(8),
                    created = reader.GetDateTime(9),
                    genres = string.IsNullOrEmpty(rawGenres) ? new() : rawGenres.Split(',').ToList()
                });
            }

            return (200, "Favorites retrieved.", favs);
        }
        catch (Exception)
        {
            return (500, "Internal Error", new List<Media>());
        }
    }
}