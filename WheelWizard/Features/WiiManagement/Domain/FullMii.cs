using WheelWizard.Models.MiiImages;
using WheelWizard.WiiManagement.Domain.Enums;

namespace WheelWizard.WiiManagement.Domain;

public class FullMii
{
    
    private Dictionary<MiiImageVariants.Variant, MiiImage> images = new ();

    public MiiImage GetImage(MiiImageVariants.Variant variant)
    {
        if (!images.ContainsKey(variant))
            images[variant] = new MiiImage(this, variant);
        return images[variant];
    }
    
    // Header (0x00 & 0x01)
    public bool IsInvalid { get; set; }
    public bool IsGirl { get; set; }
    
    public DateOnly Date { get; set; }
    public int FavoriteColor { get; set; }
    public bool IsFavorite { get; set; }

    // Name (0x02 - 0x15) – 10 UTF-16 characters
    public string Name { get; set; } = string.Empty;

    // Height & Weight (0x16 - 0x17)
    public byte Height { get; set; }            // 0 - 127
    public byte Weight { get; set; }            // 0 - 127

    // Mii ID (0x18 - 0x1B)
    public uint MiiId { get; set; }

    // System ID (0x1C - 0x1F)
    public byte SystemId0 { get; set; }
    public byte SystemId1 { get; set; }
    public byte SystemId2 { get; set; }
    public byte SystemId3 { get; set; }

    // Face and personality (0x20 - 0x21)
    public int FaceShape { get; set; }          // 3 bits (0-7)
    public int SkinColor { get; set; }          // 3 bits (0-5)
    public int FacialFeature { get; set; }      // 4 bits (0-11)
    public bool MingleOff { get; set; }         // 1 bit
    public bool Downloaded { get; set; }        // 1 bit

    // Hair (0x22 - 0x23)
    public int HairType { get; set; }           // 0 - 71
    public int HairColor { get; set; }          // 0 - 7
    public bool HairFlipped { get; set; }       // 1 = reversed part

    // Eyebrows (0x24 - 0x27)
    public int EyebrowType { get; set; }        // 0 - 23
    public int EyebrowRotation { get; set; }    // 0 - 11
    public int EyebrowColor { get; set; }       // 0 - 7
    public int EyebrowSize { get; set; }        // 0 - 8
    public int EyebrowVertical { get; set; }    // 3 - 18
    public int EyebrowSpacing { get; set; }     // 0 - 12

    // Eyes (0x28 - 0x2B)
    public int EyeType { get; set; }            // 0 - 47
    public int EyeRotation { get; set; }        // 0 - 7
    public int EyeVertical { get; set; }        // 0 - 18
    public int EyeColor { get; set; }           // 0 - 5
    public int EyeSize { get; set; }            // 0 - 7
    public int EyeSpacing { get; set; }         // 0 - 12

    // Nose (0x2C - 0x2D)
    public int NoseType { get; set; }           // 0 - 11
    public int NoseSize { get; set; }           // 0 - 8
    public int NoseVertical { get; set; }       // 0 - 18

    // Lips (0x2E - 0x2F)
    public int LipType { get; set; }            // 0 - 23
    public int LipColor { get; set; }           // 0 - 2
    public int LipSize { get; set; }            // 0 - 8
    public int LipVertical { get; set; }        // 0 - 18

    // Glasses (0x30 - 0x31)
    public int GlassesType { get; set; }        // 0 - 8
    public int GlassesColor { get; set; }       // 0 - 5
    public int GlassesSize { get; set; }        // 0 - 7
    public int GlassesVertical { get; set; }    // 0 - 20

    // Facial Hair (0x32 - 0x33)
    public int MustacheType { get; set; }       // 0 - 3
    public int BeardType { get; set; }          // 0 - 3
    public int FacialHairColor { get; set; }    // 0 - 7
    public int MustacheSize { get; set; }       // 0 - 8
    public int MustacheVertical { get; set; }   // 0 - 16

    // Mole (0x34 - 0x35)
    public bool HasMole { get; set; }           // 0 = no, 1 = yes
    public int MoleSize { get; set; }           // 0 - 8
    public int MoleVertical { get; set; }       // 0 - 30
    public int MoleHorizontal { get; set; }     // 0 - 16

    // Creator name (0x36 - 0x49) – 10 UTF-16 characters
    public string CreatorName { get; set; } = string.Empty;
}
