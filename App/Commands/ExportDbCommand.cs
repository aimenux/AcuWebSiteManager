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
    [Command(Name = "ExportDb", FullName = "Export database", Description = "Export database to bacpac file.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    [HelpOption]
    public class ExportDbCommand
    {
        private readonly IExportDbValidator _validator;
        private readonly IExportDatabaseHandler _handler;
        private readonly ILogger _logger;

        public ExportDbCommand(IExportDbValidator validator, IExportDatabaseHandler handler, ILogger logger)
        {
            _validator = validator;
            _handler = handler;
            _logger = logger;
        }

        [Required]
        [Option("-d|--db", "Database source name.", CommandOptionType.SingleValue)]
        public string DatabaseName { get; set; }

        [Required]
        [Option("-s|--server", "Server source name.", CommandOptionType.SingleValue)]
        public string ServerName { get; set; }

        [Required]
        [Option("-f|--file", "BacPac file path target.", CommandOptionType.SingleValue)]
        public string BacPacFilePath { get; set; }

        [Option("-u|--user", "Database user name.", CommandOptionType.SingleValue)]
        public string DatabaseUserName { get; set; }

        [Option("-p|--password", "Database user password.", CommandOptionType.SingleValue)]
        public string DatabasePassword { get; set; }

        public void OnExecute(CommandLineApplication _)
        {
            var request = new Request
            {
                ServerName = ServerName,
                DatabaseName = DatabaseName,
                BacPacFilePath = BacPacFilePath,
                DatabaseUserName = DatabaseUserName,
                DatabasePassword = DatabasePassword
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
