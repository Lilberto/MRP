using System.Net;
using System.Text.Json;

using Register_Service;

//* Codes
using Error_401;
using Code_201;

using Body_request;

namespace Register_Endpoint;


public static class RegisterEndpoint
{
    public static async Task RegisterSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        //* Deserialize the JSON request body into a User object
        var registerData = JsonSerializer.Deserialize<User>(await Body_Request.Body_Data(request));

        //* Null check for registerData
        if (registerData == null)
        {
            Error401.E_401(response);
            return;
        }

        bool RegisterValue = RegisterService.RegisterUser(registerData.Username, registerData.Password);
 
        await Response_Data(RegisterValue, response);
    }

    public static async Task Response_Data(bool Value, HttpListenerResponse response)
    {
        if(Value == false)
        {
            Error401.E_401(response);
        }
        else if (Value == true)
        {
            await Succesfull_Registration(response);
        }
    }

    public static async Task Succesfull_Registration(HttpListenerResponse response)
    {
        var result = new { message = "Registration successful" };

        await Code201.C_201(response, result);
    }
}