using System.Net;
using System.Text.Json;

using Token;
using Auth_util;

using Code_200;
using Error_403;
using Error_404;

using Media_Delete_Logic;
namespace MediaDeleteEndpoint;

public class Media_Delete_Endpoint
{
    public static async Task Media_Delete_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);

        bool isValid = Auth.Auth_User(Token!);
        int User_ID = UserID.User_ID.UserID_DB(Token!);

        Console.WriteLine($"Auth Validation: {isValid}, User_id: {User_ID}");

        if (routeParams.TryGetValue("id", out string? idStr) && int.TryParse(idStr, out int mediaId))
        {
            int statusCode = MediaDeleteLogic.DeleteMedia(mediaId, User_ID);

            if (statusCode == 200) {
            await Code200.C_200(response, new { message = "Media and all related data deleted" });
            }
            else if (statusCode == 403) {
                Error403.E_403(response);
            }
            else {
                await Error404.E_404(context);
            }
        }
    }
}