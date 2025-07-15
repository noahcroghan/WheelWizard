using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D;

public static class Rendering3DExtensions
{
    public static IServiceCollection AddRendering3D(this IServiceCollection services)
    {
        services.AddSingleton<IRendering3DSingletonService, Rendering3DSingletonService>();

        return services;
    }
}
