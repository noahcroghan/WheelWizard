namespace WheelWizard.WiiManagement.Domain.Mii;

public class MiiFacialHair
{
    public MustacheType MustacheType { get; }
    public BeardType BeardType { get; }
    public MustacheColor Color { get; }
    public int Size { get; }
    public int Vertical { get; }

    public MiiFacialHair(MustacheType mustacheType, BeardType beardType, MustacheColor color, int size, int vertical)
    {
        if (size is < 0 or > 8) throw new ArgumentException("Facial hair size invalid");
        if (vertical is < 0 or > 16) throw new ArgumentException("Facial hair vertical position invalid");
        MustacheType = mustacheType;
        BeardType = beardType;
        Color = color;
        Size = size;
        Vertical = vertical;
    }

    public static OperationResult<MiiFacialHair> Create(MustacheType mustacheType, BeardType beardType, MustacheColor color, int size,
        int vertical)
        => TryCatch(() => new MiiFacialHair(mustacheType, beardType, color, size, vertical));
}
