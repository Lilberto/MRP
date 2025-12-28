namespace LikeRatingEP;

using System.Net;

using LikeRatingLogic;

// utils
using Token;
using Auth_util;

// codes
using Code_200;
using Error_403;
using Error_404;
using Error_500;

public class Like_Rating_EP
{
    public static async Task Like_Rating_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int userId = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}");

        if (routeParams.TryGetValue("ratingId", out string? idStr) && int.TryParse(idStr, out int ratingId))
        {
            int statusCode = Like_Rating_Service.Like_Rating_Logic(ratingId, userId);

            Console.WriteLine($"StatusCode: {statusCode}");

            switch (statusCode)
            {
                case 200:
                    var result = new { message = "Rating deleted successfuly!" };
                    await Code200.C_200(response, result);
                    break;

                case 403:
                    Error403.E_403(response);
                    break;

                case 404:
                    await Error404.E_404(context);
                    break;

                default:
                    await Error500.E_500(response, new Exception("Unknown error"));
                    break;
            }
        }
    }
}