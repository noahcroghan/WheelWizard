using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WheelWizard.Rendering3D.Domain;

namespace WheelWizard.Rendering3D.Services;

/// <summary>
/// Implementation of a 3D scene object with easy manipulation
/// </summary>
public class SceneObject3D : I3DSceneObject, IDisposable
{
    private Vector3 _position = Vector3.Zero;
    private Vector3 _rotation = Vector3.Zero;
    private Vector3 _scale = Vector3.One;
    private Matrix _worldMatrix = Matrix.Identity;
    private bool _worldMatrixDirty = true;

    // Animation fields
    private Vector3? _animationStartPosition;
    private Vector3? _animationTargetPosition;
    private Vector3? _animationStartRotation;
    private Vector3? _animationTargetRotation;
    private float _animationDuration;
    private float _animationElapsed;
    private bool _isAnimatingPosition;
    private bool _isAnimatingRotation;

    private readonly GraphicsDevice _graphicsDevice;
    private readonly ILogger _logger;
    private readonly string? _modelPath;

    // Geometry data
    private VertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;
    private BasicEffect? _basicEffect;
    private VertexPositionColor[]? _vertices;
    private short[]? _indices;

    public string Id { get; }
    public SceneObjectType ObjectType { get; }
    public Color Color { get; set; } = Color.White;
    public bool Visible { get; set; } = true;

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            _worldMatrixDirty = true;
        }
    }

    public Vector3 Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _worldMatrixDirty = true;
        }
    }

    public Vector3 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _worldMatrixDirty = true;
        }
    }

    public Matrix WorldMatrix
    {
        get
        {
            if (_worldMatrixDirty)
            {
                _worldMatrix =
                    Matrix.CreateScale(_scale)
                    * Matrix.CreateRotationX(_rotation.X)
                    * Matrix.CreateRotationY(_rotation.Y)
                    * Matrix.CreateRotationZ(_rotation.Z)
                    * Matrix.CreateTranslation(_position);
                _worldMatrixDirty = false;
            }
            return _worldMatrix;
        }
    }

    public SceneObject3D(string id, SceneObjectType objectType, GraphicsDevice graphicsDevice, ILogger logger, string? modelPath = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        ObjectType = objectType;
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _modelPath = modelPath;

        InitializeGeometry();
    }

    private void InitializeGeometry()
    {
        if (ObjectType == SceneObjectType.Model && !string.IsNullOrEmpty(_modelPath))
        {
            // TODO: Implement model loading
            _logger.LogWarning("Model loading not yet implemented for '{ModelPath}', using cube instead", _modelPath);
            CreateCubeGeometry();
        }
        else
        {
            switch (ObjectType)
            {
                case SceneObjectType.Cube:
                    CreateCubeGeometry();
                    break;
                case SceneObjectType.Sphere:
                    CreateSphereGeometry();
                    break;
                case SceneObjectType.Cylinder:
                    CreateCylinderGeometry();
                    break;
                case SceneObjectType.Plane:
                    CreatePlaneGeometry();
                    break;
                case SceneObjectType.Pyramid:
                    CreatePyramidGeometry();
                    break;
                default:
                    CreateCubeGeometry();
                    break;
            }
        }

        CreateBuffers();
        CreateEffect();
    }

    private void CreateCubeGeometry()
    {
        _vertices = new VertexPositionColor[]
        {
            // Front face
            new(new Vector3(-0.5f, -0.5f, 0.5f), Color.Red),
            new(new Vector3(0.5f, -0.5f, 0.5f), Color.Green),
            new(new Vector3(0.5f, 0.5f, 0.5f), Color.Blue),
            new(new Vector3(-0.5f, 0.5f, 0.5f), Color.Yellow),
            // Back face
            new(new Vector3(-0.5f, -0.5f, -0.5f), Color.Magenta),
            new(new Vector3(0.5f, -0.5f, -0.5f), Color.Cyan),
            new(new Vector3(0.5f, 0.5f, -0.5f), Color.Gray),
            new(new Vector3(-0.5f, 0.5f, -0.5f), Color.Orange),
        };

        _indices = new short[]
        {
            // Front face
            0,
            2,
            1,
            0,
            3,
            2,
            // Back face
            5,
            7,
            4,
            5,
            6,
            7,
            // Left face
            4,
            3,
            0,
            4,
            7,
            3,
            // Right face
            1,
            6,
            5,
            1,
            2,
            6,
            // Top face
            3,
            6,
            2,
            3,
            7,
            6,
            // Bottom face
            4,
            1,
            5,
            4,
            0,
            1,
        };
    }

    private void CreateSphereGeometry()
    {
        // Simple sphere approximation using icosphere or UV sphere
        var vertices = new List<VertexPositionColor>();
        var indices = new List<short>();

        const int rings = 16;
        const int sectors = 32;
        const float radius = 0.5f;

        // Generate vertices
        for (int ring = 0; ring <= rings; ring++)
        {
            float phi = MathF.PI * ring / rings;
            for (int sector = 0; sector <= sectors; sector++)
            {
                float theta = 2 * MathF.PI * sector / sectors;

                float x = radius * MathF.Sin(phi) * MathF.Cos(theta);
                float y = radius * MathF.Cos(phi);
                float z = radius * MathF.Sin(phi) * MathF.Sin(theta);

                var color = Color.Lerp(Color.Red, Color.Blue, (float)ring / rings);
                vertices.Add(new VertexPositionColor(new Vector3(x, y, z), color));
            }
        }

        // Generate indices
        for (int ring = 0; ring < rings; ring++)
        {
            for (int sector = 0; sector < sectors; sector++)
            {
                int current = ring * (sectors + 1) + sector;
                int next = current + sectors + 1;

                indices.Add((short)current);
                indices.Add((short)next);
                indices.Add((short)(current + 1));

                indices.Add((short)(current + 1));
                indices.Add((short)next);
                indices.Add((short)(next + 1));
            }
        }

        _vertices = vertices.ToArray();
        _indices = indices.ToArray();
    }

    private void CreateCylinderGeometry()
    {
        var vertices = new List<VertexPositionColor>();
        var indices = new List<short>();

        const int sides = 16;
        const float radius = 0.5f;
        const float height = 1.0f;

        // Top and bottom centers
        vertices.Add(new VertexPositionColor(new Vector3(0, height / 2, 0), Color.White));
        vertices.Add(new VertexPositionColor(new Vector3(0, -height / 2, 0), Color.White));

        // Side vertices
        for (int i = 0; i <= sides; i++)
        {
            float angle = 2 * MathF.PI * i / sides;
            float x = radius * MathF.Cos(angle);
            float z = radius * MathF.Sin(angle);

            var color = Color.Lerp(Color.Green, Color.Purple, (float)i / sides);
            vertices.Add(new VertexPositionColor(new Vector3(x, height / 2, z), color));
            vertices.Add(new VertexPositionColor(new Vector3(x, -height / 2, z), color));
        }

        // Generate indices for top, bottom, and sides
        // This is a simplified version - a full implementation would be more complex
        for (int i = 0; i < sides; i++)
        {
            int topIndex = 2 + i * 2;
            int bottomIndex = 3 + i * 2;
            int nextTopIndex = 2 + ((i + 1) % sides) * 2;
            int nextBottomIndex = 3 + ((i + 1) % sides) * 2;

            // Top face
            indices.Add(0);
            indices.Add((short)topIndex);
            indices.Add((short)nextTopIndex);

            // Bottom face
            indices.Add(1);
            indices.Add((short)nextBottomIndex);
            indices.Add((short)bottomIndex);

            // Side faces
            indices.Add((short)topIndex);
            indices.Add((short)bottomIndex);
            indices.Add((short)nextTopIndex);

            indices.Add((short)nextTopIndex);
            indices.Add((short)bottomIndex);
            indices.Add((short)nextBottomIndex);
        }

        _vertices = vertices.ToArray();
        _indices = indices.ToArray();
    }

    private void CreatePlaneGeometry()
    {
        _vertices = new VertexPositionColor[]
        {
            new(new Vector3(-0.5f, 0, -0.5f), Color.White),
            new(new Vector3(0.5f, 0, -0.5f), Color.White),
            new(new Vector3(0.5f, 0, 0.5f), Color.White),
            new(new Vector3(-0.5f, 0, 0.5f), Color.White),
        };

        _indices = new short[] { 0, 2, 1, 0, 3, 2 };
    }

    private void CreatePyramidGeometry()
    {
        _vertices = new VertexPositionColor[]
        {
            // Base
            new(new Vector3(-0.5f, -0.5f, -0.5f), Color.Red),
            new(new Vector3(0.5f, -0.5f, -0.5f), Color.Green),
            new(new Vector3(0.5f, -0.5f, 0.5f), Color.Blue),
            new(new Vector3(-0.5f, -0.5f, 0.5f), Color.Yellow),
            // Apex
            new(new Vector3(0, 0.5f, 0), Color.White),
        };

        _indices = new short[]
        {
            // Base
            0,
            2,
            1,
            0,
            3,
            2,
            // Sides
            0,
            1,
            4,
            1,
            2,
            4,
            2,
            3,
            4,
            3,
            0,
            4,
        };
    }

    private void CreateBuffers()
    {
        if (_vertices != null && _indices != null)
        {
            _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionColor), _vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_vertices);

            _indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indices);
        }
    }

    private void CreateEffect()
    {
        _basicEffect = new BasicEffect(_graphicsDevice) { VertexColorEnabled = true };
    }

    public void Move(Vector3 offset) => Position += offset;

    public void MoveTo(Vector3 position) => Position = position;

    public void Rotate(Vector3 rotation) => Rotation += rotation;

    public void RotateTo(Vector3 rotation) => Rotation = rotation;

    public void ScaleBy(Vector3 scale) => Scale *= scale;

    public void ScaleTo(Vector3 scale) => Scale = scale;

    public void LookAt(Vector3 target, Vector3? up = null)
    {
        var upVector = up ?? Vector3.Up;
        var forward = Vector3.Normalize(target - Position);
        var right = Vector3.Normalize(Vector3.Cross(upVector, forward));
        var actualUp = Vector3.Cross(forward, right);

        // Convert to Euler angles (simplified)
        Rotation = new Vector3(
            MathF.Atan2(forward.Y, MathF.Sqrt(forward.X * forward.X + forward.Z * forward.Z)),
            MathF.Atan2(-forward.X, -forward.Z),
            0
        );
    }

    public void AnimateToPosition(Vector3 targetPosition, float duration)
    {
        _animationStartPosition = Position;
        _animationTargetPosition = targetPosition;
        _animationDuration = duration;
        _animationElapsed = 0;
        _isAnimatingPosition = true;
    }

    public void AnimateToRotation(Vector3 targetRotation, float duration)
    {
        _animationStartRotation = Rotation;
        _animationTargetRotation = targetRotation;
        _animationDuration = duration;
        _animationElapsed = 0;
        _isAnimatingRotation = true;
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

        // Handle rotation animation
        if (_isAnimatingRotation && _animationStartRotation.HasValue && _animationTargetRotation.HasValue)
        {
            _animationElapsed += deltaTime;
            float progress = Math.Min(_animationElapsed / _animationDuration, 1.0f);

            Rotation = Vector3.Lerp(_animationStartRotation.Value, _animationTargetRotation.Value, progress);

            if (progress >= 1.0f)
            {
                _isAnimatingRotation = false;
                _animationStartRotation = null;
                _animationTargetRotation = null;
            }
        }
    }

    public void Draw(BasicEffect effect, Matrix view, Matrix projection)
    {
        if (!Visible || _vertexBuffer == null || _indexBuffer == null || _basicEffect == null)
            return;

        effect.World = WorldMatrix;
        effect.View = view;
        effect.Projection = projection;

        _graphicsDevice.SetVertexBuffer(_vertexBuffer);
        _graphicsDevice.Indices = _indexBuffer;

        foreach (var pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _indices!.Length / 3);
        }
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _basicEffect?.Dispose();
    }
}
