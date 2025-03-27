using Refit;

namespace WheelWizard.RrRooms;

public interface IRwfcApi
{
    [Get("/api/groups")]
    Task<IApiResponse<List<RwfcRoom>>> GetWiiGroupsAsync();
}
