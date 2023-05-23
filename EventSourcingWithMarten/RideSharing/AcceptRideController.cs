using Marten;
using Marten.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace RideSharing;

public record AcceptRide(Guid DriverShiftId, Guid RideId, int Version);

public class AcceptRideController : ControllerBase
{
    [HttpPost("/ride/accept")]
    public async Task AcceptRide(
        [FromBody] AcceptRide command,
        [FromServices] IDocumentSession session)
    {
        var aggregate = await session.LoadAsync<DriverShift>(command.DriverShiftId);

        // Should also validate the DriverShift actually exists too!
        if (aggregate.Status != DriverStatus.Ready)
        {
            throw new Exception("I'm not ready!");
        }

        var accepted = new RideAccepted(command.RideId, aggregate.Location);
            
        // Append an event with optimistic concurrency
        session.Events.Append(command.DriverShiftId, command.Version + 1, accepted);

        await session.SaveChangesAsync();
    }
    
    
    
    
    [HttpPost("/ride/accept2")]
    public async Task AcceptRide2(
        [FromBody] AcceptRide command,
        [FromServices] IDocumentSession session)
    {
        var stream = await session
            .Events
            .FetchForWriting<DriverShift>(command.DriverShiftId, command.Version);

        var aggregate = stream.Aggregate;

        if (aggregate.Status != DriverStatus.Ready)
        {
            throw new Exception("I'm not ready!");
        }

        var accept = new RideAccepted(command.RideId, aggregate.Location);
        
        // Append an event with optimistic concurrency
        stream.AppendOne(accept);

        await session.SaveChangesAsync();
    }
    
    [HttpPost("/ride/accept3")]
    public Task AcceptRide3(
        [FromBody] AcceptRide command,
        [FromServices] IDocumentSession session)
    {
        return session
            .Events
            .WriteToAggregate<DriverShift>(command.DriverShiftId, command.Version, stream =>
            {
                JustTheDecision(command, stream);
            });
    }

    public static void JustTheDecision(AcceptRide command, IEventStream<DriverShift> stream)
    {
        var aggregate = stream.Aggregate;

        if (aggregate.Status != DriverStatus.Ready)
        {
            throw new Exception("I'm not ready!");
        }

        var accept = new RideAccepted(command.RideId, aggregate.Location);

        // Append an event with optimistic concurrency
        stream.AppendOne(accept);
    }

    [HttpPost("/ride/accept4")]
    public Task AcceptRide4(
        [FromBody] AcceptRide command,
        [FromServices] IMessageBus bus) =>
        bus.InvokeAsync(command);
}

public static class AcceptRideAggregateHandler
{
    // Wolverine calls this, this is the "Decider" pattern
    public static IEnumerable<object> Handle(AcceptRide command, DriverShift aggregate)
    {
        if (aggregate.Status != DriverStatus.Ready)
        {
            throw new Exception("I'm not ready!");
        }

        yield return new RideAccepted(command.RideId, aggregate.Location);
    }
}

public class RideAcceptedHandler
{
    public void Handle(RideAccepted command, ILogger logger)
    {
        logger.LogInformation("Do stuff with this ride");
    }
}