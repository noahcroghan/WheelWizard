using Refit;

namespace WheelWizard.WheelWizardData.Domain;

public interface IWhWzDataApi
{
    [Get("/badges.json")]
    Task<IApiResponse<Dictionary<string,BadgeVariant[]>>> GetBadgesAsync();
    
    [Get("/status.json")]
    Task<IApiResponse<WhWzStatus>> GetStatusAsync();
}
