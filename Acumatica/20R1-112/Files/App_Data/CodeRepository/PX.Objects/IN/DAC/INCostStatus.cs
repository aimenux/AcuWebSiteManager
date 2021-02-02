using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using PX.Objects.CS;

namespace PX.Objects.IN
{
	[Serializable]
	[PXCacheName(Messages.INCostStatus)]
	public partial class INCostStatus : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<INCostStatus>.By<inventoryID, costSubItemID, costSiteID, accountID, subID, layerType, costID>
		{
			public static INCostStatus Find(PXGraph graph, int? inventoryID, int? costSubItemID, int? costSiteID, int? accountID, int? subID, string layerType, long? costID)
				=> FindBy(graph, inventoryID, costSubItemID, costSiteID, accountID, subID, layerType, costID);
		}
		public static class FK
		{
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INCostStatus>.By<inventoryID> { }
			public class CostSubItem : INSubItem.PK.ForeignKeyOf<INCostStatus>.By<costSubItemID> { }
			public class CostSite : INSite.PK.ForeignKeyOf<INCostStatus>.By<costSiteID> { }
			public class CostItemSite : INItemSite.PK.ForeignKeyOf<INCostStatus>.By<inventoryID, costSiteID> { }
			public class Sub : GL.Sub.PK.ForeignKeyOf<INCostStatus>.By<subID> { }
		}
        #endregion
        #region CostID
		public abstract class costID : PX.Data.BQL.BqlLong.Field<costID> { }
		protected Int64? _CostID;
		[PXDBLongIdentity(IsKey = true)]
		[PXDefault()]
		public virtual Int64? CostID
		{
			get
			{
				return this._CostID;
			}
			set
			{
				this._CostID = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem()]
		[PXDefault()]
		[PXForeignReference(typeof(FK.InventoryItem))]
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
		#region CostSubItemID
		public abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
		protected Int32? _CostSubItemID;
		[SubItem()]
		[PXDefault()]
		public virtual Int32? CostSubItemID
		{
			get
			{
				return this._CostSubItemID;
			}
			set
			{
				this._CostSubItemID = value;
			}
		}
		#endregion
		#region CostSiteID
		public abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
		protected Int32? _CostSiteID;
		[PXDBInt()]
		[PXDefault()]
		[PXUIField(DisplayName = "Cost Site")]
		public virtual Int32? CostSiteID
		{
			get
			{
				return this._CostSiteID;
			}
			set
			{
				this._CostSiteID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account()]
		[PXDefault()]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		[SubAccount()]
		[PXDefault()]
		public virtual Int32? SubID
		{
			get
			{
				return this._SubID;
			}
			set
			{
				this._SubID = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		[Site(true)]
		[PXDefault(typeof(Coalesce<
			Search<INSite.siteID, Where<INSite.siteID, Equal<Current<costSiteID>>>>,
			Search<INLocation.siteID, Where<INLocation.locationID, Equal<Current<costSiteID>>>>
		>))]
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
		#region LayerType
		public abstract class layerType : PX.Data.BQL.BqlString.Field<layerType> { }
		protected String _LayerType;
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public virtual String LayerType
		{
			get
			{
				return this._LayerType;
			}
			set
			{
				this._LayerType = value;
			}
		}
		#endregion
		#region ValMethod
		public abstract class valMethod : PX.Data.BQL.BqlString.Field<valMethod> { }
		protected String _ValMethod;
		[PXDBString(1, IsFixed = true)]
		[PXDefault()]
		public virtual String ValMethod
		{
			get
			{
				return this._ValMethod;
			}
			set
			{
				this._ValMethod = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXDefault()]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
        #endregion
        #region ReceiptDate
        public abstract class receiptDate : PX.Data.BQL.BqlDateTime.Field<receiptDate> { }
		protected DateTime? _ReceiptDate;
		[PXDBDate()]
		[PXDefault()]
		public virtual DateTime? ReceiptDate
		{
			get
			{
				return this._ReceiptDate;
			}
			set
			{
				this._ReceiptDate = value;
			}
		}
		#endregion
		#region LotSerialNbr
		public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		protected String _LotSerialNbr;
		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Lot/Serial Number")]
		public virtual String LotSerialNbr
		{
			get
			{
				return this._LotSerialNbr;
			}
			set
			{
				this._LotSerialNbr = value;
			}
		}
		#endregion
		#region OrigQty
		public abstract class origQty : PX.Data.BQL.BqlDecimal.Field<origQty> { }
		protected Decimal? _OrigQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? OrigQty
		{
			get
			{
				return this._OrigQty;
			}
			set
			{
				this._OrigQty = value;
			}
		}
		#endregion
		#region QtyOnHand
		public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		protected Decimal? _QtyOnHand;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty. On Hand")]
		public virtual Decimal? QtyOnHand
		{
			get
			{
				return this._QtyOnHand;
			}
			set
			{
				this._QtyOnHand = value;
			}
		}
		#endregion
		#region OrigQtyOnHand
		public abstract class origQtyOnHand : Data.BQL.BqlDecimal.Field<origQtyOnHand> { }
		/// <summary>
		/// The unbound field is used only during inventory documents release for storing the original on hand qty before processing of oversolds.
		/// </summary>
		[PXQuantity]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual decimal? OrigQtyOnHand
		{
			get;
			set;
		}
		#endregion
		#region UnitCost
		public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
		protected Decimal? _UnitCost;
		[PXDBDecimal(6)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		public virtual Decimal? UnitCost
		{
			get
			{
				return this._UnitCost;
			}
			set
			{
				this._UnitCost = value;
			}
		}
		#endregion
		#region TotalCost
		public abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		protected Decimal? _TotalCost;
		[PXDBDecimal(4)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Total Cost")]
		public virtual Decimal? TotalCost
		{
			get
			{
				return this._TotalCost;
			}
			set
			{
				this._TotalCost = value;
			}
		}
		#endregion
		#region AvgCost
		protected Decimal? _AvgCost;
		public virtual Decimal? AvgCost
		{
			get
			{
				return this._AvgCost;
			}
			set
			{
				this._AvgCost = value;
			}
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

	public class INLayerType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Normal, Messages.Normal),
					Pair(Oversold, Messages.Oversold),
					Pair(Unmanaged, Messages.Unmanaged),
				}) {}
		}

		public const string Normal = "N";
		public const string Oversold = "O";
        public const string Unmanaged = "U";

        public class normal : PX.Data.BQL.BqlString.Constant<normal>
		{
			public normal() : base(Normal) { ;}
		}

		public class oversold : PX.Data.BQL.BqlString.Constant<oversold>
		{
			public oversold() : base(Oversold) { ;}
		}

        public class unmanaged : PX.Data.BQL.BqlString.Constant<unmanaged>
		{
            public unmanaged() : base(Unmanaged) { }
        }
	}

	public class INLayerRef
	{
		public const string ZZZ = "ZZZ";
		public const string Oversold = "OVERSOLD";

		public class zzz : PX.Data.BQL.BqlString.Constant<zzz>
		{
			public zzz() : base(ZZZ) { ;}
		}

		public class oversold : PX.Data.BQL.BqlString.Constant<oversold>
		{
			public oversold() : base(Oversold) { ;}
		}
	}


    [PXProjection(typeof(Select4<INCostStatus, Where<costSiteID, Equal<SiteAttribute.transitSiteID>>, Aggregate<
        GroupBy<INCostStatus.inventoryID,
        GroupBy<INCostStatus.costSiteID,
        Sum<INCostStatus.qtyOnHand,
        Sum<INCostStatus.totalCost>>>>>>))]
    [Serializable]
    public partial class INCostStatusTransitLineSummary : INCostStatus
    {

        #region InventoryID
        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        #endregion
        #region CostSiteID
        public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
		#endregion
		#region SiteID
	    public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
	    [Site(true, BqlField = typeof(INCostStatus.siteID))]
	    [PXDefault]
	    public override Int32? SiteID
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
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
        #endregion
        #region TotalCost
        public new abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
        #endregion
    }

	[PXCacheName(Messages.INCostStatusSummary)]
	[PXProjection(typeof(Select4<INCostStatus, Where<boolTrue, Equal<boolTrue>>, Aggregate<
		GroupBy<INCostStatus.accountID, 
		GroupBy<INCostStatus.subID, 
		GroupBy<INCostStatus.inventoryID, 
		GroupBy<INCostStatus.costSubItemID, 
		GroupBy<INCostStatus.costSiteID, 
		GroupBy<INCostStatus.lotSerialNbr, 
		Sum<INCostStatus.qtyOnHand, 
		Sum<INCostStatus.totalCost>>>>>>>>>>))]
    [Serializable]
	public partial class INCostStatusSummary : INCostStatus
	{
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		#endregion
		#region InventoryID
		public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		#endregion
		#region CostSubItemID
		public new abstract class costSubItemID : PX.Data.BQL.BqlInt.Field<costSubItemID> { }
		#endregion
		#region CostSiteID
		public new abstract class costSiteID : PX.Data.BQL.BqlInt.Field<costSiteID> { }
		#endregion
		#region SiteID
		public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		[Site(true, BqlField = typeof(INCostStatus.siteID))]
		[PXDefault]
		public override Int32? SiteID
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
		#region LotSerialNbr
		public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
		#endregion
		#region QtyOnHand
		public new abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }
		#endregion
		#region TotalCost
		public new abstract class totalCost : PX.Data.BQL.BqlDecimal.Field<totalCost> { }
		#endregion
	}

}
