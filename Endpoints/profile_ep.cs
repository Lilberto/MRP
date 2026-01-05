using System.Net;

using Profile_Service;

//* utils
using Token;

//*s codes
using Code_200;
using Error_403;
using Error_404;
using Error_500;


namespace ProfileEP;

public static class ProfileEndpoint
{
    public static async Task ProfileSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);
        int userId = UserID.User_ID.UserID_DB(Token!);

        if (routeParams.TryGetValue("username", out string? username) && !string.IsNullOrEmpty(username))
        {
            var (StatusCode, Message, profileData) = await ProfileService.Profile_User(userId, username);

            switch (StatusCode)
            {
                case 200:
                    await Code200.C_200(response, new { Message, profile = profileData });
                    break;

                case 403:
                    await Error403.E_403(response, new { Message });
                    break;

                case 404:
                    await Error404.E_404(response, new { Message });
                    break;

                case 500:
                default:
                    await Error500.E_500(response, new { Message });
                    break;
            }
        }

    }
}
