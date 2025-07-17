using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using WheelWizard.Rendering3D.Domain;
using WheelWizard.Views.Popups.Base;

namespace WheelWizard.Views.Popups;

public partial class Test3DWindow : PopupContent
{
    private readonly IMonoGameRenderer _monoGameRenderer;
    private readonly ILogger<Test3DWindow> _logger;

    public Test3DWindow(IMonoGameRenderer monoGameRenderer, ILogger<Test3DWindow> logger)
        : base(allowClose: true, allowParentInteraction: true, isTopMost: true, title: "3D Test Window")
    {
        _monoGameRenderer = monoGameRenderer ?? throw new ArgumentNullException(nameof(monoGameRenderer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        _logger.LogInformation("Test3DWindow closing");
    }

    private void SetupMonoGameControl()
    {
        _logger.LogInformation("Setting up MonoGame 3D renderer...");

        // Initialize the MonoGame renderer with specified dimensions
        _monoGameRenderer.Initialize(800, 600);

        // Get the MonoGame control from the renderer
        var monoGameControl = _monoGameRenderer.GetControl();

        // Set explicit dimensions for the MonoGame control
        monoGameControl.Width = 800;
        monoGameControl.Height = 600;
        monoGameControl.MinWidth = 400;
        monoGameControl.MinHeight = 300;

        _logger.LogInformation("MonoGame control created with size: {Width}x{Height}", monoGameControl.Width, monoGameControl.Height);

        // Set the MonoGame control as the content of the PopupWindow
        Window.PopupContent.Content = monoGameControl;
        _logger.LogInformation("MonoGame control set as PopupWindow content");

        // Force the window to update its size
        Window.SizeToContent = SizeToContent.Manual;
        Window.Width = 800;
        Window.Height = 600;
        _logger.LogInformation("Window size set to: {Width}x{Height}", Window.Width, Window.Height);

        // Start the renderer
        _monoGameRenderer.Start();

        // Ensure the control is properly attached and initialized
        monoGameControl.AttachedToVisualTree += (sender, e) =>
        {
            _logger.LogInformation("MonoGame control attached to visual tree");
        };

        // Force a layout update to ensure proper sizing
        Window.InvalidateArrange();
        Window.InvalidateMeasure();
        _logger.LogInformation("Window layout invalidated");
    }
}
