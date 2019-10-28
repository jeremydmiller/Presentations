using System.Linq;
using System.Threading.Tasks;
using async.Internal;

namespace UsingValueTask
{
    
    public interface IUserRepository
    {
        // TODO -- switch to ValueTask
        Task<User> Load(string userId);

        Task Initialize();
    }
    
    public class StubUserRepository : IUserRepository
    {
        private readonly User[] _users;

        public StubUserRepository(User[] users)
        {
            _users = users;
        }

        public Task Initialize()
        {
            // nothing
            
            // but return a Task that will allow calling code
            // to continue
            return Task.CompletedTask;
        }

        public Task<User> Load(string userId)
        {
            var user = _users.FirstOrDefault(x => x.UserName == userId);
            return Task.FromResult(user);
        }
    }
}