using Domain.Entities;
using Domain.Repositories;

namespace DataAccess
{
    public class UserRepository : IUserRepository
    {
        public User? Get(string username, string password)
        {
            var users = new List<User>
            {
                new() { Id = 1, Username = "root", Password = "root", Role = "manager" }
            };
            return users.FirstOrDefault(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && x.Password.Equals(password, StringComparison.OrdinalIgnoreCase));
        }
    }
}
