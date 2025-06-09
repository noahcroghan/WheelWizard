namespace WheelWizard.Models.GameData;

public class LicenseProfile : PlayerProfileBase
{
    public required uint TotalRaceCount { get; set; }
    public required uint TotalWinCount { get; set; }
    public List<FriendProfile> Friends { get; set; } = [];
    public Statistics Statistics { get; set; }
}
