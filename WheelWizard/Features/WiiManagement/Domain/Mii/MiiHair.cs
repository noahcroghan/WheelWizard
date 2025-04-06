namespace WheelWizard.WiiManagement.Domain.Mii;

public class MiiHair
{
    public int HairType { get; }
    public HairColor HairColor { get; }
    public bool HairFlipped { get; }

    public MiiHair(int hairType, HairColor hairColor, bool hairFlipped)
    {
        if (hairType is < 0 or > 71)
            throw new ArgumentException("HairType out of range");
        HairType = hairType;
        HairColor = hairColor;
        HairFlipped = hairFlipped;
    }

    public static OperationResult<MiiHair> Create(int hairType, HairColor hairColor, bool hairFlipped) => TryCatch(()
        => new MiiHair(hairType, hairColor, hairFlipped));
}
