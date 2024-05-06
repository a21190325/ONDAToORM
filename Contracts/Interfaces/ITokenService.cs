using Contracts.Dtos;
namespace Contracts.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(UserDto user);
    }
}
