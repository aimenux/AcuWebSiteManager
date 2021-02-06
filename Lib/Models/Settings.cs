using System.Collections.Generic;

namespace Lib.Models
{
    public class Settings
    {
        public const string ApplicationName = "AcuWebSiteManager";

        public const string ApplicationPassword = "Acumatica";

        public ICollection<int> Tenants { get; set; }
    }
}
