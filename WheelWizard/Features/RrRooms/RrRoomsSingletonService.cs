using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace WheelWizard.RrRooms;

public interface IRrRoomsSingletonService
{
    Task<List<RwfcRoom>> GetRoomsAsync();
}

public class RrRoomsSingletonService(IServiceScopeFactory scopeFactory, ILogger<RrRoomsSingletonService> logger) : IRrRoomsSingletonService
{
    public async Task<List<RwfcRoom>> GetRoomsAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IRwfcApi>();

        try
        {
            using var response = await api.GetWiiGroupsAsync();

            if (response.IsSuccessful)
                return response.Content;

            logger.LogError("Failed to get rooms from Rwfc API: {@Error}", response.Error);
            return [];
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException socketException)
        {
            logger.LogError(ex, "Failed to connect to Rwfc API: {Message}", socketException.Message);
            return [];
        }
    }
}
