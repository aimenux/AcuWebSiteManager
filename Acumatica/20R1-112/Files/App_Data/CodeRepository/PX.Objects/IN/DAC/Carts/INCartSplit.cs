using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.IN
{
	[PXCacheName(Messages.INCartSplit, PXDacType.Balance)]
	public class INCartSplit : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INCartSplit>.By<siteID, cartID, splitLineNbr>
		{
			public static INCartSplit Find(PXGraph graph, int? siteID, int? cartID, int? splitLineNbr) => FindBy(graph, siteID, cartID, splitLineNbr);
		}
		public static class FK
		{
			public class Site : INSite.PK.ForeignKeyOf<INCartSplit>.By<siteID> { }
			public class Cart : INCart.PK.ForeignKeyOf<INCartSplit>.By<siteID, cartID> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INCartSplit>.By<inventoryID> { }
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
		[PXUIField(DisplayName = "Cart ID")]
		[PXSelector(typeof(Search<INCart.cartID, Where<INCart.active, Equal<True>>>), SubstituteKey = typeof(INCart.cartCD), DescriptionField = typeof(INCart.descr))]
		[PXParent(typeof(FK.Cart))]
		public int? CartID { get; set; }
		public abstract class cartID : PX.Data.BQL.BqlInt.Field<cartID> { }
		#endregion
		#region SplitLineNbr
		[PXDBInt(IsKey = true)]
		[PXLineNbr(typeof(INCart))]
		[PXUIField(DisplayName = "Split Line Number")]
		public virtual Int32? SplitLineNbr { get; set; }
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		#endregion
		#region InventoryID
		[StockItem]
		[PXDefault]
		[PXForeignReference(typeof(FK.InventoryItem))]
		public virtual Int32? InventoryID { get; set; }
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region SubItemID
		[SubItem(typeof(inventoryID))]
		[PXDefault]
		[PXFormula(typeof(Default<inventoryID>))]
		public virtual Int32? SubItemID { get; set; }
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		#endregion
		#region LotSerialNbr
		[LotSerialNbrAttribute]
		public virtual String LotSerialNbr { get; set; }
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region ExpireDate
		[PXDBDate(InputMask = "d", DisplayMask = "d")]
		[PXUIField(DisplayName = "Expiration Date", FieldClass = "LotSerial")]
		public virtual DateTime? ExpireDate { get; set; }
		public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
		#endregion
		#region UOM
		[INUnit(typeof(inventoryID), DisplayName = "UOM")]
		[PXDefault]
		public virtual String UOM { get; set; }
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		#endregion
		#region Qty
		[PXDBQuantity(typeof(uOM), typeof(baseQty), MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]
		public virtual Decimal? Qty { get; set; }
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		#endregion
		#region BaseQty
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseQty { get; set; }
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		#endregion
		#region FromLocationID
		[IN.Location(typeof(siteID))]
		[PXDefault]
		public virtual Int32? FromLocationID { get; set; }
		public abstract class fromLocationID : PX.Data.BQL.BqlInt.Field<fromLocationID> { }
		#endregion
		#region ToLocationID
		[IN.Location(typeof(siteID))]
		public virtual Int32? ToLocationID { get; set; }
		public abstract class toLocationID : PX.Data.BQL.BqlInt.Field<toLocationID> { }
		#endregion

		#region CreatedByID
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion
		#region CreatedByScreenID
		[PXDBCreatedByScreenID]
		public virtual String CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion
		#region CreatedDateTime
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion
		#region LastModifiedByID
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion
		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID]
		public virtual String LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion
		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#region tstamp
		[PXDBTimestamp]
		public virtual Byte[] tstamp { get; set; }
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		#endregion
	}
}