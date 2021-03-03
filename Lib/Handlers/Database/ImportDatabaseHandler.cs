using Lib.ChainOfResponsibilityPattern;
using Lib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using System.Threading;

namespace Lib.Handlers.Database
{
    public class ImportDatabaseHandler : AbstractRequestHandler, IImportDatabaseHandler
    {
        private readonly ILogger _logger;

        public ImportDatabaseHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override void Handle(Request request)
        {
            ImportBacPacFile(request.BacPacFilePath, request.ServerName, request.DatabaseName);
            base.Handle(request);
        }

        public void ImportBacPacFile(string sourceBacPacFilePath, string sourceServerName, string targetDatabaseName, CancellationToken cancellationToken = default)
        {
            var connectionString = $@"Data Source={sourceServerName};Integrated Security=True";
            var services = new DacServices(connectionString);
            services.Message += (_, e) => LogMessage(e.Message?.Message);
            using (var sourcePackage = BacPackage.Load(sourceBacPacFilePath, DacSchemaModelStorageType.Memory))
            {
                services.ImportBacpac(sourcePackage, targetDatabaseName, cancellationToken);
            }
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
