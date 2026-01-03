namespace Login_Endpoint;

using System.Net;
using System.Text.Json;

// Codes
using Code_200;
using Code_201;
using Error_400;
using Error_401;
using Error_409;
using Error_500;
using Error_503;

// utils
using Successful_response;
using Body_request;

using Login_Service;



public static class LoginEndpoint
{

    public static async Task LoginSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        UserRegisterDTO? loginData = null;

        try 
        {
            string body = await Body_Request.Body_Data(context.Request);
            
            loginData = JsonSerializer.Deserialize<UserRegisterDTO>(body);

            var (StatusCode, Message, Data) = await LoginService.LoginUser(loginData!);
            
            switch (StatusCode)
            {
                case 200:
                    await Code200.C_200(response, Data);
                    break;

                case 201:
                    await Code201.C_201(response, Data);
                    break;
                
                case 400:
                    await Error400.E_400(response, new { Message });
                    break;
                
                case 401:
                    Error401.E_401(response);
                    break;
                
                case 409:
                    await Error409.E_409(response, new { Message });
                    break;
                
                case 503:
                    await Error503.E_503(response, new { Message });
                    break;
                
                default:
                    await Error500.E_500(response, new { Message });
                    break;
            }
        }
        catch (JsonException)
        {
            await Error400.E_400(response, new { Message = "Invalid JSON format." });
            return; 
        }
    }
}