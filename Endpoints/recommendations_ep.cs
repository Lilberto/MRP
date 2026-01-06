namespace RecommendationsEP;

using System.Net;

using Recommendations_Service;

//* utils
using Token;

//* codes
using Code_200;
using Error_404;
using Error_500;

public class Recommendations_EP
{
    public static async Task Recommendations_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);
        int userId = await UserID.User_ID.UserID_DB(Token!);

        if (routeParams.TryGetValue("username", out string? username) && !string.IsNullOrEmpty(username))
        {

            var (StatusCode, Message, Data) = await Recommendations_Service.Recommendations_Logic(userId);

            switch (StatusCode)
            {
                case 200:
                    await Code200.C_200(response, new { Message, recommendations = Data });
                    break;

                case 404:
                    await Error404.E_404(response, new { Message, recommendations = Data });
                    break;

                default:
                    await Error500.E_500(response, new { Message, recommendations = Data });
                    break;
            }
        }
    }
}