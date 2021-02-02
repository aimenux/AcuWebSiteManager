using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace PX.Objects.SO
{
	[PXCacheName(Messages.SOCartShipment, PXDacType.Details)]
	public class SOCartShipment : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<SOCartShipment>.By<siteID, cartID>
		{
			public static SOCartShipment Find(PXGraph graph, int? siteID, int? cartID) => FindBy(graph, siteID, cartID);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<SOCartShipment>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<SOCartShipment>.By<siteID, cartID> { }
			public class Shipment : SOShipment.PK.ForeignKeyOf<SOCartShipment>.By<shipmentNbr> { }
		}
		#endregion

		#region SiteID
		[Site(IsKey = true, Visible = false)]
		[PXParent(typeof(FK.Site))]
		public int? SiteID { get; set; }
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		#endregion
		#region CartID
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(Search<INCart.cartID, Where<INCart.active, Equal<True>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		[PXParent(typeof(FK.Cart))]
		public int? CartID { get; set; }
		public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
		#endregion
		#region ShipmentNbr
		[PXDBString(15, IsUnicode = true)]
		[PXDefault]
		[PXParent(typeof(FK.Shipment))]
		public virtual String ShipmentNbr { get; set; }
		public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
		#endregion
		#region tstamp
		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}