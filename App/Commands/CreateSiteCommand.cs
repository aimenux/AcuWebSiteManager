using System;
using System.ComponentModel.DataAnnotations;
using Lib.Builders;
using Lib.ChainOfResponsibilityPattern;
using Lib.Handlers.Password;
using Lib.Handlers.Process;
using Lib.Handlers.Reporting;
using Lib.Handlers.Serialization;
using Lib.Models;
using Lib.Validators;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;

namespace App.Commands
{
    [Command(Name = "CreateSite", FullName = "Create Acumatica Site", Description = "Create Acumatica Site (database, files).")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [HelpOption]
    public class CreateSiteCommand
    {
        private readonly IRequestBuilder _builder;
        private readonly ICreateSiteValidator _validator;
        private readonly IProcessHandler _processHandler;
        private readonly IPasswordHandler _passwordHandler;
        private readonly IReportingHandler _reportingHandler;
        private readonly ISerializationHandler _serializationHandler;
        private readonly ILogger _logger;

        public CreateSiteCommand(
            IRequestBuilder builder,
            ICreateSiteValidator validator,
            IProcessHandler processHandler,
            IPasswordHandler passwordHandler,
            IReportingHandler reportingHandler,
            ISerializationHandler serializationHandler,
            ILogger logger)
        {
            _builder = builder;
            _validator = validator;
            _processHandler = processHandler;
            _passwordHandler = passwordHandler;
            _reportingHandler = reportingHandler;
            _serializationHandler = serializationHandler;
            _logger = logger;
        }

        [Required]
        [Option("-x|--xml", "Config xml file.", CommandOptionType.SingleValue)]
        public string ConfigXmlFile { get; set; }

        [Option("-e|--exe", "Config exe file.", CommandOptionType.SingleValue)]
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
                _processHandler,
                _passwordHandler,
                _reportingHandler,
                _serializationHandler);
            var elapsed = chain.Handle(request);

            LogCommandInfo(request, elapsed);
        }

        private void LogCommandInfo(Request request, TimeSpan elapsed)
        {
            _logger.LogInformation("WebSite [{site}] was created: elapsed time [{elapsed}]", request.SiteDirectoryName, $"{elapsed:g}");
        }

        private static string GetVersion() => typeof(CreateSiteCommand).GetVersion();
    }
}
