using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Components;

public class MiiBlock : RadioButton
{
    private static ContextMenu? s_oldMenu;

    public static readonly StyledProperty<Mii?> MiiProperty = AvaloniaProperty.Register<MiiBlock, Mii?>(nameof(Mii));

    public Mii? Mii
    {
        get => GetValue(MiiProperty);
        set => SetValue(MiiProperty, value);
    }

    public static readonly StyledProperty<string?> MiiNameProperty = AvaloniaProperty.Register<MiiBlock, string?>(nameof(MiiName));

    public string? MiiName
    {
        get => GetValue(MiiNameProperty);
        private set => SetValue(MiiNameProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MiiProperty)
            MiiName = change.GetNewValue<Mii?>()?.Name.ToString();

        Tag = MiiName ?? String.Empty;
        ClipToBounds = string.IsNullOrWhiteSpace(MiiName);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled = true;

        var properties = e.GetCurrentPoint(this).Properties;
        if (properties.IsLeftButtonPressed)
            IsChecked = !IsChecked;

        RaiseEvent(new RoutedEventArgs(ClickEvent));
    }
}
