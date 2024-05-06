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
using System.Text;

namespace DataAccess
{
    public class SQLiteDatabaseRepository : BaseTemporaryDatabaseRepository, ITemporaryDatabaseRepository
    {
        public SQLiteDatabaseRepository(string connectionString) : base(connectionString)
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            builder.DataSource = Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.GetData("DataDirectory") as string
                        ?? AppDomain.CurrentDomain.BaseDirectory,
                    builder.DataSource));
            ConnectionString = builder.ToString();
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

        public Task<List<FileDto>> GetCSharpFilesFromDatabase()
        {
            List<FileDto> sourceFiles = [];

            try
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

                var scaffoldedModelSources = scaffoldService?.ScaffoldModel(ConnectionString, dbOpts, modelOpts, codeGenOpts);
                if (scaffoldedModelSources?.ContextFile != default)
                {
                    var contextFile = scaffoldedModelSources.ContextFile;
                    sourceFiles =
                    [
                        new() {
                            Code = Encoding.UTF8.GetBytes(contextFile.Code),
                            Name = contextFile.Path
                        }
                    ];
                }
                if (scaffoldedModelSources?.AdditionalFiles != default)
                    sourceFiles.AddRange(scaffoldedModelSources.AdditionalFiles.Select(x => new FileDto()
                    {
                        Code = Encoding.UTF8.GetBytes(x.Code),
                        Name = x.Path
                    }));
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Exception executing scaffolding command.", ConnectionString);
            }

            return Task.FromResult(sourceFiles);
        }

        public async Task<bool> RunSQLScript(string sqlScript)
        {
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
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
