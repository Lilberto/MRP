namespace AllMediaEndpoint;

using System.Net;

//* utils
using Token;

//* codes
using Code_200;
using Code_201;
using Error_409;
using Error_500;

using All_Media_extract;


public class All_Media_Endpoint
{
    public static async Task AllMediaSite(HttpListenerContext context, Dictionary<string, string> routeParams)
    {
        var request = context.Request;
        var response = context.Response;

        try
        {
            string? Token = await Tokens.TokenValidate(request, response);

            var filter = new MediaSearchFilter {
                Title = request.QueryString["title"],
                Genre = request.QueryString["genre"],
                MediaType = request.QueryString["mediaType"],
                AgeRestriction = request.QueryString["ageRestriction"],
                SortBy = request.QueryString["sortBy"],
                Username = request.QueryString["username"]
            };

            if (int.TryParse(request.QueryString["releaseYear"], out int year)) filter.ReleaseYear = year;
            if (double.TryParse(request.QueryString["rating"], out double rate)) filter.MinRating = rate;

            var (StatusCode, Message, Data) = await All_Media_extract_service.All_Media_extract(filter);

            switch (StatusCode)
            {
                case 200:
                    await Code200.C_200(response, new { message = Message, data = Data });
                    return;
                    
                case 201:
                    await Code201.C_201(response, new {  });
                    return;

                case 409:
                    await Error409.E_409(response, new { message = Message });
                    return;

                default:
                    await Error500.E_500(response, new { message = "Internal server error." });
                    return;
            }

        }
        catch (Exception ex)
        {
            await Error500.E_500(response, new { message = "An internal server error occurred.", detail = ex.Message });
        }

    }
}