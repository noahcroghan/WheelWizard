using WheelWizard.Models.MiiImages;
using WheelWizard.WiiManagement;
using WheelWizard.WiiManagement.Domain;

namespace WheelWizard.Views.Popups;

public partial class MiiCarouselWindow : PopupContent
{
    public MiiCarouselWindow() : base(true,true,false, "Mii Carousel", new(440, 300))
    {
        InitializeComponent();
    }

    public MiiCarouselWindow SetMii(FullMii newMii)
    {
        Window.WindowTitle = newMii.Name;
        Carousel.MiiImageLoaded += DisableLoadingIcon;
        Carousel.Mii = newMii;
        return this;
    }

    private void DisableLoadingIcon(object? sender, EventArgs e)
    {
        MiiLoadingIcon.IsVisible = false;
        Carousel.MiiImageLoaded -= DisableLoadingIcon;
    }
}

