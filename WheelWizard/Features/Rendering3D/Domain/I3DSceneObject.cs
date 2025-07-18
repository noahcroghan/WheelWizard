using Microsoft.Xna.Framework;

namespace WheelWizard.Rendering3D.Domain;

/// <summary>
/// Represents a 3D object in the scene with easy manipulation methods
/// </summary>
public interface I3DSceneObject
{
    /// <summary>
    /// Unique identifier for this object
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Type of the scene object
    /// </summary>
    SceneObjectType ObjectType { get; }

    /// <summary>
    /// Current position in 3D space
    /// </summary>
    Vector3 Position { get; set; }

    /// <summary>
    /// Current rotation in radians (X, Y, Z)
    /// </summary>
    Vector3 Rotation { get; set; }

    /// <summary>
    /// Current scale (X, Y, Z)
    /// </summary>
    Vector3 Scale { get; set; }

    /// <summary>
    /// Color tint for the object
    /// </summary>
    Color Color { get; set; }

    /// <summary>
    /// Whether the object is visible
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// World transformation matrix (calculated automatically)
    /// </summary>
    Matrix WorldMatrix { get; }

    /// <summary>
    /// Moves the object by the specified offset
    /// </summary>
    /// <param name="offset">Movement offset</param>
    void Move(Vector3 offset);

    /// <summary>
    /// Moves the object to the specified position
    /// </summary>
    /// <param name="position">New position</param>
    void MoveTo(Vector3 position);

    /// <summary>
    /// Rotates the object by the specified angles (in radians)
    /// </summary>
    /// <param name="rotation">Rotation offset</param>
    void Rotate(Vector3 rotation);

    /// <summary>
    /// Sets the object's rotation to the specified angles (in radians)
    /// </summary>
    /// <param name="rotation">New rotation</param>
    void RotateTo(Vector3 rotation);

    /// <summary>
    /// Scales the object by the specified factor
    /// </summary>
    /// <param name="scale">Scale factor</param>
    void ScaleBy(Vector3 scale);

    /// <summary>
    /// Sets the object's scale to the specified values
    /// </summary>
    /// <param name="scale">New scale</param>
    void ScaleTo(Vector3 scale);

    /// <summary>
    /// Looks at the specified target position
    /// </summary>
    /// <param name="target">Target position to look at</param>
    /// <param name="up">Up vector (optional, defaults to Vector3.Up)</param>
    void LookAt(Vector3 target, Vector3? up = null);

    /// <summary>
    /// Animates the object to a new position over time
    /// </summary>
    /// <param name="targetPosition">Target position</param>
    /// <param name="duration">Animation duration in seconds</param>
    void AnimateToPosition(Vector3 targetPosition, float duration);

    /// <summary>
    /// Animates the object to a new rotation over time
    /// </summary>
    /// <param name="targetRotation">Target rotation</param>
    /// <param name="duration">Animation duration in seconds</param>
    void AnimateToRotation(Vector3 targetRotation, float duration);

    /// <summary>
    /// Updates the object (called automatically by scene)
    /// </summary>
    /// <param name="gameTime">Game time information</param>
    void Update(GameTime gameTime);
}

/// <summary>
/// Types of built-in 3D objects that can be created
/// </summary>
public enum SceneObjectType
{
    Cube,
    Sphere,
    Cylinder,
    Plane,
    Pyramid,
    Model, // For loaded 3D models
}
