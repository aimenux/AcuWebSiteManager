using System;
using System.ComponentModel.DataAnnotations;
using Lib.Builders;
using Lib.ChainOfResponsibilityPattern;
using Lib.Handlers.Site;
using Lib.Models;
using Lib.Validators;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace App.Commands
{
    [Command(Name = "SwitchDb", FullName = "Switch site database", Description = "Switch site to use another database.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [HelpOption]
    public class SwitchDbCommand
    {
        private readonly IRequestBuilder _builder;
        private readonly ISwitchDbValidator _validator;
        private readonly ISiteHandler _siteHandler;
        private readonly ILogger _logger;

        public SwitchDbCommand(IRequestBuilder builder, ISwitchDbValidator validator, ISiteHandler siteHandler, ILogger logger)
        {
            _builder = builder;
            _validator = validator;
            _siteHandler = siteHandler;
            _logger = logger;
        }

        [Option("-x|--xml", "Xml config file.", CommandOptionType.SingleValue)]
        public string ConfigXmlFile { get; set; }

        [Required]
        [Option("-d|--db", "Database target name.", CommandOptionType.SingleValue)]
        public string DatabaseName { get; set; }

        [Option("-s|--site", "WebSite source name.", CommandOptionType.SingleValue)]
        public string WebSiteName { get; set; }

        public void OnExecute(CommandLineApplication app)
        {
            if (ShowHelp())
            {
                app.ShowHelp();

                if (AreOptionalOptionsUndefined())
                {
                    LogOptionalOptionsUndefined();
                }
                
                return;
            }

            var parameters = new Parameters
            {
                WebSiteName = WebSiteName,
                XmlConfigFile = ConfigXmlFile,
                SwitchDatabaseName = DatabaseName
            };

            var request = _builder.Build(parameters);

            var result = _validator.Validate(request);
            if (!result.IsValid)
            {
                _logger.LogValidationFailures(result);
                return;
            }

            var chain = new RequestHandlerChain(_siteHandler);
            var elapsed = chain.Handle(request);

            LogCommandInfo(request, elapsed);
        }

        private bool ShowHelp()
        {
            return string.IsNullOrWhiteSpace(DatabaseName) || AreOptionalOptionsUndefined();
        }

        private bool AreOptionalOptionsUndefined()
        {
            return string.IsNullOrWhiteSpace(ConfigXmlFile) && string.IsNullOrWhiteSpace(WebSiteName);
        }

        private void LogCommandInfo(Request request, TimeSpan elapsed)
        {
            _logger.LogInformation("WebSite [{site}] was switched to database [{db}]: elapsed time [{elapsed}]",
                request.SiteDirectoryName,
                request.SwitchDatabaseName,
                $"{elapsed:g}");
        }

        private void LogOptionalOptionsUndefined()
        {
            _logger.LogError("Either define website name [-s option] or xml config file [-x option]");
        }

        private static string GetVersion() => typeof(SwitchDbCommand).GetVersion();
    }
}