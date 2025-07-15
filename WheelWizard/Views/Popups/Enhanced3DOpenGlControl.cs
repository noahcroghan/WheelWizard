using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using WheelWizard.Rendering3D;
using WheelWizard.Rendering3D.Domain;
using WheelWizard.Rendering3D.Services;

namespace WheelWizard.Views.Popups;

public class Enhanced3DOpenGlControl : OpenGlControlBase
{
    private IRenderingEngine? _renderingEngine;
    private IRenderObject? _cubeObject;
    private bool _isInitialized;
    private readonly IRendering3DSingletonService _rendering3DService;
    private int _frameCount = 0;

    public Enhanced3DOpenGlControl(IRendering3DSingletonService rendering3DService)
    {
        _rendering3DService = rendering3DService;
        Console.WriteLine("Enhanced3DOpenGlControl created");
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        Console.WriteLine("OnOpenGlInit called");
        base.OnOpenGlInit(gl);

        try
        {
            // Check OpenGL version and capabilities
            var version = gl.GetString(0x1F02); // GL_VERSION
            var vendor = gl.GetString(0x1F00); // GL_VENDOR
            var renderer = gl.GetString(0x1F01); // GL_RENDERER

            Console.WriteLine($"OpenGL Version: {version}");
            Console.WriteLine($"OpenGL Vendor: {vendor}");
            Console.WriteLine($"OpenGL Renderer: {renderer}");

            // Check if we have proper dimensions
            var bounds = Bounds;
            Console.WriteLine($"Initial bounds: {bounds.Width}x{bounds.Height}");

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                Console.WriteLine("Warning: Control has zero or negative dimensions");
            }

            // Initialize the rendering engine
            _renderingEngine = _rendering3DService.CreateRenderingEngine();
            Console.WriteLine("Rendering engine created");
            _renderingEngine.Initialize(gl);
            Console.WriteLine("Rendering engine initialized");

            // Create and initialize a cube
            _cubeObject = _rendering3DService.CreateCube(gl);
            Console.WriteLine("Cube object created");
            _cubeObject.Initialize(gl);
            Console.WriteLine("Cube object initialized");

            // Add the cube to the rendering engine
            if (_renderingEngine is OpenGLRenderingEngine openGLEngine)
            {
                openGLEngine.AddRenderObject(_cubeObject);
                Console.WriteLine("Cube added to rendering engine");
            }

            // Set initial viewport - ensure we have a minimum size
            var width = Math.Max((int)bounds.Width, 100);
            var height = Math.Max((int)bounds.Height, 100);
            _renderingEngine.Resize(width, height);

            _isInitialized = true;
            Console.WriteLine($"OpenGL initialized successfully. Size: {width}x{height}");
        }
        catch (Exception ex)
        {
            // Log error or handle gracefully
            Console.WriteLine($"OpenGL initialization error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        _frameCount++;
        if (_frameCount % 60 == 0) // Log every 60 frames (about once per second)
        {
            Console.WriteLine($"Frame {_frameCount} rendered");
        }

        if (!_isInitialized || _renderingEngine == null)
        {
            if (_frameCount == 1) // Only log once
            {
                Console.WriteLine($"Render skipped: initialized={_isInitialized}, engine={_renderingEngine != null}");
            }
            return;
        }

        try
        {
            var bounds = Bounds;
            var width = Math.Max((int)bounds.Width, 100);
            var height = Math.Max((int)bounds.Height, 100);
            gl.Viewport(0, 0, width, height);

            _renderingEngine.Render(gl, fb, width, height);

            // Request continuous updates for animation
            RequestNextFrameRendering();
        }
        catch (Exception ex)
        {
            // Log error or handle gracefully
            Console.WriteLine($"OpenGL render error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        try
        {
            _renderingEngine?.Dispose();
            _cubeObject?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OpenGL cleanup error: {ex.Message}");
        }

        base.OnOpenGlDeinit(gl);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Console.WriteLine($"Size changed to: {e.NewSize.Width}x{e.NewSize.Height}");

        if (_isInitialized && _renderingEngine != null)
        {
            var width = Math.Max((int)e.NewSize.Width, 100);
            var height = Math.Max((int)e.NewSize.Height, 100);
            _renderingEngine.Resize(width, height);
            Console.WriteLine($"Rendering engine resized to: {width}x{height}");
        }
    }
}
