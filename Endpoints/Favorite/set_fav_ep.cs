namespace SetFavoriteMediaEP;

using System.Net;

//* utils
using Token;

//* codes
using Code_201;
using Error_400;
using Error_404;
using Error_409;
using Error_500;

using SetFavoriteMediaService;

public class Set_Favorite_Media
{
    public static async Task Set_Favorite_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);
        int userId = UserID.User_ID.UserID_DB(Token!);

        try
        {
            if (routeParams.TryGetValue("mediaId", out string? idStr) && int.TryParse(idStr, out int mediaId))
            {
                var (StatusCode, Message) = await Set_Favorite_Media_Service.Set_Favorite_Media_Logic(mediaId, userId);

                switch (StatusCode)
                {
                    case 201:
                        await Code201.C_201(response, new { Message });
                        break;

                    case 400:
                        await Error400.E_400(response, new { Message });
                        break;

                    case 404:
                        await Error404.E_404(response, new { Message });
                        break;

                    case 409:
                        await Error409.E_409(response, new { Message });
                        break;

                    default:
                        await Error500.E_500(response, new { Message });
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            await Error500.E_500(response, new { message = "An internal server error occurred.", detail = ex.Message });
        }
    }
}