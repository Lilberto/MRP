namespace FavListService;

using System.Threading.Tasks;
using Npgsql;

using DBConnection;


public class Fav_List_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static async Task<(int StatusCode, List<Media> Data)> Fav_List_Logic(string username, string token, int userId)
    {
//        using var conn = DbFactory.GetConnection();
        using var conn = new NpgsqlConnection(ConnectionString);

        await conn.OpenAsync();

        var favs = new List<Media>();

        try
        {
            // Check user & token match
            Console.WriteLine($"--Username: {username}\n--Token: {token}\n--UserID: {userId}");
            using var checkUserCmd = new NpgsqlCommand("SELECT EXISTS(SELECT 1 FROM users WHERE token = @token and username = @username);", conn);
            checkUserCmd.Parameters.AddWithValue("token", token);
            checkUserCmd.Parameters.AddWithValue("username", username);

            bool exists = (bool?)await checkUserCmd.ExecuteScalarAsync() ?? false;

            Console.WriteLine($"--User Exists: {exists}");

            if (!exists)
            {
                return (404, favs);
            }

            string favQuery = @"
                SELECT 
                    m.id, 
                    m.title, 
                    m.media_type, 
                    m.release_year, 
                    m.avg_score
                FROM favorites f
                JOIN media_entries m ON f.media_id = m.id
                WHERE f.user_id = @userId;
            ";

            using var getFavCmd = new NpgsqlCommand(favQuery, conn);
            getFavCmd.Parameters.AddWithValue("userId", userId);

            using var reader = await getFavCmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var media = new Media
                {
                    id = reader.GetInt32(0),
                    title = reader.GetString(1),
                    type = reader.GetString(2),
                    year = reader.GetInt32(3),
                };
                favs.Add(media);
            }

            return (200, favs);
        }
        catch (Exception)
        {
            return (500, favs);
        }
    }
}