using Avalonia.Controls;
using Avalonia.Interactivity;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorLips : MiiEditorBaseControl
{
    private const int MinType = 0;
    private const int MaxType = 23;
    private const int MinVertical = 0;
    private const int MaxVertical = 18;
    private const int MinSize = 0;
    private const int MaxSize = 8;

    public EditorLips(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        if (Editor?.Mii?.MiiLips == null)
            return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        var currentLips = Editor.Mii.MiiLips;

        LipTypeBox.Items.Clear();
        var lipTypes = Enumerable.Range(MinType, MaxType - MinType + 1).Cast<object>().ToList(); // 0 to 23
        LipTypeBox.ItemsSource = lipTypes;
        LipTypeBox.SelectedItem = currentLips.Type;

        LipColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(LipColor)))
        {
            LipColorBox.Items.Add(color);
            if (color == currentLips.Color.ToString())
                LipColorBox.SelectedItem = color;
        }

        UpdateValueTexts(currentLips);
    }

    private void UpdateValueTexts(MiiLip lips)
    {
        VerticalValueText.Text = lips.Vertical.ToString();
        SizeValueText.Text = lips.Size.ToString();
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
                throw new ArgumentOutOfRangeException(nameof(property), property, null);
        }

        newValue = currentValue + change;

        if (newValue < min || newValue > max)
        {
            return;
        }

        OperationResult<MiiLip> result;
        switch (property)
        {
            case LipProperty.Vertical:
                result = MiiLip.Create(current.Type, current.Color, current.Size, newValue); // Note Vertical position
                break;
            case LipProperty.Size:
                result = MiiLip.Create(current.Type, current.Color, newValue, current.Vertical); // Note Size position
                break;
            default:
                return;
        }

        if (!result.IsSuccess)
            return;

        Editor.Mii.MiiLips = result.Value;
        Editor.Mii.ClearImages();
        UpdateValueTexts(result.Value);
    }

    private void LipTypeBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || LipTypeBox.SelectedItem == null || Editor?.Mii?.MiiLips == null)
            return;
        if (LipTypeBox.SelectedItem is not int newType)
            return;

        var current = Editor.Mii.MiiLips;
        if (newType == current.Type)
            return;

        // MiiLip.Create(int type, LipColor color, int size, int vertical)
        var result = MiiLip.Create(newType, current.Color, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiLips = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            LipTypeBox.SelectedItem = current.Type;
        }
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
        {
            Editor.Mii.MiiLips = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            LipColorBox.SelectedItem = current.Color.ToString();
        }
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateLipValue(-1, LipProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateLipValue(+1, LipProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateLipValue(-1, LipProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateLipValue(+1, LipProperty.Size);
}
