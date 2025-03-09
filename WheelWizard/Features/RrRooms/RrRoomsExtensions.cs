using Refit;
using System.Text.Json;
using WheelWizard.Services;

namespace WheelWizard.RrRooms;

public static class RrRoomsExtensions
{
    public static IServiceCollection AddRrRooms(this IServiceCollection services)
    {
        services.AddRefitClient<IZplWiiApi>(new()
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            })
        }).ConfigureHttpClient((sp, client) =>
        {
            client.ConfigureWheelWizardClient(sp);

            client.BaseAddress = new(Endpoints.ZplWiiBaseAddress);
        }).AddStandardResilienceHandler();

        services.AddSingleton<IRrRoomsSingletonService, RrRoomsSingletonService>();

        return services;
    }
}
