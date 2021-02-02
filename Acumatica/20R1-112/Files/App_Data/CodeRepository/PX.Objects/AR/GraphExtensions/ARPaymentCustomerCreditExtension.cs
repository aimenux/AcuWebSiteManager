using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.Extensions.CustomerCreditHold;

namespace PX.Objects.AR.GraphExtensions
{
	/// <summary>A mapped generic graph extension that defines the AR payment credit helper functionality.</summary>	
	public class ARPaymentCustomerCreditExtension : CustomerCreditExtension<ARPaymentEntry>
	{
		#region Mapping	

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(ARPayment))
			{
				CustomerID = typeof(ARPayment.customerID),
				Hold = typeof(ARPayment.hold),
				Released = typeof(ARPayment.released),
				Status = typeof(ARPayment.status)
			};
		}

		protected virtual void _(Events.RowInserted<Document> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, e.Row, null);
		}

		protected override void _(Events.RowUpdated<Document> e)
		{
			if (e.Row != null && e.OldRow != null)
				UpdateARBalances(e.Cache, e.Row, e.OldRow);

			base._(e);
		}

		protected virtual void _(Events.RowDeleted<Document> e)
		{
			if (e.Row == null) return;

			UpdateARBalances(e.Cache, null, e.Row);
		}

		public override void UpdateARBalances(PXCache cache, object newRow, object oldRow)
		{
			if (oldRow != null)
			{
				ARRegister oldARRow = Document.Cache.GetMain((Document)oldRow) as ARRegister;
				ARReleaseProcess.UpdateARBalances(cache.Graph, (ARRegister)oldARRow, -((ARRegister)oldARRow).OrigDocAmt);
			}

			if (newRow != null)
			{
				ARRegister newARRow = Document.Cache.GetMain((Document)newRow) as ARRegister;
				ARReleaseProcess.UpdateARBalances(cache.Graph, (ARRegister)newARRow, ((ARRegister)newARRow).OrigDocAmt);
			}
		}
		#endregion
	}
}