using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.AP.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;

namespace PX.Objects.CN.ProjectAccounting.PO.GraphExtensions
{
    public class PoOrderEntryExt : PXGraphExtension<POOrderEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public void _(Events.FieldDefaulting<POLine.inventoryID> args)
        {
            if (Base.Document.Current?.VendorID != null)
            {
                args.NewValue = GetVendorDefaultInventoryId();
            }
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
            ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
        [PXFormula(typeof(Validate<POLine.projectID, POLine.costCodeID, POLine.inventoryID, POLine.siteID>))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
            Where<PMTask.projectID, Equal<Current<POLine.projectID>>,
                And<PMTask.isDefault, Equal<True>,
                And<PMTask.type, NotEqual<ProjectTaskType.revenue>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void POLine_TaskID_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(typeof(Search<PMCostCode.costCodeID,
            Where<PMCostCode.costCodeID, Equal<Current<VendorExt.vendorDefaultCostCodeId>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void POLine_CostCodeID_CacheAttached(PXCache cache)
        {
        }

        private int? GetVendorDefaultInventoryId()
        {
            var vendor = GetVendor();
            return vendor.GetExtension<VendorExt>().VendorDefaultInventoryId;
        }

        private Vendor GetVendor()
        {
            return new PXSelect<Vendor,
                    Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(Base)
                .Select(Base.Document.Current.VendorID);
        }
    }
}