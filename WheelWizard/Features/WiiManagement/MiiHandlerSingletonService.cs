using WheelWizard.WiiManagement.Domain;
using WheelWizard.WiiManagement.Domain.Enums;

namespace WheelWizard.WiiManagement;

public interface IMiiDbService
{
    List<FullMii> GetAllMiis();
    OperationResult<FullMii> GetByClientId(uint clientId);
    OperationResult Update(FullMii updatedMii);
    OperationResult UpdateName(uint clientId, string newName);
}


public class MiiDbService(IMiiRepository repository) : IMiiDbService
{
    public List<FullMii> GetAllMiis()
    {
        var result = new List<FullMii>();
        var blocks = repository.LoadAllBlocks();

        foreach (var block in blocks)
        {
            var miiResult = MiiSerializer.Deserialize(block);
            if (miiResult.IsSuccess)
                result.Add(miiResult.Value);
        }

        return result;
    }

    public OperationResult<FullMii> GetByClientId(uint clientId)
    {
        var raw = repository.GetRawBlockByClientId(clientId);
        if (raw == null || raw.Length != MiiSerializer.MiiBlockSize)
            return Fail<FullMii>("Mii block not found or invalid.");

        return MiiSerializer.Deserialize(raw);
    }

    public OperationResult Update(FullMii updatedMii)
    {
        var serialized = MiiSerializer.Serialize(updatedMii);
        if (serialized.IsFailure)
            return serialized.Error;
        return repository.UpdateBlockByClientId(updatedMii.MiiId, serialized.Value);
    }

    public OperationResult UpdateName(uint clientId, string newName)
    {
        var result = GetByClientId(clientId);
        if (result.IsFailure)
            return result;

        var mii = result.Value;

        var nameResult = MiiName.Create(newName);
        if (nameResult.IsFailure)
            return nameResult;

        mii.Name = nameResult.Value;
        return Update(mii);
    }
}
