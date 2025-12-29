namespace DeleteFavoriteMediaEP;

using System.Text.Json;
using System.Net;

using DeleteFavoriteMediaService;

// utils
using Token;
using Auth_util;
using Body_request;

// codes
using Code_200;
using Error_400;
using Error_404;
using Error_409;
using Error_500;

public class Delete_Favorite_Media
{
    public static async Task Delete_Favorite_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int userId = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}");

        if (routeParams.TryGetValue("mediaId", out string? idStr) && int.TryParse(idStr, out int mediaId))
        {
            var statusCode = Delete_Favorite_Media_Service.Delete_Favorite_Media_Logic(mediaId, userId);
            Console.WriteLine($"StatusCode: {statusCode}");

            switch (statusCode)
            {
                case 200:
                    var result = new { message = "Media has been removed from favorites!" };
                    await Code200.C_200(response, result);
                    break;
                
                case 404:
                    await Error404.E_404(context);
                    break;

                case 409:
                    await Error409.E_409(context);
                    break;

                default:
                    await Error500.E_500(response, new Exception("Unknown error"));
                    break;
            }
        }
    } 
}