namespace RecommendationsEP;

using System.Net;
using System.Text.Json;

using Recommendations_Service;

// utils
using Token;
using Auth_util;
using Body_request;

// codes
using Code_200;
using Error_500;

public class Recommendations_EP
{
    public static async Task Recommendations_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
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

            var (StatusCode, Message, Data) = await Recommendations_Service.Recommendations_Logic(userId);

            switch (StatusCode)
            {
                case 200:
                    var result = new { Message, recommendations = Data };
                    await Code200.C_200(response, result);
                    break;

                default:
                    await Error500.E_500(response, new Exception("Error fetching recommendations"));
                    break;
            }
        }
    }
}