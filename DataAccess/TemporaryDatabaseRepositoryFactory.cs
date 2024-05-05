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
        private readonly IConfiguration configuration = configuration;

        public ITemporaryDatabaseRepository CreateTemporaryDatabaseRepository(string databaseEngine)
        {
            var connStr = configuration[$"ConnectionStrings:{databaseEngine}"];

            if (string.IsNullOrWhiteSpace(databaseEngine) || string.IsNullOrWhiteSpace(connStr))
            {
                throw new NotSupportedException();
            }

            var databaseEngineLower = databaseEngine.ToLower();

            return databaseEngineLower switch
            {
                "postgresql" => new PostgreSQLDatabaseRepository(connStr),
                _ => throw new NotSupportedException()
            };
        }
    }
}
