using Domain.Repositories;
using Microsoft.Extensions.Configuration;

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
                "mysql" => new MySQLDatabaseRepository(connectionString),
                "mariadb" => new MySQLDatabaseRepository(connectionString),
                "sqlite" => new SQLiteDatabaseRepository(connectionString),
                _ => throw new ArgumentException($"Database engine '{databaseEngine}' is not supported.", nameof(databaseEngine))
            };
        }
    }
}
