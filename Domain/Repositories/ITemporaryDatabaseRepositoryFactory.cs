namespace Domain.Repositories
{
    public interface ITemporaryDatabaseRepositoryFactory
    {
        ITemporaryDatabaseRepository CreateTemporaryDatabaseRepository(string databaseEngine);
    }
}
