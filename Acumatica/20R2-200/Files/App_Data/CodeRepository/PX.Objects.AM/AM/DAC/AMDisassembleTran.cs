using System;
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    [Serializable]
    [PXPrimaryGraph(typeof(DisassemblyEntry))]
    [PXCacheName(Messages.AMDisassembleTran)]
    [PXProjection(typeof(Select<AMMTran,
        Where<AMMTran.docType, Equal<AMDocType.disassembly>>>),
        Persistent = true)]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMDisassembleTran : AMMTran
    {
        #region BranchID
        public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

        protected new Int32? _BranchID;
        [PXDBInt]
        [PXDefault(typeof(AMDisassembleBatch.branchID))]
        public override Int32? BranchID
        {
            get
            {
                return this._BranchID;
            }
            set
            {
                this._BranchID = value;
            }
        }
        #endregion
        #region DocType
        public new abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }

        protected new String _DocType;
        [PXDBString(1, IsKey = true, IsFixed = true)]
        [PXDBDefault(typeof(AMDisassembleBatch.docType))]
        public override String DocType
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
        #region BatNbr
        public new abstract class batNbr : PX.Data.BQL.BqlString.Field<batNbr> { }

        protected new String _BatNbr;
        [PXDBString(15, IsKey = true, IsUnicode = true)]
        [PXDBDefault(typeof(AMDisassembleBatch.batchNbr))]
        [PXParent(typeof(Select<AMDisassembleBatch, Where<AMDisassembleBatch.docType, Equal<Current<AMDisassembleTran.docType>>,
            And<AMDisassembleBatch.batchNbr, Equal<Current<AMDisassembleTran.batNbr>>>>>))]
        public override String BatNbr
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
        #region LineNbr
        public new abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        protected new Int32? _LineNbr;
        [PXDBInt(IsKey = true)]
        [PXDefault()]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMDisassembleBatch.lineCntr))]
        public override Int32? LineNbr
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
        #region TranType
        public new abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected new String _TranType;
        [PXDBString(3, IsFixed = true)]
        [PXDefault(typeof(AMTranType.receipt))]
        [AMTranType.DisassembleTranList]
        [PXUIField(DisplayName = "Tran. Type")]
        public override String TranType
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
        #region OrderType
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected new String _OrderType;
        [PXDBDefault(typeof(AMDisassembleBatch.orderType))]
        [AMOrderTypeField]
        [AMOrderTypeSelector]
        public override String OrderType
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
        public new abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        protected new String _ProdOrdID;
        [ProductionNbr]
        [PXDBDefault(typeof(AMDisassembleBatch.prodOrdID))]
        [ProductionOrderSelector(typeof(AMDisassembleTran.orderType))]
        public override String ProdOrdID
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
        public new abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        [OperationIDField]
        [PXDefault]
        [PXSelector(typeof(Search<
            AMProdOper.operationID, 
            Where<AMProdOper.orderType, Equal<Current<AMDisassembleTran.orderType>>,
                And<AMProdOper.prodOrdID, Equal<Current<AMDisassembleTran.prodOrdID>>>>>),
            SubstituteKey = typeof(AMProdOper.operationCD))]
        [PXFormula(typeof(Validate<AMDisassembleTran.prodOrdID>))]
        public override int? OperationID
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
        #region InventoryID

        public new abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        #endregion
        #region SubItemID
        public new abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected new Int32? _SubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMDisassembleTran.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMDisassembleTran.inventoryID),
            typeof(LeftJoin<INSiteStatus,
                On<INSiteStatus.subItemID, Equal<INSubItem.subItemID>,
                And<INSiteStatus.inventoryID, Equal<Optional<AMDisassembleTran.inventoryID>>,
                And<INSiteStatus.siteID, Equal<Optional<AMDisassembleTran.siteID>>>>>>))]
        [PXFormula(typeof(Default<AMDisassembleTran.inventoryID>))]
        public override Int32? SubItemID
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
        public new abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected new Int32? _SiteID;
        [PXDefault(typeof(AMDisassembleBatch.siteID))]
        [SiteAvail(typeof(AMDisassembleTran.inventoryID), typeof(AMDisassembleTran.subItemID))]
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
        #region LocationID
        public new abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected new Int32? _LocationID;
        [MfgLocationAvail(typeof(AMDisassembleTran.inventoryID), typeof(AMDisassembleTran.subItemID), typeof(AMDisassembleTran.siteID),
            false, true, typeof(AMDisassembleTran.isScrap))]
        public override Int32? LocationID
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
        #region TranDate
        public new abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }

        protected new DateTime? _TranDate;
        [PXDBDate]
        [PXDBDefault(typeof(AMDisassembleBatch.tranDate))]
        public override DateTime? TranDate
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
        #region Qty
        public new abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }

        protected new Decimal? _Qty;
        [PXDBQuantity(typeof(AMDisassembleTran.uOM), typeof(AMDisassembleTran.baseQty), HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Quantity")]
        public override Decimal? Qty
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
        #region QtyScrapped
        public new abstract class qtyScrapped : PX.Data.BQL.BqlDecimal.Field<qtyScrapped> { }

        protected new Decimal? _QtyScrapped;
        [PXDBQuantity(typeof(AMDisassembleTran.uOM), typeof(AMDisassembleTran.baseQtyScrapped), HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty Scrapped")]
        public override Decimal? QtyScrapped
        {
            get
            {
                return this._QtyScrapped;
            }
            set
            {
                this._QtyScrapped = value;
            }
        }
        #endregion
        #region UOM
        public new abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected new String _UOM;
        [PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<AMDisassembleTran.inventoryID>>>>))]
        [INUnit(typeof(AMDisassembleTran.inventoryID))]
        [PXFormula(typeof(Default<AMDisassembleTran.inventoryID>))]
        public override String UOM
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
        #region UnitCost
        public new abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        protected new Decimal? _UnitCost;
        [PXDBPriceCost]
        [PXUIField(DisplayName = "Unit Cost")]
        [MatlUnitCostDefault(
            typeof(AMDisassembleTran.inventoryID),
            typeof(AMDisassembleTran.siteID),
            typeof(AMDisassembleTran.uOM),
            typeof(AMDisassembleBatch),
            typeof(AMDisassembleBatch.siteID))]
        [PXFormula(typeof(Default<AMDisassembleTran.inventoryID, AMDisassembleTran.siteID, AMDisassembleTran.uOM>))]
        public override Decimal? UnitCost
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
        #region TranAmt
        public new abstract class tranAmt : PX.Data.BQL.BqlDecimal.Field<tranAmt> { }

        protected new Decimal? _TranAmt;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Ext. Cost", Enabled = false)]
        [PXFormula(typeof(Mult<qty, unitCost>))]
        public override Decimal? TranAmt

        {
            get
            {
                return this._TranAmt;
            }
            set
            {
                this._TranAmt = value;
            }
        }
        #endregion
        #region AcctID
        public new abstract class acctID : PX.Data.BQL.BqlInt.Field<acctID> { }

        protected new Int32? _AcctID;
        [Account]
        [PXDefault(typeof(Search<ReasonCode.accountID, Where<ReasonCode.reasonCodeID, 
            Equal<Current<AMDisassembleTran.reasonCodeID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMDisassembleTran.reasonCodeID>))]
        public override Int32? AcctID
        {
            get
            {
                return this._AcctID;
            }
            set
            {
                this._AcctID = value;
            }
        }
        #endregion
        #region SubID
        public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }

        protected new Int32? _SubID;
        [SubAccount(typeof(AMDisassembleTran.acctID))]
        [PXFormula(typeof(Default<AMDisassembleTran.reasonCodeID>))]
        public override Int32? SubID
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
        #region LotSerialNbr
        public new abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected new String _LotSerialNbr;
        [AMLotSerialNbr(typeof(AMDisassembleTran.inventoryID), typeof(AMDisassembleTran.subItemID),
            typeof(AMDisassembleTran.locationID), PersistingCheck = PXPersistingCheck.Nothing)]
        public override String LotSerialNbr
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
        #region MatlLineId
        public new abstract class matlLineId : PX.Data.BQL.BqlInt.Field<matlLineId> { }

        protected new Int32? _MatlLineId;
        [PXDBInt]
        [PXUIField(DisplayName = "Material Line Nbr", Visible = false)]
        [PXSelector(typeof(Search<AMProdMatl.lineID,
                Where<AMProdMatl.orderType, Equal<Current<AMDisassembleTran.orderType>>,
                    And<AMProdMatl.prodOrdID, Equal<Current<AMDisassembleTran.prodOrdID>>,
                        And<AMProdMatl.operationID, Equal<Current<AMDisassembleTran.operationID>>,
                            And<AMProdMatl.inventoryID, Equal<Current<AMDisassembleTran.inventoryID>>,
                                And<Where<AMProdMatl.subItemID, Equal<Current<AMDisassembleTran.subItemID>>,
                                    Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>>>>>>>),
            typeof(AMProdMatl.lineNbr),
            typeof(AMProdMatl.inventoryID),
            typeof(AMProdMatl.subItemID),
            typeof(AMProdMatl.statusID),
            typeof(AMProdMatl.qtyRemaining),
            typeof(AMProdMatl.uOM),
            typeof(AMProdMatl.descr))]
        public override Int32? MatlLineId
        {
            get
            {
                return this._MatlLineId;
            }
            set
            {
                this._MatlLineId = value;
            }
        }
        #endregion
        #region TranPeriodID
        public new abstract class tranPeriodID : PX.Data.BQL.BqlString.Field<tranPeriodID> { }

        protected new String _TranPeriodID;
        [FinPeriodID]
        [PXDBDefault(typeof(AMDisassembleBatch.tranPeriodID))]
        public override String TranPeriodID
        {
            get
            {
                return this._TranPeriodID;
            }
            set
            {
                this._TranPeriodID = value;
            }
        }
        #endregion
        #region FinPeriodID
        public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

        protected new String _FinPeriodID;
        [FinPeriodID]
        [PXDBDefault(typeof(AMDisassembleBatch.finPeriodID))]
        public override String FinPeriodID
        {
            get
            {
                return this._FinPeriodID;
            }
            set
            {
                this._FinPeriodID = value;
            }
        }
        #endregion
		#region WIPAcctID
        public new abstract class wIPAcctID : PX.Data.BQL.BqlInt.Field<wIPAcctID> { }

        protected new Int32? _WIPAcctID;
        [PXDBInt]
        [PXDefault(typeof(Search<AMProdItem.wIPAcctID, Where<AMProdItem.orderType, Equal<Current<AMDisassembleTran.orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleTran.prodOrdID>>>>>))]
        [PXUIField(DisplayName = "WIP Account")]
        [PXFormula(typeof(Default<AMDisassembleTran.orderType, AMDisassembleTran.prodOrdID>))]
        public override Int32? WIPAcctID
        {
            get
            {
                return this._WIPAcctID;
            }
            set
            {
                this._WIPAcctID = value;
            }
        }
        #endregion
        #region WIPSubID
        public new abstract class wIPSubID : PX.Data.BQL.BqlInt.Field<wIPSubID> { }

        protected new Int32? _WIPSubID;
        [PXDBInt]
        [PXDefault(typeof(Search<AMProdItem.wIPSubID, Where<AMProdItem.orderType, Equal<Current<AMDisassembleTran.orderType>>,
            And<AMProdItem.prodOrdID, Equal<Current<AMDisassembleTran.prodOrdID>>>>>))]
        [PXUIField(DisplayName = "WIP Subaccount")]
        [PXFormula(typeof(Default<AMDisassembleTran.orderType, AMDisassembleTran.prodOrdID>))]
        public override Int32? WIPSubID
        {
            get
            {
                return this._WIPSubID;
            }
            set
            {
                this._WIPSubID = value;
            }
        }
        #endregion
        #region InvtMult
        public new abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected new Int16? _InvtMult;
        [PXDBShort]
        [PXDefault((short)0)]
        [PXUIField(DisplayName = "Multiplier", Enabled = false, Visible = false)]
        [PXFormula(typeof(Default<AMDisassembleTran.tranType, AMDisassembleTran.isScrap, AMDisassembleTran.isStockItem>))]
        public override Int16? InvtMult
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
        #region ExpireDate
        public new abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected new DateTime? _ExpireDate;
        [INExpireDate(typeof(AMDisassembleTran.inventoryID), PersistingCheck = PXPersistingCheck.Nothing, 
            FieldClass = "LotSerial")]
        public override DateTime? ExpireDate
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
        #region TranDesc
        public new abstract class tranDesc : PX.Data.BQL.BqlString.Field<tranDesc> { }

        protected new String _TranDesc;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        [PXDefault(typeof(Search<InventoryItem.descr, Where<InventoryItem.inventoryID, 
            Equal<Current<AMDisassembleTran.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMDisassembleTran.inventoryID>))]
        public override String TranDesc
        {
            get
            {
                return this._TranDesc;
            }
            set
            {
                this._TranDesc = value;
            }
        }
        #endregion
        #region IsStockItem
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }

        [PXFormula(typeof(Selector<AMDisassembleTran.inventoryID, InventoryItem.stkItem>))]
        [PXBool]
        [PXUIField(DisplayName = "Is stock", Enabled = false, Visibility = PXUIVisibility.Invisible, Visible = false)]
        public override bool? IsStockItem { get; set; }
        #endregion
        #region ReasonCodeID
        public new abstract class reasonCodeID : PX.Data.BQL.BqlString.Field<reasonCodeID> { }

        protected new String _ReasonCodeID;
        [PXDBString(20, InputMask = ">aaaaaaaaaaaaaaaaaaaa")]
        [PXUIField(DisplayName = "Reason Code")]
        [PXSelector(typeof(Search<ReasonCode.reasonCodeID,
            Where<ReasonCode.usage, Equal<ReasonCodeExt.production>>>))]
        public override String ReasonCodeID
        {
            get
            {
                return this._ReasonCodeID;
            }
            set
            {
                this._ReasonCodeID = value;
            }
        }
        #endregion
        #region IsScrap
        public new abstract class isScrap : PX.Data.BQL.BqlBool.Field<isScrap> { }

        protected new Boolean? _IsScrap;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Qty is Scrap")]
        public override Boolean? IsScrap
        {
            get
            {
                return this._IsScrap;
            }
            set
            {
                this._IsScrap = value;
            }
        }
        #endregion
        #region TranOverride
        public new abstract class tranOverride : PX.Data.BQL.BqlBool.Field<tranOverride> { }

        protected new Boolean? _TranOverride;
        [PXDBBool]
        [PXDefault(true)]
        [PXUIField(DisplayName = "Override")]
        public override Boolean? TranOverride
        {
            get
            {
                return this._TranOverride;
            }
            set
            {
                this._TranOverride = value;
            }
        }
        #endregion

        public static implicit operator AMDisassembleTranSplit(AMDisassembleTran item)
        {
            return new AMDisassembleTranSplit
            {
                DocType = item.DocType,
                TranType = item.TranType,
                BatNbr = item.BatNbr,
                LineNbr = item.LineNbr,
                SplitLineNbr = (int) 1,
                InventoryID = item.InventoryID,
                SiteID = item.SiteID,
                SubItemID = item.SubItemID,
                LocationID = item.LocationID,
                LotSerialNbr = item.LotSerialNbr,
                ExpireDate = item.ExpireDate,
                Qty = item.Qty.GetValueOrDefault(),
                BaseQty = item.BaseQty.GetValueOrDefault(),
                UOM = item.UOM,
                TranDate = item.TranDate,
                InvtMult = item.InvtMult,
                Released = item.Released
            };
        }

        public static implicit operator AMDisassembleTran(AMDisassembleTranSplit item)
        {
            return new AMDisassembleTran
            {
                DocType = item.DocType,
                TranType = item.TranType,
                BatNbr = item.BatNbr,
                LineNbr = item.LineNbr,
                InventoryID = item.InventoryID,
                SiteID = item.SiteID,
                SubItemID = item.SubItemID,
                LocationID = item.LocationID,
                LotSerialNbr = item.LotSerialNbr,
                Qty = item.Qty.GetValueOrDefault(),
                BaseQty = item.BaseQty.GetValueOrDefault(),
                UOM = item.UOM,
                TranDate = item.TranDate,
                InvtMult = item.InvtMult,
                Released = item.Released
            };
        }
    }
}
