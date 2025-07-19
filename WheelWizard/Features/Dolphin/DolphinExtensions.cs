using Microsoft.Extensions.DependencyInjection;

namespace WheelWizard.Features.Dolphin;

public static class DolphinExtensions
{
    public static IServiceCollection AddDolphin(this IServiceCollection services)
    {
        services.AddSingleton<DolphinControllerService>();
        return services;
    }
}
