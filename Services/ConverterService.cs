using Contracts.Dtos;
using Contracts.Dtos.Errors;
using Contracts.Dtos.Shared;
using Contracts.Interfaces;
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
		private readonly string databaseConnString;
		private readonly string databaseName;

        public ConverterService(IConfiguration configuration)
        {
			databaseConnString = configuration["ConnectionStrings:PostgreSQL"] ?? throw new ArgumentNullException(nameof(databaseConnString));
			databaseName = configuration["TempDatabaseName"] ?? throw new ArgumentNullException(nameof(databaseName));
		}

        protected override async Task<ResultDto<List<FileDto>>> ExecuteAsync(ConverterInputDto inputDto, CancellationToken cancellationToken = default)
		{
			var executionErrors = ValidateInput(inputDto);
			ResultDto<List<FileDto>> result;

			if (executionErrors.Count > 0)
			{
				return BuildOperationResultDto(executionErrors);
			}

			var sqlContent = Encoding.UTF8.GetString(Convert.FromBase64String(inputDto.SqlContentInBase64));
			var connectionString = databaseConnString;

			try
			{
				var databaseExists = false;

				using (var connection = new NpgsqlConnection(connectionString))
				{
					connection.Open();
					var databaseExistsQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
					using var command = new NpgsqlCommand(databaseExistsQuery, connection);
					databaseExists = command.ExecuteScalar() != null;
				}

				if (databaseExists)
				{
					DropDatabase(connectionString, databaseName);
				}

				using (var connection = new NpgsqlConnection(connectionString))
				{
					connection.Open();
					var createDatabaseQuery = $"CREATE DATABASE {databaseName}";
					using var createDatabaseCommand = new NpgsqlCommand(createDatabaseQuery, connection);
					createDatabaseCommand.ExecuteNonQuery();
				}

				connectionString += $";Database={databaseName}";

				NpgsqlConnection.ClearAllPools();
				using (var connection = new NpgsqlConnection(connectionString))
				{
					connection.Open();
					using var command = new NpgsqlCommand(sqlContent, connection);
					command.ExecuteNonQuery();
				}

				result = GetCSharpFilesFromDatabase(connectionString);
				executionErrors.AddRange(result.Errors);

				connectionString = connectionString.Replace($";Database={databaseName}", "");

				DropDatabase(connectionString, databaseName);
			}
			catch (Exception ex)
			{
				DropDatabase(connectionString, databaseName);
				return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_PROCESSING_SQL_ON_DB_SERVER, ex.Message));
			}

			return BuildOperationResultDto(result.Data, executionErrors);
		}

		private static void DropDatabase(string connectionString, string databaseName)
		{
			using var connection = new NpgsqlConnection(connectionString);
			connection.Open();
			var dropDatabaseQuery = $"DROP DATABASE {databaseName} WITH (FORCE)";
			using var dropDatabaseCommand = new NpgsqlCommand(dropDatabaseQuery, connection);
			dropDatabaseCommand.ExecuteNonQuery();
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
