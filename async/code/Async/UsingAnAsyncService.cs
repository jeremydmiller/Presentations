using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using async.Internal;
using Dapper;
using Microsoft.Data.SqlClient;

namespace async.Async
{

    public interface IUserRepository : IDisposable
    {
        Task<User> Load(string userId);

        Task Initialize();
    }

    public class UserService
    {

        // THIS IS WRONG
        public Task<User> FindUser(string userId)
        {
            using (var repo = buildRepository())
            {
                return repo.Load(userId);
            }
        }
        
        public async Task<User> FindUserBetter(string userId)
        {
            using (var repo = buildRepository())
            {
                return await repo.Load(userId);
            }
        }

        private IUserRepository buildRepository()
        {
            throw new NotImplementedException();
        }
    }


    public class UserServiceWithDapper
    {
        private readonly DatabaseSettings _settings;

        public UserServiceWithDapper(DatabaseSettings settings)
        {
            _settings = settings;
        }

        public Task<IEnumerable<User>> FindUsersInRegion(string region)
        {
            using (var conn = new SqlConnection(_settings.ConnectionString))
            {
                // This should be async, but let that go for the sake
                // of this demo...
                conn.Open();


                // 
                return conn.QueryAsync<User>("select * from users where region = @region", new {region = region});

            }
        }
    }
}