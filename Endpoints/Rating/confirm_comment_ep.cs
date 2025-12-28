namespace ConfirmCommentEP;

using System.Text.Json;
using System.Net;

using ConfirmCommentLogic;

// utils
using Token;
using Auth_util;
using Body_request;

// codes
using Code_200;
using Error_400;
using Error_403;
using Error_404;
using Error_500;

public class Confirm_Comment
{
    public static async Task Confirm_Comment_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int userId = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}");

        if (routeParams.TryGetValue("ratingId", out string? idStr) && int.TryParse(idStr, out int ratingId))
        {
            var RatingData = JsonSerializer.Deserialize<Rating>(await Body_Request.Body_Data(request));

            var statusCode = Confirm_Comment_Service.Confirm_Comment_Logic(ratingId, userId, RatingData!);

            switch (statusCode)
            {
                case 200:
                    var result = new { message = "Rating is now visible!" };
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