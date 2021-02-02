using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;

namespace PX.Objects.IN
{
    [System.SerializableAttribute()]
    [PXCacheName(Messages.INReplenishmentLine)]
    public partial class INReplenishmentLine : IBqlTable
    {
        #region Keys
        public class PK : PrimaryKeyOf<INReplenishmentLine>.By<refNbr, lineNbr>
        {
            public static INReplenishmentLine Find(PXGraph graph, string refNbr, int? lineNbr) => FindBy(graph, refNbr, lineNbr);
        }
		public static class FK
		{
			public class ReplenishmentOrder : INReplenishmentOrder.PK.ForeignKeyOf<INReplenishmentLine>.By<refNbr> { }
			public class InventoryItem : IN.InventoryItem.PK.ForeignKeyOf<INReplenishmentLine>.By<inventoryID> { }
			public class SubItem : INSubItem.PK.ForeignKeyOf<INReplenishmentLine>.By<subItemID> { }
			public class Site : INSite.PK.ForeignKeyOf<INReplenishmentLine>.By<siteID> { }
			public class Vendor : AP.Vendor.PK.ForeignKeyOf<INReplenishmentLine>.By<vendorID> { }
			public class DestinationSite : INSite.PK.ForeignKeyOf<INReplenishmentLine>.By<destinationSiteID> { }
			public class ItemPlan : INItemPlan.PK.ForeignKeyOf<INReplenishmentLine>.By<planID> { }
		}
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		protected String _RefNbr;
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDBDefault(typeof(INReplenishmentOrder.refNbr))]
		[PXParent(typeof(FK.ReplenishmentOrder))]
		public virtual String RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		protected int? _LineNbr;
		[PXDBInt(IsKey = true)]
		[PXDefault()]
		[PXLineNbr(typeof(INReplenishmentOrder.lineCntr))]
		public virtual int? LineNbr
		{
			get
			{
				return this._LineNbr;
			}
			set
			{
				this._LineNbr = value;
			}
		}
		#endregion		
		#region OrderDate
		public abstract class orderDate : PX.Data.BQL.BqlDateTime.Field<orderDate> { }
		protected DateTime? _OrderDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Order Date")]
		[PXDefault(typeof(INReplenishmentOrder.orderDate))]
		public virtual DateTime? OrderDate
		{
			get
			{
				return this._OrderDate;
			}
			set
			{
				this._OrderDate = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[StockItem()]
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
		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
		protected Int32? _SubItemID;
		[IN.SubItem(typeof(INReplenishmentLine.inventoryID))]
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
		[IN.SiteAvail(typeof(INReplenishmentLine.inventoryID), typeof(INReplenishmentLine.subItemID))]
		[PXForeignReference(typeof(FK.Site))]
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
		#region DestinationSiteID
		public abstract class destinationSiteID : PX.Data.BQL.BqlInt.Field<destinationSiteID> { }
		protected Int32? _DestinationSiteID;
		[IN.SiteAvail(typeof(INReplenishmentLine.inventoryID), typeof(INReplenishmentLine.subItemID))]
		[PXForeignReference(typeof(FK.DestinationSite))]
		public virtual Int32? DestinationSiteID
		{
			get
			{
				return this._DestinationSiteID;
			}
			set
			{
				this._DestinationSiteID = value;
			}
		}
		#endregion
		#region UOM
		public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
		protected String _UOM;
		[PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<INReplenishmentLine.inventoryID>>>>))]		
		[INUnit(typeof(INReplenishmentLine.inventoryID))]
		public virtual String UOM
		{
			get
			{
				return this._UOM;
			}
			set
			{
				this._UOM = value;
			}
		}
		#endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity(typeof(INReplenishmentLine.uOM), typeof(INReplenishmentLine.baseQty))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Quantity")]		
		public virtual Decimal? Qty
		{
			get
			{
				return this._Qty;
			}
			set
			{
				this._Qty = value;
			}
		}
		#endregion
		#region BaseQty
		public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
		protected Decimal? _BaseQty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? BaseQty
		{
			get
			{
				return this._BaseQty;
			}
			set
			{
				this._BaseQty = value;
			}
		}
		#endregion
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor()]		
		public virtual Int32? VendorID
		{
			get
			{
				return this._VendorID;
			}
			set
			{
				this._VendorID = value;
			}
		}
		#endregion
		#region VendorLocationID
		public abstract class vendorLocationID : PX.Data.BQL.BqlInt.Field<vendorLocationID> { }
		protected Int32? _VendorLocationID;
		[PXDBInt()]
		public virtual Int32? VendorLocationID
		{
			get
			{
				return this._VendorLocationID;
			}
			set
			{
				this._VendorLocationID = value;
			}
		}
		#endregion
		#region PlanID
		public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
		protected Int64? _PlanID;
		[PXDBLong()]
		public virtual Int64? PlanID
		{
			get
			{
				return this._PlanID;
			}
			set
			{
				this._PlanID = value;
			}
		}
		#endregion		
		#region POType
		public abstract class pOType : PX.Data.BQL.BqlString.Field<pOType> { }
		protected String _POType;
		[PXDBString(2, IsFixed = true)]		
		[PXUIField(DisplayName = "PO  Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String POType
		{
			get
			{
				return this._POType;
			}
			set
			{
				this._POType = value;
			}
		}
		#endregion
		#region PONbr
		public abstract class pONbr : PX.Data.BQL.BqlString.Field<pONbr> { }
		protected String _PONbr;

		[PXDBString(15, InputMask = "", IsUnicode = true)]				
		[PXUIField(DisplayName = "PO  Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual String PONbr
		{
			get
			{
				return this._PONbr;
			}
			set
			{
				this._PONbr = value;
			}
		}
		#endregion
		#region POLineNbr
		public abstract class pOLineNbr : PX.Data.BQL.BqlInt.Field<pOLineNbr> { }
		protected Int32? _POLineNbr;
		[PXDBInt()]
		[PXUIField(DisplayName = "PO Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]		
		public virtual Int32? POLineNbr
		{
			get
			{
				return this._POLineNbr;
			}
			set
			{
				this._POLineNbr = value;
			}
		}
		#endregion
		#region SOType
		public abstract class sOType : PX.Data.BQL.BqlString.Field<sOType> { }
		protected String _SOType;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "SO Type", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual String SOType
		{
			get
			{
				return this._SOType;
			}
			set
			{
				this._SOType = value;
			}
		}
		#endregion
		#region SONbr
		public abstract class sONbr : PX.Data.BQL.BqlString.Field<sONbr> { }
		protected String _SONbr;

		[PXDBString(15, InputMask = "", IsUnicode = true)]
		[PXUIField(DisplayName = "SO Nbr.", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual String SONbr
		{
			get
			{
				return this._SONbr;
			}
			set
			{
				this._SONbr = value;
			}
		}
		#endregion
		#region SOLineNbr
		public abstract class sOLineNbr : PX.Data.BQL.BqlInt.Field<sOLineNbr> { }
		protected int? _SOLineNbr;
		[PXDBInt()]
		[PXUIField(DisplayName = "SO Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual int? SOLineNbr
		{
			get
			{
				return this._SOLineNbr;
			}
			set
			{
				this._SOLineNbr = value;
			}
		}
		#endregion
        #region SOSplitLineNbr
        public abstract class sOSplitLineNbr : PX.Data.BQL.BqlInt.Field<sOSplitLineNbr> { }
        protected int? _SOSplitLineNbr;
        [PXDBInt()]
        [PXUIField(DisplayName = "SO Split Nbr.", Visibility = PXUIVisibility.Visible, Visible = false)]
        public virtual int? SOSplitLineNbr
        {
            get
            {
                return this._SOSplitLineNbr;
            }
            set
            {
                this._SOSplitLineNbr = value;
            }
        }
        #endregion
		
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
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
}
