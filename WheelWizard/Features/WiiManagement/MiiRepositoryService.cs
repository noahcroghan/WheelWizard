using System.IO.Abstractions;
using WheelWizard.Helpers;
using WheelWizard.Services;
using WheelWizard.Services.WiiManagement.SaveData;
namespace WheelWizard.WiiManagement.Domain;

public interface IMiiRepository
{
    List<byte[]> LoadAllBlocks();
    OperationResult SaveAllBlocks(List<byte[]> blocks);
    byte[]? GetRawBlockByClientId(uint clientId);
    OperationResult UpdateBlockByClientId(uint clientId, byte[] newBlock);
}

public class MiiRepositoryService(IFileSystem fileSystem) : IMiiRepository
{
    private const int MiiLength = 74;
    private const int MaxMiiSlots = 100;
    private const int CrcOffset = 0x1F1DE;
    private const int HeaderOffset = 0x04;
    private static readonly byte[] EmptyMii = new byte[MiiLength];
    private readonly string _filePath = PathManager.WiiDbFile;
    
    public List<byte[]> LoadAllBlocks()
    {
        var result = new List<byte[]>();

        var database = ReadDatabase();
        if (database.Length < HeaderOffset)
            return result;

        using var ms = new MemoryStream(database);
        ms.Seek(HeaderOffset, SeekOrigin.Begin);

        for (var i = 0; i < MaxMiiSlots; i++)
        {
            var block = new byte[MiiLength];
            var read = ms.Read(block, 0, MiiLength);
            if (read < MiiLength)
                break;

            result.Add(block.SequenceEqual(EmptyMii) ? new byte[MiiLength] : block);
        }

        return result;
    }

    public OperationResult SaveAllBlocks(List<byte[]> blocks)
    {
        if (!fileSystem.File.Exists(_filePath))
            return Fail("RFL_DB.dat not found.");

        var db = ReadDatabase();
        using var ms = new MemoryStream(db);
        ms.Seek(HeaderOffset, SeekOrigin.Begin);

        for (int i = 0; i < MaxMiiSlots; i++)
        {
            var block = i < blocks.Count ? blocks[i] : EmptyMii;
            ms.Write(block, 0, MiiLength);
        }

        if (db.Length >= CrcOffset + 2)
        {
            var crc = CalculateCrc16(db, 0, CrcOffset);
            db[CrcOffset] = (byte)(crc >> 8);
            db[CrcOffset + 1] = (byte)(crc & 0xFF);
        }

        fileSystem.File.WriteAllBytes(_filePath, db);
        return Ok();
    }

    public byte[]? GetRawBlockByClientId(uint clientId)
    {
        if (clientId == 0) return null;

        var blocks = LoadAllBlocks();
        foreach (var block in blocks)
        {
            if (block.Length != MiiLength)
                continue;

            var thisId = BigEndianBinaryReader.ReadLittleEndianUInt32(block, 0x18);
            if (thisId == clientId)
                return block;
        }

        return null;
    }

    public OperationResult UpdateBlockByClientId(uint clientId, byte[] newBlock)
    {
        if (clientId == 0)
            return Fail("Invalid ClientId.");
        if (newBlock.Length != MiiLength)
            return Fail("Mii block size invalid.");
        if (!fileSystem.File.Exists(_filePath))
            return Fail("RFL_DB.dat not found.");

        var allBlocks = LoadAllBlocks();
        var updated = false;

        for (int i = 0; i < allBlocks.Count; i++)
        {
            var block = allBlocks[i];
            if (block.Length != MiiLength)
                continue;

            var thisId = BigEndianBinaryReader.ReadLittleEndianUInt32(block, 0x18);
            if (thisId != clientId)
                continue;

            Array.Copy(newBlock, 0, allBlocks[i], 0, MiiLength);
            updated = true;
            break;
        }

        if (!updated)
            return Fail("Mii not found.");
        
        return SaveAllBlocks(allBlocks);
    }

    private byte[] ReadDatabase()
    {
        try
        {
            return File.Exists(_filePath)
                ? File.ReadAllBytes(_filePath)
                : Array.Empty<byte>();
        }
        catch
        {
            return Array.Empty<byte>();
        }
    }

    private static ushort CalculateCrc16(byte[] data, int offset, int length)
    {
        const ushort polynomial = 0x1021;
        ushort crc = 0x0000;

        for (int i = offset; i < offset + length; i++)
        {
            crc ^= (ushort)(data[i] << 8);
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ polynomial);
                else
                    crc <<= 1;
            }
        }

        return crc;
    }
}
