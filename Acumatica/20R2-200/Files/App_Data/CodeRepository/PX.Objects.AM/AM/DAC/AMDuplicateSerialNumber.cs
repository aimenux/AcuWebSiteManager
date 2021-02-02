using System;
using System.Diagnostics;
using PX.Objects.AM.Attributes;
using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM
{
    /// <summary>
    /// PXProjection - List transaction split serial tracked items
    /// </summary>
    [PXHidden]
    [Serializable]
    [DebuggerDisplay("[{DocType};{BatNbr};{LineNbr};{SplitLineNbr}] InventoryID={InventoryCD}; LotSerialNbr={LotSerialNbr}; InvtMult={InvtMult}; LotSerAssign={LotSerAssign}")]
    [PXProjection(typeof(Select2<
        AMMTranSplit,
        InnerJoin<InventoryItem, 
            On<InventoryItem.inventoryID, Equal<AMMTranSplit.inventoryID>>,
        InnerJoin<INLotSerClass, 
            On<INLotSerClass.lotSerClassID, Equal<InventoryItem.lotSerClassID>>,
        InnerJoin<AMMTran, 
            On<AMMTran.docType, Equal<AMMTranSplit.docType>,
            And<AMMTran.batNbr, Equal<AMMTranSplit.batNbr>,
            And<AMMTran.lineNbr, Equal<AMMTranSplit.lineNbr>>>>,
        LeftJoin<INLotSerialStatus, 
            On<INLotSerialStatus.inventoryID, Equal<AMMTranSplit.inventoryID>,
            And<INLotSerialStatus.subItemID, Equal<AMMTranSplit.subItemID>,
            And<INLotSerialStatus.siteID, Equal<AMMTranSplit.siteID>,
            And<INLotSerialStatus.locationID, Equal<AMMTranSplit.locationID>,
            And<INLotSerialStatus.lotSerialNbr, Equal<AMMTranSplit.lotSerialNbr>>>>>>>>>>,
        Where<INLotSerClass.lotSerTrack, Equal<INLotSerTrack.serialNumbered>>>), Persistent = false)]
    public class AMDuplicateSerialNumber : IBqlTable
    {
        #region DocType (KEY)
        /// <summary>
        /// AMMTranSplit.docType - IsKey
        /// </summary>
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected String _DocType;
        /// <summary>
        /// AMMTranSplit.DocType - IsKey
        /// </summary>
        [PXDBString(1, IsFixed = true, IsKey = true, BqlField = typeof(AMMTranSplit.docType))]
        public virtual String DocType
        {
            get
            {
                return this._DocType;
            }
            set
            {
                this._DocType = value;
            }
        }
        #endregion
        #region BatNbr (KEY)
        /// <summary>
        /// AMMTranSplit.batNbr - IsKey
        /// </summary>
        public abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected String _BatNbr;
        /// <summary>
        /// AMMTranSplit.BatNbr - IsKey
        /// </summary>
        [PXDBString(15, IsUnicode = true, IsKey = true, BqlField = typeof(AMMTranSplit.batNbr))]
        public virtual String BatNbr
        {
            get
            {
                return this._BatNbr;
            }
            set
            {
                this._BatNbr = value;
            }
        }
        #endregion
        #region LineNbr (KEY)
        /// <summary>
        /// AMMTranSplit.lineNbr - IsKey
        /// </summary>
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected Int32? _LineNbr;
        /// <summary>
        /// AMMTranSplit.LineNbr - IsKey
        /// </summary>
        [PXDBInt(IsKey = true, BqlField = typeof(AMMTranSplit.lineNbr))]
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
        #region SplitLineNbr (KEY)
        /// <summary>
        /// AMMTranSplit.splitLineNbr - IsKey
        /// </summary>
        public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }

        protected Int32? _SplitLineNbr;
        /// <summary>
        /// AMMTranSplit.SplitLineNbr - IsKey
        /// </summary>
        [PXDBInt(IsKey = true, BqlField = typeof(AMMTranSplit.splitLineNbr))]
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
        /// <summary>
        /// AMMTran.invtMult
        /// </summary>
        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected Int16? _InvtMult;
        /// <summary>
        /// AMMTran.InvtMult
        /// </summary>
        [PXDBShort(BqlField = typeof(AMMTran.invtMult))]
        [PXUIField(DisplayName = "Multiplier")]
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
        #region OrderType
        /// <summary>
        /// AMMTran.orderType
        /// </summary>
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        /// <summary>
        /// AMMTran.OrderType
        /// </summary>
        [AMOrderTypeField(BqlField = typeof(AMMTran.orderType))]
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
        /// <summary>
        /// AMMTran.prodOrdID
        /// </summary>
        public abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected String _ProdOrdID;
        /// <summary>
        /// AMMTran.ProdOrdID
        /// </summary>
        [ProductionNbr(BqlField = typeof(AMMTran.prodOrdID))]
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
        #region InventoryID
        /// <summary>
        /// AMMTranSplit.inventoryID
        /// </summary>
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        /// <summary>
        /// AMMTranSplit.InventoryID
        /// </summary>
        [Inventory(BqlField = typeof(AMMTranSplit.inventoryID))]
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
        #region InventoryCD
        /// <summary>
        /// InventoryItem.inventoryCD
        /// </summary>
        public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }

        protected String _InventoryCD;
        /// <summary>
        /// InventoryItem.InventoryCD
        /// </summary>
        [InventoryRaw(BqlField = typeof(InventoryItem.inventoryCD))]
        public virtual String InventoryCD
        {
            get

            {
                return this._InventoryCD;
            }
            set
            {
                this._InventoryCD = value;
            }
        }
        #endregion
        #region SubItemID
        /// <summary>
        /// AMMTranSplit.subItemID
        /// </summary>
        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        /// <summary>
        /// AMMTranSplit.SubItemID
        /// </summary>
        [PXDBInt(BqlField = typeof(AMMTranSplit.subItemID))]
        [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM", Visibility = PXUIVisibility.Visible)]
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
        /// <summary>
        /// AMMTranSplit.siteID
        /// </summary>
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        /// <summary>
        /// AMMTranSplit.SiteID
        /// </summary>
        [PXDBInt(BqlField = typeof(AMMTranSplit.siteID))]
        [PXUIField(DisplayName = "Warehouse", FieldClass = "INSITE", Visibility = PXUIVisibility.Visible)]
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
        /// <summary>
        /// AMMTranSplit.locationID
        /// </summary>
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationID;
        /// <summary>
        /// AMMTranSplit.LocationID
        /// </summary>
        [PXDBInt(BqlField = typeof(AMMTranSplit.locationID))]
        [PXUIField(DisplayName = "Location", FieldClass = "INLOCATION", Visibility = PXUIVisibility.Visible)]
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
        #region LotSerialNbr
        /// <summary>
        /// AMMTranSplit.lotSerialNbr
        /// </summary>
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        /// <summary>
        /// AMMTranSplit.LotSerialNbr
        /// </summary>
        [PXDBString(100, InputMask = "", IsUnicode = true, BqlField = typeof(AMMTranSplit.lotSerialNbr))]
        [PXUIField(DisplayName = "Lot/Serial Nbr.", FieldClass = "LotSerial")]
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
        #region LastOper
        /// <summary>
        /// AMMTran.lastOper
        /// </summary>
        public abstract class lastOper : PX.Data.BQL.BqlBool.Field<lastOper> { }

        protected Boolean? _LastOper;
        /// <summary>
        /// AMMTran.LastOper
        /// </summary>
        [PXDBBool(BqlField = typeof(AMMTran.lastOper)
)]
        public virtual Boolean? LastOper
        {
            get
            {
                return this._LastOper;
            }
            set
            {
                this._LastOper = value;
            }
        }
        #endregion
        #region TranQty
        /// <summary>
        /// AMMTran.qty
        /// </summary>
        public abstract class tranQty : PX.Data.BQL.BqlDecimal.Field<tranQty> { }

        protected Decimal? _TranQty;
        /// <summary>
        /// AMMTran.Qty
        /// </summary>
        [PXDBQuantity(BqlField = typeof(AMMTran.qty))]
        public virtual Decimal? TranQty
        {
            get
            {
                return this._TranQty;
            }
            set
            {
                this._TranQty = value;
            }
        }
        #endregion
        #region QtyOnHand
        /// <summary>
        /// INLotSerialStatus.qtyOnHand
        /// </summary>
        public abstract class qtyOnHand : PX.Data.BQL.BqlDecimal.Field<qtyOnHand> { }

        protected Decimal? _QtyOnHand;
        /// <summary>
        /// INLotSerialStatus.QtyOnHand
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus.qtyOnHand))]
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
        #region QtyAvail
        /// <summary>
        /// INLotSerialStatus.qtyAvail
        /// </summary>
        public abstract class qtyAvail : PX.Data.BQL.BqlDecimal.Field<qtyAvail> { }

        protected Decimal? _QtyAvail;
        /// <summary>
        /// INLotSerialStatus.QtyAvail
        /// </summary>
        [PXDBQuantity(BqlField = typeof(INLotSerialStatus.qtyAvail))]
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
        #region LotSerTrack
        /// <summary>
        /// INLotSerClass.lotSerTrack
        /// </summary>
        public abstract class lotSerTrack : PX.Data.BQL.BqlString.Field<lotSerTrack> { }

        protected String _LotSerTrack;
        /// <summary>
        /// INLotSerClass.LotSerTrack
        /// </summary>
        [PXDBString(1, IsFixed = true, BqlField = typeof(INLotSerClass.lotSerTrack))]
        [INLotSerTrack.List]
        public virtual String LotSerTrack
        {
            get
            {
                return this._LotSerTrack;
            }
            set
            {
                this._LotSerTrack = value;
            }
        }
        #endregion
        #region LotSerAssign

        /// <summary>
        /// INLotSerClass.lotSerAssign
        /// </summary>
        public abstract class lotSerAssign : PX.Data.BQL.BqlString.Field<lotSerAssign> { }

        protected String _LotSerAssign;
        /// <summary>
        /// INLotSerClass.LotSerAssign
        /// </summary>
        [PXDBString(1, IsFixed = true, BqlField = typeof(INLotSerClass.lotSerAssign))]
        [INLotSerAssign.List]
        public virtual String LotSerAssign
        {
            get
            {
                return this._LotSerAssign;
            }
            set
            {
                this._LotSerAssign = value;
            }
        }
        #endregion
    }
}
