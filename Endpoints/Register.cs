namespace Register_Endpoint;

using System.Net;
using System.Text.Json;

using Register_Service;

// Codes
using Code_201;
using Error_400;
using Error_409;
using Error_500;
using Error_503;

using Body_request;

public static class RegisterEndpoint
{
    public static async Task RegisterSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var response = context.Response;
        UserRegisterDTO? registerData = null;

        try 
        {
            string body = await Body_Request.Body_Data(context.Request);
            
            registerData = JsonSerializer.Deserialize<UserRegisterDTO>(body);
            
            if (registerData == null || string.IsNullOrEmpty(registerData.Username) || string.IsNullOrEmpty(registerData.Password))
            {
                await Error400.E_400(response, new { Message = "Field 'username' or 'password' is misspelled or missing." });
                return;
            }
        }
        catch (JsonException)
        {
            await Error400.E_400(response, new { Message = "Invalid JSON format." });
            return; 
        }

        var (StatusCode, Message, Data) = await RegisterService.RegisterUser(registerData);
        Console.WriteLine($"Register Endpoint returned Status Code: {StatusCode}");

        switch (StatusCode)
        {
            case 201:
                await Code201.C_201(response, new { Message = Message, Data = Data });
                break;

            case 409:
                await Error409.E_409(response, new { Message = Message, Data = Data });
                break;

            case 503:
                await Error503.E_503(response, new { Message = Message, Data = Data });
                break;

            case 500:
            default:
                await Error500.E_500(response, new { Message = Message, Data = Data });
                break;
        }
    }
}