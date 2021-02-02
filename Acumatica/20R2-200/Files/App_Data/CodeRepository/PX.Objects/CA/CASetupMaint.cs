using System;
using System.Collections.Generic;
using System.Text;

using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.CA
{
	public class CASetupMaint : PXGraph<CASetupMaint>
	{
		public PXSelect<CASetup> CASetupRecord;
		public PXSave<CASetup> Save;
		public PXCancel<CASetup> Cancel;

		public CASetupMaint()
		{
			GLSetup setup = GLSetup.Current;
		}

		public PXSetup<GLSetup> GLSetup;
		public PXSelect<CASetupApproval> Approval;

		protected virtual void CASetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			CASetup row = (CASetup)e.Row;

			bool invoiceFilterByDate = row?.InvoiceFilterByDate == true;
			PXUIFieldAttribute.SetEnabled<CASetup.daysBeforeInvoiceDiscountDate>(sender, null, invoiceFilterByDate);
			PXUIFieldAttribute.SetEnabled<CASetup.daysBeforeInvoiceDueDate>(sender, null, invoiceFilterByDate);
			PXUIFieldAttribute.SetEnabled<CASetup.daysAfterInvoiceDueDate>(sender, null, invoiceFilterByDate);

			PXUIFieldAttribute.SetRequired<CASetup.daysBeforeInvoiceDiscountDate>(sender, invoiceFilterByDate);
			PXUIFieldAttribute.SetRequired<CASetup.daysBeforeInvoiceDueDate>(sender, invoiceFilterByDate);
			PXUIFieldAttribute.SetRequired<CASetup.daysAfterInvoiceDueDate>(sender, invoiceFilterByDate);
		}

		protected virtual void CASetup_InvoiceFilterByDate_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			CASetup row = (CASetup)e.Row;

			if (row == null) return;

			if (row.InvoiceFilterByDate != true)
			{
				row.DaysBeforeInvoiceDiscountDate = 0;
				row.DaysBeforeInvoiceDueDate = 0;
				row.DaysAfterInvoiceDueDate = 0;
			}
		}

		protected virtual void CASetup_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			CASetup row = (CASetup) e.NewRow;
			if (e.NewRow == null || row == null || row.TransitAcctId == null) return;

			CashAccount cashAccount =
				PXSelect<CashAccount, Where<CashAccount.accountID, Equal<Required<CASetup.transitAcctId>>>>.Select(
					this, row.TransitAcctId);
			if (cashAccount == null) return;
			if (cashAccount.SubID != (int) row.TransitSubID)
			{
				Sub subAccount = PXSelect<Sub, Where<Sub.subID, Equal<Required<CASetup.transitSubID>>>>.Select(
					this, row.TransitSubID);

				sender.RaiseExceptionHandling<CASetup.transitSubID>(row, subAccount.SubCD, new PXSetPropertyException(Messages.WrongSubIdForCashAccount));
			}
		}

		protected virtual void _(Events.RowPersisted<CASetup> e)
		{
			if (e.TranStatus == PXTranStatus.Open)
			{
				PXUpdate<Set<CashAccount.receiptTranDaysBefore, Required<CashAccount.receiptTranDaysBefore>,
						 Set<CashAccount.receiptTranDaysAfter, Required<CashAccount.receiptTranDaysAfter>,
						 Set<CashAccount.disbursementTranDaysBefore, Required<CashAccount.disbursementTranDaysBefore>,
						 Set<CashAccount.disbursementTranDaysAfter, Required<CashAccount.disbursementTranDaysAfter>,
						 Set<CashAccount.allowMatchingCreditMemo, Required<CashAccount.allowMatchingCreditMemo>,
						 Set<CashAccount.refNbrCompareWeight, Required<CashAccount.refNbrCompareWeight>,
						 Set<CashAccount.dateCompareWeight, Required<CashAccount.dateCompareWeight>,
						 Set<CashAccount.payeeCompareWeight, Required<CashAccount.payeeCompareWeight>,
						 Set<CashAccount.dateMeanOffset, Required<CashAccount.dateMeanOffset>,
						 Set<CashAccount.dateSigma, Required<CashAccount.dateSigma>,
						 Set<CashAccount.skipVoided, Required<CashAccount.skipVoided>,
						 Set<CashAccount.curyDiffThreshold, Required<CashAccount.curyDiffThreshold>,
						 Set<CashAccount.amountWeight, Required<CashAccount.amountWeight>,
						 Set<CashAccount.emptyRefNbrMatching, Required<CashAccount.emptyRefNbrMatching>,
						 Set<CashAccount.matchThreshold, Required<CashAccount.matchThreshold>,
						 Set<CashAccount.relativeMatchThreshold, Required<CashAccount.relativeMatchThreshold>,
						 Set<CashAccount.invoiceFilterByCashAccount, Required<CashAccount.invoiceFilterByCashAccount>,
						 Set<CashAccount.invoiceFilterByDate, Required<CashAccount.invoiceFilterByDate>,
						 Set<CashAccount.daysBeforeInvoiceDiscountDate, Required<CashAccount.daysBeforeInvoiceDiscountDate>,
						 Set<CashAccount.daysBeforeInvoiceDueDate, Required<CashAccount.daysBeforeInvoiceDueDate>,
						 Set<CashAccount.daysAfterInvoiceDueDate, Required<CashAccount.daysAfterInvoiceDueDate>,
						 Set<CashAccount.invoiceRefNbrCompareWeight, Required<CashAccount.invoiceRefNbrCompareWeight>,
						 Set<CashAccount.invoiceDateCompareWeight, Required<CashAccount.invoiceDateCompareWeight>,
						 Set<CashAccount.invoicePayeeCompareWeight, Required<CashAccount.invoicePayeeCompareWeight>,
						 Set<CashAccount.averagePaymentDelay, Required<CashAccount.averagePaymentDelay>,
						 Set<CashAccount.invoiceDateSigma, Required<CashAccount.invoiceDateSigma>>>>>>>>>>>>>>>>>>>>>>>>>>>,
					CashAccount,
					Where<CashAccount.matchSettingsPerAccount, NotEqual<True>>>
					.Update(this, 
						e.Row.ReceiptTranDaysBefore,
						e.Row.ReceiptTranDaysAfter,
						e.Row.DisbursementTranDaysBefore,
						e.Row.DisbursementTranDaysAfter,
						e.Row.AllowMatchingCreditMemo,
						e.Row.RefNbrCompareWeight,
						e.Row.DateCompareWeight,
						e.Row.PayeeCompareWeight,
						e.Row.DateMeanOffset,
						e.Row.DateSigma,
						e.Row.SkipVoided,
						e.Row.CuryDiffThreshold,
						e.Row.AmountWeight,
						e.Row.EmptyRefNbrMatching,
						e.Row.MatchThreshold,
						e.Row.RelativeMatchThreshold,
						e.Row.InvoiceFilterByCashAccount,
						e.Row.InvoiceFilterByDate,
						e.Row.DaysBeforeInvoiceDiscountDate,
						e.Row.DaysBeforeInvoiceDueDate,
						e.Row.DaysAfterInvoiceDueDate,
						e.Row.InvoiceRefNbrCompareWeight,
						e.Row.InvoiceDateCompareWeight,
						e.Row.InvoicePayeeCompareWeight,
						e.Row.AveragePaymentDelay,
						e.Row.InvoiceDateSigma);
			}
		}
	}
}
