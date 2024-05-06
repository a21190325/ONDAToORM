using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Scaffolding.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using System.Text;

namespace DataAccess
{
    public class PostgreSQLDatabaseRepository(string connectionString) : BaseTemporaryDatabaseRepository(connectionString), ITemporaryDatabaseRepository
    {
        public async Task<bool> CheckDatabaseExistance()
        {
            var databaseExists = false;

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                var databaseExistsQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
                using var command = new NpgsqlCommand(databaseExistsQuery, connection);
                databaseExists = await command.ExecuteScalarAsync() != null;
            }

            return databaseExists;
        }

        public async Task<bool> CreateDatabase()
        {
            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                connection.Open();
                var createDatabaseQuery = $"CREATE DATABASE {databaseName}";
                using var createDatabaseCommand = new NpgsqlCommand(createDatabaseQuery, connection);
                await createDatabaseCommand.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DropDatabase()
        {
            try
            {
                using var connection = new NpgsqlConnection(ConnectionString);
                connection.Open();
                var dropDatabaseQuery = $"DROP DATABASE {databaseName} WITH (FORCE)";
                using var dropDatabaseCommand = new NpgsqlCommand(dropDatabaseQuery, connection);
                await dropDatabaseCommand.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public Task<List<FileDto>> GetCSharpFilesFromDatabase()
        {
            var connectionStringWithDb = $"{ConnectionString};Database={databaseName}";
            List<FileDto> sourceFiles = [];

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

                var scaffoldedModelSources = scaffoldService?.ScaffoldModel(connectionStringWithDb, dbOpts, modelOpts, codeGenOpts);
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
                var connectionStringWithDb = $"{ConnectionString};Database={databaseName}";
                NpgsqlConnection.ClearAllPools();
                using var connection = new NpgsqlConnection(connectionStringWithDb);
                connection.Open();
                using var command = new NpgsqlCommand(sqlScript, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
