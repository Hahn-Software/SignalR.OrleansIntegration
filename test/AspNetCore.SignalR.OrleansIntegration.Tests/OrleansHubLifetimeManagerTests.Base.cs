// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using AspNetCore.SignalR.OrleansIntegration.TestUtils.Microsoft;
using Microsoft.AspNetCore.SignalR.Protocol;
using Xunit;

namespace AspNetCore.SignalR.OrleansIntegration.Tests
{
    public partial class OrleansHubLifetimeManagerTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendGroupAsyncWritesToAllConnectionsInGroupOutput(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.OnConnectedAsync(connection2).OrTimeout();

                if (fromManager)
                {
                    await manager.AddToGroupAsync(connection1.ConnectionId, "group").OrTimeout();

                    await manager.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();
                }
                else
                {
                    await hubProxy.AddToGroupAsync(connection1.ConnectionId, "group").OrTimeout();

                    await hubProxy.SendGroupAsync("group", "Hello", "World").OrTimeout();
                }

                await AssertMessageAsync(client1);
                Assert.Null(client2.TryRead());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendGroupExceptAsyncDoesNotWriteToExcludedConnections(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.OnConnectedAsync(connection2).OrTimeout();

                if (fromManager)
                {
                    await manager.AddToGroupAsync(connection1.ConnectionId, "group").OrTimeout();
                    await manager.AddToGroupAsync(connection2.ConnectionId, "group").OrTimeout();

                    await manager.SendGroupExceptAsync("group", "Hello", new object[] {"World"}, new[] {connection2.ConnectionId}).OrTimeout();
                }
                else
                {
                    await hubProxy.AddToGroupAsync(connection1.ConnectionId, "group").OrTimeout();
                    await hubProxy.AddToGroupAsync(connection2.ConnectionId, "group").OrTimeout();

                    await hubProxy.SendGroupExceptAsync("group", "Hello", new[] {connection2.ConnectionId}, "World").OrTimeout();
                }

                await AssertMessageAsync(client1);
                Assert.Null(client2.TryRead());
            }
        }

        // ADDED: SendGroupsAsync
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendGroupAsyncWritesToAllConnectionsInGroupsOutput(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.OnConnectedAsync(connection2).OrTimeout();

                if (fromManager)
                {
                    await manager.AddToGroupAsync(connection1.ConnectionId, "group").OrTimeout();
                    await manager.AddToGroupAsync(connection2.ConnectionId, "group2").OrTimeout();

                    await manager.SendGroupsAsync(new[] {"group", "group2"}, "Hello", new object[] {"World"}).OrTimeout();
                }
                else
                {
                    await hubProxy.AddToGroupAsync(connection1.ConnectionId, "group").OrTimeout();
                    await hubProxy.AddToGroupAsync(connection2.ConnectionId, "group2").OrTimeout();

                    await hubProxy.SendGroupsAsync(new[] {"group", "group2"}, "Hello", "World").OrTimeout();
                }

                await AssertMessageAsync(client1);
                await AssertMessageAsync(client2);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendConnectionAsyncWritesToConnectionOutput(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager.OnConnectedAsync(connection).OrTimeout();

                if (fromManager)
                    await manager.SendConnectionAsync(connection.ConnectionId, "Hello", new object[] {"World"}).OrTimeout();
                else
                    await hubProxy.SendClientAsync(connection.ConnectionId, "Hello", "World").OrTimeout();

                await AssertMessageAsync(client);
            }
        }

        // ADDED: SendConnectionsAsync
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SendConnectionAsyncWritesToConnectionsOutput(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.OnConnectedAsync(connection2).OrTimeout();

                if (fromManager)
                    await manager.SendConnectionsAsync(new[] {connection1.ConnectionId, connection2.ConnectionId}, "Hello",
                        new object[] {"World"}).OrTimeout();
                else
                    await hubProxy.SendClientsAsync(new[] {connection1.ConnectionId, connection2.ConnectionId}, "Hello", "World").OrTimeout();

                await AssertMessageAsync(client1);
                await AssertMessageAsync(client2);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisconnectConnectionRemovesConnectionFromGroup(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager.OnConnectedAsync(connection).OrTimeout();

                if (fromManager)
                    await manager.AddToGroupAsync(connection.ConnectionId, "group").OrTimeout();
                else
                    await hubProxy.AddToGroupAsync(connection.ConnectionId, "group").OrTimeout();

                await manager.OnDisconnectedAsync(connection).OrTimeout();

                if (fromManager)
                    await manager.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();
                else
                    await hubProxy.SendGroupAsync("group", "Hello", "World").OrTimeout();

                Assert.Null(client.TryRead());

                // ADDED: GetConnectionIdsAsync
                //var result = await hubProxy.GetGroupNamesAsync("group").OrTimeout();
                //Assert.Equal(0, result.Count);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task RemoveGroupFromLocalConnectionNotInGroupDoesNothing(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager.OnConnectedAsync(connection).OrTimeout();

                if (fromManager)
                    await manager.RemoveFromGroupAsync(connection.ConnectionId, "group").OrTimeout();
                else
                    await hubProxy.RemoveFromGroupAsync(connection.ConnectionId, "group").OrTimeout();
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AddGroupAsyncForLocalConnectionAlreadyInGroupDoesNothing(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client = new TestClient())
            {
                var connection = HubConnectionContextUtils.Create(client.Connection);

                await manager.OnConnectedAsync(connection).OrTimeout();

                await manager.AddToGroupAsync(connection.ConnectionId, "group").OrTimeout();
                await manager.AddToGroupAsync(connection.ConnectionId, "group").OrTimeout();

                if (fromManager)
                    await manager.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();
                else
                    await hubProxy.SendGroupAsync("group", "Hello", "World").OrTimeout();

                await AssertMessageAsync(client);
                Assert.Null(client.TryRead());

                // ADDED: GetConnectionIdsAsync
                //var result = await hubProxy.GetConnectionIdsFromGroupAsync("group").OrTimeout();
                //Assert.Equal(1, result.Count);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WritingToGroupWithOneConnectionFailingSecondConnectionStillReceivesMessage(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                // Force an exception when writing to connection
                var connectionMock = HubConnectionContextUtils.CreateMock(client1.Connection);

                var connection1 = connectionMock;
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.AddToGroupAsync(connection1.ConnectionId, "group");
                await manager.OnConnectedAsync(connection2).OrTimeout();
                await manager.AddToGroupAsync(connection2.ConnectionId, "group");

                if (fromManager)
                    await manager.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();
                else
                    await hubProxy.SendGroupAsync("group", "Hello", "World").OrTimeout();

                // connection1 will throw when receiving a group message, we are making sure other connections
                // are not affected by another connection throwing
                await AssertMessageAsync(client2);

                // Repeat to check that group can still be sent to
                await manager.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();
                await AssertMessageAsync(client2);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task InvokeUserSendsToAllConnectionsForUser(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            using (var client3 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection, userIdentifier: "userA");
                var connection2 = HubConnectionContextUtils.Create(client2.Connection, userIdentifier: "userA");
                var connection3 = HubConnectionContextUtils.Create(client3.Connection, userIdentifier: "userB");

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.OnConnectedAsync(connection2).OrTimeout();
                await manager.OnConnectedAsync(connection3).OrTimeout();

                if (fromManager)
                    await manager.SendUserAsync("userA", "Hello", new object[] {"World"}).OrTimeout();
                else
                    await hubProxy.SendUserAsync("userA", "Hello", "World").OrTimeout();

                await AssertMessageAsync(client1);
                await AssertMessageAsync(client2);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task StillSubscribedToUserAfterOneOfMultipleConnectionsAssociatedWithUserDisconnects(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            using (var client3 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection, userIdentifier: "userA");
                var connection2 = HubConnectionContextUtils.Create(client2.Connection, userIdentifier: "userA");
                var connection3 = HubConnectionContextUtils.Create(client3.Connection, userIdentifier: "userB");

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.OnConnectedAsync(connection2).OrTimeout();
                await manager.OnConnectedAsync(connection3).OrTimeout();

                if (fromManager)
                    await manager.SendUserAsync("userA", "Hello", new object[] {"World"}).OrTimeout();
                else
                    await hubProxy.SendUserAsync("userA", "Hello", "World").OrTimeout();

                await AssertMessageAsync(client1);
                await AssertMessageAsync(client2);

                // Disconnect one connection for the user
                await manager.OnDisconnectedAsync(connection1).OrTimeout();

                _output.WriteLine("Sending.");
                if (fromManager)
                    await manager.SendUserAsync("userA", "Hello", new object[] {"World"}).OrTimeout();
                else
                    await hubProxy.SendUserAsync("userA", "Hello", "World").OrTimeout();

                _output.WriteLine("Sent.");
                await AssertMessageAsync(client2);
            }
        }

        // ADDED
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task StillSubscribedToGroupAfterOneOfMultipleConnectionsAssociatedWithGroupDisconnects(bool fromManager)
        {
            var hubProxy = new HubProxy<MyHub>(_fixture.TestCluster.Client);
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.OnConnectedAsync(connection2).OrTimeout();

                if (fromManager)
                {
                    await manager.AddToGroupAsync(connection2.ConnectionId, "group");
                    await manager.AddToGroupAsync(connection1.ConnectionId, "group");
                }
                else
                {
                    await hubProxy.AddToGroupAsync(connection1.ConnectionId, "group");
                    await hubProxy.AddToGroupAsync(connection2.ConnectionId, "group");
                }

                // Disconnect one connection for the group
                await manager.OnDisconnectedAsync(connection1).OrTimeout();
                if (fromManager)
                    await manager.SendGroupAsync("group", "Hello", new object[] {"World"}).OrTimeout();
                else
                    await hubProxy.SendGroupAsync("group", "Hello", "World").OrTimeout();

                await AssertMessageAsync(client2);
            }
        }

        [Fact]
        public async Task SendAllAsyncDoesNotWriteToDisconnectedConnectionsOutput()
        {
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.OnConnectedAsync(connection2).OrTimeout();

                await manager.OnDisconnectedAsync(connection2).OrTimeout();

                await manager.SendAllAsync("Hello", new object[] {"World"}).OrTimeout();

                await AssertMessageAsync(client1);
                Assert.Null(client2.TryRead());
            }
        }

        [Fact]
        public async Task SendAllAsyncWritesToAllConnectionsOutput()
        {
            using (var manager = CreateNewHubLifetimeManager<MyHub>())
            using (var client1 = new TestClient())
            using (var client2 = new TestClient())
            {
                var connection1 = HubConnectionContextUtils.Create(client1.Connection);
                var connection2 = HubConnectionContextUtils.Create(client2.Connection);

                await manager.OnConnectedAsync(connection1).OrTimeout();
                await manager.OnConnectedAsync(connection2).OrTimeout();

                await manager.SendAllAsync("Hello", new object[] {"World"}).OrTimeout();

                var message = Assert.IsType<InvocationMessage>(client1.TryRead());
                Assert.Equal("Hello", message.Target);
                Assert.Single(message.Arguments);
                Assert.Equal("World", (string) message.Arguments[0]);

                await AssertMessageAsync(client2);
            }
        }
    }
}