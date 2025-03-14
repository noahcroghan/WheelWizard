using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using WheelWizard.Services;
using WheelWizard.Services.Settings;
using WheelWizard.Views.Popups.Generic;

namespace WheelWizard.Services.Launcher.Helpers;

public static class DolphinLaunchHelper
{
    public static void KillDolphin() //dont tell PETA
    {
        var dolphinLocation = PathManager.DolphinFilePath;
        if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(dolphinLocation)).Length == 0) return;

        var dolphinProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(dolphinLocation));
        foreach (var process in dolphinProcesses)
        {
            process.Kill();
        }
    }

    private static bool IsFixableFlatpakGamePath(string gameFilePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) &&
            PathManager.IsFlatpakSandboxed() &&
            PathManager.IsFlatpakDolphinFilePath())
        {
            // Because with the file picker on a Flatpak build, we get XDG portal paths like this...
            // We can fix Flatpak Dolphin to gain access to this path though.
            string fixablePattern = @"^/run/user/(\d+)/doc";
            Regex fixablePatternRegex = new Regex(fixablePattern);
            return fixablePatternRegex.IsMatch(gameFilePath);
        }
        return false;
    }

    private static void TryFixFlatpakGameFileAccess()
    {
        var gameFilePath = PathManager.GameFilePath;
        if (IsFixableFlatpakGamePath(gameFilePath))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "flatpak",
                    ArgumentList = {
                        "document-export",
                        "--app=org.DolphinEmu.dolphin-emu",
                        "-r",
                        "--",
                        gameFilePath,
                    },
                    CreateNoWindow = true,
                    UseShellExecute = false
                })?.WaitForExit();
            }
            catch (Exception ex)
            {
            // Ignore failed export
            }
        }
    }

    public static void LaunchDolphin(string arguments = "", bool shellExecute = false)
    {
        try
        {
            TryFixFlatpakGameFileAccess();
            var startInfo = new ProcessStartInfo();

            var dolphinLocation = (string)SettingsManager.DOLPHIN_LOCATION.Get();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows builds
                startInfo.FileName = dolphinLocation;
                startInfo.Arguments = arguments;
                startInfo.UseShellExecute = shellExecute;
            }
            else
            {
                startInfo.FileName = "/usr/bin/env";
                startInfo.ArgumentList.Add("sh");
                startInfo.ArgumentList.Add("-c");
                startInfo.ArgumentList.Add("--");
                startInfo.ArgumentList.Add($"{dolphinLocation} {arguments}");
                startInfo.UseShellExecute = false;
            }

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            new MessageBoxWindow()
                .SetMessageType(MessageBoxWindow.MessageType.Error)
                .SetTitleText("Failed to launch Dolphin")
                .SetInfoText($"Reason: {ex.Message}")
                .Show();
        }
    }
}
