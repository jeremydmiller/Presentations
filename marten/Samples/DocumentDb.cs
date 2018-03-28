using System;
using System.Collections.Generic;
using Marten;
using Shouldly;
using Xunit;

namespace Samples
{
    public class DocumentDb
    {
        private DocumentStore theStore;

        public DocumentDb()
        {
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

            using (var session = theStore.QuerySession())
            {
                var order2 = session.Load<Order>(order.Id);
                
                order2.ShouldNotBeNull();
                order2.ShouldNotBeSameAs(order);
                
                order2.CustomerId.ShouldBe(order.CustomerId);
            }
        }
    }
}