using System.Diagnostics;
using System.Reflection;
using WheelWizard.Services;

namespace WheelWizard.Branding;

/// <summary>
/// A service that provides branding information for the application.
/// </summary>
public interface IBrandingSingletonService
{
    /// <summary>
    /// The branding information for the application.
    /// </summary>
    Branding Branding { get; }
}

public class BrandingSingletonService : IBrandingSingletonService
{
    public Branding Branding { get; } = new()
    {
        DisplayName = "Wheel Wizard",
        Identifier = "WheelWizard",
        Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion ?? "0.0.0",
        RepositoryUrl = new(Endpoints.WhWzGithubUrl),
        DiscordUrl = new(Endpoints.WhWzDiscordUrl),
        SupportUrl = new(Endpoints.SupportLink)
    };
}
