using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D.Services;

/// <summary>
/// Implementation of a 3D camera with easy movement and control
/// </summary>
public class Camera3D : I3DCamera
{
    private Vector3 _position = new(0, 0, 5);
    private Vector3 _target = Vector3.Zero;
    private Vector3 _up = Vector3.Up;
    private float _fieldOfView = MathHelper.PiOver4;
    private float _nearPlane = 0.1f;
    private float _farPlane = 100f;
    private float _aspectRatio = 1.0f;

    private Matrix _viewMatrix;
    private Matrix _projectionMatrix;
    private bool _viewMatrixDirty = true;
    private bool _projectionMatrixDirty = true;

    // Animation fields
    private Vector3? _animationStartPosition;
    private Vector3? _animationTargetPosition;
    private Vector3? _animationStartLookAt;
    private Vector3? _animationTargetLookAt;
    private float _animationDuration;
    private float _animationElapsed;
    private bool _isAnimatingPosition;
    private bool _isAnimatingLookAt;

    private readonly GraphicsDevice _graphicsDevice;

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            _viewMatrixDirty = true;
        }
    }

    public Vector3 Target
    {
        get => _target;
        set
        {
            _target = value;
            _viewMatrixDirty = true;
        }
    }

    public Vector3 Up
    {
        get => _up;
        set
        {
            _up = value;
            _viewMatrixDirty = true;
        }
    }

    public float FieldOfView
    {
        get => _fieldOfView;
        set
        {
            _fieldOfView = value;
            _projectionMatrixDirty = true;
        }
    }

    public float NearPlane
    {
        get => _nearPlane;
        set
        {
            _nearPlane = value;
            _projectionMatrixDirty = true;
        }
    }

    public float FarPlane
    {
        get => _farPlane;
        set
        {
            _farPlane = value;
            _projectionMatrixDirty = true;
        }
    }

    public Matrix ViewMatrix
    {
        get
        {
            if (_viewMatrixDirty)
            {
                _viewMatrix = Matrix.CreateLookAt(_position, _target, _up);
                _viewMatrixDirty = false;
            }
            return _viewMatrix;
        }
    }

    public Matrix ProjectionMatrix
    {
        get
        {
            if (_projectionMatrixDirty)
            {
                _projectionMatrix = Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearPlane, _farPlane);
                _projectionMatrixDirty = false;
            }
            return _projectionMatrix;
        }
    }

    public Camera3D(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        UpdateAspectRatio();
    }

    private void UpdateAspectRatio()
    {
        var viewport = _graphicsDevice.Viewport;
        _aspectRatio = viewport.Width / (float)viewport.Height;
        _projectionMatrixDirty = true;
    }

    public void Move(Vector3 offset)
    {
        Position += offset;
        Target += offset; // Move target with camera to maintain look direction
    }

    public void MoveTo(Vector3 position)
    {
        var offset = position - Position;
        Position = position;
        Target += offset; // Move target with camera to maintain look direction
    }

    public void LookAt(Vector3 target, Vector3? up = null)
    {
        Target = target;
        if (up.HasValue)
            Up = up.Value;
    }

    public void OrbitAround(Vector3 target, float distance, float horizontalAngle, float verticalAngle)
    {
        // Clamp vertical angle to avoid gimbal lock
        verticalAngle = MathHelper.Clamp(verticalAngle, -MathHelper.PiOver2 + 0.1f, MathHelper.PiOver2 - 0.1f);

        var x = distance * MathF.Cos(verticalAngle) * MathF.Cos(horizontalAngle);
        var y = distance * MathF.Sin(verticalAngle);
        var z = distance * MathF.Cos(verticalAngle) * MathF.Sin(horizontalAngle);

        Position = target + new Vector3(x, y, z);
        Target = target;
    }

    public void MoveForward(float distance)
    {
        var forward = Vector3.Normalize(Target - Position);
        Move(forward * distance);
    }

    public void MoveRight(float distance)
    {
        var forward = Vector3.Normalize(Target - Position);
        var right = Vector3.Normalize(Vector3.Cross(forward, Up));
        Move(right * distance);
    }

    public void MoveUp(float distance)
    {
        Move(Up * distance);
    }

    public void Rotate(float yaw, float pitch)
    {
        var forward = Vector3.Normalize(Target - Position);
        var right = Vector3.Normalize(Vector3.Cross(forward, Up));
        var up = Vector3.Cross(right, forward);

        // Apply yaw (horizontal rotation)
        var yawMatrix = Matrix.CreateFromAxisAngle(up, yaw);
        forward = Vector3.Transform(forward, yawMatrix);

        // Apply pitch (vertical rotation)
        var pitchMatrix = Matrix.CreateFromAxisAngle(right, pitch);
        forward = Vector3.Transform(forward, pitchMatrix);

        // Update target based on new forward direction
        var distance = Vector3.Distance(Position, Target);
        Target = Position + forward * distance;
    }

    public void AnimateToPosition(Vector3 targetPosition, float duration)
    {
        _animationStartPosition = Position;
        _animationTargetPosition = targetPosition;
        _animationDuration = duration;
        _animationElapsed = 0;
        _isAnimatingPosition = true;
    }

    public void AnimateToLookAt(Vector3 targetLookAt, float duration)
    {
        _animationStartLookAt = Target;
        _animationTargetLookAt = targetLookAt;
        _animationDuration = duration;
        _animationElapsed = 0;
        _isAnimatingLookAt = true;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Handle position animation
        if (_isAnimatingPosition && _animationStartPosition.HasValue && _animationTargetPosition.HasValue)
        {
            _animationElapsed += deltaTime;
            float progress = Math.Min(_animationElapsed / _animationDuration, 1.0f);

            Position = Vector3.Lerp(_animationStartPosition.Value, _animationTargetPosition.Value, progress);

            if (progress >= 1.0f)
            {
                _isAnimatingPosition = false;
                _animationStartPosition = null;
                _animationTargetPosition = null;
            }
        }

        // Handle look-at animation
        if (_isAnimatingLookAt && _animationStartLookAt.HasValue && _animationTargetLookAt.HasValue)
        {
            _animationElapsed += deltaTime;
            float progress = Math.Min(_animationElapsed / _animationDuration, 1.0f);

            Target = Vector3.Lerp(_animationStartLookAt.Value, _animationTargetLookAt.Value, progress);

            if (progress >= 1.0f)
            {
                _isAnimatingLookAt = false;
                _animationStartLookAt = null;
                _animationTargetLookAt = null;
            }
        }

        // Update aspect ratio if viewport changed
        var viewport = _graphicsDevice.Viewport;
        var newAspectRatio = viewport.Width / (float)viewport.Height;
        if (Math.Abs(newAspectRatio - _aspectRatio) > 0.001f)
        {
            _aspectRatio = newAspectRatio;
            _projectionMatrixDirty = true;
        }
    }

    public void UpdateProjection(float aspectRatio)
    {
        _aspectRatio = aspectRatio;
        _projectionMatrixDirty = true;
    }
}
