namespace Leaderboard_EP;

using System.Net;

//* utils
using Token;

//* codes
using Code_200;
using Error_500;

using LeaderboardService;

public static class LeaderboardEndpoint
{
    public static async Task LeaderboardSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        string? Token = await Tokens.TokenValidate(request, response);
        int userId = UserID.User_ID.UserID_DB(Token!);

        var (statusCode, data) = await Leaderboard_Service.Leaderboard_Logic();

        switch (statusCode)
        {
            case 200:
                var result = new { leaderboard = data };
                await Code200.C_200(response, result);
                break;

            default:
                await Error500.E_500(response, new Exception("Unknown error"));
                break;
        }
        
    }
}
