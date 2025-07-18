using Microsoft.Xna.Framework;

namespace WheelWizard.Rendering3D.Domain;

/// <summary>
/// Interface for controlling the 3D camera with easy movement functions
/// </summary>
public interface I3DCamera
{
    /// <summary>
    /// Current camera position
    /// </summary>
    Vector3 Position { get; set; }

    /// <summary>
    /// Current camera target (what it's looking at)
    /// </summary>
    Vector3 Target { get; set; }

    /// <summary>
    /// Camera up vector
    /// </summary>
    Vector3 Up { get; set; }

    /// <summary>
    /// Field of view in radians
    /// </summary>
    float FieldOfView { get; set; }

    /// <summary>
    /// Near clipping plane distance
    /// </summary>
    float NearPlane { get; set; }

    /// <summary>
    /// Far clipping plane distance
    /// </summary>
    float FarPlane { get; set; }

    /// <summary>
    /// View matrix (calculated automatically)
    /// </summary>
    Matrix ViewMatrix { get; }

    /// <summary>
    /// Projection matrix (calculated automatically)
    /// </summary>
    Matrix ProjectionMatrix { get; }

    /// <summary>
    /// Moves the camera by the specified offset
    /// </summary>
    /// <param name="offset">Movement offset</param>
    void Move(Vector3 offset);

    /// <summary>
    /// Moves the camera to the specified position
    /// </summary>
    /// <param name="position">New position</param>
    void MoveTo(Vector3 position);

    /// <summary>
    /// Makes the camera look at the specified target
    /// </summary>
    /// <param name="target">Target position</param>
    /// <param name="up">Up vector (optional)</param>
    void LookAt(Vector3 target, Vector3? up = null);

    /// <summary>
    /// Orbits the camera around the target at the specified distance
    /// </summary>
    /// <param name="target">Target to orbit around</param>
    /// <param name="distance">Distance from target</param>
    /// <param name="horizontalAngle">Horizontal angle in radians</param>
    /// <param name="verticalAngle">Vertical angle in radians</param>
    void OrbitAround(Vector3 target, float distance, float horizontalAngle, float verticalAngle);

    /// <summary>
    /// Moves the camera forward/backward relative to its current orientation
    /// </summary>
    /// <param name="distance">Distance to move (positive = forward, negative = backward)</param>
    void MoveForward(float distance);

    /// <summary>
    /// Moves the camera left/right relative to its current orientation
    /// </summary>
    /// <param name="distance">Distance to move (positive = right, negative = left)</param>
    void MoveRight(float distance);

    /// <summary>
    /// Moves the camera up/down relative to its current orientation
    /// </summary>
    /// <param name="distance">Distance to move (positive = up, negative = down)</param>
    void MoveUp(float distance);

    /// <summary>
    /// Rotates the camera around its current position
    /// </summary>
    /// <param name="yaw">Horizontal rotation in radians</param>
    /// <param name="pitch">Vertical rotation in radians</param>
    void Rotate(float yaw, float pitch);

    /// <summary>
    /// Animates the camera to a new position over time
    /// </summary>
    /// <param name="targetPosition">Target position</param>
    /// <param name="duration">Animation duration in seconds</param>
    void AnimateToPosition(Vector3 targetPosition, float duration);

    /// <summary>
    /// Animates the camera to look at a new target over time
    /// </summary>
    /// <param name="targetLookAt">Target to look at</param>
    /// <param name="duration">Animation duration in seconds</param>
    void AnimateToLookAt(Vector3 targetLookAt, float duration);

    /// <summary>
    /// Updates the camera (called automatically by scene)
    /// </summary>
    /// <param name="gameTime">Game time information</param>
    void Update(GameTime gameTime);

    /// <summary>
    /// Updates the projection matrix when viewport changes
    /// </summary>
    /// <param name="aspectRatio">New aspect ratio</param>
    void UpdateProjection(float aspectRatio);
}
