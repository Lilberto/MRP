namespace Recommendations_Service;

using Npgsql;

//* utils
using DBConnection;

public class Recommendations_Service
{
    public static async Task<(int StatusCode, string Message, List<MediaDto> Data)> Recommendations_Logic(int userId)
    {
        using var conn = DbFactory.GetConnection();
        await conn.OpenAsync();
        
        try
        {
            //############################################//
            // Check if the user exists in the database   //
            //############################################//
            string userExistsQuery = "SELECT EXISTS(SELECT 1 FROM users WHERE id = @user_id)";
            using var userCheckCmd = new NpgsqlCommand(userExistsQuery, conn);
            userCheckCmd.Parameters.AddWithValue("user_id", userId);
            bool userExists = (bool)(await userCheckCmd.ExecuteScalarAsync() ?? false);

            if (!userExists)
            {
                return (404, "User not found", new List<MediaDto>());
            }

            //#################################//
            // Check for total media entries   //
            // not allowed to be from the user //
            //#################################//
            string sysCheckQuery = @"SELECT COUNT(*) FROM media_entries WHERE user_id != @user_id";
            using var sysCmd = new NpgsqlCommand(sysCheckQuery, conn);
            sysCmd.Parameters.AddWithValue("user_id", userId);

            var countObj = await sysCmd.ExecuteScalarAsync();
            long otherMediaCount = countObj is long l ? l : 0;

            if (otherMediaCount < 5)
            {
                return (200, "Not enough media from other users available", new List<MediaDto>());
            }

            //######################################################//
            // Verify if user has high-rated content (ratings >= 4) //
            //######################################################//
            string userHistoryQuery = "SELECT EXISTS(SELECT 1 FROM ratings WHERE user_id = @user_id AND stars >= 4)";
            using var historyCmd = new NpgsqlCommand(userHistoryQuery, conn);
            historyCmd.Parameters.AddWithValue("user_id", userId);
            bool hasHistory = (bool)(await historyCmd.ExecuteScalarAsync() ?? false);


            //###########################################################//
            // Fetch recommendations logic                               //
            // 1. Identifies genres from user's highly-rated media       //
            // 2. Selects media details and aggregates associated genres //
            // 3. Filters unrated content and sorts by score             //
            //###########################################################//
            string recQuery = @"
                WITH UserRatedGenres AS (
                    SELECT DISTINCT mg.genre 
                    FROM ratings r
                    JOIN media_genres mg ON r.media_id = mg.media_id
                    WHERE r.user_id = @user_id AND r.stars >= 4
                )
                
                SELECT 
                    m.id, 
                    m.title, 
                    m.media_type,
                    m.avg_score,                    
                    m.description, 
                    m.release_year, 
                    m.age_restriction, 
                    m.user_id,
                    u.username, 
                    (SELECT STRING_AGG(genre, ',') FROM media_genres WHERE media_id = m.id) AS genres
                FROM media_entries m
                JOIN users u ON m.user_id = u.id 
                LEFT JOIN media_genres mg ON mg.media_id = m.id

                WHERE m.user_id != @user_id
                AND m.id NOT IN (SELECT media_id FROM ratings WHERE user_id = @user_id)
                AND (
                    mg.genre IN (SELECT genre FROM UserRatedGenres)
                    OR NOT EXISTS (SELECT 1 FROM UserRatedGenres)
                )
                GROUP BY 
                    m.id, m.title, m.media_type, m.avg_score, m.description,
                    m.release_year, m.age_restriction, m.user_id, u.username
                ORDER BY m.avg_score DESC
                LIMIT 5;
            ";

            using var recCmd = new NpgsqlCommand(recQuery, conn);
            recCmd.Parameters.AddWithValue("user_id", userId);
            using var reader = await recCmd.ExecuteReaderAsync();

            //##############//
            // Data Mapping //
            //##############//
            var recommendations = new List<MediaDto>();
            while (await reader.ReadAsync())
            {
                recommendations.Add(new MediaDto
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Type = reader.GetString(2),
                    Score = Convert.ToDouble(reader.GetValue(3)),
                    Description = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Year = reader.GetInt32(5),
                    AgeRating = reader.GetString(6),
                    CreatorId = reader.GetInt32(7),
                    Username = reader.GetString(8),
                    Genres = reader.IsDBNull(9) ? new List<string>() : reader.GetString(9).Split(',').ToList()
                });
            }


            string msg = (hasHistory && recommendations.Count > 0)
                ? "Based on your rating history"
                : "Trending highlights for you";

            return (200, msg, recommendations);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL SERVICE ERROR]: {ex.Message}");
            return (500, "Internal Server Error", new List<MediaDto>());
        }
    }
}