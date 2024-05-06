using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Diagnostics.Internal;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
using System.Text;

namespace DataAccess
{
    public class MySQLDatabaseRepository(string connectionString) : BaseTemporaryDatabaseRepository(connectionString), ITemporaryDatabaseRepository
    {
        public async Task<bool> CheckDatabaseExistance()
        {
            bool databaseExists = false;
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var databaseExistsQuery = $"SELECT 1 FROM information_schema.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'";
                using var command = new MySqlCommand(databaseExistsQuery, connection);
                databaseExists = await command.ExecuteScalarAsync() != null;
            }
            return databaseExists;
        }

        public async Task<bool> CreateDatabase()
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();
                var createDatabaseQuery = $"CREATE DATABASE `{databaseName}`";
                using var createDatabaseCommand = new MySqlCommand(createDatabaseQuery, connection);
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
                using var connection = new MySqlConnection(ConnectionString);
                await connection.OpenAsync();
                var dropDatabaseQuery = $"DROP DATABASE IF EXISTS `{databaseName}`";
                using var dropDatabaseCommand = new MySqlCommand(dropDatabaseQuery, connection);
                await dropDatabaseCommand.ExecuteNonQueryAsync();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> RunSQLScript(string sqlScript)
        {
            try
            {
                var connectionStringWithDb = $"{ConnectionString};Database={databaseName}";
                using var connection = new MySqlConnection(connectionStringWithDb);
                await connection.OpenAsync();
                using var command = new MySqlCommand(sqlScript, connection);
                await command.ExecuteNonQueryAsync();
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
                   .AddEntityFrameworkMySql()
                   .AddLogging()
                   .AddEntityFrameworkDesignTimeServices()
                   .AddSingleton<LoggingDefinitions, MySqlLoggingDefinitions>()
                   .AddSingleton<IRelationalTypeMappingSource, MySqlTypeMappingSource>()
                   .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                   .AddSingleton<IDatabaseModelFactory, MySqlDatabaseModelFactory>()
                   .AddSingleton<IProviderConfigurationCodeGenerator, MySqlCodeGenerator>()
                   .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                   .AddSingleton<IPluralizer, Bricelam.EntityFrameworkCore.Design.Pluralizer>()
                   .AddSingleton<ProviderCodeGeneratorDependencies>()
                   .AddSingleton<AnnotationCodeGeneratorDependencies>()
                   .BuildServiceProvider()
                   .GetRequiredService<IReverseEngineerScaffolder>();

                var dbOpts = new DatabaseModelFactoryOptions(schemas: [databaseName]);
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
            catch (Exception)
            {
                throw new ArgumentException("Exception executing scaffolding command.", ConnectionString);
            }

            return Task.FromResult(sourceFiles);
        }
    }
}
