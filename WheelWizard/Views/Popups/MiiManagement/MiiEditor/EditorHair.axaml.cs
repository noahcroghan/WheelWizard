using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using WheelWizard.Views.Components;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorHair : MiiEditorBaseControl
{
    public EditorHair(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        if (Editor?.Mii?.MiiHair == null)
            return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        var currentHair = Editor.Mii.MiiHair;
        GenerateHairButtons();

        HairColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(HairColor)))
        {
            HairColorBox.Items.Add(color);
            if (color == currentHair.HairColor.ToString())
                HairColorBox.SelectedItem = color;
        }

        HairFlippedCheck.IsChecked = currentHair.HairFlipped;
    }

    private void GenerateHairButtons()
    {
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral100); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(ViewUtils.Colors.Black); // Hair Color
        var color4 = new SolidColorBrush(ViewUtils.Colors.Primary800); // Hat main color
        var color5 = new SolidColorBrush(ViewUtils.Colors.Primary900); // Hat accent color
        SetButtons(
            "MiiHair",
            71,
            HairTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == Editor.Mii.MiiHair.HairType;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.Color5 = color5;
                button.Click += (_, _) => SetHairType(index);
            }
        );
    }

    private void SetHairType(int type)
    {
        Editor.Mii.MiiHair = new(type, Editor.Mii.MiiHair.HairColor, Editor.Mii.MiiHair.HairFlipped);
        Editor.Mii.ClearImages();
    }

    private void HairColorBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Editor?.Mii?.MiiHair == null || !IsLoaded || HairColorBox.SelectedItem == null)
            return;

        var value = HairColorBox.SelectedItem;
        if (value is null)
            return;

        var selectedColor = (HairColor)Enum.Parse(typeof(HairColor), value.ToString()!);
        var currentHair = Editor.Mii.MiiHair;
        if (selectedColor == currentHair.HairColor)
            return;

        var result = MiiHair.Create(currentHair.HairType, selectedColor, currentHair.HairFlipped);

        if (result.IsFailure)
        {
            return;
        }

        Editor.Mii.MiiHair = result.Value;
        Editor.Mii.ClearImages();
    }

    private void HairFlippedCheck_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (Editor?.Mii?.MiiHair == null || !IsLoaded)
            return;

        var isChecked = HairFlippedCheck.IsChecked == true;
        var currentHair = Editor.Mii.MiiHair;

        if (isChecked == currentHair.HairFlipped)
            return;

        var result = MiiHair.Create(currentHair.HairType, currentHair.HairColor, isChecked); // New value

        if (result.IsFailure)
        {
            return;
        }

        Editor.Mii.MiiHair = result.Value;
        Editor.Mii.ClearImages();
    }
}
