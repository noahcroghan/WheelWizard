using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
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

    private static void Setup()
    {
        // Resolve all relative paths based on the WheelWizard executable's directory
        Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
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
