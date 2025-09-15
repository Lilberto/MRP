using System.Net;
using System.Text;
using System.Text.Json;


class Program
{
    static async Task Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Server runs at http://localhost:8080/");

        while (true)
        {
            var context = await listener.GetContextAsync();
            _ = Task.Run(() => HandleRequest(context));
        }
    }

    static async void HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string token = request.Headers["Authorization"]?.Replace("Bearer ", "") ?? "";

            // ----- LOGIN Site  -----
            if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/api/users/login")
            {
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                string body = await reader.ReadToEndAsync();
                var loginData = JsonSerializer.Deserialize<LoginRequest>(body);

                if (loginData == null || string.IsNullOrEmpty(loginData.Username) || string.IsNullOrEmpty(loginData.Password))
                {
                    response.StatusCode = 400;
                }
                else
                {
                    var user = UserRepository.Validate(loginData.Username, loginData.Password);
                    if (user == null)
                        response.StatusCode = 401;
                    else
                    {
                        string tokenGenerated = Guid.NewGuid().ToString();
                        var result = new { message = "Login successful", token = $"{loginData.Username}-mrpToken" };
                        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                        byte[] buffer = Encoding.UTF8.GetBytes(json);
                        response.ContentType = "application/json";
                        response.ContentLength64 = buffer.Length;
                        response.StatusCode = 200;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                return;
            }

            // ----- GET / (Main-Site) -----
            if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/")
            {
                if (string.IsNullOrEmpty(token))
                {
                    // No Token → 401 Unauthorized
                    response.StatusCode = 401;
                    await response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("Unauthorized"));
                }
                else
                {
                    var mainInfo = new { name = "MRP API", version = "0.1" };
                    string json = JsonSerializer.Serialize(mainInfo, new JsonSerializerOptions { WriteIndented = true });
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
                    response.ContentType = "application/json";
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = 200;
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                return;
            }

            // ----- All other URLs -----
            response.StatusCode = 404;
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            byte[] buffer = Encoding.UTF8.GetBytes($"Server error: {ex.Message}");
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

}


public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class User
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public static class UserRepository
{
    private static List<User> _users = new List<User>
    {
        new User { Username = "alice", Password = "1234" },
        new User { Username = "bob", Password = "secret" }
    };

    public static User? Validate(string username, string password)
    {
        return _users.FirstOrDefault(u => u.Username == username && u.Password == password);
    }
}
