using System.Net;
using System.Text;
using System.Text.Json;

namespace Code_201
{
    public class Code201
    {
        public static async Task C_201(HttpListenerResponse response, object result)
        {
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 201;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}