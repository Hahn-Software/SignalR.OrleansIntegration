using AspNetCore.SignalR.OrleansIntegration;
using Microsoft.AspNetCore.SignalR;
using SampleApp.Abstractions;

namespace SampleApp.Web.Hubs
{
    [HubName(HubNames.AnotherHub)]
    public class AnotherHub : Hub
    {
    }
}