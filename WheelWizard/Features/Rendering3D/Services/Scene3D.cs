using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D.Services;

/// <summary>
/// High-level 3D scene management implementation
/// </summary>
public class Scene3D : I3DScene
{
    private readonly Dictionary<string, I3DSceneObject> _objects = new();
    private readonly GraphicsDevice _graphicsDevice;
    private readonly ILogger<Scene3D> _logger;

    public IReadOnlyList<I3DSceneObject> Objects => _objects.Values.ToList();
    public I3DCamera Camera { get; }
    public I3DLighting Lighting { get; }

    public Scene3D(GraphicsDevice graphicsDevice, ILogger<Scene3D> logger)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Camera = new Camera3D(_graphicsDevice);
        Lighting = new Lighting3D();

        _logger.LogInformation("3D Scene initialized");
    }

    public I3DSceneObject AddObject(
        string objectId,
        SceneObjectType objectType,
        Vector3? position = null,
        Vector3? rotation = null,
        Vector3? scale = null
    )
    {
        if (_objects.ContainsKey(objectId))
        {
            _logger.LogWarning("Object with ID '{ObjectId}' already exists, removing old one", objectId);
            RemoveObject(objectId);
        }

        var sceneObject = new SceneObject3D(objectId, objectType, _graphicsDevice, _logger)
        {
            Position = position ?? Vector3.Zero,
            Rotation = rotation ?? Vector3.Zero,
            Scale = scale ?? Vector3.One,
        };

        _objects[objectId] = sceneObject;
        _logger.LogInformation("Added {ObjectType} object '{ObjectId}' to scene", objectType, objectId);

        return sceneObject;
    }

    public I3DSceneObject AddModel(
        string objectId,
        string modelPath,
        Vector3? position = null,
        Vector3? rotation = null,
        Vector3? scale = null
    )
    {
        if (_objects.ContainsKey(objectId))
        {
            _logger.LogWarning("Object with ID '{ObjectId}' already exists, removing old one", objectId);
            RemoveObject(objectId);
        }

        var sceneObject = new SceneObject3D(objectId, SceneObjectType.Model, _graphicsDevice, _logger, modelPath)
        {
            Position = position ?? Vector3.Zero,
            Rotation = rotation ?? Vector3.Zero,
            Scale = scale ?? Vector3.One,
        };

        _objects[objectId] = sceneObject;
        _logger.LogInformation("Added model object '{ObjectId}' from '{ModelPath}' to scene", objectId, modelPath);

        return sceneObject;
    }

    public I3DSceneObject? GetObject(string objectId)
    {
        return _objects.TryGetValue(objectId, out var obj) ? obj : null;
    }

    public bool RemoveObject(string objectId)
    {
        if (_objects.TryGetValue(objectId, out var obj))
        {
            if (obj is IDisposable disposable)
                disposable.Dispose();

            _objects.Remove(objectId);
            _logger.LogInformation("Removed object '{ObjectId}' from scene", objectId);
            return true;
        }

        return false;
    }

    public void ClearScene()
    {
        foreach (var obj in _objects.Values)
        {
            if (obj is IDisposable disposable)
                disposable.Dispose();
        }

        _objects.Clear();
        _logger.LogInformation("Cleared all objects from scene");
    }

    public void Update(GameTime gameTime)
    {
        Camera.Update(gameTime);

        foreach (var obj in _objects.Values)
        {
            obj.Update(gameTime);
        }
    }

    public void Draw(GameTime gameTime)
    {
        foreach (var obj in _objects.Values.Where(o => o.Visible))
        {
            // Drawing is handled by the individual objects
            // This method is called by the main renderer
        }
    }
}
