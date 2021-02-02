using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName("Master Production Schedule")]
    [PXPrimaryGraph(typeof(MPSMaint))]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMMPS : IBqlTable, INotable
    {
        internal string DebuggerDisplay => $"MPSID = {MPSID}, InventoryID = {InventoryID}, SiteID = {SiteID}, BOMID = {BOMID}, PlanDate = {PlanDate.GetValueOrDefault().ToShortDateString()}";

        #region MPSTypeID (Key)
        public abstract class mPSTypeID : PX.Data.BQL.BqlString.Field<mPSTypeID> { }

        protected String _MPSTypeID;
        [PXDBString(20, IsKey = true, IsUnicode = true, InputMask = ">AAAAAAAAAAAAAAAAAAAA")]
        [PXDefault(typeof(Search<AMRPSetup.defaultMPSTypeID>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [PXUIField(DisplayName = "Type")]
        [PXSelector(typeof(Search<AMMPSType.mPSTypeID>))]
        public virtual String MPSTypeID
        {
            get
            {
                return this._MPSTypeID;
            }
            set
            {
                this._MPSTypeID = value;
            }
        }
        #endregion
        #region MPSID (Key)
        public abstract class mPSID : PX.Data.BQL.BqlString.Field<mPSID> { }

        protected String _MPSID;
        [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "MPS ID", Visibility = PXUIVisibility.SelectorVisible, Visible = false, Enabled = false)]
        [Attributes.MrpNumbering.Mps]
        //[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]         // do not add this as it gets in the way of the auto number
        public virtual String MPSID
        {
            get
            {
                return this._MPSID;
            }
            set
            {
                this._MPSID = value;
            }
        }
        #endregion
        #region ActiveFlg
        public abstract class activeFlg : PX.Data.BQL.BqlBool.Field<activeFlg> { }

        protected Boolean? _ActiveFlg;
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? ActiveFlg
		{
			get
			{
				return this._ActiveFlg;
			}
			set
			{
				this._ActiveFlg = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		protected Int32? _InventoryID;
        [StockItem]
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
            Where<InventoryItem.inventoryID, Equal<Current<AMMPS.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<PX.Objects.CS.boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [SubItem(typeof(AMMPS.inventoryID))]
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
	    [PXDefault(typeof(Search<InventoryItem.dfltSiteID, Where<InventoryItem.inventoryID, Equal<Current<AMMPS.inventoryID>>>>))]
        [PXFormula(typeof(Default<AMMPS.inventoryID>))]
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
	    #region BOMID
	    public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

	    protected String _BOMID;
	    [BomID]
	    [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
	    [BOMIDSelector(typeof(Search2<
	        AMBomItemActive.bOMID,
	        LeftJoin<InventoryItem, 
	            On<AMBomItemActive.inventoryID, Equal<InventoryItem.inventoryID>>>,
	        Where<AMBomItemActive.inventoryID, Equal<Current<AMMPS.inventoryID>>,
	            And<Where<AMBomItemActive.subItemID, Equal<Current<AMMPS.subItemID>>,
	                Or<AMBomItemActive.subItemID, IsNull>>>>>))]
        [PXFormula(typeof(Default<AMMPS.siteID, AMMPS.subItemID>))]
	    public virtual String BOMID
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
        #region PlanDate
        public abstract class planDate : PX.Data.BQL.BqlDateTime.Field<planDate> { }

        protected DateTime? _PlanDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.NullOrBlank)]
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
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity()]
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
        #region Qty
        public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected Decimal? _Qty;
        [PXDBQuantity(typeof(AMMPS.uOM), typeof(AMMPS.baseQty), MinValue = 0)]
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
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<InventoryItem.salesUnit, Where<InventoryItem.inventoryID, Equal<Current<AMMPS.inventoryID>>>>))]
        [PXFormula(typeof(Default<AMMPS.inventoryID>))]
        [INUnit(typeof(AMMPS.inventoryID))]
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
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [AutoNote]
        public virtual Guid? NoteID
        {
            get
            {
                return this._NoteID;
            }
            set
            {
                this._NoteID = value;
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
	}
}
