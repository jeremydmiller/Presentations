using Marten;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace RideSharing.Tests;

public class appending_events
{
    private readonly ITestOutputHelper _output;

    public appending_events(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task append_some_events()
    {
        // Pretend this refers to an existing driver in the system
        Guid driverId = Guid.NewGuid();

        using var store =
            DocumentStore.For(
                "Host=localhost;Port=5432;Database=marten_testing;Username=postgres;password=postgres;Command Timeout=5");

        await using var session = store.LightweightSession();

        var shiftId = Guid.NewGuid();

        // Start a brand new event stream
        var event1 = new ShiftStarted(driverId, "Big Car", "98052");
        var event2 = new DriverLocated(new Location(47.673988, -122.121513));
        session.Events.StartStream(shiftId, event1, event2);

        await session.SaveChangesAsync();

        // Append more events to the new stream later
        session.Events.Append(shiftId, new DriverReady(new Location(47.7, -122.13)));
        await session.SaveChangesAsync();
        
        // Append yet another event later
        session.Events.Append(shiftId, new RideAccepted(Guid.NewGuid(), new Location(47.7, -122.13)));
        await session.SaveChangesAsync();

        var events = await session.Events.FetchStreamAsync(shiftId);

        foreach (var @event in events)
        {
            _output.WriteLine(JsonConvert.SerializeObject(@event, Formatting.Indented));
            _output.WriteLine("");
        }
    }
}