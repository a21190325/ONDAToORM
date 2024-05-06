using Domain.Entities;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class MySQLDatabaseRepository(string connectionString) : BaseTemporaryDatabaseRepository(connectionString), ITemporaryDatabaseRepository
    {
        //public async Task<bool> CheckDatabaseExistance()
        //{
        //    var databaseExists = false;

        //    using (var connection = new NpgsqlConnection(connectionString))
        //    {
        //        connection.Open();
        //        var databaseExistsQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
        //        using var command = new NpgsqlCommand(databaseExistsQuery, connection);
        //        databaseExists = await command.ExecuteScalarAsync() != null;
        //    }

        //    return databaseExists;
        //}

        //public async Task<bool> CreateDatabase()
        //{
        //    try
        //    {
        //        using var connection = new NpgsqlConnection(connectionString);
        //        connection.Open();
        //        var createDatabaseQuery = $"CREATE DATABASE {databaseName}";
        //        using var createDatabaseCommand = new NpgsqlCommand(createDatabaseQuery, connection);
        //        await createDatabaseCommand.ExecuteNonQueryAsync();
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //public async Task<bool> DropDatabase()
        //{
        //    try
        //    {
        //        using var connection = new NpgsqlConnection(connectionString);
        //        connection.Open();
        //        var dropDatabaseQuery = $"DROP DATABASE {databaseName} WITH (FORCE)";
        //        using var dropDatabaseCommand = new NpgsqlCommand(dropDatabaseQuery, connection);
        //        await dropDatabaseCommand.ExecuteNonQueryAsync();
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        //public Task<string> GetDatabaseConnectionString()
        //{
        //    return Task.FromResult(connectionString);
        //}

        //public async Task<bool> RunSQLScript(string sqlScript)
        //{
        //    try
        //    {
        //        var connectionStringWithDb = $"{connectionString};Database={databaseName}";
        //        NpgsqlConnection.ClearAllPools();
        //        using var connection = new NpgsqlConnection(connectionStringWithDb);
        //        connection.Open();
        //        using var command = new NpgsqlCommand(sqlScript, connection);
        //        await command.ExecuteNonQueryAsync();
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }

        //    return true;
        //}
        public Task<bool> CheckDatabaseExistance()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateDatabase()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DropDatabase()
        {
            throw new NotImplementedException();
        }

        public Task<List<FileDto>> GetCSharpFilesFromDatabase()
        {
            throw new NotImplementedException();
        }

        public Task<bool> RunSQLScript(string sqlScript)
        {
            throw new NotImplementedException();
        }
    }
}
