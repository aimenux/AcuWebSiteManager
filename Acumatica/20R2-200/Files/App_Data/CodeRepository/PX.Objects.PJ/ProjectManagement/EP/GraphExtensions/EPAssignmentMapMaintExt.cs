using System.Collections.Generic;
using System.Linq;
using PX.Objects.PJ.Common.Descriptor;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.PJ.ProjectManagement.EP.GraphExtensions
{
    public class EpAssignmentMapMaintExt : PXGraphExtension<EPAssignmentMapMaint>
    {
        public delegate IEnumerable<string> GetEntityTypeScreensDelegate();

        private static IEnumerable<string> Screens =>
            new List<string>
            {
                ScreenIds.ProjectIssue,
                ScreenIds.RequestForInformation
            };

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        [PXOverride]
        public IEnumerable<string> GetEntityTypeScreens(GetEntityTypeScreensDelegate baseMethod)
        {
            return baseMethod().Concat(Screens);
        }
    }
}