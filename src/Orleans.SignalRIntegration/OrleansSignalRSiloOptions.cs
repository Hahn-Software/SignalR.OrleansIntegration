using Orleans.Configuration;
using Orleans.SignalRIntegration.Core;

namespace Orleans.SignalRIntegration
{
    public class OrleansSignalRSiloOptions
    {
        /// <summary>
        /// Gets or sets a value whether to enable fire and forget delivery for the simple message stream provider
        /// <code>default: <see cref="SimpleMessageStreamProviderOptions.DEFAULT_VALUE_FIRE_AND_FORGET_DELIVERY"/></code>
        /// </summary>
        public bool FireAndForgetDelivery { get; set; } = SimpleMessageStreamProviderOptions.DEFAULT_VALUE_FIRE_AND_FORGET_DELIVERY;

        /// <summary>
        /// Gets or sets a value indicating whether to register the Default Pub/Sub Storage Provider
        /// <para>
        /// If true, registers Memory Storage as default for the PubSubStore. You can disable this behaviour
        /// by setting this to false and register your own storage provider with name "PubSubStore"
        /// </para>
        /// <code>default: true</code>
        /// </summary>
        public bool RegisterDefaultPubSubStorage { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to register the Default SignalR Grain Storage Provider
        /// <para>
        /// If true, registers Memory Storage as default SignalR grain storage. You can disable this
        /// behavior by setting this to false and register your own storage provider with name
        /// <see cref="OrleansSignalRConstants.StorageProviderName"/>
        /// </para>
        /// <code>default: true</code>
        /// </summary>
        public bool RegisterDefaultSignalRGrainStorage { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to add SignalRIntegration assembly parts
        /// <para>
        /// By default, Orleans will scan all assemblies IF no application parts are explicitly added,
        /// in which case having application parts added for <see cref="SignalRIntegration"/> may have
        /// unwanted side effects and cause some grains to not be discovered.
        /// </para>
        /// <code>default: true</code>
        /// </summary>
        public bool AddApplicationParts { get; set; } = true;
    }
}