using System.Numerics;
using Avalonia.Media;
using Microsoft.Extensions.Caching.Memory;

namespace WheelWizard.MiiImages.Domain;

public class MiiImageSpecifications
{
    public override string ToString()
    {
        // If we put all the things in this string, then the Key at least is unique
        return $"{Size}{Expression}{Type}_{BackgroundColor}{InstanceCount}_{CharacterRotate}{CameraRotate}";
    }

    public ImageSize Size { get; set; } = ImageSize.small;
    public FaceExpression Expression { get; set; } = FaceExpression.normal;
    public BodyType Type { get; set; } = BodyType.face;
    public string BackgroundColor = "FFFFFF00";
    public int InstanceCount { get; set; } = 1;

    // All between 0 and 360, obviously
    public Vector3 CharacterRotate { get; set; } = Vector3.Zero;
    public Vector3 CameraRotate { get; set; } = Vector3.Zero;

    public TimeSpan? ExpirationSeconds { get; set; } = TimeSpan.FromMinutes(30);
    public CacheItemPriority CachePriority { get; set; } = CacheItemPriority.Normal;

    #region Enums

    // IMPORTANT: keep these enums lowercase and with underscores
    public enum FaceExpression
    {
        normal,
        smile,
        frustrated,
        anger,
        blink,
        sorrow,
        surprise,
    }

    public enum ImageSize
    {
        small = 270,
        medium = 512,
    }

    public enum BodyType
    {
        face,
        all_body,
    }

    #endregion
}
