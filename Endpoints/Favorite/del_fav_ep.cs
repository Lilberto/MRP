namespace DeleteFavoriteMediaEP;

using System.Net;

using DeleteFavoriteMediaService;

//* utils
using Token;

//* codes
using Code_200;
using Error_404;
using Error_500;

public class Delete_Favorite_Media
{
    public static async Task Delete_Favorite_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);
        int userId = await UserID.User_ID.UserID_DB(Token!);

        try
        {
            if (routeParams.TryGetValue("mediaId", out string? idStr) && int.TryParse(idStr, out int mediaId))
            {
                var (StatusCode, Message) = await Delete_Favorite_Media_Service.Delete_Favorite_Media_Logic(mediaId, userId);

                switch (StatusCode)
                {
                    case 200:
                        await Code200.C_200(response, new { message = Message });
                        break;

                    case 404:
                        await Error404.E_404(response, new { message = Message });
                        break;

                    default:
                        await Error500.E_500(response, new { message = Message });
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