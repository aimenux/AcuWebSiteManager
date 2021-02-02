using PX.Data;
using System;
using PX.Objects.IN;
using PX.Objects.AM.Attributes;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.VendorShipmentLineSplit)]
    public class AMVendorShipLineSplit : IBqlTable, ILSDetail
    {
        #region ShipmentNbr
        public abstract class shipmentNbr : PX.Data.BQL.BqlString.Field<shipmentNbr> { }
        protected String _ShipmentNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = "")]
        [PXDBDefault(typeof(AMVendorShipment.shipmentNbr))]
        [PXParent(typeof(Select<AMVendorShipLine,
            Where<AMVendorShipLine.shipmentNbr, Equal<Current<AMVendorShipLineSplit.shipmentNbr>>, 
                And<AMVendorShipLine.lineNbr, Equal<Current<AMVendorShipLineSplit.lineNbr>>>>>))]
        public virtual String ShipmentNbr
        {
            get
            {
                return this._ShipmentNbr;
            }
            set
            {
                this._ShipmentNbr = value;
            }
        }
        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        protected Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault(typeof(AMVendorShipLine.lineNbr))]
        [PXUIField(DisplayName = "Line Nbr.", Enabled = false, Visible = false)]
        public virtual Int32? LineNbr
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
        #region SplitLineNbr
        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
        protected Int32? _SplitLineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        [PXLineNbr(typeof(AMVendorShipLine.lotSerCntr))]
        public virtual Int32? SplitLineNbr
        {
            get
            {
                return this._SplitLineNbr;
            }
            set
            {
                this._SplitLineNbr = value;
            }
        }
        #endregion
        #region InvtMult
        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }
        protected Int16? _InvtMult;
        [PXDBShort()]
        [PXDefault(typeof(INTran.invtMult))]
        public virtual Int16? InvtMult
        {
            get
            {
                return this._InvtMult;
            }
            set
            {
                this._InvtMult = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [Inventory(Enabled = false, Visible = true)]
        [PXDefault(typeof(AMVendorShipLine.inventoryID))]
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
        #region LineType
        public abstract class lineType : PX.Data.BQL.BqlString.Field<lineType> { }
        protected String _LineType;
        [PXDBString(2, IsFixed = true)]
        [PXDefault(typeof(AMVendorShipLine.lineType))]
        public virtual String LineType
        {
            get
            {
                return this._LineType;
            }
            set
            {
                this._LineType = value;
            }
        }
        #endregion
        #region IsStockItem
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
        [PXDBBool()]
        [PXFormula(typeof(Selector<inventoryID, InventoryItem.stkItem>))]
        public bool? IsStockItem
        {
            get;
            set;
        }
        #endregion
        #region IsComponentItem
        public abstract class isComponentItem : PX.Data.BQL.BqlBool.Field<isComponentItem> { }
        [PXDBBool()]
        [PXFormula(typeof(Switch<Case<Where<inventoryID, Equal<Current<AMVendorShipLine.inventoryID>>>, False>, True>))]
        public bool? IsComponentItem
        {
            get;
            set;
        }
        #endregion
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
        protected String _TranType;
        [PXDBString(3, IsFixed = true)]
        [PXDefault(typeof(AMVendorShipLine.tranType))]
        public virtual String TranType
        {
            get
            {
                return this._TranType;
            }
            set
            {
                this._TranType = value;
            }
        }
        #endregion
        #region TranDate

        public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected DateTime? _TranDate;
        [PXDBDate]
        [PXDBDefault(typeof(AMVendorShipLine.tranDate))]
        public virtual DateTime? TranDate
        {
            get
            {
                return this._TranDate;
            }
            set
            {
                this._TranDate = value;
            }
        }
        #endregion
        #region PlanID
        public abstract class planID : PX.Data.BQL.BqlLong.Field<planID> { }
        protected Int64? _PlanID;
        [PXDBLong(IsImmutable = true)]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        [PX.Objects.IN.Site()]
        [PXDefault(typeof(AMVendorShipLine.siteID))]
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
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        protected Int32? _LocationID;
        [MfgLocationAvail(typeof(inventoryID), typeof(subItemID), typeof(siteID), false, true)]
        [PXDefault()]
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
        #region SubItemID
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        protected Int32? _SubItemID;
        [PX.Objects.IN.SubItem(typeof(inventoryID))]
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
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        protected String _LotSerialNbr;
        [AMLotSerialNbr(typeof(inventoryID), typeof(subItemID),
            typeof(locationID), typeof(AMVendorShipLine.lotSerialNbr), FieldClass = "LotSerial")]
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
        #region LotSerClassID

        public abstract class lotSerClassID : PX.Data.BQL.BqlString.Field<lotSerClassID> { }

        protected String _LotSerClassID;
        [PXString(10, IsUnicode = true)]
        public virtual String LotSerClassID
        {
            get
            {
                return this._LotSerClassID;
            }
            set
            {
                this._LotSerClassID = value;
            }
        }
        #endregion
        #region AssignedNbr

        public abstract class assignedNbr : PX.Data.BQL.BqlString.Field<assignedNbr> { }

        protected String _AssignedNbr;
        [PXString(30, IsUnicode = true)]
        public virtual String AssignedNbr
        {
            get
            {
                return this._AssignedNbr;
            }
            set
            {
                this._AssignedNbr = value;
            }
        }
        #endregion
        #region ExpireDate
        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }
        protected DateTime? _ExpireDate;
        public virtual DateTime? ExpireDate
        {
            get
            {
                return this._ExpireDate;
            }
            set
            {
                this._ExpireDate = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        protected String _UOM;
        [INUnit(typeof(inventoryID), DisplayName = "UOM", Enabled = false)]
        [PXDefault]
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
        [PXDBQuantity(typeof(uOM), typeof(baseQty), InventoryUnitType.BaseUnit)]
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
        [PXDBDecimal(6)]
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
        #region ShipDate
        public abstract class shipDate : PX.Data.BQL.BqlDateTime.Field<shipDate> { }
        protected DateTime? _ShipDate;
        [PXDBDate()]
        [PXDBDefault(typeof(AMVendorShipment.shipmentDate))]
        public virtual DateTime? ShipDate
        {
            get
            {
                return this._ShipDate;
            }
            set
            {
                this._ShipDate = value;
            }
        }
        #endregion
        #region Confirmed
        public abstract class confirmed : PX.Data.BQL.BqlBool.Field<confirmed> { }
        protected Boolean? _Confirmed;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Confirmed")]
        public virtual Boolean? Confirmed
        {
            get
            {
                return this._Confirmed;
            }
            set
            {
                this._Confirmed = value;
            }
        }
        #endregion
        #region Released
        public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
        protected Boolean? _Released;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Released")]
        public virtual Boolean? Released
        {
            get
            {
                return this._Released;
            }
            set
            {
                this._Released = value;
            }
        }
        #endregion
        #region IsUnassigned
        public abstract class isUnassigned : PX.Data.BQL.BqlBool.Field<isUnassigned> { }
        protected Boolean? _IsUnassigned;
        [PXDBBool()]
        [PXDefault(false)]
        public virtual Boolean? IsUnassigned
        {
            get
            {
                return this._IsUnassigned;
            }
            set
            {
                this._IsUnassigned = value;
            }
        }
        #endregion
        #region ProjectID
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
        protected Int32? _ProjectID;
        [PXFormula(typeof(Selector<locationID, INLocation.projectID>))]
        [PXInt]
        public virtual Int32? ProjectID
        {
            get
            {
                return this._ProjectID;
            }
            set
            {
                this._ProjectID = value;
            }
        }
        #endregion
        #region TaskID
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
        protected Int32? _TaskID;
        [PXFormula(typeof(Selector<locationID, INLocation.taskID>))]
        [PXInt]
        public virtual Int32? TaskID
        {
            get
            {
                return this._TaskID;
            }
            set
            {
                this._TaskID = value;
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

        #region Methods

        public static implicit operator INCostSubItemXRef(AMVendorShipLineSplit item)
        {
            INCostSubItemXRef ret = new INCostSubItemXRef();
            ret.SubItemID = item.SubItemID;

            return ret;
        }

        public static implicit operator INLotSerialStatus(AMVendorShipLineSplit item)
        {
            INLotSerialStatus ret = new INLotSerialStatus();
            ret.InventoryID = item.InventoryID;
            ret.SiteID = item.SiteID;
            ret.LocationID = item.LocationID;
            ret.SubItemID = item.SubItemID;
            ret.LotSerialNbr = item.LotSerialNbr;

            return ret;
        }

        public static implicit operator INCostStatus(AMVendorShipLineSplit item)
        {
            INCostStatus ret = new INCostStatus();
            ret.InventoryID = item.InventoryID;
            ret.LayerType = INLayerType.Normal;

            return ret;
        }
        #endregion
    }
}