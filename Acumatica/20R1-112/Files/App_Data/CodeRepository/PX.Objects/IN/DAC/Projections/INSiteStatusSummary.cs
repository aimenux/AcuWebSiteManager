using System;
using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[PXProjection(typeof(Select4<INSiteStatus, Where<boolTrue, Equal<boolTrue>>, Aggregate<GroupBy<INSiteStatus.inventoryID, GroupBy<INSiteStatus.siteID, Sum<INSiteStatus.qtyOnHand, Sum<INSiteStatus.qtyAvail, Sum<INSiteStatus.qtyNotAvail>>>>>>>))]
	[Serializable]
	[PXCacheName(Messages.INSiteStatusSummary)]
	public partial class INSiteStatusSummary : INSiteStatus
	{
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public override Int32? InventoryID
		{
			get
			{
				return this._InventoryID;
			}
			set
			{
				this._InventoryID = value;
			}
		}
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		#endregion
	}
}
