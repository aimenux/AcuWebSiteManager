using System;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.CalculationServices
{
    public class CashDiscountCalculationService : CalculationServiceBase
    {
        protected VendorPreparedBalanceCalculationService VendorPreparedBalanceCalculationService;

        protected JointAmountToPayCalculationService JointAmountToPayCalculationService;

        public CashDiscountCalculationService(PXGraph graph)
            : base(graph)
        {
            VendorPreparedBalanceCalculationService = new VendorPreparedBalanceCalculationService(graph);
            JointAmountToPayCalculationService = new JointAmountToPayCalculationService(graph);
        }

        public virtual decimal? GetNonReleasedCashDiscountTakenExceptCurrentAdjustment(APAdjust apAdjust)
        {
            var adjustments = AdjustmentDataProvider.GetInvoiceAdjustments(Graph, apAdjust.AdjdRefNbr);
            var releasedCashDiscountTaken = InvoiceDataProvider
                .GetInvoice(Graph, apAdjust.AdjdDocType, apAdjust.AdjdRefNbr).CuryDiscTaken;
            var totalCashDiscountTaken = adjustments.Sum(adjustment => adjustment.CuryAdjgPPDAmt);
            return totalCashDiscountTaken - releasedCashDiscountTaken - apAdjust.CuryAdjgPPDAmt;
        }

        public virtual decimal GetAllowableCashDiscount(APAdjust apAdjust)
        {
            var totalJointAmountToPay = JointAmountToPayCalculationService
                .GetTotalJointAmountToPay(apAdjust).GetValueOrDefault();
            var cashDiscountWithJointPayees =
                GetAllowableCashDiscountConsiderJointPayees(apAdjust, totalJointAmountToPay);
            var cashDiscountWithDiscountBalance = GetAllowableCashDiscountConsiderCashDiscountBalance(apAdjust);
            var cashDiscountWithBillBalance = GetAllowableCashDiscountConsiderBillBalance(apAdjust);
            var cashDiscountWithVendorPreparedBalance = GetAllowableCashDiscountConsiderVendorPreparedBalance(apAdjust);
            var allowableCashDiscount = Math.Min(Math.Min(cashDiscountWithBillBalance, cashDiscountWithDiscountBalance),
                cashDiscountWithVendorPreparedBalance);
            return Math.Max(totalJointAmountToPay > 0
                ? Math.Min(cashDiscountWithJointPayees, allowableCashDiscount)
                : allowableCashDiscount, 0);
        }

        protected decimal GetAllowableCashDiscountConsiderJointPayees(APAdjust apAdjust, decimal totalJointAmountToPay)
        {
            var vendorPreparedBalance =
                VendorPreparedBalanceCalculationService.GetVendorPreparedBalance(apAdjust);
            var amountPaid = apAdjust.CuryAdjgAmt ?? 0;
            return Math.Min(vendorPreparedBalance.GetValueOrDefault(), Math.Max(amountPaid - totalJointAmountToPay, 0));
        }

        protected virtual decimal GetAllowableCashDiscountConsiderCashDiscountBalance(APAdjust apAdjust)
        {
            var cashDiscountTaken = GetNonReleasedCashDiscountTakenExceptCurrentAdjustment(apAdjust);
            var cashDiscountBalance = GetCashDiscountBalance(apAdjust);
            var cashDiscount = cashDiscountBalance - cashDiscountTaken;
            return Math.Max(cashDiscount.GetValueOrDefault(), 0);
        }

        protected virtual decimal GetAllowableCashDiscountConsiderBillBalance(APAdjust apAdjust)
        {
            var invoiceAdjustments = AdjustmentDataProvider.GetInvoiceAdjustments(Graph, apAdjust.AdjdRefNbr);
            var unappliedBalance = invoiceAdjustments
                .Sum(adjustment => adjustment.CuryAdjgAmt + adjustment.CuryAdjgPPDAmt);
            var billBalance = InvoiceDataProvider.GetInvoice(Graph, apAdjust.AdjdDocType, apAdjust.AdjdRefNbr)
                .CuryOrigDocAmt;
            var cashDiscount = billBalance - unappliedBalance + apAdjust.CuryAdjgPPDAmt;
            return Math.Max(cashDiscount.GetValueOrDefault(), 0);
        }

        protected decimal GetAllowableCashDiscountConsiderVendorPreparedBalance(APAdjust apAdjust)
        {
            var vendorPreparedBalance =
                VendorPreparedBalanceCalculationService.GetVendorPreparedBalance(apAdjust);
            var cashDiscountTakenFromOtherNonReleasedChecks =
                GetNonReleasedCashDiscountTakenExceptCurrentAdjustment(apAdjust);
            var cashDiscount = vendorPreparedBalance - cashDiscountTakenFromOtherNonReleasedChecks;
            return Math.Max(cashDiscount.GetValueOrDefault(), 0);
        }

        protected virtual decimal? GetCashDiscountBalance(APAdjust adjustment)
        {
            return InvoiceDataProvider.GetInvoice(Graph, adjustment.AdjdDocType, adjustment.AdjdRefNbr).DiscBal;
        }
    }
}
