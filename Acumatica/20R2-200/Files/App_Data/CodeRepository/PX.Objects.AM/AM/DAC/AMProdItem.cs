using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.AR;
using PX.Objects.GL;
using PX.Objects.CS;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.PM;
using PX.Objects.SO;
using PX.TM;
using PX.Objects.AM.GraphExtensions;
using PX.Objects.AM.Attributes;
using PX.Objects.AM.CacheExtensions;

namespace PX.Objects.AM
{
    [Serializable]
    [PXCacheName(Messages.ProductionItem)]
    [PXPrimaryGraph(typeof(ProdMaint))]
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class AMProdItem : IBqlTable, ILSPrimary, INotable, IProdOrder
    {
	    internal string DebuggerDisplay => $"OrderType = {OrderType}, ProdOrdID = {ProdOrdID}, StatusID = {StatusID}";

        #region Keys

        public class PK : PrimaryKeyOf<AMProdItem>.By<orderType, prodOrdID>
        {
            public static AMProdItem Find(PXGraph graph, string orderType, string prodOrdID) 
                => FindBy(graph, orderType, prodOrdID);
            public static AMProdItem FindDirty(PXGraph graph, string orderType, string prodOrdID)
                => PXSelect<AMProdItem,
                        Where<orderType, Equal<Required<orderType>>,
                            And<prodOrdID, Equal<Required<prodOrdID>>>>>
                    .SelectWindowed(graph, 0, 1, orderType, prodOrdID);
        }

        public static class FK
        {
            public class OrderType : AMOrderType.PK.ForeignKeyOf<AMProdItem>.By<orderType> { }
            public class InventoryItem : PX.Objects.IN.InventoryItem.PK.ForeignKeyOf<AMProdItem>.By<inventoryID> { }
            public class Site : PX.Objects.IN.INSite.PK.ForeignKeyOf<AMProdItem>.By<siteID> { }
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
        #region Delete

        public abstract class delete : PX.Data.BQL.BqlBool.Field<delete> { }

        protected bool? _Delete = false;
        [PXBool]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Delete")]
        public virtual bool? Delete
        {
            get
            {
                return _Delete;
            }
            set
            {
                _Delete = value;
            }
        }
        #endregion
        #region OrderType
        public abstract class orderType : PX.Data.BQL.BqlString.Field<orderType> { }

        protected String _OrderType;
        [PXDefault(typeof(AMPSetup.defaultOrderType))]
        [AMOrderTypeField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible)]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
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
        [ProductionNbr(IsKey = true, Required = true, Visibility = PXUIVisibility.SelectorVisible)]
        [ProductionOrderSelector(typeof(AMProdItem.orderType), true)]
        [ProductionNumbering]
        [PXDefault]
        [PX.Data.EP.PXFieldDescription]
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
        #region Function
        public abstract class function : PX.Data.BQL.BqlInt.Field<function> { }

        protected int? _Function;
        [PXDBInt]
        [PXDefault(OrderTypeFunction.Regular, typeof(Search<AMOrderType.function, Where<AMOrderType.orderType, Equal<Current<AMProdItem.orderType>>>>))]
        [PXUIField(DisplayName = "Function", Enabled = false, Visible = false)]
        [OrderTypeFunction.List]
        public virtual int? Function
        {
            get
            {
                return this._Function;
            }
            set
            {
                this._Function = value;
            }
        }
        #endregion
        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

        protected Int32? _InventoryID;
        [StockItem(Visibility = PXUIVisibility.SelectorVisible)]
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
        #region ProdDate
        public abstract class prodDate : PX.Data.BQL.BqlDateTime.Field<prodDate> { }

        protected DateTime? _ProdDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Order Date", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual DateTime? ProdDate
        {
            get
            {
                return this._ProdDate;
            }
            set
            {
                this._ProdDate = value;
            }
        }
        #endregion
        #region StatusID
        public abstract class statusID : PX.Data.BQL.BqlString.Field<statusID> { }

        protected String _StatusID;
        [PXDBString(1, IsFixed = true)]
        [PXDefault(ProductionOrderStatus.Planned)]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }

        protected Int32? _SiteID;
        [PXDefault(typeof(Search<InventoryItem.dfltSiteID, Where<InventoryItem.inventoryID, Equal<Current<AMProdItem.inventoryID>>>>), PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [Site(Visibility=PXUIVisibility.SelectorVisible)]
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
        #region QtytoProd
        public abstract class qtytoProd : PX.Data.BQL.BqlDecimal.Field<qtytoProd> { }

        protected Decimal? _QtytoProd;
        [PXDBQuantity(typeof(AMProdItem.uOM), typeof(AMProdItem.baseQtytoProd), HandleEmptyKey = true)]
        [PXDefault(TypeCode.Decimal, "1.0000")]
        [PXUIField(DisplayName = "Qty to Produce")]
        public virtual Decimal? QtytoProd
        {
            get
            {
                return this._QtytoProd;
            }
            set
            {
                this._QtytoProd = value;
            }
        }
        #endregion
        #region BaseQtytoProd
        public abstract class baseQtytoProd : PX.Data.BQL.BqlDecimal.Field<baseQtytoProd> { }

        protected Decimal? _BaseQtytoProd;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BaseQtytoProd
        {
            get
            {
                return this._BaseQtytoProd;
            }
            set
            {
                this._BaseQtytoProd = value;
            }
        }
        #endregion
        #region UOM
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }

        protected String _UOM;
        [PXDefault(typeof(Search<InventoryItem.baseUnit, Where<InventoryItem.inventoryID, Equal<Current<AMProdItem.inventoryID>>>>))]
        [INUnit(typeof(AMProdItem.inventoryID))]
        [PXFormula(typeof(Default<AMProdItem.inventoryID>))]
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
        #region QtyComplete
        public abstract class qtyComplete : PX.Data.BQL.BqlDecimal.Field<qtyComplete> { }

        protected Decimal? _QtyComplete;
        [PXUIField(DisplayName = "Qty Complete", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXDBQuantity(typeof(AMProdItem.uOM), typeof(AMProdItem.baseQtyComplete), HandleEmptyKey = true)]
        public virtual Decimal? QtyComplete
        {
            get
            {
                return this._QtyComplete;
            }
            set
            {
                this._QtyComplete = value;
            }
        }
        #endregion
	    #region BaseQtyComplete
	    public abstract class baseQtyComplete : PX.Data.BQL.BqlDecimal.Field<baseQtyComplete> { }

	    protected Decimal? _BaseQtyComplete;
	    [PXDBQuantity]
	    [PXDefault(TypeCode.Decimal, "0.0")]
	    public virtual Decimal? BaseQtyComplete
	    {
	        get
	        {
	            return this._BaseQtyComplete;
	        }
	        set
	        {
	            this._BaseQtyComplete = value;
	        }
	    }
	    #endregion
        #region BaseQtyRemaining

        public abstract class baseQtyRemaining : PX.Data.BQL.BqlDecimal.Field<baseQtyRemaining> { }

        protected Decimal? _BaseQtyRemaining;
        [PXQuantity]
        [PXFormula(typeof(Switch<Case<Where<Current<AMPSetup.inclScrap>, Equal<True>>, SubNotLessThanZero<AMProdItem.baseQtytoProd, Add<AMProdItem.baseQtyComplete, AMProdItem.baseQtyScrapped>>>,
            SubNotLessThanZero<AMProdItem.baseQtytoProd, AMProdItem.baseQtyComplete>>))]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Base Qty Remaining", Enabled = false)]
        public virtual Decimal? BaseQtyRemaining
        {
            get
            {
                return _BaseQtyRemaining;
            }
            set
            {
                _BaseQtyRemaining = value;
            }
        }
        #endregion
        #region QtyRemaining

        public abstract class qtyRemaining : PX.Data.BQL.BqlDecimal.Field<qtyRemaining> { }

        protected Decimal? _QtyRemaining;
        /// <summary>
        /// Quantity remaining to be completed on the production order
        /// </summary>
        [PXQuantity]
        [PXFormula(typeof(Switch<Case<Where<Current<AMPSetup.inclScrap>, Equal<True>>, SubNotLessThanZero<AMProdItem.qtytoProd, Add<AMProdItem.qtyComplete, AMProdItem.qtyScrapped>>>,
            SubNotLessThanZero<AMProdItem.qtytoProd, AMProdItem.qtyComplete>>))]
        [PXDefault(TypeCode.Decimal, "0.0000", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Qty Remaining", Enabled = false)]
        public virtual Decimal? QtyRemaining
        {
            get
            {
                return _QtyRemaining;
            }
            set
            {
                _QtyRemaining = value;
            }
        }
        #endregion
        #region QtyScrapped
        public abstract class qtyScrapped : PX.Data.BQL.BqlDecimal.Field<qtyScrapped> { }

        protected Decimal? _QtyScrapped;
        [PXUIField(DisplayName = "Qty Scrapped", Enabled = false)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXDBQuantity(typeof(AMProdItem.uOM), typeof(AMProdItem.baseQtyScrapped), HandleEmptyKey = true)]
        public virtual Decimal? QtyScrapped
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
        #region BaseQtyScrapped
        public abstract class baseQtyScrapped : PX.Data.BQL.BqlDecimal.Field<baseQtyScrapped> { }

        protected Decimal? _BaseQtyScrapped;
        [PXDBDecimal(6)]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual Decimal? BaseQtyScrapped
        {
            get
            {
                return this._BaseQtyScrapped;
            }
            set
            {
                this._BaseQtyScrapped = value;
            }
        }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }

        protected Int32? _LocationID;
        [MfgLocationAvail(typeof(AMProdItem.inventoryID), typeof(AMProdItem.subItemID), typeof(AMProdItem.siteID), false, true, KeepEntry = false, ResetEntry = true)]
        [PXDefault]
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
	    #region SchedulingMethod
	    public abstract class schedulingMethod : PX.Data.BQL.BqlString.Field<schedulingMethod> { }

	    protected string _SchedulingMethod;
	    [PXDBString(1, IsFixed = true)]
	    [PXDefault(ScheduleMethod.StartOn)]
	    [PXUIField(DisplayName = "Scheduling Method")]
	    [ScheduleMethod.List]
	    public virtual string SchedulingMethod
	    {
	        get
	        {
	            return this._SchedulingMethod;
	        }
	        set
	        {
	            this._SchedulingMethod = value;
	        }
	    }
	    #endregion
        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }

        protected DateTime? _StartDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Start Date")]
        public virtual DateTime? StartDate
        {
            get
            {
                return this._StartDate;
            }
            set
            {
                this._StartDate = value;
            }
        }
        #endregion
        #region EndDate
        public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }

        protected DateTime? _EndDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "End Date")]
        [UpdateChildOnFieldUpdated(typeof(AMProdItemSplit), typeof(AMProdItemSplit.tranDate))]
        public virtual DateTime? EndDate
        {
            get
            {
                return this._EndDate;
            }
            set
            {
                this._EndDate = value;
            }
        }
        #endregion
        #region ConstDate
        public abstract class constDate : PX.Data.BQL.BqlDateTime.Field<constDate> { }

        protected DateTime? _ConstDate;
        [PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "Constraint")]
        public virtual DateTime? ConstDate
        {
            get
            {
                return this._ConstDate;
            }
            set
            {
                this._ConstDate = value;
            }
        }
        #endregion
        #region Hold
        public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }

        protected Boolean? _Hold;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Boolean? Hold
        {
            get
            {
                return this._Hold;
            }
            set
            {
                this._Hold = value;
            }
        }
        #endregion
        #region FMLTime
        public abstract class fMLTime : PX.Data.BQL.BqlBool.Field<fMLTime> { }

        protected Boolean? _FMLTime;
        [PXDBBool]
        [PXDefault(typeof(AMPSetup.fMLTime))]
        [PXUIField(DisplayName = "Use Fixed Mfg Lead Times for Order Dates")]
        public virtual Boolean? FMLTime
        {
            get
            {
                return this._FMLTime;
            }
            set
            {
                this._FMLTime = value;
            }
        }
        #endregion
        #region FMLTMRPOrdorOP
        public abstract class fMLTMRPOrdorOP : PX.Data.BQL.BqlBool.Field<fMLTMRPOrdorOP> { }

        protected Boolean? _FMLTMRPOrdorOP;
        [PXDBBool]
        [PXDefault(typeof(AMPSetup.fMLTMRPOrdorOP))]
        [PXUIField(DisplayName = "Use Order Start Date for MRP")]
        public virtual Boolean? FMLTMRPOrdorOP
        {
            get
            {
                return this._FMLTMRPOrdorOP;
            }
            set
            {
                this._FMLTMRPOrdorOP = value;
            }
        }
        #endregion
        #region SchPriority
        /// <summary>
        /// Scheduling/dispatch priority from 1 (high) to 10 (low)
        /// </summary>
        public abstract class schPriority : PX.Data.BQL.BqlShort.Field<schPriority> { }

        protected Int16? _SchPriority;
        /// <summary>
        /// Scheduling/dispatch priority from 1 (high) to 10 (low)
        /// </summary>
        [PXDBShort(MinValue = 1, MaxValue = 10)]
        [PXDefault((short)5)]
        [PXUIField(DisplayName = "Dispatch Priority")]
        public virtual Int16? SchPriority
        {
            get
            {
                return this._SchPriority;
            }
            set
            {
                this._SchPriority = value;
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
        #region DetailSource
        /// <summary>
        /// Indicates where the production detail source comes from.
        /// </summary>
        public abstract class detailSource : PX.Data.BQL.BqlInt.Field<detailSource> { }

        protected Int32? _DetailSource;
        /// <summary>
        /// Indicates where the production detail source comes from.
        /// </summary>
        [PXDBInt]
        [PXDefault(ProductionDetailSource.BOM)]
        [PXUIField(DisplayName = "Source")]
        [ProductionDetailSource.List]
        public virtual Int32? DetailSource
        {
            get { return this._DetailSource; }
            set { this._DetailSource = value; }
        }
        #endregion
        #region Is Configurable
        public abstract class isConfigurable : PX.Data.BQL.BqlBool.Field<isConfigurable> { }

        /// <summary>
        /// Indicates whether it's possible to configure this production item.
        /// </summary>
        [PXBool]
        [PXUIField(DisplayName = "Is Configurable", Visibility = PXUIVisibility.Invisible, Visible = false, Enabled = false)]
        [PXFormula(typeof(Switch<
            Case<Where<Selector<AMProdItem.inventoryID, InventoryItemExt.aMConfigurationID>, 
                IsNotNull>, True>, 
            False>))]
        public virtual Boolean? IsConfigurable
        {
            get;
            set;
        }
        #endregion
        #region BOMID
        public abstract class bOMID : PX.Data.BQL.BqlString.Field<bOMID> { }

		protected string _BOMID;
		[BomID]
		[BOMIDActiveSelector(typeof(Search2<AMBomItemActive2.bOMID,
		    LeftJoin<InventoryItem, On<AMBomItemActive2.inventoryID, Equal<InventoryItem.inventoryID>>>,
		    Where<AMBomItemActive2.inventoryID, Equal<Current<AMProdItem.inventoryID>>,
		    And<Where<AMBomItemActive2.subItemID, Equal<Current<AMProdItem.subItemID>>,
		        Or<AMBomItemActive2.subItemID, IsNull>>>>>))]
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
        [PXRestrictor(typeof(Where<AMBomItem.status, Equal<AMBomStatus.active>>), Messages.BomRevisionIsNotActive, typeof(AMBomItem.bOMID), typeof(AMBomItem.revisionID), CacheGlobal = true)]
        [PXSelector(typeof(Search<AMBomItem.revisionID,
                Where<AMBomItem.bOMID, Equal<Current<AMProdItem.bOMID>>>>)
            , typeof(AMBomItem.revisionID)
            , typeof(AMBomItem.descr)
            , typeof(AMBomItem.effStartDate)
            , typeof(AMBomItem.effEndDate)
            , DescriptionField = typeof(AMBomItem.descr))]
	    [PXForeignReference(typeof(CompositeKey<Field<AMProdItem.bOMID>.IsRelatedTo<AMBomItem.bOMID>, Field<AMProdItem.bOMRevisionID>.IsRelatedTo<AMBomItem.revisionID>>))]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIRequired(typeof(IIf<Where<AMProdItem.detailSource, Equal<ProductionDetailSource.bom>, And<AMProdItem.bOMID, IsNotNull>>, True, False>))]
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
        #region BOMEffDate
        public abstract class bOMEffDate : PX.Data.BQL.BqlDateTime.Field<bOMEffDate> { }

		protected DateTime? _BOMEffDate;
		[PXDBDate]
        [PXDefault(typeof(AccessInfo.businessDate), PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "Source Date", Visibility = PXUIVisibility.Invisible)]
		public virtual DateTime? BOMEffDate
		{
			get
			{
				return this._BOMEffDate;
			}
			set
			{
				this._BOMEffDate = value;
			}
		}
		#endregion
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

		protected Int32? _CustomerID;
        [Customer(Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(Customer.acctName), Enabled = true)]
        [PXForeignReference(typeof(Field<customerID>.IsRelatedTo<PX.Objects.CR.BAccount.bAccountID>))]
        public virtual Int32? CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this._CustomerID = value;
			}
		}
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		protected String _Descr;
		[PXDBString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "Order Description")]
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
		#region MultDiv
		public abstract class multDiv : PX.Data.BQL.BqlString.Field<multDiv> { }

		protected String _MultDiv;
		[PXDBString(1, IsFixed = true)]
		[PXDefault("M")]
		[PXUIField(DisplayName = "MultDiv")]
		public virtual String MultDiv
		{
			get
			{
				return this._MultDiv;
			}
			set
			{
				this._MultDiv = value;
			}
		}
		#endregion
        #region NoteID
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        protected Guid? _NoteID;
        [PXSearchable(PX.Objects.SM.SearchCategory.IN, Messages.ProductionSearchableTitleDocument, new[] { typeof(AMProdItem.orderType), typeof(AMProdItem.prodOrdID) },
            new Type[] { typeof(AMProdItem.prodOrdID), typeof(InventoryItem.inventoryCD), typeof(AMProdItem.descr), typeof(Customer.acctCD), typeof(Customer.acctName), typeof(AMProdItem.ordNbr), typeof(AMProdItem.estimateID) },
            NumberFields = new Type[] { typeof(AMProdItem.prodOrdID) },
            Line1Format = "{0}{1:d}{2:d}{3}{4}{5}{6}", Line1Fields = new Type[] { typeof(InventoryItem.inventoryID), typeof(AMProdItem.startDate), typeof(AMProdItem.endDate), typeof(AMProdItem.statusID), typeof(Customer.acctCD), typeof(Customer.acctName), typeof(AMProdItem.ordNbr) },
            Line2Format = "{0}{1}{2}", Line2Fields = new Type[] { typeof(AMProdItem.descr), typeof(AMProdItem.bOMID), typeof(AMProdItem.estimateID) },
            SelectForFastIndexing = typeof(Select2<AMProdItem,
                InnerJoin<InventoryItem, On<InventoryItem.inventoryID, Equal<AMProdItem.inventoryID>>,
                LeftJoin<Customer, On<Customer.bAccountID, Equal<AMProdItem.customerID>>>>>)
        )]
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
        #region OrdLineRef
        public abstract class ordLineRef : PX.Data.BQL.BqlInt.Field<ordLineRef> { }

        protected int? _OrdLineRef;
        [PXDBInt]
        [PXUIField(DisplayName = "SO Line Nbr.", Enabled = false)]
        public virtual int? OrdLineRef
        {
            get
            {
                return this._OrdLineRef;
            }
            set
            {
                this._OrdLineRef = value;
            }
        }
        #endregion
        #region OrdTypeRef
        public abstract class ordTypeRef : PX.Data.BQL.BqlString.Field<ordTypeRef> { }

        protected String _OrdTypeRef;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "SO Order Type", Enabled = false)]
        public virtual String OrdTypeRef
        {
            get
            {
                return this._OrdTypeRef;
            }
            set
            {
                this._OrdTypeRef = value;
            }
        }
        #endregion
		#region OrdNbr
		public abstract class ordNbr : PX.Data.BQL.BqlString.Field<ordNbr> { }

		protected String _OrdNbr;
        [PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
        [PXUIField(DisplayName = "SO Order Nbr", Enabled = false)]
        [PXSelector(typeof(Search<SOOrder.orderNbr, Where<SOOrder.orderType, Equal<Current<AMProdItem.ordTypeRef>>>>))]
		public virtual String OrdNbr
		{
			get
			{
				return this._OrdNbr;
			}
			set
			{
				this._OrdNbr = value;
			}
		}
        #endregion
        #region ParentOrderType
        public abstract class parentOrderType : PX.Data.BQL.BqlString.Field<parentOrderType> { }

        protected String _ParentOrderType;
        [AMOrderTypeField(DisplayName = "Parent Order Type")]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        public virtual String ParentOrderType
        {
            get
            {
                return this._ParentOrderType;
            }
            set
            {
                this._ParentOrderType = value;
            }
        }
        #endregion
        #region ParentOrdID
        public abstract class parentOrdID : PX.Data.BQL.BqlString.Field<parentOrdID> { }

		protected String _ParentOrdID;
        [ProductionNbr(DisplayName = "Parent Order", Visibility = PXUIVisibility.Undefined)]
        [ProductionOrderSelector(typeof(AMProdItem.parentOrderType), includeAll: true)]
        public virtual String ParentOrdID
		{
			get
			{
				return this._ParentOrdID;
			}
			set
			{
				this._ParentOrdID = value;
			}
		}
		#endregion
		#region PerClose
		public abstract class perClose : PX.Data.BQL.BqlString.Field<perClose> { }

		protected String _PerClose;
		[PXDBString(6, IsFixed = true)]
		[PXUIField(DisplayName = "Period Closed")]
		public virtual String PerClose
		{
			get
			{
				return this._PerClose;
			}
			set
			{
				this._PerClose = value;
			}
		}
		#endregion
		#region PerEnt
		public abstract class perEnt : PX.Data.BQL.BqlString.Field<perEnt> { }

		protected String _PerEnt;
        [FinPeriodID(typeof(AMProdItem.prodDate))]
#pragma warning disable PX1030 // Consider setting PersistingCheck to Nothing or replacing PXDefault with PXUnboundDefault
        [PXDefault] //Avoid null column sql error when periods are not setup
#pragma warning restore PX1030
        [PXUIField(DisplayName = "Entered Period")]
		public virtual String PerEnt
		{
			get
			{
				return this._PerEnt;
			}
			set
			{
				this._PerEnt = value;
			}
		}
        #endregion
        #region ProductOrderType
        public abstract class productOrderType : PX.Data.BQL.BqlString.Field<productOrderType> { }

        protected String _ProductOrderType;
        [AMOrderTypeField(DisplayName = "Product Order Type")]
        [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
        [AMOrderTypeSelector]
        public virtual String ProductOrderType
        {
            get
            {
                return this._ProductOrderType;
            }
            set
            {
                this._ProductOrderType = value;
            }
        }
        #endregion
        #region ProductOrdID
        public abstract class productOrdID : PX.Data.BQL.BqlString.Field<productOrdID> { }

		protected String _ProductOrdID;
        [ProductionNbr(DisplayName = "Product Order", Visibility = PXUIVisibility.Undefined)]
        [ProductionOrderSelector(typeof(AMProdItem.productOrderType), includeAll: true)]
        public virtual String ProductOrdID
		{
			get
			{
				return this._ProductOrdID;
			}
			set
			{
				this._ProductOrdID = value;
			}
		}
		#endregion
		#region RelDate
		public abstract class relDate : PX.Data.BQL.BqlDateTime.Field<relDate> { }

		protected DateTime? _RelDate;
		[PXDBDate]
		[PXUIField(DisplayName = "Release Date")]
		public virtual DateTime? RelDate
		{
			get
			{
				return this._RelDate;
			}
			set
			{
				this._RelDate = value;
			}
		}
        #endregion
        #region RevisionNbr
	    [Obsolete("Use bOMRevisionID", true)]
        public abstract class revisionNbr : PX.Data.BQL.BqlString.Field<revisionNbr> { }

		protected String _RevisionNbr;
	    [Obsolete("Use BOMRevisionID", true)]
        [PXString(10, IsFixed = true)]
		[PXUIField(DisplayName = "BOM Revision Obsolete", Visibility = PXUIVisibility.Invisible)]
        public virtual String RevisionNbr
		{
			get
			{
				return this._RevisionNbr;
			}
			set
			{
				this._RevisionNbr = value;
			}
		}
		#endregion
		#region WIPAcctID
		public abstract class wIPAcctID : PX.Data.BQL.BqlInt.Field<wIPAcctID> { }

        protected Int32? _WIPAcctID;	
		[PXDefault]
        [Account(DisplayName ="WIP Account")]
		public virtual Int32? WIPAcctID
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
		#region WIPComp
		public abstract class wIPComp : PX.Data.BQL.BqlDecimal.Field<wIPComp> { }

		protected Decimal? _WIPComp;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "MFG to Inventory", Enabled = false)]
		public virtual Decimal? WIPComp
		{
			get
			{
				return this._WIPComp;
			}
			set
			{
				this._WIPComp = value;
			}
		}
		#endregion
		#region WIPSubID
		public abstract class wIPSubID : PX.Data.BQL.BqlInt.Field<wIPSubID> { }

		protected Int32? WipSubId;
		[PXDefault]
        [SubAccount(DisplayName = "WIP Subaccount")]
		public virtual Int32? WIPSubID
		{
			get
			{
				return this.WipSubId;
			}
			set
			{
				this.WipSubId = value;
			}
		}
		#endregion
		#region WIPTotal
		public abstract class wIPTotal : PX.Data.BQL.BqlDecimal.Field<wIPTotal> { }

		protected Decimal? _WIPTotal;
        [PXDBBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0000")]
        [PXUIField(DisplayName = "WIP Total", Enabled = false)]
		public virtual Decimal? WIPTotal
		{
			get
			{
				return this._WIPTotal;
			}
			set
			{
				this._WIPTotal = value;
			}
		}
		#endregion
        #region WIPVarianceAcctID
        public abstract class wIPVarianceAcctID : PX.Data.BQL.BqlInt.Field<wIPVarianceAcctID> { }

        protected Int32? _WIPVarianceAcctID;
        [PXDefault]        
        [Account(DisplayName ="WIP Variance Account")]
        public virtual Int32? WIPVarianceAcctID
        {
            get
            {
                return this._WIPVarianceAcctID;
            }
            set
            {
                this._WIPVarianceAcctID = value;
            }
        }
        #endregion
        #region WIPVarianceSubID
        public abstract class wIPVarianceSubID : PX.Data.BQL.BqlInt.Field<wIPVarianceSubID> { }

        protected Int32? _WIPVarianceSubID;
        [PXDefault]
        [SubAccount(DisplayName = "WIP Variance Subaccount")]
        public virtual Int32? WIPVarianceSubID
        {
            get
            {
                return this._WIPVarianceSubID;
            }
            set
            {
                this._WIPVarianceSubID = value;
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
        #region InvtMult

        public abstract class invtMult : PX.Data.BQL.BqlShort.Field<invtMult> { }

        protected Int16? _InvtMult;
        [PXDBShort]
        [PXFormula(typeof(Switch<
            Case<
                Where<AMProdItem.function, Equal<OrderTypeFunction.disassemble>>, shortMinus1>,
            short1>))]
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
        #region SubItemID

        public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }

        protected Int32? _SubItemID;
        [PXDefault(typeof(Search<InventoryItem.defaultSubItemID,
            Where<InventoryItem.inventoryID, Equal<Current<AMProdItem.inventoryID>>,
            And<InventoryItem.defaultSubItemOnEntry, Equal<boolTrue>>>>),
            PersistingCheck = PXPersistingCheck.NullOrBlank)]
        [SubItem(typeof(AMProdItem.inventoryID))]
        [PXFormula(typeof(Default<AMProdItem.inventoryID>))]
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
        [INLotSerialNbr(typeof(AMProdItem.inventoryID), typeof(AMProdItem.subItemID), typeof(AMProdItem.locationID), PersistingCheck = PXPersistingCheck.Nothing, Visible = false)]
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
        [INExpireDate(typeof(AMProdItem.inventoryID), FieldClass = "LotSerial", PersistingCheck = PXPersistingCheck.Nothing)]
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
        #region TranType

        public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }

        protected String _TranType;
        [PXDBString(3, IsFixed = true)]
        [PXFormula(typeof(Switch<
            Case<
                Where<AMProdItem.function, Equal<OrderTypeFunction.disassemble>>, INTranType.issue>,
            INTranType.receipt>))]
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
        [PXDate]
        [PXUnboundDefault(typeof(AccessInfo.businessDate))]
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
        #region WIPBalance
        public abstract class wIPBalance : PX.Data.BQL.BqlDecimal.Field<wIPBalance> { }

        protected Decimal? _WIPBalance;
        [PXBaseCury]
        [PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
        [PXUIField(DisplayName = "WIP Balance", Enabled = false)]
        [PXDependsOnFields(typeof(AMProdItem.wIPTotal), typeof(AMProdItem.wIPComp))]
        public virtual Decimal? WIPBalance
        {
            get { return this._WIPTotal.GetValueOrDefault() - this._WIPComp.GetValueOrDefault(); }
        }
        #endregion
        #region FirmSchedule
        public abstract class firmSchedule : PX.Data.BQL.BqlBool.Field<firmSchedule> { }

        protected bool? _FirmSchedule;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Firm Schedule", Visible = false, Enabled = false)]
        public virtual bool? FirmSchedule
        {
            get
            {
                return this._FirmSchedule;
            }
            set
            {
                this._FirmSchedule = value;
            }
        }
        #endregion
        #region CostMethod
        public abstract class costMethod : PX.Data.BQL.BqlInt.Field<costMethod> { }

        protected int? _CostMethod;
        [PXDBInt]
        [PXDefault(typeof(Search<AMOrderType.defaultCostMethod, Where<AMOrderType.orderType, Equal<Current<AMProdItem.orderType>>>>))]
        [PXUIField(DisplayName = "Costing Method")]
        [CostMethod.ListAll]
        public virtual int? CostMethod
        {
            get
            {
                return this._CostMethod;
            }
            set
            {
                this._CostMethod = value;
            }
        }
        #endregion
        #region LineCntrAttribute
        public abstract class lineCntrAttribute : PX.Data.BQL.BqlInt.Field<lineCntrAttribute> { }

        protected Int32? _LineCntrAttribute;
        [PXDBInt]
        [PXDefault(0)]
        public virtual Int32? LineCntrAttribute
        {
            get
            {
                return this._LineCntrAttribute;
            }
            set
            {
                this._LineCntrAttribute = value;
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
        #region FirstOperationID
        public abstract class firstOperationID : PX.Data.BQL.BqlInt.Field<firstOperationID> { }

	    protected int? _FirstOperationID;
	    [OperationIDField(DisplayName = "First Operation ID")]
	    [PXSelector(typeof(Search<AMProdOper.operationID,
	            Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>,
	                And<AMProdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>),
	        SubstituteKey = typeof(AMProdOper.operationCD), ValidateValue = false)]
        public virtual int? FirstOperationID
        {
	        get
	        {
	            return this._FirstOperationID;
	        }
	        set
	        {
	            this._FirstOperationID = value;
	        }
	    }
        #endregion
	    #region LastOperationID
	    public abstract class lastOperationID : PX.Data.BQL.BqlInt.Field<lastOperationID> { }

	    protected int? _LastOperationID;
	    [OperationIDField(DisplayName = "Last Operation ID")]
	    [PXSelector(typeof(Search<AMProdOper.operationID,
	            Where<AMProdOper.orderType, Equal<Current<AMProdItem.orderType>>,
	                And<AMProdOper.prodOrdID, Equal<Current<AMProdItem.prodOrdID>>>>>),
	        SubstituteKey = typeof(AMProdOper.operationCD), ValidateValue = false)]
	    public virtual int? LastOperationID
        {
	        get
	        {
	            return this._LastOperationID;
	        }
	        set
	        {
	            this._LastOperationID = value;
	        }
	    }
	    #endregion
        #region BuildProductionBom
        public abstract class buildProductionBom : PX.Data.BQL.BqlBool.Field<buildProductionBom> { }

        protected bool? _BuildProductionBom;
        [PXBool]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? BuildProductionBom
        {
            get
            {
                return _BuildProductionBom.GetValueOrDefault();
            }
            set
            {
                _BuildProductionBom = value;
            }
        }
        #endregion
        #region Reschedule
        public abstract class reschedule : PX.Data.BQL.BqlBool.Field<reschedule> { }

        protected bool? _Reschedule;
        [PXBool]
        [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? Reschedule
        {
            get
            {
                return _Reschedule.GetValueOrDefault();
            }
            set
            {
                _Reschedule = value;
            }
        }
        #endregion
	    #region ProjectID
	    public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

	    protected Int32? _ProjectID;
	    [ProjectDefault]
        [ActiveProjectOrContractForProd(FieldClass = ProjectAttribute.DimensionName)]
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
	    [ActiveOrInPlanningProjectTaskForProd(typeof(AMProdItem.projectID))]
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
	    #region CostCodeID
	    public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }

	    protected Int32? _CostCodeID;
        [CostCodeForProd(null, typeof(taskID), null)]
        public virtual Int32? CostCodeID
	    {
	        get
	        {
	            return this._CostCodeID;
	        }
	        set
	        {
	            this._CostCodeID = value;
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
        #region EstimateID
        public abstract class estimateID : PX.Data.BQL.BqlString.Field<estimateID> { }

        protected String _EstimateID;
        [EstimateID(Visibility = PXUIVisibility.Undefined)]
        [EstimateIDSelectAll]
        public virtual String EstimateID
        {
            get { return this._EstimateID; }
            set { this._EstimateID = value; }
        }
        #endregion
        #region EstimateRevisionID
        public abstract class estimateRevisionID : PX.Data.BQL.BqlString.Field<estimateRevisionID> { }

        protected String _EstimateRevisionID;
        [PXDBString(10, IsUnicode = true, InputMask = ">CCCCCCCCCC")]
        [PXUIField(DisplayName = "Estimate Revision")]
        [PXSelector(typeof(Search<AMEstimateItem.revisionID, Where<AMEstimateItem.estimateID, Equal<Current<AMProdItem.estimateID>>>>))]
        public virtual String EstimateRevisionID
        {
            get { return this._EstimateRevisionID; }
            set { this._EstimateRevisionID = value; }
        }
        #endregion
        #region ProductWorkgroupID
        public abstract class productWorkgroupID : PX.Data.BQL.BqlInt.Field<productWorkgroupID> { }

        protected int? _ProductWorkgroupID;
        [PXDBInt]
        [PXWorkgroupSelector]
        [PXUIField(DisplayName = "Product Workgroup")]
        public virtual int? ProductWorkgroupID
        {
            get
            {
                return this._ProductWorkgroupID;
            }
            set
            {
                this._ProductWorkgroupID = value;
            }
        }
        #endregion
        #region ProductManagerID
        public abstract class productManagerID : PX.Data.BQL.BqlInt.Field<productManagerID> { }

        protected int? _ProductManagerID;
        [Owner(typeof(AMProdItem.productWorkgroupID), DisplayName = "Product Manager")]
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
        #region ScrapOverride
        public abstract class scrapOverride : PX.Data.BQL.BqlBool.Field<scrapOverride> { }

        protected Boolean? _ScrapOverride;
        [PXDBBool]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Scrap Override")]
        public virtual Boolean? ScrapOverride
        {
            get
            {
                return this._ScrapOverride;
            }
            set
            {
                this._ScrapOverride = value;
            }
        }
        #endregion
        #region ScrapSiteID
        public abstract class scrapSiteID : PX.Data.BQL.BqlInt.Field<scrapSiteID> { }

        protected Int32? _ScrapSiteID;
        [PXRestrictor(typeof(Where<INSite.active, Equal<True>>), PX.Objects.IN.Messages.InactiveWarehouse, typeof(INSite.siteCD), CacheGlobal = true)]
        [Site(DisplayName = "Scrap Warehouse")]
        [PXForeignReference(typeof(Field<scrapSiteID>.IsRelatedTo<INSite.siteID>))]
        public virtual Int32? ScrapSiteID
        {
            get
            {
                return this._ScrapSiteID;
            }
            set
            {
                this._ScrapSiteID = value;
            }
        }
        #endregion
        #region ScrapLocationID
        public abstract class scrapLocationID : PX.Data.BQL.BqlInt.Field<scrapLocationID> { }

        protected Int32? _ScrapLocationID;
        [Location(typeof(AMProdItem.scrapSiteID), DisplayName = "Scrap Location")]
        [PXRestrictor(typeof(Where<INLocation.active, Equal<True>>),
            PX.Objects.IN.Messages.InactiveLocation, typeof(INLocation.locationCD), CacheGlobal = true)]
        [PXForeignReference(typeof(CompositeKey<Field<scrapSiteID>.IsRelatedTo<INLocation.siteID>, Field<scrapLocationID>.IsRelatedTo<INLocation.locationID>>))]
        public virtual Int32? ScrapLocationID
        {
            get
            {
                return this._ScrapLocationID;
            }
            set
            {
                this._ScrapLocationID = value;
            }
        }
        #endregion
	    #region SourceOrderType
	    public abstract class sourceOrderType : PX.Data.BQL.BqlString.Field<sourceOrderType> { }

	    protected String _SourceOrderType;
	    [AMOrderTypeField(DisplayName = "Source Order Type", Visibility = PXUIVisibility.Undefined)]
	    [PXRestrictor(typeof(Where<AMOrderType.active, Equal<True>>), PX.Objects.SO.Messages.OrderTypeInactive)]
	    [AMOrderTypeSelector]
	    [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
	    [PXUIRequired(typeof(IIf<Where<AMProdItem.detailSource, Equal<ProductionDetailSource.productionRef>>, True, False>))]
        public virtual String SourceOrderType
        {
	        get
	        {
	            return this._SourceOrderType;
	        }
	        set
	        {
	            this._SourceOrderType = value;
	        }
	    }
	    #endregion
	    #region SourceProductionNbr
	    public abstract class sourceProductionNbr : PX.Data.BQL.BqlString.Field<sourceProductionNbr> { }

	    protected String _SourceProductionNbr;
	    [ProductionNbr(DisplayName = "Source Production Nbr", Visibility = PXUIVisibility.Undefined)]
	    [ProductionOrderSelector(typeof(AMProdItem.sourceOrderType), true)]
	    [PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
	    [PXUIRequired(typeof(IIf<Where<AMProdItem.detailSource, Equal<ProductionDetailSource.productionRef>>, True, False>))]
        public virtual String SourceProductionNbr
        {
	        get
	        {
	            return this._SourceProductionNbr;
	        }
	        set
	        {
	            this._SourceProductionNbr = value;
	        }
	    }
        #endregion
	    #region ExcludeFromMRP
	    public abstract class excludeFromMRP : PX.Data.BQL.BqlBool.Field<excludeFromMRP> { }

	    protected Boolean? _ExcludeFromMRP;
	    [PXDBBool]
	    [PXDefault(typeof(Search<AMOrderType.excludeFromMRP, Where<AMOrderType.orderType, Equal<Current<AMProdItem.orderType>>>>))]
        [PXUIField(DisplayName = "Exclude from MRP")]
	    public virtual Boolean? ExcludeFromMRP
	    {
	        get
	        {
	            return this._ExcludeFromMRP;
	        }
	        set
	        {
	            this._ExcludeFromMRP = value;
	        }
	    }
        #endregion
	    #region IsReportPrinted
	    /// <summary>
	    /// Indicates if the current production order report has been printed.
	    /// </summary>
	    public abstract class isReportPrinted : PX.Data.BQL.BqlBool.Field<isReportPrinted> { }
	    /// <summary>
	    /// Indicates if the current production order report has been printed.
	    /// </summary>
	    [PXDBBool]
	    [PXDefault(false)]
	    [PXUIField(DisplayName = "Report Printed", Enabled = false)]
	    public virtual bool? IsReportPrinted { get; set; }
	    #endregion
        #region SupplyType
        public abstract class supplyType : PX.Data.BQL.BqlInt.Field<supplyType> { }

        protected int? _SupplyType;
        [PXDBInt]
        [PXDefault(ProductionSupplyType.Inventory)]
        [PXUIField(DisplayName = "Supply Type", Enabled = false)]
        [ProductionSupplyType.SupplyTypeList]
        public virtual int? SupplyType
        {
            get
            {
                return this._SupplyType;
            }
            set
            {
                this._SupplyType = value;
            }
        }
        #endregion
        #region UpdateProject
        public abstract class updateProject : PX.Data.BQL.BqlBool.Field<updateProject> { }

        protected Boolean? _UpdateProject;
        [PXDBBool]
        [PXDefault(false)]        
        [PMVisible(DisplayName = "Update Project")]
        public virtual Boolean? UpdateProject
        {
            get
            {
                return this._UpdateProject;
            }
            set
            {
                this._UpdateProject = value;
            }
        }
        #endregion
	    #region LineCntrOperation
	    public abstract class lineCntrOperation : PX.Data.BQL.BqlInt.Field<lineCntrOperation> { }

	    protected int? _LineCntrOperation;
	    [PXDBInt]
	    [PXDefault(0)]
        [PXUIField(DisplayName = "Operation Line Cntr", Enabled = false, Visible = false)]
	    public virtual int? LineCntrOperation
	    {
	        get
	        {
	            return this._LineCntrOperation;
	        }
	        set
	        {
	            this._LineCntrOperation = value;
	        }
	    }
        #endregion
        #region LineCntrEvnt
        public abstract class lineCntrEvnt : PX.Data.BQL.BqlInt.Field<lineCntrEvnt> { }

        protected int? _LineCntrEvnt;
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Event Line Cntr", Enabled = false, Visible = false)]
        public virtual int? LineCntrEvnt
        {
            get
            {
                return this._LineCntrEvnt;
            }
            set
            {
                this._LineCntrEvnt = value;
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

        #region Methods

        public static implicit operator AMProdItemSplit(AMProdItem item)
        {
            return new AMProdItemSplit
            {
                OrderType = item.OrderType,
                ProdOrdID = item.ProdOrdID,
                InventoryID = item.InventoryID,
                SiteID = item.SiteID,
                SubItemID = item.SubItemID,
                LocationID = item.LocationID,
                LotSerialNbr = item.LotSerialNbr,
                ExpireDate = item.ExpireDate,
                Qty = item.QtyRemaining.GetValueOrDefault(),
                UOM = item.UOM,
                TranDate = item.ProdDate,
                BaseQty = item.BaseQty,
                InvtMult = item.InvtMult,
                Released = false,
                StatusID = item.StatusID,
                TranType = item.TranType,
                ProjectID = item.ProjectID,
                TaskID = item.TaskID
            };
        }
        public static implicit operator AMProdItem(AMProdItemSplit item)
        {
            return new AMProdItem
            {
                ProdOrdID = item.ProdOrdID,
                InventoryID = item.InventoryID,
                SiteID = item.SiteID,
                SubItemID = item.SubItemID,
                LocationID = item.LocationID,
                LotSerialNbr = item.LotSerialNbr,
                QtyRemaining = item.Qty,
                UOM = item.UOM,
                ProdDate = item.TranDate,
                BaseQtyRemaining = item.BaseQty,
                InvtMult = item.InvtMult,
                StatusID = item.StatusID,
                TranType = item.TranType,
                ProjectID = item.ProjectID,
                TaskID = item.TaskID
            };
        }
        #endregion

    }

    /// <summary>
    /// Simplified PXProjection of <see cref="AMProdItem"/>
    /// </summary>
    [PXHidden]
    [PXProjection(typeof(Select<AMProdItem>), Persistent = false)]
    [Serializable]
    public class AMProdItemStatus : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.IBqlField
        {
        }
        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(AMProdItem.orderType))]
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
        public abstract class prodOrdID : PX.Data.IBqlField
        {
        }
        protected String _ProdOrdID;
        [ProductionNbr(IsKey = true, Required = true, Visibility = PXUIVisibility.SelectorVisible, BqlField = typeof(AMProdItem.prodOrdID))]
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
        public abstract class inventoryID : PX.Data.IBqlField
        {
        }
        protected Int32? _InventoryID;
        [PXDBInt(BqlField = typeof(AMProdItem.inventoryID))]
        [PXUIField(DisplayName = "Inventory ID")]
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
        #region SiteID
        public abstract class siteID : PX.Data.IBqlField
        {
        }
        protected Int32? _SiteID;
        [PXDBInt(BqlField = typeof(AMProdItem.siteID))]
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
        #region StatusID
        public abstract class statusID : PX.Data.IBqlField
        {
        }
        protected String _StatusID;
        [PXDBString(1, IsFixed = true, BqlField = typeof(AMProdItem.statusID))]
        [PXUIField(DisplayName = "Status", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
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
        #region ProdDate
        public abstract class prodDate : PX.Data.BQL.BqlDateTime.Field<prodDate> { }

        protected DateTime? _ProdDate;
        [PXDBDate(BqlField = typeof(AMProdItem.prodDate))]
        [PXUIField(DisplayName = "Order Date", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
        public virtual DateTime? ProdDate
        {
            get
            {
                return this._ProdDate;
            }
            set
            {
                this._ProdDate = value;
            }
        }
        #endregion
    }

    /// <summary>
    /// <see cref="AMProdItem"/> for cache updates to related parent and product orders
    /// </summary>
    [Serializable]
    [PXHidden]
    [PXProjection(typeof(Select<AMProdItem>), Persistent = true)]
    public class AMProdItemRelated : IBqlTable
    {
        #region OrderType
        public abstract class orderType : PX.Data.IBqlField
        {
        }
        protected String _OrderType;
        [AMOrderTypeField(IsKey = true, BqlField = typeof(AMProdItem.orderType))]
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
        public abstract class prodOrdID : PX.Data.IBqlField
        {
        }
        protected String _ProdOrdID;
        [ProductionNbr(IsKey = true, Required = true, BqlField = typeof(AMProdItem.prodOrdID))]
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
        public abstract class inventoryID : PX.Data.IBqlField
        {
        }
        protected Int32? _InventoryID;
        [PXDBInt(BqlField = typeof(AMProdItem.inventoryID))]
        [PXUIField(DisplayName = "Inventory ID")]
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
        #region SiteID
        public abstract class siteID : PX.Data.IBqlField
        {
        }
        protected Int32? _SiteID;
        [PXDBInt(BqlField = typeof(AMProdItem.siteID))]
        [PXUIField(DisplayName = "Warehouse")]
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
        #region ParentOrderType
        public abstract class parentOrderType : PX.Data.BQL.BqlString.Field<parentOrderType> { }

        protected String _ParentOrderType;
        [AMOrderTypeField(DisplayName = "Parent Order Type", BqlField = typeof(AMProdItem.parentOrderType))]
        public virtual String ParentOrderType
        {
            get
            {
                return this._ParentOrderType;
            }
            set
            {
                this._ParentOrderType = value;
            }
        }
        #endregion
        #region ParentOrdID
        public abstract class parentOrdID : PX.Data.BQL.BqlString.Field<parentOrdID> { }

        protected String _ParentOrdID;
        [ProductionNbr(DisplayName = "Parent Order", BqlField = typeof(AMProdItem.parentOrdID))]
        public virtual String ParentOrdID
        {
            get
            {
                return this._ParentOrdID;
            }
            set
            {
                this._ParentOrdID = value;
            }
        }
        #endregion
        #region ProductOrderType
        public abstract class productOrderType : PX.Data.BQL.BqlString.Field<productOrderType> { }

        protected String _ProductOrderType;
        [AMOrderTypeField(DisplayName = "Product Order Type", BqlField = typeof(AMProdItem.productOrderType))]
        public virtual String ProductOrderType
        {
            get
            {
                return this._ProductOrderType;
            }
            set
            {
                this._ProductOrderType = value;
            }
        }
        #endregion
        #region ProductOrdID
        public abstract class productOrdID : PX.Data.BQL.BqlString.Field<productOrdID> { }

        protected String _ProductOrdID;
        [ProductionNbr(DisplayName = "Product Order", BqlField = typeof(AMProdItem.productOrdID))]
        public virtual String ProductOrdID
        {
            get
            {
                return this._ProductOrdID;
            }
            set
            {
                this._ProductOrdID = value;
            }
        }
        #endregion
    }
}
