namespace WheelWizard.RrRooms;

public sealed class ZplWiiRoom
{
    public required string Id { get; set; }

    public string? Game { get; set; }
    public required DateTime Created { get; set; }
    public required string Type { get; set; }
    public required bool Suspend { get; set; }
    public string? Host { get; set; }
    public string? Rk { get; set; }
    public required Dictionary<string, ZplWiiPlayer> Players { get; set; }
}
