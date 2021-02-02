using System;
using PX.Objects.AM.GraphExtensions;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.CS;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.EstimateMaterial)]
    public class AMEstimateMatl : IBqlTable, IEstimateOper, IEstimateInventory, ISortOrder, INotable
    {
        #region Keys

        public class PK : PrimaryKeyOf<AMEstimateMatl>.By<estimateID, revisionID, operationID, lineID>
        {
            public static AMEstimateMatl Find(PXGraph graph, string estimateID, string revisionID, int? operationID, int? lineID)
                => FindBy(graph, estimateID, revisionID, operationID, lineID);
            public static AMEstimateMatl FindDirty(PXGraph graph, string estimateID, string revisionID, int? operationID, int? lineID)
                => PXSelect<AMEstimateMatl,
                    Where<estimateID, Equal<Required<estimateID>>,
                        And<revisionID, Equal<Required<revisionID>>,
                        And<operationID, Equal<Required<operationID>>,
                        And<lineID, Equal<Required<lineID>>>>>>>
                    .SelectWindowed(graph, 0, 1, estimateID, revisionID, operationID, lineID);
        }

        public static class FK
        {
            public class Estimate : AMEstimateItem.PK.ForeignKeyOf<AMEstimateMatl>.By<estimateID, revisionID> { }
            public class Operation : AMEstimateOper.PK.ForeignKeyOf<AMEstimateMatl>.By<estimateID, revisionID, operationID> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMEstimateMatl>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMEstimateMatl>.By<siteID> { }
        }

        #endregion

        #region Estimate ID

        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }


        protected String _EstimateID;

        [PXDBDefault(typeof (AMEstimateOper.estimateID))]
        [EstimateID(IsKey = true, Enabled = false, Visible = false)]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }

        #endregion
        #region Revision ID

        public abstract class revisionID : PX.Data.BQL.BqlString.Field<revisionID> { }


        protected String _RevisionID;

        [PXDBDefault(typeof (AMEstimateOper.revisionID))]
        [PXDBString(10, IsUnicode = true, InputMask = ">AAAAAAAAAA", IsKey = true)]
        [PXUIField(DisplayName = "Revision", Visible = false, Enabled = false)]
        public virtual String RevisionID
        {
            get { return this._RevisionID; }
            set { this._RevisionID = value; }
        }

        #endregion
        #region Operation ID
        public abstract class operationID : PX.Data.BQL.BqlInt.Field<operationID> { }


        protected Int32? _OperationID;

        [OperationIDField(IsKey = true, Visible = false, Enabled = false)]
        [PXDBDefault(typeof (AMEstimateOper.operationID))]
        [PXParent(typeof (Select<AMEstimateOper, 
            Where<AMEstimateOper.estimateID, Equal<Current<AMEstimateMatl.estimateID>>,
            And<AMEstimateOper.revisionID, Equal<Current<AMEstimateMatl.revisionID>>,
            And<AMEstimateOper.operationID, Equal<Current<AMEstimateMatl.operationID>>>>>>))]
        [PXParent(typeof(Select<AMEstimateItem, 
            Where<AMEstimateItem.estimateID, Equal<Current<AMEstimateMatl.estimateID>>,
            And<AMEstimateItem.revisionID, Equal<Current<AMEstimateMatl.revisionID>>>>>))]
        public virtual Int32? OperationID
        {
            get { return this._OperationID; }
            set { this._OperationID = value; }
            }

        #endregion
        #region Line ID
        public abstract class lineID : PX.Data.BQL.BqlInt.Field<lineID> { }

        protected Int32? _LineID;
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false, Enabled = false)]
        [PXLineNbr(typeof (AMEstimateOper.lineCntrMatl))]
        public virtual Int32? LineID
        {
            get { return this._LineID; }
            set { this._LineID = value; }
        }

        #endregion
        #region Inventory ID

        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }


        protected Int32? _InventoryID;

        [PXDBInt]
        [PXUIField(DisplayName = "Inventory ID", Enabled = false, Visible = false)]
        [PXForeignReference(typeof(Field<inventoryID>.IsRelatedTo<InventoryItem.inventoryID>))]
        public virtual Int32? InventoryID
        {
            get { return this._InventoryID; }
            set { this._InventoryID = value; }
            }

        #endregion
        #region Inventory CD

        public abstract class inventoryCD : PX.Data.BQL.BqlString.Field<inventoryCD> { }


        protected String _InventoryCD;

        [PXDefault]
        [EstimateInventoryRaw]
        public virtual String InventoryCD
        {
            get { return this._InventoryCD; }
            set { this._InventoryCD = value; }
            }

        #endregion
        #region Is Non Inventory

        public abstract class isNonInventory : PX.Data.BQL.BqlBool.Field<isNonInventory> { }


        protected Boolean? _IsNonInventory;

        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Non-Inventory", Enabled = false)]
        public virtual Boolean? IsNonInventory
        {
            get { return this._IsNonInventory; }
            set { this._IsNonInventory = value; }
            }

        #endregion
        #region Item Desc
        public abstract class itemDesc : PX.Data.BQL.BqlString.Field<itemDesc> { }

        protected String _ItemDesc;
        [PXDBString(256, IsUnicode = true)]
        [PXUIField(DisplayName = "Description")]
        [PXDefault(typeof (Search<InventoryItem.descr, Where<InventoryItem.inventoryID, Equal<Current<AMEstimateMatl.inventoryID>>>>)
            , PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual String ItemDesc
        {
            get { return this._ItemDesc; }
            set { this._ItemDesc = value; }
        }
        #endregion
        #region SubItemID

        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }


        protected Int32? _SubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMEstimateMatl.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.Nothing)]
        [SubItem(typeof(AMEstimateMatl.inventoryID))]
        [PXFormula(typeof(Default<AMEstimateMatl.inventoryID>))]
        public virtual Int32? SubItemID
        {
            get { return this._SubItemID; }
            set { this._SubItemID = value; }
        }

        #endregion
        #region Item Class ID
        public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

        protected int? _ItemClassID;
        [PXDBInt]
        [PXUIField(DisplayName = "Item Class")]
        [PXDimensionSelector(INItemClass.Dimension, typeof(Search<INItemClass.itemClassID>), typeof(INItemClass.itemClassCD), DescriptionField = typeof(INItemClass.descr))]
        public virtual int? ItemClassID
        {
            get { return this._ItemClassID; }
            set { this._ItemClassID = value; }
            }

        #endregion
        #region SiteID

        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }


        protected Int32? _SiteID;

        [Site]
        [PXForeignReference(typeof(Field<siteID>.IsRelatedTo<INSite.siteID>))]
        public virtual Int32? SiteID
        {
            get { return this._SiteID; }
            set { this._SiteID = value; }
        }

        #endregion
        #region BackFlush 
        public abstract class backFlush : PX.Data.BQL.BqlBool.Field<backFlush> { }

        protected Boolean? _BackFlush;
        [PXDBBool]
        [PXDefault(false,
            typeof (Search<AMWC.bflushMatl, Where<AMWC.wcID, Equal<Current<AMEstimateOper.workCenterID>>>>))]
        [PXUIField(DisplayName = "Backflush")]
        public virtual Boolean? BackFlush
        {
            get
            {

                return this._BackFlush;
            }
            set
            {
                this._BackFlush = value;
            }
        }
        #endregion
        #region Qty Req

        public abstract class qtyReq : PX.Data.BQL.BqlDecimal.Field<qtyReq> { }


        protected Decimal? _QtyReq;

        [EstimateDBQuantity(typeof(AMEstimateMatl.uOM), typeof(AMEstimateMatl.baseQtyReq), typeof(AMEstimateMatl.inventoryID),
            HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal, "1.0000")]
        [PXUIField(DisplayName = "Qty Required")]
        public virtual Decimal? QtyReq
        {
            get { return this._QtyReq; }
            set { this._QtyReq = value; }
            }

        #endregion
        #region Base Qty Req

        public abstract class baseQtyReq : PX.Data.BQL.BqlDecimal.Field<baseQtyReq> { }

        protected Decimal? _BaseQtyReq;

        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Base Qty Req", Enabled = false, Visible = false)]
        public virtual Decimal? BaseQtyReq
        {
            get { return this._BaseQtyReq; }
            set { this._BaseQtyReq = value; }
            }

        #endregion
        #region UOM

        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }


        protected String _UOM;
        [PXDefault(typeof(Search<INItemClass.baseUnit, Where<INItemClass.itemClassID, Equal<Current<AMEstimateMatl.itemClassID>>>>))]
        [INUnit(typeof(AMEstimateMatl.inventoryID))]
        public virtual String UOM
        {
            get { return this._UOM; }
            set { this._UOM = value; }
            }

        #endregion
        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }


        protected Decimal? _UnitCost;

        [PXDBPriceCost]
        [PXUIField(DisplayName = "Unit Cost")]
        [PXDefault]
        [MatlUnitCostDefault(
            typeof(AMEstimateMatl.inventoryID),
            typeof(AMEstimateMatl.siteID),
            typeof(AMEstimateMatl.uOM),
            typeof(AMEstimateItem),
            typeof(AMEstimateItem.siteID))]
        [PXFormula(typeof(Default<AMEstimateMatl.inventoryID, AMEstimateMatl.siteID, AMEstimateMatl.uOM>))]
        public virtual Decimal? UnitCost
        {
            get { return this._UnitCost; }
            set { this._UnitCost = value; }
            }
        #endregion
        #region MaterialType

        public abstract class materialType : PX.Data.BQL.BqlInt.Field<materialType> { }


        protected int? _MaterialType;

        [PXDBInt]
        [PXDefault(AMMaterialType.Regular)]
        [PXUIField(DisplayName = "Material Type")]
        [AMMaterialType.List]
        public virtual int? MaterialType
        {
            get { return this._MaterialType; }
            set { this._MaterialType = value; }
        }

        #endregion
        #region PhantomRouting

        public abstract class phantomRouting : PX.Data.BQL.BqlInt.Field<phantomRouting> { }


        protected int? _PhantomRouting;

        [PXDBInt]
        [PXDefault(PhantomRoutingOptions.Before)]
        [PXUIField(DisplayName = "Phantom Routing", Visible = false)]
        [PhantomRoutingOptions.List]
        public virtual int? PhantomRouting
        {
            get { return this._PhantomRouting; }
            set { this._PhantomRouting = value; }
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
            get { return this._ScrapFactor; }
            set { this._ScrapFactor = value; }
        }

        #endregion
        #region LocationID

        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationID;
        [Location(typeof(AMEstimateMatl.siteID))]
        [PXForeignReference(typeof(CompositeKey<Field<siteID>.IsRelatedTo<INLocation.siteID>, Field<locationID>.IsRelatedTo<INLocation.locationID>>))]
        public virtual Int32? LocationID
        {
            get { return this._LocationID; }
            set { this._LocationID = value; }
        }

        #endregion
        #region Material Oper Cost 
        public abstract class materialOperCost : PX.Data.BQL.BqlDecimal.Field<materialOperCost> { }

        protected Decimal? _MaterialOperCost;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "Total Cost", Enabled = false)]
        [PXFormula(typeof(Mult<AMEstimateMatl.unitCost, AMEstimateMatl.totalQtyRequired>))]
        [PXUnboundFormula(typeof(Switch<Case<Where<AMEstimateMatl.materialType, Equal<AMMaterialType.subcontract>, And<AMEstimateMatl.subcontractSource, NotEqual<AMSubcontractSource.vendorSupplied>>>,
            Mult<AMEstimateMatl.unitCost, AMEstimateMatl.totalQtyRequired>>, decimal0>), typeof(SumCalc<AMEstimateOper.subcontractUnitCost>))]
        [PXUnboundFormula(typeof(Switch<Case<Where<AMEstimateMatl.materialType, Equal<AMMaterialType.subcontract>, And<AMEstimateMatl.subcontractSource, Equal<AMSubcontractSource.vendorSupplied>>>,
            Mult<AMEstimateMatl.unitCost, AMEstimateMatl.totalQtyRequired>>, decimal0>), typeof(SumCalc<AMEstimateOper.referenceMaterialCost>))]
        [PXUnboundFormula(typeof(Switch<Case<Where<AMEstimateMatl.materialType, NotEqual<AMMaterialType.subcontract>>,
            Mult<AMEstimateMatl.unitCost, AMEstimateMatl.totalQtyRequired>>, decimal0>), typeof(SumCalc<AMEstimateOper.materialUnitCost>))]
        public virtual Decimal? MaterialOperCost
        {
            get { return this._MaterialOperCost; }
            set { this._MaterialOperCost = value; }
        }
        #endregion
        #region CreatedByID

        public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }


        protected Guid? _CreatedByID;

        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID
        {
            get { return this._CreatedByID; }
            set { this._CreatedByID = value; }
        }

        #endregion
        #region CreatedByScreenID

        public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }


        protected String _CreatedByScreenID;

        [PXDBCreatedByScreenID()]
        public virtual String CreatedByScreenID
        {
            get { return this._CreatedByScreenID; }
            set { this._CreatedByScreenID = value; }
        }

        #endregion
        #region CreatedDateTime

        public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }


        protected DateTime? _CreatedDateTime;

        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime
        {
            get { return this._CreatedDateTime; }
            set { this._CreatedDateTime = value; }
        }

        #endregion
        #region LastModifiedByID

        public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }


        protected Guid? _LastModifiedByID;

        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID
        {
            get { return this._LastModifiedByID; }
            set { this._LastModifiedByID = value; }
        }

        #endregion
        #region LastModifiedByScreenID

        public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }


        protected String _LastModifiedByScreenID;

        [PXDBLastModifiedByScreenID()]
        public virtual String LastModifiedByScreenID
        {
            get { return this._LastModifiedByScreenID; }
            set { this._LastModifiedByScreenID = value; }
        }

        #endregion
        #region LastModifiedDateTime

        public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }


        protected DateTime? _LastModifiedDateTime;

        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime
        {
            get { return this._LastModifiedDateTime; }
            set { this._LastModifiedDateTime = value; }
        }

        #endregion
        #region LineNbr
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }

        //Required for ISortOrder
        [PXInt]
        [PXUIField(DisplayName = "Line Nbr. 2", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
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
        #region tstamp

        public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }


        protected Byte[] _tstamp;

        [PXDBTimestamp()]
        public virtual Byte[] tstamp
        {
            get { return this._tstamp; }
            set { this._tstamp = value; }
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
        #region QtyRoundUp
        public abstract class qtyRoundUp : PX.Data.BQL.BqlBool.Field<qtyRoundUp> { }

        [PXDBBool]
        [PXDefault(false, typeof(Search<InventoryItemExt.aMQtyRoundUp, Where<InventoryItem.inventoryID,
            Equal<Current<AMEstimateMatl.inventoryID>>>>))]
        [PXUIField(DisplayName = "Qty Round Up", Visible = false)]
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
        [PXFormula(typeof(Mult<AMEstimateMatl.qtyReq, Add<decimal1, AMEstimateMatl.scrapFactor>>))]
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
        #region Total Qty Required
        public abstract class totalQtyRequired : PX.Data.BQL.BqlDecimal.Field<totalQtyRequired> { }

        protected Decimal? _TotalQtyRequired;
        [PXDBQuantity(typeof(AMEstimateMatl.uOM), typeof(AMEstimateMatl.baseTotalQtyRequired), HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Total Required", Enabled = false)]
        [PXFormula(typeof(Switch<Case<Where<AMEstimateMatl.qtyRoundUp, Equal<True>>,
                MathCeiling<Switch<Case<Where<AMEstimateMatl.batchSize, Equal<decimal0>>, AMEstimateMatl.qtyReqWithScrap>,
                    Mult<AMEstimateMatl.qtyReqWithScrap, Div<Parent<AMEstimateOper.baseOrderQty>, AMEstimateMatl.batchSize>>>>>,
            Case<Where<AMEstimateMatl.qtyRoundUp, NotEqual<True>>,
                Switch<Case<Where<AMEstimateMatl.batchSize, Equal<decimal0>>, AMEstimateMatl.qtyReqWithScrap>,
                    Mult<AMEstimateMatl.qtyReqWithScrap, Div<Parent<AMEstimateOper.baseOrderQty>, AMEstimateMatl.batchSize>>>>>))]
        public virtual Decimal? TotalQtyRequired
        {
            get { return this._TotalQtyRequired; }
            set { this._TotalQtyRequired = value; }
        }
        #endregion
        #region Base Total Qty Required
        public abstract class baseTotalQtyRequired : PX.Data.BQL.BqlDecimal.Field<baseTotalQtyRequired> { }

        protected Decimal? _BaseTotalQtyRequired;
        [PXDBQuantity]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Base Total Required", Enabled = false, Visible = false)]
        public virtual Decimal? BaseTotalQtyRequired
        {
            get { return this._BaseTotalQtyRequired; }
            set { this._BaseTotalQtyRequired = value; }
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

#pragma warning disable PX1031 // DACs cannot contain instance methods
        /// <summary>
        /// Makes a copy of the object excluding the created by, last mod by, and timestamps fields
        /// </summary>
        /// <returns>new object with copied values</returns>
        [Obsolete("Use PXCache<>.CreateCopy. " + InternalMessages.MethodIsObsoleteAndWillBeRemoved2020R2)]
        public virtual AMEstimateMatl Copy()
#pragma warning restore PX1031 // DACs cannot contain instance methods
        {
            return new AMEstimateMatl
            {
                EstimateID = this.EstimateID,
                RevisionID = this.RevisionID,
                OperationID = this.OperationID,
                LineID = this.LineID,
                SortOrder = this.SortOrder,
                InventoryID = this.InventoryID,
                InventoryCD = this.InventoryCD,
                IsNonInventory = this.IsNonInventory,
                ItemDesc = this.ItemDesc,
                SubItemID = this.SubItemID,
                ItemClassID = this.ItemClassID,
                BackFlush = this.BackFlush,
                QtyReq = this.QtyReq,
                BaseQtyReq = this.BaseQtyReq,
                UOM = this.UOM,
                UnitCost = this.UnitCost,
                MaterialType = this.MaterialType,
                PhantomRouting = this.PhantomRouting,
                SiteID = this.SiteID,
                ScrapFactor = this.ScrapFactor,
                BatchSize = this.BatchSize,
                QtyRoundUp = this.QtyRoundUp,
                TotalQtyRequired = this.TotalQtyRequired,
                LocationID = this.LocationID,
                MaterialOperCost = this.MaterialOperCost,
                SubcontractSource = this.SubcontractSource
            };
        }
    }
}