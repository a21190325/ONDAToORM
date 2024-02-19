using Contracts.Dtos;
using Contracts.Dtos.Shared;
using Contracts.Interfaces;

namespace Services
{
	public class ConverterService : Service<ConverterInputDto, string>, IConverterService
	{
		protected override Task<ResultDto<string>> ExecuteAsync(ConverterInputDto inputDto, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}
