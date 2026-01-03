namespace Register_Service;

using Npgsql;

// utils
using Hash_util;
using DBConnection;

public static class RegisterService
{
    public static async Task<(int StatusCode, string Message, List<UserRegisterDTO> Data)> RegisterUser(UserRegisterDTO User_data)
    {
        try
        {
            using var conn = DbFactory.GetConnection();
            await conn.OpenAsync();

            string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username;";
            using var checkCmd = new NpgsqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@username", User_data.Username);

            var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
            if (count > 0)
            {
                Console.WriteLine("User exists!");
                return (409, "User already exists", new List<UserRegisterDTO>());
            }

            //###################################//
            // Salt generation and Password hash //
            //###################################//
            string salt = Hash.GenerateSalt();
            string passwordHash = Hash.HashPassword(User_data.Password, salt);

            //################################//
            // Insert user with ALL required  //
            //################################//
            string insertQuery = @"
                INSERT INTO users (username, password_hash, salt, created_at) 
                VALUES (@username, @password_hash, @salt, @created_at)
                RETURNING id, created_at;
            ";

            using var insertCmd = new NpgsqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@username", User_data.Username);
            insertCmd.Parameters.AddWithValue("@password_hash", passwordHash);
            insertCmd.Parameters.AddWithValue("@salt", salt);
            insertCmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);

            var newUserId = await insertCmd.ExecuteScalarAsync();
            Console.WriteLine($"User '{User_data.Username}' successfuly registered (ID: {newUserId})!");

            return (201, "User registered successfully", new List<UserRegisterDTO>{ User_data });
        }
        catch (NpgsqlException ex)
        {
            return (503, $"Database error: {ex.Message}", new List<UserRegisterDTO>());
        }
        catch (Exception ex)
        {
            return (500, $"Internal error: {ex.Message}", new List<UserRegisterDTO>());
        }

    }

}