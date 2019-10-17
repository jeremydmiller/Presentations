using System;
using System.Threading.Tasks;
using Marten;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Samples.EventStore
{
    public class exercise_the_event_store
    {
        private readonly ITestOutputHelper _output;
        private DocumentStore theStore;

        public exercise_the_event_store(ITestOutputHelper output)
        {
            _output = output;
            theStore = DocumentStore.For(ConnectionSource.ConnectionString);
            
            // Just wiping out any existing database objects in the schema first
            theStore.Advanced.Clean.CompletelyRemoveAll();
        }
        
        
        [Fact]
        public void event_capture_and_show_live_aggregation()
        {
            var started = new QuestStarted
            {
                Id = Guid.NewGuid(),
                Name = "Escape Emond's Field"
            };

            var questId = started.Id;
            
            var joined = new MembersJoined { Members = new[] { "Rand", "Matt", "Perrin", "Thom" }, Day = 1};
            var departed = new MembersDeparted { Members = new[] { "Thom" }, Day = 5};

            using (var session = theStore.LightweightSession())
            {
                session.Events.StartStream(started.Id, started, joined, departed);
                session.SaveChanges();
            }
            
            
            using (var session = theStore.LightweightSession())
            {
                session.Events.Append(started.Id, new MembersJoined {Members = new string[] {"Moiraine", "Lan"}, Day = 7});
                session.SaveChanges();
            }
            
            
            // Aggregate Live
            using (var session = theStore.LightweightSession())
            {
                var party = session.Events
                    .AggregateStream<QuestParty>(questId);
                
                _output.WriteLine(JsonConvert.SerializeObject(party));
            }
            
        }
        
        
        [Fact]
        public void event_capture_and_show_inline_aggregation()
        {
            theStore = DocumentStore.For(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);
                _.Events.InlineProjections.AggregateStreamsWith<QuestParty>();
            });
            
            
            var started = new QuestStarted
            {
                Id = Guid.NewGuid(),
                Name = "Escape Emond's Field"
            };
            
            var joined = new MembersJoined { Members = new[] { "Rand", "Matt", "Perrin", "Thom" } };
            var departed = new MembersDeparted { Members = new[] { "Thom" } };

            using (var session = theStore.LightweightSession())
            {
                session.Events.StartStream(started.Id, started, joined, departed);
                session.SaveChanges();
            }
            
            
            using (var session = theStore.LightweightSession())
            {
                session.Events.Append(started.Id, new MembersJoined {Members = new string[] {"Moiraine", "Lan"}});
                session.SaveChanges();
            }
            
            
            // Aggregate Live
            using (var session = theStore.LightweightSession())
            {
                var party = session.Load<QuestParty>(started.Id);
                
                _output.WriteLine(JsonConvert.SerializeObject(party));
            }
            
        }
        
        
        [Fact]
        public async Task event_capture_and_use_the_async_daemon()
        {
            theStore = DocumentStore.For(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);

                _.Events.AsyncProjections.AggregateStreamsWith<QuestParty>();
                
            });

            
            
            
            var started = new QuestStarted
            {
                Id = Guid.NewGuid(),
                Name = "Escape Emond's Field"
            };
            
            var joined = new MembersJoined { Members = new[] { "Rand", "Matt", "Perrin", "Thom" } };
            var departed = new MembersDeparted { Members = new[] { "Thom" } };

            using (var session = theStore.LightweightSession())
            {
                session.Events.StartStream(started.Id, started, joined, departed);
                await session.SaveChangesAsync();
            }
            
            
            using (var session = theStore.LightweightSession())
            {
                session.Events.Append(started.Id, new MembersJoined {Members = new string[] {"Moiraine", "Lan"}});
                await session.SaveChangesAsync();
            }
            
            
            var daemon = theStore.BuildProjectionDaemon();

            daemon.StartAll();

            await daemon.WaitForNonStaleResults();

            await daemon.StopAll();
            
            daemon.Dispose();
            
            
            // Aggregate Live
            using (var session = theStore.LightweightSession())
            {
                var party = session.Load<QuestParty>(started.Id);
                
                _output.WriteLine(JsonConvert.SerializeObject(party));
            }
            
        }
    }
}