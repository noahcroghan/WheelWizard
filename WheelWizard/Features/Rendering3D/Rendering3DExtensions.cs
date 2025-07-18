using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
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

        // Use factory patterns for services that require GraphicsDevice
        // since GraphicsDevice is created internally by the MonoGame renderer
        services.AddTransient<I3DScene>(serviceProvider =>
        {
            // These services are created internally by the MonoGame renderer
            // and don't need to be resolved by DI
            throw new InvalidOperationException("I3DScene should be accessed through IMonoGameRenderer.Scene property");
        });

        services.AddTransient<I3DCamera>(serviceProvider =>
        {
            // These services are created internally by the MonoGame renderer
            // and don't need to be resolved by DI
            throw new InvalidOperationException("I3DCamera should be accessed through I3DScene.Camera property");
        });

        services.AddTransient<I3DLighting>(serviceProvider =>
        {
            // These services are created internally by the MonoGame renderer
            // and don't need to be resolved by DI
            throw new InvalidOperationException("I3DLighting should be accessed through I3DScene.Lighting property");
        });

        return services;
    }
}
