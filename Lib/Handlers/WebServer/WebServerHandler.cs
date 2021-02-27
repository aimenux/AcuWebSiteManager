using System;
using System.Linq;
using Dapper;
using Lib.ChainOfResponsibilityPattern;
using Lib.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Web.Administration;
using Request = Lib.Models.Request;

namespace Lib.Handlers.WebServer
{
    public class WebServerHandler : AbstractRequestHandler, IWebServerHandler
    {
        private readonly IDatabaseHelper _databaseHelper;
        private readonly ILogger _logger;

        public WebServerHandler(IDatabaseHelper databaseHelper, ILogger logger)
        {
            _databaseHelper = databaseHelper;
            _logger = logger;
        }

        public override void Handle(Request request)
        {
            RemoveSite(request);
            RemoveApplicationPool(request);
            base.Handle(request);
        }

        public void RemoveSite(Request request)
        {
            var appPoolName = request.AppPoolName;
            var siteVirtualDirectoryName = request.SiteVirtualDirectoryName;

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

        public void RemoveApplicationPool(Request request)
        {
            var appPoolName = request.AppPoolName;

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

                DropAppPoolLogin(request);

                LogProcessInfo($"Application Pool [{appPoolName}] was removed");
            }
        }

        public void DropAppPoolLogin(Request request)
        {
            try
            {
                var sqlDropLogin = GetSqlDropLogin(request);
                var connectionString = GetConnectionString(request);
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Execute(sqlDropLogin, new { ApplicationPoolName = request.AppPoolName });
                }
            }
            catch (Exception ex)
            {
                LogProcessException(ex);
            }
        }

        private string GetConnectionString(Request request)
        {
            const string databaseName = @"master";
            var serverName = request.ServerName;
            return _databaseHelper.GetConnectionString(serverName, databaseName);
        }

        private static string GetSqlDropLogin(Request request)
        {
            var appPoolName = request.AppPoolName;
            return $@"DROP LOGIN [IIS APPPOOL\{appPoolName}]";
        }

        private void LogProcessInfo(string message)
        {
            _logger.LogInformation(message);
        }

        private void LogProcessWarning(string message)
        {
            _logger.LogWarning(message);
        }

        private void LogProcessException(Exception ex)
        {
            _logger.LogError("An error has occurred on [{name}] {ex}", Name, ex);
        }

        private static bool IsDefaultApplicationPool(string appPoolName) => string.Equals(appPoolName, @"DefaultAppPool", StringComparison.OrdinalIgnoreCase);
    }
}