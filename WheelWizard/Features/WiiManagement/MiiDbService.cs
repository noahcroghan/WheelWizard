using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.WiiManagement;

/// <summary>
/// Provides high-level operations for managing Miis in the Wii Mii database.
/// </summary>
public interface IMiiDbService
{
    /// <summary>
    /// Retrieves all Miis stored in the database.
    /// </summary>
    /// <returns>A list of fully deserialized <see cref="Mii"/> instances.</returns>
    List<Mii> GetAllMiis();

    /// <summary>
    /// Retrieves a specific Mii from the database using its unique client ID.
    /// </summary>
    /// <param name="avatarId">The unique identifier of the Mii to retrieve.</param>
    /// <returns>An <see cref="OperationResult{T}"/> containing the <see cref="Mii"/> if found and valid.</returns>
    OperationResult<Mii> GetByAvatarId(uint avatarId);

    /// <summary>
    /// Updates an existing Mii in the database with new data.
    /// </summary>
    /// <param name="updatedMii">The updated <see cref="Mii"/> object to store.</param>
    /// <returns>An <see cref="OperationResult"/> indicating success or failure.</returns>
    OperationResult Update(Mii updatedMii);

    /// <summary>
    /// Updates the name of an existing Mii in the database.
    /// </summary>
    /// <param name="clientId">The unique identifier of the Mii to update.</param>
    /// <param name="newName">The new name to assign to the Mii.</param>
    /// <returns>An <see cref="OperationResult"/> indicating success or failure.</returns>
    OperationResult UpdateName(uint clientId, string newName);

    /// <summary>
    /// Adds a new Mii to the database.
    /// </summary>
    /// <param name="newMii"></param>
    /// <param name="macAddress"></param>
    /// <returns></returns>
    OperationResult AddToDatabase(Mii? newMii, string macAddress);

    /// <summary>
    /// Removes (deletes) the Mii with the given client ID by zeroing out its slot.
    /// </summary>
    OperationResult Remove(uint clientId);

    /// <summary>
    /// Duplicates the Mii with the given client ID, assigning it a new ID and creating a new slot.
    /// </summary>
    OperationResult<Mii> Duplicate(uint clientId);

    /// <summary>
    /// Duplicates the given Mii, using its own ID to find the original, and then creating a copy.
    /// </summary>
    OperationResult<Mii> Duplicate(Mii mii);

    /// <summary>
    /// Whether the database exists or not.
    /// </summary>
    bool Exists();
}

public class MiiDbService(IMiiRepositoryService repository) : IMiiDbService
{
    public List<Mii> GetAllMiis()
    {
        var result = new List<Mii>();
        var blocks = repository.LoadAllBlocks();

        foreach (var block in blocks)
        {
            var miiResult = MiiSerializer.Deserialize(block);
            if (miiResult.IsSuccess)
                result.Add(miiResult.Value);
        }

        return result;
    }

    public bool Exists() => repository.Exists();

    public OperationResult<Mii> GetByAvatarId(uint avatarId)
    {
        var raw = repository.GetRawBlockByAvatarId(avatarId);
        if (raw == null || raw.Length != MiiSerializer.MiiBlockSize)
            return "Mii block not found or invalid.";

        return MiiSerializer.Deserialize(raw);
    }

    public OperationResult Update(Mii updatedMii)
    {
        var serialized = MiiSerializer.Serialize(updatedMii);
        if (serialized.IsFailure)
            return serialized;
        var value = repository.UpdateBlockByClientId(updatedMii.MiiId, serialized.Value);
        return value;
    }

    public OperationResult UpdateName(uint clientId, string newName)
    {
        var result = GetByAvatarId(clientId);
        if (result.IsFailure)
            return result;

        var mii = result.Value;

        var nameResult = MiiName.Create(newName);
        if (nameResult.IsFailure)
            return nameResult;

        mii.Name = nameResult.Value;
        return Update(mii);
    }

    public OperationResult AddToDatabase(Mii? newMii, string macAddress)
    {
        if (newMii == null)
            return Fail("Mii cannot be null or have an invalid ID.");

        var existing = GetByAvatarId(newMii.MiiId);
        if (existing.IsSuccess)
            return Fail("A Mii with this ID already exists.");

        var macParts = macAddress.Split(':');
        if (macParts.Length != 6)
            return Fail("Invalid MAC address format.");

        var getMacAddress = TryCatch(() =>
        {
            var macBytes = new byte[6];
            for (var i = 0; i < 6; i++)
                macBytes[i] = byte.Parse(macParts[i], System.Globalization.NumberStyles.HexNumber);
            newMii.SystemId0 = (byte)((macBytes[0] + macBytes[1] + macBytes[2]) & 0xFF);
            newMii.SystemId1 = macBytes[3];
            newMii.SystemId2 = macBytes[4];
            newMii.SystemId3 = macBytes[5];
        });
        if (getMacAddress.IsFailure)
            return getMacAddress;

        if (newMii.MiiId == 0)
        {
            var newId = 0x60000000 | (uint)Random.Shared.Next(0, 0x10000000);
            newMii.MiiId = newId;
        }

        var serialized = MiiSerializer.Serialize(newMii);
        if (serialized.IsFailure)
            return serialized;

        var result = repository.AddMiiToBlocks(serialized.Value);
        return result;
    }

    public OperationResult Remove(uint clientId)
    {
        if (clientId == 0)
            return Fail("Invalid client ID.");
        var emptyBlock = new byte[74];
        return repository.UpdateBlockByClientId(clientId, emptyBlock);
    }

    public OperationResult<Mii> Duplicate(uint clientId)
    {
        var originalResult = GetByAvatarId(clientId);
        if (originalResult.IsFailure)
            return originalResult.Error!;
        var clone = originalResult.Value;
        return Duplicate(clone);
    }

    public OperationResult<Mii> Duplicate(Mii mii)
    {
        // set miiId to 0 so it will be added as a new Mii
        mii.MiiId = 0;
        var mac = $"{mii.SystemId0}:{mii.SystemId1}:{mii.SystemId2}:{mii.SystemId3}";
        var addResult = AddToDatabase(mii, mac);
        if (addResult.IsFailure)
            return addResult.Error!;
        return mii;
    }
}
