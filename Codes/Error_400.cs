using System.Net;
using System.Text;
using System.Text.Json;

namespace Error_400
{
    public class Error400
    {
        public static async Task E_400(HttpListenerContext context)
        {
            context.Response.StatusCode = 400;
            byte[] buffer = Encoding.UTF8.GetBytes("Bad Request");
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }
    }
}