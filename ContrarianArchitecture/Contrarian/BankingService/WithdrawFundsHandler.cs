using System.Diagnostics;
using JasperFx.Core;
using Marten;
using Marten.Linq;
using Wolverine;
using Wolverine.Attributes;

namespace BankingService;

// The attribute directs Wolverine to send this message with 
// a "deliver within 5 seconds, or discard" directive
[DeliverWithin(5)]
public record AccountUpdated(Guid AccountId, decimal Balance);

public record LowBalanceDetected(Guid AccountId) : IAccountCommand;

public record EnforceAccountOverdrawnDeadline(Guid AccountId) : TimeoutMessage(10.Days()), IAccountCommand;

public class Account
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public decimal MinimumThreshold { get; set; }
    
    public Guid CustomerId { get; set; }
}

public class Customer
{
    public Guid Id { get; set; }
}

public interface IAccountCommand
{
    Guid AccountId { get; }
}

// A "command" message
public record WithdrawFunds(Guid AccountId, decimal Amount) : IAccountCommand;

// An "event" message
public record AccountOverdrawn(Guid AccountId);




public static class WithdrawFundsHandler
{
    // public static async Task<(Account, HandlerContinuation)> LoadAsync(WithdrawFunds command, IDocumentSession session)
    // {
    //     var account = await session.LoadAsync<Account>(command.AccountId);
    //     return (account!, account == null ? HandlerContinuation.Stop : HandlerContinuation.Continue);
    // }
    //
    public static IEnumerable<object> Handle(
        WithdrawFunds command, 
        Account account, 
        IDocumentSession session)
    {
        account.Balance -= command.Amount;
     
        // This just marks the account as changed, but
        // doesn't actually commit changes to the database
        // yet. 
        session.Store(account);
 
        // Conditionally trigger other, cascading messages
        if (account.Balance > 0 && account.Balance < account.MinimumThreshold)
        {
            yield return new LowBalanceDetected(account.Id);
        }
        else if (account.Balance < 0)
        {
            yield return new AccountOverdrawn(account.Id);
         
            // Give the customer 10 days to deal with the overdrawn account
            yield return new EnforceAccountOverdrawnDeadline(account.Id);
        }

        yield return new AccountUpdated(account.Id, account.Balance);
    }

}

public interface IEmailGateway
{
    
}

public class EmailMessage
{
    
}

public static class LowBalanceDetectedHandler
{
    // Use advanced persistence functions 
    // This is an example of "A-Frame" architecture
    public static async Task<(Account, Customer)> LoadAsync(LowBalanceDetected detected, IDocumentSession session)
    {
        Customer customer = null;
        var account = await session
            .Query<Account>()
            .Include<Customer>(x => x.CustomerId, c => customer = c)
            .Where(x => x.Id == detected.AccountId)
            .FirstOrDefaultAsync();

        return (account, customer!);
    }

    public static EmailMessage Handle(LowBalanceDetected detected, Account account, Customer customer)
    {
        // Use the information from the Account & Customer
        // documents to craft an email message and pass that aGuid to whatever
        // handler will actually invoke the email body
        
        return new EmailMessage();
    }
}





// This is *a* way to build middleware in Wolverine by basically just
// writing functions/methods. There's a naming convention that
// looks for Before/BeforeAsync or After/AfterAsync
public static class AccountLookupMiddleware
{
    // The message *has* to be first in the parameter list
    // Before or BeforeAsync tells Wolverine this method should be called before the actual action
    public static async Task<(HandlerContinuation, Account?)> LoadAsync(
        IAccountCommand command, 
        ILogger logger, 
        
        // This app is using Marten for persistence
        IDocumentSession session, 
        
        CancellationToken cancellation)
    {
        var account = await session.LoadAsync<Account>(command.AccountId, cancellation);
        if (account == null)
        {
            logger.LogInformation("Unable to find an account for {AccountId}, aborting the requested operation", command.AccountId);
        }
        
        return (account == null ? HandlerContinuation.Stop : HandlerContinuation.Continue, account);
    }
}


public static class OtherHandler
{
    public static void Handle(AccountUpdated e) => Debug.WriteLine("Got AccountUpdated");
}