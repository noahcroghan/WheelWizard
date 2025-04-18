using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.WiiManagement;

public static class WiiManagementExtensions
{
    public static IServiceCollection AddWiiManagement(this IServiceCollection services)
    {
        services.AddSingleton<IMiiDbService, MiiDbService>();
        services.AddSingleton<IMiiRepositoryService, MiiRepositoryServiceService>();
        services.AddSingleton<IGameLicenseSingletonService, GameLicenseSingletonService>();
        return services;
    }

    public static bool IsTheSameAs(this Mii self, Mii? other)
    {
        if (other == null)
            return false;

        var selfBytes = MiiSerializer.Serialize(self);
        if (selfBytes.IsFailure)
            return false;

        var otherBytes = MiiSerializer.Serialize(other);
        if (otherBytes.IsFailure)
            return false;

        return Convert.ToBase64String(selfBytes.Value) == Convert.ToBase64String(otherBytes.Value);
    }
}
