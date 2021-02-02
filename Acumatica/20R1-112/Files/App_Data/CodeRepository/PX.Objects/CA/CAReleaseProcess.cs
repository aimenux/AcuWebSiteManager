using System;
using PX.Data;
using PX.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.TX;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.IN;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CA
{
	[PXHidden]
	public class CAReleaseProcess : PXGraph<CAReleaseProcess>
	{
		public PXSetup<CASetup> casetup;

		public PXSelectJoin<CATran, InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CATran.cashAccountID>>,
			InnerJoin<Currency, On<Currency.curyID, Equal<CashAccount.curyID>>,
			InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<CATran.curyInfoID>>,
			LeftJoin<CAAdj, On<CAAdj.tranID, Equal<CATran.tranID>>>>>>,
			Where<CATran.origModule, Equal<BatchModule.moduleCA>,
				And<CATran.origTranType, Like<Required<CATran.origTranType>>,
				And<CATran.origTranType, NotEqual<CATranType.cATransferExp>,
				And<CATran.origRefNbr, Equal<Required<CATran.origRefNbr>>,
				And<CATran.released, Equal<boolFalse>>>>>>, OrderBy<Asc<CATran.tranID>>> CATran_Ordered;

		public PXSelect<CASplit,
				Where<CASplit.adjTranType, Equal<Required<CASplit.adjTranType>>,
					And<CASplit.adjTranType, NotEqual<CATranType.cATransferExp>,
					And<CASplit.adjRefNbr, Equal<Required<CASplit.adjRefNbr>>>>>> CASplits;

		public PXSelectJoin<CATaxTran, InnerJoin<Tax, On<Tax.taxID, Equal<CATaxTran.taxID>>>, Where<CATaxTran.module, Equal<BatchModule.moduleCA>, And<CATaxTran.tranType, Equal<Required<CATaxTran.tranType>>, And<CATaxTran.refNbr, Equal<Required<CATaxTran.refNbr>>>>>, OrderBy<Asc<Tax.taxCalcLevel>>> CATaxTran_TranType_RefNbr;
		public PXSelect<SVATConversionHist> SVATConversionHistory;
		public PXSelect<CADepositEntry.ARPaymentUpdate> arDocs;
		public PXSelect<CADepositEntry.APPaymentUpdate> apDocs;
		public PXSelect<CADepositDetail> depositDetails;
		public PXSelect<CADeposit> deposit;
		public PXSelect<CAExpense, Where<CAExpense.refNbr, Equal<Required<CAExpense.refNbr>>>> CAExpense_RefNbr;
		public PXSetup<GLSetup> glsetup;

		[InjectDependency]
		public IFinPeriodUtils FinPeriodUtils { get; set; }
		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }
		public PXSelect<CAAdj> CAAdjastments;
		public PXSelect<CATransfer> CAtransfers;
		public PXSelect<CATaxTran> TaxTrans;
		public PXSelect<CAExpense> TransferExpenses;
		public PXSelect<SVATConversionHist> SVATConversionHist;
		public PXSelect<CADailySummary> DailySummary;

		public PXSelect<CADeposit> CADeposits;
		public PXSelect<CADepositDetail> CADepositDetails;
		public PXSelect<CADepositEntry.ARPaymentUpdate> ARPaymentUpdateRows;
		public PXSelect<CADepositEntry.APPaymentUpdate> APPaymentUpdateRows;

		public bool? RequireControlTaxTotal
		{
			get { return casetup.Current.RequireControlTaxTotal == true && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>(); }
		}

		public decimal? RoundingLimit
		{
			get { return glsetup.Current.RoundingLimit; }
		}



		public bool AutoPost
		{
			get
			{
				return (bool)casetup.Current.AutoPostOption;
			}
		}

		public CAReleaseProcess()
		{
			bool requireExtRefNbr = casetup.Current.RequireExtRefNbr == true;

			PXUIFieldAttribute.SetRequired<CAAdj.extRefNbr>(Caches[typeof(CAAdj)], requireExtRefNbr);
			PXDefaultAttribute.SetPersistingCheck<CAAdj.extRefNbr>(Caches[typeof(CAAdj)], null, requireExtRefNbr ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);

			PXUIFieldAttribute.SetRequired<CADeposit.extRefNbr>(Caches[typeof(CADeposit)], requireExtRefNbr);
			PXDefaultAttribute.SetPersistingCheck<CAAdj.extRefNbr>(Caches[typeof(CADeposit)], null, requireExtRefNbr ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}


		public virtual void SegregateBatch(JournalEntry je, List<Batch> batchlist, Int32? branchID, string curyID, DateTime? docDate, string finPeriodID, string description, CurrencyInfo curyInfo)
		{
			var batch = je.BatchModule.Current;

			if (batch != null)
			{
				je.Save.Press();
				if (batchlist.Contains(batch) == false)
				{
					batchlist.Add(batch);
				}
			}

			JournalEntry.SegregateBatch(je, BatchModule.CA, branchID, curyID, docDate, finPeriodID, description, curyInfo, null);
		}

		private void PostGeneralTax(
			JournalEntry je, 
			CATran catran, 
			CAAdj caadj, 
			CATaxTran x, 
			Tax salestax,
			int? accountID = null,
			int? subID = null)
		{
			bool CR = caadj.DrCr == DrCr.Credit;

			GLTran taxTran = new GLTran();
			taxTran.SummPost = false;
			taxTran.CuryInfoID = catran.CuryInfoID;
			taxTran.TranType = caadj.AdjTranType;
			taxTran.TranClass = GLTran.tranClass.Tax;
			taxTran.RefNbr = caadj.AdjRefNbr;
			taxTran.TranDate = caadj.TranDate;
			taxTran.AccountID = accountID ?? x.AccountID;
			taxTran.SubID = subID ?? x.SubID;
			taxTran.TranDesc = salestax.TaxID;
			taxTran.CuryDebitAmt = CR ? x.CuryTaxAmt : 0m;
			taxTran.DebitAmt = CR ? x.TaxAmt : 0m;
			taxTran.CuryCreditAmt = CR ? 0m : x.CuryTaxAmt;
			taxTran.CreditAmt = CR ? 0m : x.TaxAmt;
			taxTran.Released = true;
			taxTran.ReferenceID = null;
			taxTran.BranchID = caadj.BranchID;
			taxTran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
			je.GLTranModuleBatNbr.Insert(taxTran);
		}

		private void PostReverseTax(
			JournalEntry je, 
			CATran catran, 
			CAAdj caadj, 
			CATaxTran x, 
			Tax salestax)
		{
			bool CR = caadj.DrCr == DrCr.Credit;

			GLTran taxTran = new GLTran();
			taxTran.SummPost = false;
			taxTran.CuryInfoID = catran.CuryInfoID;
			taxTran.TranType = caadj.AdjTranType;
			taxTran.TranClass = GLTran.tranClass.Tax;
			taxTran.RefNbr = caadj.AdjRefNbr;
			taxTran.TranDate = caadj.TranDate;
			taxTran.AccountID = x.AccountID;
			taxTran.SubID = x.SubID;
			taxTran.TranDesc = salestax.TaxID;
			taxTran.CuryDebitAmt = CR ? 0m : x.CuryTaxAmt;
			taxTran.DebitAmt = CR ? 0m : x.TaxAmt;
			taxTran.CuryCreditAmt = CR ? x.CuryTaxAmt : 0m;
			taxTran.CreditAmt = CR ? x.TaxAmt : 0m;
			taxTran.Released = true;
			taxTran.ReferenceID = null;
			taxTran.BranchID = caadj.BranchID;
			taxTran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
			je.GLTranModuleBatNbr.Insert(taxTran);
		}

		private void PostTaxAmountByProjectKey(
			JournalEntry je,
			CATran catran,
			CAAdj caadj,
			CATaxTran caTaxTran,
			Tax tax)
		{
			bool CR = caadj.DrCr == DrCr.Credit;
			PXResultset<CATax> deductibleLines = GetDeductibleLines(tax, caTaxTran);

			APTaxAttribute apTaxAttr = new APTaxAttribute(typeof(CARegister), typeof(CATax), typeof(CATaxTran));
			apTaxAttr.DistributeTaxDiscrepancy<CATax, CATax.curyTaxAmt, CATax.taxAmt>(this, deductibleLines.FirstTableItems, caTaxTran.CuryTaxAmt.Value);

			var newTrans = new Dictionary<ProjectKey, GLTran>();
			var caTranByLineNbr = new Dictionary<int?, CASplit>();

			foreach (PXResult<CATax, CASplit> item in deductibleLines)
			{
				CATax taxLine = (CATax)item;
				CASplit split = (CASplit)item;

				int? accountID = tax.ReportExpenseToSingleAccount == true ? tax.ExpenseAccountID : split.AccountID;
				int? subID = tax.ReportExpenseToSingleAccount == true ? tax.ExpenseSubID : split.SubID;

				var projectKey = new ProjectKey(
					split.BranchID,
					accountID,
					subID,
					split.ProjectID,
					split.TaskID,
					split.CostCodeID,
					split.InventoryID);

				if (newTrans.TryGetValue(projectKey, out GLTran tran))
				{
					tran.TranLineNbr = null;
					tran.Qty = (tran.Qty ?? 0) + (split.Qty ?? 0);
					tran.CuryDebitAmt = (tran.CuryDebitAmt ?? 0) + (CR ? (taxLine.CuryTaxAmt ?? 0m) : 0m);
					tran.DebitAmt = (tran.DebitAmt ?? 0) + (CR ? (taxLine.TaxAmt ?? 0m) : 0m);
					tran.CuryCreditAmt = (tran.CuryCreditAmt ?? 0) + (CR ? 0m : (taxLine.CuryTaxAmt ?? 0m));
					tran.CreditAmt = (tran.CreditAmt ?? 0) + (CR ? 0m : (taxLine.TaxAmt ?? 0m));
				}
				else
				{
					tran = new GLTran();
					tran.SummPost = false;
					tran.BranchID = split.BranchID;
					tran.CuryInfoID = catran.CuryInfoID;
					tran.TranType = caadj.AdjTranType;
					tran.TranClass = GLTran.tranClass.Tax;
					tran.RefNbr = caadj.AdjRefNbr;
					tran.TranDate = caadj.TranDate;
					tran.AccountID = accountID;
					tran.SubID = subID;
					tran.TranDesc = tax.TaxID;
					tran.TranLineNbr = split.LineNbr;
					tran.CuryDebitAmt = CR ? taxLine.CuryTaxAmt : 0m;
					tran.DebitAmt = CR ? taxLine.TaxAmt : 0m;
					tran.CuryCreditAmt = CR ? 0m : taxLine.CuryTaxAmt;
					tran.CreditAmt = CR ? 0m : taxLine.TaxAmt;
					tran.Released = true;
					tran.ReferenceID = null;
					tran.ProjectID = split.ProjectID;
					tran.TaskID = split.TaskID;
					tran.CostCodeID = split.CostCodeID;

					tran.InventoryID = split.InventoryID;
					tran.Qty = split.Qty;
					tran.UOM = split.UOM;

					newTrans.Add(projectKey, tran);
					caTranByLineNbr.Add(split.LineNbr, split);
				}
			}

			foreach (GLTran tran in newTrans.Values)
			{
				je.GLTranModuleBatNbr.Insert(tran);
			}
		}

		private bool IsPostUseAndSalesTaxesByProjectKey(PXGraph graph, Tax tax)
		{
			bool postByProjectKey = true;

			if (tax.ReportExpenseToSingleAccount == true)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.SelectSingleBound(graph, null, tax.ExpenseAccountID);
				postByProjectKey = account?.AccountGroupID != null;
			}

			return postByProjectKey;
		}

		protected virtual PXResultset<CATax> GetDeductibleLines(Tax salestax, CATaxTran x)
		{
			return PXSelectJoin<CATax, InnerJoin<CASplit,
				On<CATax.adjTranType, Equal<CASplit.adjTranType>,
					And<CATax.adjRefNbr, Equal<CASplit.adjRefNbr>, And<CATax.lineNbr, Equal<CASplit.lineNbr>>>>>,
				Where<CATax.taxID, Equal<Required<CATax.taxID>>, And<CASplit.adjTranType, Equal<Required<CASplit.adjTranType>>,
					And<CASplit.adjRefNbr, Equal<Required<CASplit.adjRefNbr>>>>>,
				OrderBy<Desc<CATax.curyTaxAmt>>>.Select(this, salestax.TaxID, x.TranType, x.RefNbr);
		}

		public virtual IEnumerable<Batch> ReleaseDocProc<TCADocument>(JournalEntry je, ref List<Batch> batchlist, TCADocument doc)
			where TCADocument : class, ICADocument, new()
		{
			if ((bool)doc.Hold)
			{
				throw new PXException(Messages.HoldDocCanNotBeRelease);
			}
			var batches = new List<Batch>();
			var caTranIDs = new HashSet<long>();
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				GLTran rgolTranData = new GLTran();
				rgolTranData.DebitAmt = 0m;
				rgolTranData.CreditAmt = 0m;
				Currency rgol_cury = null;
				CurrencyInfo rgol_info = null;
				CurrencyInfo transit_info = null;

				CATran prev_tran = null;

				if (casetup.Current == null || casetup.Current.TransitAcctId == null || casetup.Current.TransitSubID == null)
				{
					throw new PXException(Messages.AbsenceCashTransitAccount);
				}
				Batch batch = null;

				Batch transferBatch = null;
				Batch transferBatchOut = null;
				Batch transferBatchIn = null;
				CATran transferTran = null;
				CATran transferTranOut = null;
				CATran transferTranIn = null;

				int? SourceBranchID = null;
				PXResultset<CATran> resultset = CATran_Ordered.Select(doc.DocType, doc.RefNbr);
				FinPeriodUtils.ValidateFinPeriod(resultset.AsEnumerable().Select(result => (CATran)result), typeof(OrganizationFinPeriod.cAClosed));
				foreach (PXResult<CATran, CashAccount, Currency, CurrencyInfo, CAAdj> res in resultset)
				{
					CATran catran = (CATran)res;
					CashAccount cashacct = (CashAccount)res;
					Currency cury = (Currency)res;
					CurrencyInfo info = (CurrencyInfo)res;
					CAAdj caadj = (CAAdj)res;
					if (caadj != null && caadj.TranID != null && caadj.Hold == true)
					{
						throw new PXException(Messages.HoldDocCanNotBeRelease);
					}
					if (SourceBranchID == null && catran.OrigTranType != CATranType.CATransferIn) // Type of first transaction CATransferOut or CAAdjust expected
					{
						SourceBranchID = cashacct.BranchID;
					}

					var branchID = SourceBranchID ?? cashacct.BranchID;

					bool shouldSegregate = true;

					bool isTransfer = catran.OrigTranType == CATranType.CATransferIn || catran.OrigTranType == CATranType.CATransferOut;

					if (isTransfer && branchID == transferBatch?.BranchID &&
						ShouldSegregate(catran, transferTran))
					{
						je.BatchModule.Current = transferBatch;
						shouldSegregate = false;
					}

					if (shouldSegregate)
					{
						SegregateBatch(je, batchlist, branchID, catran.CuryID, catran.TranDate, catran.FinPeriodID, catran.TranDesc, info);
					}

					batch = (Batch)je.BatchModule.Current;

					var splits = CASplits.Select(caadj.AdjTranType, caadj.AdjRefNbr).RowCast<CASplit>().ToList();

					if (splits.Any() == false)
					{
						var casplit = new CASplit();

						casplit.AdjTranType = catran.OrigTranType;
						casplit.CuryInfoID = catran.CuryInfoID;
						casplit.CuryTranAmt = (catran.DrCr == CADrCr.CADebit ? catran.CuryTranAmt : -1m * catran.CuryTranAmt);
						casplit.TranAmt = (catran.DrCr == CADrCr.CADebit ? catran.TranAmt : -1m * catran.TranAmt);
						casplit.TranDesc = PXMessages.LocalizeNoPrefix(Messages.Offset);

						casplit.AccountID = casetup.Current.TransitAcctId;
						casplit.SubID = casetup.Current.TransitSubID;
						casplit.ReclassificationProhibited = true;
						casplit.BranchID = SourceBranchID ?? cashacct.BranchID;

						switch (casplit.AdjTranType)
						{
							case CATranType.CATransferOut:
								transit_info = PXCache<CurrencyInfo>.CreateCopy(info);
								transit_info.CuryInfoID = null;
								transit_info = je.currencyinfo.Insert(transit_info);
								transit_info.BaseCalc = false;

								casplit.CuryInfoID = transit_info.CuryInfoID;
								break;
							case CATranType.CATransferIn:
								rgol_cury = cury;
								rgol_info = info;
								rgolTranData.TranDate = catran.TranDate;
								rgolTranData.BranchID = cashacct.BranchID;
								rgolTranData.TranDesc = catran.TranDesc;
								FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, rgolTranData, catran.TranPeriodID);

								if (string.Equals(info.CuryID, transit_info.CuryID))
								{
									casplit.CuryInfoID = transit_info.CuryInfoID;
								}
								break;
						}

						rgolTranData.DebitAmt += (catran.DrCr == DrCr.Debit ? 0m : casplit.TranAmt);
						rgolTranData.CreditAmt += (catran.DrCr == DrCr.Debit ? casplit.TranAmt : 0m);

						splits.Add(casplit);
					}

					if (object.Equals(prev_tran, catran) == false)
					{
						GLTran documentTran = new GLTran();
						documentTran.SummPost = false;
						documentTran.CuryInfoID = catran.CuryInfoID;
						documentTran.TranType = catran.OrigTranType;
						documentTran.RefNbr = catran.OrigRefNbr;
						documentTran.ReferenceID = catran.ReferenceID;
						documentTran.AccountID = cashacct.AccountID;
						documentTran.SubID = cashacct.SubID;
						documentTran.CATranID = catran.TranID;
						documentTran.TranDate = catran.TranDate;
						documentTran.BranchID = cashacct.BranchID;
						FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, documentTran, catran.TranPeriodID);
						documentTran.CuryDebitAmt = (catran.DrCr == DrCr.Debit ? catran.CuryTranAmt : 0m);
						documentTran.DebitAmt = (catran.DrCr == DrCr.Debit ? catran.TranAmt : 0m);
						documentTran.CuryCreditAmt = (catran.DrCr == DrCr.Debit ? 0m : -1m * catran.CuryTranAmt);
						documentTran.CreditAmt = (catran.DrCr == DrCr.Debit ? 0m : -1m * catran.TranAmt);
						documentTran.TranDesc = catran.TranDesc;
						documentTran.Released = true;
						documentTran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
						InsertDocumentTransaction(je, documentTran, new GLTranInsertionContext() { CATranRecord = catran });

						var docInclTaxDiscrepancy = 0.0m;
						foreach (PXResult<CATaxTran, Tax> r in CATaxTran_TranType_RefNbr.Select(caadj.AdjTranType, caadj.AdjRefNbr))
						{
							CATaxTran x = (CATaxTran)r;
							Tax salestax = (Tax)r;

							if (caadj.TaxCalcMode == TaxCalculationMode.Gross || salestax.TaxCalcLevel == CSTaxCalcLevel.Inclusive && caadj.TaxCalcMode != TaxCalculationMode.Net)
								docInclTaxDiscrepancy += ((x.CuryTaxAmt ?? 0.0m) - (x.CuryTaxAmtSumm ?? 0.0m)) * (salestax.ReverseTax == true ? -1.0M : 1.0M);

							if (salestax.TaxType == CSTaxType.Withholding)
							{
								continue;
							}

							if (caadj.DrCr == DrCr.Credit && (salestax.TaxType == CSTaxType.Use || salestax.TaxType == CSTaxType.Sales))
							{
								if (IsPostUseAndSalesTaxesByProjectKey(this, salestax))
								{
									PostTaxAmountByProjectKey(je, catran, caadj, x, salestax);
								}
								else
								{
									PostGeneralTax(je, catran, caadj, x, salestax, salestax.ExpenseAccountID, salestax.ExpenseSubID);
								}

								if (salestax.TaxType == CSTaxType.Use)
								{
									PostReverseTax(je, catran, caadj, x, salestax);
								}
							}
							else
							{
								if (salestax.ReverseTax != true)
								{
									PostGeneralTax(je, catran, caadj, x, salestax);
								}
								else
								{
									PostReverseTax(je, catran, caadj, x, salestax);
								}

								if (salestax.DeductibleVAT == true)
								{
									if (salestax.ReportExpenseToSingleAccount == true)
									{
										GLTran tran = new GLTran();
										tran.SummPost = false;
										tran.BranchID = caadj.BranchID;
										tran.CuryInfoID = catran.CuryInfoID;
										tran.TranType = caadj.AdjTranType;
										tran.TranClass = GLTran.tranClass.Tax;
										tran.RefNbr = caadj.AdjRefNbr;
										tran.TranDate = caadj.TranDate;
										tran.AccountID = salestax.ExpenseAccountID;
										tran.SubID = salestax.ExpenseSubID;
										tran.TranDesc = salestax.TaxID;
										bool CR = (caadj.DrCr == DrCr.Credit);
										tran.CuryDebitAmt = CR ? x.CuryExpenseAmt : 0m;
										tran.DebitAmt = CR ? x.ExpenseAmt : 0m;
										tran.CuryCreditAmt = CR ? 0m : x.CuryExpenseAmt;
										tran.CreditAmt = CR ? 0m : x.ExpenseAmt;
										tran.Released = true;
										tran.ReferenceID = null;
										tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();

										je.GLTranModuleBatNbr.Insert(tran);
									}
									else if (salestax.TaxCalcType == CSTaxCalcType.Item)
									{
										PXResultset<CATax> deductibleLines = GetDeductibleLines(salestax, x);

										APTaxAttribute apTaxAttr = new APTaxAttribute(typeof(CARegister), typeof(CATax), typeof(CATaxTran));
										apTaxAttr.DistributeTaxDiscrepancy<CATax, CATax.curyExpenseAmt, CATax.expenseAmt>(this, deductibleLines.FirstTableItems, x.CuryExpenseAmt.Value);

										foreach (PXResult<CATax, CASplit> item in deductibleLines)
										{
											CATax taxLine = (CATax)item;
											CASplit split = (CASplit)item;

											GLTran tran = new GLTran();
											tran.SummPost = false;
											tran.BranchID = split.BranchID;
											tran.CuryInfoID = catran.CuryInfoID;
											tran.TranType = caadj.AdjTranType;
											tran.TranClass = GLTran.tranClass.Tax;
											tran.RefNbr = caadj.AdjRefNbr;
											tran.TranDate = caadj.TranDate;
											tran.AccountID = split.AccountID;
											tran.SubID = split.SubID;
											tran.TranDesc = salestax.TaxID;
											tran.TranLineNbr = split.LineNbr;
											bool CR = (caadj.DrCr == DrCr.Credit);
											tran.CuryDebitAmt = CR ? taxLine.CuryExpenseAmt : 0m;
											tran.DebitAmt = CR ? taxLine.ExpenseAmt : 0m;
											tran.CuryCreditAmt = CR ? 0m : taxLine.CuryExpenseAmt;
											tran.CreditAmt = CR ? 0m : taxLine.ExpenseAmt;
											tran.Released = true;
											tran.ReferenceID = null;
											tran.ProjectID = split.ProjectID;
											tran.TaskID = split.TaskID;
											tran.CostCodeID = split.CostCodeID;

											je.GLTranModuleBatNbr.Insert(tran);
										}
									}
								}
							}

							x.Released = true;
							CATaxTran_TranType_RefNbr.Update(x);

							if (PXAccess.FeatureInstalled<FeaturesSet.vATReporting>() &&
								(x.TaxType == TaxType.PendingPurchase || x.TaxType == TaxType.PendingSales))
							{
								decimal mult = ReportTaxProcess.GetMultByTranType(BatchModule.CA, x.TranType);
								SVATConversionHist histSVAT = new SVATConversionHist
								{
									Module = BatchModule.CA,
									AdjdBranchID = x.BranchID,
									AdjdDocType = x.TranType,
									AdjdRefNbr = x.RefNbr,
									AdjgDocType = x.TranType,
									AdjgRefNbr = x.RefNbr,
									AdjdDocDate = caadj.TranDate,
									AdjdFinPeriodID = caadj.FinPeriodID,

									TaxID = x.TaxID,
									TaxType = x.TaxType,
									TaxRate = x.TaxRate,
									VendorID = x.VendorID,
									ReversalMethod = SVATTaxReversalMethods.OnDocuments,

									CuryInfoID = x.CuryInfoID,
									CuryTaxableAmt = x.CuryTaxableAmt * mult,
									CuryTaxAmt = x.CuryTaxAmt * mult,
									CuryUnrecognizedTaxAmt = x.CuryTaxAmt * mult
								};

								histSVAT.FillBaseAmounts(SVATConversionHistory.Cache);
								SVATConversionHistory.Cache.Insert(histSVAT);
							}
						}
						if (docInclTaxDiscrepancy != 0)
						{
							ProcessTaxDiscrepancy(je, batch, caadj, info, docInclTaxDiscrepancy);
						}
					}

					foreach (var casplit in splits)
					{
						PXResultset<CATax> taxes = PXSelectJoin<CATax, LeftJoin<Tax, On<Tax.taxID, Equal<CATax.taxID>>>,
							Where<CATax.adjTranType, Equal<Required<CATax.adjTranType>>,
								And<CATax.adjRefNbr, Equal<Required<CATax.adjRefNbr>>,
									And<CATax.lineNbr, Equal<Required<CATax.lineNbr>>>>>>.Select(this, casplit.AdjTranType, casplit.AdjRefNbr, casplit.LineNbr);

						//sorting on joined tables' fields does not work!
						IComparer<Tax> taxComparer = GetTaxComparer();
						taxComparer.ThrowOnNull(nameof(taxComparer));
						taxes.Sort((PXResult<CATax> x, PXResult<CATax> y) => taxComparer.Compare(x.GetItem<Tax>(), y.GetItem<Tax>()));

						Tax firstTax = taxes.Count != 0 ? taxes[0].GetItem<Tax>() : null;

						if (firstTax != null && firstTax.TaxID != null)
						{
							AdjustTaxCalculationLevelForNetGrossEntryMode(caadj, ref firstTax);
						}

						if (firstTax != null && firstTax.TaxCalcType == CSTaxCalcType.Item && PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>())
						{
							string TaxCalcMode = caadj.TaxCalcMode;
							switch (TaxCalcMode)
							{
								case TaxCalculationMode.Gross:
									firstTax.TaxCalcLevel = CSTaxCalcLevel.Inclusive;
									break;
								case TaxCalculationMode.Net:
									firstTax.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
									break;
								case TaxCalculationMode.TaxSetting:
									break;
							}
						}
						GLTran splitTran = new GLTran();
						splitTran.SummPost = (catran.OrigTranType == CATranType.CATransferIn || catran.OrigTranType == CATranType.CATransferOut);
						splitTran.ZeroPost = (catran.OrigTranType == CATranType.CATransferIn || catran.OrigTranType == CATranType.CATransferOut) ? (bool?)false : null;
						splitTran.CuryInfoID = casplit.CuryInfoID;
						splitTran.TranType = catran.OrigTranType;
						splitTran.RefNbr = catran.OrigRefNbr;

						splitTran.InventoryID = casplit.InventoryID;
						splitTran.UOM = casplit.UOM;
						splitTran.Qty = casplit.Qty;
						splitTran.AccountID = casplit.AccountID;
						splitTran.SubID = casplit.SubID;
						splitTran.ReclassificationProhibited = casplit.ReclassificationProhibited ?? false;
						splitTran.CATranID = null;
						splitTran.TranDate = catran.TranDate;
						splitTran.BranchID = casplit.BranchID;
						FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, splitTran, catran.TranPeriodID);
						splitTran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
						if (firstTax != null && firstTax.TaxCalcLevel == CSTaxCalcLevel.Inclusive && firstTax.TaxType != CSTaxType.Withholding)
						{
							splitTran.CuryDebitAmt = (catran.DrCr == DrCr.Debit ? 0m : casplit.CuryTaxableAmt);
							splitTran.DebitAmt = (catran.DrCr == DrCr.Debit ? 0m : casplit.TaxableAmt);
							splitTran.CuryCreditAmt = (catran.DrCr == DrCr.Debit ? casplit.CuryTaxableAmt : 0m);
							splitTran.CreditAmt = (catran.DrCr == DrCr.Debit ? casplit.TaxableAmt : 0m);
						}
						else
						{
							splitTran.CuryDebitAmt = (catran.DrCr == DrCr.Debit ? 0m : casplit.CuryTranAmt);
							splitTran.DebitAmt = (catran.DrCr == DrCr.Debit ? 0m : casplit.TranAmt);
							splitTran.CuryCreditAmt = (catran.DrCr == DrCr.Debit ? casplit.CuryTranAmt : 0m);
							splitTran.CreditAmt = (catran.DrCr == DrCr.Debit ? casplit.TranAmt : 0m);
						}
						splitTran.TranDesc = casplit.TranDesc;
						splitTran.ProjectID = casplit.ProjectID;
						splitTran.TaskID = casplit.TaskID;
						splitTran.CostCodeID = casplit.CostCodeID;
						splitTran.TranLineNbr = splitTran.SummPost == true ? null : casplit.LineNbr;
						splitTran.NonBillable = casplit.NonBillable;
						splitTran.Released = true;
						InsertSplitTransaction(je, splitTran, new GLTranInsertionContext() { CASplitRecord = casplit, CATranRecord = catran });
					}

					prev_tran = catran;

					if (rgol_cury != null && rgol_info != null && Math.Abs(Math.Round((decimal)(rgolTranData.DebitAmt - rgolTranData.CreditAmt), 4)) >= 0.00005m)
					{
						batch = (Batch)je.BatchModule.Current;

						CurrencyInfo new_info = PXCache<CurrencyInfo>.CreateCopy(rgol_info);
						new_info.CuryInfoID = null;
						new_info = je.currencyinfo.Insert(new_info);

						GLTran rgolTran = new GLTran();
						rgolTran.SummPost = false;
						if (Math.Sign((decimal)(rgolTranData.DebitAmt - rgolTranData.CreditAmt)) == 1)
						{
							rgolTran.AccountID = rgol_cury.RealLossAcctID;
							rgolTran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgolTranData.BranchID, rgol_cury);
							rgolTran.DebitAmt = Math.Round((decimal)(rgolTranData.DebitAmt - rgolTranData.CreditAmt), 4);
							rgolTran.CuryDebitAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) ? rgolTran.DebitAmt : 0m; //non-zero for base cury
							rgolTran.CreditAmt = 0m;
							rgolTran.CuryCreditAmt = 0m;
						}
						else
						{
							rgolTran.AccountID = rgol_cury.RealGainAcctID;
							rgolTran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgolTranData.BranchID, rgol_cury);
							rgolTran.DebitAmt = 0m;
							rgolTran.CuryDebitAmt = 0m;
							rgolTran.CreditAmt = Math.Round((decimal)(rgolTranData.CreditAmt - rgolTranData.DebitAmt), 4);
							rgolTran.CuryCreditAmt = object.Equals(new_info.CuryID, new_info.BaseCuryID) ? rgolTran.CreditAmt : 0m; //non-zero for base cury
						}
						rgolTran.TranType = CATranType.CATransferRGOL;
						rgolTran.RefNbr = doc.RefNbr;
						rgolTran.TranDesc = "RGOL";
						rgolTran.TranDate = rgolTranData.TranDate;
						rgolTran.BranchID = rgolTranData.BranchID;
						FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, rgolTran, rgolTranData.TranPeriodID);
						rgolTran.Released = true;
						rgolTran.CuryInfoID = new_info.CuryInfoID;
						rgolTran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
						je.GLTranModuleBatNbr.Insert(rgolTran);

						rgolTran.AccountID = casetup.Current.TransitAcctId;
						rgolTran.SubID = casetup.Current.TransitSubID;
						rgolTran.BranchID = SourceBranchID ?? cashacct.BranchID;

						decimal? CuryAmount = rgolTran.CuryDebitAmt;
						decimal? BaseAmount = rgolTran.DebitAmt;
						rgolTran.CuryDebitAmt = rgolTran.CuryCreditAmt;
						rgolTran.DebitAmt = rgolTran.CreditAmt;
						rgolTran.CuryCreditAmt = CuryAmount;
						rgolTran.CreditAmt = BaseAmount;

						je.GLTranModuleBatNbr.Insert(rgolTran);

						rgolTranData = new GLTran();
						rgolTranData.DebitAmt = 0m;
						rgolTranData.CreditAmt = 0m;
						rgol_cury = null;
						rgol_info = null;
					}

					if (caadj.AdjTranType == CATranType.CAAdjustment && Math.Abs(Math.Round((decimal)(batch.CuryDebitTotal - batch.CuryCreditTotal), 4)) >= 0.00005m)
					{
						if (this.RequireControlTaxTotal != true)
						{
							throw new PXException(Messages.DocumentOutOfBalance);
						}
						decimal roundDiff = Math.Abs(Math.Round((decimal)(batch.DebitTotal - batch.CreditTotal), 4));
						if (roundDiff > this.RoundingLimit)
						{
							throw new PXException(AP.Messages.RoundingAmountTooBig, je.currencyinfo.Current.BaseCuryID, roundDiff,
								PXDBQuantityAttribute.Round(this.RoundingLimit));
						}
						GLTran tran = new GLTran();
						tran.SummPost = true;
						tran.BranchID = caadj.BranchID;
						Currency c = PXSelect<Currency, Where<Currency.curyID, Equal<Required<Currency.curyID>>>>.Select(this, caadj.CuryID);

						if (c.RoundingGainAcctID == null || c.RoundingGainSubID == null)
						{
							throw new PXException(AP.Messages.NoRoundingGainLossAccSub, c.CuryID);
						}

						if (Math.Sign((decimal)(batch.CuryDebitTotal - batch.CuryCreditTotal)) == 1)
						{
							tran.AccountID = c.RoundingGainAcctID;
							tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, tran.BranchID, c);
							tran.CuryCreditAmt = Math.Round((decimal)(batch.CuryDebitTotal - batch.CuryCreditTotal), 4);
							tran.CuryDebitAmt = 0m;
						}
						else
						{
							tran.AccountID = c.RoundingLossAcctID;
							tran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, tran.BranchID, c);
							tran.CuryCreditAmt = 0m;
							tran.CuryDebitAmt = Math.Round((decimal)(batch.CuryCreditTotal - batch.CuryDebitTotal), 4);
						}
						tran.CreditAmt = 0m;
						tran.DebitAmt = 0m;
						tran.TranType = doc.DocType;
						tran.RefNbr = doc.RefNbr;
						tran.TranClass = GLTran.tranClass.Normal;
						tran.TranDesc = GL.Messages.RoundingDiff;
						tran.LedgerID = batch.LedgerID;
						FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, batch.TranPeriodID);
						tran.TranDate = batch.DateEntered;
						tran.Released = true;
						tran.ProjectID = PM.ProjectDefaultAttribute.NonProject();

						CurrencyInfo infocopy = new CurrencyInfo();
						infocopy = je.currencyinfo.Insert(infocopy) ?? infocopy;

						tran.CuryInfoID = infocopy.CuryInfoID;
						InsertRoundingTransaction(je, tran, 
							new GLTranInsertionContext()
							{
								CAAdjRecord = doc as CAAdj,
								CATransferRecord = doc as CATransfer,
								CATranRecord = catran
							});
					}

					if (batch != null && batch.CuryCreditTotal == batch.CuryDebitTotal)
					{
						//in normal case this happens on moving to next CATran
						AddRoundingTran(je, doc, batch, cury);

						je.Save.Press();

						if (isTransfer)
						{
							transferTran = catran;
							transferBatch = je.BatchModule.Current;
						}

						if (catran.OrigTranType == CATranType.CATransferOut)
						{
							transferTranOut = catran;
							transferBatchOut = je.BatchModule.Current;
						}

						if (catran.OrigTranType == CATranType.CATransferIn)
						{
							transferTranIn = catran;
							transferBatchIn = je.BatchModule.Current;
						}

						Batch expBatch = null;

						if (catran.OrigTranType == CATranType.CATransferIn)
						{
							expBatch = CreateGLTransFromCAExpencesForCATransfer(je, batchlist, doc, transferBatchOut, transferTranOut, transferBatchIn, transferTranIn);
						}

						if (batches.FirstOrDefault(b => b.BatchNbr == batch.BatchNbr) == null)
						{
							batches.Add(batch);
						}

						if (expBatch != null && batches.FirstOrDefault(b => b.BatchNbr == expBatch?.BatchNbr) == null)
						{
							batches.Add(expBatch);
						}

						if (batchlist.Find(_ => je.BatchModule.Cache.ObjectsEqual(_, batch)) == null)
						{
							batchlist.Add(batch);
						}

						doc.Released = true;
						if (catran.TranID.HasValue && catran.CuryTranAmt != 0 && catran.TranAmt != 0)
						{
							caTranIDs.Add(catran.TranID.Value);
						}
						Caches[typeof(TCADocument)].Update(doc);

						if (Caches[typeof(TCADocument)].ObjectsEqual(doc, caadj) == false)
						{
							if (caadj.AdjRefNbr != null)
							{
								caadj.Released = true;
								Caches[typeof(CAAdj)].Update(caadj);
							}
						}
					}
					else
					{
						throw new PXException(Messages.DocumentOutOfBalance);
					}
				}

				doc.Released = true;
				Caches[typeof(TCADocument)].Update(doc);

				Actions.PressSave();

				CheckMultipleGLPosting(caTranIDs);
				OnReleaseComplete(doc);
				ts.Complete(this);
			}
			return batches;
		}

		protected virtual void ProcessTaxDiscrepancy(
			JournalEntry je,
			Batch arbatch,
			CAAdj doc,
			CurrencyInfo currencyInfo,
			decimal docInclTaxDiscrepancy)
		{
			if (docInclTaxDiscrepancy != 0)
			{
				TXSetup txsetup = PXSetup<TXSetup>.Select(this);
				if (txsetup?.TaxRoundingGainAcctID == null || txsetup?.TaxRoundingLossAcctID == null)
				{
					throw new PXException(TX.Messages.TaxRoundingGainLossAccountsRequired);
				}

				var isDebit = (doc.DrCr == DrCr.Debit);
				var roundAcctID = isDebit == docInclTaxDiscrepancy > 0 ? txsetup.TaxRoundingLossAcctID : txsetup.TaxRoundingGainAcctID;
				var roundSubID = isDebit == docInclTaxDiscrepancy > 0 ? txsetup.TaxRoundingLossSubID : txsetup.TaxRoundingGainSubID;

				GLTran diffTran = new GLTran();
				diffTran.SummPost = false;
				diffTran.BranchID = doc.BranchID;
				diffTran.CuryInfoID = currencyInfo.CuryInfoID;
				diffTran.TranType = doc.DocType;
				diffTran.TranClass = GLTran.tranClass.Tax;
				diffTran.RefNbr = doc.RefNbr;
				diffTran.TranDate = doc.DocDate;
				diffTran.AccountID = roundAcctID;
				diffTran.SubID = roundSubID;
				diffTran.TranDesc = TX.Messages.DocumentInclusiveTaxDiscrepancy;
				diffTran.CuryDebitAmt = isDebit ? docInclTaxDiscrepancy : 0m;
				diffTran.DebitAmt = isDebit ? docInclTaxDiscrepancy : 0m;
				diffTran.CuryCreditAmt = isDebit ? 0m : docInclTaxDiscrepancy;
				diffTran.CreditAmt = isDebit ? 0m : docInclTaxDiscrepancy;
				diffTran.Released = true;
				diffTran.ReferenceID = null;

				je.GLTranModuleBatNbr.Insert(diffTran);
			}
		}

		public static void AdjustTaxCalculationLevelForNetGrossEntryMode(CAAdj document, ref Tax taxCorrespondingToLine)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.netGrossEntryMode>())
			{
				string documentTaxCalculationMode = document.TaxCalcMode;

				switch (documentTaxCalculationMode)
				{
					case TaxCalculationMode.Gross:
						taxCorrespondingToLine.TaxCalcLevel = CSTaxCalcLevel.Inclusive;
						break;
					case TaxCalculationMode.Net:
						taxCorrespondingToLine.TaxCalcLevel = CSTaxCalcLevel.CalcOnItemAmt;
						break;
					case TaxCalculationMode.TaxSetting:
					default:
						break;
				}
			}
		}


		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		protected virtual void OnReleaseComplete(ICADocument doc) 
		{

		}

		private Batch CreateGLTransFromCAExpencesForCATransfer<TCADocument>(JournalEntry je, List<Batch> batchlist, TCADocument doc, Batch transferBatchOut, CATran transferTranOut, Batch transferBatchIn, CATran transferTranIn)
			where TCADocument : class, ICADocument, new()
		{
			var currentBatch = (Batch)je.BatchModule.Current;
			Batch resultBatch = null;
			bool isNeedPersisting = false;

			foreach (CAExpense charge in CAExpense_RefNbr.Select(doc.RefNbr))
			{
				bool shouldSegregateWithOut = true;
				bool shouldSegregateWithIn = true;
				bool consolidatedPostingByAccounts = charge.CashAccountID == transferTranOut.CashAccountID
												|| charge.CashAccountID == transferTranIn.CashAccountID;

				if (ShouldSegregate(transferBatchOut.BranchID, transferTranOut, charge))
				{
					je.BatchModule.Current = transferBatchOut;
					shouldSegregateWithOut = false;
				}

				if (ShouldSegregate(transferBatchIn.BranchID, transferTranIn, charge))
				{
					je.BatchModule.Current = transferBatchIn;
					shouldSegregateWithIn = false;
				}

				if (shouldSegregateWithOut && shouldSegregateWithIn || !consolidatedPostingByAccounts)
				{
					CurrencyInfo info = PXSelect<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CAExpense.curyInfoID>>>>.Select(je, charge.CuryInfoID);
					SegregateBatch(je, batchlist, charge.BranchID, charge.CuryID, charge.TranDate, charge.FinPeriodID, charge.TranDesc, info);
				}

				resultBatch = je.BatchModule.Current;
				GenerateGLTranPairForExpense(je, charge);

				charge.Released = true;
				SetAdjReleased(charge);

				CAExpense_RefNbr.Update(charge);

				isNeedPersisting = true;
			}

			if (isNeedPersisting)
			{
				je.Save.Press();
			}

			je.BatchModule.Current = currentBatch;

			return resultBatch;
		}

		private static bool ShouldSegregate(CATran tranOut, CATran tranIn)
		{
			return tranOut?.CuryID == tranIn?.CuryID &&
					tranOut?.TranPeriodID == tranIn?.TranPeriodID &&
					tranOut?.TranDate == tranIn?.TranDate;
		}

		private static bool ShouldSegregate(int? BranchID, CATran transferTran, CAExpense charge)
		{
			return charge.BranchID == BranchID &&
					charge.CuryID == transferTran?.CuryID &&
					charge.TranPeriodID == transferTran?.TranPeriodID &&
					charge.TranDate == transferTran?.TranDate;
		}

		[Obsolete(PX.Objects.Common.InternalMessages.ObsoleteToBeRemovedIn2019r2)]
		private void SetAdjReleased(CAExpense charge)
		{
			if (string.IsNullOrEmpty(charge.AdjRefNbr))
			{
				return;
			}

			CAAdj adj = PXSelect<CAAdj, Where<CAAdj.adjRefNbr, Equal<Required<CAExpense.adjRefNbr>>>>.Select(this, charge.AdjRefNbr);

			adj.Released = true;
			Caches[typeof(CAAdj)].Update(adj);
		}

		private void GenerateGLTranPairForExpense(JournalEntry je, CAExpense charge)
		{
			bool isCADebit = charge.DrCr == GL.DrCr.Debit;

			CashAccount expCashcacount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(this, charge.CashAccountID);

			GLTran tran = new GLTran();
			tran.SummPost = true;
			tran.BranchID = charge.BranchID;
			tran.AccountID = expCashcacount.AccountID;
			tran.SubID = expCashcacount.SubID;
			tran.CuryDebitAmt = isCADebit ? charge.CuryTranAmt : 0m;
			tran.DebitAmt = isCADebit ? charge.TranAmt : 0m;
			tran.CuryCreditAmt = isCADebit ? 0m : charge.CuryTranAmt;
			tran.CreditAmt = isCADebit ? 0m : charge.TranAmt;
			tran.TranType = charge.DocType;
			tran.RefNbr = charge.RefNbr;
			tran.TranDesc = charge.TranDesc;
			tran.TranDate = charge.TranDate;
			FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, charge.TranPeriodID);
			tran.CuryInfoID = charge.CuryInfoID;
			tran.Released = true;
			tran.CATranID = charge.CashTranID;

			je.GLTranModuleBatNbr.Insert(tran);

			tran = new GLTran();
			tran.SummPost = true;
			tran.ZeroPost = false;
			tran.BranchID = charge.BranchID;
			tran.AccountID = charge.AccountID;
			tran.SubID = charge.SubID;
			tran.CuryDebitAmt = isCADebit ? 0m : charge.CuryTranAmt;
			tran.DebitAmt = isCADebit ? 0m : charge.TranAmt;
			tran.CuryCreditAmt = isCADebit ? charge.CuryTranAmt : 0m;
			tran.CreditAmt = isCADebit ? charge.TranAmt : 0m;
			tran.TranType = charge.DocType;
			tran.RefNbr = charge.RefNbr;
			tran.TranDesc = charge.TranDesc;
			tran.TranDate = charge.TranDate;
			FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, charge.TranPeriodID);
			tran.CuryInfoID = charge.CuryInfoID;
			tran.Released = true;

			je.GLTranModuleBatNbr.Insert(tran);
		}

		private void AddRoundingTran(JournalEntry je, ICADocument doc, Batch batch, Currency cury)
		{
			if (Math.Abs(Math.Round((decimal)(batch.DebitTotal - batch.CreditTotal), 4)) >= 0.00005m)
			{
				GLTran roundingTran = new GLTran();
				roundingTran.SummPost = true;
				roundingTran.ZeroPost = false;

				if (Math.Sign((decimal)(batch.DebitTotal - batch.CreditTotal)) == 1)
				{
					roundingTran.AccountID = cury.RoundingGainAcctID;
					roundingTran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingGainSubID>(je, batch.BranchID, cury);
					roundingTran.CreditAmt = Math.Round((decimal)(batch.DebitTotal - batch.CreditTotal), 4);
					roundingTran.DebitAmt = 0m;
				}
				else
				{
					roundingTran.AccountID = cury.RoundingLossAcctID;
					roundingTran.SubID = GainLossSubAccountMaskAttribute.GetSubID<Currency.roundingLossSubID>(je, batch.BranchID, cury);
					roundingTran.CreditAmt = 0m;
					roundingTran.DebitAmt = Math.Round((decimal)(batch.CreditTotal - batch.DebitTotal), 4);
				}
				roundingTran.CuryCreditAmt = 0m;
				roundingTran.CuryDebitAmt = 0m;
				roundingTran.TranType = doc.DocType;
				roundingTran.RefNbr = doc.RefNbr;
				roundingTran.TranClass = GLTran.tranClass.Normal;
				roundingTran.TranDesc = GL.Messages.RoundingDiff;
				roundingTran.LedgerID = batch.LedgerID;
				roundingTran.BranchID = batch.BranchID;
				FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, roundingTran, batch.TranPeriodID);
				roundingTran.TranDate = batch.DateEntered;
				roundingTran.Released = true;
				roundingTran.ProjectID = PM.ProjectDefaultAttribute.NonProject();
				CurrencyInfo infocopy = new CurrencyInfo();
				infocopy = je.currencyinfo.Insert(infocopy) ?? infocopy;

				roundingTran.CuryInfoID = infocopy.CuryInfoID;
				InsertRoundingTransaction(je, roundingTran,
							new GLTranInsertionContext()
							{
								CAAdjRecord = doc as CAAdj,
								CATransferRecord = doc as CATransfer
							});
			}
		}

		private bool IsVoidedEntryWithoutPair(CADepositDetail detail)
		{
			bool isEntryVoided = PXSelect<ARRegister,
						   Where<ARRegister.voided, Equal<True>,
						   And<ARRegister.docType, Equal<Required<ARRegister.docType>>,
						   And<ARRegister.refNbr, Equal<Required<ARRegister.refNbr>>
						   >>>>.Select(this, detail.OrigDocType, detail.OrigRefNbr).Count != 0;

			if (isEntryVoided == false)
			{
				return false;
			}

			var reverceDocType = ARPaymentType.GetVoidingARDocType(detail.OrigDocType);

			var resultset = PXSelectJoin<CATran,
				InnerJoin<CADepositDetail, On<CADepositDetail.tranType, Equal<CATran.origTranType>,
							And<CADepositDetail.refNbr, Equal<CATran.origRefNbr>,
							And<CADepositDetail.tranID, Equal<CATran.tranID>>>>>,
				Where<CATran.origModule, Equal<BatchModule.moduleCA>,
				And<CATran.origTranType, Equal<Required<CATran.origTranType>>,
				And<CATran.origRefNbr, Equal<Required<CATran.origRefNbr>>,
				And<CADepositDetail.origModule, Equal<BatchModule.moduleAR>,
				And<CADepositDetail.origDocType, Equal<Required<CADepositDetail.origDocType>>,
				And<CADepositDetail.origRefNbr, Equal<Required<CADepositDetail.origRefNbr>>>>>>>>>.Select(this, detail.TranType, detail.RefNbr, reverceDocType, detail.OrigRefNbr);

			if(resultset.Count == 1)
			{
				return false;
			}

			return true;
		}

		public virtual void ReleaseDeposit(JournalEntry je, ref List<Batch> batchlist, CADeposit doc)
		{
			je.Clear();

			Currency rgol_cury = null;
			CurrencyInfo rgol_info = null;
			Currency cury = null;
			Dictionary<int, GLTran> rgols = new Dictionary<int, GLTran>();
			GLTran tran;
			CATran prev_tran = null;
			Batch batch = CreateGLBatch(je, doc);

			var caTranIDs = new HashSet<long>();

			PXSelectBase<CATran> select = new PXSelectJoin<CATran,
									InnerJoin<CashAccount, On<CashAccount.cashAccountID, Equal<CATran.cashAccountID>>,
									InnerJoin<Currency, On<Currency.curyID, Equal<CashAccount.curyID>>,
									InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<CATran.curyInfoID>>,
									LeftJoin<CADepositDetail, On<CADepositDetail.tranType, Equal<CATran.origTranType>,
												And<CADepositDetail.refNbr, Equal<CATran.origRefNbr>,
												And<CADepositDetail.tranID, Equal<CATran.tranID>>>>,
									LeftJoin<CADepositEntry.ARPaymentUpdate, On<CADepositEntry.ARPaymentUpdate.docType, Equal<CADepositDetail.origDocType>,
													And<CADepositEntry.ARPaymentUpdate.refNbr, Equal<CADepositDetail.origRefNbr>,
													And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAR>>>>,
									LeftJoin<CADepositEntry.APPaymentUpdate, On<CADepositEntry.APPaymentUpdate.docType, Equal<CADepositDetail.origDocType>,
													And<CADepositEntry.APPaymentUpdate.refNbr, Equal<CADepositDetail.origRefNbr>,
													And<CADepositDetail.origModule, Equal<GL.BatchModule.moduleAP>>>>>>>>>>,
									Where<CATran.origModule, Equal<BatchModule.moduleCA>,
									And<CATran.origTranType, Equal<Required<CATran.origTranType>>,
									And<CATran.origRefNbr, Equal<Required<CATran.origRefNbr>>>>>,
									OrderBy<Asc<CATran.tranID>>>(this);

			PXResultset<CATran> resultset = select.Select(doc.DocType, doc.RefNbr);
			FinPeriodUtils.ValidateFinPeriod(resultset.AsEnumerable().Select(result => (CATran)result), typeof(OrganizationFinPeriod.cAClosed));

			foreach (PXResult<CATran, CashAccount, Currency, CurrencyInfo, CADepositDetail, CADepositEntry.ARPaymentUpdate, CADepositEntry.APPaymentUpdate> res in resultset)
			{
				CATran catran = (CATran)res;
				CashAccount cashacct = (CashAccount)res;
				cury = (Currency)res;
				CurrencyInfo info = (CurrencyInfo)res;
				CADepositDetail detail = (CADepositDetail)res;
				CADepositEntry.ARPaymentUpdate arDoc = (CADepositEntry.ARPaymentUpdate)res;
				CADepositEntry.APPaymentUpdate apDoc = (CADepositEntry.APPaymentUpdate)res;

				if (catran.CuryID != doc.CuryID)
				{
					throw new PXException(Messages.MultiCurDepositNotSupported);
				}

				if (!string.IsNullOrEmpty(detail.OrigRefNbr)
					&& detail.OrigModule == BatchModule.AR
					&& detail.DetailType == CADepositDetailType.VoidCheckDeposit
					&& IsVoidedEntryWithoutPair(detail))
				{
					throw new PXException(Messages.VoidedDepositCannotBeReleasedDueToInvalidStatusOfIncludedPayment);
				}

				batch = (Batch)je.BatchModule.Current;
				if (object.Equals(prev_tran, catran) == false)
				{
					tran = new GLTran();
					tran.SummPost = false;
					tran.CuryInfoID = batch.CuryInfoID;
					tran.TranType = catran.OrigTranType;
					tran.RefNbr = catran.OrigRefNbr;
					tran.ReferenceID = catran.ReferenceID;
					tran.AccountID = cashacct.AccountID;
					tran.SubID = cashacct.SubID;
					tran.BranchID = cashacct.BranchID;
					tran.CATranID = catran.TranID;
					tran.TranDate = catran.TranDate;
					FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, catran.TranPeriodID);
					tran.CuryDebitAmt = (catran.DrCr == CADrCr.CADebit ? catran.CuryTranAmt : 0m);
					tran.DebitAmt = (catran.DrCr == CADrCr.CADebit ? catran.TranAmt : 0m);
					tran.CuryCreditAmt = (catran.DrCr == CADrCr.CADebit ? 0m : -1m * catran.CuryTranAmt);
					tran.CreditAmt = (catran.DrCr == CADrCr.CADebit ? 0m : -1m * catran.TranAmt);
					tran.TranDesc = catran.TranDesc;
					tran.Released = true;

					if (catran.OrigTranType == CATranType.CADeposit || catran.OrigTranType == CATranType.CAVoidDeposit)
					{
						tran.ZeroPost = true;
					}

					je.GLTranModuleBatNbr.Insert(tran);

					if (!String.IsNullOrEmpty(arDoc.RefNbr))
					{
						if (doc.TranType == CATranType.CADeposit)
						{
							arDoc.Deposited = true;
						}
						else
						{
							arDoc.Deposited = false;
							arDoc.DepositType = null;
							arDoc.DepositNbr = null;
							arDoc.DepositDate = null;
						}
						this.Caches[typeof(CADepositEntry.ARPaymentUpdate)].Update(arDoc);
					}

					if (!String.IsNullOrEmpty(apDoc.RefNbr))
					{
						if (doc.TranType == CATranType.CADeposit)
						{
							apDoc.Deposited = true;
						}
						else
						{
							apDoc.Deposited = false;
							apDoc.DepositType = null;
							apDoc.DepositNbr = null;
							apDoc.DepositDate = null;
						}
						this.Caches[typeof(CADepositEntry.APPaymentUpdate)].Update(apDoc);
					}

					if (!String.IsNullOrEmpty(detail.OrigRefNbr))
					{

						decimal rgol = Math.Round((detail.OrigAmtSigned.Value - detail.TranAmt.Value), 3);
						if (rgol != Decimal.Zero)
						{
							GLTran rgol_tran = null;
							if (!rgols.ContainsKey(detail.AccountID.Value))
							{
								rgol_tran = new GLTran();
								rgol_tran.DebitAmt = Decimal.Zero;
								rgol_tran.CreditAmt = Decimal.Zero;
								rgol_tran.AccountID = cashacct.AccountID;
								rgol_tran.SubID = cashacct.SubID;
								rgol_tran.BranchID = cashacct.BranchID;
								rgol_tran.TranDate = catran.TranDate;
								FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, rgol_tran, catran.TranPeriodID);
								rgol_tran.TranType = CATranType.CATransferRGOL;
								rgol_tran.RefNbr = doc.RefNbr;
								rgol_tran.TranDesc = "RGOL";
								rgol_tran.Released = true;
								rgol_tran.CuryInfoID = batch.CuryInfoID;

								rgols[detail.AccountID.Value] = rgol_tran;
							}
							else
							{
								rgol_tran = rgols[detail.AccountID.Value];
							}
							rgol_tran.DebitAmt += ((catran.DrCr == CADrCr.CACredit) == rgol > 0 ? Decimal.Zero : Math.Abs(rgol));
							rgol_tran.CreditAmt += ((catran.DrCr == CADrCr.CACredit) == rgol > 0 ? Math.Abs(rgol) : Decimal.Zero);
							rgol_cury = cury;
							rgol_info = info;
						}
					}
				}
				prev_tran = catran;

				if (catran.TranID.HasValue)
				{
					caTranIDs.Add(catran.TranID.Value);
				}
			}
			if (batch != null)
			{
				foreach (CADepositCharge iCharge in PXSelect<CADepositCharge, Where<CADepositCharge.tranType, Equal<Required<CADepositCharge.tranType>>,
																	And<CADepositCharge.refNbr, Equal<Required<CADepositCharge.refNbr>>>>>.Select(this, doc.TranType, doc.RefNbr))
				{
					if (iCharge != null && iCharge.CuryChargeAmt != Decimal.Zero)
					{
						tran = new GLTran();
						tran.SummPost = false;
						tran.CuryInfoID = batch.CuryInfoID;
						tran.TranType = iCharge.TranType;
						tran.RefNbr = iCharge.RefNbr;

						tran.AccountID = iCharge.AccountID;
						tran.SubID = iCharge.SubID;
						tran.TranDate = doc.TranDate;
						tran.BranchID = doc.BranchID;
						FinPeriodIDAttribute.SetPeriodsByMaster<GLTran.finPeriodID>(je.GLTranModuleBatNbr.Cache, tran, doc.TranPeriodID);
						tran.CuryDebitAmt = (iCharge.DrCr == CADrCr.CADebit ? Decimal.Zero : iCharge.CuryChargeAmt);
						tran.DebitAmt = (iCharge.DrCr == CADrCr.CADebit ? Decimal.Zero : iCharge.ChargeAmt);
						tran.CuryCreditAmt = (iCharge.DrCr == CADrCr.CADebit ? iCharge.CuryChargeAmt : Decimal.Zero);
						tran.CreditAmt = (iCharge.DrCr == CADrCr.CADebit ? iCharge.ChargeAmt : Decimal.Zero);
						tran.Released = true;
						InsertDepositChargeTransaction(je, tran, new GLTranInsertionContext() { CADepositChargeRecord = iCharge, });
					}
				}

				foreach (KeyValuePair<int, GLTran> it in rgols)
				{
					GLTran rgol_tran = it.Value;
					decimal rgolAmt = (decimal)(rgol_tran.DebitAmt - rgol_tran.CreditAmt);
					int sign = Math.Sign(rgolAmt);
					rgolAmt = Math.Abs(rgolAmt);

					if ((rgolAmt) != Decimal.Zero)
					{
						tran = (GLTran)je.Caches[typeof(GLTran)].CreateCopy(rgol_tran);
						tran.CuryDebitAmt = Decimal.Zero;
						tran.CuryCreditAmt = Decimal.Zero;
						if (doc.DocType == CATranType.CADeposit)
						{
							tran.AccountID = (sign < 0) ? rgol_cury.RealLossAcctID : rgol_cury.RealGainAcctID;
							tran.SubID = (sign < 0)
								? GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgol_tran.BranchID, rgol_cury)
								: GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgol_tran.BranchID, rgol_cury);
						}
						else
						{
							tran.AccountID = (sign < 0) ? rgol_cury.RealGainAcctID : rgol_cury.RealLossAcctID;
							tran.SubID = (sign < 0)
								? GainLossSubAccountMaskAttribute.GetSubID<Currency.realGainSubID>(je, rgol_tran.BranchID, rgol_cury)
								: GainLossSubAccountMaskAttribute.GetSubID<Currency.realLossSubID>(je, rgol_tran.BranchID, rgol_cury);
						}

						tran.DebitAmt = sign < 0 ? rgolAmt : Decimal.Zero;
						tran.CreditAmt = sign < 0 ? Decimal.Zero : rgolAmt;
						tran.TranType = CATranType.CATransferRGOL;
						tran.RefNbr = doc.RefNbr;
						tran.TranDesc = "RGOL";
						tran.TranDate = rgol_tran.TranDate;
						tran.FinPeriodID = rgol_tran.FinPeriodID;
						tran.TranPeriodID = rgol_tran.TranPeriodID;
						tran.Released = true;
						tran.CuryInfoID = batch.CuryInfoID;
						tran = je.GLTranModuleBatNbr.Insert(tran);

						rgol_tran.CuryDebitAmt = Decimal.Zero;
						rgol_tran.DebitAmt = (sign > 0) ? rgolAmt : Decimal.Zero;
						rgol_tran.CreditAmt = (sign > 0) ? Decimal.Zero : rgolAmt;
						je.GLTranModuleBatNbr.Insert(rgol_tran);
					}
				}
			}
			if (batch != null && batch.CuryCreditTotal == batch.CuryDebitTotal)
			{
				AddRoundingTran(je, doc, batch, cury);
			}
			if (batch != null && batch.CuryCreditTotal != batch.CuryDebitTotal)
				throw new PXException(GL.Messages.BatchOutOfBalance);
			using (PXTransactionScope ts = new PXTransactionScope())
			{
				if (batch != null)
				{
					je.Save.Press();
					if (batchlist.Contains(batch) == false)
					{
						batchlist.Add(batch);
					}
					doc.Released = true;

					Caches[typeof(CADeposit)].Update(doc);
					if (doc.TranType == CATranType.CAVoidDeposit)
					{
						CADeposit orig = PXSelect<CADeposit, Where<CADeposit.tranType, Equal<CATranType.cADeposit>, And<CADeposit.refNbr, Equal<Required<CADeposit.refNbr>>>>>.Select(this, doc.RefNbr);
						orig.Voided = true;
						Caches[typeof(CADeposit)].Update(orig);
					}
				}
				Actions.PressSave();

				CheckMultipleGLPosting(caTranIDs);

				ts.Complete();
			}
		}

		private static Batch CreateGLBatch(JournalEntry je, CADeposit doc)
		{
			CurrencyInfo orig = PXSelectReadonly<CurrencyInfo, Where<CurrencyInfo.curyInfoID, Equal<Required<CurrencyInfo.curyInfoID>>>>.Select(je, doc.CuryInfoID);
			CurrencyInfo newinfo = (CurrencyInfo)je.currencyinfo.Cache.CreateCopy(orig);
			newinfo.CuryInfoID = null;
			newinfo = je.currencyinfo.Insert(newinfo);
			CashAccount cashAccount = PXSelect<CashAccount, Where<CashAccount.cashAccountID, Equal<Required<CashAccount.cashAccountID>>>>.Select(je, doc.CashAccountID);
			Batch newbatch = new Batch();
			newbatch.Module = BatchModule.CA;
			newbatch.Status = "U";
			newbatch.Released = true;
			newbatch.Hold = false;
			newbatch.DateEntered = doc.TranDate;
			newbatch.FinPeriodID = doc.FinPeriodID;
			newbatch.TranPeriodID = doc.TranPeriodID;
			newbatch.CuryID = doc.CuryID;
			newbatch.CuryInfoID = newinfo.CuryInfoID;
			newbatch.DebitTotal = 0m;
			newbatch.CreditTotal = 0m;
			newbatch.BranchID = cashAccount.BranchID;
			newbatch.Description = doc.TranDesc;
			newbatch = je.BatchModule.Insert(newbatch);

			CurrencyInfo b_info = je.currencyinfo.Select();
			if (b_info != null)
			{
				b_info.CuryID = orig.CuryID;
				je.currencyinfo.SetValueExt<CurrencyInfo.curyEffDate>(b_info, orig.CuryEffDate);
				b_info.SampleCuryRate = orig.SampleCuryRate ?? b_info.SampleCuryRate;
				b_info.CuryRateTypeID = orig.CuryRateTypeID ?? b_info.CuryRateTypeID;
				je.currencyinfo.Update(b_info);
			}
			je.BatchModule.Current = newbatch;
			return newbatch;
		}

		protected void CheckMultipleGLPosting(HashSet<long> caTranIDs)
		{
			foreach (long id in caTranIDs)
			{
				int count = PXSelectReadonly2<CATran, InnerJoin<GLTran,
								On<GLTran.cATranID, Equal<CATran.tranID>>>,
								Where<CATran.released, Equal<True>, And<GLTran.released, Equal<True>,
								And<CATran.tranID, Equal<Required<CATran.tranID>>>>>>.Select(this, id).Count;

				if (count != 1)
				{
					var errorMsg = $"Error message: {Messages.CATranHasExcessGLTran}; Date: {DateTime.Now}; Screen: {Accessinfo.ScreenID}; Count: {count}; CATranID: {id};";
					PXTrace.WriteError(errorMsg);

					if (this.casetup.Current.ValidateDataConsistencyOnRelease == true)
					{
						throw new PXException(Messages.CATranHasExcessGLTran);
					}
				}
			}
		}

		protected virtual IComparer<Tax> GetTaxComparer() => TaxByCalculationLevelAndTypeComparer.Instance;

		public class GLTranInsertionContext
		{
			public virtual CAAdj CAAdjRecord { get; set; }
			public virtual CASplit CASplitRecord { get; set; }
			public virtual CATaxTran CATaxTranRecord { get; set; }

			public virtual CATransfer CATransferRecord { get; set; }
			public virtual CAExpense CAExpenseRecord { get; set; }

			public virtual CADeposit CADepositRecord { get; set; }
			public virtual CADepositDetail CADepositDetailRecord { get; set; }
			public virtual CADepositCharge CADepositChargeRecord { get; set; }

			public virtual CATran CATranRecord { get; set; }
		}

		/// <summary>
		/// The method to insert document GL transactions 
		/// for the <see cref="CAAdj"/> or <see cref="CATransfer"/> entity.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.CATranRecord"/>.
		/// </summary>
		public virtual GLTran InsertDocumentTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert document detail GL transactions 
		/// for the <see cref="CAAdj"/> or <see cref="CATransfer"/> entity.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.CASplitRecord"/>.
		/// </summary>
		public virtual GLTran InsertSplitTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert document rounding GL transactions 
		/// for the <see cref="CAAdj"/> or <see cref="CATransfer"/> entity.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.CATranRecord"/>.
		/// <see cref="GLTranInsertionContext.CAAdjRecord"/>.
		/// <see cref="GLTranInsertionContext.CATransferRecord"/>.
		/// </summary>
		public virtual GLTran InsertRoundingTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

		/// <summary>
		/// The method to insert deposit charges GL transactions 
		/// for the <see cref="CADeposit"/> entity.
		/// <see cref="GLTranInsertionContext"/> class content:
		/// <see cref="GLTranInsertionContext.CADepositChargeRecord"/>.
		/// </summary>
		public virtual GLTran InsertDepositChargeTransaction(
			JournalEntry je,
			GLTran tran,
			GLTranInsertionContext context)
		{
			return je.GLTranModuleBatNbr.Insert(tran);
		}

	}
}
