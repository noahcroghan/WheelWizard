using System.Text.Json;
using WheelWizard.Services;

namespace WheelWizard.RrRooms;

public static class RrRoomsExtensions
{
    public static IServiceCollection AddRrRooms(this IServiceCollection services)
    {
        services.AddWhWzRefitApi<IRwfcApi>(Endpoints.RwfcBaseAddress, new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        services.AddSingleton<IRrRoomsSingletonService, RrRoomsSingletonService>();

        return services;
    }
}
