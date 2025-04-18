using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Components;

public class MiiBlock : RadioButton
{
    public static readonly StyledProperty<Mii?> MiiProperty = AvaloniaProperty.Register<MiiBlock, Mii?>(nameof(Mii));

    public Mii? Mii
    {
        get => GetValue(MiiProperty);
        set => SetValue(MiiProperty, value);
    }
}
