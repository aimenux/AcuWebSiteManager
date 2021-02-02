using System;
using System.Collections.Generic;
using System.Linq;
using Lib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Web.Administration;
using Request = Lib.Models.Request;
using SmoDatabase = Microsoft.SqlServer.Management.Smo.Database;

namespace Lib.Helpers
{
    public class SiteHelper : ISiteHelper
    {
        private readonly ILogger _logger;

        public SiteHelper(ILogger logger)
        {
            _logger = logger;
        }

        public ICollection<SiteDetails> GetSitesDetails()
        {
            var details = new List<SiteDetails>();

            try
            {
                using (var serverManager = new ServerManager())
                {
                    var sites = serverManager.Sites;
                    details.AddRange(from site in sites
                                   from app in site.Applications
                                   from virDir in app.VirtualDirectories
                                   orderby site.Name, app.ApplicationPoolName
                                   where !virDir.PhysicalPath.Contains("%")
                                   select new SiteDetails(site, app, virDir));
                }
            }
            catch(Exception ex)
            {
                LogHelperException(ex);
            }

            return details;
        }

        public bool IsSiteExists(Request request) => IsDatabaseExists(request) && IsVirtualDirectoryExists(request);

        private static bool IsDatabaseExists(Request request)
        {
            try
            {
                var server = new Server(request.ServerName);
                var _ = new SmoDatabase(server, request.DatabaseName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsVirtualDirectoryExists(Request request)
        {
            try
            {
                using (var serverManager = new ServerManager())
                {
                    var appPool = serverManager.ApplicationPools[request.AppPoolName];
                    if (appPool == null) return false;

                    var site = serverManager.Sites["Default Web Site"];
                    var application = site.Applications[$"/{request.SiteVirtualDirectoryName}"];
                    return application != null;
                }
            }
            catch
            {
                return false;
            }
        }

        private void LogHelperException(Exception ex)
        {
            _logger.LogDebug("An error has occurred on {name}: {ex}", GetType().Name, ex);
        }
    }
}