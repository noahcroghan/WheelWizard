using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;
using Refit;
using System.Net;
using System.Net.Sockets;
using WheelWizard.RrRooms;

namespace WheelWizard.Test.Features.RrRooms;

public class RrRoomsSingletonServiceTests
{
    [Fact(DisplayName = "Get rooms async with successful response, returns correct room")]
    public async Task GetRoomsAsyncWithSuccessfulResponse_ReturnsCorrectRoom()
    {
        // Arrange
        var roomId = Guid.NewGuid().ToString();

        var zplWiiApiMock = Substitute.For<IRwfcApi>();
        var apiResponseMock = Substitute.For<IApiResponse<List<RwfcRoom>>>();

        zplWiiApiMock.GetWiiGroupsAsync().ReturnsForAnyArgs(apiResponseMock);

        apiResponseMock.IsSuccessful.Returns(true);
        apiResponseMock.Content.Returns([
            new()
            {
                Id = roomId,
                Game = "Mario Kart Wii",
                Created = DateTime.Now,
                Type = "Rk",
                Suspend = false,
                Players = []
            }
        ]);

        var roomsService = CreateRoomService(zplWiiApiMock);


        // Act
        var rooms = await roomsService.GetRoomsAsync();

        // Assert
        Assert.True(rooms.IsSuccess);

        var room = Assert.Single(rooms.Value);
        Assert.Equal(roomId, room.Id);
    }

    [Fact(DisplayName = "Get rooms async with unsuccessful response, returns empty list")]
    public async Task GetRoomsAsyncWithUnsuccessfulResponse_ReturnsEmptyList()
    {
        // Arrange
        var apiException = await ApiException.Create(new(), new("GET"), new() { StatusCode = HttpStatusCode.InternalServerError }, new());

        var apiResponseMock = Substitute.For<IApiResponse<List<RwfcRoom>>>();
        apiResponseMock.IsSuccessful.Returns(false);
        apiResponseMock.Error.Returns(apiException);

        var zplWiiApiMock = Substitute.For<IRwfcApi>();
        zplWiiApiMock.GetWiiGroupsAsync().ReturnsForAnyArgs(apiResponseMock);

        var roomsService = CreateRoomService(zplWiiApiMock);


        // Act
        var roomsResult = await roomsService.GetRoomsAsync();

        // Assert
        Assert.True(roomsResult.IsFailure);
    }

    [Fact(DisplayName = "Get rooms async with http exception, returns empty list")]
    public async Task GetRoomsAsyncWithHttpException_ReturnsEmptyList()
    {
        // Arrange
        var zplWiiApiMock = Substitute.For<IRwfcApi>();

        zplWiiApiMock.GetWiiGroupsAsync()
            .Throws(new HttpRequestException("Failed to connect to ZplWii API", new SocketException((int)SocketError.HostNotFound)));

        var roomsService = CreateRoomService(zplWiiApiMock);

        // Act
        var roomsResult = await roomsService.GetRoomsAsync();

        // Assert
        Assert.True(roomsResult.IsFailure);
    }

    private static RrRoomsSingletonService CreateRoomService(IRwfcApi rwfcApiMock)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTransient<IRwfcApi>(_ => rwfcApiMock);
        serviceCollection.AddSingleton<RrRoomsSingletonService>();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider.GetRequiredService<RrRoomsSingletonService>();
    }
}
