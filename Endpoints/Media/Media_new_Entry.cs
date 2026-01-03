namespace MediaEndpoint;

using System.Net;
using System.Text.Json;

using Media_insert;

//* utils
using Token;
using Auth_util;
using Body_request;

//* codes
using Code_201;
using Error_400;
using Error_409;
using Error_500;

public class Media_Endpoint
{
    public static async Task MediaSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);

            bool isValid = Auth.Auth_User(Token!);
            int User_ID = UserID.User_ID.UserID_DB(Token!);

            Console.WriteLine($"Auth Validation: {isValid}, User_id: {User_ID}");

            var MediaData = JsonSerializer.Deserialize<Media>(await Body_Request.Body_Data(request));

            if (MediaData == null ||
                string.IsNullOrWhiteSpace(MediaData.title) ||
                string.IsNullOrWhiteSpace(MediaData.type) ||
                MediaData.genres == null || MediaData.genres.Count == 0)
            {
                await Error400.E_400(response, new { message = "Missing or invalid fields in request body." });
                return;
            }

            var (StatusCode, Message, Data) = await Media_insert_service.Media_insert(MediaData, User_ID);

            switch (StatusCode)
            {
                case 201:
                    var result = new
                    {
                        message = "Media entry successful!",
                        mediaId = Data?.id
                    };
                    await Code201.C_201(response, new { message = Message, Media = Data });
                    break;

                case 409:
                    await Error409.E_409(response, new { message = Message });
                    break;

                default:
                    await Error500.E_500(response, new { message = "Internal server error." });
                    break;
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