using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;

namespace PX.Objects.PM
{
	public class ProjectBalance
	{
		protected PXGraph graph;
		protected ProjectSettingsManager settings;
		protected IBudgetService service;

		public ProjectSettingsManager Settings
		{
			get { return settings; }
		}

		public ProjectBalance(PXGraph graph) : this(graph, new BudgetService(graph)) { }

		public ProjectBalance(PXGraph graph, IBudgetService service)
		{
			if (graph == null)
				throw new ArgumentNullException(nameof(graph));

			if (service == null)
				throw new ArgumentNullException(nameof(service));

			this.graph = graph;
			this.service = service;
			this.settings = new ProjectSettingsManager();
		}

		public virtual List<Result> Calculate(PMProject project, PMTran tran, Account acc, PMAccountGroup ag, Account offsetAcc, PMAccountGroup offsetAg)
		{
			List<Result> list = new List<Result>();
			if (tran.TaskID != null)
			{
				int invert = 1;
				if (tran.TranType == BatchModule.PM && IsFlipRequired(acc.Type, ag.Type))
				{
					//Invert transactions that originated in PM. All other transactions were already inverted when they were transformed from GLTran to PMTran.
					invert = -1;
				}

				bool debitcreditcancelout = false;
				if (offsetAcc != null && acc.AccountID == offsetAcc.AccountID)
				{
					debitcreditcancelout = true;
				}
				if (offsetAg != null && ag.GroupID == offsetAg.GroupID)
				{
					debitcreditcancelout = true;
				}

				if (offsetAcc != null && string.IsNullOrEmpty(acc.Type) && offsetAcc.AccountGroupID == tran.AccountGroupID)
				{
					return list;
				}

				if (string.IsNullOrEmpty(acc.AccountCD))
				{
					//non-gl tran.
					//DEBIT ONLY
					if (ag.Type == AccountType.Income || ag.Type == AccountType.Liability)
						list.Add(Calculate(project, tran, ag, null, -1 * invert, 1));
					else
						list.Add(Calculate(project, tran, ag, null, 1 * invert, 1));
				}
				else
				{
					if (!debitcreditcancelout)
					{
						//DEBIT
						if (acc.Type == AccountType.Income || acc.Type == AccountType.Liability)
							list.Add(Calculate(project, tran, ag, acc.Type, -1 * invert, 1));
						else
							list.Add(Calculate(project, tran, ag, acc.Type, 1 * invert, 1));
					}
				}

				//CREDIT				
				if (offsetAcc != null && offsetAg != null && offsetAcc.AccountID != null && offsetAg.GroupID != null && !debitcreditcancelout)
				{
					int offsetInvert = 1;
					if (IsFlipRequired(offsetAcc.Type, offsetAg.Type))
					{
						offsetInvert = -1;
					}

					if (offsetAcc.Type == AccountType.Income || offsetAcc.Type == AccountType.Liability)
						list.Add(Calculate(project, tran, offsetAg, offsetAcc.Type, 1 * offsetInvert, 1 * offsetInvert));
					else
						list.Add(Calculate(project, tran, offsetAg, offsetAcc.Type, -1 * offsetInvert, -1 * offsetInvert));
				}
			}

			return list;
		}

		public virtual Result Calculate(PMProject project, PMTran pmt, PMAccountGroup ag, string accountType, int amountSign, int qtySign)
		{
			PMBudgetLite existing = null;

			if (ag.Type == GL.AccountType.Income && project.BudgetLevel != BudgetLevels.CostCode)
			{
				existing = service.SelectProjectBalanceByInventory(pmt);
			}
			else
			{
				existing = service.SelectProjectBalance(pmt);
			}

			int? inventoryID = existing != null ? existing.InventoryID : PMInventorySelectorAttribute.EmptyInventoryID;
			int? costCodeID = existing != null ? existing.CostCodeID : CostCodeAttribute.GetDefaultCostCode();
			RollupQty rollup = null;

			if (CostCodeAttribute.UseCostCode())
			{
				if (settings.CostBudgetUpdateMode == CostBudgetUpdateModes.Detailed && ag.IsExpense == true)
				{
					if (existing == null || existing.CostCodeID == CostCodeAttribute.GetDefaultCostCode())
					{
						rollup = new RollupQty(pmt.UOM, pmt.Qty);
						if (pmt.CostCodeID != null)
						{
							costCodeID = pmt.CostCodeID;
						}
					}
				}
				else if (ag.Type == GL.AccountType.Income && project.BudgetLevel != BudgetLevels.CostCode)
				{
					costCodeID = CostCodeAttribute.GetDefaultCostCode();
				}
			}
			else
			{
				if (settings.CostBudgetUpdateMode == CostBudgetUpdateModes.Detailed && ag.IsExpense == true && pmt.InventoryID != settings.EmptyInventoryID)
				{
					if (existing == null || existing.InventoryID == settings.EmptyInventoryID)
					{
						rollup = new RollupQty(pmt.UOM, pmt.Qty);
						if (pmt.InventoryID != null)
						{
							inventoryID = pmt.InventoryID;
						}

						if (pmt.CostCodeID != null)
						{
							costCodeID = pmt.CostCodeID;
						}
					}
				}
			}



			if (rollup == null)
				rollup = CalculateRollupQty(pmt, existing);

			List<PMHistory> list = new List<PMHistory>();

			PMTaskTotal ta = null;
			PMBudget ps = null;
			PMForecastHistory forecast = null;

			if (pmt.TaskID != null && (rollup.Qty != 0 || pmt.Amount != 0)) //TaskID will be null for Contract
			{
				ps = new PMBudget();
				ps.ProjectID = pmt.ProjectID;
				ps.ProjectTaskID = pmt.TaskID;
				ps.AccountGroupID = ag.GroupID;
				if (ag.Type == PMAccountType.OffBalance)
					ps.Type = ag.IsExpense == true ? GL.AccountType.Expense : ag.Type;
				else
					ps.Type = ag.Type;
				ps.InventoryID = inventoryID;
				ps.CostCodeID = costCodeID;
				ps.UOM = rollup.UOM;
				if (existing != null)
				{
					ps.IsProduction = existing.IsProduction;
					ps.Description = existing.Description;
				}
				else
				{
					//initialize description:
					if (CostCodeAttribute.UseCostCode())
					{
						PMCostCode costCode = PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Required<PMCostCode.costCodeID>>>>.Select(graph, ps.CostCodeID);
						if (costCode != null)
						{
							ps.Description = costCode.Description;
						}
					}
					else if (inventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
					{
						InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(graph, inventoryID);
						if (item != null)
						{
							ps.Description = item.Descr;
						}
					}
				}

				if (ps.CuryInfoID == null)
				{
					//initialize CuryInfoID:
					ps.CuryInfoID = project.CuryInfoID;
				}

				decimal amt = amountSign * pmt.Amount.GetValueOrDefault();
				decimal curyAmt = amountSign * pmt.ProjectCuryAmount.GetValueOrDefault();

				ps.ActualQty = rollup.Qty * qtySign;
				ps.ActualAmount = amt;
				ps.CuryActualAmount = curyAmt;

				#region PMTask Totals Update

				ta = new PMTaskTotal();
				ta.ProjectID = pmt.ProjectID;
				ta.TaskID = pmt.TaskID;

				string accType = ag.IsExpense == true ? AccountType.Expense : ag.Type;

				switch (accType)
				{
					case AccountType.Asset:
						ta.CuryAsset = curyAmt;
						ta.Asset = amt;
						break;
					case AccountType.Liability:
						ta.CuryLiability = curyAmt;
						ta.Liability = amt;
						break;
					case AccountType.Income:
						ta.CuryIncome = curyAmt;
						ta.Income = amt;
						break;
					case AccountType.Expense:
						ta.CuryExpense = curyAmt;
						ta.Expense = amt;
						break;
				}

				#endregion

				#region History
				PMHistory hist = new PMHistory();
				hist.ProjectID = pmt.ProjectID;
				hist.ProjectTaskID = pmt.TaskID;
				hist.AccountGroupID = ag.GroupID;
				hist.InventoryID = pmt.InventoryID ?? PMInventorySelectorAttribute.EmptyInventoryID;
				hist.CostCodeID = pmt.CostCodeID ?? CostCodeAttribute.GetDefaultCostCode();
				hist.PeriodID = pmt.FinPeriodID;
				hist.BranchID = pmt.BranchID;
				decimal baseQty = 0;
				list.Add(hist);
				if (pmt.InventoryID != null && pmt.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID && pmt.Qty != 0)
				{
					if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
					{
						baseQty = qtySign * IN.INUnitAttribute.ConvertToBase(graph.Caches[typeof(PMHistory)], pmt.InventoryID, pmt.UOM, pmt.Qty.Value, PX.Objects.IN.INPrecision.QUANTITY);
					}
					else
					{
						IN.InventoryItem initem = PXSelectorAttribute.Select<PMTran.inventoryID>(graph.Caches[typeof(PMTran)], pmt) as IN.InventoryItem;
						if (initem != null && !string.IsNullOrEmpty(pmt.UOM))
							baseQty = qtySign * IN.INUnitAttribute.ConvertGlobalUnits(graph, pmt.UOM, initem.BaseUnit, pmt.Qty ?? 0, IN.INPrecision.QUANTITY);
					}
				}
				hist.FinPTDCuryAmount = curyAmt;
				hist.FinPTDAmount = amt;
				hist.FinYTDCuryAmount = curyAmt;
				hist.FinYTDAmount = amt;
				hist.FinPTDQty = baseQty;
				hist.FinYTDQty = baseQty;
				if (pmt.FinPeriodID == pmt.TranPeriodID)
				{
					hist.TranPTDCuryAmount = curyAmt;
					hist.TranPTDAmount = amt;
					hist.TranYTDCuryAmount = curyAmt;
					hist.TranYTDAmount = amt;
					hist.TranPTDQty = baseQty;
					hist.TranYTDQty = baseQty;
				}
				else
				{
					PMHistory tranHist = new PMHistory();
					tranHist.ProjectID = pmt.ProjectID;
					tranHist.ProjectTaskID = pmt.TaskID;
					tranHist.AccountGroupID = ag.GroupID;
					tranHist.InventoryID = pmt.InventoryID ?? PM.PMInventorySelectorAttribute.EmptyInventoryID;
					tranHist.CostCodeID = pmt.CostCodeID ?? CostCodeAttribute.GetDefaultCostCode();
					tranHist.PeriodID = pmt.TranPeriodID;
					tranHist.BranchID = pmt.BranchID;
					list.Add(tranHist);
					tranHist.TranPTDCuryAmount = curyAmt;
					tranHist.TranPTDAmount = amt;
					tranHist.TranYTDCuryAmount = curyAmt;
					tranHist.TranYTDAmount = amt;
					tranHist.TranPTDQty = baseQty;
					tranHist.TranYTDQty = baseQty;
				}
				#endregion

				forecast = new PMForecastHistory();
				forecast.ProjectID = ps.ProjectID;
				forecast.ProjectTaskID = ps.ProjectTaskID;
				forecast.AccountGroupID = ps.AccountGroupID;
				forecast.InventoryID = ps.InventoryID;
				forecast.CostCodeID = ps.CostCodeID;
				forecast.PeriodID = pmt.TranPeriodID;
				forecast.ActualQty = ps.ActualQty;
				forecast.CuryActualAmount = ps.CuryActualAmount;
				forecast.ActualAmount = ps.ActualAmount;
			}
			return new Result(list, ps, ta, forecast);
		}

		public virtual RollupQty CalculateRollupQty<T>(T row, IQuantify budget) where T : IBqlTable, IQuantify
		{
			return CalculateRollupQty(row, budget, row.Qty);
		}
		public virtual RollupQty CalculateRollupQty<T>(T row, IQuantify budget, decimal? quantity) where T : IBqlTable, IQuantify
		{
			string UOM = null;
			decimal rollupQty = 0;
			if (budget == null || string.IsNullOrEmpty(budget.UOM))
			{
				//Budget does not exist for given Inventory and <Other> is not present.
				//Or UOM not defined.
			}
			else
			{
				if (budget.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID)
				{
					//<Other> item is present. Update only if UOMs are convertable.
					decimal convertedQty;
					if (IN.INUnitAttribute.TryConvertGlobalUnits(graph, row.UOM, budget.UOM, quantity.GetValueOrDefault(), IN.INPrecision.QUANTITY, out convertedQty))
					{
						rollupQty = convertedQty;
						UOM = budget.UOM;
					}
				}
				else
				{
					UOM = budget.UOM;

					//Item matches. Convert to UOM of ProjectStatus.
					if (budget.UOM != row.UOM && !string.IsNullOrEmpty(budget.UOM) && !string.IsNullOrEmpty(row.UOM))
					{
						if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
						{
							decimal inBase = IN.INUnitAttribute.ConvertToBase(graph.Caches[typeof(T)], row.InventoryID, row.UOM, quantity ?? 0, IN.INPrecision.QUANTITY);
							try
							{
								rollupQty = IN.INUnitAttribute.ConvertFromBase(graph.Caches[typeof(T)], row.InventoryID, budget.UOM, inBase, IN.INPrecision.QUANTITY);
							}
							catch (PX.Objects.IN.PXUnitConversionException ex)
							{
								IN.InventoryItem item = PXSelectorAttribute.Select(graph.Caches[typeof(T)], row, "inventoryID") as IN.InventoryItem;
								string msg = PXMessages.LocalizeFormatNoPrefixNLA(Messages.UnitConversionNotDefinedForItemOnBudgetUpdate, item?.BaseUnit, budget.UOM, item?.InventoryCD);

								throw new PXException(msg, ex);

							}
						}
						else
						{
							rollupQty = IN.INUnitAttribute.ConvertGlobalUnits(graph, row.UOM, budget.UOM, quantity ?? 0, IN.INPrecision.QUANTITY);
						}
					}
					else if (!string.IsNullOrEmpty(budget.UOM))
					{
						rollupQty = quantity ?? 0;
					}
				}
			}

			return new RollupQty(UOM, rollupQty);
		}

		public class RollupQty
		{
			public RollupQty(string uom, decimal? qty)
			{
				this.UOM = uom;
				this.Qty = qty;
			}

			public string UOM { get; protected set; }
			public decimal? Qty { get; protected set; }
		}

		public class Result
		{
			public Result(IList<PMHistory> history, PMBudget status, PMTaskTotal taskTotal, PMForecastHistory forecast)
			{
				this.History = history;
				this.Status = status;
				this.TaskTotal = taskTotal;
				this.ForecastHistory = forecast;
			}

			public IList<PMHistory> History { get; protected set; }
			public PMBudget Status { get; protected set; }
			public PMTaskTotal TaskTotal { get; protected set; }
			public PMForecastHistory ForecastHistory { get; protected set; }
		}

		public static bool IsFlipRequired(string accountType, string accountGroupType)
		{
			if (string.IsNullOrEmpty(accountGroupType))
				return false;

			if (string.IsNullOrEmpty(accountType))
				return false;

			if (accountType == accountGroupType)
				return false;

			if (accountType == GL.AccountType.Liability && accountGroupType == GL.AccountType.Income)
				return false;

			if (accountType == GL.AccountType.Asset && accountGroupType == GL.AccountType.Expense)
				return false;

			if (accountType != accountGroupType)
				return true;

			return false;
		}
	}

	public class BudgetService : IBudgetService
	{
		private PXGraph graph;

		public BudgetService(PXGraph graph)
		{
			if (graph == null)
				throw new ArgumentNullException();

			this.graph = graph;
		}

		public virtual PMBudgetLite SelectProjectBalance(IProjectFilter filter)
		{
			if (CostCodeAttribute.UseCostCode())
			{
				return SelectProjectBalanceByCostCodes(filter);
			}
			else
			{
				return SelectProjectBalanceByInventory(filter);
			}
		}

		private PMBudgetLite SelectProjectBalanceByCostCodes(IProjectFilter filter)
		{
			PXSelectBase<PMBudgetLite> selectBudget = new PXSelect<PMBudgetLite,
				Where<PMBudgetLite.accountGroupID, Equal<Required<PMBudgetLite.accountGroupID>>,
				And<PMBudgetLite.projectID, Equal<Required<PMBudgetLite.projectID>>,
				And<PMBudgetLite.projectTaskID, Equal<Required<PMBudgetLite.projectTaskID>>,
				And<Where<PMBudgetLite.costCodeID, Equal<Required<PMBudgetLite.costCodeID>>, Or<PMBudgetLite.costCodeID, Equal<Required<PMBudgetLite.costCodeID>>>>>>>>>(graph);

			PMBudgetLite withCostCode = null;
			PMBudgetLite withoutCostCode = null;

			foreach (PMBudgetLite item in selectBudget.Select(filter.AccountGroupID, filter.ProjectID, filter.TaskID, filter.CostCodeID, CostCodeAttribute.GetDefaultCostCode()))//0..2 records
			{
				if (item.CostCodeID == CostCodeAttribute.GetDefaultCostCode())
				{
					withoutCostCode = item;
				}
				else
				{
					withCostCode = item;
				}
			}

			return withCostCode ?? withoutCostCode;
		}

		public virtual PMBudgetLite SelectProjectBalanceByInventory(IProjectFilter filter)
		{
			PXSelectBase<PMBudgetLite> selectBudget = new PXSelect<PMBudgetLite,
				Where<PMBudgetLite.accountGroupID, Equal<Required<PMBudgetLite.accountGroupID>>,
				And<PMBudgetLite.projectID, Equal<Required<PMBudgetLite.projectID>>,
				And<PMBudgetLite.projectTaskID, Equal<Required<PMBudgetLite.projectTaskID>>,
				And<Where<PMBudgetLite.inventoryID, Equal<Required<PMBudgetLite.inventoryID>>, Or<PMBudgetLite.inventoryID, Equal<Required<PMBudgetLite.inventoryID>>>>>>>>>(graph);

			PMBudgetLite withInventory = null;
			PMBudgetLite withoutInventory = null;

			foreach (PMBudgetLite item in selectBudget.Select(filter.AccountGroupID, filter.ProjectID, filter.TaskID, filter.InventoryID, PMInventorySelectorAttribute.EmptyInventoryID))//0..2 records
			{
				if (item.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID)
				{
					withoutInventory = item;
				}
				else
				{
					withInventory = item;
				}
			}

			return withInventory ?? withoutInventory;
		}
	}

	public interface IBudgetService
	{
		PMBudgetLite SelectProjectBalance(IProjectFilter filter);

		//PMBudgetLite SelectProjectBalanceByCostCodes(IProjectFilter filter);

		PMBudgetLite SelectProjectBalanceByInventory(IProjectFilter filter);
	}


	public interface IProjectFilter
	{
		int? AccountGroupID { get; }
		int? ProjectID { get; }
		int? TaskID { get; }
		int? InventoryID { get; }
		int? CostCodeID { get; }
	}

	public interface IQuantify
	{
		int? InventoryID { get; }
		string UOM { get; }
		decimal? Qty { get; }
	}
}