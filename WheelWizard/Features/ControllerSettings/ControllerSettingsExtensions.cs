using Microsoft.Extensions.DependencyInjection;
using WheelWizard.ControllerSettings;
using WheelWizard.Shared.DependencyInjection;

namespace WheelWizard.ControllerSettings;

public static class ControllerSettingsExtensions
{
    public static IServiceCollection AddControllerSettings(this IServiceCollection services)
    {
        services.AddSingleton<IControllerService, UniplatformControllerService>();
        return services;
    }
}
