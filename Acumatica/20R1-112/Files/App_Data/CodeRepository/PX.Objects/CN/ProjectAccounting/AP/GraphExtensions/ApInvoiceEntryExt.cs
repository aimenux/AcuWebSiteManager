using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.ProjectAccounting.AP.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor.Attributes.ProjectTaskWithType;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.CN.ProjectAccounting.AP.GraphExtensions
{
    public class ApInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() &&
                !SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
        }

        public void _(Events.FieldDefaulting<APTran.inventoryID> args)
        {
            if (Base.Document.Current?.VendorID != null)
            {
                args.NewValue = GetVendorDefaultInventoryId();
            }
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXRemoveBaseAttribute(typeof(ActiveProjectTaskAttribute))]
        [ActiveProjectTaskWithType(typeof(APTran.projectID))]
        [PXRemoveBaseAttribute(typeof(PXDefaultAttribute))]
        [PXDefault(typeof(Search<PMTask.taskID,
                Where<PMTask.projectID, Equal<Current<APTran.projectID>>,
                    And<PMTask.isDefault, Equal<True>,
                    And<PMTask.type, NotEqual<ProjectTaskType.revenue>,
                    And<PMTask.status, Equal<ProjectTaskStatus.active>>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [ProjectTaskTypeValidation(
            ProjectTaskIdField = typeof(APTran.taskID),
            Message = ProjectAccountingMessages.CostTaskTypeIsNotValid,
            WrongProjectTaskType = ProjectTaskType.Revenue)]
        protected virtual void APTran_TaskID_CacheAttached(PXCache cache)
        {
        }

        [PXMergeAttributes(Method = MergeMethod.Merge)]
        [PXDefault(typeof(Search<PMCostCode.costCodeID,
                Where<PMCostCode.costCodeID, Equal<Current<VendorExt.vendorDefaultCostCodeId>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        protected virtual void APTran_CostCodeID_CacheAttached(PXCache cache)
        {
        }

        private int? GetVendorDefaultInventoryId()
        {
            var vendor = GetVendor();
            return vendor == null
                ? null
                : PXCache<Vendor>.GetExtension<VendorExt>(vendor).VendorDefaultInventoryId;
        }

        private Vendor GetVendor()
        {
            return new PXSelect<Vendor,
                    Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>(Base)
                .Select(Base.Document.Current.VendorID);
        }
    }
}