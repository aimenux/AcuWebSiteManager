using System;
using System.Collections.Generic;
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    [Serializable]
    [PXCacheName(Messages.ProductionMatl)]
    [PXPrimaryGraph(
        new Type[] { typeof(ProdDetail) },
        new Type[] { typeof(Select<AMProdItem,
            Where<AMProdItem.orderType, Equal<Current<AMProdMatl.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>>>>)})]
    public class AMProdMatl : IBqlTable, ISortOrder, ILSPrimary, IProdOper, IPhantomBomReference, INotable
    {
        internal string DebuggerDisplay => $"[{OrderType}:{ProdOrdID}] OperationID = {OperationID}, LineID = {LineID}, InventoryID = {InventoryID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdMatl>.By<orderType, prodOrdID, operationID, lineID>
        {
            public static AMProdMatl Find(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? lineID) 
                => FindBy(graph, orderType, prodOrdID, operationID, lineID);
            public static AMProdMatl FindDirty(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? lineID)
                => PXSelect<AMProdMatl,
                    Where<orderType, Equal<Required<orderType>>,
                        And<prodOrdID, Equal<Required<prodOrdID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID, operationID, lineID);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdMatl>.By<orderType> { }
            public class ProductionOrder : AMProdItem.PK.ForeignKeyOf<AMProdMatl>.By<orderType, prodOrdID> { }
            public class Operation : AMProdOper.PK.ForeignKeyOf<AMProdMatl>.By<orderType, prodOrdID, operationID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMProdMatl>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMProdMatl>.By<siteID> { }
        }

        #endregion

        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdOper.orderType))]
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
        [ProductionNbr(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdOper.prodOrdID))]
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
        [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof(AMProdOper.operationID))]
        [PXParent(typeof(Select<AMProdOper,
            Where<AMProdOper.orderType, Equal<Current<AMProdMatl.orderType>>,
                And<AMProdOper.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>,
                    And<AMProdOper.operationID, Equal<Current<AMProdMatl.operationID>>>>>>))]
        [PXParent(typeof(Select<AMProdItem,
            Where<AMProdItem.orderType, Equal<Current<AMProdMatl.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>>>>))]
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
        #region LineID
        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

        protected Int32? _LineID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
        [PXLineNbr(typeof(AMProdOper.lineCntrMatl))]
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
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [Inventory(Visibility=PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
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
            Where<InventoryItem.inventoryID, Equal<Current<AMProdMatl.inventoryID>>,
            And<Where<InventoryItem.defaultSubItemOnEntry, Equal<True>,
                Or<Not<FeatureInstalled<FeaturesSet.subItem>>>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMProdMatl.inventoryID))]
        [PXFormula(typeof(Default<AMProdMatl.inventoryID>))]
        [SubItemStatusVeryfier(typeof(AMProdMatl.inventoryID), typeof(AMProdMatl.siteID), InventoryItemStatus.Inactive)]
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
        #region Descr
        public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

        protected String _Descr;
        [PXDBString(256, IsUnicode = true)]
        [PXDefault(typeof(Search<InventoryItem.descr, Where<InventoryItem.inventoryID, 
            Equal<Current<AMProdMatl.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Description", Visibility=PXUIVisibility.SelectorVisible)]
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
        #region QtyReq
        public abstract class qtyReq : PX.Data.BQL.BqlDecimal.Field<qtyReq> { }

        protected Decimal? _QtyReq;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "1.0")]
        [PXUIField(DisplayName = "Qty Required")]
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
        #region QtyReqWithScrap
        /// <summary>
        /// Qty Required with Scrap Factor worked in
        /// </summary>
        public abstract class qtyReqWithScrap : PX.Data.BQL.BqlDecimal.Field<qtyReqWithScrap> { }

	    protected Decimal? _QtyReqWithScrap;
        /// <summary>
        /// Qty Required with Scrap Factor worked in
        /// </summary>
        [PXQuantity]
        [PXUIField(DisplayName = "Qty Required with Scrap")]
        [PXFormula(typeof(Mult<AMProdMatl.qtyReq, Add<decimal1, AMProdMatl.scrapFactor>>))]
        public virtual Decimal? QtyReqWithScrap
        {
            get
            {
                return this._QtyReqWithScrap;
            }
            set
            {
                this._QtyReqWithScrap = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<AMProdMatl.inventoryID>>>>))]
        [INUnit(typeof(AMProdMatl.inventoryID))]
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
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }

        protected Decimal? _UnitCost;
        [PXDBPriceCost]
        [PXUIField(DisplayName = "Unit Cost")]
        [PXDefault]
        [MatlUnitCostDefault(
            typeof(AMProdMatl.inventoryID),
            typeof(AMProdMatl.siteID),
            typeof(AMProdMatl.uOM),
            typeof(AMProdItem),
            typeof(AMProdItem.siteID))]
        [PXFormula(typeof(Default<AMProdMatl.inventoryID, AMProdMatl.siteID, AMProdMatl.uOM>))]
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
        #region BFlush
        public abstract class bFlush : PX.Data.BQL.BqlBool.Field<bFlush> { }

        protected Boolean? _BFlush;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Backflush")]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), PX.Objects.IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true)]
        [Site]
        [PXDefault(typeof(AMProdItem.siteID))]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
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
        [MfgLocationAvail(typeof(AMProdMatl.inventoryID), typeof(AMProdMatl.subItemID), typeof(AMProdMatl.siteID),
            IsSalesType:typeof(Where<AMProdMatl.qtyReq, GreaterEqual<decimal0>>),
            IsReceiptType:typeof(Where<AMProdMatl.qtyReq, Less<decimal0>>),
            DefaultLocation = false)]
        [PXForeignReference(typeof(CompositeKey<Field<siteID>.IsRelatedTo<INLocation.siteID>, Field<locationID>.IsRelatedTo<INLocation.locationID>>))]
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
        #region CompBOMID
        public abstract class compBOMID : PX.Data.BQL.BqlString.Field<compBOMID> { }

        protected String _CompBOMID;
        [BomID(DisplayName = "Comp BOM ID", Visible = false)]
        [BOMIDSelector(typeof(Search2<AMBomItemActive.bOMID,
            LeftJoin<InventoryItem, On<AMBomItemActive.inventoryID, Equal<InventoryItem.inventoryID>>>,
            Where<AMBomItemActive.inventoryID, Equal<Current<AMProdMatl.inventoryID>>>>))]
        public virtual String CompBOMID
        {
            get
            {
                return this._CompBOMID;
            }
            set
            {
                this._CompBOMID = value;
            }
        }
        #endregion
        #region CompBOMRevisionID
        public abstract class compBOMRevisionID : PX.Data.BQL.BqlString.Field<compBOMRevisionID> { }

        protected string _CompBOMRevisionID;
        [RevisionIDField(DisplayName = "Comp BOM Revision", Visible = false)]
        [PXRestrictor(typeof(Where<AMBomItem.status, Equal<AMBomStatus.active>>), Messages.BomRevisionIsNotActive, typeof(AMBomItem.bOMID), typeof(AMBomItem.revisionID), CacheGlobal = true)]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<AMProdMatl.compBOMID>>>>)
            , typeof(AMBomItem.revisionID)
            , typeof(AMBomItem.status)
            , typeof(AMBomItem.descr)
            , typeof(AMBomItem.effStartDate)
            , typeof(AMBomItem.effEndDate)
            , DescriptionField = typeof(AMBomItem.descr))]
        [PXForeignReference(typeof(CompositeKey<Field<AMProdMatl.compBOMID>.IsRelatedTo<AMBomItem.bOMID>, Field<AMProdMatl.compBOMRevisionID>.IsRelatedTo<AMBomItem.revisionID>>))]
        [PXFormula(typeof(Default<AMProdMatl.compBOMID>))]
        public virtual string CompBOMRevisionID
        {
            get
            {
                return this._CompBOMRevisionID;
            }
            set
            {
                this._CompBOMRevisionID = value;
            }
        }
        #endregion
        #region CompEffDate
        [Obsolete("Use compBOMID & compBOMRevisionID to capture bom used on order")]
        public abstract class compEffDate : PX.Data.BQL.BqlDateTime.Field<compEffDate> { }

        protected DateTime? _CompEffDate;
        [Obsolete("Use CompBOMID & CompBOMRevisionID to capture bom used on order")]
        [PXDBDate]
        [PXUIField(DisplayName = "Obsolete Comp BOM Eff Date", Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual DateTime? CompEffDate
        {
            get
            {
                return this._CompEffDate;
            }
            set
            {
                this._CompEffDate = value;
            }
        }
        #endregion
        #region ScrapFactor
        public abstract class scrapFactor : PX.Data.BQL.BqlDecimal.Field<scrapFactor> { }

        protected Decimal? _ScrapFactor;
        [PXDBDecimal(6, MinValue = 0.0)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Scrap Factor")]
        public virtual Decimal? ScrapFactor
        {
            get
            {
                return this._ScrapFactor;
            }
            set
            {
                this._ScrapFactor = value;
            }
        }
        #endregion
        #region QtyActual
        public abstract class qtyActual : PX.Data.BQL.BqlDecimal.Field<qtyActual> { }

        protected Decimal? _QtyActual;
        [PXDBQuantity(typeof(AMProdMatl.uOM), typeof(AMProdMatl.baseQtyActual))]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty Actual", Enabled = false)]
        public virtual Decimal? QtyActual
        {
            get
            {
                return this._QtyActual;
            }
            set
            {
                this._QtyActual = value;
            }
        }
        #endregion
        #region TotActCost
        public abstract class totActCost : PX.Data.BQL.BqlDecimal.Field<totActCost> { }

        protected Decimal? _TotActCost;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Actual Cost", Enabled = false)]
        public virtual Decimal? TotActCost
        {
            get
            {
                return this._TotActCost;
            }
            set
            {
                this._TotActCost = value;
            }
        }
        #endregion
        #region MaterialType
        public abstract class materialType : PX.Data.BQL.BqlInt.Field<materialType> { }

        protected int? _MaterialType;
        [PXDBInt]
        [PXDefault(AMMaterialType.Regular)]
        [PXUIField(DisplayName = "Material Type")]
        [AMMaterialType.ProductionList]
        public virtual int? MaterialType
        {
            get
            {
                return this._MaterialType;
            }
            set
            {
                this._MaterialType = value;
            }
        }
        #endregion
        #region RefType (Obsolete)
        [Obsolete("References stored in split/plan tables")]
        public abstract class refType : PX.Data.BQL.BqlString.Field<refType> { }

        protected String _RefType;
        [Obsolete("References stored in split/plan tables")]
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Reference Type", Visibility = PXUIVisibility.Invisible)]
        [XRefType.List]
        public virtual String RefType
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
        #region RefOrderType (Obsolete)
        [Obsolete("References stored in split/plan tables")]
        public abstract class refOrderType : PX.Data.BQL.BqlString.Field<refOrderType> { }

        protected String _RefOrderType;
        [Obsolete("References stored in split/plan tables")]
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "Ref Order Type", Enabled = false, Visible = false, Visibility = PXUIVisibility.Invisible)]
        public virtual String RefOrderType
        {
            get
            {
                return this._RefOrderType;
            }
            set
            {
                this._RefOrderType = value;
            }
        }
        #endregion
        #region RefNbr (Obsolete)
        [Obsolete("References stored in split/plan tables")]
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

        protected String _RefNbr;
        [Obsolete("References stored in split/plan tables")]
        [PXDBString(15, IsFixed = true)]
        [PXUIField(DisplayName = "Reference Nbr", Enabled = false, Visibility = PXUIVisibility.Invisible)]
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
        #region StatusID
        public abstract class statusID : PX.Data.BQL.BqlString.Field<statusID> { }

        protected String _StatusID;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ProductionOrderStatus.Planned, typeof(Search<
            AMProdItem.statusID, 
            Where<AMProdItem.orderType, Equal<Current<AMProdMatl.orderType>>,
                And<AMProdItem.prodOrdID, Equal<Current<AMProdMatl.prodOrdID>>>>>))]
        [PXUIField(DisplayName = "Status")]
        [ProductionOrderStatus.List]
        public virtual String StatusID
        {
            get
            {
                return this._StatusID;
            }
            set
            {
                this._StatusID = value;
            }
        }
        #endregion
        #region BaseQtyActual
        public abstract class baseQtyActual : PX.Data.BQL.BqlDecimal.Field<baseQtyActual> { }

        protected Decimal? _BaseQtyActual;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Qty Actual")]
        public virtual Decimal? BaseQtyActual
        {
            get
            {
                return this._BaseQtyActual;
            }
            set
            {
                this._BaseQtyActual = value;
            }
        }
        #endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXNote]
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
        #region PhtmBOMID
        public abstract class phtmBOMID : PX.Data.BQL.BqlString.Field<phtmBOMID> { }

        protected String _PhtmBOMID;
        [PXDBString(15, IsFixed = true)]
        [PXUIField(DisplayName = "Phantom BOM ID", Visible = false, Enabled = false)]
        public virtual String PhtmBOMID
        {
            get
            {
                return this._PhtmBOMID;
            }
            set
            {
                this._PhtmBOMID = value;
            }
        }
        #endregion
        #region PhtmBOMRevisionID
        public abstract class phtmBOMRevisionID : PX.Data.BQL.BqlString.Field<phtmBOMRevisionID> { }

        protected string _PhtmBOMRevisionID;
        [RevisionIDField(DisplayName = "Phantom BOM Revision", Visible = false, Enabled = false)]
        public virtual string PhtmBOMRevisionID
        {
            get
            {
                return this._PhtmBOMRevisionID;
            }
            set
            {
                this._PhtmBOMRevisionID = value;
            }
        }
        #endregion
        #region PhtmBOMLineRef
        public abstract class phtmBOMLineRef : PX.Data.BQL.BqlInt.Field<phtmBOMLineRef> { }

        protected Int32? _PhtmBOMLineRef;
        [PXDBInt]
        [PXUIField(DisplayName = "Phantom BOM Ref Line Nbr", Visible = false, Enabled = false)]
        public virtual Int32? PhtmBOMLineRef
        {
            get
            {
                return this._PhtmBOMLineRef;
            }
            set
            {
                this._PhtmBOMLineRef = value;
            }
        }
        #endregion
        #region PhtmBOMOperationID
        public abstract class phtmBOMOperationID : PX.Data.BQL.BqlInt.Field<phtmBOMOperationID> { }

        protected int? _PhtmBOMOperationID;
        [OperationIDField(DisplayName = "Phantom Operation ID", Visible = false, Enabled = false)]
        public virtual int? PhtmBOMOperationID
        {
            get
            {
                return this._PhtmBOMOperationID;
            }
            set
            {
                this._PhtmBOMOperationID = value;
            }
        }
        #endregion
        #region PhtmLevel
        public abstract class phtmLevel : PX.Data.BQL.BqlInt.Field<phtmLevel> { }

        protected Int32? _PhtmLevel;
        [PXDBInt]
        [PXUIField(DisplayName = "Phantom Level", Visible = false, Enabled = false)]
        public virtual Int32? PhtmLevel
        {
            get
            {
                return this._PhtmLevel;
            }
            set
            {
                this._PhtmLevel = value;
            }
        }
        #endregion
        #region PhtmMatlBOMID
        public abstract class phtmMatlBOMID : PX.Data.BQL.BqlString.Field<phtmMatlBOMID> { }

        protected String _PhtmMatlBOMID;
        [PXDBString(15, IsFixed = true)]
        [PXUIField(DisplayName = "Phantom Matl BOM ID", Visible = false, Enabled = false)]
        public virtual String PhtmMatlBOMID
        {
            get
            {
                return this._PhtmMatlBOMID;
            }
            set
            {
                this._PhtmMatlBOMID = value;
            }
        }
        #endregion
        #region PhtmMatlRevisionID
        public abstract class phtmMatlRevisionID : PX.Data.BQL.BqlString.Field<phtmMatlRevisionID> { }

        protected string _PhtmMatlRevisionID;
        [RevisionIDField(DisplayName = "Phantom Matl Revision", Visible = false, Enabled = false)]
        public virtual string PhtmMatlRevisionID
        {
            get
            {
                return this._PhtmMatlRevisionID;
            }
            set
            {
                this._PhtmMatlRevisionID = value;
            }
        }
        #endregion
        #region PhtmMatlLineRef
        public abstract class phtmMatlLineRef : PX.Data.BQL.BqlInt.Field<phtmMatlLineRef> { }

        protected Int32? _PhtmMatlLineRef;
        [PXDBInt]
        [PXUIField(DisplayName = "Phantom Matl Line Nbr", Visible = false, Enabled = false)]
        public virtual Int32? PhtmMatlLineRef
        {
            get
            {
                return this._PhtmMatlLineRef;
            }
            set
            {
                this._PhtmMatlLineRef = value;
            }
        }
        #endregion
        #region PhtmMatlOperationID
        public abstract class phtmMatlOperationID : PX.Data.BQL.BqlInt.Field<phtmMatlOperationID> { }

        protected int? _PhtmMatlOperationID;
        [OperationIDField(DisplayName = "Phantom Matl Operation ID", Visible = false, Enabled = false)]
        public virtual int? PhtmMatlOperationID
        {
            get
            {
                return this._PhtmMatlOperationID;
            }
            set
            {
                this._PhtmMatlOperationID = value;
            }
        }
        #endregion
        #region PhantomRouting
        public abstract class phantomRouting : PX.Data.BQL.BqlInt.Field<phantomRouting> { }

        protected int? _PhantomRouting;
        [PXDBInt]
        [PXDefault(PhantomRoutingOptions.Before)]
        [PXUIField(DisplayName = "Phantom Routing", Visible = false, Enabled = false)]
        [PhantomRoutingOptions.List]
        public virtual int? PhantomRouting
        {
            get
            {
                return this._PhantomRouting;
            }
            set
            {
                this._PhantomRouting = value;
            }
        }
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
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }

        protected Guid? _CreatedByID;
        [PXDBCreatedByID]
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
        [PXDBCreatedByScreenID]
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
        [PXDBCreatedDateTime]
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
        [PXDBLastModifiedByID]
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
        [PXDBLastModifiedByScreenID]
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
        [PXDBLastModifiedDateTime]
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
	    #region LineNbr
	    public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        //Required for ISortOrder
	    [PXInt]
        //Leave visible for selectors of lineIDs...
	    [PXUIField(DisplayName = "Line Nbr", Visibility = PXUIVisibility.Invisible, Enabled = false)]
        [PXDependsOnFields(typeof(AMProdMatl.lineID))]
	    public virtual Int32? LineNbr
	    {
	        get
	        {
	            return this._LineID;
	        }
	    }
	    #endregion
	    #region SortOrder
	    public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

	    protected Int32? _SortOrder;
	    [PXUIField(DisplayName = PX.Objects.AP.APTran.sortOrder.DispalyName, Visible = false, Enabled = false)]
	    [PXDBInt]
	    public virtual Int32? SortOrder
	    {
	        get
	        {
	            return this._SortOrder;
	        }
	        set
	        {
	            this._SortOrder = value;
	        }
	    }
        #endregion
	    #region QtyRoundUp
	    public abstract class qtyRoundUp : PX.Data.BQL.BqlBool.Field<qtyRoundUp> { }

	    [PXDBBool]
	    [PXDefault(typeof(Search<InventoryItemExt.aMQtyRoundUp, Where<InventoryItem.inventoryID, 
            Equal<Current<AMProdMatl.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Qty Round Up")]
	    public Boolean? QtyRoundUp { get; set; }
	    #endregion
        #region BatchSize
        public abstract class batchSize : PX.Data.BQL.BqlDecimal.Field<batchSize> { }

	    protected Decimal? _BatchSize;
	    [BatchSize]
	    [PXDefault(TypeCode.Decimal, "1.0")]
        public virtual Decimal? BatchSize
	    {
	        get
	        {
	            return this._BatchSize;
	        }
	        set
	        {
	            this._BatchSize = value;
	        }
	    }
        #endregion
	    #region IsByproduct
	    public abstract class isByproduct : PX.Data.BQL.BqlBool.Field<isByproduct> { }

	    [PXBool]
	    [PXUIField(DisplayName = "By-product", Enabled = false, Visible = false)]
	    [PXDependsOnFields(typeof(AMProdMatl.qtyReq))]
	    public virtual Boolean? IsByproduct
	    {
	        get
	        {
	            return _QtyReq.GetValueOrDefault() < 0;
	        }
	    }
	    #endregion
        #region IsStockItem
        public abstract class isStockItem : PX.Data.BQL.BqlBool.Field<isStockItem> { }
        protected Boolean? _IsStockItem;
        [PXDBBool]
        [PXUIField(DisplayName = "Is stock", Enabled = false)]
        [PXDefault(false, typeof(Search<InventoryItem.stkItem, Where<InventoryItem.inventoryID, Equal<Current<AMProdMatl.inventoryID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMProdMatl.inventoryID>))]
        public virtual bool? IsStockItem
        {
            get
            {
                return this._IsStockItem;
            }
            set
            {
                this._IsStockItem = value;
            }
        }
        #endregion
        #region IsFixedMaterial
        public abstract class isFixedMaterial : PX.Data.BQL.BqlBool.Field<isFixedMaterial> { }

	    [PXBool]
	    [PXUIField(DisplayName = "Fixed Material", Enabled = false, Visible = false)]
	    [PXDependsOnFields(typeof(AMProdMatl.batchSize))]
	    public virtual Boolean? IsFixedMaterial
        {
	        get
	        {
	            return _BatchSize.GetValueOrDefault() == 0m;
	        }
	    }
	    #endregion
        #region TotalQtyRequired
        public abstract class totalQtyRequired : PX.Data.BQL.BqlDecimal.Field<totalQtyRequired> { }

	    protected Decimal? _TotalQtyRequired;
	    [PXDBQuantity(typeof(AMProdMatl.uOM), typeof(AMProdMatl.baseTotalQtyRequired))]
        [PXDefault(TypeCode.Decimal, "0.0")]
	    [PXUIField(DisplayName = "Total Required", Enabled = false)]
	    [PXFormula(typeof(Switch<Case<Where<AMProdMatl.qtyRoundUp, Equal<True>>,
                MathCeiling<Switch<Case<Where<AMProdMatl.batchSize, Equal<decimal0>>, Mult<AMProdMatl.qtyReq, Add<decimal1, AMProdMatl.scrapFactor>>>,
                    Mult<Mult<AMProdMatl.qtyReq, Add<decimal1, AMProdMatl.scrapFactor>>, Div<Parent<AMProdOper.baseTotalQty>, AMProdMatl.batchSize>>>>>,
                Switch<Case<Where<AMProdMatl.batchSize, Equal<decimal0>>, Mult<AMProdMatl.qtyReq, Add<decimal1, AMProdMatl.scrapFactor>>>,
                    Mult<Mult<AMProdMatl.qtyReq, Add<decimal1, AMProdMatl.scrapFactor>>, Div<Parent<AMProdOper.baseTotalQty>, AMProdMatl.batchSize>>>>))]
        public virtual Decimal? TotalQtyRequired
        {
	        get
	        {
	            return this._TotalQtyRequired;
            }
            set
            {
                this._TotalQtyRequired = value;
            }
        }
        #endregion
	    #region BaseTotalQtyRequired
	    public abstract class baseTotalQtyRequired : PX.Data.BQL.BqlDecimal.Field<baseTotalQtyRequired> { }

	    protected Decimal? _BaseTotalQtyRequired;
	    [PXDBQuantity]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    [PXUIField(DisplayName = "Base Total Required", Enabled = false, Visible = false)]
	    public virtual Decimal? BaseTotalQtyRequired
        {
	        get
	        {
	            return this._BaseTotalQtyRequired;
	        }
	        set
	        {
	            this._BaseTotalQtyRequired = value;
	        }
	    }
	    #endregion
        #region QtyRemaining
        public abstract class qtyRemaining : PX.Data.BQL.BqlDecimal.Field<qtyRemaining> { }

	    protected Decimal? _QtyRemaining;
        [PXQuantity]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
	    [PXUIField(DisplayName = "Qty Remaining", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AMProdMatl.isByproduct, Equal<True>>,
                MathMin<Sub<AMProdMatl.totalQtyRequired, AMProdMatl.qtyActual>, decimal0>>,
            SubNotLessThanZero<AMProdMatl.totalQtyRequired, AMProdMatl.qtyActual>>))]
        public virtual Decimal? QtyRemaining
        {
	        get
	        {
	            return this._QtyRemaining;
	        }
	        set
	        {
	            this._QtyRemaining = value;
	        }
	    }
        #endregion
	    #region BaseQtyRemaining
	    public abstract class baseQtyRemaining : PX.Data.BQL.BqlDecimal.Field<baseQtyRemaining> { }

	    protected Decimal? _BaseQtyRemaining;
	    [PXQuantity]
	    [PXUnboundDefault(TypeCode.Decimal, "0.0")]
	    [PXUIField(DisplayName = "Base Qty Remaining", Enabled = false, Visible = false)]
	    [PXFormula(typeof(Switch<Case<Where<AMProdMatl.isByproduct, Equal<True>>,
	        MathMin<Sub<AMProdMatl.baseTotalQtyRequired, AMProdMatl.baseQtyActual>, decimal0>>,
            SubNotLessThanZero<AMProdMatl.baseTotalQtyRequired, AMProdMatl.baseQtyActual>>))]
        public virtual Decimal? BaseQtyRemaining
        {
	        get
	        {
	            return this._BaseQtyRemaining;
	        }
	        set
	        {
	            this._BaseQtyRemaining = value;
	        }
	    }
        #endregion
        #region PlanCost
        public abstract class planCost : PX.Data.BQL.BqlDecimal.Field<planCost> { }

	    protected Decimal? _PlanCost;
        [PXBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Planned Cost", Enabled = false, Visible = true)]
        [PXFormula(typeof(Mult<AMProdMatl.unitCost, AMProdMatl.totalQtyRequired>))]
        public virtual Decimal? PlanCost
        {
            get
            {
                return this._PlanCost;
            }
            set
            {
                this._PlanCost = value;
            }
        }
        #endregion
	    #region SplitLineCntr
	    public abstract class splitLineCntr : PX.Data.BQL.BqlInt.Field<splitLineCntr> { }

	    protected int? _SplitLineCntr;
	    [PXDBInt]
	    [PXDefault(TypeCode.Int32, "0")]
	    public virtual int? SplitLineCntr
	    {
	        get
	        {
	            return this._SplitLineCntr;
	        }
	        set
	        {
	            this._SplitLineCntr = value;
	        }
	    }
        #endregion
        #region WarehouseOverride
        public abstract class warehouseOverride : PX.Data.BQL.BqlBool.Field<warehouseOverride> { }

        protected Boolean? _WarehouseOverride;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Warehouse Override", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Boolean? WarehouseOverride
        {
            get
            {
                return this._WarehouseOverride;
            }
            set
            {
                this._WarehouseOverride = value;
            }
        }
        #endregion

        #region Fields for ILSPrimary
        #region UnassignedQty
        public abstract class unassignedQty : PX.Data.BQL.BqlDecimal.Field<unassignedQty> { }

        protected Decimal? _UnassignedQty;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? UnassignedQty
        {
            get
            {
                return this._UnassignedQty;
            }
            set
            {
                this._UnassignedQty = value;
            }
        }
        #endregion
        #region HasMixedProjectTasks
        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class hasMixedProjectTasks : PX.Data.BQL.BqlBool.Field<hasMixedProjectTasks> { }

        protected bool? _HasMixedProjectTasks;
        /// <summary>
        /// Returns true if the splits associated with the line has mixed ProjectTask values.
        /// This field is used to validate the record on persist. 
        /// Project/Task is not implemented for Manufacturing. Including fields as a 5.30.0663 or greater requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        [PXBool]
        [PXFormula(typeof(False))]
        public virtual bool? HasMixedProjectTasks
        {
            get
            {
                return _HasMixedProjectTasks;
            }
            set
            {
                _HasMixedProjectTasks = value;
            }
        }
        #endregion
        #region TranType
        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true)]
        [PXFormula(typeof(Switch<
            Case<
                Where<Parent<AMProdItem.function>, Equal<OrderTypeFunction.disassemble>, And<AMProdMatl.qtyReq, GreaterEqual<decimal0>>>, INTranType.receipt,
            Case<
                Where<Parent<AMProdItem.function>, NotEqual<OrderTypeFunction.disassemble>, And<AMProdMatl.qtyReq, Less<decimal0>>>, INTranType.receipt>>,
            INTranType.issue>))]
        [INTranType.List]
        [PXUIField(DisplayName = "Tran. Type")]
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
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Tran Date")]
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
        #region InvtMult

        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected Int16? _InvtMult;
        [PXDBShort]
        [PXFormula(typeof(Switch<
            Case<
                Where<Parent<AMProdItem.function>, Equal<OrderTypeFunction.disassemble>, And<AMProdMatl.qtyReq, GreaterEqual<decimal0>>>, short1,
                Case<
                    Where<Parent<AMProdItem.function>, NotEqual<OrderTypeFunction.disassemble>, And<AMProdMatl.qtyReq, Less<decimal0>>>, short1>>,
            shortMinus1>))]
        [PXDefault((short)1)]
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
        #region LotSerialNbr

        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }

        protected String _LotSerialNbr;
        [INLotSerialNbr(typeof(AMProdMatl.inventoryID), typeof(AMProdMatl.subItemID), typeof(AMProdMatl.locationID), PersistingCheck = PXPersistingCheck.Nothing, Visible = false)]
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
        #region ExpireDate

        public abstract class expireDate : PX.Data.BQL.BqlDateTime.Field<expireDate> { }

        protected DateTime? _ExpireDate;
        [INExpireDate(typeof(AMProdMatl.inventoryID), PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region Qty
        public virtual Decimal? Qty
        {
            get
            {
                return this._QtyRemaining;
            }
            set
            {
                this._QtyRemaining = value;
            }
        }
        #endregion
        #region BaseQty

        public virtual Decimal? BaseQty
        {
            get
            {
                return this._BaseQtyRemaining;
            }
            set
            {
                this._BaseQtyRemaining = value;
            }
        }
        #endregion
        #region ProjectID
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

        protected Int32? _ProjectID;
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        [PXInt]
        [PXUIField(DisplayName = "Project", Visible = false, Enabled = false)]
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
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }

        protected Int32? _TaskID;
        /// <summary>
        /// Project/Task is not implemented for Manufacturing. Requirement for the class that implements ILSPrimary/ILSMaster
        /// </summary>
        [PXInt]
        [PXUIField(DisplayName = "Task", Visible = false, Enabled = false)]
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
        #endregion

        #region POCreate
        public abstract class pOCreate : PX.Data.BQL.BqlBool.Field<pOCreate> { }

        protected Boolean? _POCreate;
        [PXDBBool]
        [DefaultMarkFor(MaterialDefaultMarkFor.Purchase)]
        [PXFormula(typeof(Default<AMProdMatl.inventoryID>))]
        [PXUIField(DisplayName = "Mark for PO", Visible = true)]
        public virtual Boolean? POCreate
        {
            get
            {
                return this._POCreate;
            }
            set
            {
                this._POCreate = value ?? false;
            }
        }
        #endregion
        #region ProdCreate
        public abstract class prodCreate : PX.Data.BQL.BqlBool.Field<prodCreate> { }

        protected Boolean? _ProdCreate;
        [PXDBBool]
        [DefaultMarkFor(MaterialDefaultMarkFor.Production)]
        [PXFormula(typeof(Default<AMProdMatl.inventoryID>))]
        [PXUIField(DisplayName = "Mark for Production", Visible = true)]
        public virtual Boolean? ProdCreate
        {
            get
            {
                return this._ProdCreate;
            }
            set
            {
                this._ProdCreate = value ?? false;
            }
        }
        #endregion
        #region VendorID
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        protected Int32? _VendorID;
        [PX.Objects.AP.Vendor(typeof(Search<BAccountR.bAccountID,
            Where<PX.Objects.AP.Vendor.type, NotEqual<BAccountType.employeeType>>>))]
        [PXDefault(typeof(Search<INItemSiteSettings.preferredVendorID,
                Where<INItemSiteSettings.inventoryID, Equal<Current<AMProdMatl.inventoryID>>, And<INItemSiteSettings.siteID, Equal<Current<AMProdMatl.siteID>>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [PXFormula(typeof(Default<AMProdMatl.inventoryID, AMProdMatl.siteID>))]
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
        #region SubcontractSource
        public abstract class subcontractSource : PX.Data.BQL.BqlInt.Field<subcontractSource> { }

        protected int? _SubcontractSource;
        [PXDBInt]
        [PXDefault(AMSubcontractSource.None)]
        [PXUIField(DisplayName = "Subcontract Source")]
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

        #region Allocation Qty Unbound Fields
        #region LineQtyAvail
        public abstract class lineQtyAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyAvail> { }
        [PXDecimal(6)]
        public decimal? LineQtyAvail
        {
            get;
            set;
        }
        #endregion
        #region LineQtyHardAvail
        public abstract class lineQtyHardAvail : PX.Data.BQL.BqlDecimal.Field<lineQtyHardAvail> { }
        [PXDecimal(6)]
        public decimal? LineQtyHardAvail
        {
            get;
            set;
        }
        #endregion
        #endregion

        #region METHODS

#pragma warning disable PX1031 // DACs cannot contain instance methods
        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
	    public virtual decimal GetTotalRequiredQtyCompanyRounded(decimal totalOrderQty)
	    {
	        return UomHelper.QuantityRound(GetTotalRequiredQty(totalOrderQty));
	    }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual decimal GetTotalRequiredQty(decimal totalOrderQty)
	    {
	        return GetTotalRequiredQty(totalOrderQty, this.QtyRoundUp.GetValueOrDefault());
	    }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual decimal GetTotalRequiredQty(decimal totalOrderQty, bool roundUp)
	    {
	        return GetTotalRequiredQty(this, totalOrderQty, roundUp);
	    }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual decimal GetTotalBaseRequiredQty(decimal totalOrderQty)
        {
	        return GetTotalBaseRequiredQty(totalOrderQty, this.QtyRoundUp.GetValueOrDefault());
	    }

        [Obsolete(InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual decimal GetTotalBaseRequiredQty(decimal totalOrderQty, bool roundUp)
	    {
	        return GetTotalBaseRequiredQty(this, totalOrderQty, roundUp);
	    }
#pragma warning restore PX1031 // DACs cannot contain instance methods

        public static decimal GetTotalRequiredQty(AMProdMatl prodMatl, decimal totalOrderQty, bool roundUp)
        {
            if (prodMatl == null)
            {
                return 0m;
            }

            var totalQtyRequired = prodMatl.QtyReq.GetValueOrDefault() *
                                   (1 + prodMatl.ScrapFactor.GetValueOrDefault()) *
                                   (prodMatl.BatchSize.GetValueOrDefault() == 0m ? 1m
                                       : totalOrderQty / prodMatl.BatchSize.GetValueOrDefault());

            return roundUp ? Math.Ceiling(totalQtyRequired) : totalQtyRequired;
        }

        public static decimal GetTotalBaseRequiredQty(AMProdMatl prodMatl, decimal totalOrderQty, bool roundUp)
	    {
            if (prodMatl == null)
            {
                return 0m;
            }

            var totalQtyRequired = prodMatl.BaseQty.GetValueOrDefault() *
                                       (1 + prodMatl.ScrapFactor.GetValueOrDefault()) *
	                                   (prodMatl.BatchSize.GetValueOrDefault() == 0m ? 1m
	                                       : totalOrderQty / prodMatl.BatchSize.GetValueOrDefault());

	        return roundUp ? Math.Ceiling(totalQtyRequired) : totalQtyRequired;
	    }

	    public static implicit operator AMProdMatlSplit(AMProdMatl item)
	    {
	        return new AMProdMatlSplit
	        {
	            OrderType = item.OrderType,
	            ProdOrdID = item.ProdOrdID,
	            OperationID = item.OperationID,
	            LineID = item.LineID,
	            InventoryID = item.InventoryID,
	            SubItemID = item.SubItemID,
	            SiteID = item.SiteID,
	            LocationID = item.LocationID,
	            UOM = item.UOM,
	            Qty = item.QtyRemaining,
	            BaseQty = item.BaseQtyRemaining,
	            TranType = item.TranType,
	            TranDate = item.TranDate,
	            InvtMult = item.InvtMult,
	            LotSerialNbr = item.LotSerialNbr,
	            ProjectID = item.ProjectID,
	            TaskID = item.TaskID
	        };
	    }
	    public static implicit operator AMProdMatl(AMProdMatlSplit item)
	    {
	        return new AMProdMatl
	        {
	            OrderType = item.OrderType,
	            ProdOrdID = item.ProdOrdID,
	            OperationID = item.OperationID,
	            LineID = item.LineID,
	            InventoryID = item.InventoryID,
	            SubItemID = item.SubItemID,
	            SiteID = item.SiteID,
	            LocationID = item.LocationID,
	            UOM = item.UOM,
	            QtyRemaining = item.Qty,
	            BaseQtyRemaining = item.BaseQty,
	            Qty = item.Qty,
	            BaseQty = item.BaseQty,
	            TranType = item.TranType,
	            TranDate = item.TranDate,
	            InvtMult = item.InvtMult,
	            LotSerialNbr = item.LotSerialNbr,
	            ProjectID = item.ProjectID,
	            TaskID = item.TaskID
	        };
	    }
        
        internal static List<AMProdMatlSplit> GetSplits(PXGraph graph, AMProdMatl prodMatl)
        {
            return GetSplits(graph, prodMatl?.OrderType, prodMatl?.ProdOrdID, prodMatl?.OperationID, prodMatl?.LineID);
        }

        internal static List<AMProdMatlSplit> GetSplits(PXGraph graph, string orderType, string prodOrdID, int? operationID, int? lineID)
        {
            return PXSelect<AMProdMatlSplit,
                Where<AMProdMatlSplit.orderType, Equal<Required<AMProdMatlSplit.orderType>>,
                    And<AMProdMatlSplit.prodOrdID, Equal<Required<AMProdMatlSplit.prodOrdID>>,
                        And<AMProdMatlSplit.operationID, Equal<Required<AMProdMatlSplit.operationID>>,
                            And<AMProdMatlSplit.lineID, Equal<Required<AMProdMatlSplit.lineID>>
                            >>>>>.Select(graph, orderType, prodOrdID, operationID, lineID)?.ToFirstTableList();
        }
        #endregion
    }

    /// <summary>
    /// Projection to select all <see cref="AMProdMatl"/> fields with some <see cref="InventoryItem"/> and <see cref="AMProdOper"/> fields which otherwise when joined would cause sub query to InventoryItem after the main join query (based on some attributes on Inventory Item)
    /// </summary>
    [PXProjection(typeof(Select2<AMProdMatl,
        InnerJoin<InventoryItem,
            On<InventoryItem.inventoryID, Equal<AMProdMatl.inventoryID>>,
        InnerJoin<AMProdOper,
            On<AMProdMatl.orderType, Equal<AMProdOper.orderType>,
                And<AMProdMatl.prodOrdID, Equal<AMProdOper.prodOrdID>,
                    And<AMProdMatl.operationID, Equal<AMProdOper.operationID>>>>>>>), Persistent = false)]
    [Serializable]
    [PXHidden]
    public class AMProdMatlInventory : AMProdMatl
    {
        public new abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        public new abstract class prodOrdID : PX.Data.BQL.BqlString.Field<prodOrdID> { }

        public new abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }

        #region IsStockItem
        [PXBool]
        [PXUIField(DisplayName = "Is Stock Item", Enabled = false, Visible = false)]
        //Removed formula
        public override bool? IsStockItem
        {
            get
            {
                return this._StkItem.GetValueOrDefault();
            }
            set
            {
                this._StkItem = value.GetValueOrDefault();
            }
        }
        #endregion

        #region InventoryCD
        public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD>
        {
        }
        protected String _InventoryCD;

        [InventoryRaw(DisplayName = "Inventory CD", BqlField = typeof(InventoryItem.inventoryCD))]
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
        #region StkItem
        public abstract class stkItem : PX.Data.BQL.BqlBool.Field<stkItem>
        {
        }
        protected Boolean? _StkItem;

        [PXDBBool(BqlField = typeof(InventoryItem.stkItem))]
        [PXUIField(DisplayName = "Stock Item")]
        public virtual Boolean? StkItem
        {
            get
            {
                return this._StkItem;
            }
            set
            {
                this._StkItem = value;
            }
        }
        #endregion
        #region ItemDescr
        public abstract class itemDescr : PX.Data.BQL.BqlString.Field<itemDescr>
        {
        }
        protected String _ItemDescr;

        /// <summary>
        /// The description of the Inventory Item.
        /// </summary>
        [PXDBLocalizableString(PX.Objects.Common.Constants.TranDescLength, IsUnicode = true, BqlField = typeof(InventoryItem.descr), IsProjection = true)]
        [PXUIField(DisplayName = "Item Description")]
        [PX.Data.EP.PXFieldDescription]
        public virtual String ItemDescr
        {
            get
            {
                return this._ItemDescr;
            }
            set
            {
                this._ItemDescr = value;
            }
        }
        #endregion
        #region ItemClassID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID>
        {
        }
        protected int? _ItemClassID;

        [PXDBInt(BqlField = typeof(InventoryItem.itemClassID))]
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
        #region ItemStatus
        public abstract class itemStatus : PX.Data.BQL.BqlString.Field<itemStatus>
        {
        }
        protected String _ItemStatus;

        [PXDBString(2, IsFixed = true, BqlField = typeof(InventoryItem.itemStatus))]
        [PXUIField(DisplayName = "Item Status")]
        [InventoryItemStatus.List]
        public virtual String ItemStatus
        {
            get
            {
                return this._ItemStatus;
            }
            set
            {
                this._ItemStatus = value;
            }
        }
        #endregion

        #region OperationCD
        public abstract class operationCD : PX.Data.BQL.BqlString.Field<operationCD>
        {
        }
        protected string _OperationCD;
        [OperationCDField(BqlField = typeof(AMProdOper.operationCD))]
        public virtual string OperationCD
        {
            get { return this._OperationCD; }
            set { this._OperationCD = value; }
        }

        #endregion
        #region WcID
        public abstract class wcID : PX.Data.BQL.BqlString.Field<wcID>
        {
        }
        protected String _WcID;
        [WorkCenterIDField(BqlField = typeof(AMProdOper.wcID))]
        [PXSelector(typeof(Search<AMWC.wcID>))]
        public virtual String WcID
        {
            get
            {
                return this._WcID;
            }
            set
            {
                this._WcID = value;
            }
        }
        #endregion
    }
}
