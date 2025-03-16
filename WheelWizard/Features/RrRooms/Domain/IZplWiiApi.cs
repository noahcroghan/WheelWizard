using Refit;

namespace WheelWizard.RrRooms;

public interface IZplWiiApi
{
    [Get("/api/groups")]
    Task<IApiResponse<List<ZplWiiRoom>>> GetWiiGroupsAsync();
}