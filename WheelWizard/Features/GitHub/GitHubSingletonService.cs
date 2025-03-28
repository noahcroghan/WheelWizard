using WheelWizard.GitHub.Domain;
using WheelWizard.Shared.Services;

namespace WheelWizard.GitHub;

public interface IGitHubSingletonService
{
    /// <summary>
    /// Get the releases for a GitHub repository.
    /// </summary>
    Task<OperationResult<List<GithubRelease>>> GetReleasesAsync();
}

public class GitHubSingletonService(IApiCaller<IGitHubApi> apiService) : IGitHubSingletonService
{
    public async Task<OperationResult<List<GithubRelease>>> GetReleasesAsync()
    {
        return await apiService.CallApiAsync(gitHubApi => gitHubApi.GetReleasesAsync("TeamWheelWizard", "WheelWizard", 3));
    }
}
