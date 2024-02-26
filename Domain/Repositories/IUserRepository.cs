using Domain.Entities;

namespace Domain.Repositories
{
	public interface IUserRepository
	{
		User? Get(string username, string password);
	}
}
