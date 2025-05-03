using WheelWizard.MiiImages.Domain;
using WheelWizard.Services;

namespace WheelWizard.MiiImages;

public static class MiiImagesExtensions
{
    public static IServiceCollection AddMiiImages(this IServiceCollection services)
    {
        services.AddWhWzRefitApi<IMiiIMagesApi>(Endpoints.MiiImageAddress);

        services.AddSingleton<IMiiImagesSingletonService, MiiImagesSingletonService>();

        return services;
    }
}
