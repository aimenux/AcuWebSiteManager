using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.Descriptor;

namespace PX.Objects.CN.JointChecks.AP.Services.BillsAndAdjustmentsServices
{
    public class PayBillAmountsToPayValidator
    {
        private readonly APInvoiceEntry graph;
        private readonly PXSelectBase<JointPayeePayment> jointPayeePaymentsView;
        private readonly PXSelectBase<JointPayee> jointPayeesView;
        private APInvoiceJCExt invoiceExtension;

        public PayBillAmountsToPayValidator(APInvoiceEntry graph,
            PXSelectBase<JointPayeePayment> jointPayeePaymentsView, PXSelectBase<JointPayee> jointPayeesView)
        {
            this.graph = graph;
            this.jointPayeePaymentsView = jointPayeePaymentsView;
            this.jointPayeesView = jointPayeesView;
        }

        private IEnumerable<JointPayeePayment> JointPayeePayments => jointPayeePaymentsView.Select().FirstTableItems;

        private IEnumerable<JointPayee> JointPayees => jointPayeesView.Select().FirstTableItems;

        private APInvoiceJCExt InvoiceExtension =>
            invoiceExtension ?? (invoiceExtension =
                PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(graph.Document.Current));

        public void ValidateAllAmountsToPay()
        {
            ValidateVendorPaymentAmountOnNegativeValue();
            ValidateVendorBalance();
            ValidateJointPayeePayments();
        }

        private void ValidateVendorPaymentAmountOnNegativeValue()
        {
            var vendorAmountToPay = InvoiceExtension.AmountToPay;
            if (vendorAmountToPay < 0)
            {
                ShowErrorMessage<APInvoiceJCExt.amountToPay>(graph.Document.Current,
                    JointCheckMessages.ValueCanNotBeNegative);
            }
        }

        private void ValidateVendorBalance()
        {
            if (InvoiceExtension.AmountToPay > InvoiceExtension.VendorBalance)
            {
                ShowErrorMessage<APInvoiceJCExt.amountToPay>(graph.Document.Current,
                    JointCheckMessages.AmountPaidExceedsVendorBalance, InvoiceExtension.VendorBalance);
            }
        }

        private void ValidateJointPayeePayments()
        {
            JointPayeePayments.ForEach(ValidateJointPayeePayment);
            JointPayeePayments.ForEach(ValidateJointPayeeForRetainageBill);
        }

        private void ValidateJointPayeePayment(JointPayeePayment jointPayeePayment)
        {
            var jointBalance = JointPayees
                .SingleOrDefault(jp => jp.JointPayeeId == jointPayeePayment.JointPayeeId)?.JointBalance;
            if (jointPayeePayment.JointAmountToPay > jointBalance)
            {
                ShowErrorMessage<JointPayeePayment.jointAmountToPay>(jointPayeePayment,
                    JointCheckMessages.JointAmountToPayExceedsJointPayeeBalance);
            }
        }

        private void ValidateJointPayeeForRetainageBill(JointPayeePayment jointPayeePayment)
        {
            var availableAmountToPay = GetAvailableJointAmountToPay(jointPayeePayment);
            if (jointPayeePayment.JointAmountToPay > availableAmountToPay)
            {
                ShowErrorMessage<JointPayeePayment.jointAmountToPay>(jointPayeePayment,
                    JointCheckMessages.TotalAmountToPayExceedsBalance);
            }
            if (graph.Document.Current.PaymentsByLinesAllowed == true)
            {
                ValidateJointPayeeTotalAmountToPayExceedBillLineAmount(jointPayeePayment);
            }
        }

        private void ValidateJointPayeeTotalAmountToPayExceedBillLineAmount(JointPayeePayment jointPayeePayment)
        {
            var jointAmountToPayPerLine = JointPayeePayments
                .Where(jpp => jpp.BillLineNumber == jointPayeePayment.BillLineNumber)
                .Sum(jpp => jpp.JointAmountToPay);
            var lineBalance = graph.Transactions.SelectMain()
                .Single(tran => tran.LineNbr == jointPayeePayment.BillLineNumber).CuryTranBal;
            if (jointAmountToPayPerLine > lineBalance)
            {
                ShowErrorMessage<JointPayeePayment.jointAmountToPay>(jointPayeePayment,
                    JointCheckMessages.TotalJointAmountPaidExceedsBillLineBalance);
            }
        }

        private decimal? GetAvailableJointAmountToPay(JointPayeePayment jointPayeePayment)
        {
            var jointPayeesAmountToPay = JointPayeePayments.Except(jointPayeePayment).Sum(jpp => jpp.JointAmountToPay);
            var billBalance = graph.Document.Current.CuryDocBal;
            var availableAmountToPay = billBalance - InvoiceExtension.AmountToPay - jointPayeesAmountToPay;
            return Math.Max(availableAmountToPay.GetValueOrDefault(), 0);
        }

        private void ShowErrorMessage<TField>(
            object entity, string format, params object[] args)
            where TField : IBqlField
        {
            var cache = graph.Caches[entity.GetType()];
            var fieldValue = cache.GetValue<TField>(entity);
            var exception = new PXSetPropertyException(format, args);
            cache.RaiseExceptionHandling<TField>(entity, fieldValue, exception);
            throw new PXException();
        }
    }
}