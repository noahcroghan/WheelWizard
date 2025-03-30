namespace WheelWizard.WiiManagement.Domain;

public interface IMiiRepository
{
    List<byte[]> LoadAllBlocks();
    OperationResult SaveAllBlocks(List<byte[]> blocks);
    byte[]? GetRawBlockByClientId(uint clientId);
    OperationResult UpdateBlockByClientId(uint clientId, byte[] newBlock);
}
