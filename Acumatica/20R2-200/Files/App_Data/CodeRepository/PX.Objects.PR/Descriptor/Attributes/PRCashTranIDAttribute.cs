using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.GL;

namespace PX.Objects.PR
{
	public abstract class PRCashTranIDAttribute : CashTranIDAttribute
	{
		protected CATran ProcessCATran(PXCache sender, CATran caTranRow, PRPayment payment, decimal? tranAmount, string tranDescription)
		{
			caTranRow.OrigModule = BatchModule.PR;
			caTranRow.OrigTranType = payment.DocType;
			caTranRow.OrigRefNbr = payment.RefNbr;
			caTranRow.ExtRefNbr = payment.ExtRefNbr;
			caTranRow.CashAccountID = payment.CashAccountID;
			caTranRow.CuryInfoID = payment.CuryInfoID;
			caTranRow.CuryTranAmt = tranAmount;
			caTranRow.DrCr = DrCr.Credit;
			caTranRow.TranDate = payment.TransactionDate;
			caTranRow.TranDesc = tranDescription;
			caTranRow.FinPeriodID = payment.FinPeriodID;
			caTranRow.ReferenceID = payment.EmployeeID;
			caTranRow.Released = payment.Released;
			caTranRow.Hold = false;

			PXSelectBase<CashAccount> selectStatement = new PXSelectReadonly<CashAccount,
				Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>(sender.Graph);
			CashAccount cashAccount = (CashAccount)selectStatement.View.SelectSingle(caTranRow.CashAccountID);
			caTranRow.CuryID = cashAccount?.CuryID;

			if (cashAccount != null && cashAccount.Reconcile == false && (caTranRow.Cleared != true || caTranRow.TranDate == null))
			{
				caTranRow.Cleared = true;
				caTranRow.ClearDate = caTranRow.TranDate;
			}

			return caTranRow;
		}

		public override void RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (!sender.Graph.Views.Caches.Contains(typeof(CATran)))
				sender.Graph.Views.Caches.Add(typeof(CATran));
			
			base.RowDeleted(sender, e);
		}
	}

	/// <summary>
	/// Specialized for the PRPayment version of the <see cref="CashTranIDAttribute"/><br/>
	/// Since CATran created from the source row, it may be used only the fields <br/>
	/// of PRPayment compatible DAC. <br/>
	/// The main purpose of the attribute - to create CATran <br/>
	/// for the source row and provide CATran and source synchronization on persisting.<br/>
	/// CATran cache must exists in the calling Graph.<br/>
	/// </summary>
	public class PRPaymentCashTranIDAttribute : PRCashTranIDAttribute
	{
		private bool DirectDepositSplitsExist(PXGraph graph, PRPayment payment)
		{
			return SelectFrom<PRDirectDepositSplit>
				.Where<PRDirectDepositSplit.docType.IsEqual<P.AsString>
					.And<PRDirectDepositSplit.refNbr.IsEqual<P.AsString>>>.View
				.Select(graph, payment.DocType, payment.RefNbr).FirstTableItems.Any();
		}

		private bool PreventCATranRecordCreation(PXCache sender, CATran caTranRow, PRPayment payment)
		{
			var paymentMethod = (PaymentMethod)PXSelectorAttribute.Select<PRPayment.paymentMethodID>(sender, payment);
			var paymentMethodExt = paymentMethod.GetExtension<PRxPaymentMethod>();

			return payment.CashAccountID == null ||
					payment.Released == true && caTranRow.TranID != null ||
					payment.NetAmount.GetValueOrDefault() == 0 ||
					paymentMethodExt.PRPrintChecks == false && DirectDepositSplitsExist(sender.Graph, payment);
		}

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			PRPayment parentDoc = (PRPayment)orig_Row;

			if (PreventCATranRecordCreation(sender, catran_Row, parentDoc))
			{
				return null;
			}
			
			decimal? transactionAmount = -parentDoc.NetAmount;
			return ProcessCATran(sender, catran_Row, parentDoc, transactionAmount, parentDoc.DocDesc);
		}
	}

	/// <summary>
	/// Specialized for the PRDirectDepositSplit version of the <see cref="CashTranIDAttribute"/><br/>
	/// Since CATran created from the source row, it may be used only the fields <br/>
	/// of PRDirectDepositSplit compatible DAC. <br/>
	/// The main purpose of the attribute - to create CATran <br/>
	/// for the source row and provide CATran and source synchronization on persisting.<br/>
	/// CATran cache must exists in the calling Graph.<br/>
	/// </summary>
	public class PRDirectDepositCashTranIDAttribute : PRCashTranIDAttribute
	{
		private bool PreventCATranRecordCreation(CATran caTranRow, PRPayment payment, PRDirectDepositSplit directDepositSplit)
		{
			return payment.CashAccountID == null ||
					payment.Released == true && caTranRow.TranID != null ||
					directDepositSplit.Amount.GetValueOrDefault() == 0;
		}

		private string GetDescription(PRPayment payment, PRDirectDepositSplit directDepositSplit)
		{
			string paymentDescription = payment.DocDesc;
			string directDepositDescription = directDepositSplit.BankAcctNbr;

			if (!string.IsNullOrWhiteSpace(directDepositDescription) && directDepositDescription.Length > 5)
			{
				int substringToShowIndex = directDepositDescription.Length - 4;
				directDepositDescription = new string('*', substringToShowIndex) + directDepositDescription.Substring(substringToShowIndex);
			}
			else
			{
				directDepositDescription = string.Empty;
			}

			if (string.IsNullOrWhiteSpace(paymentDescription) && string.IsNullOrWhiteSpace(directDepositDescription))
				return string.Empty;

			if (string.IsNullOrWhiteSpace(paymentDescription))
				return directDepositDescription;

			if (string.IsNullOrWhiteSpace(directDepositDescription))
				return paymentDescription;

			return $"{paymentDescription}-{directDepositDescription}";
		}

		public override CATran DefaultValues(PXCache sender, CATran catran_Row, object orig_Row)
		{
			PRPayment payment = PXParentAttribute.SelectParent<PRPayment>(sender, orig_Row);
			PRDirectDepositSplit directDepositSplit = (PRDirectDepositSplit)orig_Row;

			if (PreventCATranRecordCreation(catran_Row, payment, directDepositSplit))
			{
				return null;
			}

			decimal? transactionAmount = -directDepositSplit.Amount;
			string description = GetDescription(payment, directDepositSplit);
			return ProcessCATran(sender, catran_Row, payment, transactionAmount, description);
		}
	}
}
