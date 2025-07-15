using Avalonia.OpenGL;

namespace WheelWizard.Rendering3D.Domain;

public interface IRenderingEngine
{
    void Initialize(GlInterface gl);
    void Render(GlInterface gl, int frameBuffer, int width, int height);
    void Resize(int width, int height);
    void Dispose();
}

public interface IRenderObject
{
    void Initialize(GlInterface gl);
    void Render(GlInterface gl);
    void Dispose();
}

public interface IShaderProgram
{
    uint ProgramId { get; }
    void Use(GlInterface gl);
    void SetUniform(GlInterface gl, string name, float value);
    void SetUniform(GlInterface gl, string name, float[] matrix);
    void Dispose();
}

public struct Vector3
{
    public float X,
        Y,
        Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 Zero => new(0, 0, 0);
    public static Vector3 Up => new(0, 1, 0);
}

public struct Matrix4x4
{
    public float[] Values;

    public Matrix4x4(float[] values)
    {
        if (values.Length != 16)
            throw new ArgumentException("Matrix must have 16 elements");
        Values = values;
    }

    public static Matrix4x4 Identity => new([1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]);

    public static Matrix4x4 CreatePerspective(float fovY, float aspect, float near, float far)
    {
        var tanHalfFovY = (float)Math.Tan(fovY / 2);
        var values = new float[16];

        values[0] = 1.0f / (aspect * tanHalfFovY);
        values[5] = 1.0f / tanHalfFovY;
        values[10] = -(far + near) / (far - near);
        values[11] = -1.0f;
        values[14] = -(2.0f * far * near) / (far - near);

        return new Matrix4x4(values);
    }

    public static Matrix4x4 CreateTranslation(Vector3 translation)
    {
        var values = Identity.Values;
        values[12] = translation.X;
        values[13] = translation.Y;
        values[14] = translation.Z;
        return new Matrix4x4(values);
    }

    public static Matrix4x4 CreateRotationY(float angle)
    {
        var cos = (float)Math.Cos(angle);
        var sin = (float)Math.Sin(angle);
        var values = Identity.Values;

        values[0] = cos;
        values[2] = sin;
        values[8] = -sin;
        values[10] = cos;

        return new Matrix4x4(values);
    }

    public static Matrix4x4 operator *(Matrix4x4 left, Matrix4x4 right)
    {
        var result = new float[16];

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                result[i * 4 + j] = 0;
                for (int k = 0; k < 4; k++)
                {
                    result[i * 4 + j] += left.Values[i * 4 + k] * right.Values[k * 4 + j];
                }
            }
        }

        return new Matrix4x4(result);
    }
}
