using System;

namespace AspNetCore.SignalR.OrleansIntegration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class HubAllStreamKeyAttribute : Attribute
    {
        public HubAllStreamKeyAttribute(string guid)
        {
            Guid = Guid.Parse(guid);
        }

        public Guid Guid { get; set; }
    }
}