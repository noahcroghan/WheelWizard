using System.Text;
using WheelWizard.WiiManagement.Domain;

namespace WheelWizard.WiiManagement;
public static class MiiSerializer
{
    public const int MiiBlockSize = 74; // Each Mii block is 74 bytes

    /// <summary>
    /// Deserializes a 74-byte Mii data block into a FullMii instance.
    /// </summary>
    public static OperationResult<FullMii> Deserialize(byte[]? data)
    {
        if (data == null || data.Length != MiiBlockSize)
            return OperationResult.Fail<FullMii>("Invalid Mii data block");

        var mii = new FullMii();

        // Header (0x00 - 0x01)
        ushort header = (ushort)((data[0] << 8) | data[1]);
        mii.IsInvalid = (header & 0x8000) != 0;
        mii.IsGirl = (header & 0x4000) != 0;
        mii.Month = (header >> 10) & 0x0F;
        mii.Day = (header >> 5) & 0x1F;
        mii.FavoriteColor = (header >> 1) & 0x0F;
        mii.IsFavorite = (header & 0x1) != 0;

        // Name (0x02 - 0x15) – 10 UTF-16 characters
        mii.Name = Encoding.BigEndianUnicode.GetString(data, 2, 20).TrimEnd('\0');

        // Height & Weight (0x16 - 0x17)
        mii.Height = data[0x16];
        mii.Weight = data[0x17];

        // Mii ID (0x18 - 0x1B)
        mii.MiiId = BitConverter.ToUInt32(data, 0x18);

        // System ID (0x1C - 0x1F) – stored as four separate bytes
        mii.SystemId0 = data[0x1C];
        mii.SystemId1 = data[0x1D];
        mii.SystemId2 = data[0x1E];
        mii.SystemId3 = data[0x1F];

        // Face and personality (0x20 - 0x21)
        ushort faceData = (ushort)((data[0x20] << 8) | data[0x21]);
        mii.FaceShape = faceData >> 13;                // Top 3 bits
        mii.SkinColor = (faceData >> 10) & 0x07;         // Next 3 bits
        mii.FacialFeature = (faceData >> 6) & 0x0F;      // Next 4 bits
        mii.MingleOff = ((faceData >> 2) & 0x01) != 0;
        mii.Downloaded = (faceData & 0x01) != 0;

        // Hair (0x22 - 0x23)
        ushort hairData = (ushort)((data[0x22] << 8) | data[0x23]);
        mii.HairType = (hairData >> 9) & 0x7F;           // 7 bits
        mii.HairColor = (hairData >> 6) & 0x07;          // 3 bits
        mii.HairFlipped = ((hairData >> 5) & 0x01) != 0;   // 1 bit

        // Eyebrows (0x24 - 0x27) – 4 bytes, 32 bits
        uint eyebrowData = (uint)((data[0x24] << 24) | (data[0x25] << 16) | (data[0x26] << 8) | data[0x27]);
        mii.EyebrowType = (int)((eyebrowData >> 27) & 0x1F);         // 5 bits
        mii.EyebrowRotation = (int)((eyebrowData >> 22) & 0x0F);     // 4 bits
        mii.EyebrowColor = (int)((eyebrowData >> 13) & 0x07);        // 3 bits
        mii.EyebrowSize = (int)((eyebrowData >> 9) & 0x0F);          // 4 bits
        mii.EyebrowVertical = (int)((eyebrowData >> 4) & 0x1F);      // 5 bits
        mii.EyebrowSpacing = (int)(eyebrowData & 0x0F);              // 4 bits

        // Eyes (0x28 - 0x2B) – 4 bytes, 32 bits
        uint eyeData = (uint)((data[0x28] << 24) | (data[0x29] << 16) | (data[0x2A] << 8) | data[0x2B]);
        mii.EyeType = (int)((eyeData >> 26) & 0x3F);           // 6 bits
        mii.EyeRotation = (int)((eyeData >> 21) & 0x07);       // 3 bits
        mii.EyeVertical = (int)((eyeData >> 16) & 0x1F);       // 5 bits
        mii.EyeColor = (int)((eyeData >> 13) & 0x07);          // 3 bits
        mii.EyeSize = (int)((eyeData >> 9) & 0x07);            // 3 bits
        mii.EyeSpacing = (int)((eyeData >> 5) & 0x0F);         // 4 bits

        // Nose (0x2C - 0x2D) – 2 bytes
        ushort noseData = (ushort)((data[0x2C] << 8) | data[0x2D]);
        mii.NoseType = (int)((noseData >> 12) & 0x0F);         // 4 bits
        mii.NoseSize = (int)((noseData >> 8) & 0x0F);          // 4 bits
        mii.NoseVertical = (int)((noseData >> 3) & 0x1F);      // 5 bits

        // Lips (0x2E - 0x2F) – 2 bytes
        ushort lipData = (ushort)((data[0x2E] << 8) | data[0x2F]);
        mii.LipType = (int)((lipData >> 11) & 0x1F);           // 5 bits
        mii.LipColor = (int)((lipData >> 9) & 0x03);           // 2 bits
        mii.LipSize = (int)((lipData >> 5) & 0x0F);            // 4 bits
        mii.LipVertical = (int)(lipData & 0x1F);               // 5 bits

        // Glasses (0x30 - 0x31) – 2 bytes
        ushort glassesData = (ushort)((data[0x30] << 8) | data[0x31]);
        mii.GlassesType = (int)((glassesData >> 12) & 0x0F);       // 4 bits
        mii.GlassesColor = (int)((glassesData >> 9) & 0x07);         // 3 bits
        mii.GlassesSize = (int)((glassesData >> 5) & 0x07);          // 3 bits
        mii.GlassesVertical = (int)(glassesData & 0x1F);             // 5 bits

        // Facial Hair (0x32 - 0x33) – 2 bytes
        ushort facialHairData = (ushort)((data[0x32] << 8) | data[0x33]);
        mii.MustacheType = (int)((facialHairData >> 14) & 0x03);       // 2 bits
        mii.BeardType = (int)((facialHairData >> 12) & 0x03);          // 2 bits
        mii.FacialHairColor = (int)((facialHairData >> 9) & 0x07);       // 3 bits
        mii.MustacheSize = (int)((facialHairData >> 5) & 0x0F);          // 4 bits
        mii.MustacheVertical = (int)(facialHairData & 0x1F);             // 5 bits

        // Mole (0x34 - 0x35) – 2 bytes
        ushort moleData = (ushort)((data[0x34] << 8) | data[0x35]);
        mii.HasMole = ((moleData >> 15) & 0x01) != 0;          // 1 bit
        mii.MoleSize = (int)((moleData >> 11) & 0x0F);           // 4 bits
        mii.MoleVertical = (int)((moleData >> 6) & 0x1F);        // 5 bits
        mii.MoleHorizontal = (int)((moleData >> 1) & 0x1F);      // 5 bits

        // Creator Name (0x36 - 0x49) – 10 UTF-16 characters
        mii.CreatorName = Encoding.BigEndianUnicode.GetString(data, 0x36, 20).TrimEnd('\0');

        return mii;
    }

    /// <summary>
    /// Serializes a FullMii instance into a 74-byte data block.
    /// </summary>
    public static byte[] Serialize(FullMii mii)
    {
        byte[] data = new byte[MiiBlockSize];

        // Header (0x00 - 0x01)
        ushort header = 0;
        if (mii.IsInvalid)
            header |= 0x8000;
        if (mii.IsGirl)
            header |= 0x4000;
        header |= (ushort)((mii.Month & 0x0F) << 10);
        header |= (ushort)((mii.Day & 0x1F) << 5);
        header |= (ushort)((mii.FavoriteColor & 0x0F) << 1);
        if (mii.IsFavorite)
            header |= 0x1;
        data[0] = (byte)(header >> 8);
        data[1] = (byte)(header & 0xFF);

        // Name (0x02 - 0x15)
        var fixedName = mii.Name.Length > 10 ? mii.Name.Substring(0, 10) : mii.Name.PadRight(10, '\0');
        var nameBytes = Encoding.BigEndianUnicode.GetBytes(fixedName);
        Array.Copy(nameBytes, 0, data, 2, Math.Min(nameBytes.Length, 20));

        // Height & Weight (0x16 - 0x17)
        data[0x16] = mii.Height;
        data[0x17] = mii.Weight;

        // Mii ID (0x18 - 0x1B)
        Array.Copy(BitConverter.GetBytes(mii.MiiId), 0, data, 0x18, 4);

        // System ID (0x1C - 0x1F)
        data[0x1C] = mii.SystemId0;
        data[0x1D] = mii.SystemId1;
        data[0x1E] = mii.SystemId2;
        data[0x1F] = mii.SystemId3;

        // Face and personality (0x20 - 0x21)
        ushort faceData = 0;
        faceData |= (ushort)((mii.FaceShape & 0x07) << 13);
        faceData |= (ushort)((mii.SkinColor & 0x07) << 10);
        faceData |= (ushort)((mii.FacialFeature & 0x0F) << 6);
        faceData |= (ushort)((mii.MingleOff ? 1 : 0) << 2);
        faceData |= (ushort)(mii.Downloaded ? 1 : 0);
        data[0x20] = (byte)(faceData >> 8);
        data[0x21] = (byte)(faceData & 0xFF);

        // Hair (0x22 - 0x23)
        ushort hairData = 0;
        hairData |= (ushort)((mii.HairType & 0x7F) << 9);
        hairData |= (ushort)((mii.HairColor & 0x07) << 6);
        hairData |= (ushort)((mii.HairFlipped ? 1 : 0) << 5);
        // Lower 5 bits remain 0.
        data[0x22] = (byte)(hairData >> 8);
        data[0x23] = (byte)(hairData & 0xFF);

        // Eyebrows (0x24 - 0x27)
        uint eyebrowData = 0;
        eyebrowData |= ((uint)mii.EyebrowType & 0x1F) << 27;
        eyebrowData |= ((uint)mii.EyebrowRotation & 0x0F) << 22;
        // Skip 6 unknown bits (set to 0)
        eyebrowData |= ((uint)mii.EyebrowColor & 0x07) << 13;
        eyebrowData |= ((uint)mii.EyebrowSize & 0x0F) << 9;
        eyebrowData |= ((uint)mii.EyebrowVertical & 0x1F) << 4;
        eyebrowData |= ((uint)mii.EyebrowSpacing & 0x0F);
        data[0x24] = (byte)(eyebrowData >> 24);
        data[0x25] = (byte)(eyebrowData >> 16);
        data[0x26] = (byte)(eyebrowData >> 8);
        data[0x27] = (byte)(eyebrowData);

        // Eyes (0x28 - 0x2B)
        uint eyeData = 0;
        eyeData |= ((uint)mii.EyeType & 0x3F) << 26;
        // Skip 2 unknown bits
        eyeData |= ((uint)mii.EyeRotation & 0x07) << 21;
        eyeData |= ((uint)mii.EyeVertical & 0x1F) << 16;
        eyeData |= ((uint)mii.EyeColor & 0x07) << 13;
        // Skip 1 unknown bit
        eyeData |= ((uint)mii.EyeSize & 0x07) << 9;
        eyeData |= ((uint)mii.EyeSpacing & 0x0F) << 5;
        // Skip 5 unknown bits
        data[0x28] = (byte)(eyeData >> 24);
        data[0x29] = (byte)(eyeData >> 16);
        data[0x2A] = (byte)(eyeData >> 8);
        data[0x2B] = (byte)(eyeData);

        // Nose (0x2C - 0x2D)
        ushort noseData = 0;
        noseData |= (ushort)((mii.NoseType & 0x0F) << 12);
        noseData |= (ushort)((mii.NoseSize & 0x0F) << 8);
        noseData |= (ushort)((mii.NoseVertical & 0x1F) << 3);
        // Lower 3 bits remain 0.
        data[0x2C] = (byte)(noseData >> 8);
        data[0x2D] = (byte)(noseData & 0xFF);

        // Lips (0x2E - 0x2F)
        ushort lipData = 0;
        lipData |= (ushort)((mii.LipType & 0x1F) << 11);
        lipData |= (ushort)((mii.LipColor & 0x03) << 9);
        lipData |= (ushort)((mii.LipSize & 0x0F) << 5);
        lipData |= (ushort)(mii.LipVertical & 0x1F);
        data[0x2E] = (byte)(lipData >> 8);
        data[0x2F] = (byte)(lipData & 0xFF);

        // Glasses (0x30 - 0x31)
        ushort glassesData = 0;
        glassesData |= (ushort)((mii.GlassesType & 0x0F) << 12);
        glassesData |= (ushort)((mii.GlassesColor & 0x07) << 9);
        // Skip 1 unknown bit
        glassesData |= (ushort)((mii.GlassesSize & 0x07) << 5);
        glassesData |= (ushort)(mii.GlassesVertical & 0x1F);
        data[0x30] = (byte)(glassesData >> 8);
        data[0x31] = (byte)(glassesData & 0xFF);

        // Facial Hair (0x32 - 0x33)
        ushort facialHairData = 0;
        facialHairData |= (ushort)((mii.MustacheType & 0x03) << 14);
        facialHairData |= (ushort)((mii.BeardType & 0x03) << 12);
        facialHairData |= (ushort)((mii.FacialHairColor & 0x07) << 9);
        facialHairData |= (ushort)((mii.MustacheSize & 0x0F) << 5);
        facialHairData |= (ushort)(mii.MustacheVertical & 0x1F);
        data[0x32] = (byte)(facialHairData >> 8);
        data[0x33] = (byte)(facialHairData & 0xFF);

        // Mole (0x34 - 0x35)
        ushort moleData = 0;
        moleData |= (ushort)((mii.HasMole ? 1 : 0) << 15);
        moleData |= (ushort)((mii.MoleSize & 0x0F) << 11);
        moleData |= (ushort)((mii.MoleVertical & 0x1F) << 6);
        moleData |= (ushort)((mii.MoleHorizontal & 0x1F) << 1);
        // Lowest bit remains 0.
        data[0x34] = (byte)(moleData >> 8);
        data[0x35] = (byte)(moleData & 0xFF);

        // Creator Name (0x36 - 0x49)
        var fixedCreatorName = mii.CreatorName.Length > 10 ? mii.CreatorName.Substring(0, 10) : mii.CreatorName.PadRight(10, '\0');
        var creatorBytes = Encoding.BigEndianUnicode.GetBytes(fixedCreatorName);
        Array.Copy(creatorBytes, 0, data, 0x36, Math.Min(creatorBytes.Length, 20));

        return data;
    }
}
