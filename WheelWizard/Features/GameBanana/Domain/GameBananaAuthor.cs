using System.Text.Json.Serialization;

namespace WheelWizard.GameBanana.Domain;

public class GameBananaAuthor
{
    [JsonPropertyName("_sName")] public required string Name { get; set; }
    [JsonPropertyName("_sProfileUrl")] public required string ProfileUrl { get; set; }
    [JsonPropertyName("_sAvatarUrl")] public string? AvatarUrl { get; set; } // Some Authors didn't upload an avatar
}
