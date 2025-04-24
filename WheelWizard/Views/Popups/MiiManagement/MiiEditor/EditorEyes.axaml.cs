using Avalonia.Controls;
using Avalonia.Interactivity;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorEyes : MiiEditorBaseControl
{
    private const int MinType = 0;
    private const int MaxType = 47;
    private const int MinRotation = 0;
    private const int MaxRotation = 7;
    private const int MinVertical = 0;
    private const int MaxVertical = 18;
    private const int MinSize = 0;
    private const int MaxSize = 7;
    private const int MinSpacing = 0;
    private const int MaxSpacing = 12;

    public EditorEyes(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        if (Editor?.Mii?.MiiEyes == null)
            return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        var currentEyes = Editor.Mii.MiiEyes;
        EyeTypeBox.Items.Clear();
        var eyeTypes = Enumerable.Range(MinType, MaxType - MinType + 1).Cast<object>().ToList(); // 0 to 47
        EyeTypeBox.ItemsSource = eyeTypes;
        EyeTypeBox.SelectedItem = currentEyes.Type;
        EyeColorBox.Items.Clear();
        foreach (var color in Enum.GetNames(typeof(EyeColor)))
        {
            EyeColorBox.Items.Add(color);
            if (color == currentEyes.Color.ToString())
                EyeColorBox.SelectedItem = color;
        }
        UpdateValueTexts(currentEyes);
    }

    private void UpdateValueTexts(MiiEye eyes)
    {
        VerticalValueText.Text = eyes.Vertical.ToString();
        SizeValueText.Text = eyes.Size.ToString();
        RotationValueText.Text = eyes.Rotation.ToString();
        SpacingValueText.Text = eyes.Spacing.ToString();
    }

    private enum EyeProperty
    {
        Vertical,
        Size,
        Rotation,
        Spacing,
    }

    private void TryUpdateEyeValue(int change, EyeProperty property)
    {
        if (Editor?.Mii?.MiiEyes == null || !IsLoaded)
            return;

        var current = Editor.Mii.MiiEyes;
        int currentValue,
            newValue,
            min,
            max;
        switch (property)
        {
            case EyeProperty.Vertical:
                currentValue = current.Vertical;
                min = MinVertical;
                max = MaxVertical;
                break;
            case EyeProperty.Size:
                currentValue = current.Size;
                min = MinSize;
                max = MaxSize;
                break;
            case EyeProperty.Rotation:
                currentValue = current.Rotation;
                min = MinRotation;
                max = MaxRotation;
                break;
            case EyeProperty.Spacing:
                currentValue = current.Spacing;
                min = MinSpacing;
                max = MaxSpacing;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property), property, null);
        }

        newValue = currentValue + change;
        if (newValue < min || newValue > max)
        {
            return;
        }

        OperationResult<MiiEye> result;
        switch (property)
        {
            case EyeProperty.Vertical:
                result = MiiEye.Create(current.Type, current.Rotation, newValue, current.Color, current.Size, current.Spacing); // Note Vertical position
                break;
            case EyeProperty.Size:
                result = MiiEye.Create(current.Type, current.Rotation, current.Vertical, current.Color, newValue, current.Spacing); // Note Size position
                break;
            case EyeProperty.Rotation:
                result = MiiEye.Create(current.Type, newValue, current.Vertical, current.Color, current.Size, current.Spacing); // Note Rotation position
                break;
            case EyeProperty.Spacing:
                result = MiiEye.Create(current.Type, current.Rotation, current.Vertical, current.Color, current.Size, newValue); // Note Spacing position
                break;
            default:
                return;
        }

        if (!result.IsSuccess)
            return;

        Editor.Mii.MiiEyes = result.Value;
        Editor.Mii.ClearImages();
        UpdateValueTexts(result.Value);
    }

    private void EyeTypeBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || EyeTypeBox.SelectedItem == null || Editor?.Mii?.MiiEyes == null)
            return;
        if (EyeTypeBox.SelectedItem is not int newType)
            return;

        var current = Editor.Mii.MiiEyes;
        if (newType == current.Type)
            return;

        var result = MiiEye.Create(newType, current.Rotation, current.Vertical, current.Color, current.Size, current.Spacing);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiEyes = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            EyeTypeBox.SelectedItem = current.Type;
        }
    }

    private void EyeColorBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || EyeColorBox.SelectedItem == null || Editor?.Mii?.MiiEyes == null)
            return;
        if (EyeColorBox.SelectedItem is not string colorStr)
            return;

        var newColor = (EyeColor)Enum.Parse(typeof(EyeColor), colorStr);
        var current = Editor.Mii.MiiEyes;
        if (newColor == current.Color)
            return;

        var result = MiiEye.Create(current.Type, current.Rotation, current.Vertical, newColor, current.Size, current.Spacing);
        if (result.IsSuccess)
        {
            Editor.Mii.MiiEyes = result.Value;
            Editor.Mii.ClearImages();
        }
        else
        {
            EyeColorBox.SelectedItem = current.Color.ToString();
        }
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyeValue(-1, EyeProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyeValue(+1, EyeProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyeValue(-1, EyeProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyeValue(+1, EyeProperty.Size);

    private void RotationDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyeValue(-1, EyeProperty.Rotation);

    private void RotationIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyeValue(+1, EyeProperty.Rotation);

    private void SpacingDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyeValue(-1, EyeProperty.Spacing);

    private void SpacingIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateEyeValue(+1, EyeProperty.Spacing);
}
