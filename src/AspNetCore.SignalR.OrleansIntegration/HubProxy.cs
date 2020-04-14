using Microsoft.AspNetCore.SignalR;
using Orleans;
using Orleans.SignalRIntegration.Core;

namespace AspNetCore.SignalR.OrleansIntegration
{
    public class HubProxy<THub> : HubProxy, IHubProxy<THub> where THub : Hub
    {
        public HubProxy(IClusterClient clusterClient)
            : base(clusterClient,
                () => clusterClient.GetStreamProvider(OrleansSignalRConstants.StreamProviderName),
                typeof(THub).GetHubName(),
                typeof(THub).GetAllStreamKey())
        {
        }
    }
}