using Lib.ChainOfResponsibilityPattern;

namespace Lib.Handlers.Reporting
{
    public interface IDetailsReportingHandler : IRequestHandler
    {
        void DisplayDetailsReporting();
    }
}