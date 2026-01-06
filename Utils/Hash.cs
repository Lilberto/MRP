using System.Security.Cryptography;

namespace Hash_util;

public static class Hash
{
    public static string GenerateSalt()
    {
        byte[] salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        return Convert.ToBase64String(salt);
    }
    
    public static string HashPassword(string password, string salt)
    {
        byte[] saltBytes = Convert.FromBase64String(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password: password,
            salt: saltBytes,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256);
        
        byte[] hash = pbkdf2.GetBytes(32); // 32 Bytes = 256 Bit
        return Convert.ToBase64String(hash);
    }
}