using Avalonia.Media;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.WiiManagement.Domain;

public static class MiiColorMappings
{
    public static readonly Dictionary<MiiFavoriteColor, Color> FavoriteColor = new()
    {
        [MiiFavoriteColor.Black] = Colors.Black,
        [MiiFavoriteColor.Yellow] = Color.FromRgb(255, 237, 33),
        [MiiFavoriteColor.Red] = Color.FromRgb(252, 33, 20),
        [MiiFavoriteColor.Pink] = Color.FromRgb(255, 98, 126),
        [MiiFavoriteColor.Blue] = Color.FromRgb(10, 80, 184),
        [MiiFavoriteColor.Green] = Color.FromRgb(0, 130, 50),
        [MiiFavoriteColor.Orange] = Color.FromRgb(255, 119, 27),
        [MiiFavoriteColor.Purple] = Color.FromRgb(138, 42, 176),
        [MiiFavoriteColor.LightBlue] = Color.FromRgb(71, 186, 225),
        [MiiFavoriteColor.LightGreen] = Color.FromRgb(143, 240, 31),
        [MiiFavoriteColor.Brown] = Color.FromRgb(87, 62, 23),
        [MiiFavoriteColor.White] = Color.FromRgb(255, 255, 250),
    };

    public static readonly Dictionary<MiiSkinColor, Color> SkinColor = new()
    {
        [MiiSkinColor.Light] = Color.FromRgb(255, 211, 157),
        [MiiSkinColor.Yellow] = Color.FromRgb(255, 185, 99),
        [MiiSkinColor.Red] = Color.FromRgb(222, 123, 61),
        [MiiSkinColor.Pink] = Color.FromRgb(255, 171, 128),
        [MiiSkinColor.DarkBrown] = Color.FromRgb(200, 83, 39),
        [MiiSkinColor.Brown] = Color.FromRgb(117, 46, 23),
    };

    public static readonly Dictionary<MiiHairColor, Color> HairColor = new()
    {
        [MiiHairColor.Black] = Colors.Black,
        [MiiHairColor.Brown] = Color.FromRgb(86, 45, 27),
        [MiiHairColor.Red] = Color.FromRgb(120, 37, 21),
        [MiiHairColor.LightRed] = Color.FromRgb(157, 74, 32),
        [MiiHairColor.Grey] = Color.FromRgb(152, 139, 140),
        [MiiHairColor.LightBrown] = Color.FromRgb(104, 78, 27),
        [MiiHairColor.Blonde] = Color.FromRgb(171, 106, 36),
        [MiiHairColor.Gold] = Color.FromRgb(255, 183, 87),
    };
}
