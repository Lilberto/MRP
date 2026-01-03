using System.Net;
using System.Text;
using System.Text.Json;

namespace Error_409
{
    public class Error409
    {
        public static async Task E_409(HttpListenerResponse response, object result)
        {
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            response.StatusCode = 409;
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}