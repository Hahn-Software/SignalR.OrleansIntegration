using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces;
using Orleans.Streams;

namespace Orleans.SignalRIntegration.Grains
{
    public abstract class ClientsGrain<TGrainState> : Grain<TGrainState>, IClientsGrain
        where TGrainState : ClientsGrainState, new()
    {
        public virtual async Task AddToClientsAsync(string connectionId)
        {
            if (!State.HandlesByConnectionId.ContainsKey(connectionId))
            {
                var handle = await GetStreamProvider(OrleansSignalRConstants.StreamProviderName)
                    .GetStream<EventArgs>(OrleansSignalRConstants.DisconnectionStreamId, connectionId)
                    .SubscribeAsync(async (_, token) => await RemoveFromClientsAsync(connectionId));
                State.HandlesByConnectionId.Add(connectionId, handle);
                await WriteStateAsync();
            }
        }

        public virtual async Task RemoveFromClientsAsync(string connectionId)
        {
            if (State.HandlesByConnectionId.ContainsKey(connectionId))
            {
                var handle = State.HandlesByConnectionId[connectionId];
                await handle.UnsubscribeAsync();
                State.HandlesByConnectionId.Remove(connectionId);
                await WriteStateAsync();
            }

            if (State.HandlesByConnectionId.Count == 0)
                DeactivateOnIdle();
        }

        public virtual Task SendConnectionsAsync(string methodName, object[] args)
        {
            var hubName = this.GetHubName();
            var hubProxy = GrainFactory.GetHubProxy(GetStreamProvider(OrleansSignalRConstants.StreamProviderName), hubName);
            return hubProxy.SendClientsAsync(State.HandlesByConnectionId.Keys.ToList(), methodName, args);
        }

        public virtual Task SendConnectionsExceptAsync(string methodName, object[] args, IEnumerable<string> excludedConnectionIds)
        {
            var hubName = this.GetHubName();
            var hubProxy = GrainFactory.GetHubProxy(GetStreamProvider(OrleansSignalRConstants.StreamProviderName), hubName);
            return hubProxy.SendClientsExceptAsync(State.HandlesByConnectionId.Keys.ToList(), methodName, excludedConnectionIds, args);
        }

        public virtual Task DeactivateAsync()
        {
            DeactivateOnIdle();
            return Task.CompletedTask;
        }

        public override async Task OnActivateAsync()
        {
            var tasks = State.HandlesByConnectionId.Keys.Select(ResumeRemoveHandleAsync);
            await Task.WhenAll(tasks);

            async Task ResumeRemoveHandleAsync(string connectionId)
            {
                var handles = await GetStreamProvider(OrleansSignalRConstants.StreamProviderName)
                    .GetStream<EventArgs>(OrleansSignalRConstants.DisconnectionStreamId, connectionId)
                    .GetAllSubscriptionHandles();
                var clientTasks = handles.Select(handle => handle.ResumeAsync(async (_, token) => await RemoveFromClientsAsync(connectionId)));
                await Task.WhenAll(clientTasks);
            }
        }
    }
}