using System;
using System.Linq;
using System.Collections.Generic;
using PX.Data;
using PX.Common;
using PX.Objects.Common;
using PX.Objects.GL;
using PX.Objects.TX;
using PX.Objects.CM;
using PX.Objects.Extensions.PerUnitTax;

namespace PX.Objects.AP
{
	/// <summary>
	/// A per unit taxes post on AP release helper.
	/// </summary>
	public class APReleaseProcessPerUnitTaxPoster : PerUnitTaxesPostOnReleaseExt<APReleaseProcess, APInvoice, APTran, APTax, APTaxTran>
	{
		protected delegate void PostTaxDelegate(JournalEntry journalEntry, APInvoice document, CurrencyInfo newCurrencyInfo, 
												APTaxTran perUnitAggregatedTax, Tax perUnitTax);

		private PostTaxDelegate _postGeneralTax;
		private PostTaxDelegate _postReverseTax;

		public static bool IsActive() => IsActiveBase();

		public override void Initialize()
		{
			base.Initialize();
			FillProtectedMethodDelegate("PostGeneralTax", out _postGeneralTax);
			FillProtectedMethodDelegate("PostReverseTax", out _postReverseTax);
		}

		protected override bool CheckInputDocument(APInvoice apInvoice)
		{
			if (!base.CheckInputDocument(apInvoice))
				return false;

			return apInvoice.IsOriginalRetainageDocument() && Base.apsetup.RetainTaxes == true
				? throw new PXNotSupportedException(TX.Messages.CannotPostPerUnitTaxesToItemAccountsForRetainedDocuments)
				: true;
		}

		protected override void CreateAndPostGLTransactionsOnTaxAccount(JournalEntry journalEntry, APInvoice document, CurrencyInfo newCurrencyInfo,
																		APTaxTran perUnitAggregatedTax, Tax perUnitTax, bool isDebitTaxTran)
		{
			if (perUnitTax.ReverseTax != true)
			{
				_postGeneralTax?.Invoke(journalEntry, document, newCurrencyInfo, perUnitAggregatedTax, perUnitTax);
			}
			else
			{
				_postReverseTax?.Invoke(journalEntry, document, newCurrencyInfo, perUnitAggregatedTax, perUnitTax);
			}
		}

		protected override IEnumerable<(APTax Tax, APTran Line)> GetTaxWithLines(Tax perUnitTax, APTaxTran perUnitAggregatedTax)
		{		
			return PXSelectJoin<APTax,
					  InnerJoin<APTran, On<APTax.tranType, Equal<APTran.tranType>,
							And<APTax.refNbr, Equal<APTran.refNbr>,
							And<APTax.lineNbr, Equal<APTran.lineNbr>>>>>,
						Where<APTax.taxID, Equal<Required<APTax.taxID>>,
							And<APTran.tranType, Equal<Required<APTran.tranType>>,
							And<APTran.refNbr, Equal<Required<APTran.refNbr>>>>>,
						OrderBy<
							Desc<APTax.curyTaxAmt>>>
					.Select(Base, perUnitTax.TaxID, perUnitAggregatedTax.TranType, perUnitAggregatedTax.RefNbr)
					.AsEnumerable()
					.Select(res => (res.GetItem<APTax>(), res.GetItem<APTran>()));	
		}

		protected override GLTran CreateGLTranForPerUnitLineTax(APInvoice apInvoice, CurrencyInfo newCurrencyInfo, APTaxTran perUnitAggregatedTax,
																APTran apTran, APTax perUnitLineTax, bool isDebitTaxTran)
		{
			GLTran newGlTran = base.CreateGLTranForPerUnitLineTax(apInvoice, newCurrencyInfo, perUnitAggregatedTax,
																  apTran, perUnitLineTax, isDebitTaxTran);
			newGlTran.SummPost = Base.SummPost;
			newGlTran.BranchID = apTran.BranchID;
			newGlTran.AccountID = apTran.AccountID;
			newGlTran.SubID = apTran.SubID;
			newGlTran.TranLineNbr = apTran.LineNbr;
			newGlTran.ReferenceID = apInvoice.VendorID;
			newGlTran.ProjectID = apTran.ProjectID;
			newGlTran.TaskID = apTran.TaskID;
			newGlTran.CostCodeID = apTran.CostCodeID;

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

		protected override GLTran InsertNewGLTran(JournalEntry journalEntry, APInvoice apInvoice, APTran apTran, APTaxTran perUnitAggregatedTax,
												  APTax perUnitLineTax, GLTran newGlTran)
		{
			var glTranInsertionContext = new APReleaseProcess.GLTranInsertionContext
			{
				APRegisterRecord = apInvoice,
				APTranRecord = apTran,
				APTaxTranRecord = perUnitAggregatedTax
			};

			return Base.InsertInvoicePerUnitTaxAmountsToItemAccountsTransaction(journalEntry, newGlTran, glTranInsertionContext);
		}
	}
}

