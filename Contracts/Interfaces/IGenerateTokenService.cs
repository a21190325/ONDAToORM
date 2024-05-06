using Contracts.Dtos;
using Contracts.Interfaces.Shared;

namespace Contracts.Interfaces
{
    public interface IGenerateTokenService : IService<GenerateTokenInputDto, GenerateTokenOutputDto>
    {
    }
}
