using Microsoft.Extensions.Configuration;

namespace DataAccess
{
    public abstract class BaseTemporaryDatabaseRepository(string connectionString)
    {
        internal readonly string databaseName = "db_onda_to_orm_temp";
        internal string ConnectionString = connectionString;
    }
}
