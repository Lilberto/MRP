namespace DeleteRatingEP;

using System.Net;

using DeleteRatingLogic;

// utils
using Token;
using Auth_util;

// codes
using Code_201;
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

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int userId = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}");

        if (routeParams.TryGetValue("ratingId", out string? idStr) && int.TryParse(idStr, out int ratingId))
        {
            int statusCode = delete_rating_service.delete_rating_logic(ratingId, userId);

            Console.WriteLine($"StatusCode: {statusCode}");

            switch (statusCode)
            {
                case 200:
                    var result = new { message = "Rating deleted successfuly!" };
                    await Code201.C_201(response, result);
                    break;

                case 403:
                    Error403.E_403(response);
                    break;

                case 404:
                    await Error404.E_404(response);
                    break;

                case 409:
                    await Error409.E_409(response, new { message = "Rating does not exist!" });
                    break;


                default:
                    await Error500.E_500(response, new Exception("Unknown error"));
                    break;
            }
        }
    }
}