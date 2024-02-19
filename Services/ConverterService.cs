using Contracts.Dtos;
using Contracts.Dtos.Errors;
using Contracts.Dtos.Shared;
using Contracts.Interfaces;
using Npgsql;
using System.Text;

namespace Services
{
	public class ConverterService : Service<ConverterInputDto, string>, IConverterService
	{
		protected override async Task<ResultDto<string>> ExecuteAsync(ConverterInputDto inputDto, CancellationToken cancellationToken = default)
		{
			var executionErrors = ValidateInput(inputDto);

			if (executionErrors.Count > 0)
			{
				return BuildOperationResultDto(executionErrors);
			}

			var sqlContent = Encoding.UTF8.GetString(Convert.FromBase64String(inputDto.SqlContentInBase64));
			var connectionString = "Host=192.168.68.76;Username=postgres;Password=POSTGRES_2023#";
			var databaseName = "db_onda_to_orm_temp";

			try
			{
				using (var connection = new NpgsqlConnection(connectionString))
				{
					connection.Open();

					var databaseExistsQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
					using var command = new NpgsqlCommand(databaseExistsQuery, connection);
					var databaseExists = command.ExecuteScalar() != null;

					if (databaseExists)
					{
						DropDatabase(connectionString, databaseName);
					}

					var createDatabaseQuery = $"CREATE DATABASE {databaseName}";
					using var createDatabaseCommand = new NpgsqlCommand(createDatabaseQuery, connection);
					createDatabaseCommand.ExecuteNonQuery();
				}

				connectionString += $";Database={databaseName}";

				using (var connection = new NpgsqlConnection(connectionString))
				{
					connection.Open();

					using var command = new NpgsqlCommand(sqlContent, connection);
					command.ExecuteNonQuery();
				}

				//TODO: Run scafold and get C# files

				connectionString = connectionString.Replace($";Database={databaseName}", "");

				DropDatabase(connectionString, databaseName);
			}
			catch (Exception ex)
			{
				return BuildOperationResultDto(new ErrorDto(ErrorCodes.ERROR_PROCESSING_SQL_ON_DB_SERVER, ex.Message));
			}

			return BuildOperationResultDto(sqlContent);
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
			List<ErrorDto> validationErrors = new List<ErrorDto>();

			if (string.IsNullOrWhiteSpace(inputDto.SqlContentInBase64))
				validationErrors.Add(new ErrorDto(ErrorCodes.REQUIRED_FIELD_IS_EMPTY, nameof(inputDto.SqlContentInBase64)));

			return validationErrors;
		}
	}
}
