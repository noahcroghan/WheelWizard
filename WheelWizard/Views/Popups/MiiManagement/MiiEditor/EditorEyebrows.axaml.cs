using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using WheelWizard.Views.Components;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorEyebrows : MiiEditorBaseControl
{
    // Define ranges for clarity and maintainability
    private const int MinVertical = 0;
    private const int MaxVertical = 18;
    private const int MinSize = 0;
    private const int MaxSize = 8;
    private const int MinRotation = 0;
    private const int MaxRotation = 11;
    private const int MinSpacing = 0;
    private const int MaxSpacing = 12;

    public EditorEyebrows(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        if (Editor?.Mii?.MiiEyebrows == null)
            return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        var currentEyebrows = Editor.Mii.MiiEyebrows;
        var eyebrowTypes = Enumerable.Range(0, 24).Cast<object>().ToList();
        CreateEyebrowButtons();
        EyebrowColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(EyebrowColor)))
        {
            EyebrowColorBox.Items.Add(color);
            if (color == currentEyebrows.Color.ToString())
                EyebrowColorBox.SelectedItem = color;
        }

        // Populate TextBlocks
        UpdateValueTexts(currentEyebrows);
    }

    private void CreateEyebrowButtons()
    {
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral50); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(ViewUtils.Colors.Neutral950); // Hair Color
        var color4 = new SolidColorBrush(ViewUtils.Colors.Danger800); // Hat main color
        var color5 = new SolidColorBrush(ViewUtils.Colors.Danger900); // Hat accent color
        var selectedColor3 = new SolidColorBrush(ViewUtils.Colors.Neutral700); // Hair Color - Selected
        SetButtons(
            "MiiEyebrow",
            23,
            EyebrowTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == Editor.Mii.MiiEyebrows.Type;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.Color5 = color5;
                button.Click += (_, _) => SetEyebrowType(index);
                button.SelectedColor3 = selectedColor3;
            }
        );
    }

    private void SetEyebrowType(int index)
    {
        if (Editor?.Mii?.MiiEyebrows == null)
            return;

        var current = Editor.Mii.MiiEyebrows;
        if (index == current.Type)
            return;

        var result = MiiEyebrow.Create(index, current.Rotation, current.Color, current.Size, current.Vertical, current.Spacing);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiEyebrows = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            // Reset the button to the current type if creation fails
            foreach (var child in EyebrowTypesGrid.Children)
            {
                if (child is MultiIconRadioButton button && button.IsChecked == true)
                {
                    button.IsChecked = false;
                }
            }

            var currentButton = EyebrowTypesGrid.Children[index] as MultiIconRadioButton;
            currentButton.IsChecked = true;
        }
    }

    // Helper to update all value TextBlocks
    private void UpdateValueTexts(MiiEyebrow eyebrows)
    {
        VerticalValueText.Text = eyebrows.Vertical.ToString();
        SizeValueText.Text = eyebrows.Size.ToString();
        RotationValueText.Text = eyebrows.Rotation.ToString();
        SpacingValueText.Text = eyebrows.Spacing.ToString();
    }

    private enum EyebrowProperty
    {
        Vertical,
        Size,
        Rotation,
        Spacing,
    }

    private void TryUpdateEyebrowValue(int change, EyebrowProperty property)
    {
        if (Editor?.Mii?.MiiEyebrows == null || !IsLoaded)
            return;

        var current = Editor.Mii.MiiEyebrows;
        int currentValue,
            newValue,
            min,
            max;

        // Determine current value, new value, and range based on property
        switch (property)
        {
            case EyebrowProperty.Vertical:
                currentValue = current.Vertical;
                min = MinVertical;
                max = MaxVertical;
                break;
            case EyebrowProperty.Size:
                currentValue = current.Size;
                min = MinSize;
                max = MaxSize;
                break;
            case EyebrowProperty.Rotation:
                currentValue = current.Rotation;
                min = MinRotation;
                max = MaxRotation;
                break;
            case EyebrowProperty.Spacing:
                currentValue = current.Spacing;
                min = MinSpacing;
                max = MaxSpacing;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property), property, null);
        }

        newValue = currentValue + change;

        // Check range BEFORE attempting to create
        if (newValue < min || newValue > max)
        {
            return;
        }

        OperationResult<MiiEyebrow> result;
        switch (property)
        {
            case EyebrowProperty.Vertical:
                result = MiiEyebrow.Create(current.Type, current.Rotation, current.Color, current.Size, newValue, current.Spacing);
                break;
            case EyebrowProperty.Size:
                result = MiiEyebrow.Create(current.Type, current.Rotation, current.Color, newValue, current.Vertical, current.Spacing);
                break;
            case EyebrowProperty.Rotation:
                result = MiiEyebrow.Create(current.Type, newValue, current.Color, current.Size, current.Vertical, current.Spacing);
                break;
            case EyebrowProperty.Spacing:
                result = MiiEyebrow.Create(current.Type, current.Rotation, current.Color, current.Size, current.Vertical, newValue);
                break;
            default:
                return;
        }

        if (!result.IsSuccess)
            return;

        Editor.Mii.MiiEyebrows = result.Value;
        Editor.Mii.ClearImages();
        UpdateValueTexts(result.Value);
    }

    private void EyebrowColorBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || EyebrowColorBox.SelectedItem == null || Editor?.Mii?.MiiEyebrows == null)
            return;
        if (EyebrowColorBox.SelectedItem is not string colorStr)
            return;

        var newColor = (EyebrowColor)Enum.Parse(typeof(EyebrowColor), colorStr);
        var current = Editor.Mii.MiiEyebrows;
        if (newColor == current.Color)
            return;

        var result = MiiEyebrow.Create(current.Type, current.Rotation, newColor, current.Size, current.Vertical, current.Spacing);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiEyebrows = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            EyebrowColorBox.SelectedItem = current.Color.ToString();
        }
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyebrowValue(-1, EyebrowProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyebrowValue(+1, EyebrowProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyebrowValue(-1, EyebrowProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyebrowValue(+1, EyebrowProperty.Size);

    private void RotationDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyebrowValue(-1, EyebrowProperty.Rotation);

    private void RotationIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyebrowValue(+1, EyebrowProperty.Rotation);

    private void SpacingDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyebrowValue(-1, EyebrowProperty.Spacing);

    private void SpacingIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyebrowValue(+1, EyebrowProperty.Spacing);
}
