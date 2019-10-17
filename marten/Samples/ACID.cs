using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Baseline;
using Marten;
using Marten.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Shouldly;
using Xunit;

namespace Samples
{
    public class ACID
    {
        private IDocumentStore theStore;


        public ACID()
        {
            theStore = DocumentStore.For(_ =>
            {
                _.Connection(ConnectionSource.ConnectionString);

                _.Schema.For<Target>()
                    .Index(d => d.Color);
            });
            
            theStore.Advanced.Clean.CompletelyRemoveAll();
        }
        
        
        
        [Fact]
        public async Task Proving_to_Frans_Bouma_that_Marten_is_ACID_compliant()
        {
            var targets = Target.GenerateRandomData(1000).ToArray();
            var greenCount = targets.Count(x => x.Color == Colors.Green);
            greenCount.ShouldBeGreaterThan(0);
            greenCount.ShouldBeLessThan(1000);
            
            
            
            theStore.BulkInsert(targets);
            
            // Insert all the documents
//            using (var session = theStore.LightweightSession())
//            {
//                session.Store(targets);
//                await session.SaveChangesAsync();
//            }
          
            using (var session = theStore.QuerySession())
            {
                var dbCount = await session
                    .Query<Target>()
                    .CountAsync(x => x.Color == Colors.Green);
                
                dbCount.ShouldBe(greenCount);
            }
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        




        public class FindByColor : ICompiledQuery<Target, int>
        {
            public Colors Color { get; set; } = Colors.Green;
            
            public Expression<Func<IQueryable<Target>, int>> QueryIs()
            {
                return q => q.Count(x => x.Color == Color);
            }
        }
        
        
        [Fact]
        public async Task use_a_compiled_query()
        {

            var targets = Target.GenerateRandomData(1000).ToArray();
            var greenCount = targets.Count(x => x.Color == Colors.Green);
            greenCount.ShouldBeGreaterThan(0);
            greenCount.ShouldBeLessThan(1000);
            
            
            // Insert all the documents
            using (var session = theStore.LightweightSession())
            {
                session.Store(targets);
                await session.SaveChangesAsync();
            }
            
            using (var session = theStore.QuerySession())
            {
 
                
                var dbCount = await session
                    .QueryAsync(new FindByColor {Color = Colors.Green});
                
                dbCount.ShouldBe(greenCount);
            }
        }
    }
    
    
    
    
    
    

    public class Address
    {
        public Address()
        {
        }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }

        public bool Primary { get; set; }
    }

    public enum Colors
    {
        Red,
        Blue,
        Green
    }

    public class Target
    {
        private static readonly Random _random = new Random(67);

        private static readonly string[] _strings =
        {
            "Red", "Orange", "Yellow", "Green", "Blue", "Purple", "Violet",
            "Pink", "Gray", "Black"
        };

        private static readonly string[] _otherStrings =
        {
            "one", "two", "three", "four", "five", "six", "seven", "eight",
            "nine", "ten"
        };

        public static IEnumerable<Target> GenerateRandomData(int number)
        {
            var i = 0;
            while (i < number)
            {
                yield return Random(true);

                i++;
            }
        }

        public static Target Random(bool deep = false)
        {
            var target = new Target();
            target.String = _strings[_random.Next(0, 10)];
            target.AnotherString = _otherStrings[_random.Next(0, 10)];
            target.Number = _random.Next();

            target.Flag = _random.Next(0, 10) > 5;

            target.Float = Single.Parse(_random.NextDouble().ToString());

            target.NumberArray = new[] { _random.Next(0, 10), _random.Next(0, 10), _random.Next(0, 10) };

            target.NumberArray = target.NumberArray.Distinct().ToArray();

            switch (_random.Next(0, 2))
            {
                case 0:
                    target.Color = Colors.Blue;
                    break;

                case 1:
                    target.Color = Colors.Green;
                    break;

                default:
                    target.Color = Colors.Red;
                    break;
            }

            target.Long = 100 * _random.Next();
            target.Double = _random.NextDouble();
            target.Long = _random.Next() * 10000;

            target.Date = DateTime.Today.AddDays(_random.Next(-10000, 10000)).ToUniversalTime();

            if (deep)
            {
                target.Inner = Random();

                var number = _random.Next(1, 10);
                target.Children = new Target[number];
                for (int i = 0; i < number; i++)
                {
                    target.Children[i] = Random();
                }

                target.StringDict = Enumerable.Range(0, _random.Next(1, 10)).ToDictionary(i => $"key{i}", i => $"value{i}");
            }

            return target;
        }

        public Target()
        {
            Id = Guid.NewGuid();
            StringDict = new Dictionary<string, string>();
        }

        public Guid Id { get; set; }

        public int Number { get; set; }
        public long Long { get; set; }
        public string String { get; set; }
        public string AnotherString { get; set; }

        public Guid OtherGuid { get; set; }

        public Target Inner { get; set; }

        public Colors Color { get; set; }

        public bool Flag { get; set; }

        public string StringField;

        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public DateTime Date { get; set; }
        public DateTimeOffset DateOffset { get; set; }

        public float Float;

        public int[] NumberArray { get; set; }


        public Target[] Children { get; set; }

        public int? NullableNumber { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public bool? NullableBoolean { get; set; }

        public IDictionary<string,string> StringDict { get; set; }

        public Guid UserId { get; set; }

    }
}