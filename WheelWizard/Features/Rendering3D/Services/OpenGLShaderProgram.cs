using System.Runtime.InteropServices;
using System.Text;
using Avalonia.OpenGL;
using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D.Services;

public class OpenGLShaderProgram : IShaderProgram
{
    public uint ProgramId { get; private set; }
    private readonly GlInterface _gl;

    public OpenGLShaderProgram(GlInterface gl, string vertexShader, string fragmentShader)
    {
        _gl = gl;
        ProgramId = CreateShaderProgram(vertexShader, fragmentShader);
    }

    private uint CreateShaderProgram(string vertexShaderSource, string fragmentShaderSource)
    {
        var vertexShader = CompileShader(GlConsts.GL_VERTEX_SHADER, vertexShaderSource);
        var fragmentShader = CompileShader(GlConsts.GL_FRAGMENT_SHADER, fragmentShaderSource);

        var program = (uint)_gl.CreateProgram();
        _gl.AttachShader((int)program, (int)vertexShader);
        _gl.AttachShader((int)program, (int)fragmentShader);
        _gl.LinkProgram((int)program);

        // Check for linking errors
        var success = new int[1];
        unsafe
        {
            fixed (int* successPtr = success)
            {
                _gl.GetProgramiv((int)program, GlConsts.GL_LINK_STATUS, successPtr);
            }
        }
        if (success[0] == 0)
        {
            // Get error message
            var infoLogLength = new int[1];
            unsafe
            {
                fixed (int* lengthPtr = infoLogLength)
                {
                    _gl.GetProgramiv((int)program, GlConsts.GL_INFO_LOG_LENGTH, lengthPtr);
                }
            }

            if (infoLogLength[0] > 0)
            {
                var infoLog = new byte[infoLogLength[0]];
                unsafe
                {
                    fixed (byte* logPtr = infoLog)
                    {
                        _gl.GetProgramInfoLog((int)program, infoLogLength[0], out _, logPtr);
                    }
                }
                var errorMessage = Encoding.UTF8.GetString(infoLog);
                throw new Exception($"Shader program linking failed: {errorMessage}");
            }
            else
            {
                throw new Exception($"Shader program linking failed");
            }
        }

        _gl.DeleteShader((int)vertexShader);
        _gl.DeleteShader((int)fragmentShader);

        return program;
    }

    private uint CompileShader(int type, string source)
    {
        var shader = (uint)_gl.CreateShader(type);

        // For Avalonia OpenGL binding, we need to handle strings differently
        unsafe
        {
            var sourceBytes = Encoding.UTF8.GetBytes(source);
            fixed (byte* sourcePtr = sourceBytes)
            {
                var sourcePtrPtr = new IntPtr(&sourcePtr);
                _gl.ShaderSource((int)shader, 1, sourcePtrPtr, IntPtr.Zero);
                _gl.CompileShader((int)shader);
            }
        }

        // Check for compilation errors
        var success = new int[1];
        unsafe
        {
            fixed (int* successPtr = success)
            {
                _gl.GetShaderiv((int)shader, GlConsts.GL_COMPILE_STATUS, successPtr);
            }
        }
        if (success[0] == 0)
        {
            // Get error message
            var infoLogLength = new int[1];
            unsafe
            {
                fixed (int* lengthPtr = infoLogLength)
                {
                    _gl.GetShaderiv((int)shader, GlConsts.GL_INFO_LOG_LENGTH, lengthPtr);
                }
            }

            if (infoLogLength[0] > 0)
            {
                var infoLog = new byte[infoLogLength[0]];
                unsafe
                {
                    fixed (byte* logPtr = infoLog)
                    {
                        _gl.GetShaderInfoLog((int)shader, infoLogLength[0], out _, logPtr);
                    }
                }
                var errorMessage = Encoding.UTF8.GetString(infoLog);
                throw new Exception($"Shader compilation failed: {errorMessage}");
            }
            else
            {
                throw new Exception($"Shader compilation failed");
            }
        }

        return shader;
    }

    public void Use(GlInterface gl)
    {
        gl.UseProgram((int)ProgramId);
    }

    public void SetUniform(GlInterface gl, string name, float value)
    {
        var namePtr = Marshal.StringToHGlobalAnsi(name);
        try
        {
            var location = gl.GetUniformLocation((int)ProgramId, namePtr);
            if (location != -1)
            {
                gl.Uniform1f(location, value);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(namePtr);
        }
    }

    public void SetUniform(GlInterface gl, string name, float[] matrix)
    {
        var namePtr = Marshal.StringToHGlobalAnsi(name);
        try
        {
            var location = gl.GetUniformLocation((int)ProgramId, namePtr);
            if (location != -1)
            {
                // For matrix uniforms, we need to use the matrix version
                unsafe
                {
                    fixed (float* matrixPtr = matrix)
                    {
                        gl.UniformMatrix4fv(location, 1, false, matrixPtr);
                    }
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(namePtr);
        }
    }

    public void Dispose()
    {
        _gl.DeleteProgram((int)ProgramId);
    }
}
