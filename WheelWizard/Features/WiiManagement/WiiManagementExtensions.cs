namespace WheelWizard.WiiManagement;

public static class WiiManagementExtensions
{
    public static IServiceCollection AddWiiManagement(this IServiceCollection services)
    {
        services.AddSingleton<IMiiDbService, MiiDbService>();
        services.AddSingleton<IMiiRepository, MiiRepositoryService>();
        services.AddSingleton<IGameDataSingletonService, GameDataSingletonService>();
        return services;
    }
}
