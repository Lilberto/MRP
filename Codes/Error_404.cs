using System.Net;
using System.Text;
using System.Text.Json;

namespace Error_404
{
    public class Error404
    {
        public static async Task E_404(HttpListenerResponse response, object? result = null)
        {
            response.StatusCode = 404;
            response.ContentType = "application/json";

            var finalResult = result ?? new { error = "Not Found", message = "The requested resource was not found." };

            string json = JsonSerializer.Serialize(finalResult);
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}