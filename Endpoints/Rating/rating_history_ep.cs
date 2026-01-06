namespace RatingHistoryEP;

using System.Net;

using RatingHistoryService;

//* utils
using Token;

//* codes
using Code_200;
using Error_404;
using Error_500;

public static class Rating_History
{
    public static async Task Rating_History_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);
        int userId = await UserID.User_ID.UserID_DB(Token!);

        if (routeParams.TryGetValue("username", out string? username) && !string.IsNullOrEmpty(username))
        {
            var (StatusCode, Message, history) = await Rating_History_Service.Rating_History_Logic(userId, username, Token!);

            switch (StatusCode)
            {
                case 200:
                    await Code200.C_200(response, new { message = Message, rating_history = history });
                    break;

                case 404:
                    await Error404.E_404(response);
                    break;
                    
                default:
                    await Error500.E_500(response, new { Message });
                    break;
            }
        }
    }

}