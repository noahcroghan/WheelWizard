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
        PopulateValues();
    }

    private void PopulateValues()
    {
        // Attribute:
        var currentNose = Editor.Mii.MiiNose;

        // Nose:
        GenerateNoseButtons();

        // Transform attributes:
        UpdateValueTexts(currentNose);
    }

    private void GenerateNoseButtons()
    {
        var color1 = new SolidColorBrush(ViewUtils.Colors.Black);
        SetButtons(
            "MiiNose",
            12,
            NoseTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)Editor.Mii.MiiNose.Type;
                button.Color1 = color1;
                button.Click += (_, _) => SetNoseType(index);
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
        if (result.IsFailure)
            return;

        Editor.Mii.MiiNose = result.Value;
        Editor.RefreshImage();
    }

    private void UpdateValueTexts(MiiNose nose)
    {
        VerticalValueText.Text = nose.Vertical.ToString();
        SizeValueText.Text = nose.Size.ToString();

        VerticalDecreaseButton.IsEnabled = nose.Vertical > MinVertical;
        VerticalIncreaseButton.IsEnabled = nose.Vertical < MaxVertical;
        SizeDecreaseButton.IsEnabled = nose.Size > MinSize;
        SizeIncreaseButton.IsEnabled = nose.Size < MaxSize;
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
                throw new ArgumentException($"{property} is not an option that you can change in Nose");
        }

        newValue = currentValue + change;

        if (newValue < min || newValue > max)
            return;

        var result = property switch
        {
            NoseProperty.Vertical => MiiNose.Create(current.Type, current.Size, newValue),
            NoseProperty.Size => MiiNose.Create(current.Type, newValue, current.Vertical),
            _ => throw new ArgumentException($"{property} is not an option that you can change in Nose"),
        };

        if (result.IsFailure)
            return;

        Editor.Mii.MiiNose = result.Value;
        UpdateValueTexts(result.Value);
        Editor.RefreshImage();
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(-1, NoseProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(+1, NoseProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(-1, NoseProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(+1, NoseProperty.Size);
}
