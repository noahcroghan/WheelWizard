using WheelWizard.Services.Settings;

namespace WheelWizard.Helpers;

public static class EnvHelper
{
    public static string DetectLinuxPackageManagerInstallCommand()
    {
        if (LinuxDolphinInstaller.IsValidCommand("apt")) return "apt install -y";
        if (LinuxDolphinInstaller.IsValidCommand("apt-get")) return "apt-get -y install";
        if (LinuxDolphinInstaller.IsValidCommand("dnf")) return "dnf -y install";
        if (LinuxDolphinInstaller.IsValidCommand("yum")) return "yum -y install";
        if (LinuxDolphinInstaller.IsValidCommand("pacman")) return "pacman --noconfirm -S";
        return LinuxDolphinInstaller.IsValidCommand("zypper") ? "zypper --non-interactive install" : string.Empty; // Unknown package manager
    }
}
