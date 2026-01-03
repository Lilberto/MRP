namespace Hash_util;

public static class Hash
{
    public static string GenerateSalt()
    {
        return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
    }
    
    public static string HashPassword(string password, string salt)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var saltedPassword = password + salt;
        var bytes = System.Text.Encoding.UTF8.GetBytes(saltedPassword);
        var hash = sha256.ComputeHash(bytes);
        
        return Convert.ToBase64String(hash);
    }
}