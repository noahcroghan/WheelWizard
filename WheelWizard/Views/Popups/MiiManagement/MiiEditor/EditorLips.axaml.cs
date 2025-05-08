using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using WheelWizard.Views.Components;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorLips : MiiEditorBaseControl
{
    private const int MinVertical = 0;
    private const int MaxVertical = 18;
    private const int MinSize = 0;
    private const int MaxSize = 8;

    public EditorLips(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        PopulateValues();
    }

    private void PopulateValues()
    {
        // Attribute:
        var currentLips = Editor.Mii.MiiLips;

        // Lips:
        GenerateMouthButtons();

        // Lip Colors:
        LipColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(LipColor)))
        {
            LipColorBox.Items.Add(color);
            if (color == currentLips.Color.ToString())
                LipColorBox.SelectedItem = color;
        }

        // Transform attributes:
        UpdateValueTexts(currentLips);
    }

    private void GenerateMouthButtons()
    {
        var color1 = new SolidColorBrush(new Color(255, 165, 57, 29)); // Lip Top Color
        var color2 = new SolidColorBrush(new Color(255, 255, 93, 13)); // Lip bottom Color
        var color3 = new SolidColorBrush(Colors.Black); // LipLine Color
        var color4 = new SolidColorBrush(Colors.White); // tooth color
        SetButtons(
            "MiiMouth",
            24,
            MouthTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == Editor.Mii.MiiLips.Type;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.Click += (_, _) => SetMouthType(index);
            }
        );
    }

    private void SetMouthType(int index)
    {
        if (Editor?.Mii?.MiiLips == null)
            return;

        var current = Editor.Mii.MiiLips;
        if (index == current.Type)
            return;

        // MiiLip.Create(int type, LipColor color, int size, int vertical)
        var result = MiiLip.Create(index, current.Color, current.Size, current.Vertical);
        if (result.IsFailure)
            return;

        Editor.Mii.MiiLips = result.Value;
        Editor.RefreshImage();
    }

    private void UpdateValueTexts(MiiLip lips)
    {
        VerticalValueText.Text = lips.Vertical.ToString();
        SizeValueText.Text = lips.Size.ToString();

        VerticalDecreaseButton.IsEnabled = lips.Vertical > MinVertical;
        VerticalIncreaseButton.IsEnabled = lips.Vertical < MaxVertical;
        SizeDecreaseButton.IsEnabled = lips.Size > MinSize;
        SizeIncreaseButton.IsEnabled = lips.Size < MaxSize;
    }

    private enum LipProperty
    {
        Vertical,
        Size,
    }

    private void TryUpdateLipValue(int change, LipProperty property)
    {
        if (Editor?.Mii?.MiiLips == null || !IsLoaded)
            return;

        var current = Editor.Mii.MiiLips;
        int currentValue,
            newValue,
            min,
            max;

        switch (property)
        {
            case LipProperty.Vertical:
                currentValue = current.Vertical;
                min = MinVertical;
                max = MaxVertical;
                break;
            case LipProperty.Size:
                currentValue = current.Size;
                min = MinSize;
                max = MaxSize;
                break;
            default:
                throw new ArgumentException($"{property} is not an option that you can change in Lips");
        }

        newValue = currentValue + change;

        if (newValue < min || newValue > max)
            return;

        var result = property switch
        {
            LipProperty.Vertical => MiiLip.Create(current.Type, current.Color, current.Size, newValue),
            LipProperty.Size => MiiLip.Create(current.Type, current.Color, newValue, current.Vertical),
            _ => throw new ArgumentException($"{property} is not an option that you can change in Lips"),
        };

        if (result.IsFailure)
            return;

        Editor.Mii.MiiLips = result.Value;
        UpdateValueTexts(result.Value);
        Editor.RefreshImage();
    }

    private void LipColorBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || LipColorBox.SelectedItem == null || Editor?.Mii?.MiiLips == null)
            return;
        if (LipColorBox.SelectedItem is not string colorStr)
            return;

        var newColor = (LipColor)Enum.Parse(typeof(LipColor), colorStr);
        var current = Editor.Mii.MiiLips;
        if (newColor == current.Color)
            return;

        var result = MiiLip.Create(current.Type, newColor, current.Size, current.Vertical);
        if (result.IsSuccess)
            Editor.Mii.MiiLips = result.Value;
        else
            LipColorBox.SelectedItem = current.Color.ToString();

        Editor.RefreshImage();
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateLipValue(-1, LipProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateLipValue(+1, LipProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateLipValue(-1, LipProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateLipValue(+1, LipProperty.Size);
}
