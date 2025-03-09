using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace WheelWizard.RrRooms;

public interface IRrRoomsSingletonService
{
    Task<List<ZplWiiRoom>> GetRoomsAsync();
}

public class RrRoomsSingletonService(IServiceScopeFactory scopeFactory, ILogger<RrRoomsSingletonService> logger) : IRrRoomsSingletonService
{
    public async Task<List<ZplWiiRoom>> GetRoomsAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IZplWiiApi>();

        try
        {
            using var response = await api.GetWiiGroupsAsync();

            if (response.IsSuccessful)
                return response.Content;

            logger.LogError("Failed to get rooms from ZplWii API: {StatusCode} {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            return [];
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException socketException)
        {
            logger.LogError(ex, "Failed to connect to ZplWii API: {Message}", socketException.Message);
            return [];
        }
    }
}
