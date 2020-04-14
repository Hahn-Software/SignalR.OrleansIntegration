using System;

namespace Orleans.SignalRIntegration.Core
{
    public class ClientDisconnectedEventArgs : EventArgs
    {
        public ClientDisconnectedEventArgs(string connectionId, string userId)
        {
            ConnectionId = connectionId;
            UserId = userId;
        }

        public string ConnectionId { get; }

        public string UserId { get; }
    }
}