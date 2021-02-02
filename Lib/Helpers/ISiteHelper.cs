using System.Collections.Generic;
using Lib.Models;

namespace Lib.Helpers
{
    public interface ISiteHelper
    {
        bool IsSiteExists(Request request);
        ICollection<SiteDetails> GetSitesDetails();
    }
}