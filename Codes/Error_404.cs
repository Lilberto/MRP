using System.Net;
using System.Text;
using System.Text.Json;

namespace Error_404
{
    public class Error404
    {
        public static async Task E_404(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            byte[] buffer = Encoding.UTF8.GetBytes("Not Found");
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.Close();
        }
    }
}