using WheelWizard.Helpers;

namespace WheelWizard.Models.GameData;

public class FriendProfile : PlayerProfileBase
{
    public required uint Wins { get; set; }
    public required uint Losses { get; set; }
    public bool IsMutual { get; init; }

    public required byte CountryCode { get; set; }
    public string CountryName => Humanizer.GetCountryEmoji(CountryCode);
}
