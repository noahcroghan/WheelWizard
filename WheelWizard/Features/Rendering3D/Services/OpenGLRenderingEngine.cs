using Avalonia.OpenGL;
using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D.Services;

public class OpenGLRenderingEngine : IRenderingEngine
{
    private readonly List<IRenderObject> _renderObjects = [];
    private Matrix4x4 _projectionMatrix;
    private Matrix4x4 _viewMatrix;
    private float _rotationAngle;
    private bool _isInitialized;

    public void Initialize(GlInterface gl)
    {
        if (_isInitialized)
            return;

        // Enable depth testing
        gl.Enable(0x0B71); // GL_DEPTH_TEST
        gl.DepthFunc(0x0201); // GL_LEQUAL

        // Set up view matrix (camera looking at origin from a distance)
        _viewMatrix = Matrix4x4.CreateTranslation(new Vector3(0, 0, -3));

        _isInitialized = true;
    }

    public void AddRenderObject(IRenderObject renderObject)
    {
        _renderObjects.Add(renderObject);
    }

    public void Render(GlInterface gl, int frameBuffer, int width, int height)
    {
        if (!_isInitialized)
            return;

        // Clear the screen
        gl.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        gl.Clear(0x4100); // GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT

        // Update rotation
        _rotationAngle += 0.01f;
        if (_rotationAngle > 2 * Math.PI)
            _rotationAngle = 0;

        // Create model matrix with rotation
        var modelMatrix = Matrix4x4.CreateRotationY(_rotationAngle);

        // Render all objects
        foreach (var renderObject in _renderObjects)
        {
            if (renderObject is CubeRenderObject cube)
            {
                try
                {
                    var shader = cube.GetShader();
                    shader.Use(gl);
                    shader.SetUniform(gl, "uModel", modelMatrix.Values);
                    shader.SetUniform(gl, "uView", _viewMatrix.Values);
                    shader.SetUniform(gl, "uProjection", _projectionMatrix.Values);
                }
                catch (InvalidOperationException ex)
                {
                    // Shader not available, use fallback rendering
                    // Console.WriteLine($"Shader not available: {ex.Message}"); // Reduced logging
                }
            }

            renderObject.Render(gl);
        }
    }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0)
            return;

        var aspect = (float)width / height;
        _projectionMatrix = Matrix4x4.CreatePerspective(
            (float)(Math.PI / 4), // 45 degrees FOV
            aspect,
            0.1f,
            100.0f
        );
    }

    public void Dispose()
    {
        foreach (var renderObject in _renderObjects)
        {
            renderObject.Dispose();
        }
        _renderObjects.Clear();
    }
}
