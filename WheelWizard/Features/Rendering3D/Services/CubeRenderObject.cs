using System.Runtime.InteropServices;
using Avalonia.OpenGL;
using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D.Services;

public class CubeRenderObject : IRenderObject
{
    private uint _vbo;
    private uint _ebo;
    private IShaderProgram? _shader;
    private readonly GlInterface _gl;
    private bool _initialized = false;

    // Cube vertices (position + color)
    private static readonly float[] Vertices =
    [
        // Front face
        -0.5f,
        -0.5f,
        0.5f,
        1.0f,
        0.0f,
        0.0f, // Bottom-left (Red)
        0.5f,
        -0.5f,
        0.5f,
        0.0f,
        1.0f,
        0.0f, // Bottom-right (Green)
        0.5f,
        0.5f,
        0.5f,
        0.0f,
        0.0f,
        1.0f, // Top-right (Blue)
        -0.5f,
        0.5f,
        0.5f,
        1.0f,
        1.0f,
        0.0f, // Top-left (Yellow)
        // Back face
        -0.5f,
        -0.5f,
        -0.5f,
        1.0f,
        0.0f,
        1.0f, // Bottom-left (Magenta)
        0.5f,
        -0.5f,
        -0.5f,
        0.0f,
        1.0f,
        1.0f, // Bottom-right (Cyan)
        0.5f,
        0.5f,
        -0.5f,
        0.5f,
        0.5f,
        0.5f, // Top-right (Gray)
        -0.5f,
        0.5f,
        -0.5f,
        1.0f,
        0.5f,
        0.0f, // Top-left (Orange)
    ];

    // Cube indices
    private static readonly ushort[] Indices =
    [
        // Front face
        0,
        1,
        2,
        2,
        3,
        0,
        // Back face
        4,
        5,
        6,
        6,
        7,
        4,
        // Left face
        7,
        3,
        0,
        0,
        4,
        7,
        // Right face
        1,
        5,
        6,
        6,
        2,
        1,
        // Top face
        3,
        2,
        6,
        6,
        7,
        3,
        // Bottom face
        0,
        1,
        5,
        5,
        4,
        0,
    ];

    public CubeRenderObject(GlInterface gl)
    {
        _gl = gl;
    }

    public void Initialize(GlInterface gl)
    {
        if (_initialized)
            return;

        // Create simple shader program for OpenGL ES 3.0 compatibility
        var vertexShader = """
                #version 300 es
                precision mediump float;
                
                in vec3 aPosition;
                in vec3 aColor;
                
                uniform mat4 uModel;
                uniform mat4 uView;
                uniform mat4 uProjection;
                
                out vec3 vColor;
                
                void main()
                {
                    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
                    vColor = aColor;
                }
            """;

        var fragmentShader = """
                #version 300 es
                precision mediump float;
                
                in vec3 vColor;
                out vec4 FragColor;
                
                void main()
                {
                    FragColor = vec4(vColor, 1.0);
                }
            """;

        try
        {
            _shader = new OpenGLShaderProgram(gl, vertexShader, fragmentShader);
            Console.WriteLine("Shader program created successfully");

            // Bind attributes for OpenGL ES 3.0
            var positionName = Marshal.StringToHGlobalAnsi("aPosition");
            var colorName = Marshal.StringToHGlobalAnsi("aColor");
            try
            {
                gl.BindAttribLocation((int)_shader.ProgramId, 0, positionName);
                gl.BindAttribLocation((int)_shader.ProgramId, 1, colorName);
            }
            finally
            {
                Marshal.FreeHGlobal(positionName);
                Marshal.FreeHGlobal(colorName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Shader creation failed: {ex.Message}");
            // Fall back to immediate mode rendering
            _shader = null;
        }

        // Generate VBO and EBO
        var vboArray = new uint[1];
        var eboArray = new uint[1];

        unsafe
        {
            fixed (uint* vboPtr = vboArray)
            fixed (uint* eboPtr = eboArray)
            {
                gl.GenBuffers(1, (int*)vboPtr);
                gl.GenBuffers(1, (int*)eboPtr);
            }
        }

        _vbo = vboArray[0];
        _ebo = eboArray[0];

        // Upload vertex data
        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, (int)_vbo);

        // Create GCHandle for vertex data
        var vertexHandle = GCHandle.Alloc(Vertices, GCHandleType.Pinned);
        try
        {
            gl.BufferData(
                GlConsts.GL_ARRAY_BUFFER,
                Vertices.Length * sizeof(float),
                vertexHandle.AddrOfPinnedObject(),
                GlConsts.GL_STATIC_DRAW
            );
        }
        finally
        {
            vertexHandle.Free();
        }

        // Upload index data
        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, (int)_ebo);

        // Create GCHandle for index data
        var indexHandle = GCHandle.Alloc(Indices, GCHandleType.Pinned);
        try
        {
            gl.BufferData(
                GlConsts.GL_ELEMENT_ARRAY_BUFFER,
                Indices.Length * sizeof(ushort),
                indexHandle.AddrOfPinnedObject(),
                GlConsts.GL_STATIC_DRAW
            );
        }
        finally
        {
            indexHandle.Free();
        }

        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, 0);
        _initialized = true;
    }

    public IShaderProgram GetShader()
    {
        if (_shader == null)
        {
            throw new InvalidOperationException("Shader not available. Using fallback rendering mode.");
        }
        return _shader;
    }

    public void Render(GlInterface gl)
    {
        if (!_initialized)
            return;
        if (_shader == null)
        {
            return;
        }

        // Use shader-based rendering
        _shader.Use(gl);

        // Bind buffers
        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, (int)_vbo);
        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, (int)_ebo);

        // Set up vertex attributes for OpenGL ES 3.0
        gl.VertexAttribPointer(0, 3, GlConsts.GL_FLOAT, 0, 6 * sizeof(float), IntPtr.Zero);
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(1, 3, GlConsts.GL_FLOAT, 0, 6 * sizeof(float), new IntPtr(3 * sizeof(float)));
        gl.EnableVertexAttribArray(1);

        // Draw the cube
        gl.DrawElements(GlConsts.GL_TRIANGLES, Indices.Length, GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        // Clean up
        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    public void Dispose()
    {
        if (_initialized)
        {
            var bufferArray = new uint[] { _vbo, _ebo };

            unsafe
            {
                fixed (uint* bufferPtr = bufferArray)
                {
                    _gl.DeleteBuffers(2, (int*)bufferPtr);
                }
            }

            _shader?.Dispose();
            _initialized = false;
        }
    }
}
