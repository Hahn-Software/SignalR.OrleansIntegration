using System;
using Microsoft.Extensions.Logging;

namespace AspNetCore.SignalR.OrleansIntegration
{
    internal static class OrleansHubLifetimeManagerLog
    {
        private static readonly Action<ILogger, Exception> _connected =
            LoggerMessage.Define(LogLevel.Information, new EventId(2, "Connected"), "Connected to Orleans.");

        private static readonly Action<ILogger, Guid, Exception> _subscribing =
            LoggerMessage.Define<Guid>(LogLevel.Trace, new EventId(3, "Subscribing"), "Subscribing to stream: {Stream}.");

        private static readonly Action<ILogger, Guid, Exception> _receivedFromStream =
            LoggerMessage.Define<Guid>(LogLevel.Trace, new EventId(4, "ReceivedFromStream"), "Received message from Orleans stream {Stream}.");

        private static readonly Action<ILogger, Guid, Exception> _unsubscribe =
            LoggerMessage.Define<Guid>(LogLevel.Trace, new EventId(6, "Unsubscribe"), "Unsubscribing from stream: {Stream}.");

        private static readonly Action<ILogger, Exception> _notConnected =
            LoggerMessage.Define(LogLevel.Error, new EventId(7, "Connected"), "Not connected to Orleans.");

        private static readonly Action<ILogger, Exception> _connectionRestored =
            LoggerMessage.Define(LogLevel.Information, new EventId(8, "ConnectionRestored"), "Connection to Orleans restored.");

        private static readonly Action<ILogger, Exception> _connectionFailed =
            LoggerMessage.Define(LogLevel.Error, new EventId(9, "ConnectionFailed"), "Connection to Orleans failed.");

        private static readonly Action<ILogger, Exception> _failedWritingMessage =
            LoggerMessage.Define(LogLevel.Warning, new EventId(10, "FailedWritingMessage"), "Failed writing message.");

        private static readonly Action<ILogger, Exception> _internalMessageFailed =
            LoggerMessage.Define(LogLevel.Warning, new EventId(11, "InternalMessageFailed"), "Error processing message for internal server message.");

        public static void Connected(ILogger logger)
        {
            _connected(logger, null);
        }

        public static void Subscribing(ILogger logger, Guid streamId)
        {
            _subscribing(logger, streamId, null);
        }

        public static void ReceivedFromStream(ILogger logger, Guid streamId)
        {
            _receivedFromStream(logger, streamId, null);
        }

        public static void Unsubscribe(ILogger logger, Guid streamId)
        {
            _unsubscribe(logger, streamId, null);
        }

        public static void NotConnected(ILogger logger)
        {
            _notConnected(logger, null);
        }

        public static void ConnectionRestored(ILogger logger)
        {
            _connectionRestored(logger, null);
        }

        public static void ConnectionFailed(ILogger logger, Exception exception)
        {
            _connectionFailed(logger, exception);
        }

        public static void FailedWritingMessage(ILogger logger, Exception exception)
        {
            _failedWritingMessage(logger, exception);
        }

        public static void InternalMessageFailed(ILogger logger, Exception exception)
        {
            _internalMessageFailed(logger, exception);
        }
    }
}