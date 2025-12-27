using System.Net;

using Token;
using Auth_util;

using Code_200;
using Error_400;
using Error_401;
using Error_403;
using Error_404;
using Error_500;

using Body_request;
using All_Media_extract;
using System.Text.Json;
using System.Reflection.Metadata;
using Media_update_logic;

namespace MediaUpdateEndpoint;

public class Media_update_Endpoint
{
    public static async Task UpdateMediaSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);

            bool isValid = Auth.Auth_User(Token!);
            int User_ID = UserID.User_ID.UserID_DB(Token!);

            Console.WriteLine($"Auth Validation: {isValid}");

            if (isValid)
            {
                var jsonString = await Body_request.Body_Request.Body_Data(request);
                var updateData = JsonSerializer.Deserialize<MediaUpdateDto>(jsonString);
                
                var validTypes = new List<string> { "movie", "series", "game" };

                if (updateData == null || 
                    string.IsNullOrWhiteSpace(updateData.title) || 
                    !validTypes.Contains(updateData.type)) 
                {
                    await Error400.E_400(context);
                    return;
                }

                if (routeParams.TryGetValue("id", out string? idStr) && int.TryParse(idStr, out int mediaId))
                {
                    int statusCode = Media_update_service.Media_update(mediaId, User_ID, updateData!);

                    if (statusCode == 200) {
                        await Code200.C_200(response, new { message = "Media update successful" });
                    } 
                    else if (statusCode == 403) {
                        Error403.E_403(response);
                    }
                    else if (statusCode == 404) {
                        await Error404.E_404(context);
                    }
                    else {
                        await Error500.E_500(response, new Exception("Update failed"));
                    }
                } 
                else
                {
                   await Error404.E_404(context); 
                }                   
            }
            else
            {
                Error401.E_401(response);
            }

        }
        catch (Exception ex)
        {
            await Error500.E_500(response, ex);
        }

    }
}