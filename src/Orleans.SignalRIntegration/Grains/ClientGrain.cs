using System;
using System.Threading.Tasks;
using Orleans.Providers;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces;
using Orleans.SignalRIntegration.Core.Abstractions.Messages;

namespace Orleans.SignalRIntegration.Grains
{
    [StorageProvider(ProviderName = OrleansSignalRConstants.StorageProviderName)]
    public class ClientGrain : Grain<ClientGrainState>, IClientGrain
    {
        public async Task OnConnectedAsync(Guid hubLifetimeManagerId)
        {
            State.HubLifetimeManagerId = hubLifetimeManagerId;

            State.Connected = true;
            await WriteStateAsync();
        }

        public async Task OnDisconnectedAsync()
        {
            var connectionId = this.GetId();

            await GetStreamProvider(OrleansSignalRConstants.StreamProviderName)
                .GetStream<EventArgs>(OrleansSignalRConstants.DisconnectionStreamId, connectionId)
                .OnNextAsync(EventArgs.Empty);

            State.Connected = false;
            await WriteStateAsync();

            DeactivateOnIdle();
        }

        public Task SendConnectionAsync(string methodName, object[] args)
        {
            var connectionId = this.GetId();
            return GetStreamProvider(OrleansSignalRConstants.StreamProviderName)
                .GetStream<SendToClientInvocationMessage>(State.HubLifetimeManagerId, OrleansSignalRConstants.ClientMessageSendingStreamNamespace)
                .OnNextAsync(new SendToClientInvocationMessage(methodName, args, connectionId));
        }

        public Task DeactivateAsync()
        {
            DeactivateOnIdle();
            return Task.CompletedTask;
        }
    }
}