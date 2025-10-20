using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RegisterLogic
{
    public static class RegisterStructure
    {

        public static async Task RegisterSite(HttpListenerContext context, Dictionary<string, string> routeParams)
        {
            var request = context.Request;
            var response = context.Response;

            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            string body = await reader.ReadToEndAsync();
            var registerData = JsonSerializer.Deserialize<User>(body);

            bool RegisterValue = Register_Check.RegisterUser(registerData.Username, registerData.Password);

            if (RegisterValue == false)
            {
                response.StatusCode = 401;
            }
            else if (RegisterValue == true)
            {

                // In a real application, you would save the user to a database here.
                var result = new { message = "Registration successful" };
                string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                response.StatusCode = 201;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            return;

        }
    }
}