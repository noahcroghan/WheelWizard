using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WheelWizard.Helpers;
using WheelWizard.Services.Settings;

namespace WheelWizard.Services;

public static class PathManager
{
    // IMPORTANT: To keep things consistent all paths should be Attrib expressions,
    //            and either end with `FilePath` or `FolderPath`

    public static string HomeFolderPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    // Paths set by the user
    public static string GameFilePath => (string)SettingsManager.GAME_LOCATION.Get();
    public static string DolphinFilePath => (string)SettingsManager.DOLPHIN_LOCATION.Get();
    public static string UserFolderPath => (string)SettingsManager.USER_FOLDER_PATH.Get();

    private static string AppDataFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static string LocalAppDataFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    // Wheel wizard's appdata paths  (dont have to be expressions since they dont depend on user input like the others)f
    public static readonly string WheelWizardAppdataPath = Path.Combine(AppDataFolder, "CT-MKWII");
    public static readonly string WheelWizardConfigFilePath = Path.Combine(WheelWizardAppdataPath, "config.json");
    public static readonly string ModsFolderPath = Path.Combine(WheelWizardAppdataPath, "Mods");
    public static readonly string ModConfigFilePath = Path.Combine(ModsFolderPath, "modconfig.json");
    public static readonly string TempModsFolderPath = Path.Combine(ModsFolderPath, "Temp");
    public static readonly string RetroRewindTempFile = Path.Combine(TempModsFolderPath, "RetroRewind.zip");
    public static string RetroRewindVersionFile => Path.Combine(RetroRewind6FolderPath, "version.txt");
    public static string WiiDbFile => Path.Combine(WiiFolderPath, "shared2", "menu", "FaceLib", "RFL_DB.dat");


    //In case it is unclear, the mods folder is a folder with mods that are desired to be installed (if enabled)
    //When launching we want to move the mods from the Mods folder to the MyStuff folder since that is the folder the game uses
    //Also remember that mods may not be in a subfolder, all mod files must be located in /MyStuff directly

    public static bool IsValidUnixCommand(string command)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/env",
                ArgumentList = {
                    "sh",
                    "-c",
                    "--",
                    $"command -v -- {command}",
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(processInfo);
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    // Helper paths for folders used across multiple files
    public static string MyStuffFolderPath => Path.Combine(RetroRewind6FolderPath, "MyStuff");
    public static string GetModDirectoryPath(string modName) => Path.Combine(ModsFolderPath, modName);
    public static string RiivolutionWhWzFolderPath => Path.Combine(LoadFolderPath, "Riivolution", "WheelWizard");
    public static string RetroRewind6FolderPath => Path.Combine(RiivolutionWhWzFolderPath, "RetroRewind6");

    // This is not the folder your save file is located in, but its the folder where every Region folder is, so the save file is in SaveFolderPath/Region
    public static string SaveFolderPath => Path.Combine(RiivolutionWhWzFolderPath, "riivolution", "save" ,"RetroWFC");

    private static string LinuxDolphinLegacyRelSubFolderPath => ".dolphin-emu";
    private static string LinuxDolphinLegacyFolderPath => Path.Combine(HomeFolderPath, LinuxDolphinLegacyRelSubFolderPath);
    private static string LinuxDolphinRelSubFolderPath => "dolphin-emu";

    private static string LinuxDolphinFlatpakAppDataFolderPath => Path.Combine(HomeFolderPath, ".var", "app", "org.DolphinEmu.dolphin-emu");
    public static string LinuxDolphinFlatpakDataDir => Path.Combine(LinuxDolphinFlatpakAppDataFolderPath, "data", LinuxDolphinRelSubFolderPath);
    public static string LinuxDolphinFlatpakConfigDir => Path.Combine(LinuxDolphinFlatpakAppDataFolderPath, "config", LinuxDolphinRelSubFolderPath);

    private static string EmptyLinuxPathIfRelative(string path)
    {
        return path.StartsWith('/') ? path : string.Empty;
    }

    public static bool IsFlatpakSandboxed()
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FLATPAK_ID"));
    }

    private static string LinuxXdgDataHome => LocalAppDataFolder;
    private static string LinuxXdgConfigHome => AppDataFolder;
    private static string LinuxHostXdgDataHome => EmptyLinuxPathIfRelative(Environment.GetEnvironmentVariable("HOST_XDG_DATA_HOME") ?? Path.Combine(HomeFolderPath, ".local", "share"));
    private static string LinuxHostXdgConfigHome => EmptyLinuxPathIfRelative(Environment.GetEnvironmentVariable("HOST_XDG_CONFIG_HOME") ?? Path.Combine(HomeFolderPath, ".config"));

    private static string LinuxDolphinHostNativeInstallConfigDir => Path.Combine(LinuxHostXdgConfigHome, LinuxDolphinRelSubFolderPath);
    private static string LinuxDolphinHostNativeInstallDataDir => Path.Combine(LinuxHostXdgDataHome, LinuxDolphinRelSubFolderPath);
    private static string LinuxDolphinNativeInstallConfigDir => Path.Combine(LinuxXdgConfigHome, LinuxDolphinRelSubFolderPath);
    private static string LinuxDolphinNativeInstallDataDir => Path.Combine(LinuxXdgDataHome, LinuxDolphinRelSubFolderPath);

    public static string DetermineCorrectLinuxDolphinNativeConfigDir
    {
        get
        {
            if (IsFlatpakSandboxed())
            {
                if (LinuxDolphinHostNativeInstallDataDir.Equals(UserFolderPath))
                {
                    return LinuxDolphinHostNativeInstallConfigDir;
                }
            }
            else if (LinuxDolphinNativeInstallDataDir.Equals(UserFolderPath))
            {
                return LinuxDolphinNativeInstallConfigDir;
            }

            return string.Empty;
        }
    }

    public static string LinuxDolphinConfigDir
    {
        get
        {
            if (IsFlatpakDolphinFilePath())
            {
                return LinuxDolphinFlatpakConfigDir;
            }
            else
            {
                return DetermineCorrectLinuxDolphinNativeConfigDir;
            }
        }
    }

    public static string LoadFolderPath
    {
        get
        {
            return Path.Combine(UserFolderPath, "Load");
        }
    }

    public static string ConfigFolderPath
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
                !LinuxDolphinLegacyFolderPath.Equals(UserFolderPath) &&
                LinuxDolphinRelSubFolderPath.Equals(Path.GetFileName(UserFolderPath)))
            {
                try
                {
                    return LinuxDolphinConfigDir;
                }
                catch (Exception ex)
                {
                    // Fall back to something that is likely not valid, will be checked later
                    return Path.Combine(UserFolderPath, "Config");
                }

            }
            return Path.Combine(UserFolderPath, "Config");
        }
    }

    public static string WiiFolderPath
    {
        get
        {
            return Path.Combine(UserFolderPath, "Wii");
        }
    }

    public static bool IsFlatpakDolphinFilePath(string filePath)
    {
        string[] flatpakFilePathSubStrings = { "flatpak", "run", "org.DolphinEmu.dolphin-emu" };
        if (string.IsNullOrWhiteSpace(filePath))
        {
            // Prioritize Flatpak Dolphin installation if no file path has been saved yet, so return true
            return true;
        }
        foreach (string substring in flatpakFilePathSubStrings)
        {
            if (!filePath.Contains(substring))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsFlatpakDolphinFilePath()
    {
        return IsFlatpakDolphinFilePath(DolphinFilePath);
    }

    // This should return null if not found since functions above require it
    private static string? TryFindLinuxFlatpakUserFolderPath()
    {
        if (Directory.Exists(LinuxDolphinFlatpakAppDataFolderPath))
            return Path.Combine(LinuxDolphinFlatpakDataDir);

        // If not found, return null.
        return null;
    }

    private static string? TryFindLinuxNativeUserFolderPath()
    {
        if (IsFlatpakSandboxed())
        {
            if (Directory.Exists(LinuxHostXdgConfigHome) && Directory.Exists(LinuxHostXdgDataHome))
            {
                if (Directory.Exists(LinuxDolphinHostNativeInstallConfigDir) && Directory.Exists(LinuxDolphinHostNativeInstallDataDir))
                    return LinuxDolphinHostNativeInstallDataDir;
            }
            return null;
        }

        if (Directory.Exists(LinuxDolphinNativeInstallConfigDir) && Directory.Exists(LinuxDolphinNativeInstallDataDir))
            return LinuxDolphinNativeInstallDataDir;

        if (Directory.Exists(LinuxDolphinLegacyFolderPath))
            return LinuxDolphinLegacyFolderPath;

        // If not found, return null.
        return null;
    }

    public static string? TryFindUserFolderPath()
    {
        var appDataPath = Path.Combine(AppDataFolder, "Dolphin Emulator");
        if (FileHelper.DirectoryExists(appDataPath))
            return appDataPath;

        // Macos path
        var libraryPath = Path.Combine(AppDataFolder, "Dolphin");
        if (FileHelper.DirectoryExists(libraryPath))
            return libraryPath;

        var documentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "Dolphin Emulator");
        if (FileHelper.DirectoryExists(documentsPath))
            return documentsPath;

        if (IsFlatpakDolphinFilePath())
        {
            return TryFindLinuxFlatpakUserFolderPath();
        }
        else
        {
            return TryFindLinuxNativeUserFolderPath();
        }
    }

    public static string? TryToFindApplicationPath() {

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var dolphinApplicationPath = Path.Combine("Dolphin.app", "Contents", "MacOS", "Dolphin");
            // Try system wide install on MacOS
            var path = Path.Combine("/Applications", dolphinApplicationPath);
                if (FileHelper.FileExists(path))
                return path;
            // Try user install on MacOS
            path = Path.Combine(HomeFolderPath, "Applications", dolphinApplicationPath);
            if (FileHelper.FileExists(path))
                return path;
        }
        return null;
    }
}
