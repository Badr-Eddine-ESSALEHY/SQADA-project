using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ZKTeco.Parking.API.Hubs;

[Authorize]
public class ParkingHub : Hub
{
    public async Task JoinParkingGroup(string parkingId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"parking-{parkingId}");
        await Clients.Caller.SendAsync("JoinedGroup", $"parking-{parkingId}");
    }

    public async Task LeaveParkingGroup(string parkingId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"parking-{parkingId}");
        await Clients.Caller.SendAsync("LeftGroup", $"parking-{parkingId}");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
