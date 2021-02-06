using System;
using Lib.ChainOfResponsibilityPattern;
using Lib.Handlers.Reporting;
using Lib.Models;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace App.Commands
{
    [Command(Name = "ListSites", FullName = "List iis sites", Description = "List current iis sites.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [HelpOption]
    public class ListSitesCommand
    {
        private readonly IDetailsReportingHandler _reportingHandler;
        private readonly ILogger _logger;

        public ListSitesCommand(IDetailsReportingHandler reportingHandler, ILogger logger)
        {
            _reportingHandler = reportingHandler;
            _logger = logger;
        }

        public void OnExecute(CommandLineApplication _)
        {
            const Request request = null;
            var chain = new RequestHandlerChain(_reportingHandler);
            var elapsed = chain.Handle(request);

            LogCommandInfo(elapsed);
        }
            
        private void LogCommandInfo(TimeSpan elapsed)
        {
            _logger.LogInformation("Listing WebSites: elapsed time [{elapsed}]", $"{elapsed:g}");
        }

        private static string GetVersion() => typeof(ListSitesCommand).GetVersion();
    }
}
