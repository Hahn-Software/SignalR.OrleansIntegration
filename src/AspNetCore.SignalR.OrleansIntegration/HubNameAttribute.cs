using System;

namespace AspNetCore.SignalR.OrleansIntegration
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class HubNameAttribute : Attribute
    {
        public HubNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}