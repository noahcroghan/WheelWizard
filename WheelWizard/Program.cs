using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using WheelWizard.Helpers;
using WheelWizard.Services;
using WheelWizard.Services.Settings;
using WheelWizard.Services.UrlProtocol;

namespace WheelWizard;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        PrintStartUpMessage();
        Setup();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<Views.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void SetupWorkingDirectory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && EnvHelper.IsFlatpakSandboxed())
        {
            // In this case, we would not want executable directory-relative paths, since this is in `/app/bin`.
            // We are going to use the home directory instead (this should be the original working directory anyway).
            Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        else
        {
            // Resolve all relative paths based on the WheelWizard executable's directory by default
            string executableDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            Environment.CurrentDirectory = executableDirectory;
        }

        // Enable overriding this base/working directory through the `WW_BASEDIR` environment variable
        // (this can be relative to the default WheelWizard working directory as well).
        // This override also influences the `portable-ww.txt` portability check.
        string whWzBaseDir = Environment.GetEnvironmentVariable("WW_BASEDIR") ?? string.Empty;
        try
        {
            string whWzBaseDirAbsolute = Path.GetFullPath(whWzBaseDir);
            Environment.CurrentDirectory = whWzBaseDirAbsolute;
        }
        catch
        {
            // Keep the default base/working directory
        }
    }

    private static void Setup()
    {
        SetupWorkingDirectory();
        SettingsManager.Instance.LoadSettings();
        BadgeManager.Instance.LoadBadges();
        UrlProtocolManager.SetWhWzScheme();
    }


    private static void PrintStartUpMessage()
    {
        var modeCheck = "release";
        var osCheck = "unknown";

#if DEBUG
        modeCheck = "debug";
#endif

#if WINDOWS
        osCheck = "windows";
#elif LINUX
        osCheck = "linux";
#elif MACOS
        osCheck = "macos";
#endif

        Console.WriteLine($"Application start [mode: {modeCheck}, os: {osCheck}]");
    }
}
