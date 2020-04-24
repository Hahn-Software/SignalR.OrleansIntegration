using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.SignalRIntegration.Core;
using Orleans.SignalRIntegration.Core.Abstractions;
using Orleans.SignalRIntegration.Core.Abstractions.Messages;
using Orleans.Streams;

namespace AspNetCore.SignalR.OrleansIntegration
{
    public class OrleansHubLifetimeManager<THub> : HubLifetimeManager<THub>, IDisposable where THub : Hub
    {
        private readonly Guid _clientStreamId = Guid.NewGuid();

        private readonly IClusterClient _clusterClient;

        private readonly ConcurrentDictionary<string, HubConnectionContext> _connectionsById =
            new ConcurrentDictionary<string, HubConnectionContext>();

        private readonly IOptions<OrleansSignalROptions<THub>> _options;
        private readonly ILogger _logger;
        private StreamSubscriptionHandle<SendToAllInvocationMessage> _allMessageHandle;
        private StreamSubscriptionHandle<SendToClientInvocationMessage> _clientMessageHandle;
        private IAsyncStream<SendToClientInvocationMessage> _clientMessageStream;
        private int _clusterClientConnectionRetryCount;

        private bool _initialized;

        public OrleansHubLifetimeManager(IOptions<OrleansSignalROptions<THub>> options, IHubProxy<THub> hubProxy,
            ILogger<OrleansHubLifetimeManager<THub>> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _clusterClient = options.Value.ClusterClient ?? throw new ArgumentException("You have to provide a cluster client via options.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            HubProxy = hubProxy ?? throw new ArgumentNullException(nameof(hubProxy));
        }

        public OrleansHubLifetimeManager(IOptions<OrleansSignalROptions<THub>> options, IClusterClient clusterClient, IHubProxy<THub> hubProxy,
            ILogger<OrleansHubLifetimeManager<THub>> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _clusterClient = clusterClient ?? throw new ArgumentNullException(nameof(clusterClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            HubProxy = hubProxy ?? throw new ArgumentNullException(nameof(hubProxy));
        }

        public IHubProxy HubProxy { get; }

        public void Dispose()
        {
            if (!_initialized)
                return;

            OrleansHubLifetimeManagerLog.Unsubscribe(_logger, _clientStreamId);

            var streamProvider = _clusterClient.GetStreamProvider(OrleansSignalRConstants.StreamProviderName);

            var tasks = _connectionsById.Keys.Select(connectionId => streamProvider
                .GetStream<EventArgs>(OrleansSignalRConstants.DisconnectionStreamId, connectionId)
                .OnNextAsync(EventArgs.Empty));

            Task.WaitAll(tasks.ToArray());

            Task.WaitAll(
                _allMessageHandle?.UnsubscribeAsync() ?? Task.CompletedTask,
                _clientMessageHandle?.UnsubscribeAsync() ?? Task.CompletedTask);
        }

        public override async Task OnConnectedAsync(HubConnectionContext connection)
        {
            var connectionId = connection.ConnectionId;
            _connectionsById.TryAdd(connectionId, connection);

            try
            {
                if (!_initialized)
                {
                    _initialized = true;
                    await InitializeAsync();
                }

                await EnsureOrleansClusterConnection();

                await HubProxy.OnConnectedAsync(_clientStreamId, connectionId, connection.UserIdentifier);
            }
            catch
            {
                _connectionsById.TryRemove(connectionId, out _);
                throw;
            }
        }

        public override async Task OnDisconnectedAsync(HubConnectionContext connection)
        {
            var connectionId = connection.ConnectionId;
            
            try
            {
                await HubProxy.OnDisconnectedAsync(connectionId, connection.UserIdentifier);
            }
            finally
            {
                _connectionsById.TryRemove(connectionId, out _);
            }
        }

        public override Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            return HubProxy.AddToGroupAsync(connectionId, groupName);
        }

        public override Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            return HubProxy.RemoveFromGroupAsync(connectionId, groupName);
        }

        public override Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            return HubProxy.SendAllAsync(methodName, args);
        }

        public override Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds,
            CancellationToken cancellationToken = default)
        {
            return HubProxy.SendAllExceptAsync(methodName, excludedConnectionIds, args);
        }

        public override Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            return HubProxy.SendClientAsync(connectionId, methodName, args);
        }

        public override Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args,
            CancellationToken cancellationToken = default)
        {
            return HubProxy.SendClientsAsync(connectionIds, methodName, args);
        }

        public override Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            return HubProxy.SendGroupAsync(groupName, methodName, args);
        }

        public override Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds,
            CancellationToken cancellationToken = default)
        {
            return HubProxy.SendGroupExceptAsync(groupName, methodName, excludedConnectionIds, args);
        }

        public override Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args,
            CancellationToken cancellationToken = default)
        {
            return HubProxy.SendGroupsAsync(groupNames, methodName, args);
        }

        public override Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            return HubProxy.SendUserAsync(userId, methodName, args);
        }

        public override Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args,
            CancellationToken cancellationToken = default)
        {
            return HubProxy.SendUsersAsync(userIds, methodName, args);
        }

        private async Task InitializeAsync()
        {
            await EnsureOrleansClusterConnection();

            OrleansHubLifetimeManagerLog.Subscribing(_logger, _clientStreamId);

            try
            {
                var streamProvider = _clusterClient.GetStreamProvider(OrleansSignalRConstants.StreamProviderName);

                _clientMessageStream = streamProvider
                    .GetStream<SendToClientInvocationMessage>(_clientStreamId, OrleansSignalRConstants.ClientMessageSendingStreamNamespace);
                _clientMessageHandle = await _clientMessageStream.SubscribeWithRetryAsync(4, 20, async (message, token) => await OnReceivedAsync(message));
                _allMessageHandle = await HubProxy.AllMessageStream.SubscribeWithRetryAsync(4, 20, async (message, token) => await OnReceivedAsync(message));
            }
            catch (Exception e)
            {
                OrleansHubLifetimeManagerLog.InternalMessageFailed(_logger, e);
            }
        }

        private async Task EnsureOrleansClusterConnection()
        {
            if (_clusterClient == null)
                throw new NullReferenceException(nameof(_clusterClient));

            if (_clusterClient.IsInitialized)
            {
                OrleansHubLifetimeManagerLog.Connected(_logger);
                return;
            }

            _clusterClientConnectionRetryCount = 0;

            OrleansHubLifetimeManagerLog.NotConnected(_logger);
            await _clusterClient.Connect(async ex =>
            {
                if (_clusterClientConnectionRetryCount >= _options.Value.MaxClusterClientConnectionRetries)
                    throw ex;

                OrleansHubLifetimeManagerLog.ConnectionFailed(_logger, ex);

                await Task.Delay(_options.Value.ClusterClientConnectionRetryInterval);
                _clusterClientConnectionRetryCount++;

                return true;
            });
            OrleansHubLifetimeManagerLog.ConnectionRestored(_logger);
        }

        private async Task OnReceivedAsync(SendToClientInvocationMessage message)
        {
            OrleansHubLifetimeManagerLog.ReceivedFromStream(_logger, _clientStreamId);

            if (_connectionsById.TryGetValue(message.ConnectionId, out var connection))
            {
                var invocationMessage = new InvocationMessage(message.MethodName, message.Args);

                try
                {
                    await connection.WriteAsync(invocationMessage).AsTask();
                }
                catch (Exception e)
                {
                    OrleansHubLifetimeManagerLog.FailedWritingMessage(_logger, e);
                }
            }
        }

        private async Task OnReceivedAsync(SendToAllInvocationMessage message)
        {
            OrleansHubLifetimeManagerLog.ReceivedFromStream(_logger, _clientStreamId);

            var invocationMessage = new InvocationMessage(message.MethodName, message.Args);

            var tasks = _connectionsById
                .Where(pair => !pair.Value.ConnectionAborted.IsCancellationRequested && !message.ExcludedConnectionIds.Contains(pair.Key))
                .Select(pair => pair.Value.WriteAsync(invocationMessage).AsTask());

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                OrleansHubLifetimeManagerLog.FailedWritingMessage(_logger, e);
            }
        }
    }
}