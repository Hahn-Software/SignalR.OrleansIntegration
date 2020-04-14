namespace Orleans.SignalRIntegration.Core.Abstractions.Messages
{
    public class SendToClientInvocationMessage : HubMethodInvocationMessage
    {
        public SendToClientInvocationMessage(string methodName, object[] args, string connectionId)
            : base(methodName, args)
        {
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; }
    }
}