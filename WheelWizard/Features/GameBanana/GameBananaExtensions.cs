using WheelWizard.GameBanana.Domain;
using WheelWizard.Services;

namespace WheelWizard.GameBanana;

public static class GameBananaExtensions
{
    public static IServiceCollection AddGameBanana(this IServiceCollection services)
    {
        services.AddWhWzRefitApi<IGameBananaApi>(Endpoints.GameBananaBaseAddress, null, "gamebanana.com");
        services.AddSingleton<IGameBananaSingletonService, GameBananaSingletonService>();
        return services;
    }
}
