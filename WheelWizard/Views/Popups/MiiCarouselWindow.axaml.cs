using WheelWizard.Models.MiiImages;
using WheelWizard.WiiManagement;
using WheelWizard.WiiManagement.Domain.Mii;


namespace WheelWizard.Views.Popups;

public partial class MiiCarouselWindow : PopupContent
{
    public MiiCarouselWindow() : base(true,true,false, "Mii Carousel")
    {
        InitializeComponent();
    }

    public MiiCarouselWindow SetMii(Mii newMii)
    {
        Window.WindowTitle = newMii.Name.ToString();
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

