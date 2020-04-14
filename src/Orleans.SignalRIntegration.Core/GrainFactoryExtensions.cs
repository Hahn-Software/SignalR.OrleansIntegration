using System;
using Orleans.SignalRIntegration.Core.Abstractions;
using Orleans.SignalRIntegration.Core.Abstractions.GrainInterfaces;
using Orleans.Streams;

namespace Orleans.SignalRIntegration.Core
{
    public static class GrainFactoryExtensions
    {
        public const string ClientHubScopeName = "client";
        public const string GroupHubScopeName = "group";
        public const string UserHubScopeName = "user";

        public static IClientGrain GetClientGrain(this IGrainFactory grainFactory, string connectionId, string hubName)
        {
            return grainFactory.GetGrain<IClientGrain>(connectionId.ToGrainHubKey(hubName, ClientHubScopeName));
        }

        public static IGroupGrain GetGroupGrain(this IGrainFactory grainFactory, string groupName, string hubName)
        {
            return grainFactory.GetGrain<IGroupGrain>(groupName.ToGrainHubKey(hubName, GroupHubScopeName));
        }

        public static IUserGrain GetUserGrain(this IGrainFactory grainFactory, string userId, string hubName)
        {
            return grainFactory.GetGrain<IUserGrain>(userId.ToGrainHubKey(hubName, UserHubScopeName));
        }

        public static IHubProxy GetHubProxy(this IGrainFactory grainFactory, IStreamProvider streamProvider, string hubName)
        {
            return new HubProxy(grainFactory, streamProvider, hubName);
        }

        public static IHubProxy GetHubProxy(this IGrainFactory grainFactory, IStreamProvider streamProvider, string hubName, Guid allStreamKey)
        {
            return new HubProxy(grainFactory, streamProvider, hubName, allStreamKey);
        }
    }
}