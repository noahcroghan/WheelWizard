using Refit;

namespace WheelWizard.GitHub.Domain;

public interface IGitHubApi
{
    [Get("/repos/{owner}/{repository}/releases")]
    Task<IApiResponse<List<GithubRelease>>> GetReleasesAsync(string owner, string repository, [AliasAs("per_page")] int count = 3);
}
