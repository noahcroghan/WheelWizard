using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
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
        PopulateValues();
    }

    private void PopulateValues()
    {
        // Attribute:
        var currentFacialHair = Editor.Mii.MiiFacialHair;

        // Colors both used for mustache and beard
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral50); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(ViewUtils.Colors.Black); // Hair Color
        var color4 = new SolidColorBrush(ViewUtils.Colors.Danger400); // NONE Color
        var selectedColor4 = new SolidColorBrush(ViewUtils.Colors.Danger500);

        // Mustache:
        SetButtons(
            "MiiMustache",
            4,
            MustacheTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)currentFacialHair.MustacheType;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.SelectedColor4 = selectedColor4;
                button.Click += (_, _) => SetMustacheType(index);
            }
        );

        // Beards:  (also known as goatee internally)
        SetButtons(
            "MiiGoatee",
            4,
            BeardTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)currentFacialHair.BeardType;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.SelectedColor4 = selectedColor4;
                button.Click += (_, _) => SetBeardType(index);
            }
        );

        // Facial Hair Color:
        MustacheColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(MustacheColor))) // Using MustacheColor enum
        {
            MustacheColorBox.Items.Add(color);
            if (color == currentFacialHair.Color.ToString())
                MustacheColorBox.SelectedItem = color;
        }

        // Transform attributes:
        MustacheTransformOptions.IsVisible = Editor.Mii.MiiFacialHair.MustacheType != MustacheType.None;
        UpdateTransformTextValues(currentFacialHair);
    }

    private void SetBeardType(int index)
    {
        if (Editor?.Mii?.MiiFacialHair == null || !IsLoaded)
            return;
        var current = Editor.Mii.MiiFacialHair;
        var beardType = (BeardType)index;
        var result = MiiFacialHair.Create(current.MustacheType, beardType, current.Color, current.Size, current.Vertical);
        if (result.IsFailure)
            return;

        Editor.Mii.MiiFacialHair = result.Value;
        Editor.RefreshImage();
    }

    private void SetMustacheType(int index)
    {
        if (Editor?.Mii?.MiiFacialHair == null || !IsLoaded)
            return;

        var current = Editor.Mii.MiiFacialHair;
        var mustacheType = (MustacheType)index;
        var result = MiiFacialHair.Create(mustacheType, current.BeardType, current.Color, current.Size, current.Vertical);
        if (result.IsFailure)
            return;

        MustacheTransformOptions.IsVisible = mustacheType != MustacheType.None;
        Editor.Mii.MiiFacialHair = result.Value;
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
            Editor.Mii.MiiFacialHair = result.Value;
        else
            MustacheColorBox.SelectedItem = current.Color.ToString();

        Editor.RefreshImage();
    }

    #region TransformFunctions

    private void UpdateTransformTextValues(MiiFacialHair facialHair)
    {
        VerticalValueText.Text = ((facialHair.Vertical - 10) * -1).ToString();
        SizeValueText.Text = (facialHair.Size - 4).ToString();

        VerticalDecreaseButton.IsEnabled = facialHair.Vertical > MinVertical;
        VerticalIncreaseButton.IsEnabled = facialHair.Vertical < MaxVertical;
        SizeDecreaseButton.IsEnabled = facialHair.Size > MinSize;
        SizeIncreaseButton.IsEnabled = facialHair.Size < MaxSize;
    }

    // Consolidated helper method for button clicks
    private void TryUpdateFacialHairValue(int change, MiiTransformProperty property)
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
            case MiiTransformProperty.Vertical:
                currentValue = current.Vertical;
                min = MinVertical;
                max = MaxVertical;
                break;
            case MiiTransformProperty.Size:
                currentValue = current.Size;
                min = MinSize;
                max = MaxSize;
                break;
            default:
                throw new ArgumentException($"{property} is not an option that you can change in FacialHair");
        }

        newValue = currentValue + change;
        if (newValue < min || newValue > max)
            return; // Value is out of range, do nothing

        var result = property switch
        {
            MiiTransformProperty.Vertical => MiiFacialHair.Create(
                current.MustacheType,
                current.BeardType,
                current.Color,
                current.Size,
                newValue
            ) // Note Vertical position
            ,
            MiiTransformProperty.Size => MiiFacialHair.Create(
                current.MustacheType,
                current.BeardType,
                current.Color,
                newValue,
                current.Vertical
            ) // Note Size position
            ,
            _ => throw new ArgumentException($"{property} is not an option that you can change in FacialHair"),
        };

        // Handle the result
        if (result.IsFailure)
            return;

        Editor.Mii.MiiFacialHair = result.Value;
        UpdateTransformTextValues(result.Value); // Update UI TextBlocks
        Editor.RefreshImage();
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateFacialHairValue(-1, MiiTransformProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateFacialHairValue(+1, MiiTransformProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateFacialHairValue(-1, MiiTransformProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateFacialHairValue(+1, MiiTransformProperty.Size);

    #endregion
}
