using System.Text.Json.Serialization;

namespace SWEN1.Models;

public class UserProfile
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }
    [JsonPropertyName("Bio")]
    public string? Bio { get; set; }
    [JsonPropertyName("Image")]
    public string? Image { get; set; }
}