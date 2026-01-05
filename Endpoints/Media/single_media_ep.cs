namespace SingleMediaEndpoint;

using System.Net;

//* codes
using Code_200;
using Error_400;
using Error_404;
using Error_409;
using Error_500;

//* utils
using Token;

using Single_Media_extract;

public class Single_Media_Endpoint
{
    public static async Task SingleMediaSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);

            if (routeParams.TryGetValue("id", out string? idValue) && int.TryParse(idValue, out int mediaId))
            {
                Console.WriteLine($"MediaID out of URL: {mediaId}");

                var (StatusCode, Message, Data, Rating) = await Single_Media_extract_service.Single_Media_extract(mediaId);


                switch (StatusCode)
                {
                    case 200:
                        await Code200.C_200(response, new { message = Message, Media = Data, Ratings = Rating });
                        return;

                    case 404:
                        await Error404.E_404(response);
                        return;

                    case 409:
                        await Error409.E_409(response, Message);
                        return;

                    default:
                        await Error500.E_500(response, Message);
                        return;
                }
            }
            else
            {
                await Error400.E_400(response, new { message = "Invalid Media ID format." });
                return;
            }
        }
        catch (Exception ex)
        {
            await Error500.E_500(response, new { message = "An internal server error occurred.", detail = ex.Message });
        }

    }

}