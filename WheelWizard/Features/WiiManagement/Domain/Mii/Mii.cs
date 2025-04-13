using WheelWizard.Models.MiiImages;

namespace WheelWizard.WiiManagement.Domain.Mii;

public class Mii
{
    //todo: Remove images out of class
    private readonly Dictionary<MiiImageVariants.Variant, MiiImage> _images = new();

    public MiiImage GetImage(MiiImageVariants.Variant variant)
    {
        if (!_images.ContainsKey(variant))
            _images[variant] = new(this, variant);
        return _images[variant];
    }

    public bool IsInvalid { get; set; }
    public bool IsGirl { get; set; }
    public DateOnly Date { get; set; } = new(2000, 1, 1);
    public MiiFavoriteColor MiiFavoriteColor { get; set; } = MiiFavoriteColor.Black;
    public bool IsFavorite { get; set; }

    public MiiName Name { get; set; } = new("no name");
    public MiiScale Height { get; set; } = new(1);
    public MiiScale Weight { get; set; } = new(1);

    public uint MiiId { get; set; }
    public byte SystemId0 { get; set; }
    public byte SystemId1 { get; set; }
    public byte SystemId2 { get; set; }
    public byte SystemId3 { get; set; }

    public MiiFacialFeatures MiiFacial { get; set; } = new(MiiFaceShape.Bread, MiiSkinColor.Light, MiiFacialFeature.None, false, false);

    public MiiHair MiiHair { get; set; } = new(1, HairColor.Black, false);
    public MiiEyebrow MiiEyebrows { get; set; } = new(1, 0, EyebrowColor.Black, 4, 10, 1);
    public MiiEye MiiEyes { get; set; } = new(1, 6, 7, EyeColor.Black, 3, 6);
    public MiiNose MiiNose { get; set; } = new(NoseType.Default, 6, 4);
    public MiiLip MiiLips { get; set; } = new(1, LipColor.Skin, 4, 9);
    public MiiGlasses MiiGlasses { get; set; } = new(GlassesType.None, GlassesColor.Dark, 4, 1);
    public MiiFacialHair MiiFacialHair { get; set; } = new(MustacheType.None, BeardType.None, MustacheColor.Black, 1, 1);
    public MiiMole MiiMole { get; set; } = new(false, 0, 0, 0);
    public MiiName CreatorName { get; set; } = new("no name");
}
