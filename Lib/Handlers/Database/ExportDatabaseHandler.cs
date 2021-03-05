using Lib.ChainOfResponsibilityPattern;
using Lib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using System.Threading;
using Lib.Helpers;

namespace Lib.Handlers.Database
{
    public class ExportDatabaseHandler : AbstractRequestHandler, IExportDatabaseHandler
    {
        private readonly ILogger _logger;
        private readonly IDatabaseHelper _helper;

        public ExportDatabaseHandler(ILogger logger, IDatabaseHelper helper)
        {
            _logger = logger;
            _helper = helper;
        }

        public override void Handle(Request request)
        {
            ExportToBacPacFile(request);
            base.Handle(request);
        }

        public void ExportToBacPacFile(Request request, CancellationToken cancellationToken = default)
        {
            var sourceDatabaseName = request.DatabaseName;
            var targetBacPacFilePath = request.BacPacFilePath;
            var connectionString = _helper.GetConnectionString(request);
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
