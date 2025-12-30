namespace Token;

using System.Net;

using DBTokenValid;

// Error codes
using Error_401;

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

        // string hashedToken = TokenHash.HashToken(token);
        // bool isValid = await DB_token.ValidateTokenDB(hashedToken);
        
        // if (!isValid)
        // {
        //     Error401.E_401(response);
        //     return null;
        // }
        return token;
    }
}

public class TokenHash
{
    public static string HashToken(string token)
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(token);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}