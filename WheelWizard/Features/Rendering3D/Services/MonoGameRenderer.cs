using Avalonia;
using Avalonia.Controls;
using AvaloniaInside.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WheelWizard.Rendering3D.Domain;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;

namespace WheelWizard.Rendering3D.Services;

public class MonoGameRenderer : IMonoGameRenderer, IDisposable
{
    private MonoGameControl? _monoGameControl;
    private Game3DRenderer? _game3DRenderer;
    private bool _isDisposed;
    private Vector2 _dimensions;

    public bool IsRunning => _game3DRenderer?.IsActive == true;
    public Vector2 Dimensions => _dimensions;

    public void Initialize(int width, int height)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MonoGameRenderer));

        _dimensions = new Vector2(width, height);

        _game3DRenderer = new Game3DRenderer();
        _monoGameControl = new MonoGameControl();
        _monoGameControl.Game = _game3DRenderer;

        Console.WriteLine($"MonoGame renderer initialized with dimensions: {width}x{height}");
    }

    public MonoGameControl GetControl()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MonoGameRenderer));

        if (_monoGameControl == null)
            throw new InvalidOperationException("Renderer not initialized. Call Initialize() first.");

        return _monoGameControl;
    }

    public void Start()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MonoGameRenderer));

        if (_game3DRenderer == null)
            throw new InvalidOperationException("Renderer not initialized. Call Initialize() first.");

        _game3DRenderer.RunOneFrame();
        Console.WriteLine("MonoGame renderer started");
    }

    public void Stop()
    {
        if (_game3DRenderer != null)
        {
            _game3DRenderer.Exit();
            Console.WriteLine("MonoGame renderer stopped");
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        Stop();

        _game3DRenderer?.Dispose();
        _monoGameControl = null;
        _game3DRenderer = null;

        _isDisposed = true;
        Console.WriteLine("MonoGame renderer disposed");
    }
}

public class Game3DRenderer : Game
{
    private GraphicsDeviceManager? _graphics;
    private BasicEffect? _basicEffect;
    private VertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;
    private XnaMatrix _world;
    private XnaMatrix _view;
    private XnaMatrix _projection;
    private float _rotation;

    // Cube vertices with colors
    private readonly VertexPositionColor[] _vertices =
    [
        // Front face
        new(new Vector3(-1, -1, 1), Microsoft.Xna.Framework.Color.Red),
        new(new Vector3(1, -1, 1), Microsoft.Xna.Framework.Color.Green),
        new(new Vector3(1, 1, 1), Microsoft.Xna.Framework.Color.Blue),
        new(new Vector3(-1, 1, 1), Microsoft.Xna.Framework.Color.Yellow),
        // Back face
        new(new Vector3(-1, -1, -1), Microsoft.Xna.Framework.Color.Magenta),
        new(new Vector3(1, -1, -1), Microsoft.Xna.Framework.Color.Cyan),
        new(new Vector3(1, 1, -1), Microsoft.Xna.Framework.Color.Gray),
        new(new Vector3(-1, 1, -1), Microsoft.Xna.Framework.Color.Orange),
    ];

    // Cube indices
    private readonly short[] _indices =
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

    public Game3DRenderer()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Set up camera matrices
        _view = XnaMatrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
        UpdateProjection();

        Console.WriteLine("Game3DRenderer initialized successfully");
    }

    protected override void LoadContent()
    {
        // Create basic effect for rendering
        _basicEffect = new BasicEffect(GraphicsDevice);
        _basicEffect.VertexColorEnabled = true;

        // Create vertex buffer
        _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), _vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);

        // Create index buffer
        _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);

        Console.WriteLine("Game3DRenderer content loaded successfully");
    }

    private void UpdateProjection()
    {
        if (GraphicsDevice != null)
        {
            var viewport = GraphicsDevice.Viewport;
            var aspectRatio = viewport.Width / (float)viewport.Height;
            _projection = XnaMatrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.1f, 100f);
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // Rotate the cube
        _rotation += (float)gameTime.ElapsedGameTime.TotalSeconds;
        _world = XnaMatrix.CreateRotationY(_rotation) * XnaMatrix.CreateRotationX(_rotation * 0.5f);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_basicEffect == null || _vertexBuffer == null || _indexBuffer == null)
            return;

        // Clear the screen
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

        // Set up the effect matrices
        _basicEffect.World = _world;
        _basicEffect.View = _view;
        _basicEffect.Projection = _projection;

        // Set vertex buffer and index buffer
        GraphicsDevice.SetVertexBuffer(_vertexBuffer);
        GraphicsDevice.Indices = _indexBuffer;

        // Draw the cube
        foreach (var pass in _basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices.Length / 3);
        }

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        _basicEffect?.Dispose();
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        base.UnloadContent();
    }
}
