using System.Net;
using System.Text;
using System.Text.Json;

namespace Body_request;

public static class Body_Request
{
    public static async Task<string> Body_Data(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();

        return body;
    }
}