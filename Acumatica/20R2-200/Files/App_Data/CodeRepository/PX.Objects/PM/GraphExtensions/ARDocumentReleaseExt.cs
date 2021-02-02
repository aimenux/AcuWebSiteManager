using PX.Data;
using PX.Data.BQL;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;

namespace PX.Objects.PM.GraphExtensions
{
	public class ARReleaseProcessExt : PXGraphExtension<ARReleaseProcess>
	{
		public PXSelect<PMTran, Where<PMTran.aRTranType, Equal<Required<PMTran.aRTranType>>,
				And<PMTran.aRRefNbr, Equal<Required<PMTran.aRRefNbr>>>>> ARDoc_PMTran;

		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
		}

		protected Dictionary<int, List<PMTran>> pmtranByARTranLineNbr = new Dictionary<int, List<PMTran>>();
		protected Dictionary<string, Queue<PMTran>> billedInOriginalInvoice = new Dictionary<string, Queue<PMTran>>();
		protected List<PXResult<ARTran, PMTran>> creditMemoPMReversal = new List<PXResult<ARTran, PMTran>>();
		protected List<PMTran> allocationsPMReversal = new List<PMTran>();
		protected List<PMTran> remainders = new List<PMTran>();
		protected List<Tuple<PMProformaTransactLine, PMTran>> billLaterPMList = new List<Tuple<PMProformaTransactLine, PMTran>>();
		protected Dictionary<int, Tuple<PMProformaTransactLine, PMTran>> billLaterLinesWithFirstTransaction = new Dictionary<int, Tuple<PMProformaTransactLine, PMTran>>();
		protected List<PMRegister> ProjectDocuments;
		[PXOverride]
		public virtual List<ARRegister> ReleaseInvoice(
			JournalEntry je,
			ARRegister doc,
			PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account> res,
			List<PMRegister> pmDocs, Func<JournalEntry, ARRegister, PXResult<ARInvoice, CurrencyInfo, Terms, Customer, Account>, List<PMRegister>, List<ARRegister>> baseMethod)
		{
			ProjectDocuments = pmDocs;
			pmtranByARTranLineNbr.Clear();
			billedInOriginalInvoice.Clear();
			creditMemoPMReversal.Clear();
			allocationsPMReversal.Clear();
			remainders.Clear();
			billLaterPMList.Clear();
			billLaterLinesWithFirstTransaction.Clear();

			ARInvoice ardoc = (ARInvoice)res;

			if (ardoc.ProjectID != null && !ProjectDefaultAttribute.IsNonProject(ardoc.ProjectID)) 
			{
				foreach (PMTran tran in ARDoc_PMTran.Select(ardoc.DocType, ardoc.RefNbr))
				{
					if (tran.RemainderOfTranID.HasValue)
						remainders.Add(tran);

					List<PMTran> list;
					if (!pmtranByARTranLineNbr.TryGetValue(tran.RefLineNbr.Value, out list))
					{
						list = new List<PMTran>();
						pmtranByARTranLineNbr.Add(tran.RefLineNbr.Value, list);
					}

					list.Add(tran);
				}
			}

			PMProforma proforma = PXSelect<PMProforma, Where<PMProforma.aRInvoiceDocType, Equal<Required<PMProforma.aRInvoiceDocType>>,
				And<PMProforma.aRInvoiceRefNbr, Equal<Required<PMProforma.aRInvoiceRefNbr>>>>>.Select(Base, ardoc.DocType, ardoc.RefNbr);
			if (proforma != null)
			{
				var select = new PXSelectJoin<PMTran,
					InnerJoin<PMProformaTransactLine, On<PMProformaTransactLine.refNbr, Equal<PMTran.proformaRefNbr>, And<PMProformaTransactLine.lineNbr, Equal<PMTran.proformaLineNbr>>>>,
					Where<PMProformaTransactLine.refNbr, Equal<Required<PMProformaTransactLine.refNbr>>,
						And<PMProformaTransactLine.type, Equal<PMProformaLineType.transaction>,
						And<PMProformaTransactLine.option, NotEqual<PMProformaLine.option.writeOffRemainder>,
						And<PMProformaTransactLine.option, NotEqual<PMProformaLine.option.bill>>>>>>(Base);

				foreach (PXResult<PMTran, PMProformaTransactLine> res2 in select.Select(proforma.RefNbr))
				{
					PMTran pmt = (PMTran)res2;
					PMProformaTransactLine line = (PMProformaTransactLine)res2;

					if (line.Option == PMProformaLine.option.Writeoff)
					{
						if (!string.IsNullOrEmpty(pmt.AllocationID) && pmt.Reverse != PMReverse.OnInvoiceGeneration) //if not already reversed on billing
							allocationsPMReversal.Add(pmt);

						if (pmt.RemainderOfTranID.HasValue)
							remainders.Add(pmt);
					}
					else if (line.Option == PMProformaLine.option.HoldRemainder && line.CuryLineTotal < line.CuryBillableAmount)
					{
						if (!billLaterLinesWithFirstTransaction.ContainsKey(line.LineNbr.Value))
						{
							billLaterLinesWithFirstTransaction.Add(line.LineNbr.Value, new Tuple<PMProformaTransactLine, PMTran>(line, pmt));
						}
					}
				}

				billLaterPMList.AddRange(billLaterLinesWithFirstTransaction.Values);
			}

			if (doc.DocType == ARDocType.CreditMemo && doc.OrigDocType == ARDocType.Invoice) //credit memo reversal only for Invoice created by PM billing. CreditMemo created by PM Billing should be handled same as INV.
			{
				//Credit Memo reversal will recreate (new duplicates) the transactions used for initial Invoice so that they can be billed again (except contract usage).
				var selectOriginalBilledTrans = new PXSelectJoin<PMTran,
					InnerJoin<ARTran, On<ARTran.tranType, Equal<PMTran.aRTranType>, And<ARTran.refNbr, Equal<PMTran.aRRefNbr>, And<ARTran.lineNbr, Equal<PMTran.refLineNbr>>>>>,
					Where<PMTran.aRTranType, Equal<Required<PMTran.aRTranType>>,
					And<PMTran.aRRefNbr, Equal<Required<PMTran.aRRefNbr>>,
					And<PMTran.taskID, IsNotNull>>>,
					OrderBy<Asc<ARTran.lineNbr>>>(Base);

				foreach (PXResult<PMTran, ARTran> rslt in selectOriginalBilledTrans.Select(doc.OrigDocType, doc.OrigRefNbr))
				{
					PMTran pmtran = (PMTran)rslt;
					ARTran artran = (ARTran)rslt;
					string key = string.Format("{0}.{1}", artran.TaskID, artran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID));
					if (!billedInOriginalInvoice.ContainsKey(key))
					{
						billedInOriginalInvoice.Add(key, new Queue<PMTran>());
					}
					billedInOriginalInvoice[key].Enqueue(pmtran);
				}
			}

			return baseMethod(je, doc, res, pmDocs);
		}

		[PXOverride]
		public virtual void ReleaseInvoiceTransactionPostProcessed(JournalEntry je, ARInvoice ardoc, ARTran n)
		{
			if (!Base.IsIntegrityCheck)
			{
				if (n.CommitmentID != null)
				{
					decimal sign = (n.DrCr == DrCr.Credit) ? Decimal.One : Decimal.MinusOne;

					PMCommitment container = new PMCommitment();
					container.CommitmentID = n.CommitmentID;
					container.ProjectID = n.ProjectID;
					container.UOM = n.UOM;
					container.InventoryID = n.InventoryID;
					container.InvoicedAmount = sign * n.CuryTranAmt;
					container.InvoicedQty = sign * n.Qty;

					PMCommitmentAttribute.AddToInvoiced(Base.Caches[typeof(SO.SOLine)], container, ardoc.CuryID, ardoc.InvoiceDate);
				}
			}

			if (billedInOriginalInvoice.Count != 0)
			{
				//Time & Material
				string key = string.Format("{0}.{1}", n.TaskID, n.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID));
				Queue<PMTran> billedTrans = null;
				if (billedInOriginalInvoice.TryGetValue(key, out billedTrans) && billedTrans.Count > 0)
				{
					while (billedTrans.Count > 0)
					{
						PMTran unbilled = billedTrans.Dequeue();
						creditMemoPMReversal.Add(new PXResult<ARTran, PMTran>(n, unbilled));
					}
				}
			}
			else if (n.TaskID != null && ardoc.DocType == ARDocType.CreditMemo && ardoc.OrigDocType == ARDocType.Invoice &&
			n.OrigInvoiceDate != null)//OrigInvoiceDate is used as a marker to distinguish between ARTran that were added automatically on Reversal and added manually by user.
			{
				//Progressive
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, n.AccountID);
				PMProformaRevision revision = PXSelect<PMProformaRevision, Where<PMProformaRevision.reversedARInvoiceDocType, Equal<Required<PMProformaRevision.reversedARInvoiceDocType>>,
					And<PMProformaRevision.reversedARInvoiceRefNbr, Equal<Required<PMProformaRevision.reversedARInvoiceRefNbr>>>>>.Select(Base, ardoc.DocType, ardoc.RefNbr);

				if (revision == null)
					RestoreAmountToInvoice(ardoc.CuryID, n, account?.AccountGroupID);
			}

			List<PMTran> linkedTransactions;
			if (pmtranByARTranLineNbr.TryGetValue(n.LineNbr.Value, out linkedTransactions))
			{
				foreach (PMTran pmtran in linkedTransactions)
				{
					if (pmtran.Reverse == PMReverse.OnInvoiceRelease)
						allocationsPMReversal.Add(pmtran);
				}
			}

		}

		[PXOverride]
		public virtual void ReleaseInvoiceBatchPostProcessing(JournalEntry je, ARInvoice ardoc, Batch arbatch)
		{
			if (Base.IsIntegrityCheck == false)
			{
				PMRegister existingAllocationReversal = PXSelect<PMRegister,
					Where<PMRegister.origDocType, Equal<PMOrigDocType.allocationReversal>,
						And<PMRegister.origNoteID, Equal<Required<ARInvoice.noteID>>,
						And<PMRegister.released, Equal<False>>>>>.Select(Base, ardoc.NoteID);
				if (existingAllocationReversal != null)
					ProjectDocuments.Add(existingAllocationReversal);

				if (creditMemoPMReversal.Count > 0 || billLaterPMList.Count > 0 || allocationsPMReversal.Count > 0 || remainders.Count > 0)
				{
					RegisterEntry pmEntry = PXGraph.CreateInstance<PM.RegisterEntry>();
					pmEntry.FieldVerifying.AddHandler<PMTran.projectID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					pmEntry.FieldVerifying.AddHandler<PMTran.taskID>((PXCache sender, PXFieldVerifyingEventArgs e) => { e.Cancel = true; });
					if (creditMemoPMReversal.Count > 0)
					{
						pmEntry.Clear();
						pmEntry.ReverseCreditMemo(ardoc, creditMemoPMReversal);
						pmEntry.Actions.PressSave();
						ProjectDocuments.Add(pmEntry.Document.Current);
					}
					if (billLaterPMList.Count > 0)
					{
						pmEntry.Clear();
						pmEntry.BillLater(ardoc, billLaterPMList);
						pmEntry.Actions.PressSave();
						ProjectDocuments.Add(pmEntry.Document.Current);
					}
					if (allocationsPMReversal.Count > 0)
					{
						pmEntry.Clear();
						pmEntry.ReverseAllocations(ardoc, allocationsPMReversal);
						pmEntry.Actions.PressSave();
						ProjectDocuments.Add(pmEntry.Document.Current);
					}
					if (remainders.Count > 0)
					{
						var remaindersToReverse = pmEntry.GetRemaindersToReverse(remainders);
						if (remaindersToReverse.Count > 0)
						{
							pmEntry.Clear();
							pmEntry.ReverseRemainders(ardoc, remainders);
							pmEntry.Actions.PressSave();
							ProjectDocuments.Add(pmEntry.Document.Current);
						}
					}
				}
			}
		}

		[PXOverride]
		public virtual AP.APReleaseProcess.LineBalances AdjustInvoiceDetailsBalanceByLine(ARRegister doc, ARTran tran, Func<ARRegister, ARTran, AP.APReleaseProcess.LineBalances> baseMethod)
		{
			var result = baseMethod(doc, tran);

			AdjustProjectBudgetRetainage(doc, tran);

			return result;
		}

		[PXOverride]
		public virtual void AdjustOriginalRetainageLineBalance(ARRegister document, ARTran tran, decimal? curyAmount, decimal? baseAmount, Action<ARRegister, ARTran, decimal?, decimal?> baseMethod)
		{
			baseMethod(document, tran, curyAmount, baseAmount);
			ARTran origRetainageTran = Base.GetOriginalRetainageLine(document, tran);

			if (origRetainageTran != null)
			{
				if (!Base.IsIntegrityCheck)
					DecreaseRetainedAmount(origRetainageTran, tran.CuryOrigTranAmt.GetValueOrDefault() * document.SignAmount.GetValueOrDefault());
			}
		}

		public virtual void RestoreAmountToInvoice(string docCuryID, ARTran line, int? revenueAccountGroup)
		{
			if (line.TaskID == null)
				return;

			if (revenueAccountGroup == null)
				return;

			PMProject project = PMProject.PK.Find(Base, line.ProjectID);
			if (project != null && project.NonProject != true)
			{
				PMAccountGroup ag = PMAccountGroup.PK.Find(Base, revenueAccountGroup);

				bool isExisting;
				BudgetService budgetService = new BudgetService(Base);
				PMBudgetLite budget = budgetService.SelectProjectBalance(ag, project, line.TaskID, line.InventoryID, line.CostCodeID, out isExisting);

				PMBudgetAccum invoiced = new PMBudgetAccum();
				invoiced.Type = budget.Type;
				invoiced.ProjectID = budget.ProjectID;
				invoiced.ProjectTaskID = budget.TaskID;
				invoiced.AccountGroupID = budget.AccountGroupID;
				invoiced.InventoryID = budget.InventoryID;
				invoiced.CostCodeID = budget.CostCodeID;
				invoiced.UOM = budget.UOM;
				invoiced.Description = budget.Description;

				invoiced = (PMBudgetAccum)Base.Caches[typeof(PMBudgetAccum)].Insert(invoiced);

				if (docCuryID == project.CuryID)
				{
					invoiced.CuryAmountToInvoice += line.CuryTranAmt + line.CuryRetainageAmt;
					invoiced.AmountToInvoice += line.TranAmt + line.RetainageAmt;
				}
				else
				{
					invoiced.AmountToInvoice += line.TranAmt + line.RetainageAmt;

					decimal val;
					PXCurrencyAttribute.CuryConvCury<PMProject.curyInfoID>(Base.Caches[typeof(PMProject)], project, line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault(), out val);
					invoiced.CuryAmountToInvoice += val;
				}
			}
		}

		/// <summary>
		/// Moves Project's Retained Amount out of Draft Retained Amount bucket into Retained Amount
		/// </summary>
		protected virtual void AdjustProjectBudgetRetainage(ARRegister doc, ARTran tran)
		{
			if (tran.TaskID != null)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, tran.AccountID);
				if (account != null && account.AccountGroupID != null)
				{
					AddRetainedAmount(tran, account.AccountGroupID);
					SubtractDraftRetainedAmount(tran, account.AccountGroupID);
				}
			}
		}

		protected virtual void DecreaseRetainedAmount(ARTran tran, decimal value)
		{
			if (tran.TaskID != null && value != 0)
			{
				Account account = PXSelect<Account, Where<Account.accountID, Equal<Required<Account.accountID>>>>.Select(Base, tran.AccountID);
				if (account != null && account.AccountGroupID != null)
				{
					AddRetainedAmount(tran, account.AccountGroupID, value * -1);
				}
			}
		}

		protected virtual void AddRetainedAmount(ARTran tran, int? accountGroupID, int mult = 1)
		{
			decimal retainedAmount = mult * tran.CuryRetainageAmt.GetValueOrDefault() * ARDocType.SignAmount(tran.TranType).GetValueOrDefault(1);

			AddRetainedAmount(tran, accountGroupID, retainedAmount);
		}

		protected virtual void AddRetainedAmount(ARTran tran, int? accountGroupID, decimal value)
		{
			if (value != 0)
			{
				PMBudgetAccum retained = GetTargetBudget(accountGroupID, tran);
				retained = (PMBudgetAccum)Base.Caches[typeof(PMBudgetAccum)].Insert(retained);
				retained.CuryRetainedAmount += value;
				retained.RetainedAmount += value;
			}
		}

		protected virtual void SubtractDraftRetainedAmount(ARTran tran, int? accountGroupID)
		{
			decimal retainedAmount = tran.CuryRetainageAmt.GetValueOrDefault() * ARDocType.SignAmount(tran.TranType).GetValueOrDefault(1);

			PMBudgetAccum retained = GetTargetBudget(accountGroupID, tran);
			retained = (PMBudgetAccum)Base.Caches[typeof(PMBudgetAccum)].Insert(retained);
			retained.CuryDraftRetainedAmount -= retainedAmount;
			retained.DraftRetainedAmount -= retainedAmount;
		}

		private PMBudgetAccum GetTargetBudget(int? accountGroupID, ARTran line)
		{
			PMAccountGroup ag = PMAccountGroup.PK.Find(Base, accountGroupID);
			PMProject project = PMProject.PK.Find(Base, line.ProjectID);

			bool isExisting;
			BudgetService budgetService = new BudgetService(Base);
			PMBudgetLite budget = budgetService.SelectProjectBalance(ag, project, line.TaskID, line.InventoryID, line.CostCodeID, out isExisting);

			PMBudgetAccum target = new PMBudgetAccum();
			target.Type = budget.Type;
			target.ProjectID = budget.ProjectID;
			target.ProjectTaskID = budget.TaskID;
			target.AccountGroupID = budget.AccountGroupID;
			target.InventoryID = budget.InventoryID;
			target.CostCodeID = budget.CostCodeID;
			target.UOM = budget.UOM;
			target.Description = budget.Description;
			target.CuryInfoID = project.CuryInfoID;

			return target;
		}
	}
}
