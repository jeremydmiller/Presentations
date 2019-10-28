using System.Net.Http;
using System.Threading.Tasks;
using async.Internal;
using Newtonsoft.Json;

namespace async.Async
{
    public class UsingServiceProxy
    {
        private readonly HttpClient _client;

        // Not the subject of this talk, but HttpClient is expensive
        // and needs to be a singleton in your system
        public UsingServiceProxy(HttpClient client)
        {
            _client = client;
        }

        public async Task<User> FindUser(string userId)
        {
            
            var url = "https://someserver.com/users/" + userId;


            var json = await _client.GetStringAsync(url)
                .ConfigureAwait(false);

            // Also not the subject of this talk, but this is 
            // a relatively inefficient way to deal with the
            // Json serialization. 
            return JsonConvert.DeserializeObject<User>(json);

        }
    }
    
    
    
}