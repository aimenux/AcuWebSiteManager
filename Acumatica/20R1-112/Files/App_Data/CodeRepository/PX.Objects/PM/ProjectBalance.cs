using CommonServiceLocator;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;

namespace PX.Objects.PM
{
	public class ProjectBalance
	{
		protected PXGraph graph;
		protected IProjectSettingsManager settings;
		protected IBudgetService service;

		public IProjectSettingsManager Settings
		{
			get { return settings; }
		}

		public ProjectBalance(PXGraph graph) : this(graph, new BudgetService(graph), ServiceLocator.Current.GetInstance<IProjectSettingsManager>()) { }

		public ProjectBalance(PXGraph graph, IBudgetService service, IProjectSettingsManager settings)
		{
			this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
			this.service = service ?? throw new ArgumentNullException(nameof(service));
			this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
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
			PMBudgetLite target = null;
			bool isExisting;
			target = service.SelectProjectBalance(pmt, ag, project, out isExisting);

			var rollupQty = CalculateRollupQty(pmt, target);

			List<PMHistory> list = new List<PMHistory>();
			PMTaskTotal ta = null;
			PMBudget ps = null;
			PMForecastHistory forecast = null;

			if (pmt.TaskID != null && (rollupQty != 0 || pmt.Amount != 0)) //TaskID will be null for Contract
			{
				ps = new PMBudget();
				ps.ProjectID = target.ProjectID;
				ps.ProjectTaskID = target.TaskID;
				ps.AccountGroupID = target.AccountGroupID;
				ps.Type = target.Type;
				ps.InventoryID = target.InventoryID;
				ps.CostCodeID = target.CostCodeID;
				ps.UOM = target.UOM;
				ps.IsProduction = target.IsProduction;
				ps.Description = target.Description;
				if (ps.CuryInfoID == null)
				{
					ps.CuryInfoID = project.CuryInfoID;
				}
				decimal amt = amountSign * pmt.Amount.GetValueOrDefault();
				decimal curyAmt = amountSign * pmt.ProjectCuryAmount.GetValueOrDefault();

				ps.ActualQty = rollupQty * qtySign;
				ps.ActualAmount = amt;
				ps.CuryActualAmount = curyAmt;

				#region PMTask Totals Update

				ta = new PMTaskTotal();
				ta.ProjectID = ps.ProjectID;
				ta.TaskID = ps.TaskID;

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
				hist.ProjectID = ps.ProjectID;
				hist.ProjectTaskID = ps.TaskID;
				hist.AccountGroupID = ps.AccountGroupID;
				hist.InventoryID = pmt.InventoryID ?? ps.InventoryID;
				hist.CostCodeID = pmt.CostCodeID ?? ps.CostCodeID;
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
					tranHist.ProjectID = ps.ProjectID;
					tranHist.ProjectTaskID = ps.TaskID;
					tranHist.AccountGroupID = ps.AccountGroupID;
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

		public virtual decimal CalculateRollupQty<T>(T row, IQuantify budget) where T : IBqlTable, IQuantify
		{
			return CalculateRollupQty(row, budget, row.Qty);
		}

		public virtual decimal CalculateRollupQty<T>(T row, IQuantify budget, decimal? quantity) where T : IBqlTable, IQuantify
		{
			return CalculateRollupQty(row, budget, quantity.GetValueOrDefault());
		}

		private decimal CalculateRollupQty<T>(T row, IQuantify budget, decimal quantity) where T : IBqlTable, IQuantify
		{
			if (string.IsNullOrEmpty(budget.UOM) || string.IsNullOrEmpty(row.UOM) || quantity == 0)
				return 0;

			if (budget.UOM == row.UOM)
				return quantity;

			decimal result = 0;
			if (budget.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID)
			{
				//<Other> item is present. Update only if UOMs are convertable.
				decimal convertedQty;
				if (IN.INUnitAttribute.TryConvertGlobalUnits(graph, row.UOM, budget.UOM, quantity, IN.INPrecision.QUANTITY, out convertedQty))
				{
					result = convertedQty;
				}
			}
			else
			{
				//Item matches. Convert to UOM of Project Budget.
				if (PXAccess.FeatureInstalled<FeaturesSet.multipleUnitMeasure>())
				{
					decimal inBase = IN.INUnitAttribute.ConvertToBase(graph.Caches[typeof(T)], row.InventoryID, row.UOM, quantity, IN.INPrecision.QUANTITY);
					try
					{
						result = IN.INUnitAttribute.ConvertFromBase(graph.Caches[typeof(T)], row.InventoryID, budget.UOM, inBase, IN.INPrecision.QUANTITY);
					}
					catch (IN.PXUnitConversionException ex)
					{
						IN.InventoryItem item = PXSelectorAttribute.Select(graph.Caches[typeof(T)], row, "inventoryID") as IN.InventoryItem;
						string msg = PXMessages.LocalizeFormatNoPrefixNLA(Messages.UnitConversionNotDefinedForItemOnBudgetUpdate, item?.BaseUnit, budget.UOM, item?.InventoryCD);

						throw new PXException(msg, ex);

					}
				}
				else
				{
					result = IN.INUnitAttribute.ConvertGlobalUnits(graph, row.UOM, budget.UOM, quantity, IN.INPrecision.QUANTITY);
				}
			}
			return result;
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



	public interface IQuantify
	{
		int? InventoryID { get; }
		string UOM { get; }
		decimal? Qty { get; }
	}
}