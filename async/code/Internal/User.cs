using System;

namespace async.Internal
{
    public class User
    {
        public User()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public bool Internal { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public int Age { get; set; }


        public void From(User user)
        {
            Id = user.Id;
        }
    }
}