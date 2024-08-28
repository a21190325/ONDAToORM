using Domain.Entities;
using Domain.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DataAccess
{
    public class SQLiteDatabaseRepository : BaseTemporaryDatabaseRepository, ITemporaryDatabaseRepository
    {
        private readonly ILogger _logger;

        public SQLiteDatabaseRepository(string connectionString) : base(connectionString)
        {
            _logger = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            }).CreateLogger<SQLiteDatabaseRepository>();

            var builder = new SqliteConnectionStringBuilder(connectionString);
            builder.DataSource = Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.GetData("DataDirectory") as string
                        ?? AppDomain.CurrentDomain.BaseDirectory,
                    builder.DataSource));
            ConnectionString = builder.ToString();

            _logger.LogInformation("SQLiteDatabaseRepository");
        }

        public async Task<bool> CheckDatabaseExistance()
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                await connection.OpenAsync();
                var query = "SELECT name FROM sqlite_master WHERE type='table'";
                using var command = new SqliteCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                int rowCount = 0;
                while (await reader.ReadAsync())
                {
                    rowCount++;
                }

                return rowCount > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<bool> CreateDatabase()
        {
            return Task.FromResult(true);
        }

        public async Task<bool> DropDatabase()
        {
            try
            {
                List<string> tablesToDelete = [];
                using var connection = new SqliteConnection(ConnectionString);
                await connection.OpenAsync();
                using var transaction = await connection.BeginTransactionAsync();
                var tableNamesQuery = "SELECT name FROM sqlite_master WHERE type='table'";
                using var command = connection.CreateCommand();
                command.CommandText = tableNamesQuery;
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var tableName = reader.GetString(0);
                    tablesToDelete.Add(tableName);
                }

                foreach (var tableName in tablesToDelete)
                {
                    var clearTableQuery = $"DROP TABLE IF EXISTS {tableName}";
                    using var command2 = connection.CreateCommand();
                    command2.CommandText = clearTableQuery;
                    await command2.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<List<FileDto>> GetCSharpFilesFromDatabase()
        {
            var scaffoldService = new ServiceCollection()
               .AddEntityFrameworkSqlite()
               .AddEntityFrameworkDesignTimeServices()
               .AddSingleton<LoggingDefinitions, SqliteLoggingDefinitions>()
               .AddSingleton<IRelationalTypeMappingSource, SqliteTypeMappingSource>()
               .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
               .AddSingleton<IDatabaseModelFactory, SqliteDatabaseModelFactory>()
               .AddSingleton<IProviderConfigurationCodeGenerator, SqliteCodeGenerator>()
               .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
               .AddSingleton<IPluralizer, Bricelam.EntityFrameworkCore.Design.Pluralizer>()
               .AddSingleton<ProviderCodeGeneratorDependencies>()
               .AddSingleton<AnnotationCodeGeneratorDependencies>()
               .BuildServiceProvider()
               .GetRequiredService<IReverseEngineerScaffolder>();

            return await GetCSharpFilesWithEFCore(scaffoldService);
        }

        public async Task<bool> RunSQLScript(string sqlScript)
        {
            _logger.LogInformation("RunSQLScript");

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                await connection.OpenAsync();
                using var transaction = connection.BeginTransaction();
                var command = connection.CreateCommand();
                command.CommandText = sqlScript;
                await command.ExecuteNonQueryAsync();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RunSQLScript exception");
                return false;
            }
            return true;
        }
    }
}
