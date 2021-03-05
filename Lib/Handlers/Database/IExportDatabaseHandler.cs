using Lib.ChainOfResponsibilityPattern;
using System.Threading;
using Lib.Models;

namespace Lib.Handlers.Database
{
    public interface IExportDatabaseHandler : IRequestHandler
    {
        void ExportToBacPacFile(Request request, CancellationToken cancellationToken = default);
    }
}
