using System;
using System.ComponentModel.DataAnnotations;
using Lib.Builders;
using Lib.ChainOfResponsibilityPattern;
using Lib.Handlers.Database;
using Lib.Handlers.Disk;
using Lib.Handlers.WebServer;
using Lib.Models;
using Lib.Validators;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace App.Commands
{
    [Command(Name = "DeleteSite", FullName = "Delete Acumatica Site", Description = "Delete Acumatica Site (database, files).")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [HelpOption]
    public class DeleteSiteCommand
    {
        private readonly IRequestBuilder _builder;
        private readonly IDeleteSiteValidator _validator;
        private readonly IWebServerHandler _webServerHandler;
        private readonly IDatabaseHandler _databaseHandler;
        private readonly IDiskHandler _diskHandler;
        private readonly ILogger _logger;

        public DeleteSiteCommand(
            IRequestBuilder builder,
            IDeleteSiteValidator validator,
            IWebServerHandler webServerHandler,
            IDatabaseHandler databaseHandler,
            IDiskHandler diskHandler,
            ILogger logger)
        {
            _builder = builder;
            _validator = validator;
            _webServerHandler = webServerHandler;
            _databaseHandler = databaseHandler;
            _diskHandler = diskHandler;
            _logger = logger;
        }

        [Required]
        [Option("-x|--xml", "Xml config file.", CommandOptionType.SingleValue)]
        public string ConfigXmlFile { get; set; }

        [Option("-e|--exe", "Ace exe file.", CommandOptionType.SingleValue)]
        public string ConfigExeFile { get; set; }

        public void OnExecute(CommandLineApplication app)
        {
            if (string.IsNullOrWhiteSpace(ConfigXmlFile))
            {
                app.ShowHelp();
                return;
            }

            var parameters = new Parameters
            {
                XmlExeFile = ConfigExeFile,
                XmlConfigFile = ConfigXmlFile
            };

            var request = _builder.Build(parameters);
            var result = _validator.Validate(request);
            if (!result.IsValid)
            {
                _logger.LogValidationFailures(result);
                return;
            }

            var chain = new RequestHandlerChain(
                _webServerHandler, 
                _databaseHandler, 
                _diskHandler);
            var elapsed = chain.Handle(request);

            LogCommandInfo(request, elapsed);
        }

        private void LogCommandInfo(Request request, TimeSpan elapsed)
        {
            _logger.LogInformation("WebSite [{site}] was deleted: elapsed time [{elapsed}]", request.SiteDirectoryName, $"{elapsed:g}");
        }

        private static string GetVersion() => typeof(DeleteSiteCommand).GetVersion();
    }
}
