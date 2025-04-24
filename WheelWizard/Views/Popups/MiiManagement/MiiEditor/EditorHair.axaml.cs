using Avalonia.Controls;
using Avalonia.Interactivity;
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
        HairTypeBox.Items.Clear();
        var hairTypes = Enumerable.Range(0, 72).Cast<object>().ToList();
        HairTypeBox.ItemsSource = hairTypes;
        HairTypeBox.SelectedItem = currentHair.HairType;

        HairColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(HairColor)))
        {
            HairColorBox.Items.Add(color);
            if (color == currentHair.HairColor.ToString())
                HairColorBox.SelectedItem = color;
        }

        HairFlippedCheck.IsChecked = currentHair.HairFlipped;
    }

    private void HairTypeBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (Editor?.Mii?.MiiHair == null || !IsLoaded || HairTypeBox.SelectedItem == null)
            return;

        if (HairTypeBox.SelectedItem is not int newHairType) // Safely check and cast
        {
            return;
        }

        var currentHair = Editor.Mii.MiiHair;
        if (newHairType == currentHair.HairType)
            return;

        var result = MiiHair.Create(newHairType, currentHair.HairColor, currentHair.HairFlipped);

        if (result.IsFailure)
            return;

        Editor.Mii.MiiHair = result.Value;
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
