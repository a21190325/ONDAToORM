using Contracts.Dtos.Shared;

namespace Contracts.Interfaces.Shared
{
    public interface IService<in TIn, TOut>
    {
        Task<ResultDto<TOut>> ExecuteServiceAsync(TIn inputDto, CancellationToken cancellationToken = default);
    }
}
