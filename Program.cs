using System.Net;
using System.Text.RegularExpressions;

// User management
using Register_Endpoint;
using Login_Endpoint;
using Logout_Endpoint;

// Media management
using MediaEndpoint;
using AllMediaEndpoint;
using SingleMediaEndpoint;
using MediaUpdateEndpoint;
using MediaDeleteEndpoint;

// Rating 
using NewRatingEP;
using UpdateRatingEP;
using DeleteRatingEP;
using ConfirmCommentEP;
using LikeRatingEP;

// Favorite
using SetFavoriteMediaEP;
using DeleteFavoriteMediaEP;
using FavListEP;

// History
using RatingHistoryEP;

using Profile_Endpoint;
using Leaderboard_EP;

using Error_404;
using Error_400;

//! Register DB logic refabric
//! Login DB logic refabric

//! Profile DB logic
//! Leaderboard DB logic

//! Logout may need a confirmation and a response when user is already logged out 

//! Display Media needs to display the comments/ratings

//! User shall remove his like

class Program
{
    static async Task Main(string[] args)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("Server runs at http://localhost:8080/");

        while (true)
        {
            var context = await listener.GetContextAsync();

            if (context.Request?.Url == null)
            {
                await Error400.E_400(context);
            } else {
                Console.WriteLine($"[DEBUG] {context.Request.HttpMethod} {context.Request.Url.AbsolutePath}");
            }

            _ = Task.Run(() => Router.Handle(context));

        }
    }
}


public static class Router
{
    private static readonly List<Route> _routes = new()
    {
        // User management
        new Route("POST", @"^/api/users/register$", (ctx, p) => RegisterEndpoint.RegisterSite(ctx, p)),
        new Route("POST", @"^/api/users/login$", (ctx, p) => LoginEndpoint.LoginSite(ctx, p)),
        new Route("POST", @"^/api/users/logout$", (ctx, p) => LogoutEndpoint.LogoutUser(ctx, p)),

        // Profile and Leaderboard
        new Route("GET",  @"^/api/(?<username>[A-Za-z0-9_]+)/profile$", (ctx, p) => ProfileEndpoint.ProfileSite(ctx, p)),
        new Route("PUT",  @"^/api/(?<username>[A-Za-z0-9_]+)/profile$", (ctx, p) => ProfileEndpoint.ProfileSite(ctx, p)),
        new Route("GET",  @"^/api/users/leaderboard$", (ctx, p) => LeaderboardEndpoint.LeaderboardSite(ctx, p)),

        // Media
        new Route("POST", @"^/api/media$", (ctx, p) => Media_Endpoint.MediaSite(ctx, p)), //new media post 
        new Route("GET", @"^/api/media$", (ctx, p) => All_Media_Endpoint.AllMediaSite(ctx, p)), //all media posts 
        new Route("GET", @"^/api/media/(?<id>[0-9]+)$", (ctx, p) => Single_Media_Endpoint.SingleMediaSite(ctx, p)), //show specific media by id
        new Route("PUT", @"^/api/media/(?<id>[0-9]+)$", (ctx, p) => Media_update_Endpoint.UpdateMediaSite(ctx, p)), //update specific media by id
        new Route("DELETE", @"^/api/media/(?<id>[0-9]+)$", (ctx, p) => Media_Delete_Endpoint.Media_Delete_Site(ctx, p)), //delete specific media

        // Media rating
        new Route("POST", @"^/api/media/(?<mediaId>[0-9]+)/rate$", (ctx, p) => New_Rating.New_Rating_Site(ctx, p)), //new rating
        new Route("PUT", @"^/api/ratings/(?<ratingId>[0-9]+)$", (ctx, p) => Update_Rating.Update_Rating_Site(ctx, p)), //update rating
        new Route("DELETE", @"^/api/ratings/(?<ratingId>[0-9]+)$", (ctx, p) => delete_rating.delete_rating_site(ctx, p)), //delete rating
        new Route("POST", @"^/api/ratings/(?<ratingId>[0-9]+)/confirm$", (ctx, p) => Confirm_Comment.Confirm_Comment_Site(ctx, p)), //confirm comment
        new Route("POST", @"^/api/ratings/(?<ratingId>[0-9]+)/like$", (ctx, p) => Like_Rating_EP.Like_Rating_Site(ctx, p)), //like rating

        // Favorites
        new Route("POST", @"^/api/media/(?<mediaId>[0-9]+)/favorite$", (ctx, p) => Set_Favorite_Media.Set_Favorite_Site(ctx, p)), //set media to favorites
        new Route("DELETE", @"^/api/media/(?<mediaId>[0-9]+)/favorite$", (ctx, p) => Delete_Favorite_Media.Delete_Favorite_Site(ctx, p)), //remove media from favorites
        new Route("GET", @"^/api/users/(?<username>[A-Za-z0-9_]+)/favorite$", (ctx, p) => Fav_List.Fav_List_Site(ctx, p)), //display users fav list
    
        // History
        new Route("GET", @"^/api/users/(?<username>[A-Za-z0-9_]+)/rate/history$", (ctx, p) => Rating_History.Rating_History_Site(ctx, p)), //rating history
    };

    public static async Task Handle(HttpListenerContext context)
    {
        if (context.Request?.Url == null)
        {
            await Error400.E_400(context);
            return;
        }

        string path = context.Request.Url.AbsolutePath;
        string method = context.Request.HttpMethod;

        foreach (var route in _routes)
        {
            if (route.Method == method && Regex.IsMatch(path, route.Pattern))
            {
                var match = Regex.Match(path, route.Pattern);
                var parameters = new Dictionary<string, string>();

                foreach (var groupName in route.Regex.GetGroupNames())
                {
                    if (match.Groups[groupName].Success && groupName != "0")
                        parameters[groupName] = match.Groups[groupName].Value;
                }

                await route.Handler(context, parameters);
                return;
            }
        }

        // context.Response.StatusCode = 404;
        // byte[] buffer = Encoding.UTF8.GetBytes("Not Found");
        // await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        // context.Response.Close();

        await Error404.E_404(context);
    }

    private class Route
    {
        public string Method { get; }
        public string Pattern { get; }
        public Regex Regex { get; }
        public Func<HttpListenerContext, Dictionary<string, string>, Task> Handler { get; }

        public Route(string method, string pattern, Func<HttpListenerContext, Dictionary<string, string>, Task> handler)
        {
            Method = method;
            Pattern = pattern;
            Regex = new Regex(pattern, RegexOptions.Compiled);
            Handler = handler;
        }
    }
}