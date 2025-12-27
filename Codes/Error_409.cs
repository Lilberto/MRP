using System.Net;
using System.Text;

namespace Error_409
{
    public class Error409
    {
        public static async Task E_409(HttpListenerContext context)
        {
            context.Response.StatusCode = 409;
            context.Response.ContentType = "text/plain";
            byte[] buffer = Encoding.UTF8.GetBytes("Conflict: Resource already exists");
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }
    }
}