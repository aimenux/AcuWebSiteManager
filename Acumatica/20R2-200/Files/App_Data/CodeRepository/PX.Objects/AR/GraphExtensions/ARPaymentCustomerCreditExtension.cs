using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Extensions.CustomerCreditHold;

namespace PX.Objects.AR.GraphExtensions
{
	/// <summary>A mapped generic graph extension that defines the AR payment credit helper functionality.</summary>	
	public class ARPaymentCustomerCreditExtension : CustomerCreditExtension<
			ARPaymentEntry,
			ARPayment,
			ARPayment.customerID,
			ARPayment.hold,
			ARPayment.released,
			ARPayment.status>
	{
		protected virtual void _(Events.RowInserted<ARPayment> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, e.Row, null);
		}

		protected override void _(Events.RowUpdated<ARPayment> e)
		{
			if (e.Row != null && e.OldRow != null)
				UpdateARBalances(e.Cache, e.Row, e.OldRow);

			base._(e);
		}

		protected virtual void _(Events.RowDeleted<ARPayment> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, null, e.Row);
		}

		public override void UpdateARBalances(PXCache cache, ARPayment newRow, ARPayment oldRow)
		{
			if (oldRow != null)
			{
				ARReleaseProcess.UpdateARBalances(cache.Graph, oldRow, -oldRow.OrigDocAmt);
			}

			if (newRow != null)
			{
				ARReleaseProcess.UpdateARBalances(cache.Graph, newRow, newRow.OrigDocAmt);
			}
		}

		protected override ARSetup GetARSetup()
			=> Base.arsetup.Current;
    }
}