namespace WheelWizard.WiiManagement.Domain.Mii.Custom;

/// <summary>
/// Enumeration representing the color of a Mii profile.
/// This only takes up 4 bits, so in total 15 colors are possible. (+1 for none)
/// </summary>
public enum MiiProfileColor : uint
{
    none = 0,
    Color1 = 1,
    Color2 = 2,
    Color3 = 3,
    Color4 = 4,
    Color5 = 5,
    Color6 = 6,
    Color7 = 7,
    Color8 = 8,
    Color9 = 9,
    Color10 = 10,
    Color11 = 11,
    Color12 = 12,
    Color13 = 13,
    Color14 = 14,
    Color15 = 15,
}
