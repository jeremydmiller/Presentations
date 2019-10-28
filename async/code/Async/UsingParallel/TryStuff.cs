using System.Threading.Tasks;
using async.Internal;

namespace async.Async.UsingParallel
{
    public class UserValidator
    {
        public ValidationResult[] Validate(User[] users)
        {
            return Parallel.ForEach(users, user => Validate(user));
        }

        public ValidationResult Validate(User user)
        {
            return new ValidationResult();
        }
    }
    
    public class ValidationResult{}
}