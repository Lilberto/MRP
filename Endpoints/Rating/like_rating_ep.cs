namespace LikeRatingEP;

using System.Net;

using LikeRatingLogic;

//* utils
using Token;

//* codes
using Code_200;
using Code_201;
using Error_403;
using Error_404;
using Error_409;
using Error_500;

public class Like_Rating_EP
{
    public static async Task Like_Rating_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);
        int userId = UserID.User_ID.UserID_DB(Token!);

        try
        {
            if (routeParams.TryGetValue("ratingId", out string? idStr) && int.TryParse(idStr, out int ratingId))
            {
                var (StatusCode, Message) = await Like_Rating_Service.Like_Rating_Logic(ratingId, userId);

                switch (StatusCode)
                {
                    case 200: // Unliked successfully
                        await Code200.C_200(response, new { message = Message });
                        break;

                    case 201: // Liked successfully
                        await Code201.C_201(response, new { message = Message });
                        break;

                    case 403: // Not allowed (to like own rating)
                        await Error403.E_403(response, new { message = Message });
                        break;

                    case 404: // Not found
                        await Error404.E_404(response, new { message = Message });
                        break;

                    case 409: // Already liked
                        await Error409.E_409(response, new { message = Message });
                        break;

                    default:
                        await Error500.E_500(response, new { message = Message });
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            await Error500.E_500(response, new { message = "Internal error.", detail = ex.Message });
        }
    }
}