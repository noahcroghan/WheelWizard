using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Media.Imaging;
using WheelWizard.MiiImages;
using WheelWizard.MiiImages.Domain;
using WheelWizard.Shared.DependencyInjection;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.Views.BehaviorComponent;

public abstract class BaseMiiImage : UserControlBase, INotifyPropertyChanged
{
    public enum ReloadMethodType
    {
        ClearThenNew, // Clears all images, then reloads them
        KeepAllUntilNew, // reloads all images, and only then swap them (aka, only then send signal out that they are changed)
        KeepInstanceUntilNew, // reload each image, and swap them if loaded. If there are more images, they will
    }

    [Inject]
    protected IMiiImagesSingletonService MiiImageService { get; set; } = null!;

    private bool _miiLoaded;
    public bool MiiLoaded
    {
        get => _miiLoaded;
        set
        {
            if (_miiLoaded == value)
                return;

            _miiLoaded = value;
            OnPropertyChanged(nameof(MiiLoaded));
            if (value)
                MiiImageLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    private ObservableCollection<Bitmap?> _generatedImages = new();
    public ObservableCollection<Bitmap?> GeneratedImages
    {
        get => _generatedImages;
        private set
        {
            if (_generatedImages == value)
                return;

            _generatedImages = value;
            OnPropertyChanged(nameof(GeneratedImages));
        }
    }

    public static readonly StyledProperty<ReloadMethodType> ReloadMethodProperty = AvaloniaProperty.Register<
        BaseMiiImage,
        ReloadMethodType
    >(nameof(ReloadMethod));

    public ReloadMethodType ReloadMethod
    {
        get => GetValue(ReloadMethodProperty);
        set => SetValue(ReloadMethodProperty, value);
    }

    public static readonly StyledProperty<Mii?> MiiProperty = AvaloniaProperty.Register<BaseMiiImage, Mii?>(nameof(Mii), coerce: CoerceMii);

    public Mii? Mii
    {
        get => GetValue(MiiProperty);
        set => SetValue(MiiProperty, value);
    }

    private static Mii? CoerceMii(AvaloniaObject o, Mii? value)
    {
        // Consider casting to BaseMiiImage if MiiImageLoader isn't guaranteed
        ((BaseMiiImage)o).OnMiiChanged(value);
        return value;
    }

    protected abstract void OnMiiChanged(Mii? newMii);

    protected async void ReloadImages(Mii? newMii, ICollection<MiiImageSpecifications> variants)
    {
        if (ReloadMethod == ReloadMethodType.ClearThenNew)
        {
            GeneratedImages.Clear();
            OnPropertyChanged(nameof(GeneratedImages));
        }

        MiiLoaded = false;
        if (newMii == null)
        {
            MiiLoaded = true;
            return;
        }

        if (ReloadMethod == ReloadMethodType.KeepInstanceUntilNew)
        {
            var index = 0;
            foreach (var variant in variants)
            {
                var imageResult = await MiiImageService.GetImageAsync(newMii, variant);
                var imageToAdd = imageResult.IsSuccess ? imageResult.Value : null;
                if (index < GeneratedImages.Count)
                    GeneratedImages[index] = imageToAdd;
                else if (index == GeneratedImages.Count)
                    GeneratedImages.Add(imageToAdd);
                index++;
                OnPropertyChanged(nameof(GeneratedImages));
            }
        }
        else if (ReloadMethod is ReloadMethodType.KeepAllUntilNew or ReloadMethodType.ClearThenNew)
        {
            var loadedBitmaps = new List<Bitmap?>();
            foreach (var variant in variants)
            {
                var imageResult = await MiiImageService.GetImageAsync(newMii, variant);
                loadedBitmaps.Add(imageResult.IsSuccess ? imageResult.Value : null);
            }
            GeneratedImages.Clear();
            foreach (var bmp in loadedBitmaps)
            {
                GeneratedImages.Add(bmp);
            }
            OnPropertyChanged(nameof(GeneratedImages));
        }

        MiiLoaded = true;
    }

    public event EventHandler? MiiImageLoaded;

    #region INotifyPropertyChanged

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new(propertyName));
    }

    #endregion
}
