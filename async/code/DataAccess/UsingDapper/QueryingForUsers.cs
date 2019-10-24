using System.Linq;
using async.Internal;
using Dapper;
using Microsoft.Data.SqlClient;

namespace async.UsingDapper
{
    public class UserRepository
    {
        private readonly DatabaseSettings _settings;

        public UserRepository(DatabaseSettings settings)
        {
            _settings = settings;
        }

        public User[] FindAllUsers()
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                conn.Open();

                // Convention based property to field matching
                return conn.Query<User>("select * from users").ToArray();
            }
        }
    }
}