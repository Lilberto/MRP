namespace Recommendations_Service;

using System.Net;
using System.Text.Json;

// utils
using Token;
using Auth_util;
using Body_request;

// codes
using Code_201;
using Code_200;
using Error_400;
using Error_404;
using Error_409;
using Error_500;

public class Recommendations_Service
{
    private const string ConnectionString = "Host=localhost;Port=5432;Database=mrp_db;Username=admin;Password=mrp123;";

    public static async Task<(int StatusCode, string Message, List<MediaDto> Data)> Recommendations_Logic(int userId)
    {
        try
        {
            using var conn = new Npgsql.NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            //#################################//
            // Check for total media entries   //
            // not allowed to be from the user //
            //#################################//
            string sysCheckQuery = @"SELECT COUNT(*) FROM media_entries WHERE user_id != @user_id";
            using var sysCmd = new Npgsql.NpgsqlCommand(sysCheckQuery, conn);
            sysCmd.Parameters.AddWithValue("user_id", userId);

            var countObj = await sysCmd.ExecuteScalarAsync();
            long otherMediaCount = countObj is long l ? l : 0;

            if (otherMediaCount < 5)
            {
                return (200, "Not enough media from other users available", new List<MediaDto>());
            }


            //##########################################//
            // Check user rating history (ratings >= 4) //
            //##########################################//
            string userHistoryQuery = "SELECT EXISTS(SELECT 1 FROM ratings WHERE user_id=@user_id AND stars >= 4)";
            using var historyCmd = new Npgsql.NpgsqlCommand(userHistoryQuery, conn);
            historyCmd.Parameters.AddWithValue("user_id", userId);
            bool hasHistory = (bool)(await historyCmd.ExecuteScalarAsync() ?? false);


            //#############################//
            // Fetch recommendations logic //
            // 1. Based on history         //
            // 2. Trending highlights      //
            //#############################//
            string recQuery = @"
                WITH UserRatedGenres AS (
                    SELECT DISTINCT mg.genre 
                    FROM ratings r
                    JOIN media_genres mg ON r.media_id = mg.media_id
                    WHERE r.user_id = @user_id AND r.stars >= 4
                )
                
                SELECT m.id, m.title, m.media_type, m.avg_score, m.description, m.release_year, 
                    m.age_restriction, m.user_id,
                    (SELECT STRING_AGG(genre, ',') FROM media_genres WHERE media_id = m.id)
                FROM media_entries m
                LEFT JOIN media_genres mg ON m.id = mg.media_id
                WHERE m.user_id != @user_id 
                AND m.id NOT IN (SELECT media_id FROM ratings WHERE user_id = @user_id)
                AND (mg.genre IN (SELECT genre FROM UserRatedGenres) OR NOT EXISTS (SELECT 1 FROM UserRatedGenres))
                GROUP BY m.id, m.title, m.media_type, m.avg_score, m.description, m.release_year, m.age_restriction, m.user_id
                ORDER BY m.avg_score DESC LIMIT 5;
            ";

            using var recCmd = new Npgsql.NpgsqlCommand(recQuery, conn);
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
                    Genres = reader.IsDBNull(8) ? new List<string>() : reader.GetString(8).Split(',').ToList()
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
            Console.WriteLine($"[STACKTRACE]: {ex.StackTrace}");
            return (500, "Internal Server Error", new List<MediaDto>());
        }
    }
}