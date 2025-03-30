using WheelWizard.Models.MiiImages;
using WheelWizard.WiiManagement.Domain.Enums;

public class FullMii
{
    
    private Dictionary<MiiImageVariants.Variant, MiiImage> images = new ();

    public MiiImage GetImage(MiiImageVariants.Variant variant)
    {
        if (!images.ContainsKey(variant))
            images[variant] = new MiiImage(this, variant);
        return images[variant];
    }
    public bool IsInvalid { get; set; }
    public bool IsGirl { get; set; }
    public DateOnly Date { get; set; } = new(2000, 1, 1);
    public MiiFavoriteColor MiiFavoriteColor { get; set; }
    public bool IsFavorite { get; set; }

    public MiiName Name { get; set; } = MiiName.Create("no name").Value;
    public MiiScale Height { get; set; }
    public MiiScale Weight { get; set; }

    public uint MiiId { get; set; }
    public byte SystemId0 { get; set; }
    public byte SystemId1 { get; set; }
    public byte SystemId2 { get; set; }
    public byte SystemId3 { get; set; }

    public MiiFacialFeatures MiiFacial { get; set; }
    public MiiHair MiiHair { get; set; }
    public MiiEyebrow MiiEyebrows { get; set; }
    public MiiEye MiiEyes { get; set; }
    public MiiNose MiiNose { get; set; }
    public MiiLip MiiLips { get; set; }
    public MiiGlasses MiiGlasses { get; set; }
    public MiiFacialHair MiiFacialHair { get; set; }
    public MiiMole MiiMole { get; set; }

    public MiiName CreatorName { get; set; } = MiiName.Create("no name").Value!;
    
}
