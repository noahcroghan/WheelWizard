using Avalonia.OpenGL;
using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D;

public interface IRendering3DSingletonService
{
    IRenderingEngine CreateRenderingEngine();
    IRenderObject CreateCube(GlInterface gl);
}

public class Rendering3DSingletonService : IRendering3DSingletonService
{
    public IRenderingEngine CreateRenderingEngine()
    {
        return new Services.OpenGLRenderingEngine();
    }

    public IRenderObject CreateCube(GlInterface gl)
    {
        return new Services.CubeRenderObject(gl);
    }
}
