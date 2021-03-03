using Lib.ChainOfResponsibilityPattern;
using System.Threading;

namespace Lib.Handlers.Database
{
    public interface IImportDatabaseHandler : IRequestHandler
    {
        void ImportBacPacFile(string sourceBacPacFilePath, string sourceServerName, string targetDatabaseName, CancellationToken cancellationToken = default);
    }
}
