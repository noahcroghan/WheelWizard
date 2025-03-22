using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using WheelWizard.WheelWizardData.Domain;

namespace WheelWizard.WheelWizardData;

public interface IWhWzDataSingletonService
{
    Task<WhWzStatus> GetStatusAsync();
    Task<Dictionary<string,BadgeVariant[]>> GetBadgesAsync();
}

public class WhWzDataSingletonService(IServiceScopeFactory scopeFactory, ILogger<WhWzDataSingletonService> logger) : IWhWzDataSingletonService
{
    public async Task<WhWzStatus> GetStatusAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IWhWzDataApi>();

        try
        {
            using var response = await api.GetStatusAsync();

            if (response.IsSuccessful)
                return response.Content;

            logger.LogError("Failed to get status from Wheel Wizard Data API: {@Error}", response.Error);
            return new()
            {
                Message = "Failed to get the current status of Wheel Wizard.", 
                Variant = WhWzStatusVariant.Warning
            };
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException socketException)
        {
            logger.LogError(ex, "Failed to connect to Wheel Wizard Data API: {Message}", socketException.Message);
            return new()
            {
                Message = "Can't connect to the servers. \nYou might experience internet connection issues.", 
                Variant = WhWzStatusVariant.Warning
            };
        }
    }
    
    public async Task<Dictionary<string,BadgeVariant[]>> GetBadgesAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IWhWzDataApi>();

        try
        {
            using var response = await api.GetBadgesAsync();

            if (response.IsSuccessful)
                return response.Content;

            logger.LogError("Failed to get badges from Wheel Wizard Data API: {@Error}", response.Error);
            return [];
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException socketException)
        {
            logger.LogError(ex, "Failed to connect to Wheel Wizard Data API: {Message}", socketException.Message);
            return [];
        }
    }
}
