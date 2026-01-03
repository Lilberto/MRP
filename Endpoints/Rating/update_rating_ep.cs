namespace UpdateRatingEP;

using System.Text.Json;
using System.Net;

using UpdateRatingLogic;

//* utils
using Token;
using Body_request;

//* codes
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

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);
            int userId = UserID.User_ID.UserID_DB(Token!);

            if (routeParams.TryGetValue("mediaId", out string? idStr) && int.TryParse(idStr, out int ratingId))
            {   
                //###################################//
                // Try/Catch for invalid JSON format //
                //###################################//
                Rating? RatingData = null;  
                try
                {
                    RatingData = JsonSerializer.Deserialize<Rating>(await Body_Request.Body_Data(request));
                }
                catch (JsonException)
                {
                    await Error400.E_400(response, new { message = "Invalid JSON format." });
                    return;
                }
                
                //###################################################//
                // Checks if rating is empty and the rating is 1 - 5 //
                //###################################################//
                if (RatingData == null || RatingData.Stars < 1 || RatingData.Stars > 5)
                {
                    await Error400.E_400(response, new { message = "Invalid rating data!" });
                }

                var (StatusCode, Message) = await Update_Rating_Service.Update_Rating_Logic(ratingId, userId, RatingData!);

                switch (StatusCode)
                {
                    case 200:
                        await Code200.C_200(response, new { message = Message });
                        return;

                    case 403:
                        await Error403.E_403(response, new { Message });
                        return;

                    case 404:
                        await Error404.E_404(response);
                        return;

                    default:
                        await Error500.E_500(response, Message);
                        return;
                }
            }
            else
            {
                await Error400.E_400(response, new { message = "Invalid media ID!" });
            }
        }
        catch (Exception)
        {
            await Error500.E_500(response, new { message = "An unexpected server error occurred." });
        }
    }
}