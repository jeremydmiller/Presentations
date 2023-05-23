using Marten;
using Marten.Pagination;
using Shouldly;
using Xunit.Abstractions;

namespace RideSharing.Tests;

public class storing_documents
{
    private readonly ITestOutputHelper _output;

    public storing_documents(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task store_some_documents()
    {
        var connectionString = "Host=localhost;Port=5432;Database=marten_testing;Username=postgres;password=postgres;Command Timeout=5";
        var store = DocumentStore.For(opts =>
        {
            opts.Connection(connectionString);
            opts.Logger(new TestOutputMartenLogger(_output));

            // The app will query on license type
            opts.Schema.For<Driver>()
                .Index(x => x.LicenseType);

            opts.Projections.Snapshot<DriverShift>();
        });

        await using var session = store.LightweightSession();

        var original = new Driver
        {
            FirstName = "Patrick",
            LastName = "Mahomes",
            LicenseType = "Commercial"
        };
        
        session.Store(original);

        await session.SaveChangesAsync();

        var loaded = await session.LoadAsync<Driver>(original.Id);
        
        loaded.ShouldNotBeSameAs(original);
        loaded.FirstName.ShouldBe(original.FirstName);
        loaded.LastName.ShouldBe(original.LastName);

        // Linq query
        var commercial = await session
            .Query<Driver>()
            .Where(x => x.LicenseType == "Commercial")
            .ToListAsync();
        
        // Which leads to:
        /*
select d.id, d.data from public.mt_doc_driver as d where d.data ->> 'LicenseType' = :p0
  p0: Commercial
         */
    }
}