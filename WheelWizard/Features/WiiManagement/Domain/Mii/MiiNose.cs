namespace WheelWizard.WiiManagement.Domain.Mii;

public class MiiNose
{
    public NoseType Type { get; }
    public int Size { get; }
    public int Vertical { get; }

    public MiiNose(NoseType type, int size, int vertical)
    {
        if (size is < 0 or > 8) throw new ArgumentException("Nose size invalid");
        if (vertical is < 0 or > 18) throw new ArgumentException("Nose vertical position invalid");
        Type = type;
        Size = size;
        Vertical = vertical;
    }

    public static OperationResult<MiiNose> Create(NoseType type, int size, int vertical)
        => TryCatch(() => new MiiNose(type, size, vertical));
}
