using System.Text.Json.Serialization;

namespace WheelWizard.GameBanana.Domain;

public class GameBananaPreviewMedia
{
    [JsonPropertyName("_aMetadata")]
    public GameBananaPreviewMetaData? MetaData { get; set; } // Not all previews have this

    [JsonPropertyName("_aImages")]
    public List<GameBananaImage> Images { get; set; } = [];
    //public string? FirstImageUrl => _aImages.Count > 0 ? $"{_aImages[0]._sBaseUrl}/{_aImages[0]._sFile}" : null;
}
