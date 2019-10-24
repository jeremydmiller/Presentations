using System;
using System.Data;
using async.Internal;
using Microsoft.Data.SqlClient;

namespace ContinuationPassingStyle
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
        public void CreateNewUser(NewUserRequest newUserRequest)
        {
            // Do some validation
            AssertIsValid(newUserRequest);
            
            // Create a new User domain object
            var user = new User {UserName = newUserRequest.UserName};

            // Write it to the database
            PersistUser(user);
            
            
            // Publish an event about the new user
            // to a service bus
            var @event = new UserCreated
            {
                UserName = user.UserName
            };
            
            _publisher.Publish(@event);
        }

        // Continuation Passing Style helper
        private void ExecuteAgainstDatabase(Action<SqlConnection> action)
        {
            // Generally always open connections in your own code
            // inside a "using" block
            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                // Open the connection just in time
                connection.Open();

                // Do what you need to do right here
                action(connection);

                // Close it as soon as we're done.
                // Honestly, calling Dispose() is essentially the same thing
                connection.Close();
            }
        }

        private void PersistUser(User user)
        {
            ExecuteAgainstDatabase(conn =>
            {
                writeUser(user, conn);
            });
        }

        private static void writeUser(User user, SqlConnection connection)
        {
            var cmd = connection.CreateCommand();
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