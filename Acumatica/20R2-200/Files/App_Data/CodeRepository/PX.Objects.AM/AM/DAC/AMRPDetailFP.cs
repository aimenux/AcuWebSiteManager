using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    /// <summary>
    /// MRP first pass detail records rebuilt during MRP regen process
    /// </summary>
	[Serializable]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [PXCacheName(AM.Messages.MRPFirstPassDetail)]
	public class AMRPDetailFP : IBqlTable
	{
        internal string DebuggerDisplay => $"[{RecordID}] InventoryID={InventoryID}, SiteID={SiteID}, Qty={Qty}, SDFlag={SDFlag}, Type={Type}, LowLevel={LowLevel}";

        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

		protected int? _RecordID;
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Record ID")]
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
		#region BOMID
		public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

		protected string _BOMID;
        [BomID]
		public virtual string BOMID
		{
			get
			{
				return this._BOMID;
			}
			set
			{
				this._BOMID = value;
			}
		}
        #endregion
	    #region BOMRevisionID
	    public abstract class bOMRevisionID : PX.Data.BQL.BqlString.Field<bOMRevisionID> { }

	    protected string _BOMRevisionID;
	    [RevisionIDField(DisplayName = "BOM Revision")]
	    public virtual string BOMRevisionID
	    {
	        get
	        {
	            return this._BOMRevisionID;
	        }
	        set
	        {
	            this._BOMRevisionID = value;
	        }
	    }
	    #endregion
        #region OrderQtyConsumed
        public abstract class orderQtyConsumed : PX.Data.BQL.BqlDecimal.Field<orderQtyConsumed> { }

        protected decimal? _OrderQtyConsumed;
        /// <summary>
        /// Contains forecast and MPS qty consumed numbers when the types are dependent
        /// </summary>
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal,"0.0")]
        [PXUIField(DisplayName = "Consumed Fcst Qty")]
		public virtual decimal? OrderQtyConsumed
		{
			get
			{
                return this._OrderQtyConsumed;
			}
			set
			{
                this._OrderQtyConsumed = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		protected int? _InventoryID;
        [Inventory]
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
        [PXDBInt]
        [PXUIField(DisplayName = "Subitem")]
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
		#region LowLevel
		public abstract class lowLevel : PX.Data.BQL.BqlInt.Field<lowLevel> { }

		protected int? _LowLevel;
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Low Level")]
		public virtual int? LowLevel
		{
			get
			{
				return this._LowLevel;
			}
			set
			{
				this._LowLevel = value;
			}
		}
		#endregion
		#region PlanDate
		public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

		protected DateTime? _PlanDate;
		[PXDBDate]
		[PXUIField(DisplayName = "Plan Date")]
		public virtual DateTime? PlanDate
		{
			get
			{
				return this._PlanDate;
			}
			set
			{
				this._PlanDate = value;
			}
		}
		#endregion
        #region OriginalQty
        public abstract class originalQty : PX.Data.BQL.BqlDecimal.Field<originalQty> { }

        protected decimal? _OriginalQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Original Stock Qty")]
        public virtual decimal? OriginalQty
        {
            get
            {
                return this._OriginalQty;
            }
            set
            {
                this._OriginalQty = value;
            }
        }
        #endregion
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

		protected decimal? _Qty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Stock Qty")]
		public virtual decimal? Qty
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
		#region ParentInventoryID
		public abstract class parentInventoryID : PX.Data.BQL.BqlInt.Field<parentInventoryID> { }

		protected int? _ParentInventoryID;
        [Inventory(DisplayName = "Parent Inventory ID")]
		public virtual int? ParentInventoryID
		{
			get
			{
				return this._ParentInventoryID;
			}
			set
			{
				this._ParentInventoryID = value;
			}
		}
		#endregion
        #region ParentSubItemID

        public abstract class parentSubItemID : PX.Data.BQL.BqlInt.Field<parentSubItemID> { }

        protected Int32? _ParentSubItemID;
        [PXDBInt]
        [PXUIField(DisplayName = "Parent Subitem")]
        public virtual Int32? ParentSubItemID
        {
            get
            {
                return this._ParentSubItemID;
            }
            set
            {
                this._ParentSubItemID = value;
            }
        }
        #endregion
        #region ProductInventoryID
		public abstract class productInventoryID : PX.Data.BQL.BqlInt.Field<productInventoryID> { }

		protected int? _ProductInventoryID;
        [Inventory(DisplayName = "Product Inventory ID")]
		public virtual int? ProductInventoryID
		{
			get
			{
				return this._ProductInventoryID;
			}
			set
			{
				this._ProductInventoryID = value;
			}
		}
		#endregion
        #region ProductSubItemID

        public abstract class productsubItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _ProductSubItemID;
        [PXDBInt]
        [PXUIField(DisplayName = "Product Subitem")]
        public virtual Int32? ProductSubItemID
        {
            get
            {
                return this._ProductSubItemID;
            }
            set
            {
                this._ProductSubItemID = value;
            }
        }
        #endregion
        #region Processed
		public abstract class processed : PX.Data.BQL.BqlBool.Field<processed> { }

		protected bool? _Processed;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Processed")]
		public virtual bool? Processed
		{
			get
			{
				return this._Processed;
			}
			set
			{
				this._Processed = value;
			}
		}
		#endregion
        #region OnHoldStatus
        public abstract class onHoldStatus : PX.Data.BQL.BqlInt.Field<onHoldStatus> { }

        protected int? _OnHoldStatus;
        [PXDBInt]
        [PXDefault(Attributes.OnHoldStatus.NotOnHold)]
        [PXUIField(DisplayName = "On hold status")]
        public virtual int? OnHoldStatus
        {
            get
            {
                return this._OnHoldStatus;
            }
            set
            {
                this._OnHoldStatus = value;
            }
        }
        #endregion
        #region RefType
        public abstract class refType : PX.Data.BQL.BqlInt.Field<refType> { }

        protected int? _RefType;
        [PXDBInt]
        [PXUIField(DisplayName = "Ref Type")]
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
		#region SDFlag
		public abstract class sDFlag : PX.Data.BQL.BqlString.Field<sDFlag> { }

		protected string _SDFlag;
		[PXDBString(1, IsUnicode = true, IsFixed = true)]
		[PXUIField(DisplayName = "SD Flag")]
        [MRPSDFlag.List]
		public virtual string SDFlag
		{
			get
			{
				return this._SDFlag;
			}
			set
			{
				this._SDFlag = value;
			}
		}
		#endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		protected int? _SiteID;
        [Site]
		public virtual int? SiteID
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
		#region SuppliedQty
		public abstract class suppliedQty : PX.Data.BQL.BqlDecimal.Field<suppliedQty> { }

		protected decimal? _SuppliedQty;
		[PXDBQuantity]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Supplied Qty")]
		public virtual decimal? SuppliedQty
		{
			get
			{
				return this._SuppliedQty;
			}
			set
			{
				this._SuppliedQty = value;
			}
		}
		#endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }

        protected int? _Type;
        [PXDefault]
        [PXDBInt]
        [PXUIField(DisplayName = "Type")]
        [MRPPlanningType.List]
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
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected byte[] _tstamp;
		[PXDBTimestamp]
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
	    #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }

	    protected Int64? _PlanID;
	    [PXDBLong]
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
	    #region PlanType
        public abstract class planType : PX.Data.BQL.BqlString.Field<planType> { }

	    protected String _PlanType;
	    [PXDBString(2, IsFixed = true)]
	    [PXUIField(DisplayName = "Plan Type")]
	    public virtual String PlanType
	    {
	        get
	        {
	            return this._PlanType;
	        }
	        set
	        {
	            this._PlanType = value;
	        }
	    }
	    #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

	    protected Int32? _VendorID;
	    [PXDBInt]
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
	    [PXDBInt]
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
	    #region SupplyPlanID
        public abstract class supplyPlanID : PX.Data.BQL.BqlLong.Field<supplyPlanID> { }

	    protected Int64? _SupplyPlanID;
	    [PXDBLong]
	    public virtual Int64? SupplyPlanID
	    {
	        get
	        {
	            return this._SupplyPlanID;
	        }
	        set
	        {
	            this._SupplyPlanID = value;
	        }
	    }
	    #endregion
	    #region DemandPlanID
        public abstract class demandPlanID : PX.Data.BQL.BqlLong.Field<demandPlanID> { }

	    protected Int64? _DemandPlanID;
	    [PXDBLong]
	    public virtual Int64? DemandPlanID
	    {
	        get
	        {
	            return this._DemandPlanID;
	        }
	        set
	        {
	            this._DemandPlanID = value;
	        }
	    }
        #endregion
	    #region BAccountID
        public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }

	    protected Int32? _BAccountID;
	    [PXDBInt]
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
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Ref Nbr")]
        public virtual String RefNbr { get; set; }
        #endregion
        #region ParentRefNbr
        public abstract class parentRefNbr : PX.Data.BQL.BqlString.Field<parentRefNbr> { }

        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Parent Ref Nbr")]
        public virtual String ParentRefNbr { get; set; }
        #endregion
        #region ProductRefNbr
        public abstract class productRefNbr : PX.Data.BQL.BqlString.Field<productRefNbr> { }

        [PXDBString(30, IsUnicode = true)]
        [PXUIField(DisplayName = "Product Ref Nbr")]
        public virtual String ProductRefNbr { get; set; }
        #endregion
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

	    protected Guid? _RefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [RefNoteRefNbr(typeof(refNbr))]
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
        #region ParentRefNoteID
        public abstract class parentRefNoteID : PX.Data.BQL.BqlGuid.Field<parentRefNoteID> { }

	    protected Guid? _ParentRefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [RefNoteRefNbr(typeof(parentRefNbr))]
        public virtual Guid? ParentRefNoteID
        {
	        get
	        {
	            return this._ParentRefNoteID;
	        }
	        set
	        {
	            this._ParentRefNoteID = value;
	        }
	    }
        #endregion
	    #region ProductRefNoteID
	    public abstract class productRefNoteID : PX.Data.BQL.BqlGuid.Field<productRefNoteID> { }

	    protected Guid? _ProductRefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [RefNoteRefNbr(typeof(productRefNbr))]
        public virtual Guid? ProductRefNoteID
        {
	        get
	        {
	            return this._ProductRefNoteID;
	        }
	        set
	        {
	            this._ProductRefNoteID = value;
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

    [PXHidden]
    [Serializable]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [PXProjection(typeof(Select<AMRPDetailFP, Where<AMRPDetailFP.sDFlag, Equal<MRPSDFlag.supply>>>), Persistent = true)]
    public class AMRPDetailFPSupply : IBqlTable
    {
        internal string DebuggerDisplay => $"[{RecordID}] InventoryID={InventoryID}, SiteID={SiteID}, Qty={Qty}, Type={Type}, LowLevel={LowLevel}";

        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

        protected int? _RecordID;
        [PXDBInt(IsKey = true, BqlField = typeof(AMRPDetailFP.recordID))]
        [PXDefault]
        [PXUIField(DisplayName = "Record ID")]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected int? _SiteID;
        [PXDBInt(BqlField = typeof(AMRPDetailFP.siteID))]
        public virtual int? SiteID
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected int? _InventoryID;
        [PXDBInt(BqlField = typeof(AMRPDetailFP.inventoryID))]
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
        [PXDBInt(BqlField = typeof(AMRPDetailFP.subItemID))]
        [PXUIField(DisplayName = "Subitem")]
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
        #region LowLevel
        public abstract class lowLevel : PX.Data.BQL.BqlInt.Field<lowLevel> { }

        protected int? _LowLevel;
        [PXDBInt(BqlField = typeof(AMRPDetailFP.lowLevel))]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Low Level")]
        public virtual int? LowLevel
        {
            get
            {
                return this._LowLevel;
            }
            set
            {
                this._LowLevel = value;
            }
        }
        #endregion
        #region RequiredDate
        public abstract class requiredDate : PX.Data.BQL.BqlDateTime.Field<requiredDate> { }

        protected DateTime? _RequiredDate;
        [PXDBDate(BqlField = typeof(AMRPDetailFP.requiredDate))]
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
        #region PlanDate
        public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

        protected DateTime? _PlanDate;
        [PXDBDate(BqlField = typeof(AMRPDetailFP.planDate))]
        [PXUIField(DisplayName = "Plan Date")]
        public virtual DateTime? PlanDate
        {
            get
            {
                return this._PlanDate;
            }
            set
            {
                this._PlanDate = value;
            }
        }
        #endregion
        #region OriginalQty
        public abstract class originalQty : PX.Data.BQL.BqlDecimal.Field<originalQty> { }

        protected decimal? _OriginalQty;
        [PXDBQuantity(BqlField = typeof(AMRPDetailFP.originalQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Original Stock Qty")]
        public virtual decimal? OriginalQty
        {
            get
            {
                return this._OriginalQty;
            }
            set
            {
                this._OriginalQty = value;
            }
        }
        #endregion
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected decimal? _Qty;
        [PXDBQuantity(BqlField = typeof(AMRPDetailFP.qty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Stock Qty")]
        public virtual decimal? Qty
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
        #region SuppliedQty
        public abstract class suppliedQty : PX.Data.BQL.BqlDecimal.Field<suppliedQty> { }

        protected decimal? _SuppliedQty;
        [PXDBQuantity(BqlField = typeof(AMRPDetailFP.suppliedQty))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Supplied Qty")]
        public virtual decimal? SuppliedQty
        {
            get
            {
                return this._SuppliedQty;
            }
            set
            {
                this._SuppliedQty = value;
            }
        }
        #endregion
        #region Type
        public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }

        protected int? _Type;
        [PXDefault]
        [PXDBInt(BqlField = typeof(AMRPDetailFP.type))]
        [PXUIField(DisplayName = "Type")]
        [MRPPlanningType.List]
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
        #region RefNoteID
        public abstract class refNoteID : PX.Data.BQL.BqlGuid.Field<refNoteID> { }

        protected Guid? _RefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [PXRefNote(BqlField = typeof(AMRPDetailFP.refNoteID))]
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
        #region ParentRefNoteID
        public abstract class parentRefNoteID : PX.Data.BQL.BqlGuid.Field<parentRefNoteID> { }

        protected Guid? _ParentRefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [PXRefNote(BqlField = typeof(AMRPDetailFP.parentRefNoteID))]
        public virtual Guid? ParentRefNoteID
        {
            get
            {
                return this._ParentRefNoteID;
            }
            set
            {
                this._ParentRefNoteID = value;
            }
        }
        #endregion
        #region ProductRefNoteID
        public abstract class productRefNoteID : PX.Data.BQL.BqlGuid.Field<productRefNoteID> { }

        protected Guid? _ProductRefNoteID;
        [PXUIField(DisplayName = "Related Document", Enabled = false)]
        [PXRefNote(BqlField = typeof(AMRPDetailFP.productRefNoteID))]
        public virtual Guid? ProductRefNoteID
        {
            get
            {
                return this._ProductRefNoteID;
            }
            set
            {
                this._ProductRefNoteID = value;
            }
        }
        #endregion
    }

}