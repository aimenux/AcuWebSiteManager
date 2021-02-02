using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.FS
{
    public class ServiceOrderEntryVisibilityRestriction : PXGraphExtension<ServiceOrderEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.visibilityRestriction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXRemoveBaseAttribute(typeof(RestrictCustomerByUserBranches))]
        public void _(Events.CacheAttached<FSServiceOrder.customerID> e)
        {
        }

        public delegate void CopyPasteGetScriptDelegate(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers);

        [PXOverride]
        public void CopyPasteGetScript(bool isImportSimple, List<Api.Models.Command> script, List<Api.Models.Container> containers,
            CopyPasteGetScriptDelegate baseMethod)
        {
            // We need to process fields together that are related to the Branch and Customer for proper validation. For this:
            // 1) set the right order of the fields
            // 2) insert dependent fields after the BranchID field
            // 3) all fields must belong to the same view.

            string branchViewName = nameof(ServiceOrderEntry.CurrentServiceOrder) + ": 1";
            string customerViewName = nameof(ServiceOrderEntry.ServiceOrderRecords);

            (string name, string viewName) branch = (nameof(FSServiceOrder.BranchID), branchViewName);

            List<(string name, string viewName)> fieldList = new List<(string name, string viewName)>();
            fieldList.Add((nameof(FSServiceOrder.CustomerID), customerViewName));
            fieldList.Add((nameof(FSServiceOrder.LocationID), customerViewName));
            fieldList.Add((nameof(FSServiceOrder.BranchLocationID), customerViewName));
            fieldList.Add((nameof(FSServiceOrder.BillCustomerID), branchViewName));
            fieldList.Add((nameof(FSServiceOrder.BillLocationID), branchViewName));

            Common.Utilities.SetDependentFieldsAfterBranch(script, branch, fieldList);
        }
    }
}
