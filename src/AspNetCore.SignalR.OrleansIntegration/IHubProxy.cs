using Microsoft.AspNetCore.SignalR;
using Orleans.SignalRIntegration.Core.Abstractions;

namespace AspNetCore.SignalR.OrleansIntegration
{
    public interface IHubProxy<THub> : IHubProxy 
        where THub : Hub
    {
    }
}