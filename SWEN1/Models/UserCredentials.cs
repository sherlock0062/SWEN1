using System.Text.Json.Serialization;

namespace SWEN1.Models;

public class UserCredentials
{
    [JsonPropertyName("Username")]
    public string? Username { get; set; }
    [JsonPropertyName("Password")]
    public string? Password { get; set; }
}