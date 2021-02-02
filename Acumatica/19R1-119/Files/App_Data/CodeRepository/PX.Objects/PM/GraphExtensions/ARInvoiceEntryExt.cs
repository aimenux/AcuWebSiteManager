using PX.Common;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.PM
{
	public class ARInvoiceEntryExt : PXGraphExtension<ARInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>();
		}


		protected virtual void ARTran_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			if (((ARTran)e.Row).TaskID != null && Base.Document.Current.ProformaExists != true)
			{
				AddToInvoiced((ARTran)e.Row, GetProjectedAccountGroup((ARTran)e.Row), (int)ARDocType.SignAmount(((ARTran)e.Row).TranType).GetValueOrDefault(1));
			}
		}

		protected virtual void ARTran_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			ARTran row = (ARTran)e.Row;
			ARTran oldRow = (ARTran)e.OldRow;
			if (row != null)
			{
				SyncBudgets(row, oldRow);
			}
		}

		protected virtual void _(Events.RowUpdated<CurrencyInfo> e)
		{
			if (e.Row == null) return;

			foreach (ARTran tran in Base.Transactions.Select())
			{
				decimal newTranAmt = 0;
				if (e.Row.CuryRate != null)
					PXCurrencyAttribute.CuryConvBase(e.Cache, e.Row, tran.CuryTranAmt.GetValueOrDefault(), out newTranAmt);
				var newTran = Base.Transactions.Cache.CreateCopy(tran) as ARTran;
				newTran.TranAmt = newTranAmt;

				decimal oldTranAmt = 0;
				if (e.OldRow.CuryRate != null)
					PXCurrencyAttribute.CuryConvBase(e.Cache, e.OldRow, tran.CuryTranAmt.GetValueOrDefault(), out oldTranAmt);
				var oldTran = Base.Transactions.Cache.CreateCopy(tran) as ARTran;
				oldTran.TranAmt = oldTranAmt;

				SyncBudgets(newTran, oldTran);
			}
		}

		protected virtual void ARTran_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
		{
			if (((ARTran)e.Row).TaskID != null && Base.Document.Current.ProformaExists != true)
			{
				AddToInvoiced((ARTran)e.Row, GetProjectedAccountGroup((ARTran)e.Row), -1 * (int)ARDocType.SignAmount(((ARTran)e.Row).TranType).GetValueOrDefault(1));

				var select = new PXSelect<PMTran, Where<PMTran.aRTranType, Equal<Required<PMTran.aRTranType>>,
					And<PMTran.aRRefNbr, Equal<Required<PMTran.aRRefNbr>>,
					And<PMTran.refLineNbr, Equal<Required<PMTran.refLineNbr>>>>>>(Base);

				PMTran original = select.SelectWindowed(0, 1, ((ARTran)e.Row).TranType, ((ARTran)e.Row).RefNbr, ((ARTran)e.Row).LineNbr);

				if (original == null)//progressive line
					SubtractAmountToInvoice(((ARTran)e.Row), GetProjectedAccountGroup((ARTran)e.Row), -1 * (int)ARDocType.SignAmount(((ARTran)e.Row).TranType).GetValueOrDefault(1)); //Restoring AmountToInvoice
			}
		}

		private void SyncBudgets(ARTran row, ARTran oldRow)
		{
			if (Base.Document.Current.ProformaExists != true && (row.TaskID != oldRow.TaskID || row.TranAmt != oldRow.TranAmt || row.AccountID != oldRow.AccountID))
			{
				if (oldRow.TaskID != null)
				{
					AddToInvoiced(oldRow, GetProjectedAccountGroup(oldRow), -1 * (int)ARDocType.SignAmount(oldRow.TranType).GetValueOrDefault(1));
				}
				if (row.TaskID != null)
				{
					AddToInvoiced(row, GetProjectedAccountGroup(row), (int)ARDocType.SignAmount(oldRow.TranType).GetValueOrDefault(1));
				}
			}
		}

		public virtual int? GetProjectedAccountGroup(ARTran line)
		{
			int? projectedRevenueAccountGroupID = null;
			int? projectedRevenueAccount = line.AccountID;

			if (line.AccountID != null)
			{
				Account revenueAccount = PXSelectorAttribute.Select<ARTran.accountID>(Base.Transactions.Cache, line, line.AccountID) as Account;
				if (revenueAccount != null)
				{
					if (revenueAccount.AccountGroupID == null)
						throw new PXException(PM.Messages.RevenueAccountIsNotMappedToAccountGroup, revenueAccount.AccountCD);

					projectedRevenueAccountGroupID = revenueAccount.AccountGroupID;
				}
			}

			return projectedRevenueAccountGroupID;
		}

		public virtual void AddToInvoiced(ARTran line, int? revenueAccountGroup, int mult = 1)
		{
			if (line.TaskID == null)
				return;

			if (revenueAccountGroup == null)
				return;

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, line.ProjectID);
			if (project != null && project.NonProject != true)
			{
				int? inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				int? costCodeID = line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());
				if (project.BudgetLevel == BudgetLevels.Item)
				{
					var selectRevenueBudget = new PXSelect<PMRevenueBudget,
									Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
									And<PMRevenueBudget.type, Equal<GL.AccountType.income>,
									And<PMRevenueBudget.projectTaskID, Equal<Required<PMRevenueBudget.projectTaskID>>>>>>(Base);
					var revenueBudget = selectRevenueBudget.Select(line.ProjectID, line.TaskID);

					foreach (PMRevenueBudget budget in revenueBudget)
					{
						if (budget.TaskID == line.TaskID && line.InventoryID == budget.InventoryID)
						{
							inventoryID = line.InventoryID;
						}
					}

				}
				else if (project.BudgetLevel == BudgetLevels.Task)
				{
					costCodeID = CostCodeAttribute.GetDefaultCostCode();
				}

				PMBudgetAccum invoiced = new PMBudgetAccum();
				invoiced.Type = GL.AccountType.Income;
				invoiced.ProjectID = line.ProjectID;
				invoiced.ProjectTaskID = line.TaskID;
				invoiced.AccountGroupID = revenueAccountGroup;
				invoiced.InventoryID = inventoryID;
				invoiced.CostCodeID = costCodeID;

				invoiced = Base.Budget.Insert(invoiced);

				if (project.CuryID == project.BillingCuryID)
				{
					invoiced.CuryInvoicedAmount += mult * (line.CuryTranAmt.GetValueOrDefault() + line.CuryRetainageAmt.GetValueOrDefault());
					invoiced.InvoicedAmount += mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
				}
				else
				{
					invoiced.CuryInvoicedAmount += mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
					invoiced.InvoicedAmount += mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
				}
			}
		}

		public virtual void SubtractAmountToInvoice(ARTran line, int? revenueAccountGroup, int mult = 1)
		{
			if (line.TaskID == null)
				return;

			if (revenueAccountGroup == null)
				return;

			PMProject project = PXSelect<PMProject, Where<PMProject.contractID, Equal<Required<PMProject.contractID>>>>.Select(Base, line.ProjectID);
			if (project != null && project.NonProject != true)
			{
				int? inventoryID = PMInventorySelectorAttribute.EmptyInventoryID;
				int? costCodeID = line.CostCodeID.GetValueOrDefault(CostCodeAttribute.GetDefaultCostCode());
				if (project.BudgetLevel == BudgetLevels.Item)
				{
					var selectRevenueBudget = new PXSelect<PMRevenueBudget,
									Where<PMRevenueBudget.projectID, Equal<Required<PMRevenueBudget.projectID>>,
									And<PMRevenueBudget.type, Equal<GL.AccountType.income>,
									And<PMRevenueBudget.projectTaskID, Equal<Required<PMRevenueBudget.projectTaskID>>>>>>(Base);
					var revenueBudget = selectRevenueBudget.Select(line.ProjectID, line.TaskID);

					foreach (PMRevenueBudget budget in revenueBudget)
					{
						if (budget.TaskID == line.TaskID && line.InventoryID == budget.InventoryID)
						{
							inventoryID = line.InventoryID;
						}
					}

				}
				else if (project.BudgetLevel == BudgetLevels.Task)
				{
					costCodeID = CostCodeAttribute.GetDefaultCostCode();
				}

				PMBudgetAccum invoiced = new PMBudgetAccum();
				invoiced.Type = GL.AccountType.Income;
				invoiced.ProjectID = line.ProjectID;
				invoiced.ProjectTaskID = line.TaskID;
				invoiced.AccountGroupID = revenueAccountGroup;
				invoiced.InventoryID = inventoryID;
				invoiced.CostCodeID = costCodeID;
				invoiced.CuryInfoID = line.CuryInfoID;

				invoiced = Base.Budget.Insert(invoiced);

				if (project.CuryID == project.BillingCuryID)
				{
					invoiced.CuryAmountToInvoice -= mult * (line.CuryTranAmt.GetValueOrDefault() + line.CuryRetainageAmt.GetValueOrDefault());
					invoiced.AmountToInvoice -= mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
				}
				else
				{
					invoiced.CuryAmountToInvoice -= mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
					invoiced.AmountToInvoice -= mult * (line.TranAmt.GetValueOrDefault() + line.RetainageAmt.GetValueOrDefault());
				}
			}
		}

	}
}