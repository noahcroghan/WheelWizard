using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;
using WheelWizard.MiiImages;
using WheelWizard.Models.MiiImages;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.Components.MiiImages;

public abstract class BaseMiiImage : TemplatedControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<bool> MiiLoadedProperty = AvaloniaProperty.Register<BaseMiiImage, bool>(nameof(MiiLoaded));

    protected bool MiiLoaded
    {
        get => GetValue(MiiLoadedProperty);
        set
        {
            SetValue(MiiLoadedProperty, value);
            OnPropertyChanged(nameof(MiiLoaded));
            if (value)
                MiiImageLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    public static readonly StyledProperty<Bitmap?> MiiImageProperty = AvaloniaProperty.Register<BaseMiiImage, Bitmap?>(nameof(MiiImage));

    protected Bitmap? MiiImage
    {
        get => GetValue(MiiImageProperty);
        set
        {
            SetValue(MiiImageProperty, value);
            OnPropertyChanged(nameof(MiiImage));
        }
    }

    public static readonly StyledProperty<MiiImageVariants.Variant> ImageVariantProperty = AvaloniaProperty.Register<
        BaseMiiImage,
        MiiImageVariants.Variant
    >(nameof(ImageVariant), MiiImageVariants.Variant.SMALL, coerce: CoerceVariant);

    public MiiImageVariants.Variant ImageVariant
    {
        get => GetValue(ImageVariantProperty);
        set => SetValue(ImageVariantProperty, value);
    }

    public static readonly StyledProperty<Mii?> MiiProperty = AvaloniaProperty.Register<BaseMiiImage, Mii?>(nameof(Mii), coerce: CoerceMii);

    public Mii? Mii
    {
        get => GetValue(MiiProperty);
        set => SetValue(MiiProperty, value);
    }

    private static MiiImageVariants.Variant CoerceVariant(AvaloniaObject o, MiiImageVariants.Variant value)
    {
        ((BaseMiiImage)o).OnVariantChanged(value);
        return value;
    }

    private static Mii? CoerceMii(AvaloniaObject o, Mii? value)
    {
        ((BaseMiiImage)o).OnMiiChanged(value);
        return value;
    }

    protected void OnVariantChanged(MiiImageVariants.Variant newValue) => ReloadImage(Mii, newValue);

    protected void OnMiiChanged(Mii? newValue) => ReloadImage(newValue, ImageVariant);

    protected async void ReloadImage(Mii? newMii, MiiImageVariants.Variant variant)
    {
        // If the mii was already null, it did not actually change (even if the variant did change).
        if (newMii == null && Mii != null)
            return;

        MiiLoaded = false;
        if (newMii == null)
        {
            MiiImage = null;
            MiiLoaded = true;
            return;
        }

        var imageService = App.Services.GetService<IMiiImagesSingletonService>()!;
        var image = await imageService.GetImageAsync(newMii);

        if (image.IsFailure)
        {
            MiiImage = null;
            MiiLoaded = false;
            return;
        }

        MiiImage = image.Value;
        MiiLoaded = true;
    }

    public event EventHandler MiiImageLoaded;

    #region PropertyChanged

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    #endregion
}
