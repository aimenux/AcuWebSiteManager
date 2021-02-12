using Lib.Models;
using Microsoft.Extensions.Logging;

namespace Lib.Handlers.Process
{
    public class CreateSiteProcessHandler : AbstractProcessHandler, ICreateSiteProcessHandler
    {
        public CreateSiteProcessHandler(ILogger logger) : base(logger)
        {
        }

        public override void Handle(Request request)
        {
            RunProcess(request.ConfigExeFile, request.ConfigExeArguments);
            base.Handle(request);
        }

        protected override void LogProcessOutput(string message, params string[] _)
        {
            var keywords = new[]
            {
                "Stage:", 
                "Task ended: Users"
            };

            base.LogProcessOutput(message, keywords);
        }
    }
}