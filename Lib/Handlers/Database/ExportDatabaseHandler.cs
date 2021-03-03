using Lib.ChainOfResponsibilityPattern;
using Lib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using System.Threading;

namespace Lib.Handlers.Database
{
    public class ExportDatabaseHandler : AbstractRequestHandler, IExportDatabaseHandler
    {
        private readonly ILogger _logger;

        public ExportDatabaseHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override void Handle(Request request)
        {
            ExportToBacPacFile(request.ServerName, request.DatabaseName, request.BacPacFilePath);
            base.Handle(request);
        }

        public void ExportToBacPacFile(string sourceServerName, string sourceDatabaseName, string targetBacPacFilePath, CancellationToken cancellationToken = default)
        {
            var connectionString = $@"Data Source={sourceServerName};Integrated Security=True";
            var services = new DacServices(connectionString);
            services.Message += (_, e) => LogMessage(e.Message?.Message);
            services.ExportBacpac(targetBacPacFilePath, sourceDatabaseName, cancellationToken: cancellationToken);
        }

        private void LogMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _logger.LogInformation("An output was received from [{name}] {message}", Name, TrimMessage(message));
        }

        private static string TrimMessage(string message)
        {
            return message?.Trim(' ', '\n', '\r', '\t');
        }
    }
}
