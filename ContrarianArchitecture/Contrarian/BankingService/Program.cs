using Marten;
using Oakton;
using Wolverine;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddMarten("Host=localhost;Port=5432;Database=postgres;Username=postgres;password=postgres")
    .IntegrateWithWolverine();

builder.Host.UseWolverine(opts =>
{
    //This isn't strictly necessary
    opts
        .Policies
        .ForMessagesOfType<IAccountCommand>()
        .AddMiddleware(typeof(AccountLookupMiddleware));
    
    opts.Policies.AutoApplyTransactions();
});

var app = builder.Build();

return await app.RunOaktonCommands(args);