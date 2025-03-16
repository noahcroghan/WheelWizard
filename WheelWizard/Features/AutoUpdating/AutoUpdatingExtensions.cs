using WheelWizard.AutoUpdating.Platforms;

namespace WheelWizard.AutoUpdating;

public static class AutoUpdatingExtensions
{
    public static IServiceCollection AddAutoUpdating(this IServiceCollection services)
    {
        services.AddSingleton<IAutoUpdaterSingletonService, AutoUpdaterSingletonService>();

        var implementationType = typeof(FallbackUpdatePlatform);
#if WINDOWS
        implementationType = typeof(WindowsUpdatePlatform);
#elif LINUX
        implementationType = typeof(LinuxUpdatePlatform);
#elif MACOS
        // MacOS updater
#endif

        services.AddSingleton(typeof(IUpdatePlatform), implementationType);
        
        return services;
    }
}
