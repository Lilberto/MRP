using System.Net;
using System.Text.Json;

using Token;
using Body_request;
using Profile_Service;

//* Codes
using Error_400;
using Error_401;

namespace Profile_Endpoint;

public static class ProfileEndpoint
{
    public static async Task ProfileSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string username = routeParams["username"];

        string? token = await Tokens.TokenValidate(request, response);

        var loginData = JsonSerializer.Deserialize<User>(await Body_Request.Body_Data(request));

        //* Null check for loginData
        if (loginData == null)
        {
            await Error400.E_400(context);
            return;
        }

    }

    public static void ProfileData(string username, string password, string token, HttpListenerResponse response)
    {
        var User_Data = ProfileService.Profile_User(username, password, token);

        if (User_Data == null)
        {
           Error401.E_401(response);
        }
        else
        {
            Post_Profile_Data();
        }
    }

    public static void Post_Profile_Data()
    {
        
    }
}
