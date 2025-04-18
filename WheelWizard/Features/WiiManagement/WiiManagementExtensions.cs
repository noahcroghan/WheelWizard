namespace WheelWizard.WiiManagement;

public static class WiiManagementExtensions
{
    public static IServiceCollection AddWiiManagement(this IServiceCollection services)
    {
        services.AddSingleton<IMiiDbService, MiiDbService>();
        services.AddSingleton<IMiiRepositoryService, MiiRepositoryServiceService>();
        services.AddSingleton<IGameLicenseSingletonService, GameLicenseSingletonService>();
        return services;
    }
}
