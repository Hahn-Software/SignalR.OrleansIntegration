using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.SignalRIntegration.Core.Abstractions;
using Orleans.SignalRIntegration.Core.Abstractions.Messages;
using Orleans.Streams;

namespace Orleans.SignalRIntegration.Core
{
    public class HubProxy : IHubProxy
    {
        private readonly Guid _allStreamKey;
        private readonly IGrainFactory _grainFactory;
        private readonly string _hubName;
        private readonly Func<IStreamProvider> _streamProviderFactory;
        private IAsyncStream<SendToAllInvocationMessage> _allMessageStream;
        private IStreamProvider _streamProvider;

        public HubProxy(IGrainFactory grainFactory, IStreamProvider streamProvider, string hubName)
            : this(grainFactory, streamProvider, hubName, GuidUtility.Create(OrleansSignalRConstants.DefaultAllStreamKey, hubName))
        {
        }

        public HubProxy(IGrainFactory grainFactory, IStreamProvider streamProvider, string hubName, Guid allStreamKey)
        {
            _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
            _streamProvider = streamProvider ?? throw new ArgumentNullException(nameof(streamProvider));
            _hubName = hubName ?? throw new ArgumentNullException(nameof(hubName));
            _allStreamKey = allStreamKey;
        }

        public HubProxy(IGrainFactory grainFactory, Func<IStreamProvider> streamProviderFactory, string hubName, Guid allStreamKey)
        {
            _grainFactory = grainFactory ?? throw new ArgumentNullException(nameof(grainFactory));
            _streamProviderFactory = streamProviderFactory ?? throw new ArgumentNullException(nameof(streamProviderFactory));
            _hubName = hubName;
            _allStreamKey = allStreamKey;
        }

        public IAsyncStream<SendToAllInvocationMessage> AllMessageStream
        {
            get
            {
                EnsureAllStream();

                return _allMessageStream;
            }
        }

        public async Task OnConnectedAsync(Guid managerId, string connectionId, string userId = default)
        {
            if (connectionId == null)
                throw new ArgumentNullException(nameof(connectionId));

            if (userId != null)
                await _grainFactory.GetUserGrain(userId, _hubName).AddToClientsAsync(connectionId);

            await _grainFactory.GetClientGrain(connectionId, _hubName).OnConnectedAsync(managerId);
        }

        public async Task OnDisconnectedAsync(string connectionId, string userId = default)
        {
            if (connectionId == null)
                throw new ArgumentNullException(nameof(connectionId));

            if (userId != null)
                await _grainFactory.GetUserGrain(userId, _hubName).RemoveFromClientsAsync(connectionId);

            await _grainFactory.GetClientGrain(connectionId, _hubName).OnDisconnectedAsync();
        }

        public Task AddToGroupAsync(string connectionId, string groupName)
        {
            if (connectionId == null)
                throw new ArgumentNullException(nameof(connectionId));

            if (groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            return _grainFactory.GetGroupGrain(groupName, _hubName).AddToClientsAsync(connectionId);
        }

        public Task RemoveFromGroupAsync(string connectionId, string groupName)
        {
            if (connectionId == null)
                throw new ArgumentNullException(nameof(connectionId));

            if (groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            return _grainFactory.GetGroupGrain(groupName, _hubName).RemoveFromClientsAsync(connectionId);
        }

        public Task SendAllAsync(string method, params object[] args)
        {
            return SendAllExceptAsync(method, Enumerable.Empty<string>().ToList(), args);
        }

        public Task SendAllExceptAsync(string method, IEnumerable<string> excludedConnectionIds, params object[] args)
        {
            if (excludedConnectionIds == null)
                throw new ArgumentNullException(nameof(excludedConnectionIds));

            EnsureAllStream();

            return _allMessageStream.OnNextAsync(new SendToAllInvocationMessage(method, args, excludedConnectionIds.ToList()));
        }

        public Task SendClientAsync(string connectionId, string method, params object[] args)
        {
            if (connectionId == null)
                throw new ArgumentNullException(nameof(connectionId));

            return _grainFactory.GetClientGrain(connectionId, _hubName).SendConnectionAsync(method, args);
        }

        public Task SendClientsAsync(IEnumerable<string> connectionIds, string method, params object[] args)
        {
            if (connectionIds == null)
                throw new ArgumentNullException(nameof(connectionIds));

            return SendClientsExceptAsync(connectionIds, method, Enumerable.Empty<string>().ToList(), args);
        }

        public Task SendClientsExceptAsync(IEnumerable<string> connectionIds, string method,
            IEnumerable<string> excludedConnectionIds, params object[] args)
        {
            if (connectionIds == null)
                throw new ArgumentNullException(nameof(connectionIds));

            if (excludedConnectionIds == null)
                throw new ArgumentNullException(nameof(excludedConnectionIds));

            var tasks = connectionIds.Where(id => !excludedConnectionIds.Contains(id))
                .Select(id => _grainFactory.GetClientGrain(id, _hubName).SendConnectionAsync(method, args));

            return Task.WhenAll(tasks);
        }

        public Task SendGroupAsync(string groupName, string method, params object[] args)
        {
            if (groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            return _grainFactory.GetGroupGrain(groupName, _hubName).SendConnectionsAsync(method, args);
        }

        public Task SendGroupExceptAsync(string groupName, string method, IEnumerable<string> excludedConnectionIds, params object[] args)
        {
            if (groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            if (excludedConnectionIds == null)
                throw new ArgumentNullException(nameof(excludedConnectionIds));

            return _grainFactory.GetGroupGrain(groupName, _hubName).SendConnectionsExceptAsync(method, args, excludedConnectionIds);
        }

        public Task SendGroupsAsync(IEnumerable<string> groupNames, string method, params object[] args)
        {
            if (groupNames == null)
                throw new ArgumentNullException(nameof(groupNames));

            var tasks = groupNames.Select(name => _grainFactory.GetGroupGrain(name, _hubName).SendConnectionsAsync(method, args));

            return Task.WhenAll(tasks);
        }

        public Task SendUserAsync(string userId, string method, params object[] args)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            return _grainFactory.GetUserGrain(userId, _hubName).SendConnectionsAsync(method, args);
        }

        public Task SendUsersAsync(IEnumerable<string> userIds, string method, params object[] args)
        {
            if (userIds == null)
                throw new ArgumentNullException(nameof(userIds));

            var tasks = userIds.Select(id => _grainFactory.GetUserGrain(id, _hubName).SendConnectionsAsync(method, args));

            return Task.WhenAll(tasks);
        }

        public Task SendUsersExceptAsync(IEnumerable<string> userIds, string method, IEnumerable<string> excludedConnectionIds, params object[] args)
        {
            if (userIds == null)
                throw new ArgumentNullException(nameof(userIds));

            if (excludedConnectionIds == null)
                throw new ArgumentNullException(nameof(excludedConnectionIds));

            var tasks = userIds.Select(id =>
                _grainFactory.GetUserGrain(id, _hubName).SendConnectionsExceptAsync(method, args, excludedConnectionIds));

            return Task.WhenAll(tasks);
        }

        public Task DeactivateClientAsync(string connectionId)
        {
            if (connectionId == null)
                throw new ArgumentNullException(nameof(connectionId));

            return _grainFactory.GetClientGrain(connectionId, _hubName).DeactivateAsync();
        }

        public Task DeactivateGroupAsync(string groupName)
        {
            if (groupName == null)
                throw new ArgumentNullException(nameof(groupName));

            return _grainFactory.GetGroupGrain(groupName, _hubName).DeactivateAsync();
        }

        public Task DeactivateUserAsync(string userId)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));

            return _grainFactory.GetUserGrain(userId, _hubName).DeactivateAsync();
        }

        private void EnsureAllStream()
        {
            if (_allMessageStream != null)
                return;

            if (_streamProvider == null)
                _streamProvider = _streamProviderFactory.Invoke();

            _allMessageStream = _streamProvider
                .GetStream<SendToAllInvocationMessage>(_allStreamKey, OrleansSignalRConstants.AllMessageSendingStreamNamespace);
        }
    }
}