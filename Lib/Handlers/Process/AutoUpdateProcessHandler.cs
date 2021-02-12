using Lib.Models;
using Microsoft.Extensions.Logging;

namespace Lib.Handlers.Process
{
    public class AutoUpdateProcessHandler : AbstractProcessHandler, IAutoUpdateProcessHandler
    {
        public AutoUpdateProcessHandler(ILogger logger) : base(logger)
        {
        }

        public override void Handle(Request request)
        {
            const string name = @"dotnet";
            var arguments = $"tool update -g {Settings.ApplicationName} --ignore-failed-sources";
            RunProcess(name, arguments);
            base.Handle(request);
        }
    }
}