using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using WheelWizard.GitHub.Domain;
using WheelWizard.RrRooms;

namespace WheelWizard.GitHub;

public interface IGitHubSingletonService
{
    /// <summary>
    /// Get the releases for a GitHub repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repository">The name of the repository.</param>
    /// <param name="count">The number of releases to get.</param>
    /// <returns>A list of releases for the repository.</returns>
    Task<OperationResult<List<GithubRelease>>> GetReleasesAsync(string owner, string repository, int count = 3);
}

public class GitHubSingletonService(IServiceScopeFactory scopeFactory, ILogger<RrRoomsSingletonService> logger) : IGitHubSingletonService
{
    public async Task<OperationResult<List<GithubRelease>>> GetReleasesAsync(string owner, string repository, int count = 3)
    {
        using var scope = scopeFactory.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IGitHubApi>();

        try
        {
            var response = await api.GetReleasesAsync(owner, repository, count);
            if (response.IsSuccessful)
                return response.Content;


            logger.LogError("Failed to get releases from GitHub API: {@Error}", response.Error);
            return new OperationError { Message = "Failed to get releases from GitHub API: " + response.Error.ReasonPhrase };
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException socketException)
        {
            logger.LogError(ex, "Failed to connect to GitHub API: {Message}", socketException.Message);

            return new OperationError
            {
                Message = "Failed to connect to GitHub API: " + socketException.Message,
                Exception = socketException
            };
        }
    }
}
