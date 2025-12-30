namespace RatingHistoryEP;

using System.Text.Json;
using System.Net;

using RatingHistoryService;

// utils
using Token;
using Auth_util;

// codes
using Code_200;
using Error_500;

public static class Rating_History
{
    public static async Task Rating_History_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int userId = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}");

        if (routeParams.TryGetValue("username", out string? username) && !string.IsNullOrEmpty(username))
        {
            bool userTokenValid = Auth.Auth_User_Token(username, Token!);

            if (!userTokenValid)
            {
                await Error500.E_500(response, new Exception("Authentication failed for user & token."));
                return;
            }

            var (statusCode, history) = await Rating_History_Service.Rating_History_Logic(userId);
            
            Console.WriteLine($"StatusCode: {statusCode}");
            
            switch (statusCode)
            {
                case 200:
                    var result = new { rating_history = history };
                    await Code200.C_200(response, result);
                    break;

                default:
                    await Error500.E_500(response, new Exception("Unknown error"));
                    break;
            }
        }
    }
    
}