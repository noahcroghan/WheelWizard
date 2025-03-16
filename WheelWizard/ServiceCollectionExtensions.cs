using WheelWizard.AutoUpdating;
using WheelWizard.Branding;
using WheelWizard.RrRooms;

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
    }
}
