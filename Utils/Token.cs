namespace Token;

using System.Net;
using System.Security.Cryptography;

using DBTokenValid;

//* Error codes
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

        string hashedToken = TokenHash.HashToken(token);
        bool isValid = await DB_token.ValidateTokenDB(hashedToken);
        
        if (!isValid)
        {
            Error401.E_401(response);
            return null;
        }
        return hashedToken;
    }
}

public class TokenHash
{
    public static string HashToken(string token)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(token);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

public static class SecureRandom
{
    public static string GenerateTokenPart(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        
        return string.Create(length, chars, (buffer, alphabet) =>
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                int index = RandomNumberGenerator.GetInt32(alphabet.Length);
                buffer[i] = alphabet[index];
            }
        });
    }
}