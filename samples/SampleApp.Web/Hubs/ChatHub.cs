using System.Threading.Tasks;
using AspNetCore.SignalR.OrleansIntegration;
using Microsoft.AspNetCore.SignalR;
using SampleApp.Abstractions;

namespace SampleApp.Web.Hubs
{
    [HubName(HubNames.ChatHub)]
    public class ChatHub : Hub
    {
        private readonly IHubProxy<AnotherHub> _anotherHubProxy;
        private readonly IHubProxy<ChatHub> _hubProxy;

        public ChatHub(IHubProxy<ChatHub> hubProxy, IHubProxy<AnotherHub> anotherHubProxy)
        {
            _hubProxy = hubProxy;
            _anotherHubProxy = anotherHubProxy;
        }

        public async Task SendMessage(string user, string message)
        {
            if (user.StartsWith("A"))
                await _hubProxy.AddToGroupAsync(Context.ConnectionId, "SpecialUsers");

            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMessageToAnotherHub(string message)
        {
            await _anotherHubProxy.SendAllAsync("MethodOnOtherClients", "Some message param");
        }
    }
}