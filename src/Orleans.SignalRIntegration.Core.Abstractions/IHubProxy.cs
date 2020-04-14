using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans.SignalRIntegration.Core.Abstractions.Messages;
using Orleans.Streams;

namespace Orleans.SignalRIntegration.Core.Abstractions
{
    public interface IHubProxy
    {
        IAsyncStream<SendToAllInvocationMessage> AllMessageStream { get; }

        Task OnConnectedAsync(Guid managerId, string connectionId, string userId = default);

        Task OnDisconnectedAsync(string connectionId, string userId = default);

        Task AddToGroupAsync(string connectionId, string groupName);

        Task RemoveFromGroupAsync(string connectionId, string groupName);

        Task SendAllAsync(string method, params object[] args);

        Task SendAllExceptAsync(string method, IEnumerable<string> excludedConnectionIds, params object[] args);

        Task SendClientAsync(string connectionId, string method, params object[] args);

        Task SendClientsAsync(IEnumerable<string> connectionIds, string method, params object[] args);

        Task SendClientsExceptAsync(IEnumerable<string> connectionIds, string method, IEnumerable<string> excludedConnectionIds,
            params object[] args);

        Task SendGroupAsync(string groupName, string method, params object[] args);

        Task SendGroupExceptAsync(string groupName, string method, IEnumerable<string> excludedConnectionIds, params object[] args);

        Task SendGroupsAsync(IEnumerable<string> groupNames, string method, params object[] args);

        Task SendUserAsync(string userId, string method, params object[] args);

        Task SendUsersAsync(IEnumerable<string> userIds, string method, params object[] args);

        Task SendUsersExceptAsync(IEnumerable<string> userIds, string method, IEnumerable<string> excludedConnectionIds, params object[] args);

        Task DeactivateClientAsync(string connectionId);

        Task DeactivateGroupAsync(string groupName);

        Task DeactivateUserAsync(string userId);
    }
}