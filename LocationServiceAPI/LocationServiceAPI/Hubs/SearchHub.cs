using Microsoft.AspNetCore.SignalR;

namespace LocationServiceAPI.Hubs
{
    public class SearchHub : Hub
    {
        public async Task SendSearchLog(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
