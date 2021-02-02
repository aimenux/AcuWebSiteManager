using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;
using PX.Objects.CN.JointChecks.AP.Services.PreparePaymentsServices;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PreparePayments
{
    public class ApPayBillsAddJointAmountsExtension : PXGraphExtension<ApPayBillsExt, APPayBills>
    {
        public PXAction<PayBillsFilter> AddJointAmounts;

        private AmountToPayValidationService amountToPayValidationService;

        private JointCheckVendorBalanceService jointCheckVendorBalanceService;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public override void Initialize()
        {
            Base1.JointPayeePayments.AllowInsert = false;
            amountToPayValidationService = new AmountToPayValidationService(Base);
        }

        [PXUIField(DisplayName = "Add Joint Amounts", MapEnableRights = PXCacheRights.Update,
            MapViewRights = PXCacheRights.Update, Enabled = false)]
        [PXButton]
        public virtual void addJointAmounts()
        {
            var adjustment = Base.APDocumentList.Current;
            if (adjustment == null)
            {
                return;
            }
            AddJointAmountsForJointCheckAdjustment(adjustment);
        }

        public virtual void _(Events.FieldVerifying<APInvoice, APInvoiceJCExt.amountToPay> args)
        {
            var invoice = args.Row;
            if (invoice == null)
            {
                return;
            }
            var amountToPay = args.NewValue as decimal?;
            amountToPayValidationService.ValidateVendorAmountToPay(invoice, amountToPay);
        }

        public virtual void _(Events.RowSelected<JointPayeePayment> args)
        {
            if (args.Row is JointPayeePayment jointPayeePayment && jointPayeePayment.JointAmountToPay > 0)
            {
                amountToPayValidationService.ValidateJointPayeePayment(args.Cache, jointPayeePayment);
            }
        }

        public virtual void _(Events.FieldUpdated<APInvoice, APInvoiceJCExt.amountToPay> args)
        {
            if (args.Row is APInvoice invoice)
            {
                SetAmountToPay(invoice, Base.APDocumentList.Current);
            }
        }

        public virtual void _(Events.FieldUpdated<APAdjust, APAdjust.adjdLineNbr> args)
        {
            if (!Base.HasErrors())
            {
                Base.APDocumentList.View.RequestRefresh();
            }
        }

        public virtual void _(Events.RowSelected<APInvoice> args)
        {
            var invoice = args.Row;
            if (invoice == null)
            {
                return;
            }
            SetBillLineNumberVisibility(invoice);
            SetVendorFields(invoice);
        }

        public virtual void _(Events.RowSelected<APAdjust> args)
        {
            var adjustment = args.Row;
            if (adjustment != null && !PXLongOperation.Exists(Base.UID))
            {
                SetAmountToPayIfEmpty(adjustment);
            }
        }

        private void SetAmountToPayIfEmpty(APAdjust adjustment)
        {
            var invoice = InvoiceDataProvider.GetInvoice(Base, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            var invoiceExtension = PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(invoice);
            var adjustmentExtension = PXCache<APAdjust>.GetExtension<ApAdjustExt>(adjustment);
            if (invoiceExtension.IsJointPayees == true && adjustmentExtension.AmountToPayPerLine == null)
            {
                adjustmentExtension.AmountToPayPerLine = GetVendorBalance(adjustment, invoice);
                adjustment.CuryAdjgAmt = adjustmentExtension.AmountToPayPerLine;
            }
        }

        private void SetVendorFields(APInvoice invoice)
        {
            var invoiceExtension = PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(invoice);
            if (invoiceExtension.IsJointPayees == true)
            {
                var adjustmentExtension = PXCache<APAdjust>.GetExtension<ApAdjustExt>(Base.APDocumentList.Current);
                invoiceExtension.VendorBalance = GetVendorBalance(Base.APDocumentList.Current, invoice);
                invoiceExtension.AmountToPay = adjustmentExtension.AmountToPayPerLine;
                jointCheckVendorBalanceService = new JointCheckVendorBalanceService();
                jointCheckVendorBalanceService.UpdateVendorBalanceDisplayName(invoice, Base1.CurrentBill.Cache);
            }
        }

        private decimal? GetVendorBalance(APAdjust adjustment, APInvoice invoice)
        {
            var jointPayees = Base1.JointPayees.SelectMain().Where(jp => jp.BillId == invoice.NoteID);
            if (invoice.PaymentsByLinesAllowed == true)
            {
                var vendorBalancePerLineCalculationService = new VendorBalancePerLineCalculationService(Base, jointPayees);
                return vendorBalancePerLineCalculationService.GetVendorBalancePerLine(adjustment);
            }
            var vendorBalanceCalculationService = new VendorBalanceCalculationService(Base, jointPayees);
            return vendorBalanceCalculationService.GetVendorBalancePerBill(invoice);
        }

        private void SetAmountToPay(APInvoice invoice, APAdjust adjustment)
        {
            var adjustmentExtension = PXCache<APAdjust>.GetExtension<ApAdjustExt>(adjustment);
            var invoiceExtension = PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(invoice);
            adjustmentExtension.AmountToPayPerLine = invoiceExtension.AmountToPay;
        }

        private void AddJointAmountsForJointCheckAdjustment(APAdjust adjustment)
        {
            if (Base1.JointPayees.AskExt().IsPositive())
            {
                adjustment.CuryAdjgAmt = PXCache<APAdjust>.GetExtension<ApAdjustExt>(adjustment).AmountToPayPerLine;
            }
        }

        private void SetBillLineNumberVisibility(APRegister currentInvoice)
        {
            var paymentsPerLine = currentInvoice.PaymentsByLinesAllowed.GetValueOrDefault();
            PXUIFieldAttribute.SetVisible<JointPayeePayment.billLineNumber>(Base1.JointPayees.Cache, null, paymentsPerLine);
        }
    }
}