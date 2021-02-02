using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	public class ARTranPlanIDAttribute : INItemPlanIDAttribute
	{
		#region Ctor

		public ARTranPlanIDAttribute(Type ParentNoteID, Type ParentHoldEntry)
			: base(ParentNoteID, ParentHoldEntry)
		{
		}

		#endregion

		#region Implementation

		public override void Parent_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			base.Parent_RowUpdated(sender, e);

			if (!sender.ObjectsEqual<ARRegister.docDate, ARRegister.hold, ARInvoice.creditHold>(e.Row, e.OldRow))
			{
				var tranCache = sender.Graph.Caches<ARTran>();
				foreach (ARTran tran in PXSelect<ARTran,
					Where<ARTran.tranType, Equal<Current<ARRegister.docType>>,
						And<ARTran.refNbr, Equal<Current<ARRegister.refNbr>>,
						And<ARTran.lineType, Equal<SOLineType.inventory>,
						And<ARTran.sOOrderNbr, IsNull, And<ARTran.sOShipmentNbr, IsNull>>>>>>
					.Select(sender.Graph))
				{
					RaiseRowUpdated(tranCache, tran, tran);
				}
			}
		}

		public override void Parent_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert)
			{
				_KeyToAbort = sender.GetValue<ARRegister.refNbr>(e.Row);
			}
		}

		public override void Parent_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.Operation == PXDBOperation.Insert && e.TranStatus == PXTranStatus.Open && _KeyToAbort != null)
			{
				foreach (ARTran tran in PXSelect<ARTran, Where<ARTran.tranType, Equal<Required<ARTran.tranType>>,
					And<ARTran.refNbr, Equal<Required<ARTran.refNbr>>>>>
					.Select(sender.Graph, sender.GetValue<ARRegister.docType>(e.Row), _KeyToAbort))
				{
					foreach (INItemPlan plan in sender.Graph.Caches[typeof(INItemPlan)].Inserted)
					{
						if (plan.PlanID == tran.PlanID)
						{
							plan.RefNoteID = (Guid?)sender.GetValue(e.Row, _ParentNoteID.Name);
						}
					}
				}
			}
			_KeyToAbort = null;
		}

		public override INItemPlan DefaultValues(PXCache sender, INItemPlan plan_Row, object origRow)
		{
			ARTran tran = (ARTran)origRow;
			if (tran.Released == true || tran.SOShipmentNbr != null || tran.SOOrderNbr != null || tran.LineType != SOLineType.Inventory)
			{
				return null;
			}
			PXCache cache = sender.Graph.Caches[BqlCommand.GetItemType(_ParentNoteID)];
			bool? hold = (bool?)cache.GetValue(cache.Current, _ParentHoldEntry.Name) | (bool?)cache.GetValue<ARInvoice.creditHold>(cache.Current);

			plan_Row.BAccountID = tran.CustomerID;
			plan_Row.PlanType = (hold == true) ? INPlanConstants.Plan69 : INPlanConstants.Plan62;
			plan_Row.InventoryID = tran.InventoryID;
			plan_Row.SubItemID = tran.SubItemID;
			plan_Row.SiteID = tran.SiteID;
			plan_Row.LocationID = tran.LocationID;
			plan_Row.LotSerialNbr = tran.LotSerialNbr;
			plan_Row.Reverse = (tran.InvtMult > 0) ^ (tran.BaseQty < 0m);
			plan_Row.PlanDate = (DateTime?)cache.GetValue<ARRegister.docDate>(cache.Current);
			plan_Row.PlanQty = Math.Abs(tran.BaseQty ?? 0m);
			plan_Row.RefNoteID = (Guid?)cache.GetValue(cache.Current, _ParentNoteID.Name);
			plan_Row.Hold = hold;

			if (plan_Row.RefNoteID == Guid.Empty)
			{
				plan_Row.RefNoteID = null;
			}

			return plan_Row;
		}

		#endregion
	}
}
