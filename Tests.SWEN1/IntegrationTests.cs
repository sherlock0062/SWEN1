using System.Text;
using System.Text.Json;

using SWEN1.Http;
using SWEN1.Routing;

namespace Tests.SWEN1;

[TestFixture]
public class IntegrationTests
{
    private string _baseUrl;
    private string _user1Token;
    private string _user2Token;
    private string _user1;
    private string _user2;

    private const string ConnectionString = "Host=localhost;Username=postgres;Password=postgres;Database=mtcg";
    private Queue<string> _battleQueue;
    private object _battleQueueLock;

    [OneTimeSetUp]
    public void Setup()
    {
        _baseUrl = "http://localhost:10001";
        _user1Token = "kienboec-mtcgToken";
        _user2Token = "altenhof-mtcgToken";
        _user1 = "kienboec";
        _user2 = "altenhof";

        _battleQueue = new Queue<string>();
        _battleQueueLock = new object();
    }

    private static string BuildRawHttpRequest(string method, string path, Dictionary<string, string> headers, string body)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{method} {path} HTTP/1.1");
        foreach (var header in headers)
        {
            sb.AppendLine($"{header.Key}: {header.Value}");
        }

        sb.AppendLine();
        if (!string.IsNullOrEmpty(body))
        {
            sb.Append(body);
        }

        return sb.ToString();
    }

    private string SendRawHttpRequest(string method, string path, Dictionary<string, string> headers,
        string body = "")
    {
        string rawRequest = BuildRawHttpRequest(method, path, headers, body);
        var request = HttpServer.ParseHttpRequest(rawRequest);
        string response = Router.HandleRequest(request, ConnectionString, _battleQueue, _battleQueueLock);
        return response;
    }

    private int GetStatusCodeFromResponse(string response)
    {
        var lines = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        if (lines.Length > 0)
        {
            var parts = lines[0].Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int code))
            {
                return code;
            }
        }

        return 0;
    }

    [Test]
    public void GetCards_User1()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Authorization", "Bearer " + _user1Token }
        };
        string response = SendRawHttpRequest("GET", "/cards", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.EqualTo(200), "User 1 should be able to get cards.");
    }

    [Test]
    public void GetCards_NoToken_ShouldFail()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string response = SendRawHttpRequest("GET", "/cards", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.Not.EqualTo(200), "Getting cards without a token should fail.");
    }

    [Test]
    public void GetDeck_User1()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Authorization", "Bearer " + _user1Token }
        };
        string response = SendRawHttpRequest("GET", "/deck", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.EqualTo(200), "User 1 should retrieve their deck.");
    }

    [Test]
    public void GetDeck_NoToken_ShouldFail()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string response = SendRawHttpRequest("GET", "/deck", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.Not.EqualTo(200), "Retrieving a deck without a token should fail.");
    }

    [Test]
    public void GetDeckPlainFormat_User1()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Authorization", "Bearer " + _user1Token }
        };
        string response = SendRawHttpRequest("GET", "/deck?format=plain", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.EqualTo(200), "User 1 should get plain formatted deck.");
    }

    [Test]
    public void GetUserProfile_User1()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Authorization", "Bearer " + _user1Token }
        };
        string response = SendRawHttpRequest("GET", $"/users/{_user1}", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.EqualTo(200), "User 1 should retrieve their profile.");
    }

    [Test]
    public void ShouldFail_UserNotFound()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Authorization", "Bearer " + _user1Token }
        };
        string response = SendRawHttpRequest("GET", "/users/someGuy", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.Not.EqualTo(200), "Non-existent user retrieval should fail.");
    }

    [Test]
    public void GetStats_User1()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Authorization", "Bearer " + _user1Token }
        };
        string response = SendRawHttpRequest("GET", "/stats", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.EqualTo(200), "User 1 should retrieve stats.");
    }

    [Test]
    public void GetScoreboard()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Authorization", "Bearer " + _user1Token }
        };
        string response = SendRawHttpRequest("GET", "/scoreboard", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.EqualTo(200), "Scoreboard should be retrieved.");
    }

    [Test]
    public void GetStatsAfterBattle_User1()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Authorization", "Bearer " + _user1Token }
        };
        string response = SendRawHttpRequest("GET", "/stats", headers);
        int statusCode = GetStatusCodeFromResponse(response);
        Assert.That(statusCode, Is.EqualTo(200), "User 1 should get updated stats after battle.");
    }

}