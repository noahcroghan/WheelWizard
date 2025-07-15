using Avalonia.OpenGL;
using WheelWizard.Rendering3D.Domain;
using WheelWizard.Rendering3D.Services;

namespace WheelWizard.Features.Rendering3D;

public class TestRender
{
    public static void TestBasicRendering(GlInterface gl)
    {
        // Test shader program creation - OpenGLShaderProgram requires vertex and fragment shader strings
        var vertexShader =
            @"
            #version 330 core
            layout(location = 0) in vec3 aPos;
            void main() {
                gl_Position = vec4(aPos, 1.0);
            }";

        var fragmentShader =
            @"
            #version 330 core
            out vec4 FragColor;
            void main() {
                FragColor = vec4(1.0, 0.0, 0.0, 1.0);
            }";

        // Create shader program with required parameters
        var shaderProgram = new OpenGLShaderProgram(gl, vertexShader, fragmentShader);

        // Test cube creation
        var cubeRenderer = new CubeRenderObject(gl);
        cubeRenderer.Initialize(gl);

        // Test rendering engine - OpenGLRenderingEngine doesn't have a constructor that takes GlInterface
        var renderingEngine = new OpenGLRenderingEngine();
        renderingEngine.Initialize(gl);

        // Add cube to scene
        renderingEngine.AddRenderObject(cubeRenderer);

        // Test render call - Render method requires gl, frameBuffer, width, and height parameters
        renderingEngine.Render(gl, 0, 800, 600);

        // Cleanup
        renderingEngine.Dispose();
    }
}
