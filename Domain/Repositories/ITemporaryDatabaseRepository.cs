using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface ITemporaryDatabaseRepository
    {
        Task<bool> CheckDatabaseExistance();
        Task<bool> DropDatabase();
        Task<bool> CreateDatabase();
        Task<bool> RunSQLScript(string sqlScript);
        Task<string> GetDatabaseConnectionString();
    }
}
