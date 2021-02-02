using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.PM;

namespace PX.Objects.AM
{
    public static class ProjectHelper
    {
        /// <summary>
        /// Is the project module enabled
        /// </summary>
        /// <returns></returns>
        public static bool IsProjectFeatureEnabled()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
        }

        /// <summary>
        /// Does the account contain an account group (a project GL Account).
        /// </summary>
        public static bool IsProjectAcct(PXGraph graph, int? accountID)
        {
            if (accountID == null)
            {
                return false;
            }

            Account acct = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(graph, accountID);
            return acct?.AccountGroupID != null;
        }

        /// <summary>
        /// Does the account contain an account group (a project GL Account).
        /// Only query the account when the project module is enabled.
        /// </summary>
        public static bool IsProjectAcctWithProjectEnabled(PXGraph graph, int? accountID)
        {
            return IsProjectFeatureEnabled() && IsProjectAcct(graph, accountID);
        }

        internal static bool IsPMVisible(PXGraph graph)
        {
            if (graph != null && IsProjectFeatureEnabled() && !Common.IsPortal && PXAccess.FeatureInstalled<CS.FeaturesSet.manufacturing>())
            {
                PMSetup setup = PXSelect<PMSetup>.Select(graph);
                var ext = setup?.GetExtension<PMSetupExt>();
                return ext?.VisibleInPROD == true;
            }
            
            return false;
        }
    }
}
