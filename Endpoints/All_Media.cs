using System.Net;

using Token;
using Auth_util;

using Code_200;
using Error_401;
using Error_500;

using Media_extract;

namespace AllMediaEndpoint;

public class All_Media_Endpoint
{
    public static async Task AllMediaSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);

            bool isValid = Auth.Auth_User(Token!);

            Console.WriteLine($"Auth Validation: {isValid}");

            if (isValid)
            {
                List<Media> AllMedia = Media_extract_service.Media_extract();

                Console.WriteLine($"All Media {AllMedia}");
                await Code200.C_200(response, AllMedia);
            } 
            else
            {
                Error401.E_401(response);
            }


        }
        catch (Exception ex)
        {
            await Error500.E_500(response, ex);
        }

    }
}