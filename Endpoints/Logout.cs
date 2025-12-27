using System.Net;
using System.Text.Json;

using Logout_Service;

using Token;
using Auth_util;

//* Codes
using Error_401;
using Code_200;

using Body_request;

namespace Logout_Endpoint;


public static class LogoutEndpoint
{
    public static async Task LogoutUser(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);

        Console.WriteLine($"Auth Validation: {isValid}");

        if (isValid)
        {
            LogoutService.LogoutUserService(Token!);
            Console.WriteLine("User is logged out!");

            await Code200.C_200(response, "User is logged out!");
        } 
        else
        {
            Error401.E_401(response);
        }   
    }

}