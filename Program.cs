using System.Net;
using System.Text.RegularExpressions;

using Register_Endpoint;
using Login_Endpoint;
using Profile_Endpoint;
using Leaderboard_Endpoint;
using MediaEndpoint;
using AllMediaEndpoint;

using Error_404;
using Error_400;

//! Register DB logic
//! Login DB logic
//! Profile DB logic
//! Leaderboard DB logic


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
        new Route("POST", @"^/api/users/register$", (ctx, p) => RegisterEndpoint.RegisterSite(ctx, p)),
        new Route("POST", @"^/api/users/login$", (ctx, p) => LoginEndpoint.LoginSite(ctx, p)),
        new Route("GET",  @"^/api/(?<username>[A-Za-z0-9_]+)/profile$", (ctx, p) => ProfileEndpoint.ProfileSite(ctx, p)),
        new Route("GET",  @"^/api/users/leaderboard$", (ctx, p) => LeaderboardEndpoint.LeaderboardSite(ctx, p)),

        new Route("POST", @"^/api/media$", (ctx, p) => Media_Endpoint.MediaSite(ctx, p)), //new media post 
        new Route("GET", @"^/api/media$", (ctx, p) => All_Media_Endpoint.AllMediaSite(ctx, p)), //all media posts 

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