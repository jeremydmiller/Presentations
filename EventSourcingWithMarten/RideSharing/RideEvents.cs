namespace RideSharing;

public record Location(double Latitude, double Longitude);

public record ShiftStarted(Guid DriverId, string Category, string PostalCode);
public record DriverReady(Location Location);
public record RideAccepted(Guid RideId, Location Location);
public record RideEnded(double Mileage, Location Location);

public record DriverLocated(Location Location);



public record RideRequested(Guid CustomerId, Location Location);
public record DriverAssigned(Guid DriverId, string Category);

public record RidePriced(decimal Amount, string Category);

public record DriverArrived();

public record RideStarted();



public record RideCancelled();

