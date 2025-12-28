namespace UpdateRatingEP;

using System.Text.Json;
using System.Net;

using NewRatingLogic;

// utils
using Token;
using Auth_util;
using Body_request;

// codes
using Code_201;
using Error_400;
using Error_404;
using Error_409;
using Error_500;

public class New_Rating
{
    public static async Task New_Rating_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int userId = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}");

        if (routeParams.TryGetValue("mediaId", out string? idStr) && int.TryParse(idStr, out int mediaId))
        {
            var RatingData = JsonSerializer.Deserialize<Rating>(await Body_Request.Body_Data(request));

            // Checks if rating is empty and the rating is 1 - 5
            if (RatingData == null || RatingData.Stars < 1 || RatingData.Stars > 5)
            {
                await Error400.E_400(context);
            }

            var statusCode = New_Rating_Service.New_Rating_Logic(mediaId, userId, RatingData!);

            Console.WriteLine($"StatusCode: {statusCode}");

            switch (statusCode)
            {
                case 201:
                    var result = new { message = "Rating created successful!" };
                    await Code201.C_201(response, result);
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