using WheelWizard.GameBanana.Domain;
using WheelWizard.Shared.Services;

namespace WheelWizard.GameBanana;

public interface IGameBananaSingletonService
{
    /// <summary>
    /// Get the mods catalog from GameBanana. If you don't provide a search term, it will return the featured mods.
    /// </summary>
    Task<OperationResult<GameBananaSearchResults>> GetModSearchResults(string searchTerm, int page = 1);

    /// <summary>
    /// Gets all the details of a mod from the GameBanana API.
    /// </summary>
    Task<OperationResult<GameBananaModDetails>> GetModDetails(int modId);
}

public class GameBananaSingletonService(IApiCaller<IGameBananaApi> apiService) : IGameBananaSingletonService
{
    private const int GameId = 5896;

    public async Task<OperationResult<GameBananaSearchResults>> GetModSearchResults(string searchTerm, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await apiService.CallApiAsync(gitHubApi => gitHubApi.GetFeaturedMods(GameId, page));

        return await apiService.CallApiAsync(gitHubApi => gitHubApi.GetModSearchResults(searchTerm, GameId, page));
    }

    public async Task<OperationResult<GameBananaModDetails>> GetModDetails(int modId)
    {
        return await apiService.CallApiAsync(gitHubApi => gitHubApi.GetModDetails(modId));
    }
}
