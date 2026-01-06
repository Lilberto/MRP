namespace Register_Service;

using Npgsql;
using System.Text.RegularExpressions;

//* utils
using Hash_util;
using DBConnection;

public static class RegisterService
{
    public static async Task<(int StatusCode, string Message, List<UserRegisterDTO> Data)> RegisterUser(UserRegisterDTO User_data)
    {
        //############################################//
        // Checks input against security requirements //
        //############################################//
        string userPattern = @"^[A-Za-z0-9_-]+$";
        string passPattern = @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$";

        if (!Regex.IsMatch(User_data.Username, userPattern))
        {
            return (400, "Username must be 3-20 alphanumeric characters.", new List<UserRegisterDTO>());
        }

        if (!Regex.IsMatch(User_data.Password, passPattern))
        {
            return (400, "Password needs 8+ characters, letters and numbers.", new List<UserRegisterDTO>());
        }


        try
        {
            using var conn = DbFactory.GetConnection();
            await conn.OpenAsync();

            //###################################//
            // Ensures the username is not taken //
            //###################################//
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
            // ashes password and saves new user //
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


// namespace Register_Service;

// using System.Text.RegularExpressions;

// //* utils
// using Hash_util;

// public interface IUserRepository
// {
//     Task<bool> UserExistsAsync(string username);
//     Task<int> CreateUserAsync(string username, string passwordHash, string salt);
// }

// public static class RegisterService
// {
//     private static IUserRepository _userRepository;
    
//     public static void SetUserRepository(IUserRepository repository)
//     {
//         _userRepository = repository;
//     }
    
//     static RegisterService()
//     {
//         _userRepository = new PostgresUserRepository();
//     }
    
//     private static readonly Regex _usernameRegex = new(@"^[A-Za-z0-9_-]+$", RegexOptions.Compiled);
//     private static readonly Regex _passwordRegex = new(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{8,}$", RegexOptions.Compiled);
    
//     public static async Task<(int StatusCode, string Message, List<UserRegisterDTO> Data)> RegisterUser(UserRegisterDTO userData)
//     {
//         //############################################//
//         // Checks input against security requirements //
//         //############################################//
//         if (string.IsNullOrWhiteSpace(userData.Username) || 
//             userData.Username.Length < 3 || 
//             userData.Username.Length > 20)
//         {
//             return (400, "Username must be 3-20 alphanumeric characters.", new List<UserRegisterDTO>());
//         }
        
//         if (!_usernameRegex.IsMatch(userData.Username))
//         {
//             return (400, "Username must contain only letters, numbers, underscores or hyphens.", new List<UserRegisterDTO>());
//         }

//         if (!_passwordRegex.IsMatch(userData.Password))
//         {
//             return (400, "Password needs 8+ characters, letters and numbers.", new List<UserRegisterDTO>());
//         }

//         try
//         {
//             //###################################//
//             // Ensures the username is not taken //
//             //###################################//
//             bool userExists = await _userRepository.UserExistsAsync(userData.Username);
//             if (userExists)
//             {
//                 Console.WriteLine("User exists!");
//                 return (409, "User already exists", new List<UserRegisterDTO>());
//             }

//             //###################################//
//             // Hashes password and saves new user //
//             //###################################//
//             string salt = Hash.GenerateSalt();
//             string passwordHash = Hash.HashPassword(userData.Password, salt);

//             //################################//
//             // Insert user with ALL required  //
//             //################################//
//             var newUserId = await _userRepository.CreateUserAsync(userData.Username, passwordHash, salt);
//             Console.WriteLine($"User '{userData.Username}' successfully registered (ID: {newUserId})!");

//             return (201, "User registered successfully", new List<UserRegisterDTO>{ userData });
//         }
//         catch (Exception ex)
//         {
//             return (500, $"Internal error: {ex.Message}", new List<UserRegisterDTO>());
//         }
//     }
// }

// public class PostgresUserRepository : IUserRepository
// {
//     public async Task<bool> UserExistsAsync(string username)
//     {
//         using var conn = DBConnection.DbFactory.GetConnection();
//         await conn.OpenAsync();

//         string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username;";
//         using var checkCmd = new Npgsql.NpgsqlCommand(checkQuery, conn);
//         checkCmd.Parameters.AddWithValue("@username", username);

//         var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
//         return count > 0;
//     }
    
//     public async Task<int> CreateUserAsync(string username, string passwordHash, string salt)
//     {
//         using var conn = DBConnection.DbFactory.GetConnection();
//         await conn.OpenAsync();

//         string insertQuery = @"
//             INSERT INTO users (username, password_hash, salt, created_at) 
//             VALUES (@username, @password_hash, @salt, @created_at)
//             RETURNING id;
//         ";

//         using var insertCmd = new Npgsql.NpgsqlCommand(insertQuery, conn);
//         insertCmd.Parameters.AddWithValue("@username", username);
//         insertCmd.Parameters.AddWithValue("@password_hash", passwordHash);
//         insertCmd.Parameters.AddWithValue("@salt", salt);
//         insertCmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);

//         return Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
//     }
// }