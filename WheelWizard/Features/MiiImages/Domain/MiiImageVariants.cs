using System.Numerics;
using Microsoft.Extensions.Caching.Memory;

namespace WheelWizard.MiiImages.Domain;

public static class MiiImageVariants
{
    public static readonly MiiImageSpecifications CurrentUserSmall = new()
    {
        Name = "CurrentUserSmall",
        Expression = MiiImageSpecifications.FaceExpression.normal,
        Type = MiiImageSpecifications.BodyType.face,
        Size = MiiImageSpecifications.ImageSize.small,
        CachePriority = CacheItemPriority.High,
    };

    public static readonly MiiImageSpecifications MiiEditorProfile = new()
    {
        Name = "CurrentUserSmall",
        Expression = MiiImageSpecifications.FaceExpression.normal,
        Type = MiiImageSpecifications.BodyType.face,
        Size = MiiImageSpecifications.ImageSize.medium,
    };

    public static readonly MiiImageSpecifications OnlinePlayerSmall = new()
    {
        Name = "OnlinePlayerSmall",
        Expression = MiiImageSpecifications.FaceExpression.normal,
        Type = MiiImageSpecifications.BodyType.face,
        Size = MiiImageSpecifications.ImageSize.small,
        CachePriority = CacheItemPriority.Low,
    };

    public static readonly MiiImageSpecifications CurrentUserSideProfile = new()
    {
        Expression = MiiImageSpecifications.FaceExpression.normal,
        Type = MiiImageSpecifications.BodyType.face,
        Size = MiiImageSpecifications.ImageSize.medium,
        CharacterRotate = new(350, 15, 355),
        CameraRotate = new(12, 0, 0),
    };
    public static readonly MiiImageSpecifications FriendsSideProfile = new()
    {
        Expression = MiiImageSpecifications.FaceExpression.normal,
        Type = MiiImageSpecifications.BodyType.face,
        Size = MiiImageSpecifications.ImageSize.medium,
        CharacterRotate = new(350, 15, 355),
        CameraRotate = new(12, 0, 0),
        ExpirationSeconds = TimeSpan.FromMinutes(60),
    };

    public static readonly MiiImageSpecifications FullBodyCarousel = new()
    {
        Expression = MiiImageSpecifications.FaceExpression.normal,
        Type = MiiImageSpecifications.BodyType.all_body,
        Size = MiiImageSpecifications.ImageSize.medium,
        InstanceCount = 2,
    };
}
