using System.Text.Json.Serialization;

namespace WheelWizard.GameBanana.Domain;

public class GameBananaSearchResults
{
    [JsonPropertyName("_aMetadata")]
    public required GameBananaSearchMetaData MetaData { get; set; } // Metadata for the API response (e.g., total records, pagination)

    [JsonPropertyName("_aRecords")]
    public required List<GameBananaModPreview> Records { get; set; } // List of records representing mods or other GameBanana content
}
