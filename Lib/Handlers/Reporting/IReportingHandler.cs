using Lib.ChainOfResponsibilityPattern;
using Lib.Models;

namespace Lib.Handlers.Reporting
{
    public interface IReportingHandler : IRequestHandler
    {
        void DisplayReporting(Request request);
    }
}
