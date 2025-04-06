using WheelWizard.WiiManagement.Domain;

namespace WheelWizard.WiiManagement;

public static class WiiManagementExtensions
{
    public static IServiceCollection AddWiiManagement(this IServiceCollection services)
    {
        services.AddSingleton<IMiiDbService, MiiDbService>();
        services.AddSingleton<IMiiRepository, MiiRepositoryService>();
        services.AddSingleton<IGameDataLoader, GameDataLoader>();
        return services;
    }
}
