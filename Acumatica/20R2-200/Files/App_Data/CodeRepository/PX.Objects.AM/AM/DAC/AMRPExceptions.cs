using System;
using PX.Data;
using PX.Objects.IN;
using PX.TM;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
	[PXCacheName(AM.Messages.MRPExceptions)]
    [PXPrimaryGraph(typeof(MRPExcept))]
    public class AMRPExceptions : IBqlTable
	{
        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

        protected int? _RecordID;
        [PXDBInt(IsKey = true)]
        [PXDefault]
        [PXUIField(DisplayName = "Record ID", Enabled = false, Visible = false)]
        public virtual int? RecordID
        {
            get
            {
                return this._RecordID;
            }
            set
            {
                this._RecordID = value;
            }
        }
        #endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		protected int? _InventoryID;
		[Inventory(Enabled=false)]
        [PXDefault]
		public virtual int? InventoryID
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
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMRPExceptions.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<True>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMRPExceptions.inventoryID))]
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
        #region Type
        public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }

        protected int? _Type;
        [PXDBInt]
        [PXDefault]
        [MRPExceptionType.List]
        [PXUIField(DisplayName = "Type", Enabled = false)]
        public virtual int? Type
        {
            get
            {
                return this._Type;
            }
            set
            {
                this._Type = value;
            }
        }
        #endregion
        #region RefType
        public abstract class refType : PX.Data.BQL.BqlInt.Field<refType> { }

        protected int? _RefType;
        [PXDBInt]
        [PXUIField(DisplayName = "Ref Type", Enabled = false)]
        [MRPPlanningType.List]
        public virtual int? RefType
        {
            get
            {
                return this._RefType;
            }
            set
            {
                this._RefType = value;
            }
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Ref Nbr")]
        public virtual String RefNbr { get; set; }
        #endregion
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

	    protected Guid? _RefNoteID;
        [AM.Attributes.RefNoteRefNbr(typeof(refNbr))]
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        public virtual Guid? RefNoteID
	    {
	        get
	        {
	            return this._RefNoteID;
	        }
	        set
	        {
	            this._RefNoteID = value;
	        }
	    }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quantity", Enabled = false)]
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
		#region PromiseDate
        public abstract class promiseDate : PX.Data.BQL.BqlDateTime.Field<promiseDate> { }

        protected DateTime? _PromiseDate;
		[PXDBDate]
		[PXDefault]
        [PXUIField(DisplayName = "Promise Date", Enabled = false)]
        public virtual DateTime? PromiseDate
		{
			get
			{
                return this._PromiseDate;
			}
			set
			{
                this._PromiseDate = value;
			}
		}
		#endregion
        #region RequiredDate
        public abstract class requiredDate : PX.Data.BQL.BqlDateTime.Field<requiredDate> { }

        protected DateTime? _RequiredDate;
        [PXDBDate]
        [PXUIField(DisplayName = "Required Date")]
        public virtual DateTime? RequiredDate
        {
            get
            {
                return this._RequiredDate;
            }
            set
            {
                this._RequiredDate = value;
            }
        }
        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [Site(Enabled = false)]
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
        #region SupplyQty
        public abstract class supplyQty : PX.Data.BQL.BqlDecimal.Field<supplyQty> { }

        protected Decimal? _SupplyQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Supply Qty", Enabled = false)]
        public virtual Decimal? SupplyQty
        {
            get
            {
                return this._SupplyQty;
            }
            set
            {
                this._SupplyQty = value;
            }
        }
        #endregion
        #region SupplySiteID
        public abstract class supplySiteID : PX.Data.BQL.BqlInt.Field<supplySiteID> { }

        protected Int32? _SupplySiteID;
        [Site(DisplayName = "Supply Warehouse")]
        public virtual Int32? SupplySiteID
        {
            get
            {
                return this._SupplySiteID;
            }
            set
            {
                this._SupplySiteID = value;
            }
        }
        #endregion
	    #region ProductManagerID
	    public abstract class productManagerID : PX.Data.BQL.BqlInt.Field<productManagerID> { }

	    protected int? _ProductManagerID;
	    [Owner(DisplayName = "Product Manager ID", Visible = false, Enabled = false)]
	    public virtual int? ProductManagerID
	    {
	        get
	        {
	            return this._ProductManagerID;
	        }
	        set
	        {
	            this._ProductManagerID = value;
	        }
	    }
        #endregion
	    #region tstamp
	    public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

	    protected byte[] _tstamp;
	    [PXDBTimestamp()]
	    public virtual byte[] tstamp
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
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID>
        {
        }
        protected int? _ItemClassID;
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        public virtual int? ItemClassID
        {
            get
            {
                return this._ItemClassID;
            }
            set
            {
                this._ItemClassID = value;
            }
        }
        #endregion
    }
}