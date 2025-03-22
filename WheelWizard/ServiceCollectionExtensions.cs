using WheelWizard.AutoUpdating;
using WheelWizard.Branding;
using WheelWizard.RrRooms;
using WheelWizard.WheelWizardData;

namespace WheelWizard;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the services required for WheelWizard.
    /// </summary>
    public static void AddWheelWizardServices(this IServiceCollection services)
    {
        services.AddAutoUpdating();
        services.AddBranding();
        services.AddRrRooms();
        services.AddWhWzData();
    }
}
