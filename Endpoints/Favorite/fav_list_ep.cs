namespace FavListEP;

using System.Text.Json;
using System.Net;

using FavListService;

// utils
using Token;
using Auth_util;
using Body_request;

// codes
using Code_200;
using Error_404;
using Error_500;

public class Fav_List
{
    public static async Task Fav_List_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int userId = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}");

        if (routeParams.TryGetValue("username", out string? username) && !string.IsNullOrEmpty(username))
        {
            var (statusCode, data) = await Fav_List_Service.Fav_List_Logic(username, Token!, userId);
            Console.WriteLine($"StatusCode: {statusCode}");

            switch (statusCode)
            {
                case 200:
                    var result = new { favorites = data };
                    await Code200.C_200(response, result);
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