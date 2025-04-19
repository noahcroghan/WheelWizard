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

    /*
     // Not sure if this is something we want to keep
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled = true;
        IsChecked = true;

        if (ContextMenu != null)
        {
            if (s_oldMenu != null && s_oldMenu != ContextMenu)
                s_oldMenu.Close();

            s_oldMenu = ContextMenu;
            ContextMenu?.Open();
        }

        RaiseEvent(new RoutedEventArgs(ClickEvent));
    }
    */
}
