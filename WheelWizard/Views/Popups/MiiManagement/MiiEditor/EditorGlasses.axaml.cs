using Avalonia.Controls;
using Avalonia.Interactivity;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorGlasses : MiiEditorBaseControl
{
    private const int MinVertical = 0;
    private const int MaxVertical = 20;
    private const int MinSize = 0;
    private const int MaxSize = 7;

    public EditorGlasses(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        if (Editor?.Mii?.MiiGlasses == null)
            return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        var currentGlasses = Editor.Mii.MiiGlasses;

        GlassesTypeBox.Items.Clear();
        foreach (var type in Enum.GetNames(typeof(GlassesType)))
        {
            GlassesTypeBox.Items.Add(type);
            if (type == currentGlasses.Type.ToString())
                GlassesTypeBox.SelectedItem = type;
        }

        GlassesColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(GlassesColor)))
        {
            GlassesColorBox.Items.Add(color);
            if (color == currentGlasses.Color.ToString())
                GlassesColorBox.SelectedItem = color;
        }

        UpdateValueTexts(currentGlasses);
    }

    private void UpdateValueTexts(MiiGlasses glasses)
    {
        VerticalValueText.Text = glasses.Vertical.ToString();
        SizeValueText.Text = glasses.Size.ToString();
    }

    private enum GlassesProperty
    {
        Vertical,
        Size,
    }

    private void TryUpdateGlassValue(int change, GlassesProperty property)
    {
        if (Editor?.Mii?.MiiGlasses == null || !IsLoaded)
            return;

        var current = Editor.Mii.MiiGlasses;
        int currentValue,
            newValue,
            min,
            max;

        switch (property)
        {
            case GlassesProperty.Vertical:
                currentValue = current.Vertical;
                min = MinVertical;
                max = MaxVertical;
                break;
            case GlassesProperty.Size:
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

        OperationResult<MiiGlasses> result;
        switch (property)
        {
            case GlassesProperty.Vertical:
                result = MiiGlasses.Create(current.Type, current.Color, current.Size, newValue); // Note Vertical position
                break;
            case GlassesProperty.Size:
                result = MiiGlasses.Create(current.Type, current.Color, newValue, current.Vertical); // Note Size position
                break;
            default:
                return;
        }

        if (!result.IsSuccess)
            return;

        Editor.Mii.MiiGlasses = result.Value;
        Editor.Mii.ClearImages();
        UpdateValueTexts(result.Value);
    }

    private void GlassesTypeBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || GlassesTypeBox.SelectedItem == null || Editor?.Mii?.MiiGlasses == null)
            return;
        if (GlassesTypeBox.SelectedItem is not string typeStr)
            return;

        var newType = (GlassesType)Enum.Parse(typeof(GlassesType), typeStr);
        var current = Editor.Mii.MiiGlasses;
        if (newType == current.Type)
            return;

        var result = MiiGlasses.Create(newType, current.Color, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiGlasses = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            GlassesTypeBox.SelectedItem = current.Type.ToString();
        }
    }

    private void GlassesColorBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || GlassesColorBox.SelectedItem == null || Editor?.Mii?.MiiGlasses == null)
            return;
        if (GlassesColorBox.SelectedItem is not string colorStr)
            return;

        var newColor = (GlassesColor)Enum.Parse(typeof(GlassesColor), colorStr);
        var current = Editor.Mii.MiiGlasses;
        if (newColor == current.Color)
            return;

        var result = MiiGlasses.Create(current.Type, newColor, current.Size, current.Vertical);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiGlasses = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            GlassesColorBox.SelectedItem = current.Color.ToString();
        }
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateGlassValue(-1, GlassesProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateGlassValue(+1, GlassesProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateGlassValue(-1, GlassesProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateGlassValue(+1, GlassesProperty.Size);
}
