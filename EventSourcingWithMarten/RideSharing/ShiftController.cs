using Marten;
using Microsoft.AspNetCore.Mvc;

namespace RideSharing;

public record StartDriverShift(Guid DriverId, string Category, Location Location);

public interface ILocatorService
{
    Task<string> FindPostalCodeAsync(Location location);
}

public record NewShift(Guid Id, Location Location);

public class ShiftController : ControllerBase
{
    [HttpPost("/shift/start")]
    public async Task<NewShift> Start(
        [FromBody] StartDriverShift command, 
        [FromServices] ILocatorService locator,
        [FromServices] IDocumentSession session)
    {
        var postalCode = await locator.FindPostalCodeAsync(command.Location);
        var started = new ShiftStarted(command.DriverId, command.Category, postalCode);
        var located = new DriverLocated(command.Location);

        var shiftId = session
            .Events
            .StartStream(started, located).Id;

        await session.SaveChangesAsync();

        return new NewShift(shiftId, command.Location);
    }
}