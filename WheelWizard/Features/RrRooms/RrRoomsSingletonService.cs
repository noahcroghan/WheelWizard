using WheelWizard.Shared.Services;

namespace WheelWizard.RrRooms;

public interface IRrRoomsSingletonService
{
    Task<OperationResult<List<RwfcRoom>>> GetRoomsAsync();
}

public class RrRoomsSingletonService(IApiCaller<IRwfcApi> apiCaller) : IRrRoomsSingletonService
{
    public async Task<OperationResult<List<RwfcRoom>>> GetRoomsAsync()
    {
        return await apiCaller.CallApiAsync(rwfcApi => rwfcApi.GetWiiGroupsAsync());
    }
}
