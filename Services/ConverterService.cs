using Contracts.Dtos;
using Contracts.Dtos.Errors;
using Contracts.Dtos.Shared;
using Contracts.Interfaces;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using System;
using System.Text;

namespace Services
{
    public class ConverterService : Service<ConverterInputDto, List<FileDto>>, IConverterService
    {
        private readonly ITemporaryDatabaseRepositoryFactory temporaryDatabaseRepositoryFactory;

        public ConverterService(IConfiguration configuration, ITemporaryDatabaseRepositoryFactory temporaryDatabaseRepositoryFactory)
        {
            this.temporaryDatabaseRepositoryFactory = temporaryDatabaseRepositoryFactory;
        }

        protected override async Task<ResultDto<List<FileDto>>> ExecuteAsync(ConverterInputDto inputDto, CancellationToken cancellationToken = default)
        {
            var executionErrors = ValidateInput(inputDto);
            ResultDto<List<FileDto>> result;

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
                    return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_CREATING_NEW_TEMPORARY_DATABASE));
                }

                bool runSQLSucc = await temporaryDatabaseRepository.RunSQLScript(sqlContent);
                if (!runSQLSucc)
                {
                    return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_RUNNING_SCRIPT_ON_TEMPORARY_DATABASE));
                }

                result = GetCSharpFilesFromDatabase(await temporaryDatabaseRepository.GetDatabaseConnectionString());
                executionErrors.AddRange(result.Errors);

                await temporaryDatabaseRepository.DropDatabase();
            }
            catch (Exception ex)
            {
                await temporaryDatabaseRepository.DropDatabase();
                return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_PROCESSING_SQL_ON_DB_SERVER, ex.Message));
            }

            return BuildOperationResultDto(result.Data, executionErrors);
        }

        private static List<ErrorDto> ValidateInput(ConverterInputDto inputDto)
        {
            List<ErrorDto> validationErrors = [];

            if (string.IsNullOrWhiteSpace(inputDto.SqlContentInBase64))
                validationErrors.Add(new ErrorDto(ErrorCodes.REQUIRED_FIELD_IS_EMPTY, nameof(inputDto.SqlContentInBase64)));

            return validationErrors;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        private static ResultDto<List<FileDto>> GetCSharpFilesFromDatabase(string connectionString)
        {
            List<ErrorDto> validationErrors = [];
            var sourceFiles = new List<FileDto>();

            try
            {
                var scaffoldService = new ServiceCollection()
                   .AddEntityFrameworkNpgsql()
                   .AddLogging()
                   .AddEntityFrameworkDesignTimeServices()
                   .AddSingleton<LoggingDefinitions, NpgsqlLoggingDefinitions>()
                   .AddSingleton<IRelationalTypeMappingSource, NpgsqlTypeMappingSource>()
                   .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                   .AddSingleton<IDatabaseModelFactory, NpgsqlDatabaseModelFactory>()
                   .AddSingleton<IProviderConfigurationCodeGenerator, NpgsqlCodeGenerator>()
                   .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                   .AddSingleton<IPluralizer, Bricelam.EntityFrameworkCore.Design.Pluralizer>()
                   .AddSingleton<ProviderCodeGeneratorDependencies>()
                   .AddSingleton<AnnotationCodeGeneratorDependencies>()
                   .BuildServiceProvider()
                   .GetRequiredService<IReverseEngineerScaffolder>();

                var dbOpts = new DatabaseModelFactoryOptions();
                var modelOpts = new ModelReverseEngineerOptions();
                var codeGenOpts = new ModelCodeGenerationOptions
                {
                    RootNamespace = "ONDAToORM",
                    ContextName = "DataContext",
                    ContextNamespace = "ONDAToORM.Context",
                    ModelNamespace = "ONDAToORM.Models",
                    SuppressConnectionStringWarning = true
                };

                var scaffoldedModelSources = scaffoldService?.ScaffoldModel(connectionString, dbOpts, modelOpts, codeGenOpts);
                if (scaffoldedModelSources?.ContextFile != default)
                {
                    var contextFile = scaffoldedModelSources.ContextFile;
                    sourceFiles =
                    [
                        new() {
                            Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(contextFile.Code)),
                            Name = contextFile.Path
                        }
                    ];
                }
                if (scaffoldedModelSources?.AdditionalFiles != default)
                    sourceFiles.AddRange(scaffoldedModelSources.AdditionalFiles.Select(x => new FileDto()
                    {
                        Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(x.Code)),
                        Name = x.Path
                    }));
            }
            catch (Exception ex)
            {
                validationErrors.Add(new ErrorDto(ErrorCodes.ERROR_EXECUTING_SCAFFOLDING_COMMAND, ex.Message));
            }

            return new ResultDto<List<FileDto>>
            {
                Data = sourceFiles,
                Errors = [.. validationErrors]
            };
        }
    }
}
