using System.Net;

public partial class FixUdp(ITestOutputHelper output)
{
    [Fact]
    public void OpenAndClose()
    {
        Client();
        Server();

        async void Client()
        {
            var host = HostManager.GenerateLocalHost();

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

            Thread.Sleep(millisecondsTimeout: 100);
            {
                Assert.True(client.IsOpened);
                Assert.True(isModify);
                Assert.True(isOpen);
                Assert.False(isClose);
                Assert.False(isError);
            }

            await client.To.Close();

            Thread.Sleep(millisecondsTimeout: 300);
            {
                Assert.False(client.IsOpened);
                Assert.True(isModify);
                Assert.True(isOpen);
                Assert.True(isClose);
                Assert.False(isError);
            }

            await client.To.Open(new Host(IPAddress.Any, 0));
            {
                Assert.False(client.IsOpened);
                Assert.True(isModify);
                Assert.True(isOpen);
                Assert.True(isClose);
                Assert.True(isError);
            }

            await client.To.Close();
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

            Thread.Sleep(millisecondsTimeout: 100);
            {
                Assert.True(server.IsOpened);
                Assert.True(isModify);
                Assert.True(isOpen);
                Assert.False(isClose);
                Assert.False(isError);
            }

            await server.To.Close();

            Thread.Sleep(millisecondsTimeout: 300);
            {
                Assert.False(server.IsOpened);
                Assert.True(isModify);
                Assert.True(isOpen);
                Assert.True(isClose);
                Assert.False(isError);
            }

            // Cannot assign requested address
            await server.To.Open(new Host("1.1.1.1", 0));
            {
                Assert.False(server.IsOpened);
                Assert.True(isModify);
                Assert.True(isOpen);
                Assert.True(isClose);
                Assert.True(isError);
            }

            await server.To.Close();
        }
    }
}