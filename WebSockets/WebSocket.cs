using Microsoft.AspNetCore.SignalR;
using SignalR_Example.Model;
using StackExchange.Redis;
using System.Text.Json;

namespace SignalR_Example.WebSockets
{
    public class WebSocket : Hub
    {
        //private static readonly Dictionary<string, string> _connectedClients = new Dictionary<string, string>();
        private static ConnectionMultiplexer _redisConnection;
        private static ISubscriber _redisSubscriber;
        private static string _redisChannelName = "channel";

        public WebSocket()
        {
            if (_redisConnection == null)
            {
                string redisConnectionString = "localhost";
                _redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
                _redisSubscriber = _redisConnection.GetSubscriber();
                _redisSubscriber.Subscribe(_redisChannelName, async (channel, message) =>
                {
                    await RedisReceivedMessage(message);
                });
            }
        }

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

        public async Task PublishMessageRedis(Person person)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            await _redisConnection.GetDatabase().PublishAsync(_redisChannelName, JsonSerializer.Serialize(person, options));
        }

        public async Task SendMessageToClient(string userId, string uniqueMessage)
        {
            await Clients.Client(userId).SendAsync("ReceiveMessage", Context.ConnectionId, uniqueMessage);
        }

        public async Task SendMessage(string Username, string Message)
        {
            await Clients.All.SendAsync("ReceiveMessage", Username, Message);
        }

        public async Task RedisReceivedMessage(RedisValue message)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            if (!message.IsNullOrEmpty)
            {
                var content = JsonSerializer.Deserialize<Person>(message, options);
                await Clients.All.SendAsync("ReceiveObject", content).ConfigureAwait(false);
            }
        }
    }
}