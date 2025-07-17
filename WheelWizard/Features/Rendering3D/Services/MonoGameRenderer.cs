using Avalonia;
using Avalonia.Controls;
using AvaloniaInside.MonoGame;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<MonoGameRenderer> _logger;

    public bool IsRunning => _game3DRenderer?.IsActive == true;
    public Vector2 Dimensions => _dimensions;

    public MonoGameRenderer(ILogger<MonoGameRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Initialize(int width, int height)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MonoGameRenderer));

        _dimensions = new Vector2(width, height);

        _game3DRenderer = new Game3DRenderer(_logger);
        _monoGameControl = new MonoGameControl();
        _monoGameControl.Game = _game3DRenderer;

        _logger.LogInformation("MonoGame renderer initialized with dimensions: {Width}x{Height}", width, height);
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
        _logger.LogInformation("MonoGame renderer started");
    }

    public void Stop()
    {
        if (_game3DRenderer != null)
        {
            _game3DRenderer.Exit();
            _logger.LogInformation("MonoGame renderer stopped");
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
        _logger.LogInformation("MonoGame renderer disposed");
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
    private readonly ILogger<MonoGameRenderer> _logger;

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

    // Cube indices - corrected winding order for proper backface culling
    private readonly short[] _indices =
    [
        // Front face (Z = 1)
        0,
        2,
        1, // Triangle 1
        0,
        3,
        2, // Triangle 2
        // Back face (Z = -1)
        5,
        7,
        4, // Triangle 1
        5,
        6,
        7, // Triangle 2
        // Left face (X = -1)
        4,
        3,
        0, // Triangle 1
        4,
        7,
        3, // Triangle 2
        // Right face (X = 1)
        1,
        6,
        5, // Triangle 1
        1,
        2,
        6, // Triangle 2
        // Top face (Y = 1)
        3,
        6,
        2, // Triangle 1
        3,
        7,
        6, // Triangle 2
        // Bottom face (Y = -1)
        4,
        1,
        5, // Triangle 1
        4,
        0,
        1, // Triangle 2
    ];

    public Game3DRenderer(ILogger<MonoGameRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        // Configure graphics device for transparency support
        _graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
    }

    protected override void Initialize()
    {
        base.Initialize();

        // Set up camera matrices
        _view = XnaMatrix.CreateLookAt(new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
        UpdateProjection();

        _logger.LogInformation("Game3DRenderer initialized successfully");
    }

    protected override void LoadContent()
    {
        // Create basic effect for rendering
        _basicEffect = new BasicEffect(GraphicsDevice);
        _basicEffect.VertexColorEnabled = true;

        // Configure depth testing and backface culling
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        // Configure alpha blending for transparency
        GraphicsDevice.BlendState = BlendState.AlphaBlend;

        // Create vertex buffer
        _vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), _vertices.Length, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(_vertices);

        // Create index buffer
        _indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.WriteOnly);
        _indexBuffer.SetData(_indices);

        _logger.LogInformation("Game3DRenderer content loaded successfully");
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

        // Clear the screen and depth buffer with transparent background
        GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);

        // Ensure proper render states
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        GraphicsDevice.BlendState = BlendState.AlphaBlend;

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
