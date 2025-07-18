using Avalonia;
using Avalonia.Controls;
using AvaloniaInside.MonoGame;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WheelWizard.Rendering3D.Domain;
using WheelWizard.Rendering3D.Services;
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

    /// <summary>
    /// Gets the 3D scene for easy object manipulation
    /// </summary>
    public I3DScene? Scene => _game3DRenderer?.Scene;

    /// <summary>
    /// Event fired during the update loop for custom animations
    /// </summary>
    public event Action<GameTime>? UpdateAnimation;

    /// <summary>
    /// Internal method to fire the update animation event
    /// </summary>
    /// <param name="gameTime">Current game time</param>
    internal void FireUpdateAnimation(GameTime gameTime)
    {
        try
        {
            UpdateAnimation?.Invoke(gameTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in FireUpdateAnimation");
        }
    }

    public MonoGameRenderer(ILogger<MonoGameRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Initialize(int width, int height)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(MonoGameRenderer));

        try
        {
            _dimensions = new Vector2(width, height);

            _game3DRenderer = new Game3DRenderer(_logger, this);
            _monoGameControl = new MonoGameControl();
            _monoGameControl.Game = _game3DRenderer;

            _logger.LogInformation("MonoGame renderer initialized with dimensions: {Width}x{Height}", width, height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing MonoGame renderer");
            throw;
        }
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

        // Let the MonoGame control handle its own initialization
        // The control will automatically call Initialize() and LoadContent() when needed
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
    private I3DScene? _scene;
    private readonly ILogger<MonoGameRenderer> _logger;
    private readonly MonoGameRenderer _parentRenderer;

    /// <summary>
    /// Gets the 3D scene for easy object manipulation
    /// </summary>
    public I3DScene? Scene => _scene;

    public Game3DRenderer(ILogger<MonoGameRenderer> logger, MonoGameRenderer parentRenderer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _parentRenderer = parentRenderer ?? throw new ArgumentNullException(nameof(parentRenderer));
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";

        // Configure graphics device for transparency support
        _graphics.PreferredBackBufferFormat = SurfaceFormat.Color;
        _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
    }

    protected override void Initialize()
    {
        try
        {
            base.Initialize();
            _logger.LogInformation("Game3DRenderer initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Game3DRenderer.Initialize");
            throw;
        }
    }

    protected override void LoadContent()
    {
        try
        {
            // Ensure graphics device is ready
            if (GraphicsDevice == null)
            {
                _logger.LogError("GraphicsDevice is null in LoadContent");
                return;
            }

            // Create basic effect for rendering
            _basicEffect = new BasicEffect(GraphicsDevice);
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.LightingEnabled = true;

            // Configure depth testing and backface culling
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

            // Configure alpha blending for transparency
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // Create the 3D scene
            _scene = new Scene3D(GraphicsDevice, Microsoft.Extensions.Logging.LoggerFactory.Create(builder => { }).CreateLogger<Scene3D>());

            // Add a demo rotating cube to the scene
            var cube = _scene.AddObject("demo-cube", SceneObjectType.Cube, new Vector3(0, 0, 0));

            // Set up basic lighting
            _scene.Lighting.SetupSunLighting(
                Microsoft.Xna.Framework.Color.White,
                Vector3.Normalize(new Vector3(-1, -1, -1)),
                new Microsoft.Xna.Framework.Color(0.2f, 0.2f, 0.3f)
            );

            _logger.LogInformation("Game3DRenderer content loaded successfully with scene system");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Game3DRenderer content");
            throw;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        try
        {
            // Only update the scene if it has been initialized and is valid
            if (_scene != null && GraphicsDevice != null)
            {
                _scene.Update(gameTime);
            }

            // Fire the update animation event for external animations
            // This is safe to call even if the scene isn't ready yet
            _parentRenderer?.FireUpdateAnimation(gameTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Game3DRenderer.Update");
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        try
        {
            if (_basicEffect == null || _scene == null || GraphicsDevice == null)
                return;

            // Clear the screen and depth buffer with transparent background
            GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);

            // Ensure proper render states
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            // Set up lighting on the effect
            if (_scene.Lighting.LightingEnabled)
            {
                _basicEffect.LightingEnabled = true;
                _basicEffect.AmbientLightColor = _scene.Lighting.AmbientColor.ToVector3();
                _basicEffect.DirectionalLight0.Enabled = true;
                _basicEffect.DirectionalLight0.DiffuseColor = _scene.Lighting.DirectionalColor.ToVector3();
                _basicEffect.DirectionalLight0.Direction = _scene.Lighting.DirectionalDirection;
            }
            else
            {
                _basicEffect.LightingEnabled = false;
            }

            // Draw all visible objects in the scene
            foreach (var obj in _scene.Objects.Where(o => o.Visible))
            {
                if (obj is SceneObject3D sceneObj)
                {
                    sceneObj.Draw(_basicEffect, _scene.Camera.ViewMatrix, _scene.Camera.ProjectionMatrix);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Game3DRenderer.Draw");
        }

        base.Draw(gameTime);
    }

    protected override void UnloadContent()
    {
        try
        {
            _basicEffect?.Dispose();
            _scene?.ClearScene();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Game3DRenderer.UnloadContent");
        }

        base.UnloadContent();
    }
}
