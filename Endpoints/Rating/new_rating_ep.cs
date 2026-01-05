namespace NewRatingEP;

using System.Text.Json;
using System.Net;

using NewRatingLogic;

//* utils
using Token;
using Body_request;

//* codes
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

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);
            int userId = UserID.User_ID.UserID_DB(Token!);

            if (routeParams.TryGetValue("mediaId", out string? idStr) && int.TryParse(idStr, out int mediaId))
            {
                var RatingData = JsonSerializer.Deserialize<Rating>(await Body_Request.Body_Data(request));

                // Checks if rating is empty and the rating is 1 - 5
                if (RatingData == null || RatingData.Stars < 1 || RatingData.Stars > 5)
                {
                    await Error400.E_400(response, new { message = "Invalid rating data!" });
                    return;
                }

                var (StatusCode, Message) = await New_Rating_Service.New_Rating_Logic(mediaId, userId, RatingData!);

                switch (StatusCode)
                {
                    case 201:
                        await Code201.C_201(response, Message);
                        break;

                    case 404:
                        await Error404.E_404(response, Message);
                        break;

                    case 409:
                        await Error409.E_409(response, new { Message });
                        break;

                    default:
                        await Error500.E_500(response, new { Message });
                        break;
                }
            }
            else
            {
                await Error400.E_400(response, new { message = "Invalid Media ID." });
            }
        }
        catch (JsonException ex)
        {
            await Error400.E_400(response, new { message = "Invalid JSON structure", detail = ex.Message });
        }
        catch (Exception ex)
        {
            await Error500.E_500(response, new { message = "An internal server error occurred.", detail = ex.Message });
        }
    }
}