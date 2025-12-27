using System.Net;
using System.Text.Json;

using Body_request;

using Token;
using Auth_util;
using Media_insert;

using Error_400;

using Successful_response;

namespace MediaEndpoint;

public class Media_Endpoint
{
    public static async Task MediaSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int User_ID = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}, User_id: {User_ID}");

        var MediaData = JsonSerializer.Deserialize<Media>(await Body_Request.Body_Data(request));

        var result = new
        {
          message = "Media entry successful!"  
        };

        if (MediaData == null || 
            string.IsNullOrWhiteSpace(MediaData.title) || 
            string.IsNullOrWhiteSpace(MediaData.type) ||
            MediaData.genres == null || MediaData.genres.Count == 0)
        {
            await Error400.E_400(context); 
            return;
        }

        int MediaID = Media_insert_service.Media_insert(
            User_ID, MediaData.title, MediaData.description, 
            MediaData.type, MediaData.year, MediaData.agerating, MediaData.genres
        );

        await ResponseHelper.Successful_creation(response, result);
    }
}