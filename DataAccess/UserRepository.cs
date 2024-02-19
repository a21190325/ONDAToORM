using Domain.Entities;
using Domain.Repositories;

namespace DataAccess
{
	public class UserRepository : IUserRepository
	{
		public async Task<User> Get(string username, string password)
		{
			var users = new List<User>();
			users.Add(new User { Id = 1, Username = "root", Password = "root", Role = "manager" });
			return users.FirstOrDefault(x => x.Username.ToLower() == username.ToLower() && x.Password.ToLower() == password);
		}
	}
}
