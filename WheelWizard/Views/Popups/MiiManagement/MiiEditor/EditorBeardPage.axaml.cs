using Avalonia.Controls;
using Avalonia.Interactivity;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorFacialHair : MiiEditorBaseControl
{
    private const int MinVertical = 0;
    private const int MaxVertical = 16; // Note: Range is 0-16
    private const int MinSize = 0;
    private const int MaxSize = 8; // Note: Range is 0-8

    public EditorFacialHair(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        if (Editor?.Mii?.MiiFacialHair == null)
            return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        var currentFacialHair = Editor.Mii.MiiFacialHair;

        // Populate Mustache Type ComboBox
        MustacheTypeBox.Items.Clear();
        foreach (var type in Enum.GetNames(typeof(MustacheType)))
        {
            MustacheTypeBox.Items.Add(type);
            if (type == currentFacialHair.MustacheType.ToString())
                MustacheTypeBox.SelectedItem = type;
        }

        // Populate Beard Type ComboBox
        BeardTypeBox.Items.Clear();
        foreach (var type in Enum.GetNames(typeof(BeardType)))
        {
            BeardTypeBox.Items.Add(type);
            if (type == currentFacialHair.BeardType.ToString())
                BeardTypeBox.SelectedItem = type;
        }

        // Populate Facial Hair Color ComboBox
        MustacheColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(MustacheColor))) // Using MustacheColor enum
        {
            MustacheColorBox.Items.Add(color);
            if (color == currentFacialHair.Color.ToString())
                MustacheColorBox.SelectedItem = color;
        }

        // Populate TextBlocks
        UpdateValueTexts(currentFacialHair);
    }

    private void UpdateValueTexts(MiiFacialHair facialHair)
    {
        VerticalValueText.Text = facialHair.Vertical.ToString();
        SizeValueText.Text = facialHair.Size.ToString();
    }

    // Enum to identify which property is being changed by buttons
    private enum FacialHairProperty
    {
        Vertical,
        Size,
    }

    // Consolidated helper method for button clicks
    private void TryUpdateFacialHairValue(int change, FacialHairProperty property)
    {
        if (Editor?.Mii?.MiiFacialHair == null || !IsLoaded)
            return;

        var current = Editor.Mii.MiiFacialHair;
        int currentValue,
            newValue,
            min,
            max;

        // Determine current value, new value, and range based on property
        switch (property)
        {
            case FacialHairProperty.Vertical:
                currentValue = current.Vertical;
                min = MinVertical;
                max = MaxVertical;
                break;
            case FacialHairProperty.Size:
                currentValue = current.Size;
                min = MinSize;
                max = MaxSize;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property), property, null);
        }

        newValue = currentValue + change;
        if (newValue < min || newValue > max)
        {
            return; // Value is out of range, do nothing
        }

        OperationResult<MiiFacialHair> result;
        switch (property)
        {
            case FacialHairProperty.Vertical:
                result = MiiFacialHair.Create(current.MustacheType, current.BeardType, current.Color, current.Size, newValue); // Note Vertical position
                break;
            case FacialHairProperty.Size:
                result = MiiFacialHair.Create(current.MustacheType, current.BeardType, current.Color, newValue, current.Vertical); // Note Size position
                break;
            default: // Should be unreachable
                return;
        }

        // Handle the result
        if (result.IsSuccess)
        {
            Editor.Mii.MiiFacialHair = result.Value;
            Editor.Mii.ClearImages();
            UpdateValueTexts(result.Value); // Update UI TextBlocks
        }
        else
        {
            ViewUtils.ShowSnackbar($"Error: {result.Error.Message}");
        }
    }

    // --- ComboBox Handlers ---
    private void MustacheTypeBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || MustacheTypeBox.SelectedItem == null || Editor?.Mii?.MiiFacialHair == null)
            return;
        if (MustacheTypeBox.SelectedItem is not string typeStr)
            return;

        var newType = (MustacheType)Enum.Parse(typeof(MustacheType), typeStr);
        var current = Editor.Mii.MiiFacialHair;
        if (newType == current.MustacheType)
            return;

        var result = MiiFacialHair.Create(newType, current.BeardType, current.Color, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiFacialHair = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            MustacheTypeBox.SelectedItem = current.MustacheType.ToString(); // Revert combo
        }
    }

    private void BeardTypeBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || BeardTypeBox.SelectedItem == null || Editor?.Mii?.MiiFacialHair == null)
            return;
        if (BeardTypeBox.SelectedItem is not string typeStr)
            return;

        var newType = (BeardType)Enum.Parse(typeof(BeardType), typeStr);
        var current = Editor.Mii.MiiFacialHair;
        if (newType == current.BeardType)
            return;

        var result = MiiFacialHair.Create(current.MustacheType, newType, current.Color, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiFacialHair = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            BeardTypeBox.SelectedItem = current.BeardType.ToString(); // Rever
        }
    }

    private void FacialHairColorBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || MustacheColorBox.SelectedItem == null || Editor?.Mii?.MiiFacialHair == null)
            return;
        if (MustacheColorBox.SelectedItem is not string colorStr)
            return;

        var newColor = (MustacheColor)Enum.Parse(typeof(MustacheColor), colorStr);
        var current = Editor.Mii.MiiFacialHair;
        if (newColor == current.Color)
            return;

        var result = MiiFacialHair.Create(current.MustacheType, current.BeardType, newColor, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiFacialHair = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            MustacheColorBox.SelectedItem = current.Color.ToString();
        }
    }

    // --- Button Click Handlers ---
    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateFacialHairValue(-1, FacialHairProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateFacialHairValue(+1, FacialHairProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateFacialHairValue(-1, FacialHairProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateFacialHairValue(+1, FacialHairProperty.Size);
}
