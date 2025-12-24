using System.Net;
using System.Text;
using System.Text.Json;

namespace Error_500
{
    public class Error500
    {
        public static async Task E_500(HttpListenerResponse response, object result)
        {
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 500;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}