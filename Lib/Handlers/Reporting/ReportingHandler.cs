using System.IO;
using Lib.ChainOfResponsibilityPattern;
using Lib.Helpers;
using Lib.Models;

namespace Lib.Handlers.Reporting
{
    public class ReportingHandler : AbstractRequestHandler, IReportingHandler
    {
        private readonly IConsoleHelper _consoleHelper;

        public ReportingHandler(IConsoleHelper consoleHelper)
        {
            _consoleHelper = consoleHelper;
        }

        public override void Handle(Request request)
        {
            DisplayReporting(request);
            base.Handle(request);
        }

        public void DisplayReporting(Request request)
        {
            if (Directory.Exists(request.SiteDirectoryPath))
            {
                _consoleHelper.RenderTable(request);
            }
        }
    }
}