namespace Body_request;

using System.Net;

public static class Body_Request
{
    public static async Task<string> Body_Data(HttpListenerRequest request)
    {
        using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
        string body = await reader.ReadToEndAsync();

        return body;
    }
}