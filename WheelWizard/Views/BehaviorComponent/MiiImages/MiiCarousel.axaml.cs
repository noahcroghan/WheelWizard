using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Media;
using WheelWizard.MiiImages.Domain;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.BehaviorComponent;

public partial class MiiCarousel : BaseMiiImage
{
    private int CarouselInstanceCount = 1;
    private int CurrentCarouselInstance = 0;

    public static readonly StyledProperty<MiiImageSpecifications> ImageVariantProperty = AvaloniaProperty.Register<
        MiiCarousel,
        MiiImageSpecifications
    >(nameof(ImageVariant), MiiImageVariants.OnlinePlayerSmall, coerce: CoerceVariant);

    public MiiImageSpecifications ImageVariant
    {
        get => GetValue(ImageVariantProperty);
        set => SetValue(ImageVariantProperty, value);
    }

    public MiiCarousel()
    {
        InitializeComponent();
    }

    private static MiiImageSpecifications CoerceVariant(AvaloniaObject o, MiiImageSpecifications value)
    {
        ((MiiCarousel)o).OnVariantChanged(value);
        return value;
    }

    protected void OnVariantChanged(MiiImageSpecifications newSpecifications)
    {
        CarouselInstanceCount = newSpecifications.InstanceCount;
        ReloadImages(Mii, [newSpecifications]);
    }

    protected override void OnMiiChanged(Mii? newMii)
    {
        CurrentCarouselInstance = 0;
        MiiImage.RenderTransform = new TranslateTransform(0, 0);
        ReloadImages(newMii, [ImageVariant]);
    }

    private void RotateLeft_Click(object? sender, RoutedEventArgs e)
    {
        CurrentCarouselInstance += 1;
        if (CurrentCarouselInstance > 0)
            CurrentCarouselInstance -= CarouselInstanceCount;
        CurrentCarouselInstance %= CarouselInstanceCount;
        MiiImage.RenderTransform = new TranslateTransform(CurrentCarouselInstance * MiiImage.Bounds.Height, 0);
    }

    private void RotateRight_Click(object? sender, RoutedEventArgs e)
    {
        CurrentCarouselInstance -= 1;
        CurrentCarouselInstance %= CarouselInstanceCount;
        MiiImage.RenderTransform = new TranslateTransform(CurrentCarouselInstance * MiiImage.Bounds.Height, 0);
    }
}
