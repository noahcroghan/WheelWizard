using WheelWizard.Rendering3D.Domain;
using WheelWizard.Rendering3D.Services;

namespace WheelWizard.Rendering3D;

public static class Rendering3DExtensions
{
    /// <summary>
    /// Adds the Rendering3D feature services to the service collection
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRendering3D(this IServiceCollection services)
    {
        services.AddTransient<IMonoGameRenderer, MonoGameRenderer>();

        return services;
    }
}
