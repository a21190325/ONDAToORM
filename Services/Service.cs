using Contracts.Dtos.Errors;
using Contracts.Dtos.Shared;
using Contracts.Interfaces.Shared;

namespace Services
{
    public abstract class Service<TIn, TOut> : IService<TIn, TOut>
    {
        protected abstract Task<ResultDto<TOut>> ExecuteAsync(TIn inputDto, CancellationToken cancellationToken = default);

        protected ResultDto<TOut> BuildOperationResultDto(TOut data, IEnumerable<ErrorDto>? executionWarnings = null)
        {
            return new ResultDto<TOut>
            {
                IsValid = true,
                Data = data,
                Errors = executionWarnings?.ToHashSet() ?? []
            };
        }

        public virtual async Task<ResultDto<TOut>> ExecuteServiceAsync(TIn inputDto, CancellationToken cancellationToken)
        {
            ResultDto<TOut> result;

            try
            {
                result = await ExecuteAsync(inputDto, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result = BuildOperationResultDto(new HashSet<ErrorDto> {
                    new(
                        ErrorCodes.APPLICATION_ERROR,
                        ex.Message
                    )
                });
            }

            return result;
        }

        protected ResultDto<TOut> BuildOperationResultDto(IEnumerable<ErrorDto> executionErrors) => new() { IsValid = false, Errors = executionErrors.ToHashSet() };

        protected ResultDto<TOut> BuildOperationResultDto(params ErrorDto[] errors) => new() { IsValid = false, Errors = [.. errors] };
    }
}
