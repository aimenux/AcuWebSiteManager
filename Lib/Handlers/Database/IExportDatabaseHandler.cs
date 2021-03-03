using Lib.ChainOfResponsibilityPattern;
using System.Threading;

namespace Lib.Handlers.Database
{
    public interface IExportDatabaseHandler : IRequestHandler
    {
        void ExportToBacPacFile(string sourceServerName, string sourceDatabaseName, string targetBacPacFilePath, CancellationToken cancellationToken = default);
    }
}
