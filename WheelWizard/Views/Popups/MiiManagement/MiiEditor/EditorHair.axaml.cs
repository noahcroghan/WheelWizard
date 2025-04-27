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
        var Color1 = new SolidColorBrush(ViewUtils.Colors.Neutral50); // Skin Color
        var Color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var Color3 = new SolidColorBrush(ViewUtils.Colors.Neutral950); // Hair Color
        var Color4 = new SolidColorBrush(ViewUtils.Colors.Danger800); // Hat main color
        var Color5 = new SolidColorBrush(ViewUtils.Colors.Danger900); // Hat accent color
        var SelectedColor3 = new SolidColorBrush(ViewUtils.Colors.Neutral700); // Hair Color - Selected

        for (var i = 0; i <= 71; i++)
        {
            var index = i;
            var indexStr = index < 10 ? $"0{i}" : index.ToString();
            var button = new MultiIconRadioButton()
            {
                IsChecked = index == Editor.Mii.MiiHair.HairType,
                Margin = new(6),
                IconData = GetMiiIconData($"MiiHair{indexStr}"),
                Color1 = Color1,
                Color2 = Color2,
                Color3 = Color3,
                Color4 = Color4,
                Color5 = Color5,

                SelectedColor3 = SelectedColor3,
            };

            button.Click += (_, _) => SetHairType(index);
            HairTypesGrid.Children.Add(button);
        }
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
