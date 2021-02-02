using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.Common;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PreparePayments
{
    public class ApPayBillsExt : PXGraphExtension<APPayBills>
    {
        public PXSelect<JointPayeePayment> JointPayeePayments;
        public PXSelect<JointPayee> JointPayees;

        public SelectFrom<APInvoice>
            .Where<APInvoice.refNbr.IsEqual<APAdjust.adjdRefNbr.FromCurrent>
                .And<APInvoice.docType.IsEqual<APAdjust.adjdDocType.FromCurrent>>>.View CurrentBill;

        public SelectFrom<APAdjust>
            .InnerJoin<APPayment>.On<APPayment.refNbr.IsEqual<APAdjust.adjgRefNbr>
                .And<APPayment.docType.IsEqual<APAdjust.adjgDocType>>>.View Adjustments;

        public APInvoice CurrentInvoice =>
            InvoiceDataProvider.GetInvoice(Base, Adjustments.Current?.AdjdDocType, Adjustments.Current?.AdjdRefNbr);

        public APInvoice OriginalInvoice => InvoiceDataProvider.GetOriginalInvoice(Base, CurrentInvoice);

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public IEnumerable jointPayeePayments()
        {
            return Base.APDocumentList.Current != null
                ? GetJointPayeePaymentsRelatedToBill(Base.APDocumentList.Current)
                : GetCurrentJointPayeePayments();
        }

        public IEnumerable jointPayees()
        {
            return SelectFrom<JointPayee>.Where<JointPayee.billId.IsEqual<P.AsGuid>>.View.Select(Base,
                OriginalInvoice?.NoteID);
        }

        public virtual void _(Events.RowInserted<APAdjust> args)
        {
            if (args.Row is APAdjust adjustment && IsJointPayees(adjustment))
            {
                CreateJointPayeePayments(adjustment);
                SetCashDiscountBalance(adjustment);
                adjustment.CuryAdjgDiscAmt = 0;
            }
        }

        public virtual void _(Events.RowSelected<APAdjust> args)
        {
            if (args.Row is APAdjust adjustment && !PXLongOperation.Exists(Base.UID))
            {
                UpdateAmountPaidAvailability(adjustment);
            }
        }

        public virtual void _(Events.RowUpdated<PayBillsFilter> args)
        {
            JointPayeePayments.Cache.Clear();
            JointPayeePayments.Cache.ClearQueryCache();
        }

        private void SetCashDiscountBalance(APAdjust adjustment)
        {
            var invoice = InvoiceDataProvider.GetInvoice(Base, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            adjustment.CuryDiscBal = invoice.PaymentsByLinesAllowed == true
                ? TransactionDataProvider.GetTransaction(Base, invoice.DocType, invoice.RefNbr, adjustment.AdjdLineNbr)
                    .CuryCashDiscBal
                : invoice.CuryOrigDiscAmt;
        }

        private void UpdateAmountPaidAvailability(IDocumentAdjustment adjustment)
        {
            var isJointPayees = IsJointPayees(adjustment);
            PXUIFieldAttribute.SetReadOnly<APAdjust.curyAdjgAmt>(Adjustments.Cache, adjustment, isJointPayees);
            PXUIFieldAttribute.SetReadOnly<APAdjust.curyAdjgDiscAmt>(Adjustments.Cache, adjustment, isJointPayees);
        }

        private bool IsJointPayees(IDocumentAdjustment adjustment)
        {
            var invoice = InvoiceDataProvider.GetInvoice(Base, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            return PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(invoice).IsJointPayees == true;
        }

        private void CreateJointPayeePayments(APAdjust adjustment)
        {
            var originalInvoice =
                InvoiceDataProvider.GetOriginalInvoice(Base, adjustment.AdjdRefNbr, adjustment.AdjdDocType);
            var jointPayees =
                JointPayeeDataProvider.GetJointPayees(Base, originalInvoice.RefNbr, adjustment.AdjdLineNbr);
            var jointPayeePayments = jointPayees
                .Select(jointPayee => CreateJointPayeePayment(jointPayee.JointPayeeId, adjustment));
            JointPayeePayments.Cache.InsertAll(jointPayeePayments);
        }

        private static JointPayeePayment CreateJointPayeePayment(int? jointPayeeId, IDocumentAdjustment adjustment)
        {
            return new JointPayeePayment
            {
                JointPayeeId = jointPayeeId,
                PaymentDocType = adjustment.AdjdDocType,
                PaymentRefNbr = adjustment.AdjdRefNbr,
                JointAmountToPay = 0,
                AdjustmentNumber = 0
            };
        }

        private IEnumerable<PXResult<JointPayeePayment>> GetJointPayeePaymentsRelatedToBill(APAdjust adjustment)
        {
            return GetCurrentJointPayeePayments().Where(x => IsRelatedToBill(x, adjustment));
        }

        private static bool IsRelatedToBill(PXResult jointPayeePaymentResult, APAdjust adjustment)
        {
            var jointPayeePayment = jointPayeePaymentResult.GetItem<JointPayeePayment>();
            var isRelatedToBill = jointPayeePayment.PaymentDocType == adjustment.AdjdDocType
                && jointPayeePayment.PaymentRefNbr == adjustment.AdjdRefNbr;
            return adjustment.AdjdLineNbr != 0
                ? isRelatedToBill && jointPayeePayment.BillLineNumber == adjustment.AdjdLineNbr
                : isRelatedToBill;
        }

        private PXResultset<JointPayeePayment> GetCurrentJointPayeePayments()
        {
            return OriginalInvoice == null
                ? new PXResultset<JointPayeePayment, JointPayee>()
                : JointPayeePaymentDataProvider.GetCurrentJointPayeePayments(Base, OriginalInvoice, CurrentInvoice);
        }
    }
}