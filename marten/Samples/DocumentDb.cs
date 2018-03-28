using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Marten;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Samples
{
    public class DocumentDb
    {
        private readonly ITestOutputHelper _output;
        private readonly DocumentStore theStore;

        public DocumentDb(ITestOutputHelper output)
        {
            _output = output;
            
            theStore = DocumentStore.For(ConnectionSource.ConnectionString);
            
            // Just wiping out any existing database objects in the schema first
            theStore.Advanced.Clean.CompletelyRemoveAll();
        }

        public enum Priority
        {
            Low, 
            High
        }

        public class Order
        {
            public Guid Id { get; set; }

            public Priority Priority { get; set; }
            
            public string CustomerId { get; set; }

            public IList<OrderDetail> Details { get; set; } = new List<OrderDetail>();
        }

        public class OrderDetail
        {
            public string PartNumber { get; set; }
            public int Number { get; set; }
        }
        

        [Fact]
        public void save_and_load_order()
        {
            var order = new Order
            {
                Priority = Priority.High,
                CustomerId = "STEV001",
                Details = new List<OrderDetail>
                {
                    new OrderDetail {PartNumber = "10XFX", Number = 5},
                    new OrderDetail {PartNumber = "20XFX", Number = 15},
                }
            };

            using (var session = theStore.LightweightSession())
            {
                session.Store(order);
                session.SaveChanges();
            }
            
            _output.WriteLine($"The order id is {order.Id}");

            using (var session = theStore.QuerySession())
            {
                var order2 = session.Load<Order>(order.Id);
                
                order2.ShouldNotBeNull();
                order2.ShouldNotBeSameAs(order);
                
                order2.CustomerId.ShouldBe(order.CustomerId);
            }
        }
        
        
                

        [Fact]
        public async Task save_and_load_order_async()
        {
            var order = new Order
            {
                Priority = Priority.High,
                CustomerId = "STEV001",
                Details = new List<OrderDetail>
                {
                    new OrderDetail {PartNumber = "10XFX", Number = 5},
                    new OrderDetail {PartNumber = "20XFX", Number = 15},
                }
            };

            using (var session = theStore.LightweightSession())
            {
                session.Store(order);
                await session.SaveChangesAsync();
            }

            using (var session = theStore.QuerySession())
            {
                var order2 = await session.LoadAsync<Order>(order.Id);
                
                order2.ShouldNotBeNull();
                order2.ShouldNotBeSameAs(order);
                
                order2.CustomerId.ShouldBe(order.CustomerId);
            }
        }
        
        
        
        public class Address
        {
            public string Address1 { get; set; }
            public string Address2 { get; set; }
            public string City { get; set; }
            public string StateOrProvince { get; set; }
            public string Country { get; set; }
            public string PostalCode { get; set; }

            public bool Primary { get; set; }
        }
        
    }
}