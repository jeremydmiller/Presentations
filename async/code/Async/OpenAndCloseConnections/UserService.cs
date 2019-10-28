using System;
using System.Data;
using System.Threading.Tasks;
using async.Internal;
using Microsoft.Data.SqlClient;

namespace AsyncOpenAndCloseConnections
{
    public interface IMessagePublisher
    {
        Task Publish<T>(T @event);
    }
    
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
        public async Task CreateNewUser(NewUserRequest newUserRequest)
        {
            // Do some validation
            AssertIsValid(newUserRequest);
            
            // Create a new User domain object
            var user = new User {UserName = newUserRequest.UserName};

            // Write it to the database
            await PersistUser(user);
            
            
            // Publish an event about the new user
            // to a service bus
            var @event = new UserCreated
            {
                UserName = user.UserName
            };
            
            await _publisher.Publish(@event);
        }

        private async Task PersistUser(User user)
        {
            // Generally always open connections in your own code
            // inside a "using" block
            using (var connection = new SqlConnection(_settings.ConnectionString))
            {
                // Open the connection just in time
                await connection.OpenAsync();

                // Do what you need to do right here
                await writeUser(user, connection);

                // Close it as soon as we're done.
                // Honestly, calling Dispose() is essentially the same thing
                await connection.CloseAsync();
            }
        }

        private static async Task writeUser(User user, SqlConnection connection)
        {
            var cmd = connection.CreateCommand();
            cmd.CommandText = "insert into users (UserName) values (@name)";
            cmd.Parameters.Add("name", SqlDbType.VarChar).Value = user.UserName;
            await cmd.ExecuteNonQueryAsync();
        }

        private void AssertIsValid(NewUserRequest newUserRequest)
        {
            if (string.IsNullOrEmpty(newUserRequest.UserName))
                throw new ArgumentException(nameof(newUserRequest), $"{nameof(newUserRequest.UserName)} is missing");
        }

    }
}