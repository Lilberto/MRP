namespace Logout_Service;

using Npgsql;

//* utils
using DBConnection;


public static class LogoutService
{    
    public static async Task<(int StatusCode, string Message)> LogoutUser(string token)
    {
        try
        {
            using var conn = DbFactory.GetConnection();
            await conn.OpenAsync();

            string updateQuery = "UPDATE users SET token_created_At = NULL, token = NULL WHERE token = @token;";
            using var Cmd = new NpgsqlCommand(updateQuery, conn);
            Cmd.Parameters.AddWithValue("@token", token);

            await Cmd.ExecuteNonQueryAsync();

            return (200, "Logout successful");
        }
        catch (NpgsqlException)
        {
            return (503, "Database error during logout");
        }
        catch (Exception)
        {
            return (500, "General error during logout");
        }
    }
}