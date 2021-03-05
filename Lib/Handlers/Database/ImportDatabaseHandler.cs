using Lib.ChainOfResponsibilityPattern;
using Lib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using System.Threading;
using Lib.Helpers;

namespace Lib.Handlers.Database
{
    public class ImportDatabaseHandler : AbstractRequestHandler, IImportDatabaseHandler
    {
        private readonly ILogger _logger;
        private readonly IDatabaseHelper _helper;

        public ImportDatabaseHandler(ILogger logger, IDatabaseHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public override void Handle(Request request)
        {
            ImportBacPacFile(request);
            base.Handle(request);
        }

        public void ImportBacPacFile(Request request, CancellationToken cancellationToken = default)
        {
            var targetDatabaseName = request.DatabaseName;
            var sourceBacPacFilePath = request.BacPacFilePath;
            var connectionString = _helper.GetConnectionString(request);
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
