using System;
using System.Threading.Tasks;
using Orleans.Concurrency;

namespace Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces
{
    public interface IClientGrain : IGrainWithHubKey
    {
        Task OnConnectedAsync(Guid managerId);

        Task OnDisconnectedAsync();

        [AlwaysInterleave]
        Task SendConnectionAsync(string methodName, object[] args);

        Task DeactivateAsync();
    }
}