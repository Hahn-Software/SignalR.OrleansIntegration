using System;
using Microsoft.Extensions.Logging;

namespace AspNetCore.SignalR.OrleansIntegration
{
    internal static class OrleansHubLifetimeManagerLog
    {
        private static readonly Action<ILogger, Exception> ConnectedMessage =
            LoggerMessage.Define(LogLevel.Information, new EventId(2, "Connected"), "Connected to Orleans.");

        private static readonly Action<ILogger, Guid, Exception> SubscribingMessage =
            LoggerMessage.Define<Guid>(LogLevel.Trace, new EventId(3, "Subscribing"), "Subscribing to stream: {Stream}.");

        private static readonly Action<ILogger, Guid, Exception> ReceivedFromStreamMessage =
            LoggerMessage.Define<Guid>(LogLevel.Trace, new EventId(4, "ReceivedFromStream"), "Received message from Orleans stream {Stream}.");

        private static readonly Action<ILogger, Guid, Exception> UnsubscribeMessage =
            LoggerMessage.Define<Guid>(LogLevel.Trace, new EventId(6, "Unsubscribe"), "Unsubscribing from stream: {Stream}.");

        private static readonly Action<ILogger, Exception> NotConnectedMessage =
            LoggerMessage.Define(LogLevel.Error, new EventId(7, "Connected"), "Not connected to Orleans.");

        private static readonly Action<ILogger, Exception> ConnectionRestoredMessage =
            LoggerMessage.Define(LogLevel.Information, new EventId(8, "ConnectionRestored"), "Connection to Orleans restored.");

        private static readonly Action<ILogger, Exception> ConnectionFailedMessage =
            LoggerMessage.Define(LogLevel.Error, new EventId(9, "ConnectionFailed"), "Connection to Orleans failed.");

        private static readonly Action<ILogger, Exception> FailedWritingMessageMessage =
            LoggerMessage.Define(LogLevel.Warning, new EventId(10, "FailedWritingMessage"), "Failed writing message.");

        private static readonly Action<ILogger, Exception> InternalMessageFailedMessage =
            LoggerMessage.Define(LogLevel.Warning, new EventId(11, "InternalMessageFailed"), "Error processing message for internal server message.");

        public static void Connected(ILogger logger)
        {
            ConnectedMessage(logger, null);
        }

        public static void Subscribing(ILogger logger, Guid streamId)
        {
            SubscribingMessage(logger, streamId, null);
        }

        public static void ReceivedFromStream(ILogger logger, Guid streamId)
        {
            ReceivedFromStreamMessage(logger, streamId, null);
        }

        public static void Unsubscribe(ILogger logger, Guid streamId)
        {
            UnsubscribeMessage(logger, streamId, null);
        }

        public static void NotConnected(ILogger logger)
        {
            NotConnectedMessage(logger, null);
        }

        public static void ConnectionRestored(ILogger logger)
        {
            ConnectionRestoredMessage(logger, null);
        }

        public static void ConnectionFailed(ILogger logger, Exception exception)
        {
            ConnectionFailedMessage(logger, exception);
        }

        public static void FailedWritingMessage(ILogger logger, Exception exception)
        {
            FailedWritingMessageMessage(logger, exception);
        }

        public static void InternalMessageFailed(ILogger logger, Exception exception)
        {
            InternalMessageFailedMessage(logger, exception);
        }
    }
}