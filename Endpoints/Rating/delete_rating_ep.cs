namespace DeleteRatingEP;

using System.Net;

using DeleteRatingLogic;

//* utils
using Token;

//* codes
using Code_201;
using Error_400;
using Error_403;
using Error_404;
using Error_409;
using Error_500;

public class delete_rating
{
    public static async Task delete_rating_site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);
            int userId = await UserID.User_ID.UserID_DB(Token!);

            if (routeParams.TryGetValue("mediaId", out string? idStr) && int.TryParse(idStr, out int mediaId))
            {
                var (StatusCode, Message) = await delete_rating_service.delete_rating_logic(mediaId, userId);

                switch (StatusCode)
                {
                    case 200:
                        await Code201.C_201(response, Message);
                        break;

                    case 403:
                        await Error403.E_403(response, Message);
                        break;

                    case 404:
                        await Error404.E_404(response, Message);
                        break;

                    case 409:
                        await Error409.E_409(response, Message);
                        break;


                    default:
                        await Error500.E_500(response, Message);
                        break;
                }
            }
            else
            {
                await Error400.E_400(response, new { message = "Invalid media ID!" });
            }
        }
        catch (Exception)
        {
            await Error500.E_500(response, new { message = "An unexpected server error occurred." });
        }
    }
}