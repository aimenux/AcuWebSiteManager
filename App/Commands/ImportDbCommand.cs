using Lib.ChainOfResponsibilityPattern;
using Lib.Handlers.Database;
using Lib.Models;
using Lib.Validators;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;

namespace App.Commands
{
    [Command(Name = "ImportDb", FullName = "Import bacpac file", Description = "Import bacpac file to database.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [HelpOption]
    public class ImportDbCommand
    {
        private readonly IImportDbValidator _validator;
        private readonly IImportDatabaseHandler _handler;
        private readonly ILogger _logger;

        public ImportDbCommand(IImportDbValidator validator, IImportDatabaseHandler handler, ILogger logger)
        {
            _validator = validator;
            _handler = handler;
            _logger = logger;
        }

        [Required]
        [Option("-d|--db", "Database target name.", CommandOptionType.SingleValue)]
        public string DatabaseName { get; set; }

        [Required]
        [Option("-s|--server", "Server source name.", CommandOptionType.SingleValue)]
        public string ServerName { get; set; }

        [Required]
        [Option("-f|--file", "BacPac file path source.", CommandOptionType.SingleValue)]
        public string BacPacFilePath { get; set; }

        public void OnExecute(CommandLineApplication app)
        {
            var request = new Request
            {
                ServerName = ServerName,
                DatabaseName = DatabaseName,
                BacPacFilePath = BacPacFilePath
            };

            var result = _validator.Validate(request);
            if (!result.IsValid)
            {
                _logger.LogValidationFailures(result);
                return;
            }

            var chain = new RequestHandlerChain(_handler);
            var elapsed = chain.Handle(request);

            LogCommandInfo(request, elapsed);
        }

        private void LogCommandInfo(Request request, TimeSpan elapsed)
        {
            _logger.LogInformation("Database [{db}] was exported to bacpac file [{file}]: elapsed time [{elapsed}]",
                request.DatabaseName,
                request.BacPacFilePath,
                $"{elapsed:g}");
        }

        private static string GetVersion() => typeof(ExportDbCommand).GetVersion();
    }
}
