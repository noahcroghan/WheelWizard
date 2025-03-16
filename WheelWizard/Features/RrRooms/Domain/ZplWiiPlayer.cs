namespace WheelWizard.RrRooms;

public sealed class ZplWiiPlayer
{
    public required string Count { get; set; }
    public required string Pid { get; set; }
    public required string Name { get; set; }
    public required string ConnMap { get; set; }
    public required string ConnFail { get; set; }
    public required string Suspend { get; set; }
    public required string Fc { get; set; }
    public string Ev { get; set; } = "--";
    public string Eb { get; set; } = "--";
    public List<ZplMii> Mii { get; set; } = [];
}
