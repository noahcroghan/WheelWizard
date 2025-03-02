using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using System;

namespace WheelWizard.Views.Components.WhWzLibrary;

public class Badge : TemplatedControl
{
    public enum BadgeVariant
    {
        None, // None is the default and should be the first enum value
        WhWzDev,
        RrDev,
        GoldWinner,
        SilverWinner,
        BronzeWinner
    }
    
    public static readonly StyledProperty<BadgeVariant> VariantProperty =
        AvaloniaProperty.Register<Badge, BadgeVariant>(nameof(Variant));

    public BadgeVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }
    
    private void UpdateStyleClasses(BadgeVariant variant)
    {
        var types = Enum.GetValues<BadgeVariant>();
        foreach (var enumType in types)
        {
            Classes.Remove(enumType.ToString());
        }
        Console.WriteLine("Variant + " + variant);
        Classes.Add(variant.ToString());
    }
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == VariantProperty)
            UpdateStyleClasses(change.GetNewValue<BadgeVariant>());
    }
}

