using System;
using System.IO;
using Lib.ChainOfResponsibilityPattern;
using Lib.Models;
using Microsoft.Extensions.Logging;

namespace Lib.Handlers.Disk
{
    public class DiskHandler : AbstractRequestHandler, IDiskHandler
    {
        private readonly ILogger _logger;

        public DiskHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override void Handle(Request request)
        {
            RemoveDirectories(request.AbsoluteDirectories);
            base.Handle(request);
        }

        public void RemoveDirectory(string directoryName)
        {
            try
            {
                if (Directory.Exists(directoryName))
                {
                    Directory.Delete(directoryName, true);
                }
                
                LogDirectoryRemoved(directoryName);
            }
            catch (Exception ex)
            {
                LogDirectoryException(ex);
            }
        }

        public void RemoveDirectories(params string[] directoryNames)
        {
            foreach (var directoryName in directoryNames)
            {
                RemoveDirectory(directoryName);
            }
        }

        private void LogDirectoryRemoved(string directoryName)
        {
            _logger.LogInformation("Directory [{directoryName}] was removed", directoryName);
        }

        private void LogDirectoryException(Exception ex)
        {
            _logger.LogError("An error has occurred on [{name}] {ex}", Name, ex);
        }
    }
}