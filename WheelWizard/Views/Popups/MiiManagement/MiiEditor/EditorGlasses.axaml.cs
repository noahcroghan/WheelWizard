using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
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
        PopulateValues();
    }

    private void PopulateValues()
    {
        // Attribute:
        var currentGlasses = Editor.Mii.MiiGlasses;

        // Glasses:
        var color1 = new SolidColorBrush(ViewUtils.Colors.Neutral50); // Skin Color
        var color2 = new SolidColorBrush(ViewUtils.Colors.Neutral300); // Skin border Color
        var color3 = new SolidColorBrush(ViewUtils.Colors.Neutral600); // Glass Color
        var color4 = new SolidColorBrush(ViewUtils.Colors.Danger400); // NONE Color
        var selectedColor4 = new SolidColorBrush(ViewUtils.Colors.Danger500);
        SetButtons(
            "MiiGlasses",
            9,
            GlassesTypesGrid,
            (index, button) =>
            {
                button.IsChecked = index == (int)currentGlasses.Type;
                button.Color1 = color1;
                button.Color2 = color2;
                button.Color3 = color3;
                button.Color4 = color4;
                button.SelectedColor4 = selectedColor4;
                button.Click += (_, _) => SetGlassesType(index);
            }
        );

        // Glass color:
        GlassesColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(GlassesColor)))
        {
            GlassesColorBox.Items.Add(color);
            if (color == currentGlasses.Color.ToString())
                GlassesColorBox.SelectedItem = color;
        }

        // Transform attributes:
        HideIfNoGlasses.IsVisible = Editor.Mii.MiiGlasses.Type != GlassesType.None;
        UpdateTransformTextValues(currentGlasses);
    }

    private void SetGlassesType(int index)
    {
        if (Editor?.Mii?.MiiGlasses == null)
            return;

        var current = Editor.Mii.MiiGlasses;

        if (index == (int)current.Type)
            return;

        var result = MiiGlasses.Create((GlassesType)index, current.Color, current.Size, current.Vertical);
        if (result.IsFailure)
            return;

        Editor.Mii.MiiGlasses = result.Value;
        HideIfNoGlasses.IsVisible = result.Value.Type != GlassesType.None;
        Editor.RefreshImage();
    }

    private void UpdateTransformTextValues(MiiGlasses glasses)
    {
        VerticalValueText.Text = glasses.Vertical.ToString();
        SizeValueText.Text = glasses.Size.ToString();

        VerticalDecreaseButton.IsEnabled = glasses.Vertical > MinVertical;
        VerticalIncreaseButton.IsEnabled = glasses.Vertical < MaxVertical;
        SizeDecreaseButton.IsEnabled = glasses.Size > MinSize;
        SizeIncreaseButton.IsEnabled = glasses.Size < MaxSize;
    }

    #region Transfrom

    private void TryUpdateGlassValue(int change, MiiTransformProperty property)
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
                throw new ArgumentException($"{property} is not an option that you can change in Glasses");
        }

        newValue = currentValue + change;

        if (newValue < min || newValue > max)
            return;

        var result = property switch
        {
            MiiTransformProperty.Vertical => MiiGlasses.Create(current.Type, current.Color, current.Size, newValue),
            MiiTransformProperty.Size => MiiGlasses.Create(current.Type, current.Color, newValue, current.Vertical),
            _ => throw new ArgumentException($"{property} is not an option that you can change in Glasses"),
        };

        if (result.IsFailure)
            return;

        Editor.Mii.MiiGlasses = result.Value;
        UpdateTransformTextValues(result.Value);
        Editor.RefreshImage();
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
            Editor.Mii.MiiGlasses = result.Value;
        else
            GlassesColorBox.SelectedItem = current.Color.ToString();

        Editor.RefreshImage();
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateGlassValue(-1, MiiTransformProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateGlassValue(+1, MiiTransformProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateGlassValue(-1, MiiTransformProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateGlassValue(+1, MiiTransformProperty.Size);

    #endregion
}
