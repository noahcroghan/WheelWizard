using System.Text;

namespace WheelWizard.WiiManagement.Domain.Enums;

public enum MiiFavoriteColor { Red, Orange, Yellow, Green, Blue, LightBlue, Pink, Purple, Brown, White, Black, Gray }
public enum MiiFaceShape{RoundPointChin, Circle, Oval, BlobFatChin, RightAnglePointChin, Bread, Octagon, Square}
public enum MiiSkinColor{ Light, LightTan, Tan, Pink, DarkBrown, Brown}
public enum FacialFeature {None, Cheeks, CheekAndEyes, Freckles, BaggyEyes, Chad, Tired, Chin, EyeShadow, Beard, MouthCorners, Old}
public enum HairColor {Black, Brown, Red, LightRed, Grey, LightBrown, Blonde, White}
public enum EyebrowColor {Black, Brown, Red, LightRed, Grey, LightBrown, Blonde, White}
public enum EyeColor {Black, Grey, Red, Gold, Blue, Green}
public enum NoseType {Default, SemiCircle, Dots, VShape, FullNose, Triangle, FlatC, UpsideDownC, Squidward, ArrowDown, Flat, Tunnel}
public enum LipColor {Skin, Red, Pink}
public enum GlassesColor {Dark, DarkGold, Red, Blue, Gold, White}
public enum GlasseStype {None, Square, Rectangle, Circle, Oval, Misses, SadSunGlasses, SunGlasses, CoolSunGlasses}
public enum StachColor {Black, Brown, Red, LightRed, Grey, LightBrown, Blonde, White}
public enum StachType {None, Fat, Thin, Goatee}
public enum BeardType {None, Thin, Wide, Widest}


/// <summary>
/// Represents a Mii name.
/// </summary>
public class MiiName
{
    private readonly string _value;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MiiName"/> class with the specified value.
    /// </summary>
    /// <param name="value">The Mii name value.</param>
    /// <exception cref="ArgumentException">Mii name cannot be empty or longer than 10 characters.</exception>
    public MiiName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) 
            throw new ArgumentException("Mii name cannot be empty");
        
        if (value.Length > 10) 
            throw new ArgumentException("Mii name too long, maximum is 10 characters");
        _value = value;
    }
    
    /// <summary>
    /// Creates a new instance of the <see cref="MiiName"/> class with the specified value.
    /// </summary>
    /// <param name="value">The Mii name value.</param>
    /// <returns>An <see cref="OperationResult{MiiName}"/> representing the result of the operation.</returns>
    public static OperationResult<MiiName> Create(string value) => TryCatch(() => new MiiName(value));
    
    public byte[] ToBytes() => Encoding.BigEndianUnicode.GetBytes(_value.PadRight(10, '\0'));
    public static MiiName FromBytes(byte[] data, int offset) =>
        new(Encoding.BigEndianUnicode.GetString(data, offset, 20).TrimEnd('\0'));
    public override string ToString() => _value;
}


public class MiiScale
{
    public byte Value { get; }
    public MiiScale(byte value)
    {
        if (value > 127)
            throw new ArgumentException("Scale must be between 0 and 127.");
        Value = value;
    }
    public static OperationResult<MiiScale> Create(byte value) => TryCatch(() => new MiiScale(value));
}

public record MiiFacialFeatures(
    MiiFaceShape FaceShape,
    MiiSkinColor SkinColor,
    FacialFeature FacialFeature,
    bool MingleOff,
    bool Downloaded
);

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


public class MiiEyebrow
{
    public int Type { get; }
    public int Rotation { get; }
    public EyebrowColor  Color { get; }
    public int Size { get; }
    public int Vertical { get; }
    public int Spacing { get; }

    public MiiEyebrow(int type, int rotation, EyebrowColor color, int size, int vertical, int spacing)
    {
        if (type is < 0 or > 23)
            throw new ArgumentException("Eyebrow type invalid");
        if (rotation is < 0 or > 11)
            throw new ArgumentException("Rotation invalid");
        if (size is < 0 or > 8)
            throw new ArgumentException("Size invalid");
        if (vertical is < 0 or > 18)
            throw new ArgumentException("Vertical position invalid");
        if (spacing is < 0 or > 12)
            throw new ArgumentException("Spacing invalid");
        Type = type;
        Rotation = rotation;
        Color = color;
        Size = size;
        Vertical = vertical;
        Spacing = spacing;
    }

    public static OperationResult<MiiEyebrow> Create(int type, int rotation, EyebrowColor color, int size, int vertical, int spacing)
    => TryCatch(() => new MiiEyebrow(type, rotation, color, size, vertical, spacing));
}


public class MiiEye
{
    public int Type { get; }
    public int Rotation { get; }
    public int Vertical { get; }
    public EyeColor Color { get; }
    public int Size { get; }
    public int Spacing { get; }

    public MiiEye(int type, int rotation, int vertical, EyeColor color, int size, int spacing)
    {
        if (type is < 0 or > 47)        throw new ArgumentException("Eye type invalid");
        if (rotation is < 0 or > 7)     throw new ArgumentException("Rotation invalid");
        if (vertical is < 0 or > 18)    throw new ArgumentException("Vertical position invalid");
        if (size is < 0 or > 7)         throw new ArgumentException("Size invalid");
        if (spacing is < 0 or > 12)     throw new ArgumentException("Spacing invalid");
        Type = type;
        Rotation = rotation;
        Vertical = vertical;
        Color = color;
        Size = size;
        Spacing = spacing;
    }

    public static OperationResult<MiiEye> Create(int type, int rotation, int vertical, EyeColor color, int size, int spacing)
    => TryCatch(() => new MiiEye(type, rotation, vertical, color, size, spacing));
}

public class MiiNose
{
    public NoseType Type { get; }
    public int Size { get; }
    public int Vertical { get; }

    public MiiNose(NoseType type, int size, int vertical)
    {
        if (size is < 0 or > 8)         throw new ArgumentException("Nose size invalid");
        if (vertical is < 0 or > 18)    throw new ArgumentException("Nose vertical position invalid");
        Type = type;
        Size = size;
        Vertical = vertical;
    }

    public static OperationResult<MiiNose> Create(NoseType type, int size, int vertical)
    => TryCatch(() => new MiiNose(type, size, vertical));
}


public class MiiLip
{
    public int Type { get; }
    public LipColor Color { get; }
    public int Size { get; }
    public int Vertical { get; }

    public MiiLip(int type, LipColor color, int size, int vertical)
    {
        if (type is < 0 or > 23)        throw new ArgumentException("Lip type invalid");
        if (size is < 0 or > 8)         throw new ArgumentException("Lip size invalid");
        if (vertical is < 0 or > 18)    throw new ArgumentException("Lip vertical position invalid");
        
        Type = type;
        Color = color;
        Size = size;
        Vertical = vertical;
    }

    public static OperationResult<MiiLip> Create(int type, LipColor color, int size, int vertical)
    => TryCatch(() => new MiiLip(type, color, size, vertical));
}


public class MiiGlasses
{
    public GlasseStype  Type { get; }
    public GlassesColor Color { get; }
    public int Size { get; }
    public int Vertical { get; }

    public MiiGlasses(GlasseStype type, GlassesColor color, int size, int vertical)
    {
        if (size is < 0 or > 7)             throw new ArgumentException("Glasses size invalid");
        if (vertical is < 0 or > 20)        throw new ArgumentException("Glasses vertical position invalid");
        Type = type;
        Color = color;
        Size = size;
        Vertical = vertical;
    }

    public static OperationResult<MiiGlasses> Create(GlasseStype type, GlassesColor color, int size, int vertical)
    => TryCatch(() => new MiiGlasses(type, color, size, vertical));
}
public class MiiFacialHair
{
    public StachType MustacheType { get; }
    public BeardType BeardType { get; }
    public StachColor Color { get; }
    public int Size { get; }
    public int Vertical { get; }

    public MiiFacialHair(StachType mustacheType, BeardType beardType, StachColor color, int size, int vertical)
    {
        if (size is < 0 or > 8)         throw new ArgumentException("Facial hair size invalid");
        if (vertical is < 0 or > 16)    throw new ArgumentException("Facial hair vertical position invalid");
        MustacheType = mustacheType;
        BeardType = beardType;
        Color = color;
        Size = size;
        Vertical = vertical;
    }

    public static OperationResult<MiiFacialHair> Create(StachType mustacheType, BeardType beardType, StachColor color, int size, int vertical)
    => TryCatch(() => new MiiFacialHair(mustacheType, beardType, color, size, vertical));
}

public class MiiMole
{
    public bool Exists { get; }
    public int Size { get; }
    public int Vertical { get; }
    public int Horizontal { get; }

    public MiiMole(bool exists, int size, int vertical, int horizontal)
    {
        if (size is < 0 or > 8)             throw new ArgumentException("Mole size invalid");
        if (vertical is < 0 or > 30)        throw new ArgumentException("Mole vertical position invalid");
        if (horizontal is < 0 or > 16)      throw new ArgumentException("Mole horizontal position invalid");
        Exists = exists;
        Size = size;
        Vertical = vertical;
        Horizontal = horizontal;
    }
    
    public static OperationResult<MiiMole> Create(bool exists, int size, int vertical, int horizontal)
    => TryCatch(() => new MiiMole(exists, size, vertical, horizontal));
}
