using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[PXProjection(typeof(Select4<INLocationStatus, Where<boolTrue, Equal<boolTrue>>, Aggregate<GroupBy<INLocationStatus.inventoryID, GroupBy<INLocationStatus.siteID, GroupBy<INLocationStatus.locationID, Sum<INLocationStatus.qtyOnHand, Sum<INLocationStatus.qtyAvail>>>>>>>))]
	[Serializable]
	[PXHidden]
	public partial class INLocationStatusSummary : INLocationStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region LocationID
		public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		#endregion
	}
}
