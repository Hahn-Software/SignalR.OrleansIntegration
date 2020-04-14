using System;

namespace Orleans.SignalRIntegration.Core
{
    public static class OrleansSignalRConstants
    {
        public const string NamingPrefix = "ORLEANS_SIGNALR_INTEGRATION_";

        public const string StreamProviderName = NamingPrefix + "STREAM_PROVIDER";

        public const string StorageProviderName = NamingPrefix + "STORAGE_PROVIDER";

        public const string ClientMessageSendingStreamNamespace = NamingPrefix + "SEND_CLIENT_MESSAGE_STREAM";

        public const string AllMessageSendingStreamNamespace = NamingPrefix + "SEND_ALL_MESSAGE_STREAM";

        public static readonly Guid DefaultAllStreamKey = Guid.Parse("F8D2DAEF-48DB-4006-A23B-9697EEFC9CAD");

        public static readonly Guid DisconnectionStreamId = Guid.Parse("F7469960-33C7-4214-B61A-3C917C8B5101");
    }
}