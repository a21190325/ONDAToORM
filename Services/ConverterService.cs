using Contracts.Dtos;
using Contracts.Dtos.Errors;
using Contracts.Dtos.Shared;
using Contracts.Interfaces;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Services
{
    public class ConverterService(ITemporaryDatabaseRepositoryFactory temporaryDatabaseRepositoryFactory) : Service<ConverterInputDto, List<FileDto>>, IConverterService
    {
        private readonly ITemporaryDatabaseRepositoryFactory temporaryDatabaseRepositoryFactory = temporaryDatabaseRepositoryFactory;

        protected override async Task<ResultDto<List<FileDto>>> ExecuteAsync(ConverterInputDto inputDto, CancellationToken cancellationToken = default)
        {
            var executionErrors = ValidateInput(inputDto);
            List<FileDto> result = [];

            if (executionErrors.Count > 0)
            {
                return BuildOperationResultDto(executionErrors);
            }

            ITemporaryDatabaseRepository? temporaryDatabaseRepository;
            try
            {
                temporaryDatabaseRepository = temporaryDatabaseRepositoryFactory.CreateTemporaryDatabaseRepository(inputDto.DatabaseEngine);
            }
            catch (Exception)
            {
                return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_DATABASE_ENGINE_NOT_SUPPORTED));
            }

            try
            {
                var sqlContent = Encoding.UTF8.GetString(Convert.FromBase64String(inputDto.SqlContentInBase64));
                var databaseExists = await temporaryDatabaseRepository.CheckDatabaseExistance();

                if (databaseExists)
                {
                    bool dropSucc = await temporaryDatabaseRepository.DropDatabase();
                    if (!dropSucc)
                    {
                        return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_DROPPING_TEMPORARY_DATABASE));
                    }
                }

                bool createSucc = await temporaryDatabaseRepository.CreateDatabase();
                if (!createSucc)
                {
                    await temporaryDatabaseRepository.DropDatabase();
                    return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_CREATING_NEW_TEMPORARY_DATABASE));
                }

                bool runSQLSucc = await temporaryDatabaseRepository.RunSQLScript(sqlContent);
                if (!runSQLSucc)
                {
                    await temporaryDatabaseRepository.DropDatabase();
                    return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_RUNNING_SCRIPT_ON_TEMPORARY_DATABASE));
                }

                try
                {
                    var filesResult = await temporaryDatabaseRepository.GetCSharpFilesFromDatabase();
                    result = filesResult.Select(fr => new FileDto
                    {
                        Code = Convert.ToBase64String(fr.Code),
                        Name = fr.Name
                    }).ToList();

                }
                catch (Exception ex)
                {
                    executionErrors.Add(new ErrorDto(ErrorCodes.ERROR_EXECUTING_SCAFFOLDING_COMMAND, ex.Message));
                }

                await temporaryDatabaseRepository.DropDatabase();

                return BuildOperationResultDto(result, executionErrors);
            }
            catch (Exception ex)
            {
                await temporaryDatabaseRepository.DropDatabase();
                return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_PROCESSING_SQL_ON_DB_SERVER, ex.Message));
            }
        }

        private static List<ErrorDto> ValidateInput(ConverterInputDto inputDto)
        {
            List<ErrorDto> validationErrors = [];

            if (string.IsNullOrWhiteSpace(inputDto.SqlContentInBase64))
                validationErrors.Add(new ErrorDto(ErrorCodes.REQUIRED_FIELD_IS_EMPTY, nameof(inputDto.SqlContentInBase64)));

            return validationErrors;
        }
    }
}
