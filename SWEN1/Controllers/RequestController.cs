using SWEN1.Helpers;

namespace SWEN1.Controllers;

public static class RequestController
{
    public static (int, string, string) RouteRequest(
        string method,
        string path,
        string query,
        string token,
        string body,
        string connectionString,
        Queue<string> battleQueue,
        object battleQueueLock)
    {
        if (method == "POST" && path == "/users") return UserController.RegisterUser(body, connectionString);
        if (method == "POST" && path == "/sessions") return UserController.LoginUser(body, connectionString);
        var username = UserController.GetUsernameFromToken(token, connectionString);
        if (string.IsNullOrEmpty(username)) return ResponseHelper.ErrorResponse(401, "Authentication required");
        if (method == "POST" && path == "/packages") return CardController.CreatePackage(token, body, connectionString);
        if (method == "POST" && path == "/transactions/packages") return CardController.AcquirePackage(username, connectionString);
        if (method == "GET" && path == "/cards") return CardController.GetUserCards(username, connectionString);
        if (method == "GET" && path == "/deck") return CardController.GetDeck(username, query, connectionString);
        if (method == "PUT" && path == "/deck") return CardController.ConfigureDeck(username, body, connectionString);
        if (method == "POST" && path == "/battles") return BattleController.StartBattle(username, connectionString, battleQueue, battleQueueLock);
        if (method == "GET" && path == "/stats") return StatsController.GetStats(username, connectionString);
        if (method == "GET" && path == "/scoreboard") return StatsController.GetScoreboard(connectionString);
        if (path.StartsWith("/users/"))
        {
            var requestedUsername = path.Substring("/users/".Length);
            if (method == "GET") return UserController.GetUserProfile(username, requestedUsername, connectionString);
            if (method == "PUT") return UserController.UpdateUserProfile(username, requestedUsername, body, connectionString);
        }
        return ResponseHelper.ErrorResponse(404, "Endpoint not found");
    }
}