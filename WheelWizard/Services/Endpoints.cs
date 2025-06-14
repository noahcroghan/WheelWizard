using WheelWizard.Services.Settings;

namespace WheelWizard.Services;

public static class Endpoints
{
    /// <summary>
    /// The base address for accessing room data
    /// </summary>
    public const string RwfcBaseAddress = "http://rwfc.net";

    /// <summary>
    /// The base address for accessing the WheelWizard data (data that we control)
    /// </summary>
    public const string WhWzDataBaseAddress = "https://185.199.109.133/TeamWheelWizard/WheelWizard-Data/main";

    /// <summary>
    /// The base address for accessing the GameBanana API
    /// </summary>
    public const string GameBananaBaseAddress = "https://104.26.8.16/apiv11";

    /// <summary>
    /// The address for the GitHub API
    /// </summary>
    public const string GitHubAddress = "https://api.github.com";

    /// <summary>
    /// The address for the Mii image
    /// </summary>
    public static string MiiImageAddress =>
        (string)SettingsManager.URL_OVERRIDE_MII_IMAGE.Get() != ""
            ? (string)SettingsManager.URL_OVERRIDE_MII_IMAGE.Get()
            : "https://studio.mii.nintendo.com"; //216.239.32.21

    // TODO: Refactor all the URLs seen below

    // Retro Rewind
    public const string RRUrl = "http://update.rwfc.net:8000/";
    public const string RRZipUrl = RRUrl + "RetroRewind/zip/RetroRewind.zip";
    public const string RRVersionUrl = RRUrl + "RetroRewind/RetroRewindVersion.txt";
    public const string RRVersionDeleteUrl = RRUrl + "RetroRewind/RetroRewindDelete.txt";
    public const string RRDiscordUrl = "https://discord.gg/yH3ReN8EhQ";

    // Branding Urls
    public const string WhWzDiscordUrl = "https://discord.gg/vZ7T2wJnsq";
    public const string WhWzGithubUrl = "https://github.com/TeamWheelWizard/WheelWizard";
    public const string SupportLink = "https://ko-fi.com/wheelwizard";

    // Other
    public const string MiiChannelWAD = "-";
}
