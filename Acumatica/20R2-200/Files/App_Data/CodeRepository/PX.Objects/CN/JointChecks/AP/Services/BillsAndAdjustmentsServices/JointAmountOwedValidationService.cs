using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CN.AP.Services;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.JointChecks.AP.CacheExtensions;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.CalculationServices;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;
using PX.Objects.CN.JointChecks.Descriptor;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;

namespace PX.Objects.CN.JointChecks.AP.Services.BillsAndAdjustmentsServices
{
    public class JointAmountOwedValidationService
    {
        protected PXSelect<JointPayee> JointPayees;
        protected APInvoiceEntry Graph;
        private APInvoiceJCExt invoiceExtension;

        private JointPayeeAmountsCalculationService JointPayeeAmountsCalculationService;

        private APTranCalculationService ApTranCalculationService;
		public JointAmountOwedValidationService(PXSelect<JointPayee> jointPayees, APInvoiceEntry graph)
        {
            JointPayees = jointPayees;
            Graph = graph;

            JointPayeeAmountsCalculationService = new JointPayeeAmountsCalculationService(Graph);
            ApTranCalculationService = new APTranCalculationService(Graph);
		}

        protected APInvoice CurrentBill => Graph.Document.Current;

        protected APInvoiceJCExt InvoiceExtension =>
            invoiceExtension ?? (invoiceExtension =
                PXCache<APInvoice>.GetExtension<APInvoiceJCExt>(CurrentBill));

        public virtual void ValidateAmountOwed(JointPayee jointPayee)
        {
            ValidateTotalJointAmountOwed(jointPayee);
        }

        public virtual void RecalculateTotalJointAmount()
        {
            InvoiceExtension.TotalJointAmount = JointPayees.Select().FirstTableItems.Sum(jp => jp.JointAmountOwed);
        }

        protected virtual void ValidateTotalJointAmountOwed(JointPayee jointPayee)
        {
            var cashDiscount = CurrentBill.CuryOrigDiscAmt;
            if (InvoiceExtension.TotalJointAmount > CurrentBill.CuryLineTotal - cashDiscount)
            {
                ShowAmountOwedExceedsBillAmountException(jointPayee, cashDiscount == 0,
                    JointCheckMessages.AmountOwedExceedsBalance,
                    JointCheckMessages.AmountOwedExceedsBalanceWithCashDiscount);
            }
        }

        protected void ShowAmountOwedExceedsBillAmountException(JointPayee jointPayee, bool isCashDiscount,
            string messageWithoutCashDiscount, string messageWithCashDiscount)
        {
            var errorMessage = isCashDiscount
                ? messageWithoutCashDiscount
                : messageWithCashDiscount;
            JointPayees.Cache.RaiseException<JointPayee.jointAmountOwed>(jointPayee, errorMessage,
                jointPayee.JointAmountOwed);
            throw new PXException();
        }

		public void ValidateJointPayeesAmountOwedWithUnreleasedPayments()
		{
			List<JointPayee> payees = JointPayees.Select().FirstTableItems.ToList();

			foreach (var group in payees.GroupBy(payee => payee.BillLineNumber))
			{
				List<JointPayee> groupPayees = group.ToList();

				decimal? amountOwed = groupPayees.Sum(payee => payee.JointAmountOwed);

				ValidateJointAmountOwedWithUnreleasedPayments(group.Key, groupPayees, amountOwed, false)
					.RaiseIfHasError<PXException>();
			}
		}

		public ProcessingResult ValidateJointAmountOwedWithUnreleasedPayments(int? lineNbr, List<JointPayee> jointPayees, decimal? amountOwed, bool singleModeValidation)
		{
			APInvoice document = Graph.Document.Current;

			if (document == null
			    || jointPayees.Any(p => p.BillLineNumber == null))
				return ProcessingResult.Success;

			List<JointPayee> jointPayeesOfTheSameItem = document.PaymentsByLinesAllowed == true
				? JointPayees.Select().FirstTableItems.AsEnumerable().Where(payee => payee.BillLineNumber == lineNbr).ToList()
				: JointPayees.Select().FirstTableItems.ToList();

			IEnumerable<JointPayee> allocatedJointPayeesRaw =
				singleModeValidation
					? jointPayeesOfTheSameItem.Where(payee => payee.JointPayeeId != jointPayees.Single().JointPayeeId)
					: jointPayeesOfTheSameItem;

			List<JointPayee> allocatedJointPayees = allocatedJointPayeesRaw.ToList();

			if (amountOwed < decimal.Zero)
			{
				ProcessingResult result = new ProcessingResult();

				result.AddErrorMessage(JointCheckMessages.TheValueMustNotBeLesserThanZero, amountOwed);

				return result;
			}

			decimal? minValue = 0m;
			decimal? allocatedUnreleasedAmountToPay = 0m;
			decimal? payeeUnreleasedAmountToPayWithRetainageItems = 0m;
			decimal? jointPayeesAmoutPaid = 0m;

			if (document.Released == true)
			{
				IEnumerable<JointPayeePayment> payeeUnreleasedJointPayeePaymentsWithRetainageItems = JointPayeeAmountsCalculationService.GetUnreleasedJointPayeePayments(jointPayees, true);
				payeeUnreleasedAmountToPayWithRetainageItems = payeeUnreleasedJointPayeePaymentsWithRetainageItems.Sum(jpp => jpp.JointAmountToPay);
				jointPayeesAmoutPaid = jointPayees.Sum(payee => payee.JointAmountPaid);
				minValue = jointPayeesAmoutPaid + payeeUnreleasedAmountToPayWithRetainageItems;

				APAdjust fullUnreleasedAdjusts = PXSelectGroupBy<APAdjust,
						Where<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>,
							And<APAdjust.adjdDocType, Equal<Required<APAdjust.adjdDocType>>,
							And<APAdjust.adjdLineNbr, Equal<Required<APAdjust.adjdLineNbr>>,
							And<APAdjust.released, Equal<False>>>>>,
						Aggregate<Sum<APAdjust.curyAdjgAmt>>>
						.Select(Graph, document.RefNbr, document.DocType, lineNbr);

				if (fullUnreleasedAdjusts?.CuryAdjgAmt != null)
				{
					//no unreleased payments to retainage here
					//Vendor unreleased
					allocatedUnreleasedAmountToPay = fullUnreleasedAdjusts.CuryAdjgAmt - JointPayeeAmountsCalculationService.GetUnreleasedJointPayeePayments(jointPayeesOfTheSameItem, true).Sum(jpp => jpp.JointAmountToPay);
				}
			}


			decimal? openBalance = 0m;
			if (document.PaymentsByLinesAllowed == true)
			{
				APTran tran = TransactionDataProvider.GetTransaction(Graph, document.DocType, document.RefNbr, lineNbr);

				openBalance = document.Released == true
					? tran.CuryTranBal
					: ApTranCalculationService.CalcCuryOrigTranAmts(tran.TranType, tran.RefNbr, tran.LineNbr.SingleToArray())
												.Values.Single();
			}
			else
			{
				openBalance = document.CuryDocBal;
			}

			decimal? maxValue = singleModeValidation
				? jointPayeesAmoutPaid + openBalance - allocatedJointPayees.Sum(payee => payee.JointAmountOwed - payee.JointAmountPaid) - allocatedUnreleasedAmountToPay
				: jointPayeesAmoutPaid + openBalance - allocatedUnreleasedAmountToPay;

			if (amountOwed < minValue || amountOwed > maxValue)
			{
				StringBuilder errorMessageBuilder = new StringBuilder();

				errorMessageBuilder.Append(PXMessages.LocalizeFormat(
					singleModeValidation
						? JointCheckMessages.EnteredJointAmountOwedIsIncorrect
						: JointCheckMessages.TotalAmountOwedToAllJointPayeesIsIncorrect,
					PXDBCurrencyAttribute.RoundCury(Graph.Document.Cache, document, minValue.GetValueOrDefault()),
					PXDBCurrencyAttribute.RoundCury(Graph.Document.Cache, document, maxValue.GetValueOrDefault())));
				

				ProcessingResult result = new ProcessingResult();

				result.AddErrorMessage(errorMessageBuilder.ToString());

				return result;
			}
			return ProcessingResult.Success;
		}
	}
}