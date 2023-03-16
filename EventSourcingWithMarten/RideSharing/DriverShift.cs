using Marten.Events;
using Marten.Events.Projections;

namespace RideSharing;

public enum DriverStatus
{
    Ready,
    Assigned,
    Unavailable
}

public class DriverShift
{
    // Same as the stream identity
    public Guid Id { get; set; }
    
    // This will matter later!!!
    public int Version { get; set; }

    public DriverShift(IEvent<ShiftStarted> @event)
    {
        Day = @event.Timestamp.Date;
        DriverId = @event.Data.DriverId;
        Category = @event.Data.Category;
        PostalCode = @event.Data.PostalCode;
    }

    public DateTime Day { get; set; }

    public string PostalCode { get; set; }

    public string Category { get; set; }

    public Guid DriverId { get; set; }
    public DriverStatus Status { get; set; }
    
    public Location Location { get; set; }

    public void Apply(DriverReady ready)
    {
        Status = DriverStatus.Ready;
        Location = ready.Location;
    }

    public void Apply(RideAccepted accepted)
    {
        Status = DriverStatus.Assigned;
        RideId = accepted.RideId;
        Location = accepted.Location;
    }

    public void Apply(RideEnded ended)
    {
        Status = DriverStatus.Unavailable;
        Location = ended.Location;
        RideId = null;
    }

    public Guid? RideId { get; set; }
}