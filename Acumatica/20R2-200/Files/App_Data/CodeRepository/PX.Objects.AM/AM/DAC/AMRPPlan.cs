using System;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;
using System.Text;

namespace PX.Objects.AM
{
    /// <summary>
    /// Table grouping all actual and plan orders together to support detail inquiry
    /// </summary>
    [Serializable]
    [PXCacheName("MRP Plan")]
    public class AMRPPlan : IBqlTable
    {
        #region ActionDate
        public abstract class actionDate : PX.Data.BQL.BqlDateTime.Field<actionDate> { }

        protected DateTime? _ActionDate;
        [PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Action Date")]
        public virtual DateTime? ActionDate
        {
            get
            {
                return this._ActionDate;
            }
            set
            {
                this._ActionDate = value;
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
        [SubItem(typeof(AMRPPlan.inventoryID))]
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
        #region RecordID
        public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

        protected int? _RecordID;
        [PXDBInt(IsKey = true)]
        [PXDefault]
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
        #region ParentInventoryID
        public abstract class parentInventoryID : PX.Data.BQL.BqlInt.Field<parentInventoryID> { }

        protected int? _ParentInventoryID;
        [Inventory(DisplayName = "Parent Inventory ID", Visible = false)]
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
        [SubItem(typeof(AMRPPlan.parentInventoryID), DisplayName = "Parent Subitem")]
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
        [Inventory(DisplayName = "Product Inventory ID", Visible = false)]
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

        public abstract class productSubItemID : PX.Data.BQL.BqlInt.Field<productSubItemID> { }

        protected Int32? _ProductSubItemID;
        [SubItem(typeof(AMRPPlan.productInventoryID), DisplayName = "Product Subitem")]
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
        #region PromiseDate
        public abstract class promiseDate : PX.Data.BQL.BqlDateTime.Field<promiseDate> { }

        protected DateTime? _PromiseDate;
        [PXDBDate]
        [PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
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
        #region BaseQty
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }

        protected Decimal? _BaseQty;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty")]
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
        #region RefType
        public abstract class refType : PX.Data.BQL.BqlInt.Field<refType> { }

        protected int? _RefType;
        /// <summary>
        /// Plan type related to the RefOrdertype and RefOrderNbr
        /// </summary>
        [PXDBInt]
        [PXUIField(DisplayName = "Reference Type", Visible = false, Enabled = false)]
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
        #region SDflag
        public abstract class sDflag : PX.Data.BQL.BqlString.Field<sDflag> { }

        protected String _SDflag;
        [PXDefault("")]
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "SD Flag")]
        [MRPSDFlag.List]
        public virtual string SDflag
        {
            get
            {
                return this._SDflag;
            }
            set
            {
                this._SDflag = value;
            }
        }

        #endregion
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected int? _SiteID;
        [Site]
        [PXDefault]
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
        #region Type
        public abstract class type : PX.Data.BQL.BqlInt.Field<type> { }

        protected int? _Type;
        [PXDefault(MRPPlanningType.Unknown)]
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
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [INUnit(typeof(AMRPPlan.inventoryID))]
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
        #region Tstamp
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp> { }

        protected Byte[] _Tstamp;
        [PXDBTimestamp()]
        public virtual Byte[] Tstamp
        {
            get
            {
                return this._Tstamp;
            }
            set
            {
                this._Tstamp = value;
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

        #region Qty On Hand
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }

        protected decimal? _QtyOnHand;
        [PXQuantity]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Qty On Hand")]
        public virtual decimal? QtyOnHand
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
    }
}