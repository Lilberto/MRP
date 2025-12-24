using System.Net;
using System.Text.Json;

using Body_request;

//* Codes
using Error_401;
using Code_200;

using Login_Service;

using Successful_response;

namespace Login_Endpoint;


public static class LoginEndpoint
{
    public static async Task LoginSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;
    
        var loginData = JsonSerializer.Deserialize<User>(await Body_Request.Body_Data(request));

        //* Null check for registerData
        if (loginData == null)
        {
            Error401.E_401(response);
            return;
        }
        
        bool LoginValue = LoginService.LoginUser(loginData.Username, loginData.Password);
        Console.WriteLine($"LoginValue in Login Endpoint: {LoginValue}");
        Console.WriteLine($"Userdata| Username: {loginData.Username}, Password: {loginData.Password}");

        await Response_Data(LoginValue, response, loginData.Username);
    }

    private static async Task Response_Data(bool Value, HttpListenerResponse response, string Username)
    {
        string token = $"{Username}-mrpToken";
        var result = new { message = "Login successful", Token = token};
        Console.WriteLine($"Login Value: {Value}");

        if(Value == false)
        {
            Error401.E_401(response);
        }
        else if (Value == true)
        {
            await Code200.C_200(response, result);
        }
    }
}