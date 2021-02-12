using System;
using Lib.ChainOfResponsibilityPattern;
using Lib.Handlers.Process;
using Lib.Models;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace App.Commands
{
    [Command(Name = "AutoUpdate", FullName = "Update AcuWebSiteManager", Description = "Auto Update AcuWebSiteManager.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [HelpOption]
    public class AutoUpdateCommand
    {
        private readonly ILogger _logger;
        private readonly IAutoUpdateProcessHandler _processHandler;

        public AutoUpdateCommand(IAutoUpdateProcessHandler processHandler, ILogger logger)
        {
            _logger = logger;
            _processHandler = processHandler;
        }

        public void OnExecute(CommandLineApplication _)
        {
            const Request request = null;
            var chain = new RequestHandlerChain(_processHandler);
            var elapsed = chain.Handle(request);

            LogCommandInfo(elapsed);
        }
            
        private void LogCommandInfo(TimeSpan elapsed)
        {
            _logger.LogInformation("AutoUpdate {tool}: elapsed time [{elapsed}]", Settings.ApplicationName, $"{elapsed:g}");
        }

        private static string GetVersion() => typeof(AutoUpdateCommand).GetVersion();
    }
}
