namespace Logout_Endpoint;

using System.Net;

using Logout_Service;

//* Codes
using Code_200;
using Error_500;
using Error_503;

//* Utils
using Token;

public static class LogoutEndpoint
{
    public static async Task LogoutUser(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            string? Token = await Tokens.TokenValidate(request, response);

            var (StatusCode, Message) = await LogoutService.LogoutUser(Token!);

            switch (StatusCode)
            {
                case 200:
                    await Code200.C_200(response, new { Message });
                    break;

                case 503:
                    await Error503.E_503(response, new { Message });
                    break;

                default:
                    await Error500.E_500(response, new { Message });
                    break;
            }
        }       
        catch (Exception ex)
        {
            var response = context.Response;
            await Error500.E_500(response, ex);
        }
    }
}