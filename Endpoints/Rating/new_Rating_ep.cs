namespace NewRatingEP;

using System.Text.Json;
using System.Net;

using UpdateRatingLogic;

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

public class Update_Rating
{
    public static async Task Update_Rating_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int userId = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}");

        if (routeParams.TryGetValue("ratingId", out string? idStr) && int.TryParse(idStr, out int ratingId))
        {   
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };  
            var RatingData = JsonSerializer.Deserialize<Rating>(await Body_Request.Body_Data(request), options);

            // Checks if rating is empty and the rating is 1 - 5
            if (RatingData == null || RatingData.Stars < 1 || RatingData.Stars > 5)
            {
                await Error400.E_400(context);
            }
            
            Console.WriteLine($"--RatingID: {ratingId}\n--UserID: {userId}\n--Rating Stars: {RatingData!.Stars}\n--Rating Comment: {RatingData.Comment}");
            var statusCode = Update_Rating_Service.Update_Rating_Logic(ratingId, userId, RatingData!);

            Console.WriteLine($"StatusCode: {statusCode}");

            switch (statusCode)
            {
                case 200:
                    var result = new { message = "Rating updated successfuly!" };
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