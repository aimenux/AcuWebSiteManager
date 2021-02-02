namespace PX.Objects.IN
{
	using System;
	using PX.Data;
    using PX.Data.ReferentialIntegrity.Attributes;

    [System.SerializableAttribute()]
    [PXHidden]
	public partial class INItemCustSalesStats : PX.Data.IBqlTable
	{
        #region Keys
        public class PK : PrimaryKeyOf<INItemCustSalesStats>.By<inventoryID, subItemID, siteID, bAccountID>
        {
            public static INItemCustSalesStats Find(PXGraph graph, int? inventoryID, int? subItemID, int? siteID, int? bAccountID)
                => FindBy(graph, inventoryID, subItemID, siteID, bAccountID);
        }
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INItemCustSalesStats>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INItemCustSalesStats>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INItemCustSalesStats>.By<siteID> { }
			public class BAccount : CR.BAccount.PK.ForeignKeyOf<INItemCustSalesStats>.By<bAccountID> { }
		}
        #endregion
        #region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? InventoryID
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? SubItemID
		{
			get
			{
				return this._SubItemID;
			}
			set
			{
				this._SubItemID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? SiteID
		{
			get
			{
				return this._SiteID;
			}
			set
			{
				this._SiteID = value;
			}
		}
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion

		#region LastDate
		public abstract class lastDate : PX.Data.BQL.BqlDateTime.Field<lastDate> { }
		protected DateTime? _LastDate;
		[PXDBDate]		
		public virtual DateTime? LastDate
		{
			get
			{
				return this._LastDate;
			}
			set
			{
				this._LastDate = value;
			}
		}
		#endregion		
		#region LastQty
		public abstract class lastQty : PX.Data.BQL.BqlDecimal.Field<lastQty> { }
		protected Decimal? _LastQty;
		[PXDBQuantity]
		public virtual Decimal? LastQty
		{
			get
			{
				return this._LastQty;
			}
			set
			{
				this._LastQty = value;
			}
		}
		#endregion		
		#region LastUnitPrice
		public abstract class lastUnitPrice : PX.Data.BQL.BqlDecimal.Field<lastUnitPrice> { }
		protected Decimal? _LastUnitPrice;
		[PXDBDecimal(6)]
		public virtual Decimal? LastUnitPrice
		{
			get
			{
				return this._LastUnitPrice;
			}
			set
			{
				this._LastUnitPrice = value;
			}
		}
		#endregion

		#region DropShipLastDate
		public abstract class dropShipLastDate : PX.Data.BQL.BqlDateTime.Field<dropShipLastDate> { }
		[PXDBDate]
		public virtual DateTime? DropShipLastDate
		{
			get;
			set;
		}
		#endregion
		#region DropShipLastQty
		public abstract class dropShipLastQty : PX.Data.BQL.BqlDecimal.Field<dropShipLastQty> { }
		[PXDBQuantity]
		public virtual decimal? DropShipLastQty
		{
			get;
			set;
		}
		#endregion
		#region DropShipLastUnitPrice
		public abstract class dropShipLastUnitPrice : PX.Data.BQL.BqlDecimal.Field<dropShipLastUnitPrice> { }
		[PXDBDecimal(6)]
		public virtual decimal? DropShipLastUnitPrice
		{
			get;
			set;
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}
