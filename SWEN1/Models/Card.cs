using System.Text.Json.Serialization;

namespace SWEN1.Models;

public class Card
{
    [JsonPropertyName("Id")]
    public string Id { get; set; } = "";
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "";
    [JsonPropertyName("Damage")]
    public decimal Damage { get; set; }
    public string ElementType { get; set; } = "Normal";
    public string CardType { get; set; } = "Monster";
}