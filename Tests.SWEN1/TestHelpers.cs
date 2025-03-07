using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tests.SWEN1;

public class TestHelpers
{
    public const string ValidBearerHeader = "Bearer myToken123";
    public const string SampleToken = "myToken123";
    public const string AdminToken = "admin-mtcgToken";
    public const string SomeUserToken = "someUser-mtcgToken";

    public string ExtractAuthToken(string? authorizationHeader)
    {
        if (string.IsNullOrEmpty(authorizationHeader)) return "";
        const string prefix = "Bearer ";
        return authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[prefix.Length..].Trim()
            : "";
    }

    public string BuildRawRequest(string method, string path, string body = "")
    {
        return $"{method} {path} HTTP/1.1\r\n" +
               $"Content-Length: {body.Length}\r\n" +
               "\r\n" +
               body;
    }

    public FakeRequest CreateHttpRequest(string raw)
    {
        var lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        if (lines.Length == 0) return new FakeRequest();

        var first = lines[0].Split(' ');
        if (first.Length < 2) return new FakeRequest();

        var method = first[0];
        var rawUrl = first[1];
        var path = rawUrl;
        var query = "";

        var qPos = rawUrl.IndexOf('?');
        if (qPos >= 0)
        {
            path = rawUrl[..qPos];
            query = rawUrl[qPos..];
        }

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var i = 1;
        for (; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrEmpty(line))
            {
                i++;
                break;
            }

            var colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                var key = line[..colonIndex].Trim();
                var val = line[(colonIndex + 1)..].Trim();
                headers[key] = val;
            }
        }

        var bodyBuilder = new StringBuilder();
        for (; i < lines.Length; i++) bodyBuilder.AppendLine(lines[i]);

        var body = bodyBuilder.ToString().Trim();

        return new FakeRequest
        {
            Method = method,
            Path = path,
            Query = query,
            Headers = headers,
            Body = body
        };
    }

    public double CalculateAttackDamage(FakeCard attacker, FakeCard defender)
    {
        double dmg = (double)attacker.Damage;
        if (attacker.Name.Contains("Goblin") && defender.Name.Contains("Dragon")) return 0;
        if (attacker.Name.Contains("Ork") && defender.Name.Contains("Wizzard")) return 0;
        if (attacker.Name.Contains("WaterSpell") && defender.Name.Contains("Knight")) return double.MaxValue; // instakill
        if (attacker.CardType == "Spell" && defender.Name.Contains("Kraken")) return 0;
        if (attacker.Name.Contains("Dragon") && defender.Name.Contains("FireElf")) return 0;

        if (attacker.CardType == "Spell" || defender.CardType == "Spell")
        {
            if (attacker.ElementType == "Water" && defender.ElementType == "Fire") dmg *= 2;
            else if (attacker.ElementType == "Fire" && defender.ElementType == "Normal") dmg *= 2;
            else if (attacker.ElementType == "Normal" && defender.ElementType == "Water") dmg *= 2;
            if (attacker.ElementType == "Fire" && defender.ElementType == "Water") dmg *= 0.5;
            else if (attacker.ElementType == "Normal" && defender.ElementType == "Fire") dmg *= 0.5;
            else if (attacker.ElementType == "Water" && defender.ElementType == "Normal") dmg *= 0.5;
        }
        return dmg;
    }

    public (int, string, string) HandleMockRoute(string method, string path, string query, string token,
        string body)
    {
        if (method == "POST" && path == "/users")
            return (201, "application/json", "{\"message\":\"User successfully created\"}");
        if (method == "POST" && path == "/sessions")
            return (200, "application/json", "\"test-mtcgToken\"");

        if (token == "someUser-mtcgToken" || token == "admin-mtcgToken")
        {
            if (method == "GET" && path == "/cards")
                return (200, "application/json", "[]");
            if (method == "POST" && path == "/packages" && token == "admin-mtcgToken")
                return (201, "application/json", "{\"message\":\"Package created successfully\"}");
            if (method == "POST" && path == "/transactions/packages")
                return (201, "application/json", "{\"message\":\"Package acquired\"}");
            if (method == "GET" && path == "/deck")
                return (200, "application/json", "[]");
            if (method == "POST" && path == "/battles")
                return (200, "application/json", "Battle started");
            if (method == "GET" && path == "/stats")
                return (200, "application/json", "{\"Username\":\"someUser\",\"ELO\":100,\"Wins\":0,\"Losses\":0}");
            if (method == "GET" && path == "/scoreboard")
                return (200, "application/json", "[]");
        }

        return (404, "application/json", "{\"error\":\"Endpoint not found\"}");
    }

    public class FakeRequest
    {
        public string Method { get; set; } = "";
        public string Path { get; set; } = "";
        public string Query { get; set; } = "";
        public Dictionary<string, string> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public string Body { get; set; } = "";
    }

    public class FakeCard
    {
        [JsonPropertyName("Id")] public string Id { get; set; } = "";

        [JsonPropertyName("Name")] public string Name { get; set; } = "";

        [JsonPropertyName("Damage")] public decimal Damage { get; set; }

        public string ElementType { get; set; } = "Normal";
        public string CardType { get; set; } = "Monster";
    }
}