using System.Net;
using System.Text;
using System.Text.Json;

namespace LoginLogic
{
    public static class LoginStructure
    {

        public static async Task LoginSite(HttpListenerContext context, Dictionary<string, string> routeParams)
        {
            var request = context.Request;
            var response = context.Response;

            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            string body = await reader.ReadToEndAsync();
            var loginData = JsonSerializer.Deserialize<User>(body);

            var user_status = Login_Check.LoginUser(loginData.Username, loginData.Password);

            if (user_status == false)
                response.StatusCode = 401;

            else if (user_status == true)
            {
                string token = $"{loginData.Username}-mrpToken";

                loginData.Token = token;

                var result = new { message = "Login successful", token = $"{loginData.Username}-mrpToken" };
                string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 200;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

        }
    }
}