using Lib.ChainOfResponsibilityPattern;
using System.Threading;
using Lib.Models;

namespace Lib.Handlers.Database
{
    public interface IImportDatabaseHandler : IRequestHandler
    {
        void ImportBacPacFile(Request request, CancellationToken cancellationToken = default);
    }
}
