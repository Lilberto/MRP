using System.Net;
using System.Text;
using System.Text.Json;

namespace Profile_Site
{
    public static class Profile_Structure
    {
        public static async Task ProfileSite(HttpListenerContext context, Dictionary<string, string> routeParams)
        {
            var request = context.Request;
            var response = context.Response;

            string username = routeParams["username"];

            // 1️⃣ Token prüfen
            string authHeader = request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                response.StatusCode = 401;
                await WriteJson(response, new { error = "Missing or invalid Authorization header" });
                return;
            }

            string token = authHeader.Substring("Bearer ".Length).Trim();

            // 2️⃣ Request Body lesen
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            string body = await reader.ReadToEndAsync();

            var loginData = JsonSerializer.Deserialize<User>(body);
            if (loginData == null)
            {
                response.StatusCode = 400;
                await WriteJson(response, new { error = "Invalid JSON body" });
                return;
            }

            // 3️⃣ Profildaten holen
            var user = Profile_Check.ProfileUser(loginData.Username, loginData.Password, token);

            if (user == null)
            {
                response.StatusCode = 401;
                await WriteJson(response, new { error = "Unauthorized or invalid credentials" });
                return;
            }

            // 4️⃣ Erfolgreiche Antwort
            response.StatusCode = 200;
            await WriteJson(response, new
            {
                message = "Profile loadded successfully",
                profile = new
                {
                    user.Id,
                    user.Username,
                    user.CreatedAt,
                    user.TotalRatings,
                    user.AverageScore,
                    user.FavoriteGenre
                }
            });
        }

        private static async Task WriteJson(HttpListenerResponse response, object obj)
        {
            string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}
