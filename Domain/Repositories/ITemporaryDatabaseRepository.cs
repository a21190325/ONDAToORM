using Domain.Entities;

namespace Domain.Repositories
{
    public interface ITemporaryDatabaseRepository
    {
        Task<bool> CheckDatabaseExistance();
        Task<bool> DropDatabase();
        Task<bool> CreateDatabase();
        Task<bool> RunSQLScript(string sqlScript);
        Task<List<FileDto>> GetCSharpFilesFromDatabase();
    }
}
