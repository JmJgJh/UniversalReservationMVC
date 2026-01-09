using Microsoft.AspNetCore.SignalR;

namespace UniversalReservationMVC.Hubs
{
    public class SeatHub : Hub
    {
        public async Task JoinResource(string resourceId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, resourceId);
        }
    }
}
