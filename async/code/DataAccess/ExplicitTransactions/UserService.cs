using System;
using System.Data;
using System.Linq;
using async.Internal;
using Microsoft.Data.SqlClient;

namespace ExplicitTransactions
{
    public class UserService
    {
        private readonly DatabaseSettings _settings;
        private readonly IMessagePublisher _publisher;

        public UserService(DatabaseSettings settings, IMessagePublisher publisher)
        {
            _settings = settings;
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
            PersistUsers(users);
            
            
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

        private void PersistUsers(User[] users)
        {
            // Generally always open connections in your own code
            // inside a "using" block
            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                // Open the connection just in time
                connection.Open();

                var tx = connection.BeginTransaction();

                try
                {
                    foreach (var user in users)
                    {
                        // Do what you need to do right here
                        writeUser(user, tx);
                    }
                
                    tx.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    tx.Rollback();
                    throw;
                }
            }
        }

        private static void writeUser(User user, SqlTransaction tx)
        {
            var cmd = tx.Connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "insert into users (UserName) values (@name)";
            cmd.Parameters.Add("name", SqlDbType.VarChar).Value = user.UserName;
            cmd.ExecuteNonQuery();
        }

        private void AssertIsValid(NewUserRequest newUserRequest)
        {
            if (string.IsNullOrEmpty(newUserRequest.UserName))
                throw new ArgumentException(nameof(newUserRequest), $"{nameof(newUserRequest.UserName)} is missing");
        }

    }

}