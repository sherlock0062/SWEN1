using System.Text;

namespace SWEN1.Http;

public static class HttpServer
{
    public static HttpRequest ParseHttpRequest(string raw)
    {
        var lines = raw.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        if (lines.Length == 0) return new HttpRequest();
        var first = lines[0].Split(' ');
        if (first.Length < 2) return new HttpRequest();
        var method = first[0];
        var rawUrl = first[1];
        string path = rawUrl;
        string query = "";
        int qPos = rawUrl.IndexOf('?');
        if (qPos >= 0)
        {
            path = rawUrl.Substring(0, qPos);
            query = rawUrl.Substring(qPos);
        }
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        int i = 1;
        for (; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line)) { i++; break; }
            int colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                var key = line[..colonIndex].Trim();
                var val = line[(colonIndex + 1)..].Trim();
                headers[key] = val;
            }
        }
        var bodyBuilder = new StringBuilder();
        for (; i < lines.Length; i++)
        {
            bodyBuilder.AppendLine(lines[i]);
        }
        var body = bodyBuilder.ToString().Trim();
        Console.WriteLine($"{DateTime.Now}: Parsed HTTP request: {method} {path}");
        return new HttpRequest
        {
            Method = method,
            Path = path,
            Query = query,
            Headers = headers,
            Body = body
        };
    }

    public static string ExtractBearerToken(string? authHeader)
    {
        if (string.IsNullOrEmpty(authHeader)) return "";
        const string prefix = "Bearer ";
        if (authHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return authHeader[prefix.Length..].Trim();
        return "";
    }

    public static string FormatHttpResponse(int statusCode, string contentType, string body)
    {
        string reason = statusCode switch
        {
            200 => "OK",
            201 => "Created",
            204 => "No Content",
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            500 => "Internal Server Error",
            _ => "Unknown"
        };
        var sb = new StringBuilder();
        sb.AppendLine($"HTTP/1.1 {statusCode} {reason}");
        sb.AppendLine($"Content-Type: {contentType}");
        sb.AppendLine($"Content-Length: {Encoding.UTF8.GetByteCount(body)}");
        sb.AppendLine("Connection: close");
        sb.AppendLine();
        sb.Append(body);
        return sb.ToString();
    }
}