using System;
using System.Linq;
using System.Collections.Generic;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Common;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.Extensions.PerUnitTax;

namespace PX.Objects.AR
{
	/// <summary>
	/// A per unit taxes post on AR release helper.
	/// </summary>
	public class ARReleaseProcessPerUnitTaxPoster : PerUnitTaxesPostOnReleaseExt<ARReleaseProcess, ARInvoice, ARTran, ARTax, ARTaxTran>
	{
		protected delegate void PostTaxDelegate(JournalEntry journalEntry, ARInvoice document, ARRegister doc, Tax perUnitTax, 
												ARTaxTran perUnitAggregatedTax, Account taxAccount, CurrencyInfo newCurrencyInfo, bool isDebit);

		private PostTaxDelegate _postGeneralTax;

		public static bool IsActive() => IsActiveBase();

		public override void Initialize()
		{
			base.Initialize();
			FillProtectedMethodDelegate("PostGeneralTax", out _postGeneralTax);
		}

		protected override bool CheckInputDocument(ARInvoice arInvoice)
		{
			if (!base.CheckInputDocument(arInvoice))
				return false;
			
			return arInvoice.IsOriginalRetainageDocument() && Base.arsetup.RetainTaxes == true
				? throw new PXNotSupportedException(TX.Messages.CannotPostPerUnitTaxesToItemAccountsForRetainedDocuments)
				: true;
		}

		protected override void CreateAndPostGLTransactionsOnTaxAccount(JournalEntry journalEntry, ARInvoice document, CurrencyInfo newCurrencyInfo, 
																		ARTaxTran perUnitAggregatedTax, Tax perUnitTax, bool isDebitTaxTran)
		{
			Account taxAccount = SelectFrom<Account>
									.Where<Account.accountID.IsEqual<P.AsInt>>.View
									.SelectSingleBound(Base, currents: null, pars: perUnitAggregatedTax.AccountID);

			_postGeneralTax?.Invoke(journalEntry, document, document, perUnitTax, perUnitAggregatedTax, taxAccount, newCurrencyInfo, isDebitTaxTran);
		}

		protected override IEnumerable<(ARTax Tax, ARTran Line)> GetTaxWithLines(Tax perUnitTax, ARTaxTran perUnitAggregatedTax)
		{
			return PXSelectJoin<ARTax,
					  InnerJoin<ARTran, On<ARTax.tranType, Equal<ARTran.tranType>,
							And<ARTax.refNbr, Equal<ARTran.refNbr>,
							And<ARTax.lineNbr, Equal<ARTran.lineNbr>>>>>,
						Where<ARTax.taxID, Equal<Required<ARTax.taxID>>,
							And<ARTran.tranType, Equal<Required<ARTran.tranType>>,
							And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>,
						OrderBy<
							Desc<ARTax.curyTaxAmt>>>
					.Select(Base, perUnitTax.TaxID, perUnitAggregatedTax.TranType, perUnitAggregatedTax.RefNbr)
					.AsEnumerable()
					.Select(res => (res.GetItem<ARTax>(), res.GetItem<ARTran>()));
		}

		protected override GLTran CreateGLTranForPerUnitLineTax(ARInvoice arInvoice, CurrencyInfo newCurrencyInfo, ARTaxTran perUnitAggregatedTax,
																ARTran arTran, ARTax perUnitLineTax, bool isDebitTaxTran)
		{
			GLTran newGlTran = base.CreateGLTranForPerUnitLineTax(arInvoice, newCurrencyInfo, perUnitAggregatedTax,
																  arTran, perUnitLineTax, isDebitTaxTran);
			newGlTran.SummPost = Base.SummPost;
			newGlTran.BranchID = arTran.BranchID;
			newGlTran.AccountID = arTran.AccountID;
			newGlTran.SubID = arTran.SubID;
			newGlTran.TranLineNbr = arTran.LineNbr;
			newGlTran.ReferenceID = arInvoice.CustomerID;
			newGlTran.ProjectID = arTran.ProjectID;
			newGlTran.TaskID = arTran.TaskID;
			newGlTran.CostCodeID = arTran.CostCodeID;

			if (isDebitTaxTran)
			{
				newGlTran.CuryDebitAmt = perUnitLineTax.CuryTaxAmt;
				newGlTran.DebitAmt = perUnitLineTax.TaxAmt;
			}
			else
			{
				newGlTran.CuryCreditAmt = perUnitLineTax.CuryTaxAmt;
				newGlTran.CreditAmt = perUnitLineTax.TaxAmt;
			}

			return newGlTran;
		}

		protected override GLTran InsertNewGLTran(JournalEntry journalEntry, ARInvoice arInvoice, ARTran arTran, ARTaxTran perUnitAggregatedTax,
												  ARTax perUnitLineTax, GLTran newGlTran)
		{
			var glTranInsertionContext = new ARReleaseProcess.GLTranInsertionContext
			{
				ARRegisterRecord = arInvoice,
				ARTranRecord = arTran,
				ARTaxTranRecord = perUnitAggregatedTax
			};

			return Base.InsertInvoicePerUnitTaxAmountsToItemAccountsTransaction(journalEntry, newGlTran, glTranInsertionContext);
		}
	}
}