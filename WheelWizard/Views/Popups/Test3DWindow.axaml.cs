using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using WheelWizard.Rendering3D.Domain;
using WheelWizard.Views.Popups.Base;

namespace WheelWizard.Views.Popups;

public partial class Test3DWindow : PopupContent
{
    private readonly IMonoGameRenderer _monoGameRenderer;

    public Test3DWindow(IMonoGameRenderer monoGameRenderer)
        : base(allowClose: true, allowParentInteraction: true, isTopMost: true, title: "3D Test Window")
    {
        _monoGameRenderer = monoGameRenderer ?? throw new ArgumentNullException(nameof(monoGameRenderer));
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

        // Stop and dispose the MonoGame renderer
        _monoGameRenderer.Stop();
        _monoGameRenderer.Dispose();

        Console.WriteLine("Test3DWindow closing");
    }

    private void SetupMonoGameControl()
    {
        Console.WriteLine("Setting up MonoGame 3D renderer...");

        // Initialize the MonoGame renderer with specified dimensions
        _monoGameRenderer.Initialize(800, 600);

        // Get the MonoGame control from the renderer
        var monoGameControl = _monoGameRenderer.GetControl();

        // Set explicit dimensions for the MonoGame control
        monoGameControl.Width = 800;
        monoGameControl.Height = 600;
        monoGameControl.MinWidth = 400;
        monoGameControl.MinHeight = 300;

        Console.WriteLine($"MonoGame control created with size: {monoGameControl.Width}x{monoGameControl.Height}");

        // Set the MonoGame control as the content of the PopupWindow
        Window.PopupContent.Content = monoGameControl;
        Console.WriteLine("MonoGame control set as PopupWindow content");

        // Force the window to update its size
        Window.SizeToContent = SizeToContent.Manual;
        Window.Width = 800;
        Window.Height = 600;
        Console.WriteLine($"Window size set to: {Window.Width}x{Window.Height}");

        // Start the renderer
        _monoGameRenderer.Start();

        // Ensure the control is properly attached and initialized
        monoGameControl.AttachedToVisualTree += (sender, e) =>
        {
            Console.WriteLine("MonoGame control attached to visual tree");
        };

        // Force a layout update to ensure proper sizing
        Window.InvalidateArrange();
        Window.InvalidateMeasure();
        Console.WriteLine("Window layout invalidated");
    }
}
