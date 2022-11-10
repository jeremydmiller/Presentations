using Alba;
using Microsoft.Extensions.Hosting;

namespace RideSharing.Tests;

public class AppFixture : IAsyncLifetime
{
    public IAlbaHost Host { get; private set; }

    public IServiceProvider Services => Host.Services;
    
    public async Task InitializeAsync()
    {
        Host = await AlbaHost.For<Program>(_ => {});
    }

    public Task DisposeAsync()
    {
        return Host.StopAsync();
    }
}