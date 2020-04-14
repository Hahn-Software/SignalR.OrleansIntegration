using System.Threading.Tasks;
using Orleans;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Core.Abstractions;
using SampleApp.Abstractions;

namespace SampleApp.Silo
{
    public class ChatGrain : Grain, IChatGrain
    {
        private IHubProxy _hubProxy;

        public async Task SendServerMessage()
        {
            // 3. Send a message from the chat server to all clients.
            await _hubProxy.SendAllAsync("ReceiveMessage", "Server", "Hello, Client!");

            // 4. Send a message to a specific group
            await _hubProxy.SendGroupAsync("SpecialUsers", "ReceiveMessage", "Server", "Hello, special Client!");
        }

        public override Task OnActivateAsync()
        {
            // 1. First retrieve the stream provider for SignalR integration streams 
            var streamProvider = GetStreamProvider(OrleansSignalRConstants.StreamProviderName);

            // 2. Retrieve a hub proxy for a Hub
            _hubProxy = GrainFactory.GetHubProxy(streamProvider, HubNames.ChatHub);

            return base.OnActivateAsync();
        }
    }
}