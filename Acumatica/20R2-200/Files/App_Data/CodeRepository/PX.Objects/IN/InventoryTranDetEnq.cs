using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.IN
{

    #region FilterDAC

    [Serializable]
    public partial class InventoryTranDetEnqFilter : PX.Data.IBqlTable
    {

        #region TransferNbr
        public abstract class transferNbr : PX.Data.BQL.BqlString.Field<transferNbr> { }
        protected String _TransferNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<INDocType.transfer>>>))]
        public virtual String TransferNbr
        {
            get
            {
                return this._TransferNbr;
            }
            set
            {
                this._TransferNbr = value;
            }
        }
        #endregion

        #region FinPeriodID
        public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
        protected String _FinPeriodID;
        //[FinPeriodID]
        [FinPeriodSelector(typeof(AccessInfo.businessDate))]
        [PXDefault()]
        [PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
        public virtual String FinPeriodID
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

        #region PeriodStartDate
        public abstract class periodStartDate : PX.Data.BQL.BqlDateTime.Field<periodStartDate> { }
        protected DateTime? _PeriodStartDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Period Start Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
        public virtual DateTime? PeriodStartDate
        {
            get
            {
                return this._PeriodStartDate;
            }
            set
            {
                this._PeriodStartDate = value;
            }
        }
        #endregion
        #region PeriodEndDate
        public abstract class periodEndDate : PX.Data.BQL.BqlDateTime.Field<periodEndDate> { }
        protected DateTime? _PeriodEndDate;
        [PXDBDate()]
        [PXUIField(DisplayName = "Period End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
        public virtual DateTime? PeriodEndDate
        {
            get
            {
                return this._PeriodEndDate;
            }
            set
            {
                this._PeriodEndDate = value;
            }
        }
        #endregion

        #region PeriodEndDateInclusive
        public abstract class periodEndDateInclusive : PX.Data.BQL.BqlDateTime.Field<periodEndDateInclusive> { };
        [PXDBDate()]
        [PXUIField(DisplayName = "Period End Date", Visibility = PXUIVisibility.Visible, Visible = true, Enabled = false)]
        public virtual DateTime? PeriodEndDateInclusive
        {
            get
            {
                if (this._PeriodEndDate == null)
                {
                    return null;
                }
                else
                {
                    return ((DateTime)this._PeriodEndDate).AddDays(-1);
                }
            }
        }
        #endregion


        #region StartDate
        public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
        protected DateTime? _StartDate;
        [PXDBDate()]
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
        [PXDBDate()]
        //[PXDefault(typeof(AccessInfo.businessDate))]
        [PXUIField(DisplayName = "End Date")]
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

        #region InventoryID
        public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
        protected Int32? _InventoryID;
        [PXDefault()]
        [AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>, And<Where<Match<Current<AccessInfo.userName>>>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))] // ??? zzz stock / nonstock ?
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

        #region SubItemCD
        public abstract class subItemCD : PX.Data.BQL.BqlString.Field<subItemCD> { }
        protected String _SubItemCD;
        [SubItemRawExt(typeof(InventoryTranDetEnqFilter.inventoryID), DisplayName = "Subitem")]
        public virtual String SubItemCD
        {
            get
            {
                return this._SubItemCD;
            }
            set
            {
                this._SubItemCD = value;
            }
        }
        #endregion
        #region SubItemCD Wildcard
        public abstract class subItemCDWildcard : PX.Data.BQL.BqlString.Field<subItemCDWildcard> { };
        [PXDBString(30, IsUnicode = true)]
        public virtual String SubItemCDWildcard
        {
            get
            {
                //return SubItemCDUtils.CreateSubItemCDWildcard(this._SubItemCD);
                return SubCDUtils.CreateSubCDWildcard(this._SubItemCD, SubItemAttribute.DimensionName);
            }
        }
        #endregion

        #region SiteID
        public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
        protected Int32? _SiteID;
        //        [Site(Visibility = PXUIVisibility.Visible)]
        [Site(DescriptionField = typeof(INSite.descr), Required = false, DisplayName = "Warehouse")]
        //        [PXDefault()]
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
        [Location(typeof(InventoryTranDetEnqFilter.siteID), Visibility = PXUIVisibility.Visible, KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
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
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        protected String _LotSerialNbr;
		[LotSerialNbr]
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
        #region LotSerialNbrWildcard
        public abstract class lotSerialNbrWildcard : PX.Data.BQL.BqlString.Field<lotSerialNbrWildcard> { };
        [PXDBString(100, IsUnicode = true)]
        public virtual String LotSerialNbrWildcard
        {
            get
            {
                return PXDatabase.Provider.SqlDialect.WildcardAnything + this._LotSerialNbr + PXDatabase.Provider.SqlDialect.WildcardAnything;
            }
        }
        #endregion

        #region ByFinancialPeriod
        public abstract class byFinancialPeriod : PX.Data.BQL.BqlBool.Field<byFinancialPeriod> { }
        protected bool? _ByFinancialPeriod;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "By Financial Period")]
        public virtual bool? ByFinancialPeriod
        {
            get
            {
                return this._ByFinancialPeriod;
            }
            set
            {
                this._ByFinancialPeriod = value;
            }
        }
        #endregion
        #region SummaryByDay
        public abstract class summaryByDay : PX.Data.BQL.BqlBool.Field<summaryByDay> { }
        protected bool? _SummaryByDay;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Summary By Day")]
        public virtual bool? SummaryByDay
        {
            get
            {
                return this._SummaryByDay;
            }
            set
            {
                this._SummaryByDay = value;
            }
        }
        #endregion
        #region IncludeUnreleased
        public abstract class includeUnreleased : PX.Data.BQL.BqlBool.Field<includeUnreleased> { }
        protected bool? _IncludeUnreleased;
        [PXDBBool()]
        [PXDefault(false)]
        [PXUIField(DisplayName = "Include Unreleased (Without Costs)", Visibility = PXUIVisibility.Visible)]
        public virtual bool? IncludeUnreleased
        {
            get
            {
                return this._IncludeUnreleased;
            }
            set
            {
                this._IncludeUnreleased = value;
            }
        }
        #endregion
    }

    #endregion


    #region ResultSet
    [Serializable]
    public partial class InventoryTranDetEnqResult : PX.Data.IBqlTable
    {
        #region GridLineNbr
        // just for sorting in gris
        public abstract class gridLineNbr : PX.Data.BQL.BqlInt.Field<gridLineNbr> { }
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Grid Line Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
        public virtual Int32? GridLineNbr
        {
			get;
			set;
        }
		#endregion

		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		[PXDBDate()]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? TranDate
		{
			get;
			set;
		}
		#endregion

		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		[PXString(3)]
		[INTranType.List()]
		[PXUIField(DisplayName = "Tran. Type", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TranType
		{
			get;
			set;
		}
		#endregion

		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        [PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Doc Type", Visibility = PXUIVisibility.Visible, Visible=false)]
        public virtual String DocType
        {
			get;
			set;
        }
        #endregion
        #region RefNbr
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        [PXString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
        [PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<docType>>>>))]
        public virtual String RefNbr
        {
			get;
			set;
        }
		#endregion
		#region LineNbr
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		[PXDBInt()]
		public virtual Int32? LineNbr
		{
			get;
			set;
		}
		#endregion

		#region SubItemID
		public abstract class subItemID : PX.Data.BQL.BqlInt.Field<subItemID> { }
        [SubItem(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Subitem")]
        public virtual Int32? SubItemID
        {
			get;
			set;
        }
        #endregion
        #region LocationID
        public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
        [Location(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location")]
        public virtual Int32? LocationID
        {
			get;
			set;
        }
        #endregion
        #region LotSerialNbr
        public abstract class lotSerialNbr : PX.Data.BQL.BqlString.Field<lotSerialNbr> { }
        [PXDBString(100, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Lot/Serial Number", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual String LotSerialNbr
        {
			get;
			set;
        }
        #endregion

		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXBool]
		[PXUIField(DisplayName = "Released", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? Released
		{
			get;
			set;
		}
		#endregion

        //  Qtys in stock UOM here

        #region BegQty
        public abstract class begQty : PX.Data.BQL.BqlDecimal.Field<begQty> { }
        protected Decimal? _BegQty;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Beginning Qty.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? BegQty
        {
            get
            {
                return this._BegQty;
            }
            set
            {
                this._BegQty = value;
            }
        }
        #endregion
        #region QtyIn
        public abstract class qtyIn : PX.Data.BQL.BqlDecimal.Field<qtyIn> { }
        protected Decimal? _QtyIn;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. In", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? QtyIn
        {
            get
            {
                return this._QtyIn;
            }
            set
            {
                this._QtyIn = value;
            }
        }
        #endregion
        #region QtyOut
        public abstract class qtyOut : PX.Data.BQL.BqlDecimal.Field<qtyOut> { }
        protected Decimal? _QtyOut;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Qty. Out", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? QtyOut
        {
            get
            {
                return this._QtyOut;
            }
            set
            {
                this._QtyOut = value;
            }
        }
        #endregion
        #region EndQty
        public abstract class endQty : PX.Data.BQL.BqlDecimal.Field<endQty> { }
        protected Decimal? _EndQty;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Ending Qty.", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? EndQty
        {
            get
            {
                return this._EndQty;
            }
            set
            {
                this._EndQty = value;
            }
        }
        #endregion

        #region BegBalance
        public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
        protected Decimal? _BegBalance;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Beginning Balance [*]", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? BegBalance
        {
            get
            {
                return this._BegBalance;
            }
            set
            {
                this._BegBalance = value;
            }
        }
        #endregion
        #region ExtCostIn
        public abstract class extCostIn : PX.Data.BQL.BqlDecimal.Field<extCostIn> { }
        protected Decimal? _ExtCostIn;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Cost In [*]", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? ExtCostIn
        {
            get
            {
                return this._ExtCostIn;
            }
            set
            {
                this._ExtCostIn = value;
            }
        }
        #endregion
        #region ExtCostOut
        public abstract class extCostOut : PX.Data.BQL.BqlDecimal.Field<extCostOut> { }
        protected Decimal? _ExtCostOut;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Cost Out [*]", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? ExtCostOut
        {
            get
            {
                return this._ExtCostOut;
            }
            set
            {
                this._ExtCostOut = value;
            }
        }
        #endregion
        #region EndBalance
        public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
        protected Decimal? _EndBalance;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Ending Balance [*]", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? EndBalance
        {
            get
            {
                return this._EndBalance;
            }
            set
            {
                this._EndBalance = value;
            }
        }
        #endregion

        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        protected Decimal? _UnitCost;
        [PXDBPriceCost()]
        //[PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "In/Out Unit Cost [*]", Visibility = PXUIVisibility.SelectorVisible)]
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
    }
    #endregion


    [PX.Objects.GL.TableAndChartDashboardType]
    public class InventoryTranDetEnq : PXGraph<InventoryTranDetEnq>
    {
        public PXFilter<InventoryTranDetEnqFilter> Filter;

        [PXFilterable]
        public PXSelectJoin<InventoryTranDetEnqResult,
            CrossJoin<INTran>,
            Where<True,Equal<True>>> ResultRecords;

        public PXCancel<InventoryTranDetEnqFilter> Cancel;
        public PXAction<InventoryTranDetEnqFilter> PreviousPeriod;
        public PXAction<InventoryTranDetEnqFilter> NextPeriod;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		#region Cache Attached
		public PXSelect<INTran> Tran;
        [PXDBString(2, IsFixed = true)]
        [PXUIField(DisplayName = "SO Order Type", Visible = false, Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<SO.SOOrderType.orderType>))]
        protected virtual void INTran_SOOrderType_CacheAttached(PXCache sender)
        {
        }
        protected String _SOOrderNbr;
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "SO Order Nbr.", Visible = false, Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<SO.SOOrder.orderNbr>))]
        protected virtual void INTran_SOOrderNbr_CacheAttached(PXCache sender)
        {
        }
        [PXDBString(15, IsUnicode = true)]
        [PXUIField(DisplayName = "PO Receipt Nbr.", Visible = false, Visibility = PXUIVisibility.Visible)]
        [PXSelector(typeof(Search<PO.POReceipt.receiptNbr>))]
        protected virtual void INTran_POReceiptNbr_CacheAttached(PXCache sender)
        {
        }
        #endregion

        public InventoryTranDetEnq()
        {
            ResultRecords.Cache.AllowInsert = false;
            ResultRecords.Cache.AllowDelete = false;
            ResultRecords.Cache.AllowUpdate = false;
        }

		[GL.FinPeriodID()]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible)]
		public void _(Events.CacheAttached<INTran.finPeriodID> args)
		{
		}

		[GL.FinPeriodID()]
		[PXUIField(DisplayName = "Tran. Period", Visibility = PXUIVisibility.SelectorVisible)]
		public void _(Events.CacheAttached<INTran.tranPeriodID> args)
		{
		}

		protected virtual IEnumerable resultRecords()
        {
            InventoryTranDetEnqFilter filter = Filter.Current;

            bool summaryByDay = filter.SummaryByDay ?? false;
            bool includeUnreleased = filter.IncludeUnreleased ?? false;
            bool byFinancialPeriod = filter.ByFinancialPeriod ?? false;

			// if summaryByDay : hide all document-specific values (refnbr, trandate etc.)
			// if includeUnreleased: don't calc and hide cost values
			// if byFinancialPeriod : calc and show running values (Beg, End Qty and Cost)

			PXCache intranCache = this.Caches<INTran>();

            PXUIFieldAttribute.SetVisible<INTran.inventoryID>(intranCache, null, false);

            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.begQty>(ResultRecords.Cache, null, byFinancialPeriod);
            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.endQty>(ResultRecords.Cache, null, byFinancialPeriod);

            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.begBalance>(ResultRecords.Cache, null, !includeUnreleased & byFinancialPeriod);
            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.endBalance>(ResultRecords.Cache, null, !includeUnreleased & byFinancialPeriod);

            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.extCostIn>(ResultRecords.Cache, null, !includeUnreleased);
            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.extCostOut>(ResultRecords.Cache, null, !includeUnreleased);
            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.unitCost>(ResultRecords.Cache, null, !includeUnreleased & !summaryByDay);

            PXUIFieldAttribute.SetVisible<INTran.finPeriodID>(intranCache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<INTran.tranPeriodID>(intranCache, null, !summaryByDay);

            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.tranType>(ResultRecords.Cache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.refNbr>(ResultRecords.Cache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.subItemID>(ResultRecords.Cache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<INTran.siteID>(intranCache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.locationID>(ResultRecords.Cache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.lotSerialNbr>(ResultRecords.Cache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<InventoryTranDetEnqResult.released>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<INTran.releasedDateTime>(intranCache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible(Tran.Cache, null, !summaryByDay);

            var resultList = new List<PXResult<InventoryTranDetEnqResult,INTran>>();

			if (PXView.MaximumRows == 1 && PXView.Searches != null && PXView.Searches.Length == 1)
			{
				InventoryTranDetEnqResult ither = new InventoryTranDetEnqResult();
				ither.GridLineNbr = (int?)PXView.Searches[0];
				ither = (InventoryTranDetEnqResult)ResultRecords.Cache.Locate(ither);
				if (ither != null && ither.TranDate != null)
				{
					PXDelegateResult oneRowResult = new PXDelegateResult();

					oneRowResult.AddRange(new List<InventoryTranDetEnqResult>() { ither });
					oneRowResult.IsResultFiltered = true;
					oneRowResult.IsResultSorted = true;
					oneRowResult.IsResultTruncated = true;

					return oneRowResult;
				}
			}

			if (filter.InventoryID == null)
            {
                return resultList;  //empty
            }

            if (filter.FinPeriodID == null)
            {
                return resultList;  //empty
            }

			PXSelectBase<INTran> cmdBeginning = new PXSelectJoinGroupBy<INTran,
					LeftJoin<INTranSplit, On<INTranSplit.FK.Tran>,
					LeftJoin<INSubItem,
									On<INSubItem.subItemID, Equal<INTran.subItemID>, Or<INSubItem.subItemID, Equal<INTranSplit.subItemID>>>,
					InnerJoin<INSite, On<INTran.FK.Site>>>>
					, Where<INTran.inventoryID, Equal<Current<InventoryTranDetEnqFilter.inventoryID>>, And<Match<INSite, Current<AccessInfo.userName>>>>
				, Aggregate<
						GroupBy<INTran.inventoryID,
						GroupBy<INTran.invtMult,
						GroupBy<INTranSplit.invtMult,
						Sum<INTranSplit.baseQty,
						Sum<INTran.tranCost,
						Sum<INTranSplit.totalQty,
						Sum<INTranSplit.estCost,
						Sum<INTranSplit.totalCost>>>>>>>>>>(this);


			PXSelectBase<INTran> cmd = new PXSelectReadonly2<INTran,
                    LeftJoin<INTranSplit, On<INTranSplit.FK.Tran>,
                    LeftJoin<INSubItem,
                                    On<INSubItem.subItemID, Equal<INTran.subItemID>, Or<INSubItem.subItemID, Equal<INTranSplit.subItemID>>>,
                    InnerJoin<INSite, On<INTran.FK.Site>>>>,
					Where<INTran.inventoryID, Equal<Current<InventoryTranDetEnqFilter.inventoryID>>, And<Match<INSite, Current<AccessInfo.userName>>>>>(this);

			AlterSortsAndFilters(out string[] newSortColumns, out bool[] newDescendings, out bool sortsChanged, out PXFilterRow[] newFilters, out bool filtersChanged);

			if (!byFinancialPeriod)
			{
				cmd.WhereAnd<Where<INTran.tranPeriodID, Equal<Current<InventoryTranDetEnqFilter.finPeriodID>>>>();

				if (filter.StartDate != null)
				{
					cmd.WhereAnd<Where<INTran.tranDate, GreaterEqual<Current<InventoryTranDetEnqFilter.startDate>>>>();

					cmdBeginning.WhereAnd<Where<INTran.tranPeriodID, Less<Current<InventoryTranDetEnqFilter.finPeriodID>>,
						Or<Where<INTran.tranPeriodID, Equal<Current<InventoryTranDetEnqFilter.finPeriodID>>,
							And<INTran.tranDate, Less<Current<InventoryTranDetEnqFilter.startDate>>>>>>>();
				}
				else
				{
					cmdBeginning.WhereAnd<Where<INTran.tranPeriodID, Less<Current<InventoryTranDetEnqFilter.finPeriodID>>>>();
				}

				if (filter.EndDate != null)
				{
					cmd.WhereAnd<Where<INTran.tranDate, LessEqual<Current<InventoryTranDetEnqFilter.endDate>>>>();
				}
			}
			else
			{
				cmd.WhereAnd<Where<INTran.finPeriodID, Equal<Current<InventoryTranDetEnqFilter.finPeriodID>>>>();
				cmdBeginning.WhereAnd<Where<INTran.finPeriodID, Less<Current<InventoryTranDetEnqFilter.finPeriodID>>>>();
			}

            if (filter.SiteID != null)
            {
				cmdBeginning.WhereAnd<Where<INTran.siteID, Equal<Current<InventoryTranDetEnqFilter.siteID>>>>();
                cmd.WhereAnd<Where<INTran.siteID, Equal<Current<InventoryTranDetEnqFilter.siteID>>>>();
            }

            //TODO: consider use of local filtering
            if (!SubCDUtils.IsSubCDEmpty(filter.SubItemCD))
            {
				cmdBeginning.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventoryTranDetEnqFilter.subItemCDWildcard>>>>();
                cmd.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventoryTranDetEnqFilter.subItemCDWildcard>>>>();
            }

            if ((filter.LocationID ?? -1) != -1) // there are cases when filter.LocationID = -1
            {
				cmdBeginning.WhereAnd<Where<INTran.locationID, Equal<Current<InventoryTranDetEnqFilter.locationID>>, Or<INTranSplit.locationID, Equal<Current<InventoryTranDetEnqFilter.locationID>>>>>();
                cmd.WhereAnd<Where<INTran.locationID, Equal<Current<InventoryTranDetEnqFilter.locationID>>, Or<INTranSplit.locationID, Equal<Current<InventoryTranDetEnqFilter.locationID>>>>>();
            }

            if ((filter.LotSerialNbr ?? "") != "")
            {
				cmdBeginning.WhereAnd<Where<INTran.lotSerialNbr, Like<Current<InventoryTranDetEnqFilter.lotSerialNbrWildcard>>, Or<INTranSplit.lotSerialNbr, Like<Current<InventoryTranDetEnqFilter.lotSerialNbrWildcard>>>>>();
                cmd.WhereAnd<Where<INTran.lotSerialNbr, Like<Current<InventoryTranDetEnqFilter.lotSerialNbrWildcard>>, Or<INTranSplit.lotSerialNbr, Like<Current<InventoryTranDetEnqFilter.lotSerialNbrWildcard>>>>>();
            }

            if (!includeUnreleased)
            {
				cmdBeginning.WhereAnd<Where<INTran.released, Equal<boolTrue>>>();
                cmd.WhereAnd<Where<INTran.released, Equal<boolTrue>>>();
            }

			int startRow = 0;
			int totalRows = 0;

			decimal cumulativeQty = 0;
			decimal cumulativeBalance = 0;

			if (byFinancialPeriod)
			{
				foreach (PXResult<INTran, INTranSplit, INSubItem> item in cmdBeginning.View.Select(PXView.Currents, PXView.Parameters, new object[newSortColumns.Length], newSortColumns, newDescendings, PXView.Filters, ref startRow, 0, ref totalRows))
				{

					INTranSplit ts_first = item;
					INTran t_first = item;
					cumulativeQty += (ts_first.InvtMult * ts_first.BaseQty) ?? 0m;
					if (ts_first.InvtMult == null)
					{
						cumulativeBalance += (t_first.InvtMult * t_first.TranCost) ?? 0m;
					}
					else if (ts_first.TotalQty != 0m)
					{
						cumulativeBalance += (ts_first.InvtMult * ts_first.EstCost) ?? 0m;
					}
					else
					{
						cumulativeBalance += (ts_first.InvtMult * ts_first.TotalCost) ?? 0m;
					}

				}
			}

			bool allowSelectWithTop = !summaryByDay && !sortsChanged && !filtersChanged && !PXView.ReverseOrder;
			startRow = allowSelectWithTop ? PXView.StartRow : 0;
			int maximumRows = allowSelectWithTop ? (PXView.StartRow + PXView.MaximumRows) : 0;
			totalRows = 0;
			List<object> intermediateResult = cmd.View.Select(PXView.Currents, PXView.Parameters, new object[newSortColumns.Length], newSortColumns, newDescendings, PXView.Filters, ref startRow, maximumRows, ref totalRows);
			if (allowSelectWithTop) PXView.StartRow = 0;


			int gridLineNbr = 0;

            foreach (PXResult<INTran, INTranSplit, INSubItem> it in intermediateResult)
            {
                INTranSplit ts_rec = (INTranSplit)it;
                INTran t_rec = (INTran)it;

                decimal rowQty = (ts_rec.InvtMult * ts_rec.BaseQty) ?? 0m;
                decimal rowExtCost;

                if (ts_rec.InvtMult == null)
                {
                    rowExtCost = (t_rec.InvtMult * t_rec.TranCost) ?? 0m;
                }
                else if (ts_rec.TotalQty != 0m)
                {
					rowExtCost = (ts_rec.InvtMult * ts_rec.EstCost) ?? 0m;
				}
                else
                {
                    rowExtCost = (ts_rec.InvtMult * ts_rec.TotalCost) ?? 0m;
                }

                if (!(((byFinancialPeriod & (t_rec.FinPeriodID.CompareTo(filter.FinPeriodID) < 0))
               || (!byFinancialPeriod & (t_rec.TranPeriodID.CompareTo(filter.FinPeriodID) < 0)))
               || (filter.StartDate != null && (t_rec.TranDate < filter.StartDate))))
                {
                    if (summaryByDay)
                    {
                        if ((resultList.Count > 0) && (((InventoryTranDetEnqResult)resultList[resultList.Count - 1]).TranDate == t_rec.TranDate))
                        {
                            InventoryTranDetEnqResult lastItem = resultList[resultList.Count - 1];
                            if (rowQty >= 0)
                            {
                                lastItem.QtyIn += rowQty;
                            }
                            else
                            {
                                lastItem.QtyOut -= rowQty;
                            }
                            if (!byFinancialPeriod) { lastItem.EndQty += rowQty; }
                            if (!includeUnreleased)
                            {
                                if (rowExtCost >= 0m)
                                {
                                    lastItem.ExtCostIn += rowExtCost;
                                }
                                else
                                {
                                    lastItem.ExtCostOut -= rowExtCost;
                                }
                                if (!byFinancialPeriod) { lastItem.EndBalance += rowExtCost; }
                            }
                            // UnitCost not set - since not shown as meaningless
							cumulativeQty += rowQty;
							cumulativeBalance += rowExtCost;
                        }
                        else
                        {
                            InventoryTranDetEnqResult item = new InventoryTranDetEnqResult();
                            item.TranDate = t_rec.TranDate;
                            if (rowQty >= 0)
                            {
                                item.QtyIn = rowQty;
                                item.QtyOut = 0m;
                            }
                            else
                            {
                                item.QtyIn = 0m;
                                item.QtyOut = -rowQty;
                            }
                            if (!includeUnreleased)
                            {
                                if (rowExtCost >= 0)
                                {
                                    item.ExtCostIn = rowExtCost;
                                    item.ExtCostOut = 0m;
                                }
                                else
                                {
                                    item.ExtCostIn = 0m;
                                    item.ExtCostOut = -rowExtCost;
                                }
                            }
                            if (!byFinancialPeriod)
                            {
                                item.EndQty = item.BegQty + rowQty;
                                if (!includeUnreleased) { item.EndBalance = item.BegBalance + rowExtCost; }
                            }
                            item.GridLineNbr = ++gridLineNbr;
							item.BegQty = cumulativeQty;
							item.BegBalance = cumulativeBalance;
							cumulativeQty += (item.QtyIn ?? 0m) - (item.QtyOut ?? 0m);
							cumulativeBalance += (item.ExtCostIn ?? 0m) - (item.ExtCostOut ?? 0m);
							item.EndQty = cumulativeQty;
							item.EndBalance = cumulativeBalance;
                            resultList.Add(new PXResult<InventoryTranDetEnqResult, INTran>(item, null));
                        }
                    }
                    else
                    {
                        InventoryTranDetEnqResult item = new InventoryTranDetEnqResult();
                        item.TranDate = t_rec.TranDate;
                        if (rowQty >= 0)
                        {
                            item.QtyIn = rowQty;
                            item.QtyOut = 0m;
                        }
                        else
                        {
                            item.QtyIn = 0m;
                            item.QtyOut = -rowQty;
                        }
                        if (!includeUnreleased)
                        {
                            if (rowExtCost >= 0)
                            {
                                item.ExtCostIn = rowExtCost;
                                item.ExtCostOut = 0m;
                            }
                            else
                            {
                                item.ExtCostIn = 0m;
                                item.ExtCostOut = -rowExtCost;
                            }
                        }

                        if (rowQty != 0m) { item.UnitCost = rowExtCost / rowQty; }

                        if (!byFinancialPeriod)
                        {
                            item.EndQty = item.BegQty + rowQty;
                            if (!includeUnreleased) { item.EndBalance = item.BegBalance + rowExtCost; }
                        }

						item.TranType = t_rec.TranType;
                        item.DocType = t_rec.DocType;
                        item.RefNbr = t_rec.RefNbr;
						item.LineNbr = t_rec.LineNbr;
						item.SubItemID = ts_rec.SubItemID ?? t_rec.SubItemID;
                        item.LocationID = ts_rec.LocationID ?? t_rec.LocationID;
                        item.LotSerialNbr = ts_rec.LotSerialNbr ?? t_rec.LotSerialNbr;
						item.Released = t_rec.Released;
                        item.GridLineNbr = ++gridLineNbr;
						item.BegQty = cumulativeQty;
						item.BegBalance = cumulativeBalance;
						cumulativeQty += (item.QtyIn ?? 0m) - (item.QtyOut ?? 0m);
						cumulativeBalance += (item.ExtCostIn ?? 0m) - (item.ExtCostOut ?? 0m);
						item.EndQty = cumulativeQty;
						item.EndBalance = cumulativeBalance;
                        resultList.Add(new PXResult<InventoryTranDetEnqResult, INTran>(item, t_rec));
                    }
                }

            }

			ResultRecords.Cache.Clear();

			foreach (PXResult<InventoryTranDetEnqResult, INTran> item in resultList)
			{
				InventoryTranDetEnqResult it = (InventoryTranDetEnqResult)item;
				ResultRecords.Cache.SetStatus(it, PXEntryStatus.Held);
			}

			if (summaryByDay)
				return resultList;
			else
			{
				PXDelegateResult delegateResult = new PXDelegateResult();

				delegateResult.IsResultFiltered = !filtersChanged;
				delegateResult.IsResultSorted = !sortsChanged;
				delegateResult.IsResultTruncated = totalRows > resultList.Count;

				if (!PXView.ReverseOrder)
				{
					delegateResult.AddRange(resultList);
				}
				else
				{
					var sortedList = PXView.Sort(resultList);
					delegateResult.AddRange(sortedList.Cast<PXResult<InventoryTranDetEnqResult, INTran>>());
					delegateResult.IsResultSorted = true;
				}

				return delegateResult;
			}
		}

		protected virtual void AlterSortsAndFilters(out string[] newSorts, out bool[] newDescs, out bool sortsChanged, out PXFilterRow[] newFilters, out bool filtersChanged)
		{
			bool byTranDate = false;
			bool byReleasedDate = false;
			bool includeUnreleased = Filter.Current?.IncludeUnreleased ?? false;
			bool summaryByDay = Filter.Current?.SummaryByDay ?? false;
			List<string> newSortColumns = new List<string>();
			List<bool> newDescendings = new List<bool>();
			List<PXFilterRow> filters = new List<PXFilterRow>();
			string intranPrefix = $"{nameof(INTran)}__";
			string intranSplitPrefix = $"{nameof(INTranSplit)}__";


			sortsChanged = false;
			filtersChanged = false;
			
			foreach (PXFilterRow filter in PXView.Filters)
				filtersChanged |= ProcessField(filter);

			for (int columnIndex = 0; columnIndex < PXView.SortColumns.Length; columnIndex++)
				sortsChanged |= ProcessField(null, PXView.SortColumns[columnIndex], PXView.Descendings[columnIndex]);

			newSorts = newSortColumns.ToArray();
			newDescs = newDescendings.ToArray();
			newFilters = filters.ToArray();


			void AddFilter(PXFilterRow oldFilter, string newField)
				=> filters.Add(new PXFilterRow(oldFilter) { DataField = newField });

			void AddSort(string field, bool desc)
			{
				newSortColumns.Add(field);
				newDescendings.Add(desc);
			}

			void AddSortByReleasedDate(bool desc)
			{
				if (!byReleasedDate)
				{
					AddSort(nameof(INTran.ReleasedDateTime), desc);
					AddSort(nameof(INTran.DocType), desc);
					AddSort(nameof(INTran.RefNbr), desc);
					AddSort(intranSplitPrefix + nameof(INTranSplit.LotSerialNbr), desc);
					AddSort(intranPrefix + nameof(INTran.LotSerialNbr), desc);
					AddSort(nameof(INTran.LineNbr), desc);
					byReleasedDate = true;
				}
			}

			void AddSortByTranDate(bool desc)
			{
				if (!byTranDate)
				{
					AddSort(nameof(INTran.TranDate), desc);
					AddSort(nameof(INTran.Released), !desc);
					AddSort(nameof(INTran.ReleasedDateTime), desc);
					AddSort(nameof(INTran.DocType), desc);
					AddSort(nameof(INTran.RefNbr), desc);
					AddSort(intranSplitPrefix + nameof(INTranSplit.LotSerialNbr), desc);
					AddSort(intranPrefix + nameof(INTran.LotSerialNbr), desc);
					AddSort(nameof(INTran.LineNbr), desc);
					byTranDate = true;
				}
			}

			bool ProcessField(PXFilterRow filter, string sortField = null, bool desc = false)
			{
				bool changed = false;

				if ((filter == null && sortField == null) || (filter != null && sortField != null))
					throw new PXArgumentException();

				string field = sortField;
				if (filter != null) field = filter.DataField;

				if (field.StartsWith(intranPrefix)) field = field.Substring(intranPrefix.Length);

				switch(field)
				{
					case var f when string.Compare(f, nameof(InventoryTranDetEnqResult.GridLineNbr), true) == 0:
						if (sortField != null)
						{
							if (!includeUnreleased && !summaryByDay)
								AddSortByReleasedDate(desc);
							else
								AddSortByTranDate(desc);
						}
						if (filter != null) changed = true;
						break;


					case var f when string.Compare(f, nameof(INTran.ReleasedDateTime), true) == 0:
						if (sortField != null) AddSortByReleasedDate(desc);
						if (filter != null) AddFilter(filter, field);
						break;


					case var f when string.Compare(f, nameof(INTran.TranDate), true) == 0:
						if (sortField != null) AddSortByTranDate(desc);
						if (filter != null) AddFilter(filter, field);
						break;


					case var f when string.Compare(f, nameof(INTran.LotSerialNbr), true) == 0 ||
									string.Compare(f, nameof(INTran.SubItemID), true) == 0 ||
									string.Compare(f, nameof(INTran.LocationID), true) == 0:
						if (sortField != null)
						{
							AddSort(intranSplitPrefix + field, desc);
							AddSort(field, desc);
						}
						if (filter != null) AddFilter(filter, intranSplitPrefix + field);
						break;


					case var f when string.Compare(f, nameof(INTran.DocType), true) == 0 ||
									string.Compare(f, nameof(INTran.SiteID), true) == 0 ||
									string.Compare(f, nameof(INTran.FinPeriodID), true) == 0 ||
									string.Compare(f, nameof(INTran.TranPeriodID), true) == 0 ||
									string.Compare(f, nameof(INTran.Released), true) == 0 ||
									string.Compare(f, nameof(INTran.ReleasedDateTime), true) == 0 ||
									string.Compare(f, nameof(INTran.SOOrderType), true) == 0 ||
									string.Compare(f, nameof(INTran.SOOrderNbr), true) == 0:
						if (sortField != null) AddSort(field, desc);
						if (filter != null) AddFilter(filter, field);
						break;
						

					default:
						changed = true;
						break;
				}

				return changed;
			}
		}

		public override bool IsDirty => false;

        protected virtual void InventoryTranDetEnqFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (e.Row == null)
            {
                return;
            }

            InventoryTranDetEnqFilter f = (InventoryTranDetEnqFilter)e.Row;
            if ((f.PeriodStartDate == null) && (f.PeriodEndDate == null))
            {
                ResetFilterDates(f);
            }
        }


        protected virtual void ResetFilterDates(InventoryTranDetEnqFilter aRow)
        {
			FinPeriod period = FinPeriodRepository.FindByID(FinPeriod.organizationID.MasterValue, aRow.FinPeriodID);

			if (period != null)
            {
                aRow.PeriodStartDate = period.StartDate;
                aRow.PeriodEndDate = (DateTime)period.EndDate;
                aRow.EndDate = null;
                aRow.StartDate = null;
            }
        }

		protected virtual void InventoryTranDetEnqFilter_ByFinancialPeriod_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			InventoryTranDetEnqFilter row = (InventoryTranDetEnqFilter)e.Row;
			bool byFinancialPeriod = row.ByFinancialPeriod ?? false;

			PXUIFieldAttribute.SetEnabled<InventoryTranDetEnqFilter.startDate>(cache, null, !byFinancialPeriod);
			PXUIFieldAttribute.SetEnabled<InventoryTranDetEnqFilter.endDate>(cache, null, !byFinancialPeriod);

			if (byFinancialPeriod)
			{
				cache.SetValueExt<InventoryTranDetEnqFilter.startDate>(row, null);
				cache.SetValueExt<InventoryTranDetEnqFilter.endDate>(row, null);
			}
		}


		protected virtual void InventoryTranDetEnqFilter_FinPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            InventoryTranDetEnqFilter row = (InventoryTranDetEnqFilter)e.Row;
            this.ResetFilterDates(row);
        }

        //private bool _StartEndDateVerificationChain = false;

        protected virtual void InventoryTranDetEnqFilter_StartDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            InventoryTranDetEnqFilter row = (InventoryTranDetEnqFilter)e.Row;
            DateTime? newValue = (DateTime?)e.NewValue;
            /*          // �� ��������� : ��� ByFinancialPeriod==true ���� ����� �� �������� � ������
                                    if (newValue.HasValue && row.PeriodStartDate.HasValue && row.PeriodEndDate.HasValue)
                                    {
                                            if ((newValue < row.PeriodStartDate.Value) || (newValue >= row.PeriodEndDate.Value))
                                            {
                                                    throw new PXSetPropertyException("Start Date must fall into the period");
                                            }
                                    } 
            */

            if (newValue.HasValue && row.EndDate.HasValue)
            {
                if ((newValue > row.EndDate.Value))
                {
                    throw new PXSetPropertyException(Messages.StartDateMustBeLessOrEqualToTheEndDate);
                }
            }

            InventoryTranDetEnqFilter currentFilter = Filter.Current;

            //PXFieldState state = (PXFieldState)cache.GetStateExt(currentFilter, typeof(InventoryTranDetEnqFilter.endDate).Name);

            //          string error = PXUIFieldAttribute.GetError<InventoryTranDetEnqFilter.endDate>(cache, e.Row);
            //          if (string.IsNullOrEmpty(error) == false) { };

            /*
                                    if (!_StartEndDateVerificationChain)
                                    {
                                            _StartEndDateVerificationChain = true;
                                            object endDate = row.EndDate;
                                            cache.RaiseFieldVerifying<InventoryTranDetEnqFilter.endDate>(e.Row, ref endDate);
                                            _StartEndDateVerificationChain = false;
                                    }
            */

        }

        protected virtual void InventoryTranDetEnqFilter_EndDate_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
        {
            InventoryTranDetEnqFilter row = (InventoryTranDetEnqFilter)e.Row;
            DateTime? newValue = (DateTime?)e.NewValue;
            if (newValue.HasValue && row.StartDate.HasValue)
            {
                if ((newValue < row.StartDate.Value))
                {
                    throw new PXSetPropertyException(Messages.StartDateMustBeLessOrEqualToTheEndDate);
                }
            }
            /*
                                    if (!_StartEndDateVerificationChain)
                                    {
                                            _StartEndDateVerificationChain = true;
                                            object startDate = row.StartDate;
                                            cache.RaiseFieldVerifying<InventoryTranDetEnqFilter.startDate>(e.Row, ref startDate);
                                            _StartEndDateVerificationChain = false;
                                    }
            */
        }


        #region Button Delegates
        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXPreviousButton]
        public virtual IEnumerable previousperiod(PXAdapter adapter)
        {
            InventoryTranDetEnqFilter filter = Filter.Current as InventoryTranDetEnqFilter;

			FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(FinPeriod.organizationID.MasterValue, filter.FinPeriodID, looped: true);

			filter.FinPeriodID = prevPeriod?.FinPeriodID;
            ResetFilterDates(filter);
            return adapter.Get();
        }

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXNextButton]
        public virtual IEnumerable nextperiod(PXAdapter adapter)
        {
            InventoryTranDetEnqFilter filter = Filter.Current as InventoryTranDetEnqFilter;

			FinPeriod nextperiod = FinPeriodRepository.FindNextPeriod(FinPeriod.organizationID.MasterValue, filter.FinPeriodID, looped: true);

            filter.FinPeriodID = nextperiod.FinPeriodID;
            ResetFilterDates(filter);
            return adapter.Get();
        }
        #endregion

        #region View Actions

        public PXAction<InventoryTranDetEnqFilter> viewSummary;
        [PXButton()]
        [PXUIField(DisplayName = Messages.InventorySummary)]
        protected virtual IEnumerable ViewSummary(PXAdapter a)
        {
            if (this.ResultRecords.Current != null)
            {
				var currentResult = ResultRecords.Current;
				var intran = INTran.PK.Find(this, currentResult.DocType, currentResult.RefNbr, currentResult.LineNbr);

				if (intran != null)
				{
					PXSegmentedState subItem =
						ResultRecords.Cache.GetValueExt<InventoryTranDetEnqResult.subItemID>(currentResult) as PXSegmentedState;

					InventorySummaryEnq.Redirect(intran.InventoryID, subItem != null ? (string)subItem.Value : null,
						intran.SiteID, ResultRecords.Current.LocationID, false);
				}
            }
            return a.Get();
        }

        public PXAction<InventoryTranDetEnqFilter> viewAllocDet;
        [PXButton()]
        [PXUIField(DisplayName = Messages.InventoryAllocDet)]
        protected virtual IEnumerable ViewAllocDet(PXAdapter a)
        {
            if (this.ResultRecords.Current != null)
            {
				var currentResult = ResultRecords.Current;
				var intran = INTran.PK.Find(this, currentResult.DocType, currentResult.RefNbr, currentResult.LineNbr);

				if (intran != null)
				{
					PXSegmentedState subItem =
						ResultRecords.Cache.GetValueExt<InventoryTranDetEnqResult.subItemID>(currentResult) as PXSegmentedState;

					InventoryAllocDetEnq.Redirect(intran.InventoryID, subItem != null ? (string)subItem.Value : null,
						null, intran.SiteID, currentResult.LocationID);
				}
            }
            return a.Get();
        }

        #endregion

        public static void Redirect(string finPeriodID, int? inventoryID, string subItemCD, string lotSerNum, int? siteID, int? locationID)
        {
            InventoryTranDetEnq graph = PXGraph.CreateInstance<InventoryTranDetEnq>();
            if (!string.IsNullOrEmpty(finPeriodID))
                graph.Filter.Current.FinPeriodID = finPeriodID;

            graph.Filter.Current.InventoryID = inventoryID;
            graph.Filter.Current.SubItemCD = subItemCD;
            graph.Filter.Current.SiteID = siteID;
            graph.Filter.Current.LocationID = locationID;
            graph.Filter.Current.LotSerialNbr = lotSerNum;

            throw new PXRedirectRequiredException(graph, Messages.InventoryTranDet);
        }
    }
}


