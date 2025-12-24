using System.Net;
using System.Text;
using System.Text.Json;

namespace Error_401
{
    public class Error401
    {
        public static void E_401(HttpListenerResponse response)
        {
            response.StatusCode = 401;
            response.StatusDescription = "Unauthorized";
            response.ContentType = "application/json";
            
            var errorResponse = new { 
                error = "Unauthorized" 
            };
            
            string json = JsonSerializer.Serialize(errorResponse);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }
    }
}