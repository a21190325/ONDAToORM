using Contracts;
using Contracts.Dtos;
using Contracts.Dtos.Errors;
using Contracts.Dtos.Shared;
using Contracts.Interfaces;
using Domain.Repositories;

namespace Services
{
	public class GenerateTokenService : Service<GenerateTokenInputDto, GenerateTokenOutputDto>, IGenerateTokenService
	{
		private IUserRepository userRepository;
		private ITokenService tokenService;
		private IInstanceMapper instanceMapper;

		public GenerateTokenService(IUserRepository userRepository, ITokenService tokenService, IInstanceMapper instanceMapper)
		{
			this.userRepository = userRepository;
			this.tokenService = tokenService;
			this.instanceMapper = instanceMapper;
		}
		protected override async Task<ResultDto<GenerateTokenOutputDto>> ExecuteAsync(GenerateTokenInputDto inputDto, CancellationToken cancellationToken = default)
		{
			var executionErrors = ValidateInput(inputDto);

			var user = await userRepository.Get(inputDto.Username, inputDto.Password);

			if (user == null)
			{
				executionErrors.Add(new ErrorDto(ErrorCodes.UNAUTHORIZED_ACTION_FOR_CALLING_USER));
			}

			if (executionErrors.Count > 0)
			{
				return BuildOperationResultDto(executionErrors);
			}

			var token = tokenService.GenerateToken(user);
			user.Password = "";

			return BuildOperationResultDto(new GenerateTokenOutputDto
			{
				Token = token,
				User = instanceMapper.Map<UserDto>(user)
			});
		}

		private static List<ErrorDto> ValidateInput(GenerateTokenInputDto inputDto)
		{
			List<ErrorDto> validationErrors = new List<ErrorDto>();

			if (string.IsNullOrWhiteSpace(inputDto.Username))
				validationErrors.Add(new ErrorDto(ErrorCodes.REQUIRED_FIELD_IS_EMPTY, nameof(inputDto.Username)));

			if (string.IsNullOrWhiteSpace(inputDto.Password))
				validationErrors.Add(new ErrorDto(ErrorCodes.REQUIRED_FIELD_IS_EMPTY, nameof(inputDto.Password)));

			return validationErrors;
		}

	}
}
