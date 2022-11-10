using Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMarten(opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("Marten");
    opts.Connection(connectionString);
    
    // More later!
});

builder.Services.AddMvc();

var app = builder.Build();

app.MapControllers();

app.Run();