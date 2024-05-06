using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Oracle.EntityFrameworkCore.Diagnostics.Internal;
using Oracle.EntityFrameworkCore.Scaffolding.Internal;
using Oracle.EntityFrameworkCore.Storage.Internal;
using Oracle.ManagedDataAccess.Client;

namespace DataAccess
{
    public class OracleDatabaseRepository(string connectionString) : BaseTemporaryDatabaseRepository(connectionString), ITemporaryDatabaseRepository
    {
        public async Task<bool> CheckDatabaseExistance()
        {
            var databaseExists = false;

            using (OracleConnection connection = new(ConnectionString))
            {
                await connection.OpenAsync();
                var databaseExistsQuery = $"SELECT COUNT(*) FROM dba_users WHERE username = '{databaseName.ToUpper()}'";
                using OracleCommand command = new(databaseExistsQuery, connection);
                var result = await command.ExecuteScalarAsync();
                databaseExists = result != DBNull.Value && Convert.ToInt32(result) > 0;
            }

            return databaseExists;
        }

        public async Task<bool> CreateDatabase()
        {
            try
            {
                using OracleConnection connection = new(ConnectionString);
                connection.Open();
                var createDatabaseQuery = $"CREATE USER {databaseName} IDENTIFIED BY password";
                using OracleCommand createDatabaseCommand = new(createDatabaseQuery, connection);
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
                using OracleConnection connection = new(ConnectionString);
                await connection.OpenAsync();
                var dropDatabaseQuery = $"DROP USER {databaseName} CASCADE";
                using OracleCommand dropDatabaseCommand = new(dropDatabaseQuery, connection);
                await dropDatabaseCommand.ExecuteNonQueryAsync();
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
               .AddEntityFrameworkOracle()
               .AddLogging()
               .AddEntityFrameworkDesignTimeServices()
               .AddSingleton<LoggingDefinitions, OracleLoggingDefinitions>()
               .AddSingleton<IRelationalTypeMappingSource, OracleTypeMappingSource>()
               .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
               .AddSingleton<IDatabaseModelFactory, OracleDatabaseModelFactory>()
               .AddSingleton<IProviderConfigurationCodeGenerator, OracleCodeGenerator>()
               .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
               .AddSingleton<IPluralizer, Bricelam.EntityFrameworkCore.Design.Pluralizer>()
               .AddSingleton<ProviderCodeGeneratorDependencies>()
               .AddSingleton<AnnotationCodeGeneratorDependencies>()
               .BuildServiceProvider()
               .GetRequiredService<IReverseEngineerScaffolder>();

            return await GetCSharpFilesWithEFCore(scaffoldService, schemas: [databaseName.ToUpper()]);
        }

        public async Task<bool> RunSQLScript(string sqlScript)
        {
            sqlScript = $"alter session set current_schema = {databaseName};{sqlScript}";
            string[] splitQuerys = sqlScript.Split(';');

            using OracleConnection connection = new(ConnectionString);
            await connection.OpenAsync();

            foreach (var query in splitQuerys)
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    try
                    {
                        OracleCommand command = connection.CreateCommand();
                        command.CommandText = query;
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
