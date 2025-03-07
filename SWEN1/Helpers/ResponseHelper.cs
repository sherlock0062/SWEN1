using System.Text.Json;

namespace SWEN1.Helpers;

public static class ResponseHelper
{
    public static (int, string, string) ErrorResponse(int code, string message)
    {
        return (code, "application/json", $"{{\"HTTP:\":\"{code}\":\"error\":\"{message}\"}}");
    }

    public static (int, string, string) JsonResponse(int code, object data)
    {
        return (code, "application/json", JsonSerializer.Serialize(data));
    }
}