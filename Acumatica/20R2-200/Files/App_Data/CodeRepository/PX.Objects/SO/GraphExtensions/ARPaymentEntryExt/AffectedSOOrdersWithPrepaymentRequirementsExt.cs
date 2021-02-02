using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.Objects.Extensions;

namespace PX.Objects.SO.GraphExtensions.ARPaymentEntryExt
{
	public class AffectedSOOrdersWithPrepaymentRequirementsExt : ProcessAffectedEntitiesInPrimaryGraphBase<AffectedSOOrdersWithPrepaymentRequirementsExt, ARPaymentEntry, SOOrder, SOOrderEntry>
	{
		private PXCache<SOOrder> orders => Base.Caches<SOOrder>();
		protected override bool PersistInSameTransaction => false;

		protected override bool EntityIsAffected(SOOrder order)
		{
			return
				!Equals(orders.GetValueOriginal<SOOrder.curyPrepaymentReqAmt>(order), order.CuryPrepaymentReqAmt) ||
				!Equals(orders.GetValueOriginal<SOOrder.curyPaymentOverall>(order), order.CuryPaymentOverall);
		}

		protected override void ProcessAffectedEntity(SOOrderEntry orderEntry, SOOrder order)
		{
			if (order.CuryPaymentOverall >= order.CuryPrepaymentReqAmt)
				order.SatisfyPrepaymentRequirements(orderEntry);
			else
				order.ViolatePrepaymentRequirements(orderEntry);
		}

		protected override SOOrder ActualizeEntity(SOOrderEntry orderEntry, SOOrder order) => SOOrder.PK.Find(orderEntry, order);
	}
}