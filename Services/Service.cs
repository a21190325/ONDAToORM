using Contracts.Dtos.Errors;
using Contracts.Dtos.Shared;
using Contracts.Interfaces.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
	public abstract class Service<TIn, TOut> : IService<TIn, TOut>
	{
		protected abstract Task<ResultDto<TOut>> ExecuteAsync(TIn inputDto, CancellationToken cancellationToken = default);

		protected ResultDto<TOut> BuildOperationResultDto(TOut data, IEnumerable<ErrorDto> executionWarnings = null)
		{
			return new ResultDto<TOut>
			{
				IsValid = true,
				Data = data,
				Errors = executionWarnings?.ToHashSet() ?? new HashSet<ErrorDto>()
			};
		}

		public virtual async Task<ResultDto<TOut>> ExecuteServiceAsync(TIn inputDto, CancellationToken cancellationToken)
		{
			ResultDto<TOut> result = new();

			try
			{
				result = await ExecuteAsync(inputDto, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception)
			{
				result = BuildOperationResultDto(new HashSet<ErrorDto> {
					new(
						ErrorCodes.APPLICATION_ERROR,
						"Contact system owner"
					)
				});
			}

			return result;
		}

		protected ResultDto<TOut> BuildOperationResultDto(IEnumerable<ErrorDto> executionErrors) => new ResultDto<TOut> { IsValid = false, Errors = executionErrors.ToHashSet() };

		protected ResultDto<TOut> BuildOperationResultDto(params ErrorDto[] errors) => new ResultDto<TOut> { IsValid = false, Errors = errors.ToHashSet() };
	}
}
