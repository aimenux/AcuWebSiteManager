using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic; // to use Space(int)

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
	public partial class InventoryTranByAcctEnqFilter : PX.Data.IBqlTable
	{

		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		//[FinPeriodID(typeof(AccessInfo.businessDate))]
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

		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[PXDefault()]
		[GL.Account(null, typeof(Search5<Account.accountID,
                        InnerJoin<INItemCostHist, On<Account.accountID, Equal<INItemCostHist.accountID>>>,
                        Where<Match<Current<AccessInfo.userName>>>,
						Aggregate<GroupBy<Account.accountID>>>),
			 DisplayName = "Inventory Account", DescriptionField = typeof(GL.Account.description))]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubCD
		public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }
		protected String _SubCD;
		[SubAccountRaw(DisplayName = "Subaccount")]
		public virtual String SubCD
		{
			get
			{
				return this._SubCD;
			}
			set
			{
				this._SubCD = value;
			}
		}
		#endregion
		#region SubCD Wildcard
		public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
		[PXDBString(30, IsUnicode = true)]
		public virtual String SubCDWildcard
		{
			get
			{
				return SubCDUtils.CreateSubCDWildcard(this._SubCD, SubAccountAttribute.DimensionName);
			}
		}
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

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[AnyInventory(typeof(Search<InventoryItem.inventoryID, Where<InventoryItem.stkItem, NotEqual<boolFalse>, And<Where<Match<Current<AccessInfo.userName>>>>>>), typeof(InventoryItem.inventoryCD), typeof(InventoryItem.descr))]
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
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;		
		[Site(DescriptionField = typeof(INSite.descr), DisplayName = "Warehouse")]		
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
	}

	#endregion

	#region ResultSet
    [Serializable]
	public partial class InventoryTranByAcctEnqResult : PX.Data.IBqlTable
	{
		#region GridLineNbr
		// just for sorting in gris
		public abstract class gridLineNbr : PX.Data.BQL.BqlInt.Field<gridLineNbr> { }
		protected Int32? _GridLineNbr;
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Grid Line Nbr.", Visibility = PXUIVisibility.SelectorVisible, Visible = false)]
		public virtual Int32? GridLineNbr
		{
			get
			{
				return this._GridLineNbr;
			}
			set
			{
				this._GridLineNbr = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Int32? AccountID
		{
			get
			{
				return this._AccountID;
			}
			set
			{
				this._AccountID = value;
			}
		}
		#endregion
		#region SubID
		public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		protected Int32? _SubID;
		//[PXDBInt()]
		[SubAccount(Visibility = PXUIVisibility.Visible)]
		public virtual Int32? SubID
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
		#region TranType
		public abstract class tranType : PX.Data.BQL.BqlString.Field<tranType> { }
		protected String _TranType;
		[PXString(3)] // ???
		[INTranType.List()]
		[PXUIField(DisplayName = "Tran. Type", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXString(1, IsFixed = true)]
		public virtual string DocType { get; set; }
		#endregion
		#region DocRefNbr
		public abstract class docRefNbr : PX.Data.BQL.BqlString.Field<docRefNbr> { }
		protected String _DocRefNbr;
		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(Search<INRegister.refNbr, Where<INRegister.docType, Equal<Current<docType>>>>))]
		public virtual String DocRefNbr
		{
			get
			{
				return this._DocRefNbr;
			}
			set
			{
				this._DocRefNbr = value;
			}
		}
		#endregion
		#region ReceiptNbr
		public abstract class receiptNbr : PX.Data.BQL.BqlString.Field<receiptNbr> { }
		protected String _ReceiptNbr;
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Receipt Nbr.", Visible = false)]
		public virtual String ReceiptNbr
		{
			get
			{
				return this._ReceiptNbr;
			}
			set
			{
				this._ReceiptNbr = value;
			}
		}
		#endregion
		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }
		protected Int32? _InventoryID;
		[Inventory(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Inventory ID")]
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
		[SubItemRawExt(typeof(InventoryTranByAcctEnqResult.inventoryID), DisplayName = "Cost Subitem")]
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
		#region SiteId
		public abstract class siteID : PX.Data.BQL.BqlInt.Field<siteID> { }
		protected Int32? _SiteID;
		//[PXDBInt(IsKey = true)] //???
		[Site(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Warehouse")]
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
		//            [PXDBInt(IsKey = true)] //???
		[Location(Visibility = PXUIVisibility.SelectorVisible, DisplayName = "Location (if costed)")]
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
		#region CostAdj
		public abstract class costAdj : PX.Data.BQL.BqlBool.Field<costAdj> { }
		protected Boolean? _CostAdj;
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Cost Adjustment")]
		public virtual Boolean? CostAdj
		{
			get
			{
				return this._CostAdj;
			}
			set
			{
				this._CostAdj = value;
			}
		}
		#endregion
		#region TranDate
		public abstract class tranDate : PX.Data.BQL.BqlDateTime.Field<tranDate> { }
		protected DateTime? _TranDate;
		[PXDBDate()]
		[PXUIField(DisplayName = "Date")]
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
		#region FinPerNbr
		public abstract class finPerNbr : PX.Data.BQL.BqlString.Field<finPerNbr> { };
		protected String _FinPerNbr;
		[GL.FinPeriodID()]
		[PXUIField(DisplayName = "Fin. Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String FinPerNbr
		{
			get
			{
				return this._FinPerNbr;
			}
			set
			{
				this._FinPerNbr = value;
			}
		}
		#endregion
		#region TranPerNbr
		public abstract class tranPerNbr : PX.Data.BQL.BqlString.Field<tranPerNbr> { };
		protected String _TranPerNbr;
		[GL.FinPeriodID()]
		[PXUIField(DisplayName = "Tran. Period", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String TranPerNbr
		{
			get
			{
				return this._TranPerNbr;
			}
			set
			{
				this._TranPerNbr = value;
			}
		}
		#endregion
		//  Qtys in stock UOM here
		#region Qty
		public abstract class qty : PX.Data.BQL.BqlDecimal.Field<qty> { }
		protected Decimal? _Qty;
		[PXDBQuantity()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Qty.", Visibility = PXUIVisibility.SelectorVisible)]
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
		#region Debit
		public abstract class debit : PX.Data.BQL.BqlDecimal.Field<debit> { }
		protected Decimal? _Debit;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? Debit
		{
			get
			{
				return this._Debit;
			}
			set
			{
				this._Debit = value;
			}
		}
		#endregion
		#region Credit
		public abstract class credit : PX.Data.BQL.BqlDecimal.Field<credit> { }
		protected Decimal? _Credit;
		[PXDBBaseCury()]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual Decimal? Credit
		{
			get
			{
				return this._Credit;
			}
			set
			{
				this._Credit = value;
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
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXUIField(DisplayName = "Release Date", Visible = false)]
		[PXDBDate()]
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
	}
	#endregion


	[PX.Objects.GL.TableAndChartDashboardType]
	public class InventoryTranByAcctEnq : PXGraph<InventoryTranByAcctEnq>
	{

		public PXFilter<InventoryTranByAcctEnqFilter> Filter;

		[PXFilterable]
		public PXSelectJoin<InventoryTranByAcctEnqResult, 
			CrossJoin<INTran>,
			Where<True, Equal<True>>, 
		OrderBy<Asc<InventoryTranByAcctEnqResult.gridLineNbr>>> ResultRecords;
        public PXSelectJoin<InventoryTranByAcctEnqResult,
            CrossJoin<INTran>,
            Where<True, Equal<True>>,
        OrderBy<Asc<InventoryTranByAcctEnqResult.gridLineNbr>>> InternalResultRecords;
		public PXCancel<InventoryTranByAcctEnqFilter> Cancel;
		public PXAction<InventoryTranByAcctEnqFilter> PreviousPeriod;
		public PXAction<InventoryTranByAcctEnqFilter> NextPeriod;

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
		/*
					 protected virtual void InventoryTranByAcctEnqFilter_StartDate_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
					 {
							 if ( true )
							 {
									 e.NewValue = new DateTime( ((DateTime)this.Accessinfo.BusinessDate).Year, 01, 01);
									 e.Cancel = true;
							 }
					 }
	 */
		public InventoryTranByAcctEnq()
		{
			ResultRecords.Cache.AllowInsert = false;
			ResultRecords.Cache.AllowDelete = false;
			ResultRecords.Cache.AllowUpdate = false;
		}


        protected virtual IEnumerable resultRecords()
        {
            InventoryTranByAcctEnqFilter filter = Filter.Current;


            int startRow = 0;
            int totalRows = 0;

            PXResultset<InventoryTranByAcctEnqResult> usortedList = InternalResultRecords.Select();
            if (usortedList.Count == 0)
                return usortedList;
            decimal beginBalance = ((InventoryTranByAcctEnqResult)usortedList[0]).BegBalance ?? 0m;
            List<object> list = InternalResultRecords.View.Select(PXView.Currents, PXView.Parameters, new object[PXView.SortColumns.Length], PXView.SortColumns, PXView.Descendings, PXView.Filters, ref startRow, 0, ref totalRows);
            foreach (PXResult<InventoryTranByAcctEnqResult> item in list)
            {
                InventoryTranByAcctEnqResult it = (InventoryTranByAcctEnqResult)item;
                it.BegBalance = beginBalance;
                decimal? debit = it.Debit;
                decimal? credit = it.Credit;
                beginBalance += (debit ?? 0m) - (credit ?? 0m);
                it.EndBalance = beginBalance;
            }
            return list;
        }
		protected virtual IEnumerable internalResultRecords()
		{
			InventoryTranByAcctEnqFilter filter = Filter.Current;

			bool summaryByDay = filter.SummaryByDay ?? false;
			bool byFinancialPeriod = filter.ByFinancialPeriod ?? false;

			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.tranType>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.docRefNbr>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.subItemCD>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.siteID>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.locationID>(ResultRecords.Cache, null, !summaryByDay);

			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.accountID>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.subID>(ResultRecords.Cache, null, !summaryByDay);
			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.inventoryID>(ResultRecords.Cache, null, !summaryByDay);

			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.costAdj>(ResultRecords.Cache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.finPerNbr>(ResultRecords.Cache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.tranPerNbr>(ResultRecords.Cache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.qty>(ResultRecords.Cache, null, !summaryByDay);
            PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.unitCost>(ResultRecords.Cache, null, !summaryByDay);

			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.begBalance>(ResultRecords.Cache, null, byFinancialPeriod);
			PXUIFieldAttribute.SetVisible<InventoryTranByAcctEnqResult.endBalance>(ResultRecords.Cache, null, byFinancialPeriod);

			PXUIFieldAttribute.SetVisible(Tran.Cache, null, !summaryByDay);

			var resultList = new List<PXResult<InventoryTranByAcctEnqResult, INTran>>();

			decimal cumulativeBalance = 0m;

			if (filter.AccountID == null)
			{
				return resultList;  //empty
			}

			if (filter.FinPeriodID == null)
			{
				return resultList;  //empty
			}

			if (byFinancialPeriod)
			{
				PXSelectBase<INItemCostHist> cmd_CostHist = new PXSelectJoinGroupBy<INItemCostHist,

						InnerJoin<Sub,
										On<INItemCostHist.FK.Sub>>,

						Where<INItemCostHist.finPeriodID, Less<Current<InventoryTranByAcctEnqFilter.finPeriodID>>>,

						Aggregate<
								Sum<INItemCostHist.tranYtdCost,
								Sum<INItemCostHist.tranBegCost,
								Sum<INItemCostHist.finYtdCost,
								Sum<INItemCostHist.finBegCost>>>>>>(this);

				//if (filter.AccountID != null) // checked above
				{
					cmd_CostHist.WhereAnd<Where<INItemCostHist.accountID, Equal<Current<InventoryTranByAcctEnqFilter.accountID>>>>();
				}

				if (!SubCDUtils.IsSubCDEmpty(filter.SubCD))
				{
					cmd_CostHist.WhereAnd<Where<Sub.subCD, Like<Current<InventoryTranByAcctEnqFilter.subCDWildcard>>>>();
				}
				
				PXResultset<INItemCostHist> costHistResult = (PXResultset<INItemCostHist>)cmd_CostHist.Select();
				if (costHistResult.Count == 1) // 0 is possible too
				{
					cumulativeBalance += (((INItemCostHist)costHistResult[0]).FinYtdCost ?? 0m) - (((INItemCostHist)costHistResult[0]).FinBegCost ?? 0m);
				}
			}

			PXSelectBase<INTranCost> cmd = new PXSelectReadonly2<INTranCost,
				  InnerJoin<INTran,
						On<INTranCost.FK.Tran>,
					InnerJoin<InventoryItem, On2<INTranCost.FK.InventoryItem,
						And<Match<InventoryItem, Current<AccessInfo.userName>>>>,
					InnerJoin<Sub, On<INTranCost.FK.InvtSub>,
					InnerJoin<INSubItem, On<INTranCost.FK.CostSubItem>,
					LeftJoin<INSite, On<INTranCost.FK.CostSite>,
					LeftJoin<INLocation, On<INLocation.locationID, Equal<INTranCost.costSiteID>>,
					LeftJoin<INCostStatus, On<INTranCost.costID, Equal<INCostStatus.costID>>>>>>>>>,
					Where<INSite.siteID, IsNull,
						Or<Match<INSite, Current<AccessInfo.userName>>>>,
					OrderBy<Asc<INTranCost.tranDate,
						Asc<INTranCost.createdDateTime>>>>(this);

			//if (filter.FinPeriodID != null) // checked above
			if (byFinancialPeriod)
			{
				cmd.WhereAnd<Where<INTranCost.finPeriodID, Equal<Current<InventoryTranByAcctEnqFilter.finPeriodID>>>>();
			}
			else
			{
				cmd.WhereAnd<Where<INTranCost.tranDate, GreaterEqual<Current<InventoryTranByAcctEnqFilter.periodStartDate>>>>();
				cmd.WhereAnd<Where<INTranCost.tranDate, Less<Current<InventoryTranByAcctEnqFilter.periodEndDate>>>>();

				if (filter.StartDate != null)
				{
					cmd.WhereAnd<Where<INTranCost.tranDate, GreaterEqual<Current<InventoryTranByAcctEnqFilter.startDate>>>>();
				}
				if (filter.EndDate != null)
				{
					cmd.WhereAnd<Where<INTranCost.tranDate, LessEqual<Current<InventoryTranByAcctEnqFilter.endDate>>>>();
				}
			}

			//if (filter.AccountID != null) // checked above
			{
				cmd.WhereAnd<Where<INTranCost.invtAcctID, Equal<Current<InventoryTranByAcctEnqFilter.accountID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(filter.SubCD))
			{
				cmd.WhereAnd<Where<Sub.subCD, Like<Current<InventoryTranByAcctEnqFilter.subCDWildcard>>>>();
			}

			if (filter.InventoryID != null)
				cmd.WhereAnd<Where<INTranCost.inventoryID, Equal<Current<InventoryTranByAcctEnqFilter.inventoryID>>>>();

			if (filter.SiteID != null)
				cmd.WhereAnd<Where<INTranCost.costSiteID, Equal<Current<InventoryTranByAcctEnqFilter.siteID>>>>();

			int gridLineNbr = 0;

			foreach (PXResult<INTranCost, INTran, InventoryItem, Sub, INSubItem, INSite, INLocation, INCostStatus> it in cmd.Select())
			{
				INTranCost tc_rec = (INTranCost)it;
				INTran t_rec = (INTran)it;				
				INSite s_rec = (INSite)it;
				INLocation l_rec = (INLocation)it;
				INSubItem si_rec = (INSubItem)it;
				INCostStatus cs_rec = (INCostStatus)it;

				decimal rowCost = (tc_rec.InvtMult * tc_rec.TranCost) ?? 0m;

				if (tc_rec.TranDate < filter.StartDate)
				{
					cumulativeBalance += rowCost;
				}
				else
				{
					if (summaryByDay)
					{
						if ((resultList.Count > 0) && (((InventoryTranByAcctEnqResult)resultList[resultList.Count - 1]).TranDate == tc_rec.TranDate))
						{
							InventoryTranByAcctEnqResult lastItem = resultList[resultList.Count - 1];
							if (rowCost >= 0)
							{
								lastItem.Debit += rowCost;
							}
							else
							{
								lastItem.Credit -= rowCost;
							}
							lastItem.EndBalance += rowCost;
							resultList[resultList.Count - 1] = new PXResult<InventoryTranByAcctEnqResult, INTran>(lastItem,null);
						}
						else
						{
							InventoryTranByAcctEnqResult item = new InventoryTranByAcctEnqResult();
							item.BegBalance = cumulativeBalance;
							item.TranDate = tc_rec.TranDate;
							if (rowCost >= 0)
							{
								item.Debit = rowCost;
								item.Credit = 0m;
							}
							else
							{
								item.Debit = 0m;
								item.Credit = -rowCost;
							}
							item.EndBalance = item.BegBalance + rowCost;
							item.GridLineNbr = ++gridLineNbr;
							item.CreatedDateTime = tc_rec.CreatedDateTime;
							resultList.Add( new PXResult<InventoryTranByAcctEnqResult, INTran>(item, null));
						}
						cumulativeBalance += rowCost;
					}
					else
					{
						InventoryTranByAcctEnqResult item = new InventoryTranByAcctEnqResult();
						item.BegBalance = cumulativeBalance;
						item.TranDate = tc_rec.TranDate;
						if (rowCost >= 0)
						{
							item.Debit = rowCost;
							item.Credit = 0m;
						}
						else
						{
							item.Debit = 0m;
							item.Credit = -rowCost;
						}
						item.EndBalance = item.BegBalance + rowCost;

						item.AccountID = tc_rec.InvtAcctID;
						item.SubID = tc_rec.InvtSubID;
						item.TranType = tc_rec.TranType;
						item.DocType = tc_rec.DocType;
						item.DocRefNbr = tc_rec.RefNbr;
						item.ReceiptNbr = cs_rec.ReceiptNbr;
						item.InventoryID = tc_rec.InventoryID;
						item.SubItemCD = si_rec.SubItemCD;
						if (s_rec.SiteID != null)
						{
							item.SiteID = s_rec.SiteID;
							item.LocationID = null;
						}
						else
							if (l_rec.LocationID != null) //though it's more or less guaranteed
							{
								item.SiteID = l_rec.SiteID;
								item.LocationID = l_rec.LocationID;
							}
						item.TranDate = tc_rec.TranDate;
						item.FinPerNbr = tc_rec.FinPeriodID;
						item.TranPerNbr = tc_rec.TranPeriodID;
						item.Qty = tc_rec.Qty*tc_rec.InvtMult;
						item.UnitCost = (tc_rec.Qty ?? 0m) == 0m ? null : ((tc_rec.TranCost ?? 0m) + (tc_rec.VarCost ?? 0m)) / tc_rec.Qty;
						item.CostAdj = tc_rec.CostRefNbr != tc_rec.RefNbr;
						item.GridLineNbr = ++gridLineNbr;
						item.CreatedDateTime = tc_rec.CreatedDateTime;
						resultList.Add( new PXResult<InventoryTranByAcctEnqResult, INTran>(item, t_rec));
						cumulativeBalance += rowCost;
					}
				}
			}
			return resultList;
		}

		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		protected virtual void InventoryTranByAcctEnqFilter_PeriodStartDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as InventoryTranByAcctEnqFilter;

			if (row != null)
			{
				FinPeriod period = FinPeriodRepository.FindByID(FinPeriod.organizationID.MasterValue, row.FinPeriodID);
				e.NewValue = period?.StartDate;
			}
		}

		protected virtual void InventoryTranByAcctEnqFilter_PeriodEndDate_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
		{
			var row = e.Row as InventoryTranByAcctEnqFilter;

			if (row != null)
			{
				FinPeriod period = FinPeriodRepository.FindByID(FinPeriod.organizationID.MasterValue, row.FinPeriodID);
				e.NewValue = period?.EndDate;
			}
		}

		protected virtual void ResetFilterDates(InventoryTranByAcctEnqFilter aRow)
		{
			Filter.Cache.SetDefaultExt<InventoryTranByAcctEnqFilter.periodStartDate>(aRow);
			Filter.Cache.SetDefaultExt<InventoryTranByAcctEnqFilter.periodEndDate>(aRow);
			Filter.Cache.SetDefaultExt<InventoryTranByAcctEnqFilter.startDate>(aRow);
			Filter.Cache.SetDefaultExt<InventoryTranByAcctEnqFilter.endDate>(aRow);
		}

		protected virtual void InventoryTranByAcctEnqFilter_FinPeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			InventoryTranByAcctEnqFilter row = (InventoryTranByAcctEnqFilter)e.Row;
			this.ResetFilterDates(row);
		}

		protected virtual void InventoryTranByAcctEnqFilter_ByFinancialPeriod_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			InventoryTranByAcctEnqFilter row = (InventoryTranByAcctEnqFilter)e.Row;
			bool byFinancialPeriod = row.ByFinancialPeriod ?? false;

			if (byFinancialPeriod)
			{
				Filter.Cache.SetValueExt<InventoryTranByAcctEnqFilter.startDate>(row, null);
				Filter.Cache.SetValueExt<InventoryTranByAcctEnqFilter.endDate>(row, null);
			}
		}

		protected virtual void InventoryTranByAcctEnqFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			InventoryTranByAcctEnqFilter row = (InventoryTranByAcctEnqFilter)e.Row;
			bool byFinancialPeriod = row.ByFinancialPeriod ?? false;

			PXUIFieldAttribute.SetEnabled<InventoryTranByAcctEnqFilter.startDate>(Filter.Cache, null, !byFinancialPeriod);
			PXUIFieldAttribute.SetEnabled<InventoryTranByAcctEnqFilter.endDate>(Filter.Cache, null, !byFinancialPeriod);
		}

		protected virtual void InventoryTranByAcctEnqFilter_SubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
        protected virtual void InventoryTranByAcctEnqFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
        {
            ResultRecords.Select();
        }

		#region Button Delegates
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable previousperiod(PXAdapter adapter)
		{
			InventoryTranByAcctEnqFilter filter = Filter.Current as InventoryTranByAcctEnqFilter;

			FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(FinPeriod.organizationID.MasterValue, filter.FinPeriodID, looped: true);

			filter.FinPeriodID = prevPeriod?.FinPeriodID;
            ResetFilterDates(filter);
            return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable nextperiod(PXAdapter adapter)
		{
			InventoryTranByAcctEnqFilter filter = Filter.Current as InventoryTranByAcctEnqFilter;

			FinPeriod nextperiod = FinPeriodRepository.FindNextPeriod(FinPeriod.organizationID.MasterValue, filter.FinPeriodID, looped: true);

			filter.FinPeriodID = nextperiod?.FinPeriodID;
            ResetFilterDates(filter);
            return adapter.Get();
		}
		#endregion

		#region View Actions
		public PXAction<InventoryTranByAcctEnqFilter> viewItem;
		[PXButton()]
		[PXUIField(DisplayName = "")]
		protected virtual IEnumerable ViewItem(PXAdapter a)
		{
			if (this.ResultRecords.Current != null)
				InventoryItemMaint.Redirect(this.ResultRecords.Current.InventoryID, true);
			return a.Get();
		}

		public PXAction<InventoryTranByAcctEnqFilter> viewSummary;
		[PXButton()]
		[PXUIField(DisplayName = Messages.InventorySummary)]
		protected virtual IEnumerable ViewSummary(PXAdapter a)
		{
			if (this.ResultRecords.Current != null)
				InventorySummaryEnq.Redirect(
					this.ResultRecords.Current.InventoryID,
					this.ResultRecords.Current.SubItemCD,
					this.ResultRecords.Current.SiteID,
					this.ResultRecords.Current.LocationID, false);
			return a.Get();
		}

		public PXAction<InventoryTranByAcctEnqFilter> viewAllocDet;
		[PXButton()]
		[PXUIField(DisplayName = Messages.InventoryAllocDet)]
		protected virtual IEnumerable ViewAllocDet(PXAdapter a)
		{
			if (this.ResultRecords.Current != null)
				InventoryAllocDetEnq.Redirect(
					this.ResultRecords.Current.InventoryID,
					this.ResultRecords.Current.SubItemCD,
					null,
					this.ResultRecords.Current.SiteID,
					this.ResultRecords.Current.LocationID);
			return a.Get();
		}
		#endregion

	}
}


