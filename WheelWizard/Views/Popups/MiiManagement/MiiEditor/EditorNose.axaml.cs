using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using WheelWizard.Views.Components;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorNose : MiiEditorBaseControl
{
    private const int MinVertical = 0;
    private const int MaxVertical = 18;
    private const int MinSize = 0;
    private const int MaxSize = 8;

    public EditorNose(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        if (Editor?.Mii?.MiiNose == null)
            return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        var currentNose = Editor.Mii.MiiNose;
        GenerateNoseButtons();
        UpdateValueTexts(currentNose);
    }

    private void GenerateNoseButtons()
    {
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral50); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(ViewUtils.Colors.Neutral950); // Hair Color
        var color4 = new SolidColorBrush(ViewUtils.Colors.Danger800); // Hat main color
        var color5 = new SolidColorBrush(ViewUtils.Colors.Danger900); // Hat accent color
        var selectedColor3 = new SolidColorBrush(ViewUtils.Colors.Neutral700); // Hair Color - Selected

        SetButtons(
            "MiiNose",
            11,
            NoseTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)Editor.Mii.MiiNose.Type;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.Color5 = color5;
                button.Click += (_, _) => SetNoseType(index);
                button.SelectedColor3 = selectedColor3;
            }
        );
    }

    private void SetNoseType(int index)
    {
        if (Editor?.Mii?.MiiNose == null || !IsLoaded)
            return;
        var current = Editor.Mii.MiiNose;
        var noseType = (NoseType)index;
        var result = MiiNose.Create(noseType, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiNose = result.Value;
            Editor.Mii.ClearImages();
            UpdateValueTexts(result.Value);
        }
        else
        {
            // Reset the button to the current type if creation fails
            foreach (var child in NoseTypesGrid.Children)
            {
                if (child is MultiIconRadioButton button && button.IsChecked == true)
                {
                    button.IsChecked = false;
                }
            }

            var currentButton = NoseTypesGrid.Children[index] as MultiIconRadioButton;
            currentButton.IsChecked = true;
        }
    }

    private void UpdateValueTexts(MiiNose nose)
    {
        VerticalValueText.Text = nose.Vertical.ToString();
        SizeValueText.Text = nose.Size.ToString();
    }

    private enum NoseProperty
    {
        Vertical,
        Size,
    }

    private void TryUpdateNoseValue(int change, NoseProperty property)
    {
        if (Editor?.Mii?.MiiNose == null || !IsLoaded)
            return;

        var current = Editor.Mii.MiiNose;
        int currentValue,
            newValue,
            min,
            max;

        switch (property)
        {
            case NoseProperty.Vertical:
                currentValue = current.Vertical;
                min = MinVertical;
                max = MaxVertical;
                break;
            case NoseProperty.Size:
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
            return;
        }

        OperationResult<MiiNose> result;
        switch (property)
        {
            case NoseProperty.Vertical:
                result = MiiNose.Create(current.Type, current.Size, newValue);
                break;
            case NoseProperty.Size:
                result = MiiNose.Create(current.Type, newValue, current.Vertical);
                break;
            default:
                return;
        }

        if (result.IsSuccess)
        {
            Editor.Mii.MiiNose = result.Value;
            Editor.Mii.ClearImages();
            UpdateValueTexts(result.Value);
        }
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(-1, NoseProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(+1, NoseProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(-1, NoseProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(+1, NoseProperty.Size);
}
