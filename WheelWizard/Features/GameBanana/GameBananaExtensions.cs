using System.Text.Json;
using WheelWizard.GameBanana.Domain;
using WheelWizard.Services;
using WheelWizard.Shared.JsonConverters;

namespace WheelWizard.GameBanana;

public static class GitHubExtensions
{
    public static IServiceCollection AddGitHub(this IServiceCollection services)
    {
        services.AddWhWzRefitApi<IGameBananaApi>(
            Endpoints.GameBananaBaseAddress
        );

        services.AddSingleton<IGameBananaSingletonService, GameBananaSingletonService>();
        return services;
    }
}
