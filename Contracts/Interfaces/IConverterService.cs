using Contracts.Dtos;
using Contracts.Dtos.Shared;
using Contracts.Interfaces.Shared;

namespace Contracts.Interfaces
{
    public interface IConverterService : IService<ConverterInputDto, List<FileDto>>
    {
    }
}
