using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using PX.SM;
using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.Common.Tools;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.IN
{
    #region FilterDAC

    [Serializable]
    public partial class InventoryTranHistEnqFilter : PX.Data.IBqlTable
    {
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
        [SubItemRawExt(typeof(InventoryTranHistEnqFilter.inventoryID), DisplayName = "Subitem")]
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
        [Location(typeof(InventoryTranHistEnqFilter.siteID), Visibility = PXUIVisibility.Visible, KeepEntry = false, DescriptionField = typeof(INLocation.descr), DisplayName = "Location")]
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
        #region ByFinancialPeriod (commented)
        /*
        public abstract class byFinancialPeriod : PX.Data.BQL.BqlBool.Field<byFinancialPeriod> { }
        protected bool? _ByFinancialPeriod;
        [PXDBBool()]
        [PXDefault()]
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
*/
        #endregion
        #region SummaryByDay
        public abstract class summaryByDay : PX.Data.BQL.BqlBool.Field<summaryByDay> { }
        protected bool? _SummaryByDay;
        [PXDBBool()]
        [PXDefault()]
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
        [PXDefault()]
        [PXUIField(DisplayName = "Include Unreleased", Visibility = PXUIVisibility.Visible)]
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
        #region ShowAdjUnitCost
        public abstract class showAdjUnitCost : PX.Data.BQL.BqlBool.Field<showAdjUnitCost> { }
        protected bool? _ShowAdjUnitCost;
        [PXDBBool()]
        [PXDefault()]
        [PXUIField(DisplayName = "Include Landed Cost in Unit Cost", Visibility = PXUIVisibility.Visible)]
        public virtual bool? ShowAdjUnitCost
        {
            get
            {
                return this._ShowAdjUnitCost;
            }
            set
            {
                this._ShowAdjUnitCost = value;
            }
        }
        #endregion
    }




#endregion

    #region ResultSet
    [Serializable]
    public partial class InventoryTranHistEnqResult : PX.Data.IBqlTable
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
        #region DocType
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        protected String _DocType;
		[PXDBString(1, IsFixed = true)]
        [PXUIField(DisplayName = "Doc Type", Visibility = PXUIVisibility.Visible, Visible = false)]
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
		#region SplitLineNbr
		public abstract class splitLineNbr : PX.Data.BQL.BqlInt.Field<splitLineNbr> { }
		[PXDBInt()]
		public virtual Int32? SplitLineNbr
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

        #region UOM (commented)
        /*
        public abstract class uOM : PX.Data.BQL.BqlString.Field<uOM> { }
        protected String _UOM;
        //[INUnit(typeof(INTranSplit.inventoryID))]
        //[PXDefault(typeof(INTran.uOM))]
        [PXUIField(DisplayName = "UOM", Enabled = false)]
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
*/
        #endregion
        #region BaseQty (commented)
        /*
        public abstract class baseQty : PX.Data.BQL.BqlDecimal.Field<baseQty> { }
        protected Decimal? _BaseQty;
        [PXDBQuantity()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "BaseQty", Visibility = PXUIVisibility.SelectorVisible)]
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
*/
        #endregion

        #region UnitCost
        public abstract class unitCost : PX.Data.BQL.BqlDecimal.Field<unitCost> { }
        protected Decimal? _UnitCost;
		[PXDBPriceCost()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Unit Cost", Visibility = PXUIVisibility.SelectorVisible)]
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

        // not used currently :
        #region ExtCost
        public abstract class extCost : PX.Data.BQL.BqlDecimal.Field<extCost> { }
        protected Decimal? _ExtCost;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Extended Cost", Visibility = PXUIVisibility.SelectorVisible)]
        public virtual Decimal? ExtCost
        {
            get
            {
                return this._ExtCost;
            }
            set
            {
                this._ExtCost = value;
            }
        }
        #endregion
        #region BegBalance
        public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
        protected Decimal? _BegBalance;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Beginning Balance", Visibility = PXUIVisibility.SelectorVisible)]
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
        #region EndBalance
        public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
        protected Decimal? _EndBalance;
        [PXDBBaseCury()]
        [PXDefault(TypeCode.Decimal, "0.0")]
        [PXUIField(DisplayName = "Ending Balance", Visibility = PXUIVisibility.SelectorVisible)]
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
    }
    #endregion

    [PX.Objects.GL.TableAndChartDashboardType]
    public class InventoryTranHistEnq : PXGraph<InventoryTranHistEnq>
    {
        public PXFilter<InventoryTranHistEnqFilter> Filter;

        [PXFilterable]
        public PXSelectJoin<InventoryTranHistEnqResult,
            CrossJoin<INTran, CrossJoin<INTranSplit>>,
            Where<True, Equal<True>>> ResultRecords;
        public PXSelectJoin<InventoryTranHistEnqResult,
			CrossJoin<INTran, CrossJoin<INTranSplit>>,
			Where<True, Equal<True>>> InternalResultRecords;
        public PXCancel<InventoryTranHistEnqFilter> Cancel;
        public PXAction<InventoryTranHistEnqFilter> viewSummary;
        public PXAction<InventoryTranHistEnqFilter> viewAllocDet;

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

        protected virtual void InventoryTranHistEnqFilter_StartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            if (true)
            {
                DateTime businessDate = (DateTime)this.Accessinfo.BusinessDate;
                e.NewValue = new DateTime(businessDate.Year, businessDate.Month, 01);
                e.Cancel = true;
            }
        }

        public InventoryTranHistEnq()
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
	        InventoryTranHistEnqFilter filter = Filter.Current;

	        bool summaryByDay = filter.SummaryByDay ?? false;
	        bool includeUnreleased = filter.IncludeUnreleased ?? false;

			PXCache intranCache = this.Caches<INTran>();
			PXCache intranSplitCache = this.Caches<INTranSplit>();

			PXUIFieldAttribute.SetVisible<INTran.inventoryID>(intranCache, null, false);
	        PXUIFieldAttribute.SetVisible<INTran.finPeriodID>(intranCache, null, false); //???
	        PXUIFieldAttribute.SetVisible<INTran.tranPeriodID>(intranCache, null, false); //???


	        PXUIFieldAttribute.SetVisible<INTran.tranType>(intranCache, null, !summaryByDay);
	        PXUIFieldAttribute.SetVisible<InventoryTranHistEnqResult.refNbr>(ResultRecords.Cache, null, !summaryByDay);
	        PXUIFieldAttribute.SetVisible<INTranSplit.subItemID>(intranSplitCache, null, !summaryByDay);
	        PXUIFieldAttribute.SetVisible<INTran.siteID>(intranCache, null, !summaryByDay);
	        PXUIFieldAttribute.SetVisible<INTranSplit.locationID>(intranSplitCache, null, !summaryByDay);
	        PXUIFieldAttribute.SetVisible<INTranSplit.lotSerialNbr>(intranSplitCache, null, !summaryByDay);
	        PXUIFieldAttribute.SetVisible(Tran.Cache, null, !summaryByDay);

			int startRow = PXView.StartRow;
            int totalRows = 0;
			decimal? beginQty = null;

            if (PXView.MaximumRows == 1 && PXView.Searches != null && PXView.Searches.Length == 1)
            {
                InventoryTranHistEnqResult ither = new InventoryTranHistEnqResult();
                ither.GridLineNbr = (int?)PXView.Searches[0];
                ither = (InventoryTranHistEnqResult)ResultRecords.Cache.Locate(ither);
				if (ither != null && ither.TranDate != null)
				{
					PXDelegateResult oneRowResult = new PXDelegateResult();

					oneRowResult.AddRange(new List<InventoryTranHistEnqResult>() { ither });
					oneRowResult.IsResultFiltered = true;
					oneRowResult.IsResultSorted = true;
					oneRowResult.IsResultTruncated = true;

					return oneRowResult;
				}
            }

            ResultRecords.Cache.Clear();
			List<object> list = InternalResultRecords.View.Select(PXView.Currents, PXView.Parameters, new object[PXView.SortColumns.Length], PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
			PXView.StartRow = 0;

			foreach (PXResult<InventoryTranHistEnqResult> item in list)
			{
				InventoryTranHistEnqResult it = (InventoryTranHistEnqResult)item;
				it.BegQty = beginQty = (beginQty ?? it.BegQty);
                decimal? QtyIn = it.QtyIn;
                decimal? QtyOut = it.QtyOut;
                beginQty += (QtyIn ?? 0m) - (QtyOut ?? 0m);
                it.EndQty = beginQty;
                ResultRecords.Cache.SetStatus(it, PXEntryStatus.Held);
            }

			PXDelegateResult delegateResult = new PXDelegateResult();

			delegateResult.AddRange(list);
			delegateResult.IsResultFiltered = true;
			delegateResult.IsResultSorted = true;
			delegateResult.IsResultTruncated = totalRows > delegateResult.Count;

			return delegateResult;
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

				switch (field)
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


					case var f when string.Compare(f, intranSplitPrefix + nameof(INTran.LotSerialNbr), true) == 0 ||
									string.Compare(f, intranSplitPrefix + nameof(INTran.SubItemID), true) == 0 ||
									string.Compare(f, intranSplitPrefix + nameof(INTran.LocationID), true) == 0:
						if (sortField != null) AddSort(field, desc);
						if (filter != null) AddFilter(filter, field);
						break;


					case var f when string.Compare(f, nameof(INTran.DocType), true) == 0 ||
									string.Compare(f, nameof(INTran.SiteID), true) == 0 ||
									string.Compare(f, nameof(INTran.Released), true) == 0 ||
									string.Compare(f, nameof(INTran.ReleasedDateTime), true) == 0:
						if (sortField != null) AddSort(field, desc);
						if (filter != null) AddFilter(filter, field);
						break;

					case var f when string.Compare(f, nameof(INTran.FinPeriodID), true) == 0 ||
									string.Compare(f, nameof(INTran.TranPeriodID), true) == 0 ||
									string.Compare(f, nameof(INTran.SOOrderType), true) == 0 ||
									string.Compare(f, nameof(INTran.SOOrderNbr), true) == 0:
						if (sortField != null) AddSort(intranPrefix + field, desc);
						if (filter != null) AddFilter(filter, intranPrefix + field);
						break;

					case var f when string.Compare(f, nameof(InventoryTranHistEnqResult.QtyIn), true) == 0 ||
									string.Compare(f, nameof(InventoryTranHistEnqResult.QtyOut), true) == 0:
						if (sortField != null) changed = true;
						if (filter != null)
						{
							if (!summaryByDay) AddFilter(filter, intranSplitPrefix + field);
							else changed = true;
						}
						break;

					default:
						changed = true;
						break;
				}

				return changed;
			}
		}

	    protected virtual IEnumerable internalResultRecords()
	    {
		    InventoryTranHistEnqFilter filter = Filter.Current;

		    bool summaryByDay = filter.SummaryByDay ?? false;
		    bool includeUnreleased = filter.IncludeUnreleased ?? false;

		    var resultList = new List<PXResult<InventoryTranHistEnqResult, INTran, INTranSplit>>();

		    if (filter.InventoryID == null)
		    {
				PXDelegateResult emptyResult = new PXDelegateResult();
				emptyResult.IsResultFiltered = true;
				emptyResult.IsResultSorted = true;
				emptyResult.IsResultTruncated = true;

				return emptyResult;  //empty
		    }

		    PXSelectBase<INTranSplit> cmd = new PXSelectReadonly2<INTranSplit,
                    InnerJoin<INTran, On<INTranSplit.FK.Tran>,
                    InnerJoin<INSubItem, On<INTranSplit.FK.SubItem>,
                    InnerJoin<INSite, On<INTran.FK.Site>>>>,
			    Where<INTranSplit.inventoryID, Equal<Current<InventoryTranHistEnqFilter.inventoryID>>, And<Match<INSite, Current<AccessInfo.userName>>>>,
			    OrderBy<Asc<INTranSplit.docType, Asc<INTranSplit.refNbr, Asc<INTranSplit.lineNbr, Asc<INTranSplit.splitLineNbr>>>>>>(this);

		    PXSelectBase<INItemSiteHistByDay> cmdBegBalanceNew = new PXSelectReadonly2<INItemSiteHistByDay,
			    InnerJoin<INItemSiteHistDay,
				    On<INItemSiteHistDay.inventoryID, Equal<INItemSiteHistByDay.inventoryID>,
					    And<INItemSiteHistDay.siteID, Equal<INItemSiteHistByDay.siteID>,
						    And<INItemSiteHistDay.subItemID, Equal<INItemSiteHistByDay.subItemID>,
							    And<INItemSiteHistDay.locationID, Equal<INItemSiteHistByDay.locationID>,
								    And<INItemSiteHistDay.sDate, Equal<INItemSiteHistByDay.lastActivityDate>>>>>>,
				    InnerJoin<INSubItem,
					    On<INSubItem.subItemID, Equal<INItemSiteHistByDay.subItemID>>,
					    InnerJoin<INSite, On<INSite.siteID, Equal<INItemSiteHistByDay.siteID>>>>>,
			    Where<INItemSiteHistByDay.inventoryID, Equal<Current<InventoryTranHistEnqFilter.inventoryID>>,
				    And<INItemSiteHistByDay.date, Equal<Required<INItemSiteHistByDay.date>>,
					    And<Match<INSite, Current<AccessInfo.userName>>>>>>(this);

		    if (!SubCDUtils.IsSubCDEmpty(filter.SubItemCD) && PXAccess.FeatureInstalled<FeaturesSet.subItem>())
		    {
			    cmd.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventoryTranHistEnqFilter.subItemCDWildcard>>>>();
			    cmdBegBalanceNew.WhereAnd<Where<INSubItem.subItemCD, Like<Current<InventoryTranHistEnqFilter.subItemCDWildcard>>>>();
		    }

		    if (filter.SiteID != null && PXAccess.FeatureInstalled<FeaturesSet.warehouse>())
		    {
			    cmd.WhereAnd<Where<INTranSplit.siteID, Equal<Current<InventoryTranHistEnqFilter.siteID>>>>();
			    cmdBegBalanceNew.WhereAnd<Where<INItemSiteHistByDay.siteID, Equal<Current<InventoryTranHistEnqFilter.siteID>>>>();
		    }

		    if ((filter.LocationID ?? -1) != -1 && PXAccess.FeatureInstalled<FeaturesSet.warehouseLocation>()) // there are cases when filter.LocationID = -1
		    {
			    cmd.WhereAnd<Where<INTranSplit.locationID, Equal<Current<InventoryTranHistEnqFilter.locationID>>>>();
			    cmdBegBalanceNew.WhereAnd<Where<INItemSiteHistByDay.locationID, Equal<Current<InventoryTranHistEnqFilter.locationID>>>>();
		    }

		    if ((filter.LotSerialNbr ?? "") != "" && PXAccess.FeatureInstalled<FeaturesSet.lotSerialTracking>())
		    {
			    cmd.WhereAnd<Where<INTranSplit.lotSerialNbr, Like<Current<InventoryTranHistEnqFilter.lotSerialNbrWildcard>>>>();
		    }

		    if (!includeUnreleased)
		    {
			    cmd.WhereAnd<Where<INTranSplit.released, Equal<True>>>();
		    }

		    decimal cumulativeQty = 0m;

		    if (filter.StartDate != null)
		    {
			    foreach (PXResult<INItemSiteHistByDay, INItemSiteHistDay> res in cmdBegBalanceNew.Select(filter.StartDate))
			    {
				    INItemSiteHistByDay byday = res;
				    INItemSiteHistDay hist = res;

				    cumulativeQty +=
				    ((byday.LastActivityDate != null && byday.Date != null &&
				      byday.Date.Value.Date == byday.LastActivityDate.Value.Date)
					    ? hist.BegQty
					    : hist.EndQty) ?? 0m;
			    }

			    if (includeUnreleased)
			    {
					INSite site = INSite.PK.Find(this, filter.SiteID);

					int calendarOrganizationID = PXAccess.GetParentOrganizationID(site?.BranchID) ?? FinPeriod.organizationID.MasterValue;

					string TranPeriodID;
					DateTime? PeriodStartDate;

					try
					{
						TranPeriodID = FinPeriodRepository.GetPeriodIDFromDate(filter.StartDate, calendarOrganizationID);
						PeriodStartDate = FinPeriodRepository.PeriodStartDate(TranPeriodID, calendarOrganizationID);
					}
					catch (PXFinPeriodException)
					{
						TranPeriodID = null;
						PeriodStartDate = filter.StartDate;
					}

					PXSelectBase<OrganizationFinPeriod> periodCmd =
					new PXSelectGroupBy<OrganizationFinPeriod,
								Where<OrganizationFinPeriod.finPeriodID, LessEqual<Required<OrganizationFinPeriod.finPeriodID>>,
										And<OrganizationFinPeriod.iNClosed, Equal<False>,
										Or<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>>>>,
								Aggregate<GroupBy<OrganizationFinPeriod.finPeriodID>>,
								OrderBy<Asc<OrganizationFinPeriod.finPeriodID>>>(this);

					List<object> periodCmdParams = new List<object>() { TranPeriodID, TranPeriodID };


					if (calendarOrganizationID != FinPeriod.organizationID.MasterValue)
					{
						periodCmd.WhereAnd<Where<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>();

						periodCmdParams.Add(calendarOrganizationID);
					}

					OrganizationFinPeriod firstOpenOrCurrentClosedPeriod = periodCmd.SelectWindowed(0, 1, periodCmdParams.ToArray());

					if (firstOpenOrCurrentClosedPeriod != null)
				    {
						TranPeriodID = firstOpenOrCurrentClosedPeriod.FinPeriodID;
						PeriodStartDate = FinPeriodRepository.PeriodStartDate(firstOpenOrCurrentClosedPeriod.FinPeriodID, calendarOrganizationID);

					    PXView v2 = new PXView(this, true, cmd.View.BqlSelect
							.WhereAnd<Where<INTranSplit.tranDate, GreaterEqual<Required<INTranSplit.tranDate>>>>()
							.WhereAnd<Where<INTranSplit.tranDate, Less<Required<INTranSplit.tranDate>>>>()
						    .WhereAnd<Where<INTranSplit.released, Equal<False>>>()
						    .AggregateNew<Aggregate<
							    GroupBy<INTranSplit.inventoryID, GroupBy<INTranSplit.invtMult, Sum<INTranSplit.baseQty>>>>>());

					    int splitStartRow = 0;
					    int splitTotalRows = 0;

					    foreach (PXResult<INTranSplit> res in v2.Select(new object[0], new object[] { PeriodStartDate, filter.StartDate.Value}, new object[0],
						    new string[0], new bool[0], new PXFilterRow[0], ref splitStartRow, 0, ref splitTotalRows))
					    {
						    INTranSplit tsRec = res;
						    cumulativeQty += (tsRec.InvtMult * tsRec.BaseQty) ?? 0m;
					    }
				    }
			    }
		    }

		    if (filter.StartDate != null)
		    {
			    cmd.WhereAnd<Where<INTranSplit.tranDate, GreaterEqual<Current<InventoryTranHistEnqFilter.startDate>>>>();
		    }

		    if (filter.EndDate != null)
		    {
			    cmd.WhereAnd<Where<INTranSplit.tranDate, LessEqual<Current<InventoryTranHistEnqFilter.endDate>>>>();
		    }

			AlterSortsAndFilters(out string[] newSortColumns, out bool[] newDescendings, out bool sortsChanged, out PXFilterRow[] newFilters, out bool filtersChanged);

			//if user clicks last, sorts will be inverted
			//as it is not possible to calculate beginning balance from the end
			//we will select without top from the start and then apply reverse order and select top n records
			//for next page we will ommit startrow to set beginning balance correctly
			//top (n, m) will be set in the outer search results since we do not reset PXView.StartRow to 0
			//Also, user can set a filter or sort by a column from result DAC (sortsChanged = true, filtersChanged = true) and need to get full result set.
			int startRow = 0;
			bool allowSelectWithTop = !sortsChanged && !filtersChanged && !PXView.ReverseOrder;
			int maximumRows = allowSelectWithTop ? PXView.StartRow + PXView.MaximumRows : 0;
		    int totalRows = 0;

		    PXView selectView = !summaryByDay
			    ? cmd.View
			    : new PXView(this, true,
				    cmd.View.BqlSelect
					    .AggregateNew<Aggregate<GroupBy<INTranSplit.tranDate, Sum<INTranSplit.qtyIn, Sum<INTranSplit.qtyOut>>>>>());

		    List<object> intermediateResult = selectView.Select(PXView.Currents, new object[] {filter.StartDate},
			    new string[newSortColumns.Length], newSortColumns, newDescendings, newFilters, ref startRow, maximumRows,
			    ref totalRows);

		    int gridLineNbr = 0;

		    foreach (PXResult<INTranSplit, INTran, INSubItem> it in intermediateResult)
		    {
			    INTranSplit ts_rec = (INTranSplit)it;
			    INTran t_rec = (INTran)it;

			    if (summaryByDay)
			    {
				    InventoryTranHistEnqResult item = new InventoryTranHistEnqResult();
				    item.BegQty = cumulativeQty;
				    item.TranDate = ts_rec.TranDate;
				    item.QtyIn = ts_rec.QtyIn;
				    item.QtyOut = ts_rec.QtyOut;
				    item.EndQty = item.BegQty + ts_rec.QtyIn - ts_rec.QtyOut;
				    item.GridLineNbr = ++gridLineNbr;
				    resultList.Add(new PXResult<InventoryTranHistEnqResult, INTran, INTranSplit>(item, null, null));
				    cumulativeQty += (ts_rec.QtyIn - ts_rec.QtyOut) ?? 0m;
			    }
			    else
			    {
				    InventoryTranHistEnqResult item = new InventoryTranHistEnqResult();
				    item.BegQty = cumulativeQty;
				    item.TranDate = ts_rec.TranDate;
				    item.QtyIn = ts_rec.QtyIn;
				    item.QtyOut = ts_rec.QtyOut;
				    item.EndQty = item.BegQty + ts_rec.QtyIn - ts_rec.QtyOut;

				    item.DocType = ts_rec.DocType;
				    item.RefNbr = ts_rec.RefNbr;
					item.LineNbr = ts_rec.LineNbr;
				    item.SplitLineNbr = ts_rec.SplitLineNbr;
					item.GridLineNbr = ++gridLineNbr;

				    decimal? unitcost;
				    if(filter.ShowAdjUnitCost ?? false)
				    {
					    unitcost = ts_rec.TotalQty != null && ts_rec.TotalQty != 0m ? (ts_rec.TotalCost + ts_rec.AdditionalCost) / ts_rec.TotalQty : 0m;
				    }
				    else
				    {
					    unitcost = ts_rec.TotalQty != null && ts_rec.TotalQty != 0m ? ts_rec.TotalCost / ts_rec.TotalQty : 0m;
				    }

				    item.UnitCost = unitcost;
				    resultList.Add(new PXResult<InventoryTranHistEnqResult, INTran, INTranSplit>(item, t_rec, ts_rec));
				    cumulativeQty += (ts_rec.InvtMult * ts_rec.BaseQty) ?? 0m;
			    }
		    }


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
				delegateResult.AddRange(sortedList.Cast<PXResult<InventoryTranHistEnqResult, INTran, INTranSplit>>());
				delegateResult.IsResultSorted = true;
			}

			return delegateResult;
	    }

		public override bool IsDirty => false;

        #region View Actions

        [PXButton()]
        [PXUIField(DisplayName = Messages.InventorySummary)]
        protected virtual IEnumerable ViewSummary(PXAdapter a)
        {
            if (this.ResultRecords.Current != null)
            {
				var currentResult = ResultRecords.Current;
				var intran = INTran.PK.Find(this, currentResult.DocType, currentResult.RefNbr, currentResult.LineNbr);
				var intransplit = INTranSplit.PK.Find(this, currentResult.DocType, currentResult.RefNbr, currentResult.LineNbr, currentResult.SplitLineNbr);

				if (intran != null)
				{
					PXSegmentedState subItem = ResultRecords.Cache.GetValueExt<INTranSplit.subItemID>(intransplit) as PXSegmentedState;
					InventorySummaryEnq.Redirect(intran.InventoryID, subItem != null ? (string)subItem.Value : null,
						intran.SiteID, intransplit.LocationID, false);
				}
            }
            return a.Get();
        }


        [PXButton()]
        [PXUIField(DisplayName = Messages.InventoryAllocDet)]
        protected virtual IEnumerable ViewAllocDet(PXAdapter a)
        {
            if (this.ResultRecords.Current != null)
            {
				var currentResult = ResultRecords.Current;
				var intran = INTran.PK.Find(this, currentResult.DocType, currentResult.RefNbr, currentResult.LineNbr);
				var intransplit = INTranSplit.PK.Find(this, currentResult.DocType, currentResult.RefNbr, currentResult.LineNbr, currentResult.SplitLineNbr);

				if (intran != null)
				{
					PXSegmentedState subItem = ResultRecords.Cache.GetValueExt<INTranSplit.subItemID>(intransplit) as PXSegmentedState;
					InventoryAllocDetEnq.Redirect(intran.InventoryID, subItem != null ? (string)subItem.Value : null,
						null, intran.SiteID, intransplit.LocationID);
				}
            }
            return a.Get();
        }

        #endregion

        public static void Redirect(string finPeriodID, int? inventoryID, string subItemCD, string lotSerNum, int? siteID, int? locationID)
        {
            InventoryTranHistEnq graph = PXGraph.CreateInstance<InventoryTranHistEnq>();

            graph.Filter.Current.InventoryID = inventoryID;
            graph.Filter.Current.SubItemCD = subItemCD;
            graph.Filter.Current.SiteID = siteID;
            graph.Filter.Current.LocationID = locationID;
            graph.Filter.Current.LotSerialNbr = lotSerNum;

            throw new PXRedirectRequiredException(graph, Messages.InventoryTranHist);
        }
    }
}


