using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.IN
{
	[PXHidden]
	public class INReplenishmentMaint : PXGraph<INReplenishmentMaint,INReplenishmentOrder>
	{
		public PXSelect<INReplenishmentOrder> Document;
		public PXSelect<POLine> planRelease;
		public PXSelect<INItemPlan,
			Where<INItemPlan.refNoteID, Equal<INReplenishmentOrder.noteID>>> Plans;
		public PXSetup<INSetup> setup;

		//This declaration restores legacy behavior that existed before ItemPlanAttribute was removed from the model
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[POLinePlanID(typeof(POOrder.noteID), typeof(POOrder.hold))]
		protected virtual void POLine_PlanID_CacheAttached(PXCache sender)
		{
		}

		protected virtual void INReplenishmentOrder_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			PXNoteAttribute.GetNoteID<INReplenishmentOrder.noteID>(sender, e.Row);
		}

		protected virtual void INItemPlan_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
            if ((e.Operation & PXDBOperation.Command) == PXDBOperation.Delete) return;
            INItemPlan plan = (INItemPlan)e.Row;
			if (plan != null)
			{
				PXCache parent = this.Document.Cache;
				plan.RefNoteID = PXNoteAttribute.GetNoteID<INReplenishmentOrder.noteID>(parent, parent.Current);
				plan.RefEntityType = parent.GetItemType()?.FullName;
			}
		}
	}
}
