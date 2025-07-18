using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using WheelWizard.Rendering3D.Domain;
using WheelWizard.Views.Popups.Base;

namespace WheelWizard.Views.Popups;

public partial class Test3DWindow : PopupContent
{
    private readonly IMonoGameRenderer _monoGameRenderer;
    private readonly ILogger<Test3DWindow> _logger;

    // 3D scene objects for manipulation
    private I3DSceneObject? _cube;
    private I3DSceneObject? _sphere;
    private I3DSceneObject? _pyramid;
    private I3DSceneObject? _cylinder;

    // Animation variables
    private float _cubeRotationTime = 0f;
    private float _cubeRotationSpeed = 1f;

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

        try
        {
            SetupMonoGameControl();

            // Subscribe to the update animation event
            _monoGameRenderer.UpdateAnimation += UpdateAnimation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening Test3DWindow");
        }
    }

    protected override void BeforeClose()
    {
        base.BeforeClose();

        try
        {
            // Unsubscribe from the update animation event
            _monoGameRenderer.UpdateAnimation -= UpdateAnimation;

            // Stop and dispose the MonoGame renderer
            _monoGameRenderer.Stop();
            _monoGameRenderer.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing Test3DWindow");
        }

        _logger.LogInformation("Test3DWindow closing");
    }

    private void SetupMonoGameControl()
    {
        try
        {
            _logger.LogInformation("Setting up MonoGame 3D renderer...");

            // Initialize the MonoGame renderer with specified dimensions
            _monoGameRenderer.Initialize(700, 680); // Adjusted for the new layout

            // Get the MonoGame control from the renderer
            var monoGameControl = _monoGameRenderer.GetControl();

            // Set explicit dimensions for the MonoGame control
            monoGameControl.Width = 700;
            monoGameControl.Height = 680;
            monoGameControl.MinWidth = 400;
            monoGameControl.MinHeight = 300;

            _logger.LogInformation("MonoGame control created with size: {Width}x{Height}", monoGameControl.Width, monoGameControl.Height);

            // Set the MonoGame control as the content of the RenderArea
            RenderArea.Child = monoGameControl;
            _logger.LogInformation("MonoGame control set as RenderArea content");

            // Force the window to update its size
            Window.SizeToContent = SizeToContent.Manual;
            Window.Width = 1000;
            Window.Height = 700;
            _logger.LogInformation("Window size set to: {Width}x{Height}", Window.Width, Window.Height);

            // Ensure the window supports transparency
            Window.TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };

            // Set up the 3D scene with the new high-level API
            // We'll do this after the MonoGame control is properly initialized
            SetupDemoScene();

            // Ensure the control is properly attached and initialized
            monoGameControl.AttachedToVisualTree += (sender, e) =>
            {
                _logger.LogInformation("MonoGame control attached to visual tree");

                try
                {
                    // Start the renderer after the control is attached
                    _monoGameRenderer.Start();

                    // Set up the scene after a longer delay to ensure proper initialization
                    DispatcherTimer.RunOnce(
                        () =>
                        {
                            SetupDemoScene();
                        },
                        TimeSpan.FromMilliseconds(200)
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting MonoGame renderer");
                }
            };

            // Force a layout update to ensure proper sizing
            Window.InvalidateArrange();
            Window.InvalidateMeasure();
            _logger.LogInformation("Window layout invalidated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up MonoGame control");
        }
    }

    private void SetupDemoScene()
    {
        try
        {
            var scene = _monoGameRenderer.Scene;
            if (scene == null)
            {
                _logger.LogWarning("3D Scene is not available, retrying in 50ms...");
                // Retry after a short delay
                DispatcherTimer.RunOnce(() => SetupDemoScene(), TimeSpan.FromMilliseconds(50));
                return;
            }

            _logger.LogInformation("Setting up demo 3D scene...");

            // Clear any existing objects
            scene.ClearScene();

            // Add various 3D objects to demonstrate the system
            _cube = scene.AddObject("demo-cube", SceneObjectType.Cube, position: new Vector3(-2, 0, 0), scale: new Vector3(1.2f));

            _sphere = scene.AddObject("demo-sphere", SceneObjectType.Sphere, position: new Vector3(2, 0, 0));

            _pyramid = scene.AddObject("demo-pyramid", SceneObjectType.Pyramid, position: new Vector3(0, -1.5f, 0));

            _cylinder = scene.AddObject(
                "demo-cylinder",
                SceneObjectType.Cylinder,
                position: new Vector3(0, 1.5f, 0),
                scale: new Vector3(0.6f, 1.5f, 0.6f)
            );

            // Set up camera for a nice view
            scene.Camera.MoveTo(new Vector3(0, 3, 8));
            scene.Camera.LookAt(Vector3.Zero);

            // Set up nice lighting
            scene.Lighting.SetupSunLighting(Color.White, Vector3.Normalize(new Vector3(-1, -1, -1)), new Color(0.3f, 0.3f, 0.4f));

            _logger.LogInformation("Demo 3D scene setup complete with {ObjectCount} objects", scene.Objects.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up demo scene");
        }
    }

    // Slider event handlers
    private void OnLightDirectionChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        try
        {
            if (_monoGameRenderer.Scene?.Lighting == null)
                return;

            var direction = new Vector3(
                (float)LightDirectionXSlider.Value,
                (float)LightDirectionYSlider.Value,
                (float)LightDirectionZSlider.Value
            );

            // Normalize the direction vector
            if (direction.Length() > 0.001f)
            {
                direction = Vector3.Normalize(direction);
            }
            else
            {
                direction = new Vector3(0, 0, -1); // Default direction
            }

            _monoGameRenderer.Scene.Lighting.DirectionalDirection = direction;
            _logger.LogDebug("Light direction updated to: {Direction}", direction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating light direction");
        }
    }

    private void OnAmbientIntensityChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        try
        {
            if (_monoGameRenderer.Scene?.Lighting == null)
                return;

            var intensity = (float)AmbientIntensitySlider.Value;
            var ambientColor = new Color(intensity, intensity, intensity);
            _monoGameRenderer.Scene.Lighting.AmbientColor = ambientColor;
            _logger.LogDebug("Ambient intensity updated to: {Intensity}", intensity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ambient intensity");
        }
    }

    private void OnCameraDistanceChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        try
        {
            if (_monoGameRenderer.Scene?.Camera == null)
                return;

            var distance = (float)CameraDistanceSlider.Value;
            var height = (float)CameraHeightSlider.Value;
            var cameraPos = new Vector3(0, height, distance);
            _monoGameRenderer.Scene.Camera.MoveTo(cameraPos);
            _monoGameRenderer.Scene.Camera.LookAt(Vector3.Zero);
            _logger.LogDebug("Camera distance updated to: {Distance}", distance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating camera distance");
        }
    }

    private void OnCameraHeightChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        try
        {
            if (_monoGameRenderer.Scene?.Camera == null)
                return;

            var distance = (float)CameraDistanceSlider.Value;
            var height = (float)CameraHeightSlider.Value;
            var cameraPos = new Vector3(0, height, distance);
            _monoGameRenderer.Scene.Camera.MoveTo(cameraPos);
            _monoGameRenderer.Scene.Camera.LookAt(Vector3.Zero);
            _logger.LogDebug("Camera height updated to: {Height}", height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating camera height");
        }
    }

    private void OnCubeRotationSpeedChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        try
        {
            _cubeRotationSpeed = (float)CubeRotationSpeedSlider.Value;
            _logger.LogDebug("Cube rotation speed updated to: {Speed}", _cubeRotationSpeed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cube rotation speed");
        }
    }

    private void OnSphereScaleChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        try
        {
            if (_sphere == null)
                return;

            var scale = (float)SphereScaleSlider.Value;
            _sphere.ScaleTo(new Vector3(scale));
            _logger.LogDebug("Sphere scale updated to: {Scale}", scale);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sphere scale");
        }
    }

    private void OnPyramidPositionChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        try
        {
            if (_pyramid == null)
                return;

            var yPos = (float)PyramidPositionYSlider.Value;
            var currentPos = _pyramid.Position;
            _pyramid.MoveTo(new Vector3(currentPos.X, yPos, currentPos.Z));
            _logger.LogDebug("Pyramid Y position updated to: {Y}", yPos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pyramid position");
        }
    }

    private void OnCubeColorChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        try
        {
            if (_cube == null)
                return;

            var red = (float)CubeColorRedSlider.Value;
            var green = (float)CubeColorGreenSlider.Value;
            var blue = (float)CubeColorBlueSlider.Value;
            var color = new Color(red, green, blue);
            _cube.Color = color;
            _logger.LogDebug("Cube color updated to: {Color}", color);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cube color");
        }
    }

    private void OnResetSceneClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            // Reset all sliders to their default values
            LightDirectionXSlider.Value = 0;
            LightDirectionYSlider.Value = 0;
            LightDirectionZSlider.Value = -1;
            AmbientIntensitySlider.Value = 0.3;
            CameraDistanceSlider.Value = 8;
            CameraHeightSlider.Value = 3;
            CubeRotationSpeedSlider.Value = 1;
            SphereScaleSlider.Value = 1;
            PyramidPositionYSlider.Value = -1.5;
            CubeColorRedSlider.Value = 1;
            CubeColorGreenSlider.Value = 0.5;
            CubeColorBlueSlider.Value = 0.2;

            // Reset scene objects
            if (_cube != null)
            {
                _cube.MoveTo(new Vector3(-2, 0, 0));
                _cube.RotateTo(Vector3.Zero);
                _cube.ScaleTo(new Vector3(1.2f));
                _cube.Color = new Color(1, 0.5f, 0.2f);
            }

            if (_sphere != null)
            {
                _sphere.ScaleTo(Vector3.One);
            }

            if (_pyramid != null)
            {
                _pyramid.MoveTo(new Vector3(0, -1.5f, 0));
            }

            _cubeRotationTime = 0f;
            _logger.LogInformation("Scene reset to default values");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting scene");
        }
    }

    // Animation update method (called by the renderer)
    private void UpdateAnimation(GameTime gameTime)
    {
        try
        {
            if (_cube == null || _cubeRotationSpeed <= 0)
                return;

            // Update cube rotation
            _cubeRotationTime += (float)gameTime.ElapsedGameTime.TotalSeconds * _cubeRotationSpeed;
            var rotation = new Vector3(_cubeRotationTime, _cubeRotationTime * 0.7f, _cubeRotationTime * 0.3f);
            _cube.RotateTo(rotation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateAnimation");
        }
    }
}
