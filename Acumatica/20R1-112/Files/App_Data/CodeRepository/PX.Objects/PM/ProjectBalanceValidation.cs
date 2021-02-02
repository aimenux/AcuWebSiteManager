using PX.SM;
using System;
using PX.Data;
using System.Collections.Generic;
using System.Collections;
using PX.Objects.GL;
using System.Diagnostics;
using PX.Objects.PO;
using PX.Objects.SO;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;
using CommonServiceLocator;
using PX.Objects.CM;

namespace PX.Objects.PM
{
	public class ProjectBalanceValidation : PXGraph<ProjectBalanceValidation>
	{
		public PXCancel<PMValidationFilter> Cancel;
		public PXFilter<PMValidationFilter> Filter;
		public PXFilteredProcessing<PMProject, PMValidationFilter, Where<PMProject.baseType, Equal<CT.CTPRType.project>,
			And<PMProject.nonProject, Equal<False>,
			And2<Match<PMProject, Current<AccessInfo.userName>>,
			And<Where<PMProject.isActive, Equal<True>, Or<PMProject.isCompleted, Equal<True>>>>>>>> Items;


		public ProjectBalanceValidation()
		{
			Items.SetSelected<PMProject.selected>();
			Items.SetProcessCaption(GL.Messages.ProcValidate);
			Items.SetProcessAllCaption(GL.Messages.ProcValidateAll);
		}

		public PXAction<PMValidationFilter> viewProject;
		[PXUIField(DisplayName = Messages.ViewProject, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewProject(PXAdapter adapter)
		{
			ProjectEntry graph = CreateInstance<ProjectEntry>();
			graph.Project.Current = PXSelect<PMProject, Where<PMProject.contractCD, Equal<Current<PMProject.contractCD>>>>.Select(this);
			throw new PXRedirectRequiredException(graph, true, Messages.ViewProject) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
		}

		protected virtual void PMValidationFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			PMValidationFilter filter = Filter.Current;

			Items.SetProcessDelegate<ProjectBalanceValidationProcess>(
					delegate (ProjectBalanceValidationProcess graph, PMProject item)
					{
						graph.Clear();
						graph.RunProjectBalanceVerification(item, filter);
						graph.Actions.PressSave();
					});
		}
	}

	[Serializable]
	public class ProjectBalanceValidationProcess : PXGraph<ProjectBalanceValidationProcess>
	{
		#region DAC Overrides
		[POCommitment]
		[PXDBGuid]
		protected virtual void POLine_CommitmentID_CacheAttached(PXCache sender) { }

		[PXDBString(2, IsKey = true, IsFixed = true)]
		protected virtual void POLine_OrderType_CacheAttached(PXCache sender) { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXParent(typeof(Select<POOrder, Where<POOrder.orderType, Equal<Current<POLine.orderType>>, And<POOrder.orderNbr, Equal<Current<POLine.orderNbr>>>>>))]
		protected virtual void POLine_OrderNbr_CacheAttached(PXCache sender) { }

		[PXDBDate()]
		protected virtual void POLine_OrderDate_CacheAttached(PXCache sender) { }

		[PXDBInt]
		protected virtual void POLine_VendorID_CacheAttached(PXCache sender) { }

		[SOCommitment]
		[PXDBGuid]
		protected virtual void SOLine_CommitmentID_CacheAttached(PXCache sender) { }

		[PXDBString(2, IsKey = true, IsFixed = true)]
		protected virtual void SOLine_OrderType_CacheAttached(PXCache sender) { }

		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXParent(typeof(Select<SOOrder, Where<SOOrder.orderType, Equal<Current<SOLine.orderType>>, And<SOOrder.orderNbr, Equal<Current<SOLine.orderNbr>>>>>))]
		protected virtual void SOLine_OrderNbr_CacheAttached(PXCache sender) { }

		[PXDBDate()]
		protected virtual void SOLine_OrderDate_CacheAttached(PXCache sender) { }

		[PXDefault]
		[PXDBInt] //NO Selector Validation
		protected virtual void PMCommitment_ProjectID_CacheAttached(PXCache sender) { }

		[PXDefault]
		[PXDBInt] //NO Selector Validation
		protected virtual void PMCommitment_ProjectTaskID_CacheAttached(PXCache sender) { }
		#endregion


		public PXSelect<PMBudgetAccum> Budget;
		public PXSelect<PMForecastHistoryAccum> ForecastHistory;
		public PXSelect<PMTaskTotal> TaskTotals;
		public PXSelect<PMTaskAllocTotalAccum> AllocationTotals;
		public PXSelect<PMHistoryAccum> History;
		public PXSelect<PMTran, Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
				And<PMTran.released, Equal<True>>>> Transactions;
		public PXSelectJoin<POLine,
			InnerJoin<POOrder, On<POOrder.orderType, Equal<POLine.orderType>, And<POOrder.orderNbr, Equal<POLine.orderNbr>>>>,
			Where<POLine.projectID, Equal<Required<POLine.projectID>>>> polines;
		public PXSelectJoin<APTran, 
			InnerJoin<APRegister, On<APRegister.docType, Equal<APTran.tranType>, And<APRegister.refNbr, Equal<APTran.refNbr>>>>,
			Where<APTran.released, Equal<True>, And<APTran.projectID, Equal<Required<APTran.projectID>>, And<APTran.pOLineNbr, IsNotNull>>>> aptran;
		public PXSelect<ARTran, Where<ARTran.released, Equal<True>, And<ARTran.projectID, Equal<Required<ARTran.projectID>>, And<ARTran.sOOrderLineNbr, IsNotNull>>>> artran;
		public PXSelectJoin<SOLine,
			InnerJoin<SOOrder, On<SOOrder.orderType, Equal<SOLine.orderType>, And<SOOrder.orderNbr, Equal<SOLine.orderNbr>>>>,
			Where<SOLine.projectID, Equal<Required<SOLine.projectID>>>> solines;
		public PMCommitmentSelect<PMCommitment, Where<PMCommitment.type, Equal<PMCommitmentType.externalType>, And<PMCommitment.projectID, Equal<Required<PMCommitment.projectID>>>>> ExternalCommitments;
		public PXSetup<PMSetup> Setup;
		public Dictionary<int, PMAccountGroup> AccountGroups;
		public Dictionary<int, Account> Accounts;

		private BudgetServiceMassUpdate budgetService;

		private IFinPeriodRepository finPeriodsRepo;
		public virtual IFinPeriodRepository FinPeriodRepository
		{
			get
			{
				if (finPeriodsRepo == null)
				{
					finPeriodsRepo = new FinPeriodRepository(this);
				}

				return finPeriodsRepo;
			}
		}

		public virtual void RunProjectBalanceVerification(PMProject project, PMValidationFilter options)
		{
			budgetService = new BudgetServiceMassUpdate(this, project);
			InitAccountGroup();
			InitAccounts();

			PXDatabase.Delete<PMTaskTotal>(new PXDataFieldRestrict<PMTaskTotal.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ));
			PXDatabase.Delete<PMTaskAllocTotal>(new PXDataFieldRestrict<PMTaskAllocTotal.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ));
			PXDatabase.Delete<PMHistory>(new PXDataFieldRestrict<PMHistory.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ));

			if (options.RebuildCommitments == true)
			{
				PXDatabase.Delete<PMCommitment>(new PXDataFieldRestrict<PMCommitment.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ),
				new PXDataFieldRestrict<PMCommitment.type>(PXDbType.Char, 1, PMCommitmentType.Internal, PXComp.EQ));
			}

			if (options.RecalculateUnbilledSummary == true)
			{
				PXDatabase.Delete<PMUnbilledDailySummary>(new PXDataFieldRestrict(typeof(PMUnbilledDailySummary.projectID).Name, PXDbType.Int, 4, project.ContractID, PXComp.EQ));
			}

			ProjectBalance pb = CreateProjectBalance();

			if (options.RecalculateChangeOrders == true && project.ChangeOrderWorkflow == true)
			{
				foreach (PMBudgetLiteEx record in budgetService.BudgetRecords)
				{
					List<PXDataFieldParam> list = BuildBudgetClearCommandWithChangeOrders(options, record);
					PXDatabase.Update<PMBudget>(list.ToArray());
				}
			}
			else
			{
				foreach(int accountGroupID in budgetService.GetUsedAccountGroups() )
				{
					List<PXDataFieldParam> list = BuildBudgetClearCommand(options, project, accountGroupID);
					PXDatabase.Update<PMBudget>(list.ToArray());
				}
			}

			List<PXDataFieldParam> listForecast = BuildForecastClearCommand(options, project.ContractID);
			PXDatabase.Update<PMForecastHistory>(listForecast.ToArray());

			PXSelectBase<PMTran> select = null;

			if (options.RecalculateUnbilledSummary == true)
			{
				select = new PXSelectGroupBy<PMTran,
				Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
				And<PMTran.released, Equal<True>>>,
				Aggregate<GroupBy<PMTran.tranType,
				GroupBy<PMTran.branchID,
				GroupBy<PMTran.finPeriodID,
				GroupBy<PMTran.tranPeriodID,
				GroupBy<PMTran.projectID,
				GroupBy<PMTran.taskID,
				GroupBy<PMTran.inventoryID,
				GroupBy<PMTran.costCodeID,
				GroupBy<PMTran.date,
				GroupBy<PMTran.accountID,
				GroupBy<PMTran.accountGroupID,
				GroupBy<PMTran.offsetAccountID,
				GroupBy<PMTran.offsetAccountGroupID,
				GroupBy<PMTran.uOM,
				GroupBy<PMTran.released,
				GroupBy<PMTran.remainderOfTranID,
				Sum<PMTran.qty,
				Sum<PMTran.amount,
				Sum<PMTran.projectCuryAmount,
				Max<PMTran.billable,
				GroupBy<PMTran.billed,
				GroupBy<PMTran.excludedFromBilling>>>>>>>>>>>>>>>>>>>>>>>>(this);
			}
			else
			{
				select = new PXSelectGroupBy<PMTran,
				Where<PMTran.projectID, Equal<Required<PMTran.projectID>>,
				And<PMTran.released, Equal<True>>>,
				Aggregate<GroupBy<PMTran.tranType,
				GroupBy<PMTran.branchID,
				GroupBy<PMTran.finPeriodID,
				GroupBy<PMTran.tranPeriodID,
				GroupBy<PMTran.projectID,
				GroupBy<PMTran.taskID,
				GroupBy<PMTran.inventoryID,
				GroupBy<PMTran.costCodeID,
				GroupBy<PMTran.accountID,
				GroupBy<PMTran.accountGroupID,
				GroupBy<PMTran.offsetAccountID,
				GroupBy<PMTran.offsetAccountGroupID,
				GroupBy<PMTran.uOM,
				GroupBy<PMTran.released,
				GroupBy<PMTran.remainderOfTranID,
				Sum<PMTran.qty,
				Sum<PMTran.amount,
				Sum<PMTran.projectCuryAmount>>>>>>>>>>>>>>>>>>>>(this);
			}

			using (new PXFieldScope(select.View
				, typeof(PMTran.tranID)
				, typeof(PMTran.tranType)
				, typeof(PMTran.branchID)
				, typeof(PMTran.finPeriodID)
				, typeof(PMTran.tranPeriodID)
				, typeof(PMTran.tranDate)
				, typeof(PMTran.date)
				, typeof(PMTran.projectID)
				, typeof(PMTran.taskID)
				, typeof(PMTran.inventoryID)
				, typeof(PMTran.costCodeID)
				, typeof(PMTran.accountID)
				, typeof(PMTran.accountGroupID)
				, typeof(PMTran.offsetAccountID)
				, typeof(PMTran.offsetAccountGroupID)
				, typeof(PMTran.uOM)
				, typeof(PMTran.released)
				, typeof(PMTran.remainderOfTranID)
				, typeof(PMTran.qty)
				, typeof(PMTran.amount)
				, typeof(PMTran.projectCuryAmount)
				, typeof(PMTran.billable)
				, typeof(PMTran.billed)
				, typeof(PMTran.excludedFromBilling)
				, typeof(PMTran.excludedFromBalance)))
			{
				foreach (PMTran tran in select.Select(project.ContractID))
				{
					Account account = null;
					Account offsetAccount = null;
					PMAccountGroup accountGroup = null;
					PMAccountGroup offsetAccountGroup = null;

					#region Init Account and AccountGroups emulating BQL's LEFT JOIN

					if (!AccountGroups.TryGetValue(tran.AccountGroupID.Value, out accountGroup))
					{
						accountGroup = new PMAccountGroup();
					}

					if (tran.AccountID == null)
					{
						account = new Account();
					}
					else
					{
						if (!Accounts.TryGetValue(tran.AccountID.Value, out account))
						{
							account = new Account();
						}
					}

					if (tran.OffsetAccountID == null)
					{
						offsetAccount = new Account();
						offsetAccountGroup = new PMAccountGroup();
					}
					else
					{
						if (!Accounts.TryGetValue(tran.OffsetAccountID.Value, out offsetAccount))
						{
							offsetAccount = new Account();
							offsetAccountGroup = new PMAccountGroup();
						}
						else
						{
							if (!AccountGroups.TryGetValue(offsetAccount.AccountGroupID.Value, out offsetAccountGroup))
							{
								offsetAccountGroup = new PMAccountGroup();
							}
						}
					}
					#endregion

					RegisterReleaseProcess.AddToUnbilledSummary(this, tran);

					if (tran.ExcludedFromBalance == true)
						continue;

					ProcessTransaction(project, tran, account, accountGroup, offsetAccount, offsetAccountGroup, pb);
				}
			}

			RebuildAllocationTotals(project);

			if (options.RebuildCommitments == true)
			{
				ProcessPOCommitments(project);
				if (Setup.Current.RevenueCommitmentTracking == true)
				ProcessSOCommitments(project);
				ProcessExternalCommitments(project);
			}

			if (options.RecalculateDraftInvoicesAmount == true)
			{
				RecalculateDraftInvoicesAmount(project, pb);
			}

			if (options.RecalculateChangeOrders == true && project.ChangeOrderWorkflow == true)
			{
                RecalculateChangeRequests(project, pb);
                RecalculateChangeOrders(project, pb);
			}

			InitCostCodeOnModifiedEntities();

			if (PXAccess.FeatureInstalled<FeaturesSet.retainage>())
			{
				ProcessRetainage(project, options);
			}
		}

		public virtual void ProcessTransaction(PMProject project, PMTran tran, Account acc, PMAccountGroup ag, Account offsetAcc, PMAccountGroup offsetAg, ProjectBalance pb)
		{
			IList<ProjectBalance.Result> balances = pb.Calculate(project, tran, acc, ag, offsetAcc, offsetAg);

			foreach (ProjectBalance.Result balance in balances)
			{
				if (balance.Status != null)
				{
					PMBudgetAccum ps = new PMBudgetAccum();
					ps.ProjectID = balance.Status.ProjectID;
					ps.ProjectTaskID = balance.Status.ProjectTaskID;
					ps.AccountGroupID = balance.Status.AccountGroupID;
					ps.InventoryID = balance.Status.InventoryID;
					ps.CostCodeID = balance.Status.CostCodeID;
					ps.UOM = balance.Status.UOM;
					ps.IsProduction = balance.Status.IsProduction;
					ps.Type = balance.Status.Type;
					ps.Description = balance.Status.Description;
					ps.CuryInfoID = balance.Status.CuryInfoID;

					ps = Budget.Insert(ps);

					ps.ActualQty += balance.Status.ActualQty.GetValueOrDefault();
					ps.CuryActualAmount += balance.Status.CuryActualAmount.GetValueOrDefault();
					ps.ActualAmount += balance.Status.ActualAmount.GetValueOrDefault();
				}

				if (balance.ForecastHistory != null)
				{
					PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
					forecast.ProjectID = balance.ForecastHistory.ProjectID;
					forecast.ProjectTaskID = balance.ForecastHistory.ProjectTaskID;
					forecast.AccountGroupID = balance.ForecastHistory.AccountGroupID;
					forecast.InventoryID = balance.ForecastHistory.InventoryID;
					forecast.CostCodeID = balance.ForecastHistory.CostCodeID;
					forecast.PeriodID = balance.ForecastHistory.PeriodID;

					forecast = ForecastHistory.Insert(forecast);

					forecast.ActualQty += balance.ForecastHistory.ActualQty.GetValueOrDefault();
					forecast.CuryActualAmount += balance.ForecastHistory.CuryActualAmount.GetValueOrDefault();
					forecast.ActualAmount += balance.ForecastHistory.ActualAmount.GetValueOrDefault();
				}

				if (balance.TaskTotal != null)
				{
					PMTaskTotal ta = new PMTaskTotal();
					ta.ProjectID = balance.TaskTotal.ProjectID;
					ta.TaskID = balance.TaskTotal.TaskID;

					ta = TaskTotals.Insert(ta);
					ta.CuryAsset += balance.TaskTotal.CuryAsset.GetValueOrDefault();
					ta.Asset += balance.TaskTotal.Asset.GetValueOrDefault();
					ta.CuryLiability += balance.TaskTotal.CuryLiability.GetValueOrDefault();
					ta.Liability += balance.TaskTotal.Liability.GetValueOrDefault();
					ta.CuryIncome += balance.TaskTotal.CuryIncome.GetValueOrDefault();
					ta.Income += balance.TaskTotal.Income.GetValueOrDefault();
					ta.CuryExpense += balance.TaskTotal.CuryExpense.GetValueOrDefault();
					ta.Expense += balance.TaskTotal.Expense.GetValueOrDefault();
				}


				foreach (PMHistory item in balance.History)
				{
					PMHistoryAccum hist = new PMHistoryAccum();
					hist.ProjectID = item.ProjectID;
					hist.ProjectTaskID = item.ProjectTaskID;
					hist.AccountGroupID = item.AccountGroupID;
					hist.InventoryID = item.InventoryID;
					hist.CostCodeID = item.CostCodeID;
					hist.PeriodID = item.PeriodID;
					hist.BranchID = item.BranchID;

					hist = History.Insert(hist);
					hist.FinPTDCuryAmount += item.FinPTDCuryAmount.GetValueOrDefault();
					hist.FinPTDAmount += item.FinPTDAmount.GetValueOrDefault();
					hist.FinYTDCuryAmount += item.FinYTDCuryAmount.GetValueOrDefault();
					hist.FinYTDAmount += item.FinYTDAmount.GetValueOrDefault();
					hist.FinPTDQty += item.FinPTDQty.GetValueOrDefault();
					hist.FinYTDQty += item.FinYTDQty.GetValueOrDefault();
					hist.TranPTDCuryAmount += item.TranPTDCuryAmount.GetValueOrDefault();
					hist.TranPTDAmount += item.TranPTDAmount.GetValueOrDefault();
					hist.TranYTDCuryAmount += item.TranYTDCuryAmount.GetValueOrDefault();
					hist.TranYTDAmount += item.TranYTDAmount.GetValueOrDefault();
					hist.TranPTDQty += item.TranPTDQty.GetValueOrDefault();
					hist.TranYTDQty += item.TranYTDQty.GetValueOrDefault();
				}
			}
		}

		public virtual void RebuildAllocationTotals(PMProject project)
		{
			PXSelectBase<PMTran> select2 = new PXSelect<PMTran,
				Where<PMTran.origProjectID, Equal<Required<PMTran.origProjectID>>,
				And<PMTran.origTaskID, IsNotNull,
				And<PMTran.origAccountGroupID, IsNotNull>>>>(this);

			using (new PXFieldScope(select2.View
				, typeof(PMTran.tranID)
				, typeof(PMTran.origProjectID)
				, typeof(PMTran.origTaskID)
				, typeof(PMTran.inventoryID)
				, typeof(PMTran.origAccountGroupID)
				, typeof(PMTran.qty)
				, typeof(PMTran.amount)))
			{
				foreach (PMTran tran in select2.Select(project.ContractID))
				{
					PMTaskAllocTotalAccum tat = new PMTaskAllocTotalAccum();
					tat.ProjectID = tran.OrigProjectID;
					tat.TaskID = tran.OrigTaskID;
					tat.AccountGroupID = tran.OrigAccountGroupID;
					tat.InventoryID = tran.InventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID);
					tat.CostCodeID = tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());

					tat = AllocationTotals.Insert(tat);
					tat.Amount += tran.Amount.GetValueOrDefault();
					tat.Quantity += tran.Qty.GetValueOrDefault();
				}
			}
		}

		public virtual void ProcessPOCommitments(PMProject project)
		{
			Dictionary<string, string> curyIdByOrderNbr = new Dictionary<string, string>(); 
			foreach (PXResult<POLine, POOrder> res in polines.Select(project.ContractID))
			{
				POLine poline = (POLine)res;
				POOrder order = (POOrder)res;
				PXParentAttribute.SetParent(polines.Cache, poline, typeof(POOrder), order);

				PMCommitmentAttribute.Sync(polines.Cache, poline);

				
			}

			foreach (PXResult<APTran, APRegister> res in aptran.Select(project.ContractID))
			{
				APTran tran = (APTran)res;
				APRegister doc = (APRegister)res;

				POLine poline = new POLine();
				poline.OrderType = tran.POOrderType;
				poline.OrderNbr = tran.PONbr;
				poline.LineNbr = tran.POLineNbr;

				poline = polines.Locate(poline);

				if (poline != null && poline.CommitmentID != null)
				{
					decimal sign = (tran.DrCr == DrCr.Debit) ? Decimal.One : Decimal.MinusOne;

					PMCommitment container = new PMCommitment();
					container.ProjectID = poline.ProjectID;
					container.CommitmentID = poline.CommitmentID;
					container.UOM = tran.UOM;
					container.InventoryID = tran.InventoryID;
					container.CostCodeID = tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());
					container.InvoicedAmount = sign * tran.CuryLineAmt.GetValueOrDefault();
					container.InvoicedQty = sign * tran.Qty.GetValueOrDefault();

					PMCommitmentAttribute.AddToInvoiced(polines.Cache, container, doc.CuryID, doc.DocDate);
				}
			}
		}

		public virtual void ProcessSOCommitments(PMProject project)
		{
			foreach (PXResult<SOLine, SOOrder> res in solines.Select(project.ContractID))
			{
				SOLine soline = (SOLine)res;
				SOOrder order = (SOOrder)res;
				PXParentAttribute.SetParent(solines.Cache, soline, typeof(SOOrder), order);

				PMCommitmentAttribute.Sync(solines.Cache, soline);
			}

			foreach (ARTran tran in artran.Select(project.ContractID))
			{
				SOLine soline = new SOLine();
				soline.OrderType = tran.SOOrderType;
				soline.OrderNbr = tran.SOOrderNbr;
				soline.LineNbr = tran.SOOrderLineNbr;

				soline = solines.Locate(soline);

				if (soline != null && soline.CommitmentID != null)
				{
					decimal sign = (tran.DrCr == DrCr.Credit) ? Decimal.One : Decimal.MinusOne;

					PMCommitment container = new PMCommitment();
					container.ProjectID = soline.ProjectID;
					container.CommitmentID = soline.CommitmentID;
					container.UOM = tran.UOM;
					container.InventoryID = tran.InventoryID;
					container.CostCodeID = tran.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());
					container.InvoicedAmount = sign * tran.CuryTranAmt.GetValueOrDefault();
					container.InvoicedQty = sign * tran.Qty.GetValueOrDefault();

					SOOrder order = (SOOrder)PXParentAttribute.SelectParent(solines.Cache, soline, typeof(SOOrder));

					PMCommitmentAttribute.AddToInvoiced(solines.Cache, container, order.CuryID, order.OrderDate);
				}
			}
		}

		public virtual void ProcessExternalCommitments(PMProject project)
		{
			foreach (PMCommitment item in ExternalCommitments.Select(project.ContractID))
			{
				ExternalCommitments.RollUpCommitmentBalance(polines.Cache, item, 1);
			}
		}

		public virtual void RecalculateDraftInvoicesAmount(PMProject project, ProjectBalance pb)
		{
			var selectProforma = new PXSelectJoinGroupBy<PMProformaLine,
				InnerJoin<Account, On<PMProformaLine.accountID, Equal<Account.accountID>>,
				InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Account.accountGroupID>>>>,
				Where<PMProformaLine.projectID, Equal<Required<PMProformaLine.projectID>>,
				And<PMProformaLine.released, Equal<False>>>,
				Aggregate<GroupBy<PMProformaLine.projectID,
				GroupBy<PMProformaLine.taskID,
				GroupBy<PMProformaLine.accountID,
				GroupBy<PMProformaLine.inventoryID,
				GroupBy<PMProformaLine.costCodeID,
				Sum<PMProformaLine.curyLineTotal,
				Sum<PMProformaLine.lineTotal>>>>>>>>>(this);

			var selectInvoice = new PXSelectJoinGroupBy<ARTran,
				InnerJoin<Account, On<ARTran.accountID, Equal<Account.accountID>>,
				InnerJoin<PMAccountGroup, On<PMAccountGroup.groupID, Equal<Account.accountGroupID>>>>,
				Where<ARTran.projectID, Equal<Required<ARTran.projectID>>,
				And<ARTran.released, Equal<False>>>,
				Aggregate<GroupBy<ARTran.tranType,
				GroupBy<ARTran.projectID,
				GroupBy<ARTran.taskID,
				GroupBy<ARTran.accountID,
				GroupBy<ARTran.inventoryID,
				GroupBy<ARTran.costCodeID,
				Sum<ARTran.curyTranAmt,
				Sum<ARTran.tranAmt>>>>>>>>>>(this);


			var selectRevenueBudget = new PXSelect<PMRevenueBudget,
									Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
									And<PMRevenueBudget.type, Equal<GL.AccountType.income>>>>(this);
			var revenueBudget = selectRevenueBudget.Select(project.ContractID);

			foreach (PXResult<PMProformaLine, Account, PMAccountGroup> res in selectProforma.Select(project.ContractID))
			{
				PMProformaLine line = (PMProformaLine)res;
				Account account = (Account)res;
				PMAccountGroup ag = (PMAccountGroup)res;

				int? inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;

				foreach (PMRevenueBudget rev in revenueBudget)
				{
					foreach (PMRevenueBudget budget in revenueBudget)
					{
						if (budget.TaskID == line.TaskID && line.InventoryID == budget.InventoryID)
						{
							inventoryID = line.InventoryID;
						}
					}
				}

				PMBudgetAccum invoiced = new PMBudgetAccum();
				invoiced.Type = ag.Type;
				invoiced.ProjectID = line.ProjectID;
				invoiced.ProjectTaskID = line.TaskID;
				invoiced.AccountGroupID = account.AccountGroupID;
				invoiced.InventoryID = inventoryID;
				invoiced.CostCodeID = line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());

				invoiced = Budget.Insert(invoiced);

				if (project.CuryID == project.BillingCuryID)
				{
					invoiced.CuryInvoicedAmount += line.CuryLineTotal.GetValueOrDefault();
					invoiced.InvoicedAmount += line.LineTotal.GetValueOrDefault();

					if (line.IsPrepayment == true)
					{
						invoiced.CuryPrepaymentInvoiced += line.CuryLineTotal.GetValueOrDefault();
						invoiced.PrepaymentInvoiced += line.LineTotal.GetValueOrDefault();
					}
				}
				else
				{
					invoiced.CuryInvoicedAmount += line.LineTotal.GetValueOrDefault();
					invoiced.InvoicedAmount += line.LineTotal.GetValueOrDefault();

					if (line.IsPrepayment == true)
					{
						invoiced.CuryPrepaymentInvoiced += line.LineTotal.GetValueOrDefault();
						invoiced.PrepaymentInvoiced += line.LineTotal.GetValueOrDefault();
					}
				}
				
				
			}

			foreach (PXResult<ARTran, Account, PMAccountGroup> res in selectInvoice.Select(project.ContractID))
			{
				ARTran line = (ARTran)res;
				Account account = (Account)res;
				PMAccountGroup ag = (PMAccountGroup)res;

				int? inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;

				foreach (PMRevenueBudget rev in revenueBudget)
				{
					foreach (PMRevenueBudget budget in revenueBudget)
					{
						if (budget.TaskID == line.TaskID && line.InventoryID == budget.InventoryID)
						{
							inventoryID = line.InventoryID;
						}
					}
				}

				PMBudgetAccum invoiced = new PMBudgetAccum();
				invoiced.Type = ag.Type;
				invoiced.ProjectID = line.ProjectID;
				invoiced.ProjectTaskID = line.TaskID;
				invoiced.AccountGroupID = account.AccountGroupID;
				invoiced.InventoryID = inventoryID;
				invoiced.CostCodeID = line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());

				invoiced = Budget.Insert(invoiced);

				if (project.CuryID == project.BillingCuryID)
				{
					invoiced.CuryInvoicedAmount += line.CuryTranAmt.GetValueOrDefault() * ARDocType.SignAmount(line.TranType);
					invoiced.InvoicedAmount += line.TranAmt.GetValueOrDefault() * ARDocType.SignAmount(line.TranType);
				}
				else
				{
					invoiced.CuryInvoicedAmount += line.TranAmt.GetValueOrDefault() * ARDocType.SignAmount(line.TranType);
					invoiced.InvoicedAmount += line.TranAmt.GetValueOrDefault() * ARDocType.SignAmount(line.TranType);
				}
			}
		}

		public virtual void RecalculateChangeOrders(PMProject project, ProjectBalance projectBalance)
		{
			var select = new PXSelectJoin<PMChangeOrderBudget,
				InnerJoin<PMChangeOrder, On<PMChangeOrder.refNbr, Equal<PMChangeOrderBudget.refNbr>>>,
				Where<PMChangeOrderBudget.projectID, Equal<Required<PMChangeOrderBudget.projectID>>>>(this);

			foreach (PXResult<PMChangeOrderBudget, PMChangeOrder> res in select.Select(project.ContractID))
			{
				PMChangeOrderBudget change = (PMChangeOrderBudget)res;
				PMChangeOrder order = (PMChangeOrder)res;

                UpdateChangeBuckets(change, projectBalance, project, order.Date);
            }
		}

        public virtual void RecalculateChangeRequests(PMProject project, ProjectBalance projectBalance)
        {
            var select = new PXSelectJoin<PMChangeRequestLine,
                InnerJoin<PMChangeRequest, On<PMChangeRequest.refNbr, Equal<PMChangeRequestLine.refNbr>>>,
                Where<PMChangeRequestLine.projectID, Equal<Required<PMChangeRequestLine.projectID>>,
                And<PMChangeRequest.released, Equal<False>,
				And<PMChangeRequest.approved, Equal<True>>>>>(this);

            foreach (PXResult<PMChangeRequestLine, PMChangeRequest> res in select.Select(project.ContractID))
            {
                PMChangeRequestLine change = (PMChangeRequestLine)res;
                PMChangeRequest request = (PMChangeRequest)res;

                PMChangeOrderBudget cost = new PMChangeOrderBudget();
                cost.ProjectID = change.ProjectID;
                cost.ProjectTaskID = change.CostTaskID;
                cost.AccountGroupID = change.CostAccountGroupID;
                cost.InventoryID = change.InventoryID;
                cost.CostCodeID = change.CostCodeID;
                cost.Qty = change.Qty;
                cost.Amount = change.ExtCost;
				cost.UOM = change.UOM;

                if (cost.TaskID != null && cost.AccountGroupID != null)
                {
                    UpdateChangeBuckets(cost, projectBalance, project, request.Date);
                }

                PMChangeOrderBudget revenue = new PMChangeOrderBudget();
                revenue.ProjectID = change.ProjectID;
                revenue.ProjectTaskID = change.RevenueTaskID;
                revenue.AccountGroupID = change.RevenueAccountGroupID;
                revenue.InventoryID = change.InventoryID;
                revenue.CostCodeID = change.RevenueCodeID;
                revenue.Qty = change.Qty;
                revenue.Amount = change.LineAmount;
				revenue.UOM = change.UOM;

				if (revenue.TaskID != null && revenue.AccountGroupID != null)
                {
                    UpdateChangeBuckets(revenue, projectBalance, project, request.Date);
                }
            }

            var selectMarkups = new PXSelectJoin<PMChangeRequestMarkup,
                InnerJoin<PMChangeRequest, On<PMChangeRequest.refNbr, Equal<PMChangeRequestMarkup.refNbr>>>,
                Where<PMChangeRequest.projectID, Equal<Required<PMChangeRequest.projectID>>,
				And<PMChangeRequest.released, Equal<False>,
				And<PMChangeRequest.approved, Equal<True>>>>>(this);

			foreach (PXResult<PMChangeRequestMarkup, PMChangeRequest> res in selectMarkups.Select(project.ContractID))
            {
                PMChangeRequestMarkup markup = (PMChangeRequestMarkup)res;
                PMChangeRequest request = (PMChangeRequest)res;

                PMChangeOrderBudget revenue = new PMChangeOrderBudget();
                revenue.ProjectID = request.ProjectID;
                revenue.ProjectTaskID = markup.TaskID;
                revenue.AccountGroupID = markup.AccountGroupID;
                revenue.InventoryID = markup.InventoryID;
                revenue.CostCodeID = markup.CostCodeID;
                revenue.Amount = markup.MarkupAmount;
                                
                if (revenue.TaskID != null && revenue.AccountGroupID != null)
                {
                    UpdateChangeBuckets(revenue, projectBalance, project, request.Date);
                }
            }
        }

        protected virtual void UpdateChangeBuckets(PMChangeOrderBudget change, ProjectBalance projectBalance, PMProject project, DateTime? changeDate)
        {
            PMBudgetAccum budget = null;
				bool isExisting;
				PMBudgetLite existing = budgetService.SelectProjectBalance(change, AccountGroups[change.AccountGroupID.Value], project, out isExisting );
				if (isExisting)
            {
                budget = new PMBudgetAccum();
                budget.ProjectID = existing.ProjectID;
                budget.ProjectTaskID = existing.ProjectTaskID;
                budget.AccountGroupID = existing.AccountGroupID;
                budget.InventoryID = existing.InventoryID;
                budget.CostCodeID = existing.CostCodeID;
				budget.UOM = existing.UOM;

                budget = Budget.Insert(budget);

				if (change.Released == true)
				{
					budget.CuryChangeOrderAmount += change.Amount.GetValueOrDefault();
					budget.CuryRevisedAmount += change.Amount.GetValueOrDefault();
				}
				else
				{
					budget.CuryDraftChangeOrderAmount += change.Amount.GetValueOrDefault();
				}


				var rollupQty = projectBalance.CalculateRollupQty(change, existing);
                if (rollupQty != 0)
                {
                    if (change.Released == true)
                    {
                        budget.ChangeOrderQty += change.Qty.GetValueOrDefault();
                        budget.RevisedQty += change.Qty.GetValueOrDefault();
                    }
                    else
                    {
                        budget.DraftChangeOrderQty += change.Qty.GetValueOrDefault();
                    }
                }
            }
            else
            {
                PMAccountGroup accountGroup = null;
                if (AccountGroups.TryGetValue(change.AccountGroupID.Value, out accountGroup))
                {
                    budget = new PMBudgetAccum();
						budget.ProjectID = existing.ProjectID;
						budget.ProjectTaskID = existing.ProjectTaskID;
						budget.AccountGroupID = existing.AccountGroupID;
						budget.InventoryID = existing.InventoryID;
						budget.CostCodeID = existing.CostCodeID;
						budget.Type = existing.Type;
						budget.Description = existing.Description;
						budget.IsProduction = existing.IsProduction;

                    budget = Budget.Insert(budget);
                    if (change.Released == true)
                    {
                        budget.CuryChangeOrderAmount += change.Amount.GetValueOrDefault();
                        budget.CuryRevisedAmount += change.Amount.GetValueOrDefault();
                    }
                    else
                    {
                        budget.CuryDraftChangeOrderAmount += change.Amount.GetValueOrDefault();
                    }
                }
            }

            if (budget != null)
            {
                FinPeriod finPeriod = FinPeriodRepository.GetFinPeriodByDate(changeDate, FinPeriod.organizationID.MasterValue);

                if (finPeriod != null)
                {
                    PMForecastHistoryAccum forecast = new PMForecastHistoryAccum();
                    forecast.ProjectID = budget.ProjectID;
                    forecast.ProjectTaskID = budget.ProjectTaskID;
                    forecast.AccountGroupID = budget.AccountGroupID;
                    forecast.InventoryID = budget.InventoryID;
                    forecast.CostCodeID = budget.CostCodeID;
                    forecast.PeriodID = finPeriod.FinPeriodID;

                    forecast = ForecastHistory.Insert(forecast);
                    if (change.Released == true)
                    {
                        forecast.CuryChangeOrderAmount += change.Amount.GetValueOrDefault();
                        forecast.ChangeOrderQty += change.Qty.GetValueOrDefault();
                    }
                    else
                    {
                        forecast.CuryDraftChangeOrderAmount += change.Amount.GetValueOrDefault();
                        forecast.DraftChangeOrderQty += change.Qty.GetValueOrDefault();
                    }
                }
                else
                {
                    PXTrace.WriteError("Failed to find FinPeriodID for date {0}", changeDate);
                }
            }
        }

        public virtual void InitAccountGroup()
		{
			if (AccountGroups == null)
			{
				AccountGroups = new Dictionary<int, PMAccountGroup>();
				foreach (PMAccountGroup ag in PXSelect<PMAccountGroup>.Select(this))
				{
					AccountGroups.Add(ag.GroupID.Value, ag);
				}
			}
		}

		public virtual void InitAccounts()
		{
			if (Accounts == null)
			{
				Accounts = new Dictionary<int, Account>();
				foreach (Account account in PXSelect<Account, Where<Account.accountGroupID, IsNotNull>>.Select(this))
				{
					Accounts.Add(account.AccountID.Value, account);
				}
			}
		}

		public virtual void InitCostCodeOnModifiedEntities()
		{
			if (CostCodeAttribute.UseCostCode())
			{
				int? defaultCostCodeID = CostCodeAttribute.DefaultCostCode;
				foreach (POLine line in polines.Cache.Updated)
				{
					if (line.CostCodeID == null)
					{
						polines.Cache.SetValue<POLine.costCodeID>(line, defaultCostCodeID);
					}
				}

				foreach (SOLine line in solines.Cache.Updated)
				{
					if (line.CostCodeID == null)
					{
						solines.Cache.SetValue<POLine.costCodeID>(line, defaultCostCodeID);
					}
				}
			}
		}

		public virtual string GetAccountGroupType(int? accountGroup)
		{
			PMAccountGroup ag = AccountGroups[accountGroup.Value];

			if (ag.Type == PMAccountType.OffBalance)
				return ag.IsExpense == true ? GL.AccountType.Expense : ag.Type;
			else
				return ag.Type;
		}

		public virtual ProjectBalance CreateProjectBalance()
		{
			return new ProjectBalance(this, budgetService, ServiceLocator.Current.GetInstance<IProjectSettingsManager>());
		}

		public List<PXDataFieldParam> BuildBudgetClearCommand(PMValidationFilter options, PMProject project, int? accountGroupID)
		{
			List<PXDataFieldParam> list = new List<PXDataFieldParam>();
			list.Add(new PXDataFieldRestrict<PMBudget.projectID>(PXDbType.Int, 4, project.ContractID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.accountGroupID>(PXDbType.Int, 4, accountGroupID, PXComp.EQ));

			list.Add(new PXDataFieldAssign<PMBudget.type>(PXDbType.Char, 1, GetAccountGroupType(accountGroupID)));
			list.Add(new PXDataFieldAssign<PMBudget.curyActualAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.actualAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.actualQty>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyDraftRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.draftRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.retainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyTotalRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.totalRetainedAmount>(PXDbType.Decimal, 0m));

			if (options.RebuildCommitments == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedOpenAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOpenAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOpenQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedReceivedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedInvoicedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOrigQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedOrigAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOrigAmount>(PXDbType.Decimal, 0m));
			}

			if (options.RecalculateDraftInvoicesAmount == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.invoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyPrepaymentInvoiced>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.prepaymentInvoiced>(PXDbType.Decimal, 0m));
			}

			return list;
		}

		public List<PXDataFieldParam> BuildBudgetClearCommandWithChangeOrders(PMValidationFilter options, PMBudgetLiteEx status)
		{
			List<PXDataFieldParam> list = new List<PXDataFieldParam>();
			list.Add(new PXDataFieldRestrict<PMBudget.projectID>(PXDbType.Int, 4, status.ProjectID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.accountGroupID>(PXDbType.Int, 4, status.AccountGroupID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.projectTaskID>(PXDbType.Int, 4, status.ProjectTaskID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.inventoryID>(PXDbType.Int, 4, status.InventoryID, PXComp.EQ));
			list.Add(new PXDataFieldRestrict<PMBudget.costCodeID>(PXDbType.Int, 4, status.CostCodeID, PXComp.EQ));

			list.Add(new PXDataFieldAssign<PMBudget.type>(PXDbType.Char, 1, GetAccountGroupType(status.AccountGroupID)));
			list.Add(new PXDataFieldAssign<PMBudget.curyActualAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.actualAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.actualQty>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.revisedQty>(PXDbType.Decimal, status.Qty));
			list.Add(new PXDataFieldAssign<PMBudget.curyRevisedAmount>(PXDbType.Decimal, status.CuryAmount));
			list.Add(new PXDataFieldAssign<PMBudget.revisedAmount>(PXDbType.Decimal, status.Amount));
			list.Add(new PXDataFieldAssign<PMBudget.draftChangeOrderQty>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyDraftChangeOrderAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.draftChangeOrderAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.changeOrderQty>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyChangeOrderAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.changeOrderAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyDraftRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.draftRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.retainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.curyTotalRetainedAmount>(PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign<PMBudget.totalRetainedAmount>(PXDbType.Decimal, 0m));

			if (options.RebuildCommitments == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedOpenAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOpenAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOpenQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedReceivedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedInvoicedQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOrigQty>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyCommittedOrigAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.committedOrigAmount>(PXDbType.Decimal, 0m));
			}

			if (options.RecalculateDraftInvoicesAmount == true)
			{
				list.Add(new PXDataFieldAssign<PMBudget.curyInvoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.invoicedAmount>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.curyPrepaymentInvoiced>(PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign<PMBudget.prepaymentInvoiced>(PXDbType.Decimal, 0m));
			}

			return list;

		}

		public List<PXDataFieldParam> BuildForecastClearCommand(PMValidationFilter options, int? projectID)
		{
			List<PXDataFieldParam> list = new List<PXDataFieldParam>();
			AddRestrictorsForcast(options, projectID, list);
			AddFieldAssignsForecast(options, list);

			return list;
		}

		public virtual void AddRestrictorsForcast(PMValidationFilter options, int? projectID, List<PXDataFieldParam> list)
		{
			list.Add(new PXDataFieldRestrict(typeof(PMForecastHistory.projectID).Name, PXDbType.Int, 4, projectID, PXComp.EQ));
			//list.Add(new PXDataFieldRestrict(typeof(PMForecastHistory.accountGroupID).Name, PXDbType.Int, 4, status.AccountGroupID, PXComp.EQ));
		}

		public virtual void AddFieldAssignsForecast(PMValidationFilter options, List<PXDataFieldParam> list)
		{
			list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.curyActualAmount).Name, PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.actualAmount).Name, PXDbType.Decimal, 0m));
			list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.actualQty).Name, PXDbType.Decimal, 0m));

			if (options.RecalculateChangeOrders == true)
			{
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.draftChangeOrderQty).Name, PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.curyDraftChangeOrderAmount).Name, PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.changeOrderQty).Name, PXDbType.Decimal, 0m));
				list.Add(new PXDataFieldAssign(typeof(PMForecastHistory.curyChangeOrderAmount).Name, PXDbType.Decimal, 0m));
			}
		}

		protected virtual void ProcessRetainage(PMProject project, PMValidationFilter options)
		{
			//Retained Amount:
			var selectReleasedNotByLines = new PXSelectJoinGroupBy<ARTran,
				InnerJoin<ARInvoice, On<ARInvoice.docType, Equal<ARTran.tranType>, And<ARInvoice.refNbr, Equal<ARTran.refNbr>>>,
				InnerJoin<Account, On<ARTran.accountID, Equal<Account.accountID>, And<Account.accountGroupID, IsNotNull>>>>,
				Where<ARTran.projectID, Equal<Required<ARTran.projectID>>,
				And<ARTran.released, Equal<True>,
				And<ARInvoice.paymentsByLinesAllowed, Equal<False>>>>,
				Aggregate<GroupBy<ARTran.accountID,
				GroupBy<ARTran.projectID,
				GroupBy<ARTran.taskID,
				GroupBy<ARTran.inventoryID,
				GroupBy<ARTran.costCodeID,
				Sum<ARTran.retainageAmt,
				Sum<ARTran.curyRetainageAmt,
				Sum<ARTran.retainageBal,
				Sum<ARTran.curyRetainageBal>>>>>>>>>>>(this);

			foreach (PXResult<ARTran, ARInvoice, Account> res in selectReleasedNotByLines.Select(project.ContractID))
			{
				ARTran tran = (ARTran)res;
				Account account = (Account)res;

				decimal retainedAmount = tran.RetainageAmt.GetValueOrDefault() * ARDocType.SignAmount(tran.TranType).GetValueOrDefault(1);

				PMBudgetAccum retained = GetTargetBudget(project, account.AccountGroupID, tran);
				if (retained != null)
				{
					retained = Budget.Insert(retained);
					retained.RetainedAmount += retainedAmount;

					if (ConversionRequired(this, project))
					{
						decimal val;
						PXCurrencyAttribute.CuryConvCury<PMProject.curyInfoID>(Caches[typeof(PMProject)], project, retainedAmount, out val);
						retained.CuryRetainedAmount += val;
					}
					else
					{
						retained.CuryRetainedAmount += retainedAmount;
					}
				}
			}


			var selectReleasedByLines = new PXSelectJoinGroupBy<ARTran,
				InnerJoin<ARInvoice, On<ARInvoice.docType, Equal<ARTran.tranType>, And<ARInvoice.refNbr, Equal<ARTran.refNbr>>>,
				InnerJoin<Account, On<ARTran.accountID, Equal<Account.accountID>, And<Account.accountGroupID, IsNotNull>>>>,
				Where<ARTran.projectID, Equal<Required<ARTran.projectID>>,
				And<ARTran.released, Equal<True>,
				And<ARInvoice.paymentsByLinesAllowed, Equal<True>>>>,
				Aggregate<GroupBy<ARTran.accountID,
				GroupBy<ARTran.projectID,
				GroupBy<ARTran.taskID,
				GroupBy<ARTran.inventoryID,
				GroupBy<ARTran.costCodeID,
				Sum<ARTran.retainageAmt,
				Sum<ARTran.curyRetainageAmt,
				Sum<ARTran.retainageBal,
				Sum<ARTran.curyRetainageBal>>>>>>>>>>>(this);

			foreach (PXResult<ARTran, ARInvoice, Account> res in selectReleasedByLines.Select(project.ContractID))
			{
				ARTran tran = (ARTran)res;
				Account account = (Account)res;

				decimal retainedAmount = tran.RetainageBal.GetValueOrDefault() * ARDocType.SignAmount(tran.TranType).GetValueOrDefault(1);

				PMBudgetAccum retained = GetTargetBudget(project, account.AccountGroupID, tran);
				if (retained != null)
				{
					retained = Budget.Insert(retained);
					retained.RetainedAmount += retainedAmount;

					if (ConversionRequired(this, project))
					{
						decimal val;
						PXCurrencyAttribute.CuryConvCury<PMProject.curyInfoID>(Caches[typeof(PMProject)], project, retainedAmount, out val);
						retained.CuryRetainedAmount += val;
					}
					else
					{
						retained.CuryRetainedAmount += retainedAmount;
					}
				}
			}

			//Draft Retained Amount:
			var selectUnreleased = new PXSelectJoinGroupBy<ARTran,
				InnerJoin<Account, On<ARTran.accountID, Equal<Account.accountID>, And<Account.accountGroupID, IsNotNull>>>,
				Where<ARTran.projectID, Equal<Required<ARTran.projectID>>, And<ARTran.released, Equal<False>>>,
				Aggregate<GroupBy<ARTran.accountID,
				GroupBy<ARTran.projectID,
				GroupBy<ARTran.taskID,
				GroupBy<ARTran.inventoryID,
				GroupBy<ARTran.costCodeID,
				Sum<ARTran.retainageAmt,
				Sum<ARTran.curyRetainageAmt,
				Sum<ARTran.retainageBal,
				Sum<ARTran.curyRetainageBal>>>>>>>>>>>(this);

			foreach (PXResult<ARTran, Account> res in selectUnreleased.Select(project.ContractID))
			{
				ARTran tran = (ARTran)res;
				Account account = (Account)res;

				decimal retainedAmount = tran.RetainageAmt.GetValueOrDefault() * ARDocType.SignAmount(tran.TranType).GetValueOrDefault(1);
				PMBudgetAccum retained = GetTargetBudget(project, account.AccountGroupID, tran);
				if (retained != null)
				{
					retained = Budget.Insert(retained);
					retained.DraftRetainedAmount += retainedAmount;

					if (ConversionRequired(this, project))
					{
						decimal val;
						PXCurrencyAttribute.CuryConvCury<PMProject.curyInfoID>(Caches[typeof(PMProject)], project, retainedAmount, out val);
						retained.CuryDraftRetainedAmount += val;
					}
					else
					{
						retained.CuryDraftRetainedAmount += retainedAmount;
					}
				}
			}


			var selectProformaProgressive = new PXSelectGroupBy<PMProformaLine,
			Where<PMProformaLine.projectID, Equal<Required<PMProformaLine.projectID>>,
				And<PMProformaLine.type, Equal<PMProformaLineType.progressive>>>,
			Aggregate<GroupBy<PMProformaLine.accountGroupID,
			GroupBy<PMProformaLine.projectID,
			GroupBy<PMProformaLine.taskID,
			GroupBy<PMProformaLine.inventoryID,
			GroupBy<PMProformaLine.costCodeID,
			GroupBy<PMProformaLine.released,
			Sum<PMProformaLine.retainage,
			Sum<PMProformaLine.curyRetainage>>>>>>>>>>(this);

			foreach (PMProformaLine tran in selectProformaProgressive.Select(project.ContractID))
			{
				decimal retainedAmount = tran.Retainage.GetValueOrDefault();

				PMBudgetAccum retained = GetTargetBudget(project, tran.AccountGroupID, tran);
				if (retained != null)
				{
					retained = Budget.Insert(retained);

					if (tran.Released != true)
						retained.DraftRetainedAmount += retainedAmount;
					retained.TotalRetainedAmount += retainedAmount;

					if (ConversionRequired(this, project))
					{
						decimal val;
						PXCurrencyAttribute.CuryConvCury<PMProject.curyInfoID>(Caches[typeof(PMProject)], project, retainedAmount, out val);
						if (tran.Released != true)
							retained.CuryDraftRetainedAmount += val;
						retained.CuryTotalRetainedAmount += val;
					}
					else
					{
						if (tran.Released != true)
							retained.CuryDraftRetainedAmount += retainedAmount;
						retained.CuryTotalRetainedAmount += retainedAmount;
					}
				}
			}

			var selectProformaTransaction = new PXSelectJoinGroupBy<PMProformaLine,
			InnerJoin<Account, On<PMProformaLine.accountID, Equal<Account.accountID>, And<Account.accountGroupID, IsNotNull>>>,
			Where<PMProformaLine.projectID, Equal<Required<PMProformaLine.projectID>>,
				And<PMProformaLine.type, Equal<PMProformaLineType.transaction>>>,
			Aggregate<GroupBy<PMProformaLine.accountID,
			GroupBy<PMProformaLine.projectID,
			GroupBy<PMProformaLine.taskID,
			GroupBy<PMProformaLine.inventoryID,
			GroupBy<PMProformaLine.costCodeID,
			GroupBy<PMProformaLine.released,
			Sum<PMProformaLine.retainage,
			Sum<PMProformaLine.curyRetainage>>>>>>>>>>(this);

			foreach (PXResult<PMProformaLine, Account> res in selectProformaTransaction.Select(project.ContractID))
			{
				PMProformaLine tran = (PMProformaLine)res;
				Account account = (Account)res;

				decimal retainedAmount = tran.Retainage.GetValueOrDefault();

				PMBudgetAccum retained = GetTargetBudget(project, account.AccountGroupID, tran);
				if (retained != null)
				{
					Debug.Print("Draft Proforma T- {0}", retainedAmount);

					retained = Budget.Insert(retained);
					if (tran.Released != true)
						retained.DraftRetainedAmount += retainedAmount;
					retained.TotalRetainedAmount += retainedAmount;

					if (ConversionRequired(this, project))
					{
						decimal val;
						PXCurrencyAttribute.CuryConvCury<PMProject.curyInfoID>(Caches[typeof(PMProject)], project, retainedAmount, out val);
						if (tran.Released != true)
							retained.CuryDraftRetainedAmount += val;
						retained.CuryTotalRetainedAmount += val;
					}
					else
					{
						if (tran.Released != true)
							retained.CuryDraftRetainedAmount += retainedAmount;
						retained.CuryTotalRetainedAmount += retainedAmount;
					}
				}
			}
		}

		private bool ConversionRequired(PXGraph graph, PMProject project)
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectMultiCurrency>())
				return false;

			Company company = PXSelect<Company>.Select(graph);
			if (project != null && company != null && project.CuryID == company.BaseCuryID)
			{
				return false;
			}

			return true;
		}

		private PMBudgetAccum GetTargetBudget(PMProject project, int? accountGroupID, ARTran line)
		{
			PMAccountGroup ag;
			if (AccountGroups.TryGetValue(accountGroupID.Value, out ag))
			{
				bool isExisting;
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

			return null;
		}

		private PMBudgetAccum GetTargetBudget(PMProject project, int? accountGroupID, PMProformaLine line)
		{
			PMAccountGroup ag;
			if (AccountGroups.TryGetValue(accountGroupID.Value, out ag))
			{
				bool isExisting;
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

			return null;
		}

		[PXHidden]
		[Serializable]
		[PXBreakInheritance]
		[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
		public partial class PMBudgetLiteEx : PMBudgetLite
		{
			#region CuryAmount
			public abstract class curyAmount : PX.Data.BQL.BqlDecimal.Field<curyAmount>
			{
			}
			[PXDBDecimal]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Original Budgeted Amount")]
			public virtual Decimal? CuryAmount
			{
				get;
				set;
			}
			#endregion
			#region Amount
			public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount>
			{
			}
			[PXDBDecimal]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Original Budgeted Amount in Base Currency")]
			public virtual Decimal? Amount
			{
				get;
				set;
			}
			#endregion
		}

		public class BudgetServiceMassUpdate : BudgetService
		{
			private int? projectID;
			private Dictionary<BudgetKeyTuple, PMBudgetLiteEx> Budget;
			private HashSet<int> accountGroups;
			
			public BudgetServiceMassUpdate(PXGraph graph, PMProject project):base(graph)
			{
				this.projectID = project.ContractID;
			}
						
			protected override List<PMBudgetLite> SelectExistingBalances(int projectID, int taskID, int accountGroupID, int?[] costCodes, int?[] items)
			{
				if (Budget == null)
				{
					PreSelectProjectBudget();
				}

				List<PMBudgetLite> list = new List<PMBudgetLite>();
				foreach(int costCodeID in costCodes)
				{
					foreach(int inventoryID in items)
					{
						PMBudgetLiteEx result;
						if (Budget.TryGetValue(new BudgetKeyTuple(projectID, taskID, accountGroupID, inventoryID, costCodeID), out result))
						{
							list.Add(result);
						}
					}
				}

				return list;
			}

			private void PreSelectProjectBudget()
			{
				Budget = new Dictionary<BudgetKeyTuple, PMBudgetLiteEx>();
				accountGroups = new HashSet<int>();

				PXSelectBase<PMBudgetLiteEx> selectBudget = new PXSelect<PMBudgetLiteEx,
					Where<PMBudgetLiteEx.projectID, Equal<Required<PMBudgetLiteEx.projectID>>>>(graph);

				foreach(PMBudgetLiteEx budget in selectBudget.Select(projectID))
				{
					Budget.Add(BudgetKeyTuple.Create(budget), budget);
					accountGroups.Add(budget.AccountGroupID.Value);
				}
			}

			public ICollection<int> GetUsedAccountGroups()
			{
				if (Budget == null)
				{
					PreSelectProjectBudget();
				}

				return accountGroups;
			}

			public Dictionary<BudgetKeyTuple, PMBudgetLiteEx>.ValueCollection BudgetRecords
			{
				get
				{
					if (Budget == null)
					{
						PreSelectProjectBudget();
					}

					return Budget.Values;
				}
			}

			
		}
	}

	[PXHidden]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMValidationFilter : IBqlTable
	{
		#region RecalculateUnbilledSummary
		public abstract class recalculateUnbilledSummary : PX.Data.BQL.BqlBool.Field<recalculateUnbilledSummary> { }
		protected Boolean? _RecalculateUnbilledSummary;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Recalculate Unbilled Summary")]
		public virtual Boolean? RecalculateUnbilledSummary
		{
			get
			{
				return this._RecalculateUnbilledSummary;
			}
			set
			{
				this._RecalculateUnbilledSummary = value;
			}
		}
		#endregion
		#region RecalculateDraftInvoicesAmount
		public abstract class recalculateDraftInvoicesAmount : PX.Data.BQL.BqlBool.Field<recalculateDraftInvoicesAmount> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Recalculate Draft Invoices Amount")]
		public virtual Boolean? RecalculateDraftInvoicesAmount
		{
			get;
			set;
		}
		#endregion
		#region RebuildCommitments
		public abstract class rebuildCommitments : PX.Data.BQL.BqlBool.Field<rebuildCommitments> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Rebuild Commitments")]
		public virtual Boolean? RebuildCommitments
		{
			get;
			set;
		}
		#endregion
		#region RecalculateChangeOrders
		public abstract class recalculateChangeOrders : PX.Data.BQL.BqlBool.Field<recalculateChangeOrders> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Recalculate Change Orders", FieldClass = PMChangeOrder.FieldClass)]
		public virtual Boolean? RecalculateChangeOrders
		{
			get;
			set;
		}
		#endregion
	}


}