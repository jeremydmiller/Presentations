﻿using System;
using System.Collections.Generic;
using System.Linq;
using Marten;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Samples
{
    public class HierarchicalDocuments
    {
        private readonly ITestOutputHelper _output;
        private DocumentStore theStore;

        public HierarchicalDocuments(ITestOutputHelper output)
        {
            _output = output;
            theStore = DocumentStore.For(x =>
            {
                x.Connection(ConnectionSource.ConnectionString);
                x.Schema.For<Order>()
                    .AddSubClass<DomesticOrder>()
                    .AddSubClass<InternationalOrder>();
            });
            
            theStore.Advanced.Clean.CompletelyRemoveAll();
        }
        
        public enum Priority
        {
            Low, 
            High
        }

        public abstract class Order
        {
            public Guid Id { get; set; }

            public Priority Priority { get; set; }
            
            public string CustomerId { get; set; }

            public IList<OrderDetail> Details { get; set; } = new List<OrderDetail>();
        }

        public class InternationalOrder : Order
        {
            public string Country { get; set; }
            
        }
        public class DomesticOrder : Order{}

        public class OrderDetail
        {
            public string PartNumber { get; set; }
            public int Number { get; set; }
        }
        
        
        [Fact]
        public void save_and_load_order()
        {
            var domestic1 = new DomesticOrder
            {
                Priority = Priority.High,
                CustomerId = "STEV001",
                Details = new List<OrderDetail>
                {
                    new OrderDetail {PartNumber = "10XFX", Number = 5},
                    new OrderDetail {PartNumber = "20XFX", Number = 15},
                }
            };
            
            var international1 = new InternationalOrder()
            {
                Country = "Spain",
                Priority = Priority.High,
                CustomerId = "INT002",
                Details = new List<OrderDetail>
                {
                    new OrderDetail {PartNumber = "10XFX", Number = 3},
                }
            };

            using (var session = theStore.LightweightSession())
            {
                // Save both orders
                session.Store(domestic1);
                session.Store(international1);
                session.SaveChanges();
            }
            
            using (var session = theStore.QuerySession())
            {
                // Load as the base type
                var order = session.Load<Order>(domestic1.Id);

                // Load as the subclass type
                var order2 = session.Load<InternationalOrder>(international1.Id);

                // Query against the parent type
                var importantOrders = session
                    .Query<Order>()
                    .Count(x => x.Priority == Priority.High);

                // Query against the subclass
                var domestics = session
                    .Query<DomesticOrder>()
                    .Count(x => x.CustomerId == "SOMEBODY");


                var cmd = session
                    .Query<DomesticOrder>()
                    .Where(x => x.CustomerId == "SOMEBODY")
                    .ToCommand();
                
               _output.WriteLine(cmd.CommandText);
                
               /*
                *
                *
                *select d.data, d.id, d.mt_doc_type, d.mt_version from public.mt_doc_hierarchicaldocuments_order as d where (d.data ->> 'CustomerId' = :arg0 and (d.mt_doc_type = 'hierarchical_documents___domestic_order'))

                * 
                */
                   
                   
                   
                
                
            }
        }
        
    }
}