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

    public static OperationResult<Mii> Clone(this Mii self)
    {
        // This is not the fastest way to clone, but it is the easiest way.
        var miiId = self.MiiId;
        self.MiiId = 1; // Make it 1 for the cloning process to ensure that that is not a reason for failure.
        // That being said, we still reset the ID afterward to make sure that if the og is false, then the clone is also false.

        var selfBytes = MiiSerializer.Serialize(self);
        self.MiiId = miiId; // IMPORTANT: Set the MiiId back, since you are changing the original record here, we don't want that.
        if (selfBytes.IsFailure)
            return selfBytes.Error;

        var cloneResult = MiiSerializer.Deserialize(selfBytes.Value);
        if (cloneResult.IsFailure)
            return cloneResult.Error;

        cloneResult.Value.MiiId = miiId; // IMPORTANT: Also make sure that the clone has the same id as the OG, it's a clone after all.
        return cloneResult.Value;
    }
}
