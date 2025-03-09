namespace WheelWizard.Branding;

public static class BrandingExtensions
{
    public static IServiceCollection AddBranding(this IServiceCollection services)
    {
        // TODO: Once we deploy using Github workflows we can use the normal branding service with fileversion
        services.AddSingleton<IBrandingSingletonService, StaticBrandingSingletonService>();

        return services;
    }
}
