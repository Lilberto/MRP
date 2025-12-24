using System.Net;

using Code_201;

namespace Successful_response;

public class ResponseHelper
{
    public static async Task Successful_creation(HttpListenerResponse response, object data)
    {
        await Code201.C_201(response, data);
    }
}