using System;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.ProjectManagement.EP.GraphExtensions
{
    public class EpApprovalMapMaintExt : PXGraphExtension<EPApprovalMapMaint>
    {
        private static IEnumerable<string> Screens =>
            new List<string>
            {
                ScreenIds.DailyFieldReport
            };

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        [PXOverride]
        public IEnumerable<string> GetEntityTypeScreens(Func<IEnumerable<string>> baseMethod)
        {
            return baseMethod().Concat(Screens);
        }
    }
}