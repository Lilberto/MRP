using System.Net;
using System.Text;
using System.Text.Json;

using Error_401;

namespace Token;

public static class Tokens
{
    public static async Task<string?> TokenValidate(HttpListenerRequest request, HttpListenerResponse response)
    {
        string? authHeader = request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            Error401.E_401(response);
            return null;
        }

        string token = authHeader.Substring("Bearer ".Length).Trim();

        return token;
    }
}
