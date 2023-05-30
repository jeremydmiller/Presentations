using Alba;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Shouldly;
using Wolverine;
using Wolverine.Tracking;
using Xunit;

namespace BankingService.Tests;

public class withdrawing_funds : IntegrationContext
{
    public withdrawing_funds(AppFixture fixture) : base(fixture)
    {
    }
    
    [Fact]
    public async Task should_increase_the_account_balance_happy_path()
    {
        // Drive in a known data, so the "Arrange"
        var account = new Account
        {
            Balance = 2500,
            MinimumThreshold = 200
        };
 
        await using (var session = Store.LightweightSession())
        {
            session.Store(account);
            await session.SaveChangesAsync();
        }

        var tracked = await Host.InvokeMessageAndWaitAsync(new WithdrawFunds(account.Id, 1200));
        
        // Finally, let's do the "assert"
        await using (var session = Store.LightweightSession())
        {
            // Load the newly persisted copy of the data from Marten
            var persisted = await session.LoadAsync<Account>(account.Id);
            persisted.Balance.ShouldBe(1300); // Started with 2500, debited 1200
        }
 
        // And also assert that an AccountUpdated message was published as well
        var updated = tracked.Sent.SingleMessage<AccountUpdated>();
        updated.AccountId.ShouldBe(account.Id);
        updated.Balance.ShouldBe(1300);
    }
}


public class AppFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // Workaround for Oakton with WebApplicationBuilder
        // lifecycle issues. Doesn't matter to you w/o Oakton
        OaktonEnvironment.AutoStartHost = true;
         
        // This is bootstrapping the actual application using
        // its implied Program.Main() set up
        Host = await AlbaHost.For<Program>(x =>
        {
            // I'm overriding 
            x.ConfigureServices(services =>
            {
                // Let's just take any pesky message brokers out of
                // our integration tests for now so we can work in
                // isolation
                services.DisableAllExternalWolverineTransports();
                 
                // Just putting in some baseline data for our database
                // There's usually *some* sort of reference data in 
                // enterprise-y systems
                services.InitializeMartenWith<InitialAccountData>();
            });
        });
    }
 
    public IAlbaHost Host { get; private set; }
 
    public Task DisposeAsync()
    {
        return Host.DisposeAsync().AsTask();
    }
}

[CollectionDefinition("integration")]
public class ScenarioCollection : ICollectionFixture<AppFixture>
{
     
}

[Collection("integration")]
public abstract class IntegrationContext : IAsyncLifetime
{
    public IntegrationContext(AppFixture fixture)
    {
        Host = fixture.Host;
        Store = Host.Services.GetRequiredService<IDocumentStore>();
    }
     
    public IAlbaHost Host { get; }
    public IDocumentStore Store { get; }
     
    public async Task InitializeAsync()
    {
        // Using Marten, wipe out all data and reset the state
        // back to exactly what we described in InitialAccountData
        await Store.Advanced.ResetAllData();
    }
 
    // This is required because of the IAsyncLifetime 
    // interface. Note that I do *not* tear down database
    // state after the test. That's purposeful
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

public class InitialAccountData : IInitialData
{
    public static Guid Account1 = Guid.NewGuid();
    public static Guid Account2 = Guid.NewGuid();
    public static Guid Account3 = Guid.NewGuid();
     
    public Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        return store.BulkInsertAsync(accounts().ToArray());
    }
 
    private IEnumerable<Account> accounts()
    {
        yield return new Account
        {
            Id = Account1,
            Balance = 1000,
            MinimumThreshold = 500
        };
         
        yield return new Account
        {
            Id = Account2,
            Balance = 1200
        };
 
        yield return new Account
        {
            Id = Account3,
            Balance = 2500,
            MinimumThreshold = 100
        };
    }
}