using Domain.Entities;

namespace Domain.Repositories
{
	public interface IUserRepository
	{
		Task<User> Get(string username, string password);
	}
}
