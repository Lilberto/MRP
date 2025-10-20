using System.Net;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LeaderboardLogic
{
    public static class LeaderboardStructure
    {

        public static async Task LeaderboardSite(HttpListenerContext context, Dictionary<string, string> routeParams)
        {
            var request = context.Request;
            var response = context.Response;

            // Token aus dem Header lesen
            string authHeader = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                response.StatusCode = 401;
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Unauthorized"));
                return;
            }

            string Token = authHeader.Substring("Bearer ".Length);

            // Token-Check (ganz simpel hier)
            if (Token != "alice-mrpToken" && Token != "bob-mrpToken")
            {
                response.StatusCode = 403;
                await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Forbidden"));
                return;
            }
    
            // Leaderboard generieren
            var leaderboard = MediaRepository.GetAll()
                .Select(m => new
                {
                    Title = m.Title,
                    RatingCount = m.RatingCount,
                    AverageScore = m.AverageScore,
                    FavoritesCount = m.Favorites.Count
                })
                .OrderByDescending(m => m.AverageScore)
                .ToList();

            string json = JsonSerializer.Serialize(leaderboard, new JsonSerializerOptions { WriteIndented = true });
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            return;

        }
    }
}