using System.Text.Json.Serialization;

namespace WheelWizard.GameBanana.Domain;

public class GameBananaSearchMetaData
{
    [JsonPropertyName("_nRecordCount")]
    public required int RecordCount { get; set; }

    [JsonPropertyName("_nPerpage")]
    public required int PerPage { get; set; } // Number of records per page returned by the API

    [JsonPropertyName("_bIsComplete")]
    public required bool IsComplete { get; set; }
}
