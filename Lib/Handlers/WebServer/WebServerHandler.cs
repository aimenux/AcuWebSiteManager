using System;
using System.Linq;
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
            RemoveApplicationPool(request.AppPoolName);
            base.Handle(request);
        }

        public void RemoveSite(string appPoolName, string siteVirtualDirectoryName)
        {
            using (var serverManager = new ServerManager())
            {
                var appPool = serverManager.ApplicationPools[appPoolName];
                if (appPool == null)
                {
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

        public void RemoveApplicationPool(string appPoolName)
        {
            using (var serverManager = new ServerManager())
            {
                var appPool = serverManager.ApplicationPools[appPoolName];
                if (appPool == null || IsDefaultApplicationPool(appPoolName))
                {
                    return;
                }

                var applications = serverManager.Sites
                    .SelectMany(site => site.Applications)
                    .Where(app => app.ApplicationPoolName.Equals(appPoolName))
                    .ToList();

                if (applications.Any())
                {
                    LogProcessWarning($"Application pool [{appPoolName}] [{applications.Count} application(s)] removing is skipped");
                    return;
                }

                var appPoolCollection = serverManager.ApplicationPools;
                appPoolCollection.Remove(appPool);
                serverManager.CommitChanges();
                LogProcessInfo($"Application Pool [{appPoolName}] was removed");
            }
        }

        private void LogProcessInfo(string message)
        {
            _logger.LogInformation(message);
        }

        private void LogProcessWarning(string message)
        {
            _logger.LogWarning(message);
        }

        private static bool IsDefaultApplicationPool(string appPoolName) => !string.Equals(appPoolName, @"DefaultAppPool", StringComparison.OrdinalIgnoreCase);
    }
}