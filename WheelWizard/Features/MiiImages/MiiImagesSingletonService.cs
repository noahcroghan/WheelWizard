using Avalonia.Media.Imaging;
using WheelWizard.MiiImages.Domain;
using WheelWizard.Shared.Services;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.MiiImages;

public interface IMiiImagesSingletonService
{
    Task<OperationResult<Bitmap>> GetImageAsync(Mii? mii);
}

public class MiiImagesSingletonService(IApiCaller<IMiiIMagesApi> apiCaller) : IMiiImagesSingletonService
{
    public async Task<OperationResult<Bitmap>> GetImageAsync(Mii? mii)
    {
        var data = MiiStudioDataSerializer.Serialize(mii);
        if (data.IsFailure)
            return data.Error;

        return await apiCaller.CallApiAsync(api => GetBitmapAsync(api, data.Value));
    }

    private static async Task<Bitmap> GetBitmapAsync(IMiiIMagesApi api, string data)
    {
        var result = await api.GetImageAsync(data);
        using var memoryStream = new MemoryStream();
        await result.CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset stream position for Bitmap constructor

        if (memoryStream.Length == 0)
            throw new InvalidOperationException("Received empty image stream.");

        var bitmap = new Bitmap(memoryStream);
        return bitmap;
    }
}
