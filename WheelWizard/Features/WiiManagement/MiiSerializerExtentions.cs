namespace WheelWizard.WiiManagement;

public static class MiiSerializerExtensions
{
    public static IServiceCollection AddMiiSerializer(this IServiceCollection services)
    {
        services.AddSingleton<IMiiSerializerSingletonService, MiiSerializerSingletonService>();
        return services;
    }
}

