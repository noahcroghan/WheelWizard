using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using WheelWizard.Rendering3D;
using WheelWizard.Views.Popups.Base;

namespace WheelWizard.Views.Popups;

public partial class Test3DWindow : PopupContent
{
    private readonly IRendering3DSingletonService _rendering3DService;
    private Enhanced3DOpenGlControl? _openGlControl;

    public Test3DWindow(IRendering3DSingletonService rendering3DService)
        : base(allowClose: true, allowParentInteraction: true, isTopMost: true, title: "3D Test Window")
    {
        _rendering3DService = rendering3DService;
        InitializeComponent();
    }

    protected override void BeforeOpen()
    {
        base.BeforeOpen();
        SetupOpenGLControl();
    }

    protected override void BeforeClose()
    {
        base.BeforeClose();
        Console.WriteLine("Test3DWindow closing");
    }

    private void SetupOpenGLControl()
    {
        Console.WriteLine("Setting up OpenGL control...");

        // Create the enhanced OpenGL control with dependency injection
        _openGlControl = new Enhanced3DOpenGlControl(_rendering3DService);

        // Set explicit dimensions for the OpenGL control
        _openGlControl.Width = 800;
        _openGlControl.Height = 600;
        _openGlControl.MinWidth = 400;
        _openGlControl.MinHeight = 300;

        Console.WriteLine($"OpenGL control created with size: {_openGlControl.Width}x{_openGlControl.Height}");

        // Set the OpenGL control as the content of the PopupWindow
        Window.PopupContent.Content = _openGlControl;
        Console.WriteLine("OpenGL control set as PopupWindow content");

        // Force the window to update its size
        Window.SizeToContent = SizeToContent.Manual;
        Window.Width = 800;
        Window.Height = 600;
        Console.WriteLine($"Window size set to: {Window.Width}x{Window.Height}");

        // Ensure the control is properly attached and initialized
        _openGlControl.AttachedToVisualTree += (sender, e) =>
        {
            Console.WriteLine("OpenGL control attached to visual tree");
        };

        // Force a layout update to ensure proper sizing
        Window.InvalidateArrange();
        Window.InvalidateMeasure();
        Console.WriteLine("Window layout invalidated");
    }
}
