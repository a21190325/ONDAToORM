using Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class TemporaryDatabaseRepositoryFactory(IConfiguration configuration) : ITemporaryDatabaseRepositoryFactory
    {
        private readonly IConfiguration configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        public ITemporaryDatabaseRepository CreateTemporaryDatabaseRepository(string databaseEngine)
        {
            if (string.IsNullOrWhiteSpace(databaseEngine))
            {
                throw new ArgumentException("Database engine cannot be null or empty.", nameof(databaseEngine));
            }

            var connectionString = configuration[$"ConnectionStrings:{databaseEngine}"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException($"Connection string for '{databaseEngine}' not found.", nameof(connectionString));
            }

            return databaseEngine.ToLower() switch
            {
                "postgresql" => new PostgreSQLDatabaseRepository(connectionString),
                "oracle" => new OracleDatabaseRepository(connectionString),
                _ => throw new ArgumentException($"Database engine '{databaseEngine}' is not supported.", nameof(databaseEngine))
            };
        }
    }
}
