using System;
using System.Reflection;
using Microsoft.AspNetCore.SignalR;
using Orleans.SignalRIntegration.Core;

namespace AspNetCore.SignalR.OrleansIntegration
{
    public static class HubExtensions
    {
        public static string GetHubName<T>(this T hub)
            where T : Hub
        {
            return GetHubName(hub.GetType());
        }

        public static string GetHubName(this Type hubType)
        {
            var hubNameAttribute = hubType.GetCustomAttribute<HubNameAttribute>();

            if (hubNameAttribute == null)
                return hubType.Name;

            return hubNameAttribute.Name;
        }

        public static Guid GetAllStreamKey<T>(this T hub)
            where T : Hub
        {
            return GetAllStreamKey(hub.GetType());
        }

        public static Guid GetAllStreamKey(this Type hubType)
        {
            var allStreamKeyAttribute = hubType.GetCustomAttribute<HubAllStreamKeyAttribute>();

            if (allStreamKeyAttribute == null)
                return GuidUtility.Create(OrleansSignalRConstants.DefaultAllStreamKey, GetHubName(hubType));

            return allStreamKeyAttribute.Guid;
        }
    }
}