using System;
using System.Data;
using System.Linq;
using async.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace UnitOfWork
{
    public class UsersDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(_ =>
            {
                _.Property(x => x.UserName).IsRequired();
                _.Property(x => x.Id).UseIdentityColumn();
            });


        }
    }
    
    
    public class UserService
    {
        private readonly UsersDbContext _dbContext;
        private readonly IMessagePublisher _publisher;

        public UserService(UsersDbContext dbContext, IMessagePublisher publisher)
        {
            _dbContext = dbContext;
            _publisher = publisher;
        }

        // Entry Point
        public void CreateNewUsers(NewUserRequest[] newUsers)
        {
            // Do some validation
            foreach (var request in newUsers)
            {
                AssertIsValid(request);
            }
            
            // Create new users
            var users = newUsers
                .Select(x => new User {UserName = x.UserName}).ToArray();

            // Write it to the database
            // Notice I'm not doing anything to start
            // a transaction
            foreach (var user1 in users)
            {
                _dbContext.Add(user1);
            }

            // But calling this will persist the entire
            // "unit of work" in one database transaction
            _dbContext.SaveChanges();


            // Publish an event about the new user
            // to a service bus
            foreach (var user in users)
            {
                var @event = new UserCreated
                {
                    UserName = user.UserName
                };
            
                _publisher.Publish(@event);
            }
            

        }

        private void AssertIsValid(NewUserRequest newUserRequest)
        {
            if (string.IsNullOrEmpty(newUserRequest.UserName))
                throw new ArgumentException(nameof(newUserRequest), $"{nameof(newUserRequest.UserName)} is missing");
        }

    }

}