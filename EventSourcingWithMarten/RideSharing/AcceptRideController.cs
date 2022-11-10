using Marten;
using Microsoft.AspNetCore.Mvc;

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

        if (aggregate.Status != DriverStatus.Ready)
        {
            throw new Exception("I'm not ready!");
        }

        var accept = new RideAccepted(command.RideId, aggregate.Location);
        
        // Append an event with optimistic concurrency
        session.Events.Append(command.DriverShiftId, command.Version + 1, accept);

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
                var aggregate = stream.Aggregate;

                if (aggregate.Status != DriverStatus.Ready)
                {
                    throw new Exception("I'm not ready!");
                }

                var accept = new RideAccepted(command.RideId, aggregate.Location);
        
                // Append an event with optimistic concurrency
                stream.AppendOne(accept);
            });
    }
    
}