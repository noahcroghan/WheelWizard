using Avalonia.Media.Imaging;
using Microsoft.Extensions.Caching.Memory;
using WheelWizard.MiiImages.Domain;
using WheelWizard.Shared.Services;
using WheelWizard.WiiManagement.Domain.Mii;

namespace WheelWizard.MiiImages;

public interface IMiiImagesSingletonService
{
    Task<OperationResult<Bitmap>> GetImageAsync(Mii? mii, MiiImageSpecifications specifications);
}

public class MiiImagesSingletonService(IApiCaller<IMiiIMagesApi> apiCaller, IMemoryCache cache) : IMiiImagesSingletonService
{
    public async Task<OperationResult<Bitmap>> GetImageAsync(Mii? mii, MiiImageSpecifications specifications)
    {
        var data = MiiStudioDataSerializer.Serialize(mii);
        if (data.IsFailure)
            return data.Error;

        var miiConfigKey = data.Value + specifications;
        var isCached = cache.TryGetValue(miiConfigKey, out Bitmap? cachedValue);
        if (isCached)
            return cachedValue ?? Fail<Bitmap>("Cached image is null.");

        var newImageResult = await apiCaller.CallApiAsync(api => GetBitmapAsync(api, data.Value, specifications));
        Bitmap? newImage = null;
        if (newImageResult.IsSuccess)
            newImage = newImageResult.Value;

        using (var entry = cache.CreateEntry(miiConfigKey))
        {
            entry.Value = newImage;
            entry.SlidingExpiration = specifications.ExpirationSeconds;
            entry.Priority = specifications.CachePriority;
        }

        return newImage ?? Fail<Bitmap>("Failed to get new image.");
    }

    private static async Task<Bitmap> GetBitmapAsync(IMiiIMagesApi api, string data, MiiImageSpecifications specifications)
    {
        var result = await api.GetImageAsync(
            data,
            specifications.Type.ToString(),
            specifications.Expression.ToString(),
            (int)specifications.Size,
            characterXRotate: (int)specifications.CharacterRotate.X,
            characterYRotate: (int)specifications.CharacterRotate.Y,
            characterZRotate: (int)specifications.CharacterRotate.Z,
            bgColor: specifications.BackgroundColor,
            instanceCount: specifications.InstanceCount,
            cameraXRotate: (int)specifications.CameraRotate.X,
            cameraYRotate: (int)specifications.CameraRotate.Y,
            cameraZRotate: (int)specifications.CameraRotate.Z
        );

        using var memoryStream = new MemoryStream();
        await result.CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset stream position for Bitmap constructor

        if (memoryStream.Length == 0)
            throw new InvalidOperationException("Received empty image stream.");

        var bitmap = new Bitmap(memoryStream);
        return bitmap;
    }
}
