using WheelWizard.WiiManagement.Domain.Enums;

namespace WheelWizard.WiiManagement;

public static class MiiSerializer
{
    public const int MiiBlockSize = 74;
    public static OperationResult<byte[]> Serialize(FullMii? mii)
    {
        if (mii == null)
            return Fail<byte[]>("Mii cannot be null.");
        byte[] data = new byte[MiiBlockSize];

        // Header (0x00 - 0x01)
        ushort header = 0;
        if (mii.IsInvalid) header |= 0x8000;
        if (mii.IsGirl) header |= 0x4000;
        header |= (ushort)((mii.Date.Month & 0x0F) << 10);
        header |= (ushort)((mii.Date.Day & 0x1F) << 5);
        header |= (ushort)(((int)mii.MiiFavoriteColor & 0x0F) << 1);
        if (mii.IsFavorite) header |= 0x1;
        data[0] = (byte)(header >> 8);
        data[1] = (byte)(header & 0xFF);

        // Name (0x02 - 0x15)
        Buffer.BlockCopy(mii.Name.ToBytes(), 0, data, 2, 20);

        // Height & Weight (0x16 - 0x17)
        data[0x16] = mii.Height.Value;
        data[0x17] = mii.Weight.Value;

        // Mii ID (0x18 - 0x1B)
        BitConverter.GetBytes(mii.MiiId).CopyTo(data, 0x18);

        // System ID (0x1C - 0x1F)
        data[0x1C] = mii.SystemId0;
        data[0x1D] = mii.SystemId1;
        data[0x1E] = mii.SystemId2;
        data[0x1F] = mii.SystemId3;

        // Face (0x20 - 0x21)
        ushort face = 0;
        face |= (ushort)((mii.MiiFacial.FaceShape & 0x07) << 13);
        face |= (ushort)((mii.MiiFacial.SkinColor & 0x07) << 10);
        face |= (ushort)((mii.MiiFacial.FacialFeature & 0x0F) << 6);
        face |= (ushort)((mii.MiiFacial.MingleOff ? 1 : 0) << 2);
        face |= (ushort)((mii.MiiFacial.Downloaded ? 1 : 0));
        data[0x20] = (byte)(face >> 8);
        data[0x21] = (byte)(face & 0xFF);

        // Hair (0x22 - 0x23)
        ushort hair = 0;
        hair |= (ushort)((mii.MiiHair.HairType & 0x7F) << 9);
        hair |= (ushort)((mii.MiiHair.HairColor & 0x07) << 6);
        hair |= (ushort)((mii.MiiHair.HairFlipped ? 1 : 0) << 5);
        data[0x22] = (byte)(hair >> 8);
        data[0x23] = (byte)(hair & 0xFF);

        // Eyebrows (0x24 - 0x27)
        uint brow = 0;
        brow |= (uint)(mii.MiiEyebrows.Type & 0x1F) << 27;
        brow |= (uint)(mii.MiiEyebrows.Rotation & 0x0F) << 22;
        brow |= (uint)(mii.MiiEyebrows.Color & 0x07) << 13;
        brow |= (uint)(mii.MiiEyebrows.Size & 0x0F) << 9;
        brow |= (uint)(mii.MiiEyebrows.Vertical & 0x1F) << 4;
        brow |= (uint)(mii.MiiEyebrows.Spacing & 0x0F);
        data[0x24] = (byte)(brow >> 24);
        data[0x25] = (byte)(brow >> 16);
        data[0x26] = (byte)(brow >> 8);
        data[0x27] = (byte)(brow);

        // Eyes (0x28 - 0x2B)
        uint eye = 0;
        eye |= (uint)(mii.MiiEyes.Type & 0x3F) << 26;
        eye |= (uint)(mii.MiiEyes.Rotation & 0x07) << 21;
        eye |= (uint)(mii.MiiEyes.Vertical & 0x1F) << 16;
        eye |= (uint)(mii.MiiEyes.Color & 0x07) << 13;
        eye |= (uint)(mii.MiiEyes.Size & 0x07) << 9;
        eye |= (uint)(mii.MiiEyes.Spacing & 0x0F) << 5;
        data[0x28] = (byte)(eye >> 24);
        data[0x29] = (byte)(eye >> 16);
        data[0x2A] = (byte)(eye >> 8);
        data[0x2B] = (byte)(eye);

        // Nose (0x2C - 0x2D)
        ushort nose = 0;
        nose |= (ushort)((mii.MiiNose.Type & 0x0F) << 12);
        nose |= (ushort)((mii.MiiNose.Size & 0x0F) << 8);
        nose |= (ushort)((mii.MiiNose.Vertical & 0x1F) << 3);
        data[0x2C] = (byte)(nose >> 8);
        data[0x2D] = (byte)(nose & 0xFF);

        // Lips (0x2E - 0x2F)
        ushort lip = 0;
        lip |= (ushort)((mii.MiiLips.Type & 0x1F) << 11);
        lip |= (ushort)((mii.MiiLips.Color & 0x03) << 9);
        lip |= (ushort)((mii.MiiLips.Size & 0x0F) << 5);
        lip |= (ushort)((mii.MiiLips.Vertical & 0x1F));
        data[0x2E] = (byte)(lip >> 8);
        data[0x2F] = (byte)(lip & 0xFF);

        // Glasses (0x30 - 0x31)
        ushort glasses = 0;
        glasses |= (ushort)((mii.MiiGlasses.Type & 0x0F) << 12);
        glasses |= (ushort)((mii.MiiGlasses.Color & 0x07) << 9);
        glasses |= (ushort)((mii.MiiGlasses.Size & 0x07) << 5);
        glasses |= (ushort)((mii.MiiGlasses.Vertical & 0x1F));
        data[0x30] = (byte)(glasses >> 8);
        data[0x31] = (byte)(glasses & 0xFF);

        // Facial hair (0x32 - 0x33)
        ushort facialHair = 0;
        facialHair |= (ushort)((mii.MiiFacialHair.MustacheType & 0x03) << 14);
        facialHair |= (ushort)((mii.MiiFacialHair.BeardType & 0x03) << 12);
        facialHair |= (ushort)((mii.MiiFacialHair.Color & 0x07) << 9);
        facialHair |= (ushort)((mii.MiiFacialHair.Size & 0x0F) << 5);
        facialHair |= (ushort)((mii.MiiFacialHair.Vertical & 0x1F));
        data[0x32] = (byte)(facialHair >> 8);
        data[0x33] = (byte)(facialHair & 0xFF);

        // Mole (0x34 - 0x35)
        ushort mole = 0;
        mole |= (ushort)((mii.MiiMole.Exists ? 1 : 0) << 15);
        mole |= (ushort)((mii.MiiMole.Size & 0x0F) << 11);
        mole |= (ushort)((mii.MiiMole.Vertical & 0x1F) << 6);
        mole |= (ushort)((mii.MiiMole.Horizontal & 0x1F) << 1);
        data[0x34] = (byte)(mole >> 8);
        data[0x35] = (byte)(mole & 0xFF);

        // Creator Name (0x36 - 0x49)
        Buffer.BlockCopy(mii.CreatorName.ToBytes(), 0, data, 0x36, 20);

        return data;
    }
    
    public static OperationResult<FullMii> Deserialize(byte[]? data)
    {
        if (data == null || data.Length != 74)
            return Fail<FullMii>("Invalid Mii data length.");

        var mii = new FullMii();

        // Header (0x00 - 0x01)
        ushort header = (ushort)((data[0] << 8) | data[1]);
        mii.IsInvalid = (header & 0x8000) != 0;
        mii.IsGirl = (header & 0x4000) != 0;
        int month = (header >> 10) & 0x0F;
        int day = (header >> 5) & 0x1F;
        mii.Date = new DateOnly(2000, Math.Clamp(month, 1, 12), Math.Clamp(day, 1, 31));
        mii.MiiFavoriteColor = (MiiFavoriteColor)((header >> 1) & 0x0F);
        mii.IsFavorite = (header & 0x01) != 0;

        // Name (0x02 - 0x15)
        mii.Name = MiiName.FromBytes(data, 2);

        // Height & Weight (0x16 - 0x17)
        mii.Height = MiiScale.Create(data[0x16]).Value;
        mii.Weight = MiiScale.Create(data[0x17]).Value;

        // Mii ID (0x18 - 0x1B)
        mii.MiiId = BitConverter.ToUInt32(data, 0x18);

        // System ID (0x1C - 0x1F)
        mii.SystemId0 = data[0x1C];
        mii.SystemId1 = data[0x1D];
        mii.SystemId2 = data[0x1E];
        mii.SystemId3 = data[0x1F];

        // Face (0x20 - 0x21)
        ushort face = (ushort)((data[0x20] << 8) | data[0x21]);
        var MiiFacialResult = MiiFacialFeatures.Create(
            (face >> 13) & 0x07,
            (face >> 10) & 0x07,
            (face >> 6) & 0x0F,
            ((face >> 2) & 0x01) != 0,
            (face & 0x01) != 0
        );
        if (MiiFacialResult.IsFailure)
            return MiiFacialResult.Error;
        mii.MiiFacial = MiiFacialResult.Value;

        // Hair (0x22 - 0x23)
        ushort hair = (ushort)((data[0x22] << 8) | data[0x23]);
        var miiHairResult = MiiHair.Create(
            (hair >> 9) & 0x7F,
            (hair >> 6) & 0x07,
            ((hair >> 5) & 0x01) != 0
        );
        if (miiHairResult.IsFailure)
            return miiHairResult.Error;
        mii.MiiHair = miiHairResult.Value;

        // Eyebrows (0x24 - 0x27)
        uint brow = (uint)((data[0x24] << 24) | (data[0x25] << 16) | (data[0x26] << 8) | data[0x27]);
        var miiEyebrowsResult = MiiEyebrow.Create(
            (int)((brow >> 27) & 0x1F),
            (int)((brow >> 22) & 0x0F),
            (int)((brow >> 13) & 0x07),
            (int)((brow >> 9) & 0x0F),
            (int)((brow >> 4) & 0x1F),
            (int)(brow & 0x0F)
        );
        if (miiEyebrowsResult.IsFailure)
            return miiEyebrowsResult.Error;
        mii.MiiEyebrows = miiEyebrowsResult.Value;

        // Eyes (0x28 - 0x2B)
        uint eye = (uint)((data[0x28] << 24) | (data[0x29] << 16) | (data[0x2A] << 8) | data[0x2B]);
        var miiEyesResult = MiiEye.Create(
            (int)((eye >> 26) & 0x3F),
            (int)((eye >> 21) & 0x07),
            (int)((eye >> 16) & 0x1F),
            (int)((eye >> 13) & 0x07),
            (int)((eye >> 9) & 0x07),
            (int)((eye >> 5) & 0x0F)
        );
        if (miiEyesResult.IsFailure)
            return miiEyesResult.Error;
        mii.MiiEyes = miiEyesResult.Value;
        
        // Nose (0x2C - 0x2D)
        ushort nose = (ushort)((data[0x2C] << 8) | data[0x2D]);
        var miiNoseResult = MiiNose.Create(
            (int)((nose >> 12) & 0x0F),
            (int)((nose >> 8) & 0x0F),
            (int)((nose >> 3) & 0x1F)
        );
        if (miiNoseResult.IsFailure)
            return miiNoseResult.Error;
        mii.MiiNose = miiNoseResult.Value;

        // Lips (0x2E - 0x2F)
        ushort lip = (ushort)((data[0x2E] << 8) | data[0x2F]);
        var miiLipResult = MiiLip.Create(
            (int)((lip >> 11) & 0x1F),
            (int)((lip >> 9) & 0x03),
            (int)((lip >> 5) & 0x0F),
            (int)(lip & 0x1F)
        );
        if (miiLipResult.IsFailure)
            return miiLipResult.Error;
        mii.MiiLips = miiLipResult.Value;

        // Glasses (0x30 - 0x31)
        ushort glasses = (ushort)((data[0x30] << 8) | data[0x31]);
        var miiGlassesResult = MiiGlasses.Create(
            (int)((glasses >> 12) & 0x0F),
            (int)((glasses >> 9) & 0x07),
            (int)((glasses >> 5) & 0x07),
            (int)(glasses & 0x1F)
        );
        if (miiGlassesResult.IsFailure)
            return miiGlassesResult.Error;
        mii.MiiGlasses = miiGlassesResult.Value;

        // Facial hair (0x32 - 0x33)
        ushort facial = (ushort)((data[0x32] << 8) | data[0x33]);
        
        var miiFacialHairResult = MiiFacialHair.Create(
            (int)((facial >> 14) & 0x03),
            (int)((facial >> 12) & 0x03),
            (int)((facial >> 9) & 0x07),
            (int)((facial >> 5) & 0x0F),
            (int)(facial & 0x1F)
        );
        if (miiFacialHairResult.IsFailure)
            return miiFacialHairResult.Error;
        mii.MiiFacialHair = miiFacialHairResult.Value;

        // Mole (0x34 - 0x35)
        ushort mole = (ushort)((data[0x34] << 8) | data[0x35]);
        var miiMoleResult = MiiMole.Create(
            ((mole >> 15) & 0x01) != 0,
            (mole >> 11) & 0x0F,
            (mole >> 6) & 0x1F,
            (mole >> 1) & 0x1F
        );
        if (miiMoleResult.IsFailure)
            return miiMoleResult.Error;
        mii.MiiMole = miiMoleResult.Value;
        
        // Creator Name (0x36 - 0x49)
        mii.CreatorName = MiiName.FromBytes(data, 0x36);

        return mii;
    }
}
