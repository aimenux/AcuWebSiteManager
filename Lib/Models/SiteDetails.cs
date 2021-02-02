using System.IO;
using Microsoft.Web.Administration;

namespace Lib.Models
{
    public class SiteDetails
    {
        public Site Site { get; set; }
        public Application Application { get; set; }
        public VirtualDirectory VirtualDirectory { get; set; }

        public string SiteFriendlyName => Path.GetFileName(Path.GetDirectoryName(VirtualDirectory.PhysicalPath));

        public SiteDetails()
        {
        }

        public SiteDetails(Site site, Application application, VirtualDirectory virtualDirectory)
        {
            Site = site;
            Application = application;
            VirtualDirectory = virtualDirectory;
        }
    }
}