using Domain.Entities;
using Microsoft.EntityFrameworkCore.Scaffolding;
using System.Text;

namespace DataAccess
{
    public abstract class BaseTemporaryDatabaseRepository(string connectionString)
    {
        internal readonly string databaseName = "db_onda_to_orm_temp";
        internal string ConnectionString = connectionString;

        internal Task<List<FileDto>> GetCSharpFilesWithEFCore(IReverseEngineerScaffolder scaffoldService, string? connectionStringSufix = null, List<string>? schemas = null)
        {
            var connectionStringScaffold = ConnectionString;
            if (connectionStringSufix != null)
                connectionStringScaffold += $";{connectionStringSufix}";
            List<FileDto> sourceFiles = [];

            try
            {
                var dbOpts = new DatabaseModelFactoryOptions(schemas: schemas);
                var modelOpts = new ModelReverseEngineerOptions();
                var codeGenOpts = new ModelCodeGenerationOptions
                {
                    RootNamespace = "ONDAToORM",
                    ContextName = "DataContext",
                    ContextNamespace = "ONDAToORM.Context",
                    ModelNamespace = "ONDAToORM.Models",
                    SuppressConnectionStringWarning = true
                };

                var scaffoldedModelSources = scaffoldService?.ScaffoldModel(connectionStringScaffold, dbOpts, modelOpts, codeGenOpts);
                if (scaffoldedModelSources?.ContextFile != default)
                {
                    var contextFile = scaffoldedModelSources.ContextFile;
                    sourceFiles =
                    [
                        new() {
                            Code = Encoding.UTF8.GetBytes(contextFile.Code),
                            Name = contextFile.Path
                        }
                    ];
                }
                if (scaffoldedModelSources?.AdditionalFiles != default)
                    sourceFiles.AddRange(scaffoldedModelSources.AdditionalFiles.Select(x => new FileDto()
                    {
                        Code = Encoding.UTF8.GetBytes(x.Code),
                        Name = x.Path
                    }));
            }
            catch (Exception)
            {
                throw new ArgumentException("Exception executing scaffolding command.", ConnectionString);
            }

            return Task.FromResult(sourceFiles);
        }
    }
}
