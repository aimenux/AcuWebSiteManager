using PX.Data;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PM.BudgetControl
{
	public class APInvoiceEntryExt : BudgetControlGraph<APInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.projectModule>();
		}

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(APInvoice))
			{
				CuryID = typeof(APInvoice.curyID),
				Date = typeof(APInvoice.docDate),
				Hold = typeof(APInvoice.hold),
				WarningAmount = typeof(APInvoice.curyDocBal)
			};
		}

		protected override DetailMapping GetDetailMapping()
		{
			return new DetailMapping(typeof(APTran))
			{
				ProjectID = typeof(APTran.projectID),
				TaskID = typeof(APTran.taskID),
				InventoryID = typeof(APTran.inventoryID),
				CostCodeID = typeof(APTran.costCodeID),
				WarningAmount = typeof(APTran.curyLineAmt),
				LineNbr = typeof(APTran.lineNbr)
			};
		}

		protected override int? GetAccountGroup(Detail row)
		{
			APTran tran = (APTran)Details.Cache.GetMain(row);

			InventoryItem item = (InventoryItem)PXSelectorAttribute.Select<APTran.inventoryID>(Base.Transactions.Cache, tran);
			if (item != null && item.StkItem == true && item.COGSAcctID != null)
			{
				Account account = (Account)PXSelectorAttribute.Select<InventoryItem.cOGSAcctID>(Base.Caches[typeof(InventoryItem)], item);
				if (account != null && account.AccountGroupID != null)
					return account.AccountGroupID;
			}
			else
			{
				Account account = (Account)PXSelectorAttribute.Select<APTran.accountID>(Base.Transactions.Cache, tran);
				if (account != null && account.AccountGroupID != null)
					return account.AccountGroupID;
			}
			return null;
		}

		protected override bool DetailIsChanged(Detail oldRow, Detail row)
		{
			APTran oldTran = (APTran)Details.Cache.GetMain(oldRow);
			APTran tran = (APTran)Details.Cache.GetMain(row);
			var result =
				oldTran.ProjectID != tran.ProjectID ||
				oldTran.TaskID != tran.TaskID ||
				oldTran.InventoryID != tran.InventoryID ||
				oldTran.CostCodeID != tran.CostCodeID ||
				oldTran.AccountID != tran.AccountID ||
				oldTran.PONbr != tran.PONbr ||
				oldTran.CuryTranAmt != tran.CuryTranAmt ||
				oldTran.CuryRetainageAmt != tran.CuryRetainageAmt;
			return result;
		}

		protected override decimal? GetDetailBaseAmount(Detail row)
		{
			APTran line = (APTran)Details.Cache.GetMain(row);
			var result = line.TranAmt + line.RetainageAmt.GetValueOrDefault();
			return result;
		}

		protected override decimal? GetDetailCuryAmount(Detail row)
		{
			APTran line = (APTran)Details.Cache.GetMain(row);
			var result = line.CuryTranAmt + line.CuryRetainageAmt.GetValueOrDefault();
			return result;
		}

		protected override bool IsBudgetControlRequiredForDocument()
		{
			if (Base.Document?.Current == null) return false;
			if (Base.Document.Current.DocType == APDocType.DebitAdj) return false;
			var result = Base.Document.Current.Released == false && Base.Document.Current.Voided == false && Base.Document.Current.Rejected == false;
			return result;
		}

		protected override decimal? GetConsumedAmount(Detail row, Lite.PMBudget budget)
		{
			APTran line = (APTran)Details.Cache.GetMain(row);
			var isCommitmentRelated = line.PONbr != null;
			var result = isCommitmentRelated ? (budget.CuryActualAmount ?? 0) : base.GetConsumedAmount(row, budget);
			return result;
		}
	}
}