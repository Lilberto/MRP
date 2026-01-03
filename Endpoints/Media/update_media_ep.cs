namespace MediaUpdateEndpoint;

using System.Net;
using System.Text.Json;

//* codes
using Code_200;
using Code_201;
using Error_400;
using Error_401;
using Error_403;
using Error_404;
using Error_409;
using Error_500;

//* utils
using Token;

using Media_update_logic;

public class Media_update_Endpoint
{
    public static async Task UpdateMediaSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);
            int User_ID = UserID.User_ID.UserID_DB(Token!);

            var updateData = JsonSerializer.Deserialize<MediaUpdateDto>(await Body_request.Body_Request.Body_Data(request));

            if (updateData == null || string.IsNullOrWhiteSpace(updateData.title) || string.IsNullOrWhiteSpace(updateData.type))
            {
                await Error400.E_400(response, new { message = "Missing or invalid fields in request body." });
                return;
            }


            if (routeParams.TryGetValue("id", out string? idStr) && int.TryParse(idStr, out int mediaId))
            {
                var (StatusCode, Message, Media) = await Media_update_service.Media_update(mediaId, User_ID, updateData!);

                switch (StatusCode)
                {
                    case 200:
                        await Code200.C_200(response, new { message = Message, media = Media });
                        return;

                    case 403:
                        Error403.E_403(response);
                        return;
                    
                    case 404:
                        await Error404.E_404(response);
                        return;
                    
                    case 409:
                        await Error409.E_409(response, new { message = Message });
                        return;
                    
                    default:
                        await Error500.E_500(response, new { message = Message });
                        return;
                }
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