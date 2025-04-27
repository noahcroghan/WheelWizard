using Avalonia.Media.Imaging;
using Refit;

namespace WheelWizard.MiiImages.Domain;

public interface IMiiIMagesApi
{
    [Get("/miis/image.png")]
    Task<Stream> GetImageAsync(string data);
}
