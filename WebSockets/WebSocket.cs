using Microsoft.AspNetCore.SignalR;

namespace SignalR_Example.WebSockets
{
    public class WebSocket : Hub
    {
        //private static readonly Dictionary<string, string> _connectedClients = new Dictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            var id = Context.ConnectionId;

            await Clients.Client(Context.ConnectionId).SendAsync("Connected", id);

            await base.OnConnectedAsync();
        }

        //public override async Task OnDisconnectedAsync(Exception)
        //{
        //    var userId = _connectedClients[Context.ConnectionId];
        //    _connectedClients.Remove(Context.ConnectionId);

        //    await Clients.AllExcept(new[] { Context.ConnectionId })
        //        .SendAsync("UserDisconnected", userId);

        //    await base.OnDisconnectedAsync(exception);
        //}

        public async Task SendMessageToClient(string userId, string uniqueMessage)
        {
            await Clients.Client(userId).SendAsync("ReceiveMessage", Context.ConnectionId, uniqueMessage);
        }

        public async Task SendMessage(string Username, string Message)
        {
            await Clients.All.SendAsync("ReceiveMessage", Username, Message);
        }
    }
}