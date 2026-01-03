using System.Net;
using System.Text;
using System.Text.Json;

namespace Error_503;

public class Error503
{
    public static async Task E_503(HttpListenerResponse response, object result)
    {
        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        byte[] buffer = Encoding.UTF8.GetBytes(json);

        response.ContentType = "application/json";
        response.ContentLength64 = buffer.Length;
        response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

        response.AddHeader("Retry-After", "30");

        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}
