using Microsoft.Xna.Framework;

namespace WheelWizard.Rendering3D.Domain;

public interface IMonoGameRenderer
{
    /// <summary>
    /// Event fired during the update loop for custom animations
    /// </summary>
    event Action<GameTime>? UpdateAnimation;

    /// <summary>
    /// Initializes the MonoGame renderer with the specified dimensions
    /// </summary>
    /// <param name="width">Width of the rendering area</param>
    /// <param name="height">Height of the rendering area</param>
    void Initialize(int width, int height);

    /// <summary>
    /// Gets the MonoGame control that can be embedded in Avalonia UI
    /// </summary>
    /// <returns>The MonoGame control</returns>
    AvaloniaInside.MonoGame.MonoGameControl GetControl();

    /// <summary>
    /// Starts the rendering loop
    /// </summary>
    void Start();

    /// <summary>
    /// Stops the rendering loop
    /// </summary>
    void Stop();

    /// <summary>
    /// Disposes of the renderer and its resources
    /// </summary>
    void Dispose();

    /// <summary>
    /// Gets whether the renderer is currently running
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the current dimensions of the renderer
    /// </summary>
    Vector2 Dimensions { get; }

    /// <summary>
    /// Gets the 3D scene for easy object manipulation
    /// </summary>
    I3DScene? Scene { get; }
}
