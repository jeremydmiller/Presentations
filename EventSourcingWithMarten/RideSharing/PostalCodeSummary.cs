using Marten;
using Marten.AspNetCore;
using Marten.Events;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Mvc;

namespace RideSharing;

public record DriverShiftAssignment(Guid Id, string SummaryIdentifier);

public class AssignmentProjection : EventProjection
{
    public void Apply(IDocumentOperations ops, IEvent<ShiftStarted> started)
    {
        var identifier = PostalCodeSummary.CreateIdentifier(started.Data.PostalCode, started.Timestamp.Date);
        var assignment = new DriverShiftAssignment(started.StreamId, identifier);
        ops.Store(assignment);
    }
}

public class PostalCodeSummary
{
    public static string CreateIdentifier(string postalCode, DateTime date)
    {
        return $"{postalCode}_{date.Day}/{date.Month}/{date.Year}";
    }
    
    public string PostalCode { get; }

    public PostalCodeSummary(string postalCode, DateTime date)
    {
        PostalCode = postalCode;
        Date = date;
        Id = $"{PostalCode}_{date.Day}/{date.Month}/{date.Year}";
    }

    public PostalCodeSummary()
    {
    }

    public string Id { get; set; }
    public DateTime Date { get; set; }
    public int Rides { get; set; }
    public double Mileage { get; set; }
}

public class PostalCodeSummaryAggregation : ExperimentalMultiStreamAggregation<PostalCodeSummary, string>
{
    protected override async ValueTask GroupEvents(IEventGrouping<string> grouping, IQuerySession session, List<IEvent> events)
    {
        // Avert your eyes, I know this is ugly! We're working on it.
        var shiftIdList = events.Select(x => x.StreamId).Distinct();
        var assignments = await session.LoadManyAsync<DriverShiftAssignment>(shiftIdList);

        var groups = assignments.GroupBy(x => x.SummaryIdentifier);

        foreach (var group in groups)
        {
            var groupEvents = events.Where(x =>
            {
                var streamIds = group.Select(x => x.Id);
                return x.StreamId.IsOneOf(streamIds.ToArray());
            });
            
            grouping.AddEvents(group.Key, groupEvents);
        }
    }

    public void Apply(RideEnded ended, PostalCodeSummary summary)
    {
        summary.Rides++;
        summary.Mileage += ended.Mileage;
    }
}

public class PostalCodeSummaryController : ControllerBase
{
    // This uses Marten.AspNetCore
    [HttpGet("/summary/{code}/{date}")]
    public Task GetSummary(string code, DateTime date, 
        [FromServices] IQuerySession session)
    {
        var identifier = PostalCodeSummary.CreateIdentifier(code, date);

        return session.Json.WriteById<PostalCodeSummary>(identifier, HttpContext);
    }
}