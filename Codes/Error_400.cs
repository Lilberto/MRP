using System.Net;
using System.Text;
using System.Text.Json;

namespace Error_400
{
    public class Error400
    {
        public static async Task E_400(HttpListenerResponse response, object result)
        {
            string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
            byte[] buffer = Encoding.UTF8.GetBytes(json);

            response.StatusCode = 400; 
            response.ContentType = "application/json";
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);            
            response.OutputStream.Close();
        }
    }
}