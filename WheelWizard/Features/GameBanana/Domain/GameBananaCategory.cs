using System.Text.Json.Serialization;

namespace WheelWizard.GameBanana.Domain;

public class GameBananaCategory
{
    [JsonPropertyName("_sName")]
    public required string Name { get; set; } //  (e.g., "Maps", "Characters")

    [JsonPropertyName("_sProfileUrl")]
    public required string ProfileUrl { get; set; }

    [JsonPropertyName("_sIconUrl")]
    public string? IconUrl { get; set; } // Unsure if all categories have an icon, and don't want risk adding `required` here
}
