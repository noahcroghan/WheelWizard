using Microsoft.Extensions.DependencyInjection;
using WheelWizard.Dolphin;

namespace WheelWizard.Features.Dolphin;

public static class DolphinExtensions
{
    public static IServiceCollection AddDolphin(this IServiceCollection services)
    {
        services.AddSingleton<DolphinControllerService>();
        return services;
    }
}
