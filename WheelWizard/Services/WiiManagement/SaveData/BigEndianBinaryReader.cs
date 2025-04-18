using System.Text;

namespace WheelWizard.Services.WiiManagement.SaveData;

public static class BigEndianBinaryReader
{
    //Helper functions to convert a buffer to an uint using big endian

    public static ulong BufferToUint64(byte[] buffer, int offset)
    {
        return ((ulong)buffer[offset] << 56)
            | ((ulong)buffer[offset + 1] << 48)
            | ((ulong)buffer[offset + 2] << 40)
            | ((ulong)buffer[offset + 3] << 32)
            | ((ulong)buffer[offset + 4] << 24)
            | ((ulong)buffer[offset + 5] << 16)
            | ((ulong)buffer[offset + 6] << 8)
            | ((ulong)buffer[offset + 7]);
    }

    public static uint BufferToUint32(byte[] data, int offset)
    {
        return (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
    }

    public static uint BufferToUint16(byte[] data, int offset)
    {
        return (uint)((data[offset] << 8) | data[offset + 1]);
    }

    //big endian get the string
    public static string GetUtf16String(byte[] data, int offset, int maxLength)
    {
        var bytes = new List<byte>();
        for (var i = 0; i < maxLength * 2; i += 2)
        {
            var b1 = data[offset + i];
            var b2 = data[offset + i + 1];
            if (b1 == 0 && b2 == 0)
                break;
            bytes.Add(b1);
            bytes.Add(b2);
        }
        return Encoding.BigEndianUnicode.GetString(bytes.ToArray());
    }

    public static void WriteUInt32BigEndian(byte[] data, int offset, uint value)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)((value >> 16) & 0xFF);
        data[offset + 2] = (byte)((value >> 8) & 0xFF);
        data[offset + 3] = (byte)(value & 0xFF);
    }

    public static void WriteUInt16BigEndian(byte[] data, int offset, ushort value)
    {
        data[offset] = (byte)(value >> 8);
        data[offset + 1] = (byte)(value & 0xFF);
    }

    public static void WriteUInt64BigEndian(byte[] buf, int offset, ulong value)
    {
        buf[offset + 0] = (byte)(value >> 56);
        buf[offset + 1] = (byte)(value >> 48);
        buf[offset + 2] = (byte)(value >> 40);
        buf[offset + 3] = (byte)(value >> 32);
        buf[offset + 4] = (byte)(value >> 24);
        buf[offset + 5] = (byte)(value >> 16);
        buf[offset + 6] = (byte)(value >> 8);
        buf[offset + 7] = (byte)(value);
    }
}
