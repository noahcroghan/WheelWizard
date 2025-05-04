using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using WheelWizard.Views.Components;
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
        GenerateMustacheButtons();
        GenerateBeardButtons(); //also known as goatee internally
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

    private void GenerateBeardButtons()
    {
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral50); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(ViewUtils.Colors.Black); // Hair Color
        var color4 = new SolidColorBrush(ViewUtils.Colors.Danger500); // Skin Color
        SetButtons(
            "MiiGoatee",
            4,
            BeardTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)Editor.Mii.MiiFacialHair.BeardType;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.Click += (_, _) => SetBeardType(index);
            }
        );
    }

    private void SetBeardType(int index)
    {
        if (Editor?.Mii?.MiiFacialHair == null || !IsLoaded)
            return;
        var current = Editor.Mii.MiiFacialHair;
        var beardType = (BeardType)index;
        var result = MiiFacialHair.Create(current.MustacheType, beardType, current.Color, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiFacialHair = result.Value;
            UpdateValueTexts(result.Value); // Update UI TextBlocks
        }
        else
        {
            // Reset the button to the current type if creation fails
            foreach (var child in BeardTypesGrid.Children)
            {
                if (child is MultiIconRadioButton button && button.IsChecked == true)
                {
                    button.IsChecked = false;
                }
            }
        }

        Editor.RefreshImage();
    }

    private void GenerateMustacheButtons()
    {
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral50); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(ViewUtils.Colors.Black); // Hair Color
        var color4 = new SolidColorBrush(ViewUtils.Colors.Danger500); // Skin Color
        SetButtons(
            "MiiMustache",
            4,
            MustacheTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)Editor.Mii.MiiFacialHair.MustacheType;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.Click += (_, _) => SetMustacheType(index);
            }
        );
    }

    private void SetMustacheType(int index)
    {
        if (Editor?.Mii?.MiiFacialHair == null || !IsLoaded)
            return;
        var current = Editor.Mii.MiiFacialHair;
        var mustacheType = (MustacheType)index;
        var result = MiiFacialHair.Create(mustacheType, current.BeardType, current.Color, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiFacialHair = result.Value;
            UpdateValueTexts(result.Value); // Update UI TextBlocks
        }
        else
        {
            // Reset the button to the current type if creation fails
            foreach (var child in MustacheTypesGrid.Children)
            {
                if (child is MultiIconRadioButton button && button.IsChecked == true)
                {
                    button.IsChecked = false;
                }
            }
        }

        Editor.RefreshImage();
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
        if (result.IsFailure)
            return;

        Editor.Mii.MiiFacialHair = result.Value;
        UpdateValueTexts(result.Value); // Update UI TextBlocks
        Editor.RefreshImage();
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
