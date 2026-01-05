namespace Login_Service;

using Npgsql;

// utils
using DBConnection;
using Hash_util;

public static class LoginService
{
    public static async Task<(int StatusCode, string Message, object Data)> LoginUser(UserRegisterDTO loginData)
    {
        try
        {
            using var conn = DbFactory.GetConnection();
            await conn.OpenAsync();

            string checkQuery = "SELECT salt, password_hash FROM users WHERE username = @username;";
            using var checkCmd = new NpgsqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@username", loginData.Username);
            checkCmd.Parameters.AddWithValue("@password", loginData.Password);
            
            using var reader = await checkCmd.ExecuteReaderAsync();
            if(!await reader.ReadAsync())
            {
                return (401, "Invalid username or password", new { });
            }

            string dbSalt = reader.GetString(0);
            string dbHash = reader.GetString(1);
            reader.Close();

            string computedHash = Hash.HashPassword(loginData.Password, dbSalt);

            if (computedHash == dbHash)
            {
                string rawToken = $"{loginData.Username}-mrptoken-{Token.SecureRandom.GenerateTokenPart()}";
                string token = Token.TokenHash.HashToken(rawToken);

                string updateQuery = "UPDATE users SET token = @token, token_created_At = CURRENT_TIMESTAMP WHERE username = @username";
                using var updateCmd = new NpgsqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@token", token);
                updateCmd.Parameters.AddWithValue("@username", loginData.Username);

                var insertToken = await updateCmd.ExecuteNonQueryAsync();
                Console.WriteLine($"User '{loginData.Username}' successfuly registered token: {token}");

                return (200, "Login successful", new { Token = rawToken });
            }
            else
            {
                Console.WriteLine("Wrong login");
                return (401, "Invalid username or password", new { });
            }
        }
        catch (NpgsqlException ex)
        {
            return (503, $"Database error: {ex.Message}", new { });
        }
        catch (Exception ex)
        {
            return (500, $"Internal error: {ex.Message}", new { });
        }
    }
}