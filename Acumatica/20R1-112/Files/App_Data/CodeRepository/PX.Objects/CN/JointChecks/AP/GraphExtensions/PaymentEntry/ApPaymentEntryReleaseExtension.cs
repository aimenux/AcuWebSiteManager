using System;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;
using PX.Objects.CS;

namespace PX.Objects.CN.JointChecks.AP.GraphExtensions.PaymentEntry
{
    public class ApPaymentEntryReleaseExtension : PXGraphExtension<ApPaymentEntryExt, APPaymentEntry>
    {
        private InvoiceBalanceCalculationService invoiceBalanceCalculationService;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXOverride]
        public virtual IEnumerable Release(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
        {
            if (Base.Document.Current?.DocType == APDocType.Check)
            {
                UpdateFinalAdjustments();
            }
            return baseHandler(adapter);
        }

        private void UpdateFinalAdjustments()
        {
            var finalAdjustments = Base.Adjustments.SelectMain().Where(IsFinalAdjustment);
            Base.Adjustments.Cache.UpdateAll(finalAdjustments);
        }

        private bool IsFinalAdjustment(APAdjust adjustment)
        {
            var invoice = InvoiceDataProvider.GetInvoice(Base, adjustment.AdjdDocType, adjustment.AdjdRefNbr);
            invoiceBalanceCalculationService = invoice.PaymentsByLinesAllowed == true
                ? new InvoiceLineBalanceCalculationService(Base)
                : new InvoiceBalanceCalculationService(Base);
            return invoiceBalanceCalculationService.GetInvoiceBalance(adjustment) == adjustment.CuryAdjgAmt;
        }
    }
}