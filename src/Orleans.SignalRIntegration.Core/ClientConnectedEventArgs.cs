using System;

namespace Orleans.SignalRIntegration.Core
{
    public class ClientConnectedEventArgs : EventArgs
    {
        public ClientConnectedEventArgs(string connectionId, string userId)
        {
            ConnectionId = connectionId;
            UserId = userId;
        }

        public string ConnectionId { get; }

        public string UserId { get; }
    }
}