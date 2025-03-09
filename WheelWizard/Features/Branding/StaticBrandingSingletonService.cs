using WheelWizard.Services;

namespace WheelWizard.Branding;

/// <summary>
/// Temporary branding service to be replaced with a dynamic service in the future.
/// </summary>
public class StaticBrandingSingletonService : IBrandingSingletonService
{
    public Branding Branding { get; } = new()
    {
        DisplayName = "Wheel Wizard",
        Identifier = "WheelWizard",
        Version = "2.0.1",
        RepositoryUrl = new(Endpoints.WhWzGithubUrl),
        DiscordUrl = new(Endpoints.WhWzDiscordUrl),
        SupportUrl = new(Endpoints.SupportLink)
    };
}
