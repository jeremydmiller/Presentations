using Marten;
using Shouldly;

namespace RideSharing.Tests;

public class storing_documents
{
    [Fact]
    public async Task store_some_documents()
    {
        var store = DocumentStore.For(
            "Host=localhost;Port=5432;Database=marten_testing;Username=postgres;password=postgres;Command Timeout=5");

        await using var session = store.LightweightSession();

        var original = new Driver
        {
            FirstName = "Patrick",
            LastName = "Mahomes"
        };
        
        session.Store(original);

        await session.SaveChangesAsync();

        var loaded = await session.LoadAsync<Driver>(original.Id);
        
        loaded.ShouldNotBeSameAs(original);
        loaded.FirstName.ShouldBe(original.FirstName);
        loaded.LastName.ShouldBe(original.LastName);
    }
}