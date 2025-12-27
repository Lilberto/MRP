using System.Net;

using Token;
using Auth_util;

using Code_200;
using Error_400;
using Error_404;
using Error_500;

using Single_Media_extract;

namespace SingleMediaEndpoint;

public class Single_Media_Endpoint
{
    public static async Task SingleMediaSite(HttpListenerContext context, Dictionary<string, string> routeParams)
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
                if (routeParams.TryGetValue("id", out string? idValue))
                {
                    int mediaId = int.Parse(idValue);
                    Console.WriteLine($"MediaID out of URL: {mediaId}");

                    var media = Single_Media_extract_service.Single_Media_extract(mediaId);

                    if (media != null)
                    {
                        await Code200.C_200(response, media);
                    }
                }
                else
                {
                    await Error400.E_400(context);
                }
            } 
            else
            {
                await Error404.E_404(context);
            }


        }
        catch (Exception ex)
        {
            await Error500.E_500(response, ex);
        }

    }
}