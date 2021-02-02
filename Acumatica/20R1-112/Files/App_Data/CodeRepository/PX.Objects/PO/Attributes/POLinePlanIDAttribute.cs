using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.PO
{
	public abstract class POLinePlanIDBaseAttribute : INItemPlanIDAttribute
	{
		#region Ctor
		public POLinePlanIDBaseAttribute(Type ParentNoteID, Type ParentHoldEntry)
			: base(ParentNoteID, ParentHoldEntry)
		{
		}
		#endregion
		#region Implementation

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object orig_Row)
		{
			string orderType = (string)sender.GetValue<POLine.orderType>(orig_Row);
			string orderNbr = (string)sender.GetValue<POLine.orderNbr>(orig_Row);
			int? inventoryID = (int?)sender.GetValue<POLine.inventoryID>(orig_Row);
			int? siteID = (int?)sender.GetValue<POLine.siteID>(orig_Row);
			bool? cancelled = (bool?)sender.GetValue<POLine.cancelled>(orig_Row);
			bool? completed = (bool?)sender.GetValue<POLine.completed>(orig_Row);

			if (orderType.IsNotIn(POOrderType.RegularOrder, POOrderType.DropShip, POOrderType.Blanket)
				|| inventoryID == null || siteID == null
				|| cancelled == true || completed == true)
			{
				return null;
			}
			PXCache parentCache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			POOrder currentOrder = (POOrder)parentCache.Current;
			POOrder order =
				currentOrder?.OrderType == orderType && currentOrder.OrderNbr == orderNbr
				? currentOrder
				: (POOrder)PXParentAttribute.SelectParent(sender, orig_Row, typeof(POOrder)); ;
			bool IsOnHold = IsOrderOnHold(order);

			string newPlanType;
			if (!TryCalcPlanType(sender, orig_Row, IsOnHold, out newPlanType))
			{
				return null;
			}
			plan_Row.PlanType = newPlanType;
			plan_Row.BAccountID = (int?)sender.GetValue<POLine.vendorID>(orig_Row);
			plan_Row.InventoryID = inventoryID;
			plan_Row.SubItemID = (int?)sender.GetValue<POLine.subItemID>(orig_Row);
			plan_Row.SiteID = siteID;
			plan_Row.PlanDate = (DateTime?)sender.GetValue<POLine.promisedDate>(orig_Row);
			plan_Row.PlanQty = (decimal?)sender.GetValue<POLine.baseOpenQty>(orig_Row);

			plan_Row.RefNoteID = (Guid?)parentCache.GetValue(order, _ParentNoteID.Name);
			plan_Row.Hold = IsOnHold;

			if (string.IsNullOrEmpty(plan_Row.PlanType))
			{
				return null;
			}
			return plan_Row;
		}

		protected virtual bool IsOrderOnHold(POOrder order)
		{
			return (order != null) && order.Status.IsNotIn(POOrderStatus.Open, POOrderStatus.Completed, POOrderStatus.Closed);
		}

		protected virtual bool TryCalcPlanType(PXCache sender, object line, bool isOnHold, out string newPlanType)
		{
			string orderType = (string)sender.GetValue<POLine.orderType>(line);
			string lineType = (string)sender.GetValue<POLine.lineType>(line);
			newPlanType = null;
			if (orderType == POOrderType.Blanket)
				newPlanType = INPlanConstants.Plan7B;
			else
				switch (lineType)
				{
					case POLineType.GoodsForManufacturing:
					case POLineType.NonStockForManufacturing:
						newPlanType = isOnHold ? INPlanConstants.PlanM3 : INPlanConstants.PlanM4;
						break;
					case POLineType.GoodsForSalesOrder:
					case POLineType.NonStockForSalesOrder:
						newPlanType = isOnHold ? INPlanConstants.Plan78 : INPlanConstants.Plan76;
						break;
					case POLineType.GoodsForDropShip:
					case POLineType.NonStockForDropShip:
						newPlanType = isOnHold ? INPlanConstants.Plan79 : INPlanConstants.Plan74;
						break;
					case POLineType.GoodsForInventory:
					case POLineType.GoodsForReplenishment:
					case POLineType.NonStock:
						newPlanType = isOnHold ? INPlanConstants.Plan73 : INPlanConstants.Plan70;
						break;
					case POLineType.GoodsForServiceOrder:
					case POLineType.NonStockForServiceOrder:
						newPlanType = isOnHold ? INPlanConstants.PlanF8 : INPlanConstants.PlanF7;
						break;
				}
			return newPlanType != null;
		}

		protected override void SetPlanID(PXCache sender, object row, long? planID)
		{
			base.SetPlanID(sender, row, planID);
			sender.SetValue<POLine.clearPlanID>(row, false);
		}

		protected override void ClearPlanID(PXCache sender, object row)
		{
			long? planID = (long?)sender.GetValue<POLine.planID>(row);
			if (planID == null || planID < 0L)
			{
				base.ClearPlanID(sender, row);
			}
			else
			{
				// we need to postpone clearing of the PlanID field till row persisting
				// to make other pieces of code work
				sender.SetValue<POLine.clearPlanID>(row, true);
			}
		}

		public override void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if ((bool?)sender.GetValue<POLine.clearPlanID>(e.Row) == true)
			{
				base.ClearPlanID(sender, e.Row);
				sender.SetValue<POLine.clearPlanID>(e.Row, null);
			}

			base.RowPersisting(sender, e);
		}

		#endregion
	}

	public class POLinePlanIDAttribute : POLinePlanIDBaseAttribute
	{
		#region Ctor
		public POLinePlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry)
			: base(ParentNoteID, ParentHoldEntry)
		{
		}
		#endregion
		#region Implementation

		protected virtual bool TryCalcPlanType(INItemPlan plan, bool isOnHold, out string newPlanType)
		{
			newPlanType = null;
			switch (plan.PlanType)
			{
				case INPlanConstants.PlanM3:
				case INPlanConstants.PlanM4:
					newPlanType = isOnHold ? INPlanConstants.PlanM3 : INPlanConstants.PlanM4;
					break;
				case INPlanConstants.Plan70:
				case INPlanConstants.Plan73:
					newPlanType = isOnHold ? INPlanConstants.Plan73 : INPlanConstants.Plan70;
					break;
				case INPlanConstants.Plan76:
				case INPlanConstants.Plan78:
					newPlanType = isOnHold ? INPlanConstants.Plan78 : INPlanConstants.Plan76;
					break;
				case INPlanConstants.Plan74:
				case INPlanConstants.Plan79:
					newPlanType = isOnHold ? INPlanConstants.Plan79 : INPlanConstants.Plan74;
					break;
				case INPlanConstants.PlanF7:
				case INPlanConstants.PlanF8:
					newPlanType = isOnHold ? INPlanConstants.PlanF8 : INPlanConstants.PlanF7;
					break;
			}
			return newPlanType != null;
		}

		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Parent_RowUpdated(sender, e);

			if (!sender.ObjectsEqual<POOrder.status, POOrder.cancelled>(e.Row, e.OldRow) ||
				//There are two graphs when automation step with notification is getting applied. One of them (the one made by automation) will be 
				//persisted, while another one is only displayed on the screen. Due to cache sharing and because of automatic screen refresh 
				//we will get the same Status in e.Row and e.OldRow while in fact they are different (example - transition from NL Pending Email to NL Open step) 
				(sender.Graph.UnattendedMode && (sender.Graph.AutomationStep != sender.Graph.AutomationStepOriginal)))
			{
				POOrder order = (POOrder)e.Row;
				PXCache plancache = sender.Graph.Caches[typeof(INItemPlan)];
				bool Cancelled = (bool?)sender.GetValue<POOrder.cancelled>(e.Row) == true;

				foreach (INItemPlan plan in PXSelect<INItemPlan, Where<INItemPlan.refNoteID, Equal<Current<POOrder.noteID>>>>.Select(sender.Graph))
				{
					if (Cancelled)
					{
						plancache.Delete(plan);
					}
					else
					{
						INItemPlan copy = PXCache<INItemPlan>.CreateCopy(plan);

						bool IsOnHold = IsOrderOnHold(order);
						string newPlanType;
						if (TryCalcPlanType(plan, IsOnHold, out newPlanType))
						{
							plan.PlanType = newPlanType;
						}

						plan.Hold = IsOnHold;

						if (!string.Equals(copy.PlanType, plan.PlanType))
							plancache.RaiseRowUpdated(plan, copy);

						plancache.MarkUpdated(plan);
					}
				}
			}
		}
		#endregion
	}

	public class POLineRPlanIDAttribute : POLinePlanIDBaseAttribute
	{
		#region Ctor
		public POLineRPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry)
			: base(ParentNoteID, ParentHoldEntry)
		{
		}
		#endregion
		#region Implementation
		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Parent_RowUpdated(sender, e);

			if (e.Row != null && e.OldRow != null && (
				(bool?)sender.GetValue(e.Row, _ParentHoldEntry.Name) != (bool?)sender.GetValue(e.OldRow, _ParentHoldEntry.Name)))
			{
				PXCache cache = sender.Graph.Caches[typeof(POLineUOpen)];
				foreach (POLineUOpen split in PXSelect<POLineUOpen,
					Where<POLineUOpen.orderType, Equal<Required<POOrder.orderType>>,
						And<POLineUOpen.orderNbr, Equal<Required<POOrder.orderNbr>>>>>
					.Select(sender.Graph, ((POOrder)e.Row).OrderType, ((POOrder)e.Row).OrderNbr))
				{
					cache.RaiseRowUpdated(split, PXCache<POLineUOpen>.CreateCopy(split));
				}
			}
		}

		#endregion
	}
}
