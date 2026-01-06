using System.Net;
using System.Text.Json;

//* utils
using Token;

//* codes
using Code_200;
using Error_403;
using Error_404;
using Error_500;

using Media_Delete_Logic;
namespace MediaDeleteEndpoint;

public class Media_Delete_Endpoint
{
    public static async Task Media_Delete_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);
            int User_ID = await UserID.User_ID.UserID_DB(Token!);

            if (routeParams.TryGetValue("id", out string? idStr) && int.TryParse(idStr, out int mediaId))
            {
                var (StatusCode, Message) = await MediaDeleteLogic.DeleteMedia(mediaId, User_ID);

                switch (StatusCode)
                {
                    case 200:
                        await Code200.C_200(response, new { message = Message });
                        return;

                    case 403:
                        await Error403.E_403(response);
                        return;

                    case 404:
                        await Error404.E_404(response);
                        return;

                    default:
                        await Error500.E_500(response, new { message = Message ?? "Internal server error." });
                        return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Media_Delete_Site: {ex.Message}");
        }
    }
}