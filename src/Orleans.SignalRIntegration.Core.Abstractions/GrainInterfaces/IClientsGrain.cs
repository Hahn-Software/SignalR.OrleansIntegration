using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.Concurrency;

namespace Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces
{
    public interface IClientsGrain : IGrainWithHubKey
    {
        Task AddToClientsAsync(string connectionId);

        Task RemoveFromClientsAsync(string connectionId);

        [AlwaysInterleave]
        Task SendConnectionsAsync(string methodName, object[] args);

        [AlwaysInterleave]
        Task SendConnectionsExceptAsync(string methodName, object[] args, IEnumerable<string> excludedConnectionIds);

        Task DeactivateAsync();
    }
}