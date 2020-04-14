namespace Orleans.SignalRIntegration.Core.Abstractions.Messages
{
    public abstract class HubMethodInvocationMessage
    {
        protected HubMethodInvocationMessage(string methodName, object[] args)
        {
            MethodName = methodName;
            Args = args;
        }

        public string MethodName { get; }

        public object[] Args { get; }
    }
}