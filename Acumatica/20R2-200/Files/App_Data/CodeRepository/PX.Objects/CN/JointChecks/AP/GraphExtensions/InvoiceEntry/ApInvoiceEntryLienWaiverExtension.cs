using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.LienWaiverValidationServices;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.InvoiceEntry
{
    public class ApInvoiceEntryLienWaiverExtension : PXGraphExtension<APInvoiceEntry>
    {
        public PXSetup<LienWaiverSetup> LienWaiverSetup;

        private LienWaiverValidationService lienWaiverValidationService;

        [InjectDependency]
        public IProjectDataProvider ProjectDataProvider
        {
            get;
            set;
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() &&
                !SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
        }

        public override void Initialize()
        {
            lienWaiverValidationService = new LienWaiverValidationService(Base, ProjectDataProvider);
        }

        protected virtual void _(Events.RowSelected<APInvoice> args)
        {
            var invoice = args.Row;
            if (invoice != null && LienWaiverSetup.Current.ShouldWarnOnBillEntry == true)
            {
                var projectIds = LienWaiverProjectDataProvider.GetProjectIds(Base);
                lienWaiverValidationService.ValidatePrimaryVendor<APInvoice.vendorID>(args.Cache, invoice,
                    invoice.VendorID, projectIds, ComplianceMessages.LienWaiver.VendorHasOutstandingLienWaiver);
            }
        }

        protected virtual void _(Events.RowSelected<JointPayee> args)
        {
            var jointPayee = args.Row;
            if (jointPayee != null && LienWaiverSetup.Current.ShouldWarnOnBillEntry == true)
            {
                var projectIds = LienWaiverProjectDataProvider.GetProjectIds(Base, jointPayee);
                lienWaiverValidationService.ValidateJointPayee(args.Cache, jointPayee, projectIds.ToList(),
                    ComplianceMessages.LienWaiver.JointPayeeHasOutstandingLienWaiver);
            }
        }
    }
}