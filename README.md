# SignalR.OrleansIntegration

An integration between AspNetCore.SignalR and Orleans to allow communication between clients distributed across multiple
servers and backend services.

Originally inspired by [AspNetCore.SignalR.Orleans](https://github.com/slango0513/AspNetCore.SignalR.Orleans) and 
[SignalR.Orleans](https://github.com/OrleansContrib/SignalR.Orleans).

The previous implementations lacked some functionality and handling mechanisms required by our applications so
we created a new implementation based on the other projects merging in new functionality and taking the existing
functionality to a new level suitable for our internal applications.

The integration plugin is pushing the SignalR communication between different SignalR clients and backend server
parts onto Orleans streams and enables seamless communication between the servers and clients distributed across
multiple servers and clustered services.

SignalR Hubs are enabled to communicate with clients connected to different servers and can synchronize their
messages to huge applications. Also any Orleans Grain is able to access SignalR Hub Proxies for sending
messages from distributed backend services to all connected clients. It's also possible to send messages
to specific groups of clients and also single clients or users connected with multiple clients.

For enabling groups of clients the implementation pushes group memberships onto Orleans Grain states decoupling
the grouping mechanism from SignalR. This way it's also possible to add clients to groups from other service
parts outside the hubs.

## Setting up the integration plugins

### Web / Client project (SignalR hub host)
Install the integration plugin using [NuGet](https://www.nuget.org/packages?q=AspNetCore.SignalR.OrleansIntegration):
```
Install-Package AspNetCore.SignalR.OrleansIntegration
```
or
```
dotnet add package AspNetCore.SignalR.OrleansIntegration
```

Next you have to add an Orleans cluster client (IClusterClient) to your service collection. It's up to you how
to configure or create your cluster client. You're only required to include a call to `.UseSignalR()` in your
cluster client setup:
```csharp
var clusterClient = new ClientBuilder()
    .UseSignalR()
    // ...
    .Build();

services.AddSingleton(clusterClient);
```

In your SignalR service registration you also need to add the Orleans integration for each Hub you want to 
integrate:
```csharp
services.AddSignalR()
    .AddOrleans<ChatHub>()
    .AddOrleans<AnotherHub>();
```

If necessary you can configure a specific cluster client to be used for a hub but this step is not required
if you're good with your previously registered cluster client from MS DI.
```csharp
.AddOrleans<ChatHub>(options => options.ClusterClient = clusterClient)
```

Now that you've set up all the integration it's time to add identifiers to your hubs in order to be able to
get proxies for them in the backend code. You can put the identifiers in shared common libraries or hardcode
them. You can add an identifier using the `HubName`-Attribute:
```csharp
// Share this class between projects
public static class HubNames {
    public const string ChatHub = "MyChatHub";
}



[HubName(HubNames.ChatHub)]
public class ChatHub : Hub
{
    // ...
}

```
Notice: If you don't provide a name for your hub class it's using the class name by default. But it's
recommended to add a dedicated name to each hub in order to prevent issues with renaming classes or 
problems introduced by multiple hubs with the same class name.


### Orleans Server project (Orleans silo host)

Install the integration plugin using [NuGet](https://www.nuget.org/packages?q=Orleans.SignalRIntegration):
```
Install-Package Orleans.SignalRIntegration
```
or
```
dotnet add package Orleans.SignalRIntegration
```

On your silo host builder add a call to `.UseSignalR()` like for the orleans client in the SignalR Host App.
```csharp
var siloHost = new SiloHostBuilder()
    .UseSignalR()
    // ...
    .Build();
```

That's all. You're ready to start using your new cluster-able multi server SignalR services.

Note that by default the Orleans plugin is registering Memory Storage as default for the `PubSubStore`. You
can disable this behaviour by passing in a parameter to the `UseSignalR` call on the silo host builder. 
Then you are able to register your own storage providers for the the mentioned grain storage provider:
```csharp
const string invariant = "System.Data.SqlClient";
var connectionString = _configuration.GetConnectionString("Orleans");

var siloHost = new SiloHostBuilder()
    .UseSignalR(false)
    .AddAdoNetGrainStorage("PubSubStore", options =>
    {
        options.Invariant = invariant;
        options.ConnectionString = connectionString;
        options.UseJsonFormat = true;
    })
    // ...
    .Build();
```

## Using the hub proxy

### Orleans Grains
You can retrieve SignalR hub proxies using your well known GrainFactory of every Grain.
Use the `OnActivateAsync` method to retrieve your proxies. After retrieving them you can call any
method for sending messages to clients you already know from SignalR.

```csharp
public class ChatChannelGrainState
{
    public ISet<string> ChannelMembers { get; set; } = new HashSet<string>();
}

public class ChatChannelGrain : Grain<ChatChannelGrainState>, IChatChannelGrain
{
    private IHubProxy _hubProxy;
    private IHubProxy _anotherHubProxy;

    public override Task OnActivateAsync()
    {
        // For now there's no easy way for us to access the GetStreamProvider method from our integration
        // so you have to retrieve the stream provider yourself.
        var streamProvider = GetStreamProvider(SignalRConstants.STREAM_PROVIDER);

        // Retrieve some hub proxies
        _hubProxy = GrainFactory.GetHubProxy(streamProvider, Guid.Parse(HubNames.ChatHub));
        _anotherHubProxy = GrainFactory.GetHubProxy(streamProvider, Guid.Parse(HubNames.AnotherHub));

        return base.OnActivateAsync();
    }

    public async Task AddToGroupWithConditionAsync(string connectionId, string groupName)
    {
        // Maybe limit the number of members in a channel to 5. Why not.
        if (State.ChannelMembers.Count <= 5)
        {
            // Add a client (identified by the connection id) to a group
            await _hubProxy.AddToGroupAsync(connectionId, groupName);

            // Also save the member list to our ChatChannel state
            State.ChannelMembers.Add(connectionId);
        }
    }

    public Task NotifyAllExceptCurrentMembersAsync()
    {
        // Send a message to all clients except our own channel members.
        // There are plenty more methods for sending messages. See the documentation further below.
        return _hubProxy.SendAllExceptAsync("ReceiveMessage", State.ChannelMembers.ToList(), "Server", "Hello, Client!");
    }

    public Task NotifyUserFromAnotherHubAsync(string userId)
    {
        // Send a message to a specific user on another hub.
        return _anotherHubProxy.SendUserAsync(userId, "ReceiveMessage", "Hello, User!");
    }
}
```


### SignalR Hubs
The following example shows how to send messages to the same or other hubs utilizing the generic IHubProxy.
```cs
[HubName(HubNames.ChatHub)]
public class ChatHub : Hub
{
    private readonly IHubProxy<SampleHub> _hubProxy;
    private readonly IHubProxy<AnotherSampleHub> _anotherHubProxy;

    // Just inject some hub proxies like every other service in ASP.NET Core
    public SampleHub(IHubProxy<ChatHub> hubProxy, IHubProxy<AnotherHub> anotherHubProxy)
    {
        _hubProxy = hubProxy;
        _anotherHubProxy = anotherHubProxy;
    }

    public Task NotifyUserFromAnotherHubAsync(string userId)
    {
        // Use the same methods like on the hub proxy in Orleans Grains. They're the same abstraction!
        return _anotherHubProxy.SendUserAsync(userId, "ReceiveMessage", "Server", "Hello, user!");
    }
}
```

### Any other code of your application

As long as you're inside of your SignalR host application use it like in the example for SignalR Hubs a few lines above.
You can inject it and you're good to go.

For your other distributed backend services you may use a cluster client and grains for proxying in the messages.
But you're also able to add the hub proxy to your dependency injection by using the `HubProxy` class as implementation
for `IHubProxy`.
Create a factory method and construct a hub proxy using the constructor of your choice. For example:
```csharp
public HubProxy(IGrainFactory grainFactory, IStreamProvider streamProvider, string hubName)
```
You can pass in the IClusterClient as IGrainFactory without a problem and you can use the IHubProxy in every
part of your application.


## IHubProxy

Any hub proxy you're using with the integration plugins implement the same `IHubProxy` interface and therefore share
the same abstractions of SignalR hubs.
For convenience it's recommended to use the patterns described in the samples above to retrieve your IHubProxy.

```csharp
public interface IHubProxy
{
    // ... Some method definitions which are mainly used for internal processes are omitted for clarity.

    Task AddToGroupAsync(string connectionId, string groupName);
    
    Task RemoveFromGroupAsync(string connectionId, string groupName);
    
    Task SendAllAsync(string method, params object[] args);
    
    Task SendAllExceptAsync(string method, IEnumerable<string> excludedConnectionIds, params object[] args);
    
    Task SendClientAsync(string connectionId, string method, params object[] args);
    
    Task SendClientsAsync(IEnumerable<string> connectionIds, string method, params object[] args);
    
    Task SendClientsExceptAsync(IEnumerable<string> connectionIds, string method, IEnumerable<string> excludedConnectionIds, params object[] args);
    
    Task SendGroupAsync(string groupName, string method, params object[] args);
    
    Task SendGroupExceptAsync(string groupName, string method, IEnumerable<string> excludedConnectionIds, params object[] args);
    
    Task SendGroupsAsync(IEnumerable<string> groupNames, string method, params object[] args);
    
    Task SendUserAsync(string userId, string method, params object[] args);
    
    Task SendUsersAsync(IEnumerable<string> userIds, string method, params object[] args);
    
    Task SendUsersExceptAsync(IEnumerable<string> userIds, string method, IEnumerable<string> excludedConnectionIds, params object[] args);
}
```

## Planned Features

* Events / Streams for connecting and disconnecting clients in order to use those for processing in backend
* Dynamic API like SignalR's .Clients.All.SomeMethod(arg1, arg2, ...)
* API for strongly typed Clients
* Automated Stream Provider resolution in Grains
* Advanced logging and metric mechanisms
* Infrastructure Stream for providing information about active SignalR hosts
* More error handling possibilities
* More documentation
* More automated tests
* More samples