using System.Numerics;

namespace WheelWizard.MiiImages.Domain;

public static class MiiImageVariants
{
    public static readonly MiiImageSpecifications Small = new()
    {
        Expression = MiiImageSpecifications.FaceExpression.normal,
        Type = MiiImageSpecifications.BodyType.face,
        Size = MiiImageSpecifications.ImageSize.small,
    };

    public static readonly MiiImageSpecifications FullBodyCarousel = new()
    {
        Expression = MiiImageSpecifications.FaceExpression.normal,
        Type = MiiImageSpecifications.BodyType.all_body,
        Size = MiiImageSpecifications.ImageSize.medium,
        InstanceCount = 8,
    };

    public static readonly MiiImageSpecifications SlightSideProfile = new()
    {
        Expression = MiiImageSpecifications.FaceExpression.normal,
        Type = MiiImageSpecifications.BodyType.face,
        Size = MiiImageSpecifications.ImageSize.medium,
        CharacterRotate = new(350, 15, 355),
        CameraRotate = new(12, 0, 0),
    };
}
