using System.Net;
using System.Text;
using System.Text.Json;

namespace Error_403
{
    public class Error403
    {
        public static async Task E_403(HttpListenerResponse response, object? result = null)
        {
            response.StatusCode = 403;
            response.ContentType = "application/json";

            var finalResult = result ?? new { error = "Forbidden", message = "You do not have permission to access this resource." };

            string json = JsonSerializer.Serialize(finalResult);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}