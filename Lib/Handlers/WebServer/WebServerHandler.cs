using Lib.ChainOfResponsibilityPattern;
using Microsoft.Extensions.Logging;
using Microsoft.Web.Administration;
using Request = Lib.Models.Request;

namespace Lib.Handlers.WebServer
{
    public class WebServerHandler : AbstractRequestHandler, IWebServerHandler
    {
        private readonly ILogger _logger;

        public WebServerHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override void Handle(Request request)
        {
            RemoveSite(request.AppPoolName, request.SiteVirtualDirectoryName);
            base.Handle(request);
        }

        public void RemoveSite(string appPoolName, string siteVirtualDirectoryName)
        {
            using (var serverManager = new ServerManager())
            {
                var appPool = serverManager.ApplicationPools[appPoolName];
                if (appPool == null)
                {
                    LogProcessError($"AppPool {appPoolName} is not found");
                    return;
                }

                if (appPool.State != ObjectState.Stopped)
                {
                    appPool.Recycle();
                    appPool.Start();
                }
		
                var site = serverManager.Sites["Default Web Site"];
                var application = site.Applications[$"/{siteVirtualDirectoryName}"];
                if (application != null)
                {
                    site.Applications.Remove(application);
                    serverManager.CommitChanges();
                }

                LogProcessInfo($"Virtual Site [{siteVirtualDirectoryName}] was removed");
            }
        }

        private void LogProcessInfo(string message)
        {
            _logger.LogInformation(message);
        }

        private void LogProcessError(string message)
        {
            _logger.LogError("An error has occurred on [{name}] {message}", Name, message);
        }
    }
}