using Avalonia.Controls;
using Avalonia.Interactivity;
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

        NoseTypeBox.Items.Clear();
        foreach (var type in Enum.GetNames(typeof(NoseType)))
        {
            NoseTypeBox.Items.Add(type);
            if (type == currentNose.Type.ToString())
                NoseTypeBox.SelectedItem = type;
        }

        UpdateValueTexts(currentNose);
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

    private void NoseTypeBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || NoseTypeBox.SelectedItem == null || Editor?.Mii?.MiiNose == null)
            return;
        if (NoseTypeBox.SelectedItem is not string typeStr)
            return;

        var newType = (NoseType)Enum.Parse(typeof(NoseType), typeStr);
        var current = Editor.Mii.MiiNose;
        if (newType == current.Type)
            return;

        var result = MiiNose.Create(newType, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiNose = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            NoseTypeBox.SelectedItem = current.Type.ToString();
        }
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(-1, NoseProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(+1, NoseProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(-1, NoseProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateNoseValue(+1, NoseProperty.Size);
}
