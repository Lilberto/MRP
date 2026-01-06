namespace FavListEP;

using System.Net;

using FavListService;

//* utils
using Token;

//* codes
using Code_200;
using Error_404;
using Error_500;

public class Fav_List
{
    public static async Task Fav_List_Site(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);
        int userId = await UserID.User_ID.UserID_DB(Token!);

        if (routeParams.TryGetValue("username", out string? username) && !string.IsNullOrEmpty(username))
        {
            var (StatusCode, Message, Data) = await Fav_List_Service.Fav_List_Logic(userId, username);

            switch (StatusCode)
            {
                case 200:
                    await Code200.C_200(response, new { mesage = Message, Media = Data});
                    break;

                case 404:
                    await Error404.E_404(response, new { mesage = Message, Media = Data});
                    break;

                default:
                    await Error500.E_500(response, new { mesage = Message, Media = Data});
                    break;
            }
        }
    }
}