using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorHair : MiiEditorBaseControl
{
    private static readonly Dictionary<MiiHairColor, Color> ColorMap = new()
    {
        [MiiHairColor.Black] = Colors.Black,
        [MiiHairColor.Brown] = Color.FromRgb(86, 45, 27),
        [MiiHairColor.Red] = Color.FromRgb(120, 37, 21),
        [MiiHairColor.LightRed] = Color.FromRgb(157, 74, 32),
        [MiiHairColor.Grey] = Color.FromRgb(152, 139, 140),
        [MiiHairColor.LightBrown] = Color.FromRgb(104, 78, 27),
        [MiiHairColor.Blonde] = Color.FromRgb(171, 106, 36),
        [MiiHairColor.White] = Color.FromRgb(255, 183, 87),
    };

    public EditorHair(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        PopulateValues();
    }

    private void PopulateValues()
    {
        // Attribute:
        var currentHair = Editor.Mii.MiiHair;

        // Hair:
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral100); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(ViewUtils.Colors.Black); // Hair Color
        var color4 = new SolidColorBrush(ViewUtils.Colors.Primary800); // Hat main color
        var color5 = new SolidColorBrush(ViewUtils.Colors.Primary900); // Hat accent color
        SetButtons(
            "MiiHair",
            72,
            HairTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == currentHair.HairType;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.Color5 = color5;
                button.Click += (_, _) => SetHairType(index);
            }
        );

        // Hair Color:
        HairColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(MiiHairColor)))
        {
            HairColorBox.Items.Add(color);
            if (color == currentHair.MiiHairColor.ToString())
                HairColorBox.SelectedItem = color;
        }

        // Hair Flipped:
        HairFlippedCheck.IsChecked = currentHair.HairFlipped;
    }

    private void SetHairType(int type)
    {
        Editor.Mii.MiiHair = new(type, Editor.Mii.MiiHair.MiiHairColor, Editor.Mii.MiiHair.HairFlipped);
        Editor.RefreshImage();
    }

    private void HairColorBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Editor?.Mii?.MiiHair == null || !IsLoaded || HairColorBox.SelectedItem == null)
            return;

        var value = HairColorBox.SelectedItem;
        if (value is null)
            return;

        var selectedColor = (MiiHairColor)Enum.Parse(typeof(MiiHairColor), value.ToString()!);
        var currentHair = Editor.Mii.MiiHair;
        if (selectedColor == currentHair.MiiHairColor)
            return;

        var result = MiiHair.Create(currentHair.HairType, selectedColor, currentHair.HairFlipped);

        if (result.IsFailure)
            return;

        Editor.Mii.MiiHair = result.Value;
        Editor.RefreshImage();
    }

    private void HairFlippedCheck_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (Editor?.Mii?.MiiHair == null || !IsLoaded)
            return;

        var isChecked = HairFlippedCheck.IsChecked == true;
        var currentHair = Editor.Mii.MiiHair;

        if (isChecked == currentHair.HairFlipped)
            return;

        var result = MiiHair.Create(currentHair.HairType, currentHair.MiiHairColor, isChecked); // New value

        if (result.IsFailure)
            return;

        Editor.Mii.MiiHair = result.Value;
        Editor.RefreshImage();
    }
}
