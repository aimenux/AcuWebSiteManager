using PX.Data;
using PX.Objects.EP;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.AM.GraphExtensions
{
    public class EPApprovalMapMaintAMExtension : PXGraphExtension<EPApprovalMapMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturingECC>();
        }

        [PXOverride]
        public virtual IEnumerable<string> GetEntityTypeScreens(Func<IEnumerable<string>> method)
        {
            var list = method?.Invoke().ToList();
            if (list != null)
            {
                list.Add("AM210000");
                list.Add("AM215000");
            }
            return list;
        }
    }
}
