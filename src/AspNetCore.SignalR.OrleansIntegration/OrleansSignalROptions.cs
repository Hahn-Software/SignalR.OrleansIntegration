using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace AspNetCore.SignalR.OrleansIntegration
{
    public class OrleansSignalROptions<THub> where THub : Hub
    {
        public IClusterClient ClusterClient { get; set; }

        public int MaxClusterClientConnectionRetries { get; set; } = 5;

        public int ClusterClientConnectionRetryInterval { get; set; } = 2000;
    }
}