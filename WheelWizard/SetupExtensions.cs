using WheelWizard.AutoUpdating;
using WheelWizard.Branding;
using WheelWizard.GitHub;
using WheelWizard.RrRooms;

namespace WheelWizard;

public static class SetupExtensions
{
    /// <summary>
    /// Adds the services required for WheelWizard.
    /// </summary>
    public static void AddWheelWizardServices(this IServiceCollection services)
    {
        services.AddAutoUpdating();
        services.AddBranding();
        services.AddGitHub();
        services.AddRrRooms();
    }
}
