using Contracts.Dtos;

namespace Contracts.Interfaces
{
    public interface IUserTokenService
    {
        Task<string> GetUserTokenAsync(GenerateTokenInputDto inputDto);
    }
}
