using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.AM
{
	[Serializable]
    [PXCacheName("Material Work Temp")]
	public class AMWrkMatl : IBqlTable, IProdOper
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMWrkMatl>.By<autoNbr>
        {
            public static AMWrkMatl Find(PXGraph graph, int autoNbr)
                => FindBy(graph, autoNbr);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMWrkMatl>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMWrkMatl>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMWrkMatl>.By<orderType, prodOrdID, operationID> { }
        }
        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

        protected bool? _Selected = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                _Selected = value;
            }
        }
        #endregion
		#region AutoNbr
		public abstract class autoNbr : PX.Data.BQL.BqlInt.Field<autoNbr> { }

		protected Int32? _AutoNbr;
		[PXDBIdentity(IsKey = true)]
        [PXUIField(Enabled = false)]
		public virtual Int32? AutoNbr
		{
			get
			{
				return this._AutoNbr;
			}
			set
			{
				this._AutoNbr = value;
			}
		}
		#endregion
		#region BFlush
		public abstract class bFlush : PX.Data.BQL.BqlBool.Field<bFlush> { }

		protected Boolean? _BFlush;
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "BFlush")]
		public virtual Boolean? BFlush
		{
			get
			{
				return this._BFlush;
			}
			set
			{
				this._BFlush = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected String _Descr;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Descr")]
		public virtual String Descr
		{
			get
			{
				return this._Descr;
			}
			set
			{
				this._Descr = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		protected Int32? _InventoryID;
		[Inventory(DisplayName = "Inventory ID")]
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
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMWrkMatl.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<PX.Objects.CS.boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMWrkMatl.inventoryID))]
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
		#region LineID
		public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

		protected Int32? _LineID;
		[PXDBInt]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Line ID")]
		public virtual Int32? LineID
		{
			get
			{
				return this._LineID;
			}
			set
			{
				this._LineID = value;
			}
		}
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

		protected Int32? _LocationID;
        [Location(ValidComboRequired = false, Visible = false)]
		public virtual Int32? LocationID
		{
			get
			{
				return this._LocationID;
			}
			set
			{
				this._LocationID = value;
			}
		}
		#endregion
		#region MatlQty
		public abstract class matlQty : PX.Data.BQL.BqlDecimal.Field<matlQty> { }

		protected Decimal? _MatlQty;
        /// <summary>
        /// Material batch release qty to be processed in a material batch.
        /// </summary>
        [PXDBQuantity(typeof(AMWrkMatl.uOM), typeof(AMWrkMatl.baseMatlQty), HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Release Qty")]
		public virtual Decimal? MatlQty
		{
			get
			{
				return this._MatlQty;
			}
			set
			{
				this._MatlQty = value;
			}
		}
		#endregion
        #region BaseMatlQty
        public abstract class baseMatlQty : PX.Data.BQL.BqlDecimal.Field<baseMatlQty> { }

        protected Decimal? _BaseMatlQty;
        /// <summary>
        /// Material batch release BASE qty to be processed in a material batch.
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Release Base Qty")]
        public virtual Decimal? BaseMatlQty
        {
            get
            {
                return this._BaseMatlQty;
            }
            set
            {
                this._BaseMatlQty = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField]
        [AMOrderTypeSelector(ValidateValue = false)]
        public virtual String OrderType
        {
            get
            {
                return this._OrderType;
            }
            set
            {
                this._OrderType = value;
            }
        }
        #endregion
        #region ProdOrdID
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

		protected String _ProdOrdID;
        [ProductionNbr]
        [PXSelector(typeof(Search<AMProdItem.prodOrdID, Where<AMProdItem.orderType, Equal<Current<AMWrkMatl.orderType>>>>), ValidateValue = false)]
        public virtual String ProdOrdID
		{
			get
			{
				return this._ProdOrdID;
			}
			set
			{
				this._ProdOrdID = value;
			}
		}
        #endregion
        #region OperationID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        protected int? _OperationID;
        [OperationIDField]
        [PXSelector(typeof(Search<AMProdOper.operationID,
                Where<AMProdOper.orderType, Equal<Current<AMWrkMatl.orderType>>,
                    And<AMProdOper.prodOrdID, Equal<Current<AMWrkMatl.prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD), ValidateValue = false)]
        public virtual int? OperationID
        {
            get
            {
                return this._OperationID;
            }
            set
            {
                this._OperationID = value;
            }
        }
        #endregion
        #region QtyAvail
        public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }

		protected Decimal? _QtyAvail;
        [PXDBQuantity(typeof(AMWrkMatl.uOM), typeof(AMWrkMatl.baseQtyAvail), HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Available Qty")]
		public virtual Decimal? QtyAvail
		{
			get
			{
				return this._QtyAvail;
			}
			set
			{
				this._QtyAvail = value;
			}
		}
		#endregion
        #region BaseQtyAvail
        public abstract class baseQtyAvail : PX.Data.BQL.BqlDecimal.Field<baseQtyAvail> { }

        protected Decimal? _BaseQtyAvail;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Available Qty")]
        public virtual Decimal? BaseQtyAvail
        {
            get
            {
                return this._BaseQtyAvail;
            }
            set
            {
                this._BaseQtyAvail = value;
            }
        }
        #endregion
		#region QtyReq
		public abstract class qtyReq : PX.Data.BQL.BqlDecimal.Field<qtyReq> { }

		protected Decimal? _QtyReq;
        /// <summary>
        /// Total required quantity for the material line
        /// </summary>
        [PXDBQuantity(typeof(AMWrkMatl.uOM), typeof(AMWrkMatl.baseQtyReq), HandleEmptyKey = true)]
		[PXDefault(TypeCode.Decimal,"0.0")]
		[PXUIField(DisplayName = "Required Qty")]
		public virtual Decimal? QtyReq
		{
			get
			{
				return this._QtyReq;
			}
			set
			{
				this._QtyReq = value;
			}
		}
		#endregion
        #region BaseQtyReq
        public abstract class baseQtyReq : PX.Data.BQL.BqlDecimal.Field<baseQtyReq> { }

        protected Decimal? _BaseQtyReq;
        /// <summary>
        /// Total required BASE quantity for the material line
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Required Base Qty")]
        public virtual Decimal? BaseQtyReq
        {
            get
            {
                return this._BaseQtyReq;
            }
            set
            {
                this._BaseQtyReq = value;
            }
        }
        #endregion
		#region SiteID
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

		protected Int32? _SiteID;
		[PXDefault]
        [Site]
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
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<AMWrkMatl.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [INUnit(typeof(AMWrkMatl.inventoryID))]
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
		#region UserID
		public abstract class userID : PX.Data.BQL.BqlGuid.Field<userID> { }

		protected Guid? _UserID;
        [PXDBCreatedByID]
		public virtual Guid? UserID
		{
			get
			{
				return this._UserID;
			}
			set
			{
				this._UserID = value;
			}
		}
		#endregion
        #region IsByproduct
        public abstract class isByproduct : PX.Data.BQL.BqlBool.Field<isByproduct> { }

        protected Boolean? _IsByproduct;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "By-product",Enabled = false)]
        public virtual Boolean? IsByproduct
        {
            get
            {
                return this._IsByproduct;
            }
            set
            {
                this._IsByproduct = value;
            }
        }
        #endregion
	    #region UnreleasedBatchQty
	    public abstract class unreleasedBatchQty : PX.Data.BQL.BqlDecimal.Field<unreleasedBatchQty> { }

	    protected Decimal? _UnreleasedBatchQty;
        /// <summary>
        /// Total Unreleased Batch quantity for the material line
        /// </summary>
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
	    [PXUIField(DisplayName = "Unreleased Batch Qty", Enabled = false, Visible = false)]
	    public virtual Decimal? UnreleasedBatchQty
	    {
	        get
	        {
	            return this._UnreleasedBatchQty;
	        }
	        set
	        {
	            this._UnreleasedBatchQty = value;
	        }
	    }
	    #endregion
	    #region BaseUnreleasedBatchQty
	    public abstract class baseUnreleasedBatchQty : PX.Data.BQL.BqlDecimal.Field<baseUnreleasedBatchQty> { }

	    protected Decimal? _BaseUnreleasedBatchQty;
	    /// <summary>
	    /// Total Unreleased Batch BASE quantity for the material line
	    /// </summary>
	    [PXDBQuantity]
	    [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Unreleased Batch Base Qty", Enabled = false, Visible = false)]
	    public virtual Decimal? BaseUnreleasedBatchQty
	    {
	        get
	        {
	            return this._BaseUnreleasedBatchQty;
	        }
	        set
	        {
	            this._BaseUnreleasedBatchQty = value;
	        }
	    }
        #endregion
        #region QtyRemaining
        public abstract class qtyRemaining : PX.Data.BQL.BqlDecimal.Field<qtyRemaining> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty Remaining", Enabled = false)]
        public virtual Decimal? QtyRemaining { get; set; }
        #endregion
        #region BaseQtyRemaining
        public abstract class baseQtyRemaining : PX.Data.BQL.BqlDecimal.Field<baseQtyRemaining> { }

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty Remaining", Enabled = false, Visible = false)]
        public virtual Decimal? BaseQtyRemaining { get; set; }
        #endregion
        #region OverIssueMaterial
        /// <summary>
        /// Check for over issued material
        /// </summary>
        public abstract class overIssueMaterial : PX.Data.BQL.BqlBool.Field<overIssueMaterial> { }

        /// <summary>
        /// Check for over issued material 
        /// </summary>
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Over Issue Material", Enabled = false, Visible = false)]
        [SetupMessage.List]
        public virtual String OverIssueMaterial { get; set; }
        #endregion
        #region tstamp
        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }

		protected Byte[] _tstamp;
		[PXDBTimestamp]
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
        #region SubcontractSource
        public abstract class subcontractSource : PX.Data.BQL.BqlInt.Field<subcontractSource> { }

        protected int? _SubcontractSource;
        [PXDBInt]
        [PXUIField(DisplayName = "Subcontract Source", Enabled = false)]
        [AMSubcontractSource.List]
        public virtual int? SubcontractSource
        {
            get
            {
                return this._SubcontractSource;
            }
            set
            {
                this._SubcontractSource = value;
            }
        }
        #endregion
    }
}