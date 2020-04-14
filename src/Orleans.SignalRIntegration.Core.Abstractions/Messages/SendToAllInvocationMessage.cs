using System.Collections.Generic;

namespace Orleans.SignalRIntegration.Core.Abstractions.Messages
{
    public class SendToAllInvocationMessage : HubMethodInvocationMessage
    {
        public SendToAllInvocationMessage(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds)
            : base(methodName, args)
        {
            ExcludedConnectionIds = excludedConnectionIds;
        }

        public IReadOnlyList<string> ExcludedConnectionIds { get; }
    }
}