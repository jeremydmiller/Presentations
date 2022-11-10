namespace RideSharing;

public class Customer
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class Driver
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public record ShiftStarted(Guid DriverId);
public record DriverReady(Location Location);
public record RideAccepted(Guid RideId);

public record DriverLocated(Location location);


public class DriverShift
{
    public Guid Id { get; set; }

    public DriverShift(ShiftStarted started)
    {
        DriverId = started.DriverId;
    }

    public Guid DriverId { get; set; }
}

public record Location(double Latitude, double Longitude);

public record RideRequested(Guid CustomerId, Location Location);
public record DriverAssigned(Guid DriverId, string Category);

public record RidePriced(decimal Amount, string Category);

public record DriverArrived();

public record RideStarted();
public record RideEnded();


public record RideCancelled();

