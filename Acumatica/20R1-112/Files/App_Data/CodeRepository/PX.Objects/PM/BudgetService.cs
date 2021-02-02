using CommonServiceLocator;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.IN;
using System;
using System.Collections.Generic;
using System.Linq;
using PMBudgetLite = PX.Objects.PM.Lite.PMBudget;

namespace PX.Objects.PM
{
	public class BudgetService : IBudgetService
	{
		protected PXGraph graph;
		protected IProjectSettingsManager settings;

		public BudgetService(PXGraph graph):this(graph, ServiceLocator.Current.GetInstance<IProjectSettingsManager>())
		{			
		}

		public BudgetService(PXGraph graph, IProjectSettingsManager settings)
		{
			this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
			this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}


		public virtual PMBudgetLite SelectProjectBalance(IProjectFilter filter, PMAccountGroup ag, PMProject project, out bool isExisting)
		{
			return SelectProjectBalance(ag, project, filter.TaskID, filter.InventoryID, filter.CostCodeID, out isExisting);
		}
		public virtual PMBudgetLite SelectProjectBalance(PMAccountGroup ag, PMProject project, int? taskID, int? inventoryID, int? costCodeID, out bool isExisting)
		{
			BudgetKeyTuple key = new BudgetKeyTuple(project.ContractID.Value, taskID.Value, ag.GroupID.Value, inventoryID.GetValueOrDefault(PMInventorySelectorAttribute.EmptyInventoryID), costCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode()));
						
			string budgetLevel = BudgetLevels.Task;
			string updateMode = CostBudgetUpdateModes.Summary;
			if (ag.Type == GL.AccountType.Income)
			{
				budgetLevel = project.BudgetLevel;
				updateMode = settings.RevenueBudgetUpdateMode;
			}
			else if (ag.IsExpense == true)
			{
				budgetLevel = project.CostBudgetLevel;
				updateMode = settings.CostBudgetUpdateMode;
			}
			
			PMBudgetLite target = SelectExistingBalance(key, budgetLevel, updateMode);
			if (target != null)
			{
				isExisting = true;
			}
			else
			{
				isExisting = false;
				target = BuildTarget(key, ag, budgetLevel, updateMode);
			}
			
			return target;
		}

		protected virtual PMBudgetLite SelectExistingBalance(BudgetKeyTuple key, string budgetLevel, string budgetUpdateMode)
		{			
			PMBudgetLite result = null;
			PMBudgetLite fallback = null;

			foreach (PMBudgetLite budget in SelectExistingBalances(key, budgetLevel, budgetUpdateMode))
			{
				if (budget.CostCodeID == key.CostCodeID && budget.InventoryID == key.InventoryID)
				{
					result = budget;
					break;
				}
				else
				{
					if (budgetLevel == BudgetLevels.CostCode)
					{
						if (budget.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID && budget.CostCodeID == key.CostCodeID)
						{
							result = budget;
							break;
						}
					}
					else
					{
						if (budget.InventoryID == key.InventoryID && budget.CostCodeID == CostCodeAttribute.DefaultCostCode)
						{
							result = budget;
							break;
						}
					}

					if (budget.InventoryID == PMInventorySelectorAttribute.EmptyInventoryID && budget.CostCodeID == CostCodeAttribute.DefaultCostCode)
					{
						fallback = budget;
					}
				}
			}

			return result ?? fallback;
		}

		protected virtual List<PMBudgetLite> SelectExistingBalances(BudgetKeyTuple key, string budgetLevel, string budgetUpdateMode)
		{
			List<int?> costCodes = new List<int?>();
			List<int?> items = new List<int?>();

			if (budgetLevel == BudgetLevels.Task)
			{
				items.Add(PMInventorySelectorAttribute.EmptyInventoryID);
				costCodes.Add(CostCodeAttribute.DefaultCostCode);
			}
			else if (budgetLevel == BudgetLevels.Item)
			{
				items.Add(key.InventoryID);
				costCodes.Add(CostCodeAttribute.DefaultCostCode);

				if (budgetUpdateMode == CostBudgetUpdateModes.Summary && key.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					items.Add(PMInventorySelectorAttribute.EmptyInventoryID);
				}
			}
			else if (budgetLevel == BudgetLevels.CostCode)
			{
				items.Add(PMInventorySelectorAttribute.EmptyInventoryID);
				costCodes.Add(key.CostCodeID);

				if (budgetUpdateMode == CostBudgetUpdateModes.Summary && key.CostCodeID != CostCodeAttribute.DefaultCostCode)
				{
					costCodes.Add(CostCodeAttribute.DefaultCostCode);
				}
			}
			else if (budgetLevel == BudgetLevels.Detail)
			{
				items.Add(key.InventoryID);
				costCodes.Add(key.CostCodeID);

				if (budgetUpdateMode == CostBudgetUpdateModes.Summary && key.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					items.Add(PMInventorySelectorAttribute.EmptyInventoryID);
				}

				if (budgetUpdateMode == CostBudgetUpdateModes.Summary && key.CostCodeID != CostCodeAttribute.DefaultCostCode)
				{
					costCodes.Add(CostCodeAttribute.DefaultCostCode);
				}				
			}
			else
			{
				throw new ArgumentException(string.Format("Unknown budget level = {0}", budgetLevel), nameof(budgetLevel));
			}

			return SelectExistingBalances(key.ProjectID, key.ProjectTaskID, key.AccountGroupID, costCodes.ToArray(), items.ToArray());
		}

		protected virtual List<PMBudgetLite> SelectExistingBalances(int projectID, int taskID, int accountGroupID, int?[] costCodes, int?[] items)
		{
			List<PMBudgetLite> list = new List<PMBudgetLite>();

			PXSelectBase<PMBudgetLite> selectBudget = new PXSelectReadonly<PMBudgetLite,
				Where<PMBudgetLite.accountGroupID, Equal<Required<PMBudgetLite.accountGroupID>>,
				And<PMBudgetLite.projectID, Equal<Required<PMBudgetLite.projectID>>,
				And<PMBudgetLite.projectTaskID, Equal<Required<PMBudgetLite.projectTaskID>>,
				And<PMBudgetLite.costCodeID, In<Required<PMBudgetLite.costCodeID>>,
				And<PMBudgetLite.inventoryID, In<Required<PMBudgetLite.inventoryID>>>>>>>>(graph);

			foreach (PMBudgetLite budget in selectBudget.Select(accountGroupID, projectID, taskID, costCodes.ToArray(), items.ToArray()))
			{
				list.Add(budget);
			}

			return list;
		}

		protected virtual PMBudgetLite BuildTarget(BudgetKeyTuple key, PMAccountGroup accountGroup, string budgetLevel, string budgetUpdateMode)
		{
			if (accountGroup == null) throw new ArgumentNullException(nameof(accountGroup));
			if (accountGroup.GroupID != key.AccountGroupID) throw new ArgumentException("AccountGroup doesnot match key.AccountGroupID");
						
			PMBudgetLite target = new PMBudgetLite();
			target.ProjectID = key.ProjectID;
			target.ProjectTaskID = key.ProjectTaskID;
			target.AccountGroupID = key.AccountGroupID;
			target.InventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
			target.CostCodeID = CostCodeAttribute.GetDefaultCostCode();
			target.IsProduction = false;
			target.Type = accountGroup.IsExpense == true ? GL.AccountType.Expense : accountGroup.Type;

			if (budgetLevel == BudgetLevels.Task)
			{
				//default
			}
			else if (budgetLevel == BudgetLevels.Item)
			{
				target.InventoryID = budgetUpdateMode == CostBudgetUpdateModes.Summary ? PMInventorySelectorAttribute.EmptyInventoryID : key.InventoryID;
				if (target.InventoryID != PMInventorySelectorAttribute.EmptyInventoryID)
				{
					InventoryItem item = PXSelect<InventoryItem, Where<InventoryItem.inventoryID, Equal<Required<InventoryItem.inventoryID>>>>.Select(graph, target.InventoryID);
					if (item != null)
					{
						target.Description = item.Descr;
						target.UOM = item.BaseUnit;
					}
				}
			}
			else if (budgetLevel == BudgetLevels.CostCode)
			{
				target.CostCodeID = budgetUpdateMode == CostBudgetUpdateModes.Summary ? CostCodeAttribute.GetDefaultCostCode() : key.CostCodeID;
				PMCostCode costCode = PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Required<PMCostCode.costCodeID>>>>.Select(graph, target.CostCodeID);
				if (costCode != null)
				{
					target.Description = costCode.Description;
				}
			}
			else if (budgetLevel == BudgetLevels.Detail)
			{
				target.InventoryID = budgetUpdateMode == CostBudgetUpdateModes.Summary ? PMInventorySelectorAttribute.EmptyInventoryID : key.InventoryID;
				target.CostCodeID = budgetUpdateMode == CostBudgetUpdateModes.Summary ? CostCodeAttribute.GetDefaultCostCode() : key.CostCodeID;
				PMCostCode costCode = PXSelect<PMCostCode, Where<PMCostCode.costCodeID, Equal<Required<PMCostCode.costCodeID>>>>.Select(graph, target.CostCodeID);
				if (costCode != null)
				{
					target.Description = costCode.Description;
				}
			}
			else
			{
				throw new ArgumentException(string.Format("Unknown budget level = {0}", budgetLevel), nameof(budgetLevel));
			}
						
			return target;
		}
	}

	public interface IBudgetService
	{
		PMBudgetLite SelectProjectBalance(IProjectFilter filter, PMAccountGroup accountGroup, PMProject project, out bool isExisting);
	}

	public interface IProjectFilter
	{
		int? AccountGroupID { get; }
		int? ProjectID { get; }
		int? TaskID { get; }
		int? InventoryID { get; }
		int? CostCodeID { get; }
	}
}
