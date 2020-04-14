using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.TestingHost;

namespace AspNetCore.SignalR.OrleansIntegration.Tests.BuilderConfigurators
{
    public class TestClientBuilderConfigurator : IClientBuilderConfigurator
    {
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
        {
            clientBuilder.UseSignalR();
        }
    }
}