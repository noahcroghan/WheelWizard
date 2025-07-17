using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WheelWizard.Views.Popups.Base;

namespace WheelWizard.Views.Popups;

public partial class Test3DWindow : PopupContent
{
    private MonoGame3DControl? _monoGameControl;

    public Test3DWindow()
        : base(allowClose: true, allowParentInteraction: true, isTopMost: true, title: "3D Test Window")
    {
        InitializeComponent();
    }

    protected override void BeforeOpen()
    {
        base.BeforeOpen();
        SetupMonoGameControl();
    }

    protected override void BeforeClose()
    {
        base.BeforeClose();
        Console.WriteLine("Test3DWindow closing");
    }

    private void SetupMonoGameControl()
    {
        Console.WriteLine("Setting up MonoGame 3D control...");

        // Create the MonoGame 3D control
        _monoGameControl = new MonoGame3DControl();

        // Set explicit dimensions for the MonoGame control
        _monoGameControl.Width = 800;
        _monoGameControl.Height = 600;
        _monoGameControl.MinWidth = 400;
        _monoGameControl.MinHeight = 300;

        Console.WriteLine($"MonoGame control created with size: {_monoGameControl.Width}x{_monoGameControl.Height}");

        // Set the MonoGame control as the content of the PopupWindow
        Window.PopupContent.Content = _monoGameControl;
        Console.WriteLine("MonoGame control set as PopupWindow content");

        // Force the window to update its size
        Window.SizeToContent = SizeToContent.Manual;
        Window.Width = 800;
        Window.Height = 600;
        Console.WriteLine($"Window size set to: {Window.Width}x{Window.Height}");

        // Ensure the control is properly attached and initialized
        _monoGameControl.AttachedToVisualTree += (sender, e) =>
        {
            Console.WriteLine("MonoGame control attached to visual tree");
        };

        // Force a layout update to ensure proper sizing
        Window.InvalidateArrange();
        Window.InvalidateMeasure();
        Console.WriteLine("Window layout invalidated");
    }
}
