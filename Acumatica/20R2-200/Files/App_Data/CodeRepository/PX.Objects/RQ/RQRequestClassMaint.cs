using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.RQ
{
	public class RQRequestClassMaint : PXGraph<RQRequestClassMaint, RQRequestClass>
	{		
		public PXSelect<RQRequestClass> Classes;
		public PXSelect<RQRequestClass, Where<RQRequestClass.reqClassID, Equal<Current<RQRequestClass.reqClassID>>>> CurrentClass;
		
		public PXSelectJoin<RQRequestClassItem,
			InnerJoin<RQInventoryItem, On<RQInventoryItem.inventoryID, Equal<RQRequestClassItem.inventoryID>>>,
			Where<RQRequestClassItem.reqClassID, Equal<Optional<RQRequestClass.reqClassID>>>> ClassItems;

		protected virtual void RQRequestClass_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			RQRequestClass row = (RQRequestClass)e.Row;
			if (row != null)
			{

				ClassItems.Cache.AllowInsert =
					ClassItems.Cache.AllowUpdate =
						ClassItems.Cache.AllowDelete = row.RestrictItemList == true;

				bool budgetValidation = row.BudgetValidation > RQRequestClassBudget.None;
				PXUIFieldAttribute.SetEnabled<RQRequestClass.expenseAccountDefault>(sender, row, budgetValidation);
				PXUIFieldAttribute.SetEnabled<RQRequestClass.expenseSubMask>(sender, row, budgetValidation);
				PXUIFieldAttribute.SetEnabled<RQRequestClass.expenseAcctID>(sender, row, budgetValidation);
				PXUIFieldAttribute.SetEnabled<RQRequestClass.expenseSubID>(sender, row, budgetValidation);				


				PXUIFieldAttribute.SetEnabled<RQRequestClass.hideInventoryID>(sender, row, !(row.RestrictItemList == true));
				PXUIFieldAttribute.SetEnabled<RQRequestClass.vendorMultiply>(sender, row, !(row.VendorNotRequest == true));
				PXUIFieldAttribute.SetEnabled<RQRequestClass.budgetValidation>(sender, row, !(row.CustomerRequest == true));

				RQRequest req = PXSelect<RQRequest,
				Where<RQRequest.reqClassID, Equal<Required<RQRequest.reqClassID>>>>.Select(this, row.ReqClassID);				
				PXUIFieldAttribute.SetEnabled<RQRequestClass.customerRequest>(sender, row, req == null);				
				
			}					
		}
		
		protected virtual void RQRequestClass_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			RQRequestClass row = (RQRequestClass)e.NewRow;
			
			if (row != null && row.CustomerRequest == true)
			{
				row.BudgetValidation = RQRequestClassBudget.None;
				if (row.ExpenseAccountDefault == RQAccountSource.Department)
				{
					sender.RaiseExceptionHandling<RQRequestClass.expenseAccountDefault>(row,
						row.ExpenseAccountDefault,
						new PXSetPropertyException(Messages.ExpenseAccDefaultByDepartment));					
				}
				if (row.ExpenseSubMask != null && 
					  row.ExpenseAccountDefault != RQAccountSource.None &&
					  row.ExpenseSubMask.Contains(RQAcctSubDefault.MaskDepartment))
				{
					sender.RaiseExceptionHandling<RQRequestClass.expenseSubMask>(row,
						row.ExpenseSubMask,
						new PXSetPropertyException(Messages.SubAccDefaultByDepartment));					
				}
			}
			if (row != null && row.HideInventoryID == true)
			{
				if (row.ExpenseAccountDefault == RQAccountSource.PurchaseItem)
				{
					sender.RaiseExceptionHandling<RQRequestClass.expenseAccountDefault>(row,
						row.ExpenseAccountDefault,
						new PXSetPropertyException(Messages.ExpenseAccDefaultByItem));
				}
				if (row.ExpenseSubMask != null &&
					  row.ExpenseAccountDefault != RQAccountSource.None &&
					  row.ExpenseSubMask.Contains(RQAcctSubDefault.MaskItem))
				{
					sender.RaiseExceptionHandling<RQRequestClass.expenseSubMask>(row,
						row.ExpenseSubMask,
						new PXSetPropertyException(Messages.SubAccDefaultByItem));
				}
			}
		}			
		protected virtual void RQRequestClass_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			RQRequestClass row = (RQRequestClass)e.Row;
			if(row == null) return;
			if (row.VendorNotRequest == true) row.VendorMultiply = false;
			if (row.RestrictItemList == true) row.HideInventoryID = false;			
	}

		protected virtual void RQRequestClass_RestrictItemList_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			RQRequestClass row = (RQRequestClass)e.Row;

		}
		protected virtual void RQRequestClass_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			RQRequestClass row = (RQRequestClass)e.Row;

			PXDefaultAttribute.SetPersistingCheck<RQRequestClass.expenseAcctID>
					(sender, row,
					row.ExpenseAccountDefault == RQAccountSource.RequestClass ||
					row.ExpenseAccountDefault == RQAccountSource.PurchaseItem
					? PXPersistingCheck.NullOrBlank
					: PXPersistingCheck.Nothing);

			PXDefaultAttribute.SetPersistingCheck<RQRequestClass.expenseSubID>(sender, row,
				row.ExpenseAccountDefault == RQAccountSource.RequestClass ||
				row.ExpenseAccountDefault == RQAccountSource.PurchaseItem ||
				row.ExpenseSubMask != null && (row.ExpenseSubMask.Contains(RQAcctSubDefault.MaskClass) || row.ExpenseSubMask.Contains(RQAcctSubDefault.MaskItem))
				? PXPersistingCheck.NullOrBlank
				: PXPersistingCheck.Nothing);

			if (e.Operation == PXDBOperation.Insert || e.Operation == PXDBOperation.Update)
			{
				if (row.RestrictItemList == true)
				{
					RQRequestClassItem item = this.ClassItems.SelectWindowed(0, 1, row.ReqClassID);
					if (item == null)
						throw new PXRowPersistedException(typeof(RQRequestClass).Name, item, Messages.ItemListShouldBeDefined);
				}
			}
		}
		protected virtual void RQRequestClass_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			RQRequestClass row = (RQRequestClass)e.Row;
			if (row != null)
			{
				RQSetup setup = PXSelect<RQSetup,
					Where<RQSetup.defaultReqClassID, Equal<Required<RQSetup.defaultReqClassID>>>>.Select(this, row.ReqClassID);
				if (setup != null)
				{
					e.Cancel = true;
					throw new PXRowPersistingException(sender.GetItemType().Name, row, Messages.DeleteSetupClass);
				}

				RQRequest req = PXSelect<RQRequest, Where<RQRequest.reqClassID, Equal<Required<RQRequest.reqClassID>>>>.SelectWindowed(this, 0, 1, row.ReqClassID);
				if (req != null)
				{
					e.Cancel = true;
					throw new PXRowPersistingException(sender.GetItemType().Name, row, Messages.UnableDeleteReqClass);
				}
			}
		}

		protected virtual void RQRequestClassItem_RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			this.Classes.Cache.MarkUpdated(this.Classes.Cache.Current);
			RQRequestClassItem row = (RQRequestClassItem)e.Row;
			if (row != null)
				e.Cancel = !ValidateDuplicates(sender, row, null);
		}

		protected virtual void RQRequestClassItem_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			this.Classes.Cache.SetStatus(this.Classes.Cache.Current, PXEntryStatus.Updated);
			RQRequestClassItem row = (RQRequestClassItem)e.Row;
			RQRequestClassItem newRow = (RQRequestClassItem)e.NewRow;
			if (row != null && newRow != null && row != newRow &&
				 (row.ReqClassID != newRow.ReqClassID || row.InventoryID != newRow.InventoryID))
				e.Cancel = !ValidateDuplicates(sender, newRow, row);			
		}
		protected virtual void RQRequestClassItem_RowDeleting(PXCache sender, PXRowDeletingEventArgs e)
		{
			this.Classes.Cache.SetStatus(this.Classes.Cache.Current, PXEntryStatus.Updated);
		}

		private bool ValidateDuplicates(PXCache sender, RQRequestClassItem row, RQRequestClassItem oldRow)
		{
			if(row.InventoryID != null)
				foreach (RQRequestClassItem sibling in ClassItems.Select(row.ReqClassID))
				{
					if (String.Equals(sibling.ReqClassID, row.ReqClassID, StringComparison.OrdinalIgnoreCase) &&						
							sibling.InventoryID == row.InventoryID &&
							row.LineID != sibling.LineID)
					{
						if (oldRow == null || oldRow.ReqClassID != row.ReqClassID)
							sender.RaiseExceptionHandling<RQRequestClassItem.reqClassID>(
								row, row.ReqClassID, new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded)
								);
						if (oldRow == null || oldRow.InventoryID != row.InventoryID)
							sender.RaiseExceptionHandling<RQRequestClassItem.inventoryID>(
								row, row.InventoryID, new PXSetPropertyException(ErrorMessages.DuplicateEntryAdded)
								);
						return false;
					}
				}
			PXUIFieldAttribute.SetError<RQRequestClassItem.reqClassID>(sender, row, null);
			PXUIFieldAttribute.SetError<RQRequestClassItem.inventoryID>(sender, row, null);
			return true;
		}		
	}
}
