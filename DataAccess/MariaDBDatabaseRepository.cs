using Domain.Entities;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class MariaDBDatabaseRepository(string connectionString) : BaseTemporaryDatabaseRepository(connectionString), ITemporaryDatabaseRepository
    {
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
