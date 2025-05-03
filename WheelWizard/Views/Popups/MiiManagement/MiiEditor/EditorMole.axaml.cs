using Avalonia.Interactivity;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Popups.MiiManagement.MiiEditor;

public partial class EditorMole : MiiEditorBaseControl
{
    private const int MinVertical = 0;
    private const int MaxVertical = 30;
    private const int MinSize = 0;
    private const int MaxSize = 8;
    private const int MinHorizontal = 0;
    private const int MaxHorizontal = 16;

    public EditorMole(MiiEditorWindow ew)
        : base(ew)
    {
        InitializeComponent();
        if (Editor?.Mii?.MiiMole == null)
            return;
        PopulateValues();
    }

    private void PopulateValues()
    {
        var currentMole = Editor.Mii.MiiMole;

        MoleEnabledCheck.IsChecked = currentMole.Exists;
        MoleControlsPanel.IsVisible = currentMole.Exists;

        UpdateValueTexts(currentMole);
    }

    private void UpdateValueTexts(MiiMole mole)
    {
        VerticalValueText.Text = mole.Vertical.ToString();
        SizeValueText.Text = mole.Size.ToString();
        HorizontalValueText.Text = mole.Horizontal.ToString();
    }

    private enum MoleProperty
    {
        Vertical,
        Size,
        Horizontal,
    }

    private void TryUpdateMoleValue(int change, MoleProperty property)
    {
        if (Editor?.Mii?.MiiMole == null || !IsLoaded || MoleEnabledCheck.IsChecked != true)
            return;

        var current = Editor.Mii.MiiMole;
        int currentValue,
            newValue,
            min,
            max;

        switch (property)
        {
            case MoleProperty.Vertical:
                currentValue = current.Vertical;
                min = MinVertical;
                max = MaxVertical;
                break;
            case MoleProperty.Size:
                currentValue = current.Size;
                min = MinSize;
                max = MaxSize;
                break;
            case MoleProperty.Horizontal:
                currentValue = current.Horizontal;
                min = MinHorizontal;
                max = MaxHorizontal;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(property), property, null);
        }

        newValue = currentValue + change;

        if (newValue < min || newValue > max)
        {
            Console.WriteLine($"Mole {property} limit reached ({newValue}).");
            return;
        }

        OperationResult<MiiMole> result;
        switch (property)
        {
            case MoleProperty.Vertical:
                result = MiiMole.Create(current.Exists, current.Size, newValue, current.Horizontal);
                break;
            case MoleProperty.Size:
                result = MiiMole.Create(current.Exists, newValue, current.Vertical, current.Horizontal);
                break;
            case MoleProperty.Horizontal:
                result = MiiMole.Create(current.Exists, current.Size, current.Vertical, newValue);
                break;
            default:
                return;
        }

        if (result.IsSuccess)
        {
            Editor.Mii.MiiMole = result.Value;
            UpdateValueTexts(result.Value);
        }
        else { }
    }

    private void MoleEnabledCheck_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (!IsLoaded || Editor?.Mii?.MiiMole == null)
            return;

        bool isEnabled = MoleEnabledCheck.IsChecked == true;
        var current = Editor.Mii.MiiMole;

        if (isEnabled == current.Exists)
            return;

        var result = MiiMole.Create(isEnabled, current.Size, current.Vertical, current.Horizontal);

        if (result.IsSuccess)
        {
            Editor.Mii.MiiMole = result.Value;
        }
        else
        {
            MoleEnabledCheck.IsChecked = current.Exists;
        }
        MoleControlsPanel.IsVisible = Editor.Mii.MiiMole.Exists;
    }

    private void VerticalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateMoleValue(-1, MoleProperty.Vertical);

    private void VerticalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateMoleValue(+1, MoleProperty.Vertical);

    private void SizeDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateMoleValue(-1, MoleProperty.Size);

    private void SizeIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateMoleValue(+1, MoleProperty.Size);

    private void HorizontalDecrease_Click(object? sender, RoutedEventArgs e) => TryUpdateMoleValue(-1, MoleProperty.Horizontal);

    private void HorizontalIncrease_Click(object? sender, RoutedEventArgs e) => TryUpdateMoleValue(+1, MoleProperty.Horizontal);
}
