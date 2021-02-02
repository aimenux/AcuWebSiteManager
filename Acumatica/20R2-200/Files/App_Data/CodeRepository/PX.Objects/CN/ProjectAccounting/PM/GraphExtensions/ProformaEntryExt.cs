using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.CacheExtensions;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.AR;
using PX.Objects.CR;
using System;
using System.Collections;
using System.Collections.Generic;
using static PX.Objects.PM.ProformaEntry;
using PX.Objects.GL;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace PX.Objects.CN.ProjectAccounting.PM.GraphExtensions
{
    public class ProformaEntryExt : PXGraphExtension<ProformaEntry>
    {
		[PXCopyPasteHiddenView]
		[PXHidden]
		public PXSelect<PMProject, Where<PMProject.contractID, Equal<Current<PMProforma.projectID>>>> ProjectProperties;

		public const string AIAReport = "PM644000";
		public const string AIAWithQtyReport = "PM644500";
		

		public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        [PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.revenue>>),
			ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
		[PXFormula(typeof(Validate<PMProformaLine.costCodeID, PMProformaLine.description>))]
		protected virtual void _(Events.CacheAttached<PMProformaTransactLine.taskID> e)
		{
		}

		public PXAction<PMProforma> aia;
		[PXUIField(DisplayName = "AIA Report")]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		public virtual IEnumerable Aia(PXAdapter adapter)
		{
			if (Base.Document.Current != null)
			{
				Base.RecalculateExternalTaxesSync = true;
				Base.Document.Cache.SetValue<PMProforma.isAIAOutdated>(Base.Document.Current, false);
				Base.Document.Cache.MarkUpdated(Base.Document.Current);
				Base.Save.Press();

				var resultset = BuildResultsetForAIA();
				
				throw new PXReportRequiredException(resultset, GetAIAReportID(), "AIA Report");
			}

			return adapter.Get();
		}

		public virtual string GetAIAReportID()
		{
			if (Base.Document.Current != null && Base.Project.Current != null)
			{
				if (Base.Project.Current.IncludeQtyInAIA == true)
					return AIAWithQtyReport;
			}

			return AIAReport;
		}

		
		
		public virtual PXReportResultset BuildResultsetForAIA()
		{
			var contractTotalSelect = new PXSelectGroupBy<PMRevenueBudget,
						Where<PMRevenueBudget.projectID, Equal<Current<PMProforma.projectID>>,
						And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>,
						Aggregate<Sum<PMRevenueBudget.curyAmount,
						Sum<PMRevenueBudget.qty,
						Sum<PMRevenueBudget.curyChangeOrderAmount,
						Sum<PMRevenueBudget.changeOrderQty,
						Sum<PMRevenueBudget.curyRevisedAmount,
						Sum<PMRevenueBudget.revisedQty>>>>>>>>(Base);

			PMRevenueBudget contractTotal = contractTotalSelect.Select();

			Dictionary<TotalsKey, ProformaTotals> proformaTotalsByTask = GetProformaTotalsToDate();
			Dictionary<TotalsKey, decimal> billedRetainage = Base.GetBilledRetainageToDate(Base.Document.Current.InvoiceDate);

			decimal retainageHeldTodateTotal = 0;
			decimal retainageEverHeldTodateTotal = 0;
			decimal completedTodateTotal = 0;
			foreach (ProformaTotals total in proformaTotalsByTask.Values)
			{
				retainageHeldTodateTotal += total.CuryRetainage;
				completedTodateTotal += total.CuryLineTotal;
			}
			retainageEverHeldTodateTotal = retainageHeldTodateTotal;
			foreach (decimal amount in billedRetainage.Values)
			{
				retainageHeldTodateTotal -= amount;
			}

			PMProforma previousProforma = PXSelectGroupBy<PMProforma,
				Where<PMProforma.projectID, Equal<Current<PMProforma.projectID>>,
				And<PMProforma.refNbr, Less<Current<PMProforma.refNbr>>,
				And<PMProforma.corrected, NotEqual<True>>>>,
				Aggregate<Max<PMProforma.refNbr>>>.Select(Base);

			DateTime? cutoffDate = null;
			decimal? line6FromPriorCertificate = 0;
			if (previousProforma != null)
			{
				cutoffDate = previousProforma.InvoiceDate;

				var completedToDateTotalSelect = new PXSelectGroupBy<PMProformaLine,
				Where<PMProformaLine.refNbr, LessEqual<Required<PMProforma.refNbr>>,
				And<PMProformaLine.projectID, Equal<Required<PMProforma.projectID>>,
				And<PMProformaLine.type, Equal<PMProformaLineType.progressive>,
				And<PMProformaLine.corrected, NotEqual<True>>>>>,
				Aggregate<Sum<PMProformaLine.curyLineTotal, Sum<PMProformaLine.curyRetainage>>>>(Base);

				PMProformaLine completedToDatePreviousTotal = completedToDateTotalSelect.Select(previousProforma.RefNbr, previousProforma.ProjectID);
				if (completedToDatePreviousTotal != null)
				{
					Dictionary<TotalsKey, decimal> billedRetainagePrevious = Base.GetBilledRetainageToDate(cutoffDate);
					decimal retainageHeldTodatePreviousTotal = completedToDatePreviousTotal.CuryRetainage.GetValueOrDefault();
					foreach (decimal amount in billedRetainagePrevious.Values)
					{
						retainageHeldTodatePreviousTotal -= amount;
					}
					line6FromPriorCertificate = completedToDatePreviousTotal.CuryLineTotal.GetValueOrDefault() - retainageHeldTodatePreviousTotal;
				}
			}

			var changeOrders = new PXSelectReadonly<PMChangeOrder,
				Where<PMChangeOrder.projectID, Equal<Current<PMProforma.projectID>>,
				And<PMChangeOrder.approved, Equal<True>,
				And<PMChangeOrder.completionDate, LessEqual<Current<PMProforma.invoiceDate>>>>>>(Base);

			decimal additions = 0;
			decimal deduction = 0;
			decimal additionsPrevious = 0;
			decimal deductionPrevious = 0;
			foreach (PMChangeOrder order in changeOrders.Select())
			{
				if (cutoffDate != null && order.CompletionDate.Value.Date <= cutoffDate.Value.Date)
				{
					if (order.RevenueTotal.GetValueOrDefault() < 0)
					{
						deductionPrevious += -1 * order.RevenueTotal.GetValueOrDefault();
					}
					else
					{
						additionsPrevious += order.RevenueTotal.GetValueOrDefault();
					}
				}
				else
				{
					if (order.RevenueTotal.GetValueOrDefault() < 0)
					{
						deduction += -1 * order.RevenueTotal.GetValueOrDefault();
					}
					else
					{
						additions += order.RevenueTotal.GetValueOrDefault();
					}
				}
			}

			Dictionary<TotalsKey, ChangeOrderTotals> coRevenue = new Dictionary<TotalsKey, ChangeOrderTotals>();

			var changeOrderRevenue = new PXSelectJoinGroupBy<PMChangeOrderRevenueBudget,
				InnerJoin<PMChangeOrder, On<PMChangeOrder.refNbr, Equal<PMChangeOrderRevenueBudget.refNbr>>>,
				Where<PMChangeOrder.projectID, Equal<Current<PMProforma.projectID>>,
				And<PMChangeOrder.approved, Equal<True>,
				And<PMChangeOrder.completionDate, LessEqual<Current<PMProforma.invoiceDate>>,
				And<PMChangeOrderRevenueBudget.type, Equal<GL.AccountType.income>>>>>,
				Aggregate<GroupBy<PMChangeOrderRevenueBudget.projectID,
				GroupBy<PMChangeOrderRevenueBudget.projectTaskID,
				GroupBy<PMChangeOrderRevenueBudget.inventoryID,
				GroupBy<PMChangeOrderRevenueBudget.costCodeID,
				Sum<PMChangeOrderRevenueBudget.amount,
				Sum<PMChangeOrderRevenueBudget.qty>>>>>>>>(Base);

			foreach (PMChangeOrderRevenueBudget revenueChange in changeOrderRevenue.Select())
			{
				TotalsKey key = new TotalsKey(revenueChange.ProjectTaskID.Value, revenueChange.CostCodeID.Value, revenueChange.InventoryID.Value, 0);
				coRevenue.Add(key, new ChangeOrderTotals() { Key = key, Amount = revenueChange.Amount.GetValueOrDefault(), Quantity = revenueChange.Qty.GetValueOrDefault() });
			}

			PMProformaInfo proformaInfo = new PMProformaInfo();
			proformaInfo.RefNbr = Base.Document.Current.RefNbr;
			proformaInfo.OriginalContractTotal = contractTotal.CuryAmount;
			proformaInfo.ChangeOrderTotal = additionsPrevious + additions - deductionPrevious - deduction;
			proformaInfo.RevisedContractTotal = proformaInfo.OriginalContractTotal + proformaInfo.ChangeOrderTotal;
			proformaInfo.PriorProformaLineTotal = line6FromPriorCertificate;
			proformaInfo.CompletedToDateLineTotal = completedTodateTotal;
			proformaInfo.RetainageHeldToDateTotal = retainageHeldTodateTotal;
			proformaInfo.ChangeOrderAdditions = additions;
			proformaInfo.ChangeOrderAdditionsPrevious = additionsPrevious;
			proformaInfo.ChangeOrderDeduction = deduction;
			proformaInfo.ChangeOrderDeductionPrevious = deductionPrevious;

			proformaInfo = CustomizeProformaInfo(proformaInfo);

			PXReportResultset resultset = new PXReportResultset(typeof(PMProformaProgressLine), typeof(PMProformaLineInfo), typeof(PMRevenueBudget), typeof(PMTask), typeof(PMProforma), typeof(PMProformaInfo), typeof(PMProject), typeof(Customer), typeof(Address), typeof(GL.Branch), typeof(CompanyBAccount));

			Address address = PXSelectReadonly<Address, Where<Address.addressID, Equal<Required<Address.addressID>>>>.Select(Base, Base.Customer.Current.DefAddressID);
			PX.Objects.GL.Branch branch = PXSelectReadonly<PX.Objects.GL.Branch, Where<PX.Objects.GL.Branch.branchID, Equal<Required<PX.Objects.GL.Branch.branchID>>>>.Select(Base, Base.Document.Current.BranchID);
			CompanyBAccount company = PXSelectReadonly<CompanyBAccount, Where<CompanyBAccount.organizationID, Equal<Required<CompanyBAccount.organizationID>>>>.Select(Base, branch.OrganizationID);

			var selectTasks = new PXSelectReadonly<PMTask, Where<PMTask.projectID, Equal<Current<PMProforma.projectID>>>>(Base);
			Dictionary<int, PMTask> tasksLookup = new Dictionary<int, PMTask>();
			foreach (PMTask task in selectTasks.Select())
			{
				tasksLookup.Add(task.TaskID.Value, task);
			}

			decimal roundingOverflow = 0;
			PMProformaProgressLine lineWithLargestRetainage = null;

			foreach (PXResult<PMProformaProgressLine, PMRevenueBudget> res in Base.ProgressiveLines.Select())
			{
				PMProformaProgressLine line = PXCache<PMProformaProgressLine>.CreateCopy((PMProformaProgressLine)res);
				PMRevenueBudget budget = (PMRevenueBudget)res;
				PMProformaLineInfo lineInfo = new PMProformaLineInfo();
				lineInfo.RefNbr = line.RefNbr;
				lineInfo.LineNbr = line.LineNbr;
				lineInfo.ChangeOrderAmountToDate = 0m;
				lineInfo.ChangeOrderQtyToDate = 0m;

				decimal totalRetainedToDate = 0;
				decimal curyPreviouslyInvoiced = 0;
				decimal billedRetainageWithoutByLine = 0;
				decimal billedRetainageByLine = 0;
				decimal unitRate = 0;
				ProformaTotals totals;

				if (budget.Qty.GetValueOrDefault() != 0)
				{
					unitRate = budget.CuryAmount.GetValueOrDefault() / budget.Qty.Value;
				}

				TotalsKey key = new TotalsKey(line.TaskID.GetValueOrDefault(), line.CostCodeID.GetValueOrDefault(), line.InventoryID.GetValueOrDefault(), 0);

				if (proformaTotalsByTask.TryGetValue(key, out totals))
				{
					curyPreviouslyInvoiced = totals.CuryLineTotal - line.CuryLineTotal.GetValueOrDefault();
					totalRetainedToDate = totals.CuryRetainage;

					if (retainageEverHeldTodateTotal != 0 && billedRetainage.ContainsKey(Base.PayByLineOffKey))
					{
						decimal ratio = totals.CuryRetainage / retainageEverHeldTodateTotal;
						decimal billedRetainageWithoutByLineRaw = billedRetainage[Base.PayByLineOffKey] * ratio;
						billedRetainageWithoutByLine = Math.Round(billedRetainageWithoutByLineRaw, 2);
						decimal roundingDiff = billedRetainageWithoutByLineRaw - billedRetainageWithoutByLine;
						roundingOverflow += roundingDiff;
						Debug.Print("TaskID:{0} Ratio:{1} billedRetainageWithoutByLineRaw:{2}", key.TaskID, ratio, billedRetainageWithoutByLineRaw);
					}
				}

				billedRetainage.TryGetValue(key, out billedRetainageByLine);

				line.CuryPreviouslyInvoiced = curyPreviouslyInvoiced;

				if (unitRate != 0)
				{
					line.Qty = line.CuryLineTotal.GetValueOrDefault() / unitRate;
					lineInfo.PreviousQty = curyPreviouslyInvoiced / unitRate;
				}
				else
					lineInfo.PreviousQty = 0;

				ChangeOrderTotals coTotal;
				if (coRevenue.TryGetValue(key, out coTotal))
				{
					lineInfo.ChangeOrderAmountToDate = coTotal.Amount;
					lineInfo.ChangeOrderQtyToDate = coTotal.Quantity;
				}

				lineInfo = CustomizeProformaLineInfo(lineInfo);
							
				if (Base.Project.Current.RetainageMode == RetainageModes.Contract)
				{
					line.CuryRetainage = line.CuryAllocatedRetainedAmount;
				}
				else
				{
					//Calculate Held Retainage to date for a line and store in CuryRetainage field:
					line.CuryRetainage = totalRetainedToDate - (billedRetainageWithoutByLine + billedRetainageByLine);
				}

				if (lineWithLargestRetainage == null)
				{
					lineWithLargestRetainage = line;
				}
				else if (line.CuryRetainage > lineWithLargestRetainage.CuryRetainage)
				{
					lineWithLargestRetainage = line;
				}

				resultset.Add(line, lineInfo, budget, tasksLookup[line.TaskID.Value], Base.Document.Current, proformaInfo, Base.Project.Current, Base.Customer.Current, address, branch, company);
			}
			Debug.Print("Rounding overflow total: {0}", roundingOverflow);
			lineWithLargestRetainage.CuryRetainage += roundingOverflow;

			if (GroupByTask())
			{
				resultset = GroupResultsetByTask(resultset);
			}
						
			return resultset;
		}

		public virtual PXReportResultset GroupResultsetByTask(PXReportResultset input)
		{
			PXReportResultset output = new PXReportResultset(typeof(PMProformaProgressLine), typeof(PMProformaLineInfo), typeof(PMRevenueBudget), typeof(PMTask), typeof(PMProforma), typeof(PMProformaInfo), typeof(PMProject), typeof(Customer), typeof(Address), typeof(GL.Branch), typeof(CompanyBAccount));

			var byTask = new Dictionary<int, object[]>();
			foreach (object[] record in input)
			{
				PMProformaProgressLine line = (PMProformaProgressLine)record[0];
				PMProformaLineInfo info = (PMProformaLineInfo)record[1];
				PMRevenueBudget budget = (PMRevenueBudget)record[2];

				object[] existing;
				if (byTask.TryGetValue(line.TaskID.Value, out existing))
				{
					PMProformaProgressLine summaryLine = (PMProformaProgressLine)existing[0];
					PMProformaLineInfo summaryInfo = (PMProformaLineInfo)existing[1];
					PMRevenueBudget summaryBudget = (PMRevenueBudget)existing[2];

					BudgetAddToSummary(summaryBudget, budget);
					InfoAddToSummary(summaryInfo, info);
					LineAddToSummary(summaryLine, line);
				}
				else
				{
					output.Add(record);
					byTask.Add(line.TaskID.Value, record);
				}
			}

			return output;
		}

		public virtual bool GroupByTask()
		{
			if (Base.Project.Current.BudgetLevel == BudgetLevels.Task)
			{
				return false;
			}
			else
			{
				if (Base.Project.Current.AIALevel == CT.Contract.aIALevel.Summary)
				{
					return true;
				}
			}

			return false;
		}

		public virtual void BudgetAddToSummary(PMRevenueBudget summary, PMRevenueBudget record)
		{
			summary.CuryAmount = summary.CuryAmount.GetValueOrDefault() + record.CuryAmount.GetValueOrDefault();
			summary.Qty = summary.Qty.GetValueOrDefault() + record.Qty.GetValueOrDefault();
		}

		public virtual void InfoAddToSummary(PMProformaLineInfo summary, PMProformaLineInfo record)
		{
			summary.ChangeOrderAmountToDate = summary.ChangeOrderAmountToDate.GetValueOrDefault() + record.ChangeOrderAmountToDate.GetValueOrDefault();
			summary.ChangeOrderQtyToDate = summary.ChangeOrderQtyToDate.GetValueOrDefault() + record.ChangeOrderQtyToDate.GetValueOrDefault();
			summary.PreviousQty = summary.PreviousQty.GetValueOrDefault() + record.PreviousQty.GetValueOrDefault();
		}

		public virtual void LineAddToSummary(PMProformaProgressLine summary, PMProformaProgressLine record)
		{
			summary.CuryAmount = summary.CuryAmount.GetValueOrDefault() + record.CuryAmount.GetValueOrDefault();
			summary.Amount = summary.Amount.GetValueOrDefault() + record.Amount.GetValueOrDefault();
			summary.CuryMaterialStoredAmount = summary.CuryMaterialStoredAmount.GetValueOrDefault() + record.CuryMaterialStoredAmount.GetValueOrDefault();
			summary.MaterialStoredAmount = summary.MaterialStoredAmount.GetValueOrDefault() + record.MaterialStoredAmount.GetValueOrDefault();
			summary.CuryRetainage = summary.CuryRetainage.GetValueOrDefault() + record.CuryRetainage.GetValueOrDefault();
			summary.Retainage = summary.Retainage.GetValueOrDefault() + record.Retainage.GetValueOrDefault();
			summary.CuryLineTotal = summary.CuryLineTotal.GetValueOrDefault() + record.CuryLineTotal.GetValueOrDefault();
			summary.LineTotal = summary.LineTotal.GetValueOrDefault() + record.LineTotal.GetValueOrDefault();
			summary.CuryPreviouslyInvoiced = summary.CuryPreviouslyInvoiced.GetValueOrDefault() + record.CuryPreviouslyInvoiced.GetValueOrDefault();
			summary.PreviouslyInvoiced = summary.PreviouslyInvoiced.GetValueOrDefault() + record.PreviouslyInvoiced.GetValueOrDefault();
		}

		public virtual PMProformaInfo CustomizeProformaInfo(PMProformaInfo data)
		{
			return data;
		}

		public virtual PMProformaLineInfo CustomizeProformaLineInfo(PMProformaLineInfo data)
		{
			return data;
		}

		public virtual Dictionary<TotalsKey, ProformaTotals> GetProformaTotalsToDate()
		{
			Dictionary<TotalsKey, ProformaTotals> totalsToDate = new Dictionary<TotalsKey, ProformaTotals>();

			var totalsToDateSelect = new PXSelectJoinGroupBy<PMProformaLine,
			InnerJoin<PMProforma, On<PMProformaLine.refNbr, Equal<PMProforma.refNbr>,
				And<PMProformaLine.revisionID, Equal<PMProforma.revisionID>,
				And<PMProforma.curyID, Equal<Current<PMProforma.curyID>>>>>>,
			Where<PMProformaLine.refNbr, LessEqual<Current<PMProforma.refNbr>>,
			And<PMProformaLine.projectID, Equal<Current<PMProforma.projectID>>,
			And<PMProformaLine.type, Equal<PMProformaLineType.progressive>,
			And<PMProformaLine.corrected, NotEqual<True>>>>>,
			Aggregate<GroupBy<PMProformaLine.taskID,
			GroupBy<PMProformaLine.costCodeID,
			GroupBy<PMProformaLine.inventoryID,
			Sum<PMProformaLine.curyRetainage,
			Sum<PMProformaLine.retainage,
			Sum<PMProformaLine.curyLineTotal,
			Sum<PMProformaLine.lineTotal,
			Sum<PMProformaLine.qty>>>>>>>>>>(Base);

			foreach (PMProformaLine line in totalsToDateSelect.Select())
			{
				TotalsKey key = new TotalsKey(line.TaskID.GetValueOrDefault(), line.CostCodeID.GetValueOrDefault(), line.InventoryID.GetValueOrDefault(), 0);

				ProformaTotals totals = new ProformaTotals();
				totals.Key = key;
				totals.CuryRetainage = line.CuryRetainage.GetValueOrDefault();
				totals.Retainage = line.Retainage.GetValueOrDefault();
				totals.CuryLineTotal = line.CuryLineTotal.GetValueOrDefault();
				totals.LineTotal = line.LineTotal.GetValueOrDefault();

				totalsToDate.Add(key, totals);
			}

			return totalsToDate;
		}

		public struct ChangeOrderTotals
		{
			public TotalsKey Key { get; set; }
			public decimal Amount { get; set; }
			public decimal Quantity { get; set; }
		}

		public PXAction<PMProforma> correct;
		[PXUIField(DisplayName = "Correct Pro Forma Invoice")]
		[PXProcessButton]
		public IEnumerable Correct(PXAdapter adapter)
		{
			if (Base.Document.Current != null)
			{
				ValidateAndRaiseExceptionCanCorrect();

				if (Base.Project.Current.RetainageMode != RetainageModes.Normal || Base.Project.Current.SteppedRetainage == true)
				{
					string retainageMode = PX.Objects.PM.Messages.Retainage_Normal;
					switch (Base.Project.Current.RetainageMode)
					{
						case RetainageModes.Contract:
							retainageMode = PX.Objects.PM.Messages.Retainage_Contract;
							break;
						case RetainageModes.Line:
							retainageMode = PX.Objects.PM.Messages.Retainage_Line;
							break;
					}

					string msg = PXLocalizer.LocalizeFormat(PX.Objects.PM.Messages.CorrectRetainageWarning, retainageMode, PX.Objects.PM.Messages.WithSteps, Base.Document.Current.RefNbr);
					WebDialogResult res = Base.DocumentSettings.Ask(msg, MessageButtons.OKCancel);
					if (res == WebDialogResult.Cancel)
					{
						return adapter.Get();
					}
				}


				List<Tuple<PMProformaProgressLine, string, Guid[]>> progressLines = new List<Tuple<PMProformaProgressLine, string, Guid[]>>();
				foreach (PMProformaProgressLine line in Base.ProgressiveLines.Select())
				{
					string note = PXNoteAttribute.GetNote(Base.ProgressiveLines.Cache, line);
					Guid[] files = PXNoteAttribute.GetFileNotes(Base.ProgressiveLines.Cache, line);

					progressLines.Add(new Tuple<PMProformaProgressLine, string, Guid[]>(CreateCorrectionProformaProgressiveLine(line), note, files));

					Base.SubtractFromTotalRetained(line);
					Base.SubtractPerpaymentRemainder(line, -1);
					Base.ProgressiveLines.Cache.SetValue<PMProformaProgressLine.corrected>(line, true);
					Base.ProgressiveLines.Cache.MarkUpdated(line);
				}

				List<Tuple<PMProformaTransactLine, string, Guid[]>> transactionLines = new List<Tuple<PMProformaTransactLine, string, Guid[]>>();
				foreach (PMProformaTransactLine line in Base.TransactionLines.Select())
				{
					string note = PXNoteAttribute.GetNote(Base.TransactionLines.Cache, line);
					Guid[] files = PXNoteAttribute.GetFileNotes(Base.TransactionLines.Cache, line);

					transactionLines.Add(new Tuple<PMProformaTransactLine, string, Guid[]>(CreateCorrectionProformaTransactLine(line), note, files));

					Base.SubtractFromTotalRetained(line);
					Base.SubtractPerpaymentRemainder(line, -1);
					Base.TransactionLines.Cache.SetValue<PMProformaTransactLine.corrected>(line, true);
					Base.TransactionLines.Cache.MarkUpdated(line);
				}

				ProformaEntry target = PXGraph.CreateInstance<ProformaEntry>();
				target.Clear();
				ProformaAutoNumberAttribute.DisableAutonumbiring(target.Document.Cache);
				PXFieldVerifying suppress = (_, e) => e.Cancel = true;
				target.FieldVerifying.AddHandler<PMProforma.finPeriodID>(suppress);
				OpenPeriodAttribute.SetValidatePeriod<PMProforma.finPeriodID>(target.Document.Cache, null, PeriodValidation.Nothing);

				CorrectProforma(target, Base.Document.Current);

				foreach (Tuple<PMProformaProgressLine, string, Guid[]> res in progressLines)
				{
					var line = target.ProgressiveLines.Insert(res.Item1);

					if (res.Item2 != null)
						PXNoteAttribute.SetNote(target.ProgressiveLines.Cache, line, res.Item2);
					if (res.Item3 != null && res.Item3.Length > 0)
						PXNoteAttribute.SetFileNotes(target.ProgressiveLines.Cache, line, res.Item3);
				}

				foreach (Tuple<PMProformaTransactLine, string, Guid[]> res in transactionLines)
				{
					var line = target.TransactionLines.Insert(res.Item1);

					if (res.Item2 != null)
						PXNoteAttribute.SetNote(target.TransactionLines.Cache, line, res.Item2);
					if (res.Item3 != null && res.Item3.Length > 0)
						PXNoteAttribute.SetFileNotes(target.TransactionLines.Cache, line, res.Item3);
				}

				using (var ts = new PXTransactionScope())
				{
					Base.Save.Press();
					target.SelectTimeStamp();
					target.Save.Press();
					ts.Complete();
				}

				PXRedirectHelper.TryRedirect(target, PXRedirectHelper.WindowMode.Same);
			}

			return adapter.Get();
		}

		public virtual PMProforma CorrectProforma(ProformaEntry target, PMProforma doc)
		{
			PMProforma correction = CreateCorrectionProforma(doc);
			string docNote = PXNoteAttribute.GetNote(Base.Document.Cache, doc);
			Guid[] docFiles = PXNoteAttribute.GetFileNotes(Base.Document.Cache, doc);

			Base.Document.Cache.SetValue<PMProforma.corrected>(doc, true);
			Base.Document.Cache.MarkUpdated(doc);

			correction = target.Document.Insert(correction);
			if (docNote != null)
				PXNoteAttribute.SetNote(target.Document.Cache, correction, docNote);
			if (docFiles != null && docFiles.Length > 0)
				PXNoteAttribute.SetFileNotes(target.Document.Cache, correction, docFiles);

			var selectFutureProformas = new PXSelect<PMProforma,
				Where<PMProforma.projectID, Equal<Required<PMProforma.projectID>>,
				And<PMProforma.refNbr, Greater<Required<PMProforma.refNbr>>>>>(Base);

			foreach (PMProforma proforma in selectFutureProformas.Select(doc.ProjectID, doc.RefNbr))
			{
				Base.Document.Cache.SetValue<PMProforma.isAIAOutdated>(proforma, true);
				Base.Document.Cache.MarkUpdated(proforma);
			}

			return correction;
		}

		public virtual void ValidateAndRaiseExceptionCanCorrect()
		{
			if (Base.TransactionLines.Select().Count > 0)
			{
				throw new PXException(PX.Objects.PM.Messages.CannotCorrectContainsTM);
			}

			if (!string.IsNullOrEmpty(Base.Document.Current.ARInvoiceRefNbr))
			{
				var selectRetainageInvoices = new PXSelect<ARInvoice, Where<ARInvoice.isRetainageDocument, Equal<True>,
					And<ARInvoice.origDocType, Equal<Current<PMProforma.aRInvoiceDocType>>,
					And<ARInvoice.origRefNbr, Equal<Current<PMProforma.aRInvoiceRefNbr>>>>>>(Base);

				StringBuilder sb = new StringBuilder();
				foreach (ARInvoice doc in selectRetainageInvoices.Select())
				{
					sb.AppendFormat(" {0}.{1},", doc.DocType, doc.RefNbr);
				}

				string docList = sb.ToString();
				if (!string.IsNullOrEmpty(docList))
				{
					throw new PXException(PX.Objects.PM.Messages.CannotCorrectContainsRetainage, docList.TrimEnd(','));
				}


				var selectAdjustments = new PXSelectGroupBy<ARAdjust,
					Where<ARAdjust.adjdDocType, Equal<Current<PMProforma.aRInvoiceDocType>>,
					And<ARAdjust.adjdRefNbr, Equal<Current<PMProforma.aRInvoiceRefNbr>>,
					And<ARAdjust.voided, Equal<False>>>>,
					Aggregate<GroupBy<ARAdjust.adjgDocType, GroupBy<ARAdjust.adjgRefNbr>>>>(Base);

				sb.Clear();
				foreach (ARAdjust doc in selectAdjustments.Select())
				{
					sb.AppendFormat(" {0}.{1},", doc.AdjgDocType, doc.AdjgRefNbr);
				}

				docList = sb.ToString();
				if (!string.IsNullOrEmpty(docList))
				{
					throw new PXException(PX.Objects.PM.Messages.CannotCorrectContainsApplications, string.Format("{0}.{1}", Base.Document.Current.ARInvoiceDocType, Base.Document.Current.ARInvoiceRefNbr), docList.TrimEnd(','));
				}
			}
		}

		public virtual PMProforma CreateCorrectionProforma(PMProforma original)
		{
			PMProforma proforma = (PMProforma)Base.Document.Cache.CreateCopy(original);
			proforma.RevisionID++;
			proforma.Status = null;
			proforma.Hold = null;
			proforma.Approved = null;
			proforma.Rejected = null;
			proforma.Released = null;
			proforma.Corrected = null;
			proforma.ARInvoiceDocType = null;
			proforma.ARInvoiceRefNbr = null;
			proforma.TransactionalTotal = null;
			proforma.CuryTransactionalTotal = null;
			proforma.ProgressiveTotal = null;
			proforma.CuryProgressiveTotal = null;
			proforma.RetainageDetailTotal = null;
			proforma.CuryRetainageDetailTotal = null;
			proforma.RetainageTaxTotal = null;
			proforma.CuryRetainageTaxTotal = null;
			proforma.TaxTotal = null;
			proforma.CuryTaxTotal = null;
			proforma.DocTotal = null;
			proforma.CuryDocTotal = null;
			proforma.CuryAllocatedRetainedTotal = null;
			proforma.AllocatedRetainedTotal = null;
			proforma.IsTaxValid = null;
			proforma.tstamp = null;
			proforma.NoteID = null;
			proforma.IsAIAOutdated = true;

			return proforma;
		}

		public virtual PMProformaTransactLine CreateCorrectionProformaTransactLine(PMProformaTransactLine original)
		{
			PMProformaTransactLine line = (PMProformaTransactLine)Base.TransactionLines.Cache.CreateCopy(original);
			line.Released = null;
			line.Corrected = null;
			line.ARInvoiceDocType = null;
			line.ARInvoiceRefNbr = null;
			line.ARInvoiceLineNbr = null;
			line.RevisionID = null;
			line.NoteID = null;
			line.tstamp = null;

			return line;
		}

		public virtual PMProformaProgressLine CreateCorrectionProformaProgressiveLine(PMProformaProgressLine original)
		{
			PMProformaProgressLine line = (PMProformaProgressLine)Base.ProgressiveLines.Cache.CreateCopy(original);
			line.Released = null;
			line.Corrected = null;
			line.ARInvoiceDocType = null;
			line.ARInvoiceRefNbr = null;
			line.ARInvoiceLineNbr = null;
			line.RevisionID = null;
			line.NoteID = null;
			line.tstamp = null;

			return line;
		}

		public virtual bool CanBeCorrected(PMProforma row)
		{
			if (row.Released != true)
				return false;

			if (row.Corrected == true)
				return false;

			ARInvoice arDoc = PXSelect<ARInvoice, Where<ARInvoice.docType, Equal<Current<PMProforma.aRInvoiceDocType>>,
				And<ARInvoice.refNbr, Equal<Current<PMProforma.aRInvoiceRefNbr>>>>>.Select(Base);

			if (arDoc != null && arDoc.Released != true)
			{
				return false;
			}

			return true;
		}
				

		[PXOverride]
		public virtual void InitalizeActionsMenu(Action baseMethod)
		{
			baseMethod();
			Base.action.AddMenuAction(correct);
		}
			
		protected virtual void _(Events.RowSelected<PMProforma> e)
		{
			if (Base.SuppressRowSeleted)
				return;

			if (e.Row != null)
			{
				correct.SetEnabled(CanBeCorrected(e.Row));
				PXUIFieldAttribute.SetEnabled<PMProforma.projectNbr>(e.Cache, e.Row, e.Row.Hold == true);
			}
		}

		protected virtual void _(Events.FieldVerifying<PMProforma, PMProforma.projectNbr> e)
		{
			string val = (string)e.NewValue;

			if (string.IsNullOrEmpty(val) || val.Equals(PX.Objects.PM.Messages.NotAvailable, StringComparison.InvariantCultureIgnoreCase))
				return;

			var selectDuplicate = new PXSelect<PMProforma, Where<PMProforma.projectID, Equal<Current<PMProforma.projectID>>,
				And<PMProforma.projectNbr, Equal<Required<PMProforma.projectNbr>>,
				And<PMProforma.corrected, NotEqual<True>>>>>(Base);

			if (e.Cache.GetStatus(e.Row) != PXEntryStatus.Inserted)
			{
				selectDuplicate.WhereAnd<Where<PMProforma.refNbr, NotEqual<Current<PMProforma.refNbr>>>>();
			}

			PMProforma duplicate = selectDuplicate.Select(e.NewValue);

			if (duplicate != null)
			{
				throw new PXSetPropertyException(PX.Objects.PM.Messages.DuplicateProformaNumber, duplicate.RefNbr);
			}
		}

		protected virtual void _(Events.FieldUpdated<PMProforma, PMProforma.projectNbr> e)
		{
			PMProject project = ProjectProperties.Select();
			if (project != null && e.Row.ProjectNbr != PX.Objects.PM.Messages.NotAvailable)
			{
				if (string.Compare(e.Row.ProjectNbr, project.LastProformaNumber, StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					project.LastProformaNumber = e.Row.ProjectNbr;
					ProjectProperties.Update(project);
				}
			}
		}

		protected virtual void _(Events.RowDeleted<PMProforma> e)
		{
			PMProject project = ProjectProperties.Select();
			if (project != null && project.LastProformaNumber == e.Row.ProjectNbr)
			{
				project.LastProformaNumber = DecNumber(project.LastProformaNumber);
				ProjectProperties.Update(project);
			}
		}

		public virtual string DecNumber(string lastProformarNumber)
		{
			if (string.IsNullOrEmpty(lastProformarNumber))
				return null;

			char[] lastNumber = lastProformarNumber.ToCharArray();
			for (int i = lastNumber.Length - 1; i >= 0; i--)
			{
				if (char.IsDigit(lastNumber[i]))
				{
					int lastDigit = int.Parse(new string(lastNumber[i], 1));
					if (lastDigit != 0)
					{
						lastDigit--;
						lastNumber[i] = lastDigit.ToString().ToCharArray()[0];
						for (int j = i + 1; j < lastNumber.Length; j++)
						{
							lastNumber[j] = '9';
						}

						break;
					}
				}
				else
				{
					break;
				}
			}

			return new string(lastNumber);
		}
	}
}
