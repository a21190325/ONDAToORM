using Domain.Entities;

namespace Contracts.Interfaces
{
	public interface ITokenService
	{
		string GenerateToken(User user);
	}
}
