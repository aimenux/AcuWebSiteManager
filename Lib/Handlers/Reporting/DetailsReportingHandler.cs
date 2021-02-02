using Lib.ChainOfResponsibilityPattern;
using Lib.Helpers;
using Lib.Models;

namespace Lib.Handlers.Reporting
{
    public class DetailsReportingHandler : AbstractRequestHandler, IDetailsReportingHandler
    {
        private readonly ISiteHelper _siteHelper;
        private readonly IConsoleHelper _consoleHelper;

        public DetailsReportingHandler(ISiteHelper siteHelper, IConsoleHelper consoleHelper)
        {
            _siteHelper = siteHelper;
            _consoleHelper = consoleHelper;
        }

        public override void Handle(Request request)
        {
            DisplayDetailsReporting();
            base.Handle(request);
        }

        public void DisplayDetailsReporting()
        {
            var details = _siteHelper.GetSitesDetails();
            _consoleHelper.RenderTable(details);
        }
    }
}