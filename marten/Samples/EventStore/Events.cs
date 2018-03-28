using System;
using Baseline;

namespace Samples.EventStore
{
    public class Quest
    {
        public Guid Id { get; set; }
    }

    // SAMPLE: sample-events
    public class ArrivedAtLocation
    {

        public int Day { get; set; }

        public string Location { get; set; }

        public override string ToString()
        {
            return $"Arrived at {Location} on Day {Day}";
        }
    }

    public class MembersJoined
    {

        public MembersJoined()
        {
        }

        public MembersJoined(int day, string location, params string[] members)
        {
            Day = day;
            Location = location;
            Members = members;
        }

        public Guid QuestId { get; set; }

        public int Day { get; set; }

        public string Location { get; set; }

        public string[] Members { get; set; }

        public override string ToString()
        {
            return $"Members {Members.Join(", ")} joined at {Location} on Day {Day}";
        }
    }


    public class QuestStarted
    {
        public string Name { get; set; }
        public Guid Id { get; set; }

        public override string ToString()
        {
            return $"Quest {Name} started";
        }
    }

    public class QuestEnded
    {
        public string Name { get; set; }
        public Guid Id { get; set; }

        public override string ToString()
        {
            return $"Quest {Name} ended";
        }
    }

    public class MembersDeparted
    {
        public Guid Id { get; set; }

        public Guid QuestId { get; set; }

        public int Day { get; set; }

        public string Location { get; set; }

        public string[] Members { get; set; }

        public override string ToString()
        {
            return $"Members {Members.Join(", ")} departed at {Location} on Day {Day}";
        }
    }

    public class MembersEscaped
    {
        public Guid Id { get; set; }

        public Guid QuestId { get; set; }

        public string Location { get; set; }

        public string[] Members { get; set; }

        public override string ToString()
        {
            return $"Members {Members.Join(", ")} escaped from {Location}";
        }
    }
}