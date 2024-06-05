using System.Net;

public partial class FixUdp
{
    [Fact]
    public void ServerAccept()
    {
        // Skip macOS Test: Firewall isn't allowing UDP connection.
        if (OperatingSystem.IsMacOS())
        {
            // https://github.com/dotnet/runtime/issues/97718
            // https://support.apple.com/guide/mac-help/change-firewall-settings-on-mac-mh11783/mac
            output.WriteLine("macOS is skipped because firewall problem");
            return;
        }

        Server();

        async Task Client(Host host)
        {
            UDP.Client client = new();

            bool isOpen = false, isClose = false, isError = false, isModify = false;

            client.On.Open(() => isOpen = true);
            client.On.Close(() => isClose = true);
            client.On.Error(_ => isError = true);
            client.On.Modify(_ => isModify = true);
            {
                Assert.False(client.IsOpened);
                Assert.False(isOpen);
                Assert.False(isClose);
                Assert.False(isError);
                Assert.False(isModify);
            }

            await client.To.Open(host);

            client.To.Data(Guid.NewGuid().ToString());

            Thread.Sleep(millisecondsTimeout: 10);
            {
                Assert.True(client.IsOpened);
                Assert.True(isModify);
                Assert.True(isOpen);
                Assert.False(isClose);
                Assert.False(isError);
            }
        }

        async void Server()
        {
            var host = HostManager.GenerateLocalHost();

            UDP.Server server = new();

            bool isOpen = false, isClose = false, isError = false, isModify = false;

            server.On.Open(() => isOpen = true);
            server.On.Close(() => isClose = true);
            server.On.Modify(_ => isModify = true);
            server.On.Error(exception =>
            {
                isError = true;
                output.WriteLine(exception.ToString());
            });

            {
                Assert.False(server.IsOpened);
                Assert.False(isOpen);
                Assert.False(isClose);
                Assert.False(isError);
                Assert.False(isModify);
            }

            await server.To.Open(host);

            Thread.Sleep(millisecondsTimeout: 10);
            {
                Assert.True(server.IsOpened);
                Assert.True(isModify);
                Assert.True(isOpen);
                Assert.False(isClose);
                Assert.False(isError);
            }

            const int maxConnection = 100;

            for (int i = 0; i < maxConnection; i++)
            {
                await Client(server.Host);
            }

            Thread.Sleep(1000);

            Assert.Equal(maxConnection, server.Clients.Length);
        }
    }
}