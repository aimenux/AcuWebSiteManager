using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.InvoiceEntry
{
    public class ApInvoiceEntryJointApplicationsExtension : PXGraphExtension<ApInvoiceEntryExt, APInvoiceEntry>
    {
        [PXCopyPasteHiddenView]
        public SelectFrom<JointPayeePayment>
            .InnerJoin<JointPayee>.On<JointPayee.jointPayeeId.IsEqual<JointPayeePayment.jointPayeeId>>
            .InnerJoin<APPayment>.On<APPayment.refNbr.IsEqual<JointPayeePayment.paymentRefNbr>
                .And<APPayment.docType.IsEqual<JointPayeePayment.paymentDocType>>>
            .Where<JointPayeePayment.invoiceRefNbr.IsEqual<APInvoice.refNbr.FromCurrent>
                .And<JointPayeePayment.invoiceDocType.IsEqual<APInvoice.docType.FromCurrent>>
                .And<JointPayeePayment.jointAmountToPay.IsNotEqual<decimal0>>>.View.ReadOnly JointAmountApplications;

        public PXAction<APInvoice> ViewApPayment;

        public override void Initialize()
        {
            SetJointAmountApplicationFieldNames();
        }

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>() &&
                !SiteMapExtension.IsTaxBillsAndAdjustmentsScreenId();
        }

        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXLookupButton]
        public virtual void viewApPayment()
        {
            if (JointAmountApplications.Current == null)
            {
                return;
            }
            var graph = PXGraph.CreateInstance<APPaymentEntry>();
            graph.Document.Current = GetCurrentApPayment();
            throw new PXRedirectRequiredException(graph, JointCheckActions.ViewPayment)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private void SetJointAmountApplicationFieldNames()
        {
            var apPaymentCache = Base.Caches<APPayment>();
            PXUIFieldAttribute.SetDisplayName<JointPayeePayment.jointAmountToPay>(
                JointAmountApplications.Cache, JointCheckLabels.JointPaidAmount);
        }

        private APPayment GetCurrentApPayment()
        {
            var jointPayeePaymentId = JointAmountApplications.Current.JointPayeePaymentId;
            return JointAmountApplications
                .Search<JointPayeePayment.jointPayeePaymentId>(jointPayeePaymentId)
                .Select(x => x.GetItem<APPayment>())
                .First();
        }
    }
}