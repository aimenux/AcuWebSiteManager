using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Web.Administration;
using Request = Lib.Models.Request;

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
            catch (Exception ex)
            {
                LogHelperException(ex);
            }

            return details;
        }

        public string GetSiteConfigXmlFile(string websiteName)
        {
            var site = GetSiteDetails(websiteName);
            var path = site?.VirtualDirectory?.PhysicalPath;
            if (path == null) return null;
            var name = $"{site.SiteFriendlyName}.xml";
            return Path.Combine(path, name);
        }

        public bool IsSiteExists(Request request) => IsVirtualDirectoryExists(request);

        private SiteDetails GetSiteDetails(string websiteName)
        {
            var sites = GetSitesDetails();
            var site = sites.FirstOrDefault(x => IgnoreEquals(x.SiteFriendlyName, websiteName));
            return site;
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

        private static bool IgnoreEquals(string str1, string str2)
        {
            return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
        }

        private void LogHelperException(Exception ex)
        {
            _logger.LogDebug("An error has occurred on {name}: {ex}", GetType().Name, ex);
        }
    }
}