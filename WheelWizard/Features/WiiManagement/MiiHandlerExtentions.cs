using WheelWizard.WiiManagement.Domain;

namespace WheelWizard.WiiManagement;

public static class MiiSerializerExtensions
{
    public static IServiceCollection AddMiiSerializer(this IServiceCollection services)
    {
        services.AddSingleton<IMiiDbService, MiiDbService>();
        services.AddSingleton<IMiiRepository, FileMiiRepository>(); 
        return services;
    }
}

