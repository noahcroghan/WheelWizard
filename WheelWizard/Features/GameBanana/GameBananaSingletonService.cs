using WheelWizard.GameBanana.Domain;
using WheelWizard.Shared.Services;

namespace WheelWizard.GameBanana;

public interface IGameBananaSingletonService
{
    /// <summary>
    /// Get the releases for a GitHub repository.
    /// </summary>
    Task<OperationResult<int>> GetReleasesAsync();
}

public class GameBananaSingletonService(IApiCaller<IGameBananaApi> apiService) : IGameBananaSingletonService
{
    public async Task<OperationResult<int>> GetReleasesAsync()
    {
        throw new NotImplementedException();
        //return await apiService.CallApiAsync(whWzDataApi => whWzDataApi);
    }
}
