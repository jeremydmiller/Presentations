using Marten;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Oakton;
using RideSharing;
using Wolverine;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("Marten");
    opts.Connection(connectionString);

    // Tell Marten to update this aggregate inline
    opts.Projections.SelfAggregate<DriverShift>(ProjectionLifecycle.Inline);
    
    // Add the big async projection
    opts.Projections.Add(new PostalCodeSummaryAggregation(), ProjectionLifecycle.Async);
})
    // This adds in async projection support
    .AddAsyncDaemon(DaemonMode.HotCold)
    .IntegrateWithWolverine()
    .EventForwardingToWolverine();

builder.Host.UseWolverine(opts =>
{
    // Just applying durable outbox mechanics to each local
    // queue
    opts.Policies.UseDurableLocalQueues();
});

builder.Services.AddMvc();

var app = builder.Build();

app.MapControllers();

return await app.RunOaktonCommands(args);