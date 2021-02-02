using System.Diagnostics;
using System.Linq;
using Lib.ChainOfResponsibilityPattern;
using Lib.Models;
using Microsoft.Extensions.Logging;
using SystemProcess = System.Diagnostics.Process;

namespace Lib.Handlers.Process
{
    public class ProcessHandler : AbstractRequestHandler, IProcessHandler
    {
        private readonly ILogger _logger;

        public ProcessHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override void Handle(Request request)
        {
            RunProcess(request.ConfigExeFile, request.ConfigExeArguments);
            base.Handle(request);
        }

        public void RunProcess(string name, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = $@"{name}",
                Arguments = $@"{arguments}"
            };

            var process = new SystemProcess
            {
                StartInfo = startInfo
            };

            process.ErrorDataReceived += (sender, args) => LogProcessError(args.Data);
            process.OutputDataReceived += (sender, args) => LogProcessOutput(args.Data);

            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        private void LogProcessOutput(string message)
        {
            var keywords = new[]
            {
                "Stage:", 
                "Task ended: Users"
            };

            if (string.IsNullOrWhiteSpace(message)) return;
            var logLevel = keywords.Any(message.Contains) ? LogLevel.Information : LogLevel.Trace;
            _logger.Log(logLevel, "An output was received from [{name}] {message}", Name, message);
        }

        private void LogProcessError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _logger.LogError("An error has occurred on [{name}] {message}", Name, message);
        }
    }
}