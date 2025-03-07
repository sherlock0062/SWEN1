using SWEN1.Http;
using SWEN1.Controllers;

namespace SWEN1.Routing;

public static class Router
{
    public static string HandleRequest(
        HttpRequest request,
        string connectionString,
        Queue<string> battleQueue,
        object battleQueueLock)
    {
        request.Headers.TryGetValue("Authorization", out var authHeader);
        string token = HttpServer.ExtractBearerToken(authHeader);
        Console.WriteLine($"{DateTime.Now}: Token extracted: '{token}'");
        var (statusCode, contentType, responseBody) = RequestController.RouteRequest(
            request.Method,
            request.Path,
            request.Query,
            token,
            request.Body,
            connectionString,
            battleQueue,
            battleQueueLock
        );
        Console.WriteLine($"{DateTime.Now}: Route handled: {statusCode} {responseBody}");
        return HttpServer.FormatHttpResponse(statusCode, contentType, responseBody);
    }
}