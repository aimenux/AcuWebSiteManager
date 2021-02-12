using System.Diagnostics;
using System.Linq;
using Lib.ChainOfResponsibilityPattern;
using Microsoft.Extensions.Logging;
using SystemProcess = System.Diagnostics.Process;

namespace Lib.Handlers.Process
{
    public abstract class AbstractProcessHandler : AbstractRequestHandler, IProcessHandler
    {
        private readonly ILogger _logger;

        protected AbstractProcessHandler(ILogger logger)
        {
            _logger = logger;
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

        protected virtual void LogProcessOutput(string message, params string[] keywords)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var logLevel = GetLogLevel(message, keywords);
            _logger.Log(logLevel, "An output was received from [{name}] {message}", Name, message);
        }

        protected virtual void LogProcessError(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            _logger.LogError("An error has occurred on [{name}] {message}", Name, message);
        }

        private static LogLevel GetLogLevel(string message, string[] keywords)
        {
            if (keywords == null || !keywords.Any()) return LogLevel.Information;
            return keywords.Any(message.Contains) ? LogLevel.Information : LogLevel.Trace;
        }
    }
}