using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Common;
using PX.Dashboards;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.SQLTree;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.Common;

using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Tools;
using PX.Objects.Common.Utility;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.AR
{
	/// <summary>
	/// A projection DAC over <see cref="ARHistory"/> that is intended to close the gaps 
	/// in AR history records. (The gaps in AR history records appear if AR history records 
	/// do not exist for every financial period defined in the system.) That is, the purpose 
	/// of this DAC is to calculate the <see cref="LastActivityPeriod">last activity period</see> 
	/// for every existing <see cref="MasterFinPeriod">financial period</see>, so that inquiries and reports 
	/// that produce information for a given financial period can look at the latest available 
	/// <see cref="ARHistory"/> record. For example, this projection is used in the Customer 
	/// Summary (AR401000) form, which corresponds to the <see cref="ARCustomerBalanceEnq"/> graph.
	/// </summary>
	[Serializable]
	[PXProjection(typeof(Select5<ARHistory,
		InnerJoin<MasterFinPeriod,
			On<MasterFinPeriod.finPeriodID, GreaterEqual<ARHistory.finPeriodID>>>,
		Aggregate<
		GroupBy<ARHistory.branchID,
		GroupBy<ARHistory.customerID,
		GroupBy<ARHistory.accountID,
		GroupBy<ARHistory.subID,
		Max<ARHistory.finPeriodID,
		GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>))]
	[PXPrimaryGraph(
		new Type[] {
			typeof(ARDocumentEnq),
			typeof(ARCustomerBalanceEnq)
		},
		new Type[] {
			typeof(Where<BaseARHistoryByPeriod.customerID, IsNotNull>),
			typeof(Where<BaseARHistoryByPeriod.customerID, IsNull>)
		},
		Filters = new Type[] {
			typeof(ARDocumentEnq.ARDocumentFilter),
			typeof(ARCustomerBalanceEnq.ARHistoryFilter)
		})]
	[PXCacheName(Messages.BaseARHistoryByPeriod)]
	public partial class BaseARHistoryByPeriod : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(IsKey = true, BqlField = typeof(ARHistory.branchID))]
		public virtual Int32? BranchID
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer(IsKey = true, BqlField = typeof(ARHistory.customerID), CacheGlobal = true)]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(IsKey = true, BqlField = typeof(ARHistory.accountID))]
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
		[SubAccount(IsKey = true, BqlField = typeof(ARHistory.subID))]
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
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;
		[GL.FinPeriodID(BqlField = typeof(ARHistory.finPeriodID))]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
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
	}

	/// <summary>
	/// A projection DAC over <see cref="CuryARHistory"/> that is intended to close the gaps 
	/// in AR history records. (The gaps in AR history records appear if AR history records do 
	/// not exist for every financial period defined in the system.) That is, the purpose of 
	/// this DAC is to calculate the <see cref="LastActivityPeriod">last activity period</see> 
	/// for every existing <see cref="MasterFinPeriod">financial period</see>, so that inquiries and 
	/// reports that produce information for a given financial period can look at the latest
	/// available <see cref="CuryARHistory"/> record. For example, this projection is
	/// used in the Customer Summary (AR401000) form, which corresponds to the
	/// <see cref="ARCustomerBalanceEnq"/> graph.
	/// </summary>
	[Serializable]
	[PXProjection(typeof(Select5<CuryARHistory,
		InnerJoin<MasterFinPeriod,
			On<MasterFinPeriod.finPeriodID, GreaterEqual<CuryARHistory.finPeriodID>>>,
		Aggregate<
		GroupBy<CuryARHistory.branchID,
		GroupBy<CuryARHistory.customerID,
		GroupBy<CuryARHistory.accountID,
		GroupBy<CuryARHistory.subID,
		GroupBy<CuryARHistory.curyID,
		Max<CuryARHistory.finPeriodID,
		GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>>))]
	[PXCacheName(Messages.ARHistoryByPeriod)]
	public partial class ARHistoryByPeriod : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(IsKey = true, BqlField = typeof(CuryARHistory.branchID))]
		public virtual Int32? BranchID
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer(IsKey = true, BqlField = typeof(CuryARHistory.customerID))]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(IsKey = true, BqlField = typeof(CuryARHistory.accountID))]
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
		[SubAccount(IsKey = true, BqlField = typeof(CuryARHistory.subID))]
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
		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true, IsKey = true, InputMask = ">LLLLL", BqlField = typeof(CuryARHistory.curyID))]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
		#endregion
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;
		[GL.FinPeriodID(BqlField = typeof(CuryARHistory.finPeriodID))]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
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

	}

	[Serializable]
	[PXHidden]
	[PXProjection(typeof(Select5<ARHistory,
	InnerJoin<MasterFinPeriod,
		On<MasterFinPeriod.finPeriodID, GreaterEqual<ARHistory.finPeriodID>>>,
	Aggregate<
	GroupBy<ARHistory.branchID,
	GroupBy<ARHistory.customerID,
	GroupBy<ARHistory.accountID,
	GroupBy<ARHistory.subID,
	Max<ARHistory.finPeriodID,
	GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>))]
	[PXCacheName(Messages.ARHistoryByPeriod)]
	public partial class ARHistory2ByPeriod : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(IsKey = true, BqlField = typeof(ARHistory.branchID))]
		public virtual Int32? BranchID
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
		#region CustomerID
		public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
		protected Int32? _CustomerID;
		[Customer(IsKey = true, BqlField = typeof(ARHistory.customerID))]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(IsKey = true, BqlField = typeof(ARHistory.accountID))]
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
		[SubAccount(IsKey = true, BqlField = typeof(ARHistory.subID))]
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
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;
		[GL.FinPeriodID(BqlField = typeof(ARHistory.finPeriodID))]
		public virtual String LastActivityPeriod
		{
			get
			{
				return this._LastActivityPeriod;
			}
			set
			{
				this._LastActivityPeriod = value;
			}
		}
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		protected String _FinPeriodID;
		[GL.FinPeriodID(IsKey = true, BqlField = typeof(MasterFinPeriod.finPeriodID))]
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

	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class ARCustomerBalanceEnq : PXGraph<ARCustomerBalanceEnq>
	{
		#region Internal Types
		[Serializable]
		public partial class ARHistoryFilter : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			[Organization(false, Required = false)]
			public int? OrganizationID { get; set; }
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			[BranchOfOrganization(typeof(ARHistoryFilter.organizationID), false)]
			public int? BranchID { get; set; }
			#endregion
			#region OrgBAccountID
			public abstract class orgBAccountID : IBqlField { }

			[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
			public int? OrgBAccountID { get; set; }
			#endregion
			#region ARAcctID
			public abstract class aRAcctID : PX.Data.BQL.BqlInt.Field<aRAcctID> { }
			[GL.Account(null,typeof(Search5<Account.accountID,
						InnerJoin<ARHistory, On<Account.accountID, Equal<ARHistory.accountID>>>,
						Where<Match<Current<AccessInfo.userName>>>,
					   Aggregate<GroupBy<Account.accountID>>>),
				DisplayName = "AR Account", DescriptionField = typeof(GL.Account.description))]
			public virtual int? ARAcctID
			{
				get;
				set;
			}
			#endregion
			#region ARSubID
			public abstract class aRSubID : PX.Data.BQL.BqlInt.Field<aRSubID> { }
			[GL.SubAccount(DisplayName = "AR Sub.", DescriptionField = typeof(GL.Sub.description), Visible = false)]
			public virtual int? ARSubID
			{
				get;
				set;
			}
			#endregion
			#region SubCD
			public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }
			[PXDBString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "AR Subaccount", Visibility = PXUIVisibility.Invisible, FieldClass = SubAccountAttribute.DimensionName)]
			[PXDimension("SUBACCOUNT", ValidComboRequired = false)]
			public virtual string SubCD
			{
				get;
				set;
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXSelector(typeof(CM.Currency.curyID), CacheGlobal = true)]
			[PXUIField(DisplayName = "Currency ID")]
			public virtual string CuryID
			{
				get;
				set;
			}
			#endregion
			#region CustomerClassID
			public abstract class customerClassID : PX.Data.BQL.BqlString.Field<customerClassID> { }
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(CustomerClass.customerClassID), DescriptionField = typeof(CustomerClass.descr), CacheGlobal = true)]
			[PXUIField(DisplayName = "Customer Class")]
			public virtual string CustomerClassID
			{
				get;
				set;
			}
			#endregion
			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			[PXDBString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Status")]
			[BAccount.status.List]
			public virtual string Status
			{
				get;
				set;
			}
			#endregion
			#region ShowWithBalanceOnly
			public abstract class showWithBalanceOnly : PX.Data.BQL.BqlBool.Field<showWithBalanceOnly> { }
			[PXDBBool]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Customers with Balance Only")]
			public virtual bool? ShowWithBalanceOnly
			{
				get;
				set;
			}
			#endregion
			#region UseMasterCalendar
			public abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }

			[PXBool]
			[PXUIField(DisplayName = Common.Messages.UseMasterCalendar)]
			[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.multipleCalendarsSupport>))]
			public bool? UseMasterCalendar { get; set; }
			#endregion

			#region Period
			public abstract class period : PX.Data.BQL.BqlString.Field<period> { }
			// Acuminator disable once PX1030 PXDefaultIncorrectUse [Incorrect validation]
			[PXDefault(typeof(Coalesce<
				Search<FinPeriod.finPeriodID,
				Where<FinPeriod.organizationID, Equal<Current<organizationID>>,
					And<FinPeriod.startDate, LessEqual<Current<AccessInfo.businessDate>>>>,
				OrderBy<Desc<FinPeriod.startDate>>>,
				Search<FinPeriod.finPeriodID,
						Where<FinPeriod.organizationID, Equal<Zero>,
					And<FinPeriod.startDate, LessEqual<Current<AccessInfo.businessDate>>>>,
				OrderBy<Desc<FinPeriod.startDate>>>>
				))]
			[AnyPeriodFilterable(null, null,
				branchSourceType: typeof(ARHistoryFilter.branchID),
				organizationSourceType: typeof(ARHistoryFilter.organizationID),
				useMasterCalendarSourceType: typeof(ARHistoryFilter.useMasterCalendar),
				redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
			[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.Visible)]
			public virtual string Period
			{
				get;
				set;
			}
			#endregion
			#region SubCD Wildcard
			public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
			[PXDBString(30, IsUnicode = true)]
			public virtual String SubCDWildcard
			{
				get
				{
					return SubCDUtils.CreateSubCDWildcard(this.SubCD, SubAccountAttribute.DimensionName);
				}
			}
            #endregion
            #region CustomerID
            public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
            [PXDBInt]
            public virtual int? CustomerID
            {
                get;
                set;
            }
            #endregion
            #region SplitByCurrency
            public abstract class splitByCurrency : PX.Data.BQL.BqlBool.Field<splitByCurrency> { }
            [PXDBBool]
            [PXDefault(true)]
            [PXUIField(DisplayName = Common.Messages.SplitByCurrency)]
            public virtual bool? SplitByCurrency
            {
                get;
                set;
            }
            #endregion
            #region CuryBalanceSummary
            public abstract class curyBalanceSummary : PX.Data.BQL.BqlDecimal.Field<curyBalanceSummary> { }
			protected Decimal? _CuryBalanceSummary;
			[PXCury(typeof(ARHistoryFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Balance (Currency)", Enabled = false)]
			public virtual Decimal? CuryBalanceSummary
			{
				get
				{
					return this._CuryBalanceSummary;
				}
				set
				{
					this._CuryBalanceSummary = value;
				}
			}
			#endregion
			#region BalanceSummary
			public abstract class balanceSummary : PX.Data.BQL.BqlDecimal.Field<balanceSummary> { }
			protected Decimal? _BalanceSummary;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Balance", Enabled = false)]
			public virtual Decimal? BalanceSummary
			{
				get
				{
					return this._BalanceSummary;
				}
				set
				{
					this._BalanceSummary = value;
				}
			}
			#endregion

			#region CuryRevaluedSummary
			public abstract class curyRevaluedSummary : PX.Data.BQL.BqlDecimal.Field<curyRevaluedSummary> { }
			protected Decimal? _CuryRevaluedSummary;
			[PXCury(typeof(ARHistoryFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Unrealized Gain/Loss", Enabled = false)]
			public virtual Decimal? CuryRevaluedSummary
			{
				get
				{
					return this._CuryRevaluedSummary;
				}
				set
				{
					this._CuryRevaluedSummary = value;
				}
			}
			#endregion
			#region RevaluedSummary
			public abstract class revaluedSummary : PX.Data.BQL.BqlDecimal.Field<revaluedSummary> { }
			protected Decimal? _RevaluedSummary;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Unrealized Gain/Loss", Enabled = false)]
			public virtual Decimal? RevaluedSummary
			{
				get
				{
					return this._RevaluedSummary;
				}
				set
				{
					this._RevaluedSummary = value;
				}
			}
			#endregion

			#region DepositsSummary
			public abstract class depositsSummary : PX.Data.BQL.BqlDecimal.Field<depositsSummary> { }
			protected Decimal? _DepositsSummary;
			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Prepayments", Enabled = false)]
            public virtual Decimal? DepositsSummary
            {
				get;
				set;
			}
			#endregion
			#region IncludeChildAccounts
			public abstract class includeChildAccounts : PX.Data.BQL.BqlBool.Field<includeChildAccounts> { }

			[PXDBBool]
			[PXDefault(typeof(Search<CS.FeaturesSet.parentChildAccount>))]
			[PXUIField(DisplayName = "Consolidate by Parent")]
			public virtual bool? IncludeChildAccounts { get; set; }
			#endregion
			
		}

        public partial class ARHistorySummary : IBqlTable
	    {
            #region CuryBalanceSummary
            public abstract class curyBalanceSummary : PX.Data.BQL.BqlDecimal.Field<curyBalanceSummary> { }

            [PXCury(typeof(ARHistoryFilter.curyID))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Balance (Currency)", Enabled = false)]
            public virtual decimal? CuryBalanceSummary
            {
                get;
                set;
            }
            #endregion
            #region BalanceSummary
            public abstract class balanceSummary : PX.Data.BQL.BqlDecimal.Field<balanceSummary> { }

            [PXBaseCury]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Balance", Enabled = false)]
            public virtual decimal? BalanceSummary
            {
                get;
                set;
            }
            #endregion
			#region IncludeChildAccounts
			public abstract class includeChildAccounts : PX.Data.BQL.BqlBool.Field<includeChildAccounts> { }
			public virtual bool? IncludeChildAccounts { get; set; }
			#endregion
			#region CuryRevaluedSummary
			public abstract class curyRevaluedSummary : PX.Data.BQL.BqlDecimal.Field<curyRevaluedSummary> { }

            [PXCury(typeof(ARHistoryFilter.curyID))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Unrealized Gain/Loss", Enabled = false)]
            public virtual decimal? CuryRevaluedSummary
            {
                get;
                set;
            }
			#endregion
			#region RevaluedSummary
			public abstract class revaluedSummary : PX.Data.BQL.BqlDecimal.Field<revaluedSummary> { }

            [PXBaseCury]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Unrealized Gain/Loss", Enabled = false)]
            public virtual decimal? RevaluedSummary
            {
                get;
                set;
            }
            #endregion
            #region CuryDepositsSummary
            public abstract class curyDepositsSummary : PX.Data.BQL.BqlDecimal.Field<curyDepositsSummary> { }

            [PXCury(typeof(ARHistoryFilter.curyID))]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Prepayments (Currency)", Enabled = false)]
            public virtual decimal? CuryDepositsSummary
            {
                get;
                set;
            }
            #endregion
            #region DepositsSummary
            public abstract class depositsSummary : PX.Data.BQL.BqlDecimal.Field<depositsSummary> { }
            [PXBaseCury]
            [PXDefault(TypeCode.Decimal, "0.0")]
            [PXUIField(DisplayName = "Total Prepayments", Enabled = false)]
            public virtual decimal? DepositsSummary
            {
                get;
                set;
            }
			#endregion
			#region CuryBalanceRetainedSummary
			public abstract class curyBalanceRetainedSummary : PX.Data.BQL.BqlDecimal.Field<curyBalanceRetainedSummary> { }

			[PXBaseCury(typeof(ARHistoryFilter.curyID))]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Retained Balance (Currency)", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual decimal? CuryBalanceRetainedSummary
			{
				get;
				set;
			}
			#endregion
			#region BalanceRetainedSummary
			public abstract class balanceRetainedSummary : PX.Data.BQL.BqlDecimal.Field<balanceRetainedSummary> { }

			[PXBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Total Retained Balance", Enabled = false, FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual decimal? BalanceRetainedSummary
			{
				get;
				set;
			}
			#endregion
			#region Calculated
			public abstract class calculated : PX.Data.BQL.BqlBool.Field<calculated> { }
			/// <summary>
			/// Specifies (if set to <c>true</c>) that the <see cref="ARCustomerBalanceEnq.history"/> delegate calculated the summary.
			/// </summary>
			[PXBool]
			[PXDefault(false)]
			public virtual bool? Calculated
			{
				get;
				set;
			}
			#endregion
			public virtual void ClearSummary()
			{
				this.BalanceSummary = decimal.Zero;
				this.RevaluedSummary = decimal.Zero;
				this.DepositsSummary = decimal.Zero;
				this.CuryBalanceSummary = decimal.Zero;
				this.CuryRevaluedSummary = decimal.Zero;
				this.CuryDepositsSummary = decimal.Zero;
				this.BalanceRetainedSummary = decimal.Zero;
				this.CuryBalanceRetainedSummary = decimal.Zero;
				this.Calculated = false;
			}
		}

		[Serializable]
		public partial class ARHistoryResult : IBqlTable
		{
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }

            [PXDBInt]
			[PXDefault]
			public virtual int? CustomerID
			{
				get;
				set;
			}
			#endregion
			#region AcctCD
			public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
            [PXDimensionSelector(CustomerAttribute.DimensionName, typeof(CA.Light.Customer.acctCD), typeof(acctCD),
                typeof(CA.Light.Customer.acctCD), typeof(CA.Light.Customer.acctName))]
            [PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXUIField(DisplayName = "Customer ID", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string AcctCD
			{
				get;
				set;
			}
			#endregion
			#region AcctName
			public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }

			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Customer Name", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string AcctName
			{
				get;
				set;
			}
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

			[GL.FinPeriodID]
			[PXUIField(DisplayName = "Last Activity Period", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string FinPeriodID
			{
				get;
				set;
			}
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }

			[PXDBString(5, IsUnicode = true, IsKey=true)]
			[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string CuryID
			{
				get;
				set;
			}
			#endregion
			#region CuryBegBalance
			public abstract class curyBegBalance : PX.Data.BQL.BqlDecimal.Field<curyBegBalance> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Beginning Balance", Visible = false)]
			public virtual decimal? CuryBegBalance
			{
				get;
				set;
			}
			#endregion
			#region BegBalance
			public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "Beginning Balance", Visible = false)]
			public virtual decimal? BegBalance
			{
				get;
				set;
			}
			#endregion
			#region CuryEndBalance
			public abstract class curyEndBalance : PX.Data.BQL.BqlDecimal.Field<curyEndBalance> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Ending Balance", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
			public virtual decimal? CuryEndBalance
			{
				get;
				set;
			}
			#endregion
			#region EndBalance
			public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "Ending Balance", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
			public virtual decimal? EndBalance
			{
				get;
				set;
			}
			#endregion
			#region CuryBalance
			public abstract class curyBalance : PX.Data.BQL.BqlDecimal.Field<curyBalance> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Balance", Visible = false)]
			public virtual decimal? CuryBalance
			{
				get;
				set;
			}
			#endregion
			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "Balance", Visible=false)]
			public virtual decimal? Balance
			{
				get;
				set;
			}
			#endregion

			#region CurySales
			public abstract class curySales : PX.Data.BQL.BqlDecimal.Field<curySales> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Sales")]
			public virtual decimal? CurySales
			{
				get;
				set;
			}
			#endregion
			#region Sales
			public abstract class sales : PX.Data.BQL.BqlDecimal.Field<sales> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "PTD Sales")]
			public virtual decimal? Sales
			{
				get;
				set;
			}
			#endregion

			#region CuryPayments
			public abstract class curyPayments : PX.Data.BQL.BqlDecimal.Field<curyPayments> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Payments")]
			public virtual decimal? CuryPayments
			{
				get;
				set;
			}
			#endregion
			#region Payments
			public abstract class payments : PX.Data.BQL.BqlDecimal.Field<payments> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "PTD Payments")]
			public virtual decimal? Payments
			{
				get;
				set;
			}
			#endregion
			#region CuryDiscount
			public abstract class curyDiscount : PX.Data.BQL.BqlDecimal.Field<curyDiscount> { } 

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Cash Discount Taken")]
			public virtual decimal? CuryDiscount
			{
				get;
				set;
			}
			#endregion
			#region Discount
			public abstract class discount : PX.Data.BQL.BqlDecimal.Field<discount> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "PTD Cash Discount Taken")]
			public virtual decimal? Discount
			{
				get;
				set;
			}
			#endregion
			#region RGOL
			public abstract class rGOL : PX.Data.BQL.BqlDecimal.Field<rGOL> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "PTD Realized Gain/Loss")]
			public virtual decimal? RGOL
			{
				get;
				set;
			}
			#endregion
			#region CuryCrAdjustments
			public abstract class curyCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyCrAdjustments> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Credit Memos")]
			public virtual decimal? CuryCrAdjustments
			{
				get;
				set;
			}
			#endregion
			#region CrAdjustments
			public abstract class crAdjustments : PX.Data.BQL.BqlDecimal.Field<crAdjustments> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "PTD Credit Memos")]
			public virtual decimal? CrAdjustments
			{
				get;
				set;
			}
			#endregion
			#region CuryDrAdjustments
			public abstract class curyDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyDrAdjustments> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Debit Memos")]
			public virtual decimal? CuryDrAdjustments
			{
				get;
				set;
			}
			#endregion
			#region DrAdjustments
			public abstract class drAdjustments : PX.Data.BQL.BqlDecimal.Field<drAdjustments> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "PTD Debit Memos")]
			public virtual decimal? DrAdjustments
			{
				get;
				set;
			}
			#endregion
			#region COGS
			public abstract class cOGS : PX.Data.BQL.BqlDecimal.Field<cOGS> { }

			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "PTD COGS")]
			public virtual decimal? COGS
			{
				get;
				set;
			}
			#endregion
			#region FinPtdRevaluated
			public abstract class finPtdRevaluated : PX.Data.BQL.BqlDecimal.Field<finPtdRevaluated> { }

			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Unrealized Gain/Loss")]
			public virtual decimal? FinPtdRevaluated
			{
				get;
				set;
			}
			#endregion
			#region CuryFinCharges
			public abstract class curyFinCharges : PX.Data.BQL.BqlDecimal.Field<curyFinCharges> { }

			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "Currency PTD Overdue Charges", Visible = true)]
			public virtual decimal? CuryFinCharges
			{
				get;
				set;
			}
			#endregion
			#region FinCharges
			public abstract class finCharges : PX.Data.BQL.BqlDecimal.Field<finCharges> { }

			[PXDBBaseCury]
			[PXDefault(TypeCode.Decimal, "0.0")]
			[PXUIField(DisplayName = "PTD Overdue Charges", Visible = true)]
			public virtual decimal? FinCharges
			{
				get;
				set;
			}
			#endregion
			#region CuryDeposits
			public abstract class curyDeposits : PX.Data.BQL.BqlDecimal.Field<curyDeposits> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Prepayments")]
			public virtual decimal? CuryDeposits
			{
				get;
				set;
			}
			#endregion
			#region Deposits
			public abstract class deposits : PX.Data.BQL.BqlDecimal.Field<deposits> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "PTD Prepayments")]
			public virtual decimal? Deposits
			{
				get;
				set;
			}
			#endregion
			#region CuryDepositsBalance
			public abstract class curyDepositsBalance : PX.Data.BQL.BqlDecimal.Field<curyDepositsBalance> { }

            [PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Prepayments Balance")]
			public virtual decimal? CuryDepositsBalance
			{
				get;
				set;
			}
			#endregion
			#region DepositsBalance
			public abstract class depositsBalance : PX.Data.BQL.BqlDecimal.Field<depositsBalance> { }

			[PXBaseCury]
			[PXUIField(DisplayName = "Prepayments Balance")]
			public virtual decimal? DepositsBalance
			{
				get;
				set;
			}
			#endregion
			#region CuryRetainageWithheld
			public abstract class curyRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyRetainageWithheld> { }
			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Retainage Withheld", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? CuryRetainageWithheld { get; set; }
			#endregion
			#region RetainageWithheld
			public abstract class retainageWithheld : PX.Data.BQL.BqlDecimal.Field<retainageWithheld> { }
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Retainage Withheld", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? RetainageWithheld { get; set; }

			#endregion
			#region CuryRetainageReleased
			public abstract class curyRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyRetainageReleased> { }
			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Retainage Released", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? CuryRetainageReleased { get; set; }
			#endregion
			#region RetainageReleased
			public abstract class retainageReleased : PX.Data.BQL.BqlDecimal.Field<retainageReleased> { }
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Retainage Released", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? RetainageReleased { get; set; }

			#endregion
			#region CuryBegRetainedBalance
			public abstract class curyBegRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyBegRetainedBalance> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Beginning Retained Balance", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? CuryBegRetainedBalance { get; set; }
			#endregion
			#region BegRetainedBalance
			public abstract class begRetainedBalance : PX.Data.BQL.BqlDecimal.Field<begRetainedBalance> { }

			[PXBaseCury()]
			[PXUIField(DisplayName = "Beginning Retained Balance", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? BegRetainedBalance { get; set; }
			#endregion
			#region CuryEndRetainedBalance
			public abstract class curyEndRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyEndRetainedBalance> { }

			[PXDBCury(typeof(ARHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Ending Retained Balance", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? CuryEndRetainedBalance { get; set; }
			#endregion
			#region EndRetainedBalance
			public abstract class endRetainedBalance : PX.Data.BQL.BqlDecimal.Field<endRetainedBalance> { }

			[PXBaseCury()]
			[PXUIField(DisplayName = "Ending Retained Balance", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? EndRetainedBalance { get; set; }
			#endregion
			#region Converted
			public abstract class converted : PX.Data.BQL.BqlBool.Field<converted> { }

			[PXDBBool]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Converted to Base Currency", Visible = false, Enabled = false)]
			public virtual bool? Converted
			{
				get;
				set;
			}
			#endregion
			#region NoteID
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

			[PXNote]
			public virtual Guid? NoteID
			{
				get;
				set;
			}
			#endregion
			public virtual void RecalculateEndBalance()
			{
				const decimal zero = 0m;
				this.RecalculateBalance();
				this.EndBalance = (this.BegBalance ?? zero) +
							   +(this.Balance ?? zero);
				this.CuryEndBalance = (this.CuryBegBalance ?? zero) +
							   +(this.CuryBalance ?? zero);
			}

			public virtual void RecalculateBalance()
			{
				const decimal zero = 0m;
				this.Balance = (this.Sales ?? zero)
							   - (this.Payments ?? zero)
							   - (this.Discount ?? zero)							   
							   + (this.RGOL ?? zero)
							   - (this.CrAdjustments ?? zero)
							   + (this.FinCharges ?? zero)
							   + (this.DrAdjustments ?? zero);
				this.CuryBalance = (this.CurySales ?? zero)
							   - (this.CuryPayments ?? zero)
							   - (this.CuryDiscount ?? zero)							   
							   - (this.CuryCrAdjustments ?? zero)
							   + (this.CuryFinCharges ?? zero)
							   + (this.CuryDrAdjustments ?? zero);
							   
			}

			public virtual void CopyValueToCuryValue(string aBaseCuryID)
			{
				this.CuryBegBalance = this.BegBalance ?? Decimal.Zero;
				this.CurySales = this.Sales ?? Decimal.Zero;
				this.CuryPayments = this.Payments ?? Decimal.Zero;
				this.CuryDiscount = this.Discount ?? Decimal.Zero;
				this.CuryFinCharges = this.FinCharges?? Decimal.Zero;
				this.CuryCrAdjustments = this.CrAdjustments ?? Decimal.Zero;
				this.CuryDrAdjustments = this.DrAdjustments ?? Decimal.Zero;
				this.CuryDeposits = this.Deposits ?? Decimal.Zero;
				this.CuryDepositsBalance = this.DepositsBalance ?? Decimal.Zero;
				this.CuryEndBalance = this.EndBalance ?? Decimal.Zero;
				this.CuryRetainageWithheld = this.RetainageWithheld ?? Decimal.Zero;
				this.CuryRetainageReleased = this.RetainageReleased ?? Decimal.Zero;
				this.CuryBegRetainedBalance = this.BegRetainedBalance ?? Decimal.Zero;
				this.CuryEndRetainedBalance = this.EndRetainedBalance ?? Decimal.Zero;
				this.CuryID = aBaseCuryID;
				this.Converted = true;
			}
		}
        #endregion
		[Serializable]
		[PXHidden]
		[PXProjection(typeof(Select<BAccount>))]
		public class ChildBAccount : IBqlTable
		{
			#region BAccountID
			public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
			[PXDBInt(IsKey = true, BqlTable = typeof(BAccount))]
			public virtual int? BAccountID { get; set; }
			#endregion
			#region ConsolidatingBAccountID
			public abstract class consolidatingBAccountID : PX.Data.BQL.BqlInt.Field<consolidatingBAccountID> { }
			[PXDBInt(BqlTable = typeof(BAccount))]
			public virtual int? ConsolidatingBAccountID { get; set; }
			#endregion
		}

        [Serializable]
		[PXProjection(typeof(Select4<CuryARHistory,
			Aggregate<
			GroupBy<CuryARHistory.branchID,
			GroupBy<CuryARHistory.customerID,
			GroupBy<CuryARHistory.accountID,
			GroupBy<CuryARHistory.subID,
			GroupBy<CuryARHistory.curyID,
			Max<CuryARHistory.finPeriodID
			>>>>>>>>))]
		[PXCacheName(Messages.ARLatestHistory)]
		public partial class ARLatestHistory : PX.Data.IBqlTable
		{
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected Int32? _BranchID;
			[PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.branchID))]
			[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD))]
			public virtual Int32? BranchID
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
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			protected Int32? _CustomerID;
			[PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.customerID))]
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
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			protected Int32? _AccountID;
			[PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.accountID))]
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
			[PXDBInt(IsKey = true, BqlField = typeof(CuryARHistory.subID))]
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
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(CuryARHistory.curyID))]
			public virtual String CuryID
			{
				get
				{
					return this._CuryID;
				}
				set
				{
					this._CuryID = value;
				}
			}
			#endregion
			#region LastActivityPeriod
			public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
			protected String _LastActivityPeriod;
			[GL.FinPeriodID(BqlField = typeof(CuryARHistory.finPeriodID))]
			public virtual String LastActivityPeriod
			{
				get
				{
					return this._LastActivityPeriod;
				}
				set
				{
					this._LastActivityPeriod = value;
				}
			}
			#endregion
		}

		[Serializable]
		[PXHidden]
		[PXProjection(typeof(Select<CuryARHistory>))]
		public class CuryARHistoryTran : IBqlTable
		{
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			[PXDBInt(IsKey = true, BqlTable = typeof(CuryARHistory))]
			public virtual int? BranchID { get; set; }
			#endregion
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			[PXDBInt(IsKey = true, BqlTable = typeof(CuryARHistory))]
			public virtual int? AccountID { get; set; }
			#endregion
			#region SubID
			public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
			[PXDBInt(IsKey = true, BqlTable = typeof(CuryARHistory))]
			public virtual int? SubID { get; set; }
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			[PXDBString(/*IsKey = true, */BqlTable = typeof(CuryARHistory))]
			public virtual string CuryID { get; set; }
			#endregion
			#region CustomerID
			public abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			[PXDBInt(IsKey = true, BqlTable = typeof(CuryARHistory))]
			public virtual int? CustomerID { get; set; }
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			[PXDBString(/*IsKey = true,*/ BqlTable = typeof(CuryARHistory))]
			public virtual string FinPeriodID { get; set; }
			#endregion
			#region FinBegBalance
			public abstract class finBegBalance : PX.Data.BQL.BqlDecimal.Field<finBegBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finBegBalance>, CuryARHistory.finYtdBalance>), typeof(decimal))]
			public virtual decimal? FinBegBalance { get; set; }
			#endregion
			#region FinPtdSales
			public abstract class finPtdSales : PX.Data.BQL.BqlDecimal.Field<finPtdSales> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdSales>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdSales { get; set; }
			#endregion
			#region FinPtdPayments
			public abstract class finPtdPayments : PX.Data.BQL.BqlDecimal.Field<finPtdPayments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdPayments>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdPayments { get; set; }
			#endregion
			#region FinPtdDrAdjustments
			public abstract class finPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdDrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdDrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdDrAdjustments { get; set; }
			#endregion
			#region FinPtdCrAdjustments
			public abstract class finPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdCrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdCrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdCrAdjustments { get; set; }
			#endregion
			#region FinPtdDiscounts
			public abstract class finPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<finPtdDiscounts> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdDiscounts>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdDiscounts { get; set; }
			#endregion
			#region FinPtdCOGS
			public abstract class finPtdCOGS : PX.Data.BQL.BqlDecimal.Field<finPtdCOGS> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdCOGS>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdCOGS { get; set; }
			#endregion
			#region FinPtdRGOL
			public abstract class finPtdRGOL : PX.Data.BQL.BqlDecimal.Field<finPtdRGOL> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdRGOL>, Zero>>), typeof(decimal))]
			public virtual decimal? FinPtdRGOL { get; set; }
			#endregion
			#region FinPtdFinCharges
			public abstract class finPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<finPtdFinCharges> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdFinCharges>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdFinCharges { get; set; }
			#endregion
			#region FinYtdBalance
			public abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? FinYtdBalance { get; set; }
			#endregion
			#region FinPtdDeposits
			public abstract class finPtdDeposits : PX.Data.BQL.BqlDecimal.Field<finPtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdDeposits>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdDeposits { get; set; }
			#endregion
			#region FinYtdDeposits
			public abstract class finYtdDeposits : PX.Data.BQL.BqlDecimal.Field<finYtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<CuryARHistory.finYtdDeposits>), typeof(decimal))]
			public virtual decimal? FinYtdDeposits { get; set; }
			#endregion
			#region TranBegBalance
			public abstract class tranBegBalance : PX.Data.BQL.BqlDecimal.Field<tranBegBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranBegBalance>, CuryARHistory.tranYtdBalance>), typeof(decimal))]
			public virtual decimal? TranBegBalance { get; set; }
			#endregion
			#region TranPtdSales
			public abstract class tranPtdSales : PX.Data.BQL.BqlDecimal.Field<tranPtdSales> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdSales>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdSales { get; set; }
			#endregion
			#region TranPtdPayments
			public abstract class tranPtdPayments : PX.Data.BQL.BqlDecimal.Field<tranPtdPayments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdPayments>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdPayments { get; set; }
			#endregion
			#region TranPtdDrAdjustments
			public abstract class tranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdDrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdDrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdDrAdjustments { get; set; }
			#endregion
			#region TranPtdCrAdjustments
			public abstract class tranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdCrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdCrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdCrAdjustments { get; set; }
			#endregion
			#region TranPtdDiscounts
			public abstract class tranPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<tranPtdDiscounts> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdDiscounts>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdDiscounts { get; set; }
			#endregion
			#region TranPtdRGOL
			public abstract class tranPtdRGOL : PX.Data.BQL.BqlDecimal.Field<tranPtdRGOL> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdRGOL>, Zero>>), typeof(decimal))]
			public virtual decimal? TranPtdRGOL { get; set; }
			#endregion
			#region TranPtdCOGS
			public abstract class tranPtdCOGS : PX.Data.BQL.BqlDecimal.Field<tranPtdCOGS> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdCOGS>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdCOGS { get; set; }
			#endregion
			#region TranPtdFinCharges
			public abstract class tranPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<tranPtdFinCharges> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdFinCharges>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdFinCharges { get; set; }
			#endregion
			#region TranYtdBalance
			public abstract class tranYtdBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdBalance> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? TranYtdBalance { get; set; }
			#endregion
			#region TranPtdDeposits
			public abstract class tranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranPtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdDeposits>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdDeposits { get; set; }
			#endregion
			#region TranYtdDeposits
			public abstract class tranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranYtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<CuryARHistory.tranYtdDeposits>), typeof(decimal))]
			public virtual decimal? TranYtdDeposits { get; set; }
			#endregion
			#region CuryFinBegBalance
			public abstract class curyFinBegBalance : PX.Data.BQL.BqlDecimal.Field<curyFinBegBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinBegBalance>, CuryARHistory.curyFinYtdBalance>), typeof(decimal))]
			public virtual decimal? CuryFinBegBalance { get; set; }
			#endregion
			#region CuryFinPtdSales
			public abstract class curyFinPtdSales : PX.Data.BQL.BqlDecimal.Field<curyFinPtdSales> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdSales>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdSales { get; set; }
			#endregion
			#region CuryFinPtdPayments
			public abstract class curyFinPtdPayments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdPayments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdPayments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdPayments { get; set; }
			#endregion
			#region CuryFinPtdDrAdjustments
			public abstract class curyFinPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdDrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdDrAdjustments { get; set; }
			#endregion
			#region CuryFinPtdCrAdjustments
			public abstract class curyFinPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdCrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdCrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdCrAdjustments { get; set; }
			#endregion
			#region CuryFinPtdDiscounts
			public abstract class curyFinPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDiscounts> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdDiscounts>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdDiscounts { get; set; }
			#endregion
			#region CuryFinPtdFinCharges
			public abstract class curyFinPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<curyFinPtdFinCharges> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdFinCharges>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdFinCharges { get; set; }
			#endregion
			#region CuryFinYtdBalance
			public abstract class curyFinYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalance> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? CuryFinYtdBalance { get; set; }
			#endregion
			#region CuryFinPtdDeposits
			public abstract class curyFinPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdDeposits>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdDeposits { get; set; }
			#endregion
			#region CuryFinYtdDeposits
			public abstract class curyFinYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinYtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<CuryARHistory.curyFinYtdDeposits>), typeof(decimal))]
			public virtual decimal? CuryFinYtdDeposits { get; set; }
			#endregion
			#region CuryTranBegBalance
			public abstract class curyTranBegBalance : PX.Data.BQL.BqlDecimal.Field<curyTranBegBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranBegBalance>, CuryARHistory.curyTranYtdBalance>), typeof(decimal))]
			public virtual decimal? CuryTranBegBalance { get; set; }
			#endregion
			#region CuryTranPtdSales
			public abstract class curyTranPtdSales : PX.Data.BQL.BqlDecimal.Field<curyTranPtdSales> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdSales>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdSales { get; set; }
			#endregion
			#region CuryTranPtdPayments
			public abstract class curyTranPtdPayments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdPayments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdPayments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdPayments { get; set; }
			#endregion
			#region CuryTranPtdDrAdjustments
			public abstract class curyTranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdDrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdDrAdjustments { get; set; }
			#endregion
			#region CuryTranPtdCrAdjustments
			public abstract class curyTranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdCrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdCrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdCrAdjustments { get; set; }
			#endregion
			#region CuryTranPtdDiscounts
			public abstract class curyTranPtdDiscounts : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDiscounts> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdDiscounts>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdDiscounts { get; set; }
			#endregion
			#region CuryTranPtdFinCharges
			public abstract class curyTranPtdFinCharges : PX.Data.BQL.BqlDecimal.Field<curyTranPtdFinCharges> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdFinCharges>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdFinCharges { get; set; }
			#endregion
			#region CuryTranYtdBalance
			public abstract class curyTranYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyTranYtdBalance> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? CuryTranYtdBalance { get; set; }
			#endregion
			#region CuryTranPtdDeposits
			public abstract class curyTranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdDeposits>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdDeposits { get; set; }
			#endregion
			#region CuryTranYtdDeposits
			public abstract class curyTranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranYtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<CuryARHistory.curyTranYtdDeposits>), typeof(decimal))]
			public virtual decimal? CuryTranYtdDeposits { get; set; }
			#endregion
			#region DetDeleted
			public abstract class detDeleted : PX.Data.BQL.BqlBool.Field<detDeleted> { }
			[PXDBBool(BqlTable = typeof(CuryARHistory))]
			public virtual bool? DetDeleted { get; set; }
			#endregion
			#region FinPtdRevalued
			public abstract class finPtdRevalued : PX.Data.BQL.BqlDecimal.Field<finPtdRevalued> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdRevalued>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdRevalued { get; set; }
			#endregion
			#region CuryFinPtdRetainageWithheld
			public abstract class curyFinPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyFinPtdRetainageWithheld> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdRetainageWithheld>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdRetainageWithheld { get; set; }
			#endregion
			#region FinPtdRetainageWithheld
			public abstract class finPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageWithheld> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdRetainageWithheld>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdRetainageWithheld { get; set; }
			#endregion
			#region CuryTranPtdRetainageWithheld
			public abstract class curyTranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyTranPtdRetainageWithheld> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdRetainageWithheld>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdRetainageWithheld { get; set; }
			#endregion
			#region TranPtdRetainageWithheld
			public abstract class tranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageWithheld> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdRetainageWithheld>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdRetainageWithheld { get; set; }
			#endregion
			#region CuryFinYtdRetainageWithheld
			public abstract class curyFinYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageWithheld> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? CuryFinYtdRetainageWithheld { get; set; }
			#endregion
			#region FinYtdRetainageWithheld
			public abstract class finYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageWithheld> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? FinYtdRetainageWithheld { get; set; }
			#endregion
			#region curyFinRetainedBalance
			public abstract class curyFinBegRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyFinBegRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
				Sub<CuryARHistory.curyFinYtdRetainageWithheld,
				Add<CuryARHistory.curyFinYtdRetainageReleased, 
				Sub<Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdRetainageWithheld>, Zero>,
				Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdRetainageReleased>, Zero>>>>),
				typeof(decimal))]
			public virtual decimal? CuryFinBegRetainedBalance { get; set; }
			#endregion
			#region FinRetainedBalance
			public abstract class finBegRetainedBalance : PX.Data.BQL.BqlDecimal.Field<finBegRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
				Sub<CuryARHistory.finYtdRetainageWithheld, 
				Add<CuryARHistory.finYtdRetainageReleased,
				Sub<Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdRetainageWithheld>, Zero>,
				Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdRetainageReleased>, Zero>>>>),
				typeof(decimal))]
			public virtual decimal? FinBegRetainedBalance { get; set; }
			#endregion
			#region curyTranRetainedBalance
			public abstract class curyTranBegRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyTranBegRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
				Sub<CuryARHistory.curyTranYtdRetainageWithheld,
				Add<CuryARHistory.curyTranYtdRetainageReleased,
				Sub<Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdRetainageWithheld>, Zero>,
				Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdRetainageReleased>, Zero>>>>),
				typeof(decimal))]
			public virtual decimal? CuryTranBegRetainedBalance { get; set; }
			#endregion
			#region TranRetainedBalance
			public abstract class tranBegRetainedBalance : PX.Data.BQL.BqlDecimal.Field<tranBegRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
				Sub<CuryARHistory.tranYtdRetainageWithheld,
				Add<CuryARHistory.tranYtdRetainageReleased,
				Sub<Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdRetainageWithheld>, Zero>,
				Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdRetainageReleased>, Zero>>>>),
				typeof(decimal))]
			public virtual decimal? TranBegRetainedBalance { get; set; }
			#endregion
			#region CuryFinYtdRetainedBalance
			public abstract class curyFinYtdRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
					Sub<CuryARHistory.curyFinYtdRetainageWithheld,
						CuryARHistory.curyFinYtdRetainageReleased>),
				typeof(decimal))]
			public virtual decimal? CuryFinYtdRetainedBalance { get; set; }
			#endregion
			#region FinYtdRetainedBalance
			public abstract class finYtdRetainedBalance : PX.Data.BQL.BqlDecimal.Field<finYtdRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
					Sub<CuryARHistory.finYtdRetainageWithheld,
						CuryARHistory.finYtdRetainageReleased>),
				typeof(decimal))]
			public virtual decimal? FinYtdRetainedBalance { get; set; }
			#endregion
			#region CuryTranYtdRetainedBalance
			public abstract class curyTranYtdRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
					Sub<CuryARHistory.curyTranYtdRetainageWithheld,
						CuryARHistory.curyTranYtdRetainageReleased>),
				typeof(decimal))]
			public virtual decimal? CuryTranYtdRetainedBalance { get; set; }
			#endregion
			#region FinYtdRetainedBalance
			public abstract class tranYtdRetainedBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
					Sub<CuryARHistory.tranYtdRetainageWithheld,
						CuryARHistory.tranYtdRetainageReleased>),
				typeof(decimal))]
			public virtual decimal? TranYtdRetainedBalance { get; set; }
			#endregion
			#region CuryTranYtdRetainageWithheld
			public abstract class curyTranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageWithheld> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? CuryTranYtdRetainageWithheld { get; set; }
			#endregion
			#region TranYtdRetainageWithheld
			public abstract class tranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageWithheld> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? TranYtdRetainageWithheld { get; set; }
			#endregion
			#region CuryFinPtdRetainageReleased
			public abstract class curyFinPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyFinPtdRetainageReleased> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyFinPtdRetainageReleased>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdRetainageReleased { get; set; }
			#endregion
			#region FinPtdRetainageReleased
			public abstract class finPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageReleased> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.finPtdRetainageReleased>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdRetainageReleased { get; set; }
			#endregion
			#region CuryTranPtdRetainageReleased
			public abstract class curyTranPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyTranPtdRetainageReleased> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.curyTranPtdRetainageReleased>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdRetainageReleased { get; set; }
			#endregion
			#region TranPtdRetainageReleased
			public abstract class tranPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageReleased> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryARHistory.finPeriodID, Equal<CurrentValue<ARHistoryFilter.period>>>, CuryARHistory.tranPtdRetainageReleased>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdRetainageReleased { get; set; }
			#endregion
			#region CuryFinYtdRetainageReleased
			public abstract class curyFinYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageReleased> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? CuryFinYtdRetainageReleased { get; set; }
			#endregion
			#region FinYtdRetainageReleased
			public abstract class finYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageReleased> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? FinYtdRetainageReleased { get; set; }
			#endregion
			#region CuryTranYtdRetainageReleased
			public abstract class curyTranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageReleased> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? CuryTranYtdRetainageReleased { get; set; }
			#endregion
			#region TranYtdRetainageReleased
			public abstract class tranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageReleased> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryARHistory))]
			public virtual decimal? TranYtdRetainageReleased { get; set; }
			#endregion
		}

		private sealed class decimalZero : PX.Data.BQL.BqlDecimal.Constant<decimalZero>
		{
			public decimalZero()
				: base(0m)
			{
			}
		}

		#region Public Membsers
		public PXFilter<ARHistoryFilter> Filter;
        [PXFilterable]
		public PXSelect<ARHistoryResult> History;
        [PXVirtualDAC]
        public PXFilter<ARHistorySummary> Summary;
        public PXSetup<ARSetup> ARSetup;
		public PXSetup<Company> Company;
		#endregion
		
		#region Ctor + Overrides

	    public ARCustomerBalanceEnq()
	    {
	        ARSetup setup = ARSetup.Current;
	        Company company = this.Company.Current;
	        this.History.Cache.AllowDelete = false;
	        this.History.Cache.AllowInsert = false;
	        this.History.Cache.AllowUpdate = false;
	        this.reports.MenuAutoOpen = true;
	        this.reports.AddMenuAction(this.aRBalanceByCustomerReport);
	        this.reports.AddMenuAction(this.customerHistoryReport);
	        this.reports.AddMenuAction(this.aRAgedPastDueReport);
	        this.reports.AddMenuAction(this.aRAgedOutstandingReport);
	    }
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Actions

		public PXCancel<ARHistoryFilter> Cancel;
		public PXAction<ARHistoryFilter> viewDetails;
		public PXAction<ARHistoryFilter> previousPeriod;
		public PXAction<ARHistoryFilter> nextPeriod;
		public PXAction<ARHistoryFilter> aRBalanceByCustomerReport;
		public PXAction<ARHistoryFilter> customerHistoryReport;
		public PXAction<ARHistoryFilter> aRAgedPastDueReport;
		public PXAction<ARHistoryFilter> aRAgedOutstandingReport;
		public PXAction<ARHistoryFilter> reports;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable PreviousPeriod(PXAdapter adapter)
		{
			ARHistoryFilter filter = Filter.Current as ARHistoryFilter;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.Period, looped: true);

			filter.Period = prevPeriod?.FinPeriodID;
			this.Summary.Current.ClearSummary();
			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextPeriod(PXAdapter adapter)
		{
			ARHistoryFilter filter = Filter.Current as ARHistoryFilter;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod nextPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.Period, looped: true);

			filter.Period = nextPeriod?.FinPeriodID;
			this.Summary.Current.ClearSummary();
			return adapter.Get();
		}

		[PXUIField(DisplayName = "View Details", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true, Enabled = true)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			if (this.History.Current != null && this.Filter.Current != null)
			{
				ARHistoryResult res = this.History.Current;
				ARHistoryFilter currentFilter = this.Filter.Current;
				ARDocumentEnq graph = PXGraph.CreateInstance<ARDocumentEnq>();

				ARDocumentEnq.ARDocumentFilter filter = graph.Filter.Current;
				Copy(filter, currentFilter);
				filter.CustomerID = res.CustomerID;				
				filter.BalanceSummary = null;
				graph.Filter.Update(filter);
				filter = graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Customer Details");

			}
			return Filter.Select();
		}

		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Reports(PXAdapter adapter)
		{
			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.ARBalanceByCustomerReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ARBalanceByCustomerReport(PXAdapter adapter)
		{
			ARHistoryFilter filter = Filter.Current;
			ARHistoryResult history = History.Current;

			if (filter != null && history != null)
			{
                CA.Light.Customer customer = PXSelect<CA.Light.Customer, Where<CA.Light.Customer.bAccountID, Equal<Current<ARHistoryResult.customerID>>>>.Select(this);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				if (!string.IsNullOrEmpty(filter.Period))
				{
					parameters["PeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.Period);
				}
				parameters["CustomerID"] = customer.AcctCD;
				parameters["UseMasterCalendar"] = filter.UseMasterCalendar == true ? true.ToString() : false.ToString();
				throw new PXReportRequiredException(parameters, "AR632500", PXBaseRedirectException.WindowMode.NewWindow, Messages.ARBalanceByCustomerReport);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.CustomerHistoryReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable CustomerHistoryReport(PXAdapter adapter)
		{
			ARHistoryFilter filter = Filter.Current;
			ARHistoryResult history = History.Current;
			if (filter != null && history != null)
			{
                CA.Light.Customer customer = PXSelect<CA.Light.Customer, Where<CA.Light.Customer.bAccountID, Equal<Current<ARHistoryResult.customerID>>>>.Select(this);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				if (!string.IsNullOrEmpty(filter.Period))
				{
					parameters["FromPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.Period);
					parameters["ToPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.Period);
				}
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR652000", PXBaseRedirectException.WindowMode.NewWindow , Messages.CustomerHistoryReport);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.ARAgedPastDueReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ARAgedPastDueReport(PXAdapter adapter)
		{
			ARHistoryResult history = History.Current;
			if (history != null)
			{
                CA.Light.Customer customer = PXSelect<CA.Light.Customer, Where<CA.Light.Customer.bAccountID, Equal<Current<ARHistoryResult.customerID>>>>.Select(this);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR631000" ,PXBaseRedirectException.WindowMode.NewWindow, Messages.ARAgedPastDueReport);
			}
			return adapter.Get();
		}


		[PXUIField(DisplayName = Messages.ARAgedOutstandingReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable ARAgedOutstandingReport(PXAdapter adapter)
		{
			ARHistoryResult history = History.Current;
			if (history != null)
			{
                CA.Light.Customer customer = PXSelect<CA.Light.Customer, Where<CA.Light.Customer.bAccountID, Equal<Current<ARHistoryResult.customerID>>>>.Select(this);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters["CustomerID"] = customer.AcctCD;
				throw new PXReportRequiredException(parameters, "AR631500", PXBaseRedirectException.WindowMode.NewWindow , Messages.ARAgedOutstandingReport);
			}
			return adapter.Get();
		}
        #endregion

        #region Select Overrides	

	    protected virtual IEnumerable history()
	    {
		    return RetrieveHistoryForPeriod(Filter.Current);
	        }

        protected virtual IEnumerable summary()
        {
            if (Summary.Current.Calculated == false)
            {
                var summary = Summary.Cache.CreateInstance() as ARHistorySummary;
                summary.ClearSummary();
                Summary.Insert(summary);
                RetrieveHistoryForPeriod(Filter.Current, summary);
	            Summary.Update(summary);
				Summary.Current.Calculated = true;

            }
            yield return Summary.Current;
        }

        protected virtual IEnumerable RetrieveHistoryForPeriod(ARHistoryFilter header, ARHistorySummary summary = null) 
		{
			bool isCurySelected = string.IsNullOrEmpty(header.CuryID) == false;
			bool splitByCurrency = header.SplitByCurrency ?? false;
			bool useFinancial = (header.UseMasterCalendar != true);
			bool IncludeChildAccounts = header.IncludeChildAccounts == true && summary == null;
			

			#region Specific Fiscal Period
			Type typeCustomer = typeof(CA.Light.Customer.bAccountID);

			List<Type> typesList = new List<Type>
			{
				typeof(Select5<,,,,>), typeof(CA.Light.Customer),
			};
				
			if (IncludeChildAccounts)
			{
				typesList.AddRange(
				new Type[]
				{
						typeof(LeftJoin<,,>),
						typeof(ChildBAccount),
						typeof(On<ChildBAccount.consolidatingBAccountID, Equal<CA.Light.Customer.bAccountID>>)
				});
				typeCustomer = typeof(ChildBAccount.bAccountID);
			}

			Type having = null;
			if (header.ShowWithBalanceOnly == true && summary == null)
			{
				if (useFinancial)
					having = typeof(Having<CuryARHistoryTran.curyFinYtdBalance.Summarized.IsNotEqual<Zero>
						.Or<CuryARHistoryTran.finYtdBalance.Summarized.IsNotEqual<Zero>>
						.Or<CuryARHistoryTran.finPtdRevalued.Summarized.IsNotEqual<Zero>>
						.Or<CuryARHistoryTran.curyFinYtdDeposits.Summarized.IsNotEqual<Zero>>
						.Or<CuryARHistoryTran.curyFinYtdRetainedBalance.Summarized.IsNotEqual<Zero>>>);
				else
					having = typeof(Having<CuryARHistoryTran.curyTranYtdBalance.Summarized.IsNotEqual<Zero>
						.Or<CuryARHistoryTran.tranYtdBalance.Summarized.IsNotEqual<Zero>>
						.Or<CuryARHistoryTran.finPtdRevalued.Summarized.IsNotEqual<Zero>>
						.Or<CuryARHistoryTran.curyTranYtdDeposits.Summarized.IsNotEqual<Zero>>
						.Or<CuryARHistoryTran.curyTranYtdRetainedBalance.Summarized.IsNotEqual<Zero>>>);
			}

			typesList.AddRange(
				new Type[]
			{
					typeof(LeftJoin<,,>),
					typeof(ARHistoryByPeriod),
					typeof(On<,>), typeof(ARHistoryByPeriod.customerID), typeof(Equal<>), typeCustomer,
					typeof(LeftJoin<CuryARHistoryTran, On<ARHistoryByPeriod.accountID, Equal<CuryARHistoryTran.accountID>,
							And<ARHistoryByPeriod.branchID, Equal<CuryARHistoryTran.branchID>,
							And<ARHistoryByPeriod.customerID, Equal<CuryARHistoryTran.customerID>,
							And<ARHistoryByPeriod.subID, Equal<CuryARHistoryTran.subID>,
							And<ARHistoryByPeriod.curyID, Equal<CuryARHistoryTran.curyID>,
							And<ARHistoryByPeriod.lastActivityPeriod, Equal<CuryARHistoryTran.finPeriodID>>>>>>>>),
					typeof(Where<ARHistoryByPeriod.finPeriodID, Equal<Current<ARHistoryFilter.period>>,
							 And<Match<CA.Light.Customer, Current<AccessInfo.userName>>>>),
					having == null ? typeof(Aggregate<>) : typeof(Aggregate<,>)
			}
			);
			typesList.AddRange(BqlHelper.GetDecimalFieldsAggregate<CuryARHistoryTran>(this));
			Type groupBy = null;
			Type orderBy = null;
			if (summary != null)
			{
				groupBy = typeof(GroupBy<ARHistoryByPeriod.curyID>);
				orderBy = typeof(OrderBy<Asc<ARHistoryByPeriod.curyID>>);
			}
			else
			{
				if (splitByCurrency == false)
				{
					groupBy = typeof(GroupBy<CA.Light.Customer.acctCD>);
					orderBy = typeof(OrderBy<Asc<CA.Light.Customer.acctCD>>);
				}
				else
				{
					groupBy = typeof(GroupBy<CA.Light.Customer.acctCD, GroupBy<ARHistoryByPeriod.curyID>>);
					orderBy = typeof(OrderBy<Asc<CA.Light.Customer.acctCD, Asc<ARHistoryByPeriod.curyID>>>);
				}
			}
			typesList.Add(groupBy);
			if (having != null)
				typesList.Add(having);
			typesList.Add(orderBy);

			Type select = BqlCommand.Compose(typesList.ToArray());
			if (!SubCDUtils.IsSubCDEmpty(header.SubCD))
			{
				select = BqlCommand.AppendJoin(select, BqlCommand.Compose(typeof(LeftJoin<Sub, On<ARHistoryByPeriod.subID, Equal<Sub.subID>>>)));
			}
			BqlCommand cmd = BqlCommand.CreateInstance(select);
			PXView view = new PXView(this, false, cmd);
			if (IncludeChildAccounts)
			{
				view.WhereAnd<Where<CA.Light.Customer.consolidatingBAccountID,
					Equal<CA.Light.Customer.consolidatingBAccountID>>>();
			}
			

            if (isCurySelected)
			{
				view.WhereAnd<Where<ARHistoryByPeriod.curyID, Equal<Current<ARHistoryFilter.curyID>>>>();
			}

			if (header.OrgBAccountID != null)
			{
				view.WhereAnd<Where<ARHistoryByPeriod.branchID, Inside<Current<ARHistoryFilter.orgBAccountID>>>>(); //MatchWithOrg
			}
			
			if (header.ARAcctID != null)
			{
				view.WhereAnd<Where<ARHistoryByPeriod.accountID, Equal<Current<ARHistoryFilter.aRAcctID>>>>();
			}

			if (header.ARSubID != null)
			{
				view.WhereAnd<Where<ARHistoryByPeriod.subID, Equal<Current<ARHistoryFilter.aRSubID>>>>();
			}

			AppendCommonWhereFilters(view, header);
			int startRow = PXView.StartRow;
			int totalRows = 0;
			var mapper = new PXResultMapper(this, useFinancial ? mapFin : mapTran, typeof(ARHistoryResult));
			mapper.ExtFilters.Add(typeof(CuryARHistoryTran));

			
			var records = summary == null
				? view.Select(null, null, PXView.Searches, mapper.SortColumns, PXView.Descendings, mapper.Filters,
					ref startRow, PXView.MaximumRows, ref totalRows)
				: view.SelectMulti();

			PXDelegateResult list = mapper.CreateDelegateResult(summary == null);

			foreach (PXResult<CA.Light.Customer> record in records)
			{
				ARHistoryResult res = mapper.CreateResult(record) as ARHistoryResult;
				res.RecalculateBalance();
				if ((!isCurySelected) && splitByCurrency == false)
				{
					res.CopyValueToCuryValue(this.Company.Current.BaseCuryID);
				}
				list.Add(res);
				if (summary != null)
				{
					Aggregate(summary, res);
				}
			}
			PXView.StartRow = 0;
			return list;

			#endregion
		}

		protected readonly Dictionary<Type, Type> mapFin = new Dictionary<Type, Type>
        {
	        {typeof(ARHistoryResult.acctCD), typeof(CA.Light.Customer.acctCD)},
	        {typeof(ARHistoryResult.acctName), typeof(CA.Light.Customer.acctName)},
	        {typeof(ARHistoryResult.customerID), typeof(CA.Light.Customer.bAccountID)},
	        {typeof(ARHistoryResult.noteID), typeof(CA.Light.Customer.noteID)},

	        {typeof(ARHistoryResult.curyID), typeof(ARHistoryByPeriod.curyID)},
	        {typeof(ARHistoryResult.finPeriodID), typeof(CuryARHistoryTran.finPeriodID)},

	        {typeof(ARHistoryResult.curyBegBalance), typeof(CuryARHistoryTran.curyFinBegBalance)},
	        {typeof(ARHistoryResult.curyEndBalance), typeof(CuryARHistoryTran.curyFinYtdBalance)},
			{typeof(ARHistoryResult.curySales), typeof(CuryARHistoryTran.curyFinPtdSales)},
	        {typeof(ARHistoryResult.curyPayments), typeof(CuryARHistoryTran.curyFinPtdPayments)},
	        {typeof(ARHistoryResult.curyDiscount), typeof(CuryARHistoryTran.curyFinPtdDiscounts)},
	        {typeof(ARHistoryResult.curyCrAdjustments), typeof(CuryARHistoryTran.curyFinPtdCrAdjustments)},
	        {typeof(ARHistoryResult.curyDrAdjustments), typeof(CuryARHistoryTran.curyFinPtdDrAdjustments)},
	        {typeof(ARHistoryResult.curyDeposits), typeof(CuryARHistoryTran.curyFinPtdDeposits)},
	        //aDest.CuryDepositsBalance = -aHistory.CuryFinYtdDeposits ?? Decimal.Zero;
	        {typeof(ARHistoryResult.curyDepositsBalance), typeof(CuryARHistoryTran.curyFinYtdDeposits)},
	        {typeof(ARHistoryResult.curyFinCharges), typeof(CuryARHistoryTran.curyFinPtdFinCharges)},
	        {typeof(ARHistoryResult.curyRetainageWithheld), typeof(CuryARHistoryTran.curyFinPtdRetainageWithheld)},
	        {typeof(ARHistoryResult.curyRetainageReleased), typeof(CuryARHistoryTran.curyFinPtdRetainageReleased)},
	        //aDest.CuryBegRetainedBalance = (aHistory.CuryFinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinYtdRetainageReleased ?? Decimal.Zero) -
	        //	((aHistory.CuryFinPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinPtdRetainageReleased ?? Decimal.Zero));
	        {typeof(ARHistoryResult.curyBegRetainedBalance), typeof(CuryARHistoryTran.curyFinBegRetainedBalance)},
	        //aDest.CuryEndRetainedBalance = (aHistory.CuryFinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinYtdRetainageReleased ?? Decimal.Zero);
	        {typeof(ARHistoryResult.curyEndRetainedBalance), typeof(CuryARHistoryTran.curyFinYtdRetainedBalance)},

	        {typeof(ARHistoryResult.begBalance), typeof(CuryARHistoryTran.finBegBalance)},
	        {typeof(ARHistoryResult.endBalance), typeof(CuryARHistoryTran.finYtdBalance)},
			{typeof(ARHistoryResult.sales), typeof(CuryARHistoryTran.finPtdSales)},
	        {typeof(ARHistoryResult.payments), typeof(CuryARHistoryTran.finPtdPayments)},
	        {typeof(ARHistoryResult.discount), typeof(CuryARHistoryTran.finPtdDiscounts)},
	        {typeof(ARHistoryResult.crAdjustments), typeof(CuryARHistoryTran.finPtdCrAdjustments)},
	        {typeof(ARHistoryResult.drAdjustments), typeof(CuryARHistoryTran.finPtdDrAdjustments)},
	        {typeof(ARHistoryResult.deposits), typeof(CuryARHistoryTran.finPtdDeposits)},
	        {typeof(ARHistoryResult.rGOL), typeof(CuryARHistoryTran.finPtdRGOL)},
	        //aDest.DepositsBalance = -aHistory.FinYtdDeposits ?? Decimal.Zero;
	        {typeof(ARHistoryResult.depositsBalance), typeof(CuryARHistoryTran.finYtdDeposits)},
	        {typeof(ARHistoryResult.finCharges), typeof(CuryARHistoryTran.finPtdFinCharges)},
	        {typeof(ARHistoryResult.cOGS), typeof(CuryARHistoryTran.finPtdCOGS)},
	        {typeof(ARHistoryResult.finPtdRevaluated), typeof(CuryARHistoryTran.finPtdRevalued)},
	        {typeof(ARHistoryResult.retainageWithheld), typeof(CuryARHistoryTran.finPtdRetainageWithheld)},
	        {typeof(ARHistoryResult.retainageReleased), typeof(CuryARHistoryTran.finPtdRetainageReleased)},
	        //aDest.BegRetainedBalance = (aHistory.FinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.FinYtdRetainageReleased ?? Decimal.Zero) -
	        //	((aHistory.FinPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.FinPtdRetainageReleased ?? Decimal.Zero));
	        {typeof(ARHistoryResult.begRetainedBalance), typeof(CuryARHistoryTran.finBegRetainedBalance)},
	        //aDest.EndRetainedBalance = (aHistory.FinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.FinYtdRetainageReleased ?? Decimal.Zero);
	        {typeof(ARHistoryResult.endRetainedBalance), typeof(CuryARHistoryTran.finYtdRetainedBalance)}
        };
		protected readonly Dictionary<Type, Type> mapTran = new Dictionary<Type, Type>
		{
			{typeof(ARHistoryResult.acctCD), typeof(CA.Light.Customer.acctCD)},
			{typeof(ARHistoryResult.acctName), typeof(CA.Light.Customer.acctName)},
			{typeof(ARHistoryResult.customerID), typeof(CA.Light.Customer.bAccountID)},
			{typeof(ARHistoryResult.noteID), typeof(CA.Light.Customer.noteID)},
					
			{typeof(ARHistoryResult.curyID), typeof(ARHistoryByPeriod.curyID)},
			{typeof(ARHistoryResult.finPeriodID), typeof(CuryARHistoryTran.finPeriodID)},

			{typeof(ARHistoryResult.curyBegBalance), typeof(CuryARHistoryTran.curyTranBegBalance)},
			{typeof(ARHistoryResult.curyEndBalance), typeof(CuryARHistoryTran.curyTranYtdBalance)},
			{typeof(ARHistoryResult.curySales), typeof(CuryARHistoryTran.curyTranPtdSales)},
			{typeof(ARHistoryResult.curyPayments), typeof(CuryARHistoryTran.curyTranPtdPayments)},
			{typeof(ARHistoryResult.curyDiscount), typeof(CuryARHistoryTran.curyTranPtdDiscounts)},
			{typeof(ARHistoryResult.curyCrAdjustments), typeof(CuryARHistoryTran.curyTranPtdCrAdjustments)},
			{typeof(ARHistoryResult.curyDrAdjustments), typeof(CuryARHistoryTran.curyTranPtdDrAdjustments)},
			{typeof(ARHistoryResult.curyDeposits), typeof(CuryARHistoryTran.curyTranPtdDeposits)},
	        //aDest.CuryDepositsBalance = -aHistory.CuryTranYtdDeposits ?? Decimal.Zero;
	        {typeof(ARHistoryResult.curyDepositsBalance), typeof(CuryARHistoryTran.curyTranYtdDeposits)},
			{typeof(ARHistoryResult.curyFinCharges), typeof(CuryARHistoryTran.curyTranPtdFinCharges)},
			{typeof(ARHistoryResult.curyRetainageWithheld), typeof(CuryARHistoryTran.curyTranPtdRetainageWithheld)},
			{typeof(ARHistoryResult.curyRetainageReleased), typeof(CuryARHistoryTran.curyTranPtdRetainageReleased)},
	        //aDest.CuryBegRetainedBalance = (aHistory.CuryTranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranYtdRetainageReleased ?? Decimal.Zero) -
	        //	((aHistory.CuryTranPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranPtdRetainageReleased ?? Decimal.Zero));
	        {typeof(ARHistoryResult.curyBegRetainedBalance), typeof(CuryARHistoryTran.curyTranBegRetainedBalance)},
	        //aDest.CuryEndRetainedBalance = (aHistory.CuryTranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranYtdRetainageReleased ?? Decimal.Zero);
	        {typeof(ARHistoryResult.curyEndRetainedBalance), typeof(CuryARHistoryTran.curyTranYtdRetainedBalance)},

			{typeof(ARHistoryResult.begBalance), typeof(CuryARHistoryTran.tranBegBalance)},
			{typeof(ARHistoryResult.endBalance), typeof(CuryARHistoryTran.tranYtdBalance)},
			{typeof(ARHistoryResult.sales), typeof(CuryARHistoryTran.tranPtdSales)},
			{typeof(ARHistoryResult.payments), typeof(CuryARHistoryTran.tranPtdPayments)},
			{typeof(ARHistoryResult.discount), typeof(CuryARHistoryTran.tranPtdDiscounts)},
			//aDest.RGOL = -aHistory.FinPtdRGOL ?? Decimal.Zero;
			{typeof(ARHistoryResult.rGOL), typeof(CuryARHistoryTran.tranPtdRGOL)},
			{typeof(ARHistoryResult.crAdjustments), typeof(CuryARHistoryTran.tranPtdCrAdjustments)},
			{typeof(ARHistoryResult.drAdjustments), typeof(CuryARHistoryTran.tranPtdDrAdjustments)},
			{typeof(ARHistoryResult.deposits), typeof(CuryARHistoryTran.tranPtdDeposits)},
	        //aDest.DepositsBalance = -aHistory.tranYtdDeposits ?? Decimal.Zero;
	        {typeof(ARHistoryResult.depositsBalance), typeof(CuryARHistoryTran.tranYtdDeposits)},
			{typeof(ARHistoryResult.finCharges), typeof(CuryARHistoryTran.tranPtdFinCharges)},
			{typeof(ARHistoryResult.cOGS), typeof(CuryARHistoryTran.tranPtdCOGS)},
			{typeof(ARHistoryResult.finPtdRevaluated), typeof(CuryARHistoryTran.finPtdRevalued)},
			{typeof(ARHistoryResult.retainageWithheld), typeof(CuryARHistoryTran.tranPtdRetainageWithheld)},
			{typeof(ARHistoryResult.retainageReleased), typeof(CuryARHistoryTran.tranPtdRetainageReleased)},
	        //aDest.BegRetainedBalance = (aHistory.tranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.tranYtdRetainageReleased ?? Decimal.Zero) -
	        //	((aHistory.tranPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.tranPtdRetainageReleased ?? Decimal.Zero));
	        {typeof(ARHistoryResult.begRetainedBalance), typeof(CuryARHistoryTran.tranBegRetainedBalance)},
	        //aDest.EndRetainedBalance = (aHistory.tranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.tranYtdRetainageReleased ?? Decimal.Zero);
	        {typeof(ARHistoryResult.endRetainedBalance), typeof(CuryARHistoryTran.tranYtdRetainedBalance)}
		};

		protected virtual void AppendCommonWhereFilters(PXView view, ARHistoryFilter filter)
		{
			if (!SubCDUtils.IsSubCDEmpty(filter.SubCD))
			{
				view.WhereAnd<Where<Sub.subCD, Like<Current<ARHistoryFilter.subCDWildcard>>>>();
			}

			if (filter.CustomerClassID != null)
			{
				view.WhereAnd<Where<CA.Light.Customer.customerClassID, Equal<Current<ARHistoryFilter.customerClassID>>>>();
			}

			if (filter.CustomerID != null)
			{
				view.WhereAnd<Where<CA.Light.Customer.bAccountID, Equal<Current<ARHistoryFilter.customerID>>,
					Or<CA.Light.Customer.consolidatingBAccountID, Equal<Current<ARHistoryFilter.customerID>>,
						And<Current<ARHistoryFilter.includeChildAccounts>, Equal<True>>>>>();
			}

			if (filter.Status != null)
			{
				view.WhereAnd<Where<CA.Light.Customer.status, Equal<Current<ARHistoryFilter.status>>>>();
			}
		}
		#endregion

		#region Event Subscribers
		public virtual void ARHistoryFilter_CuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<ARHistoryFilter.splitByCurrency>(e.Row);
		}
		public virtual void ARHistoryFilter_SubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		public virtual void ARHistoryFilter_ARAcctID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			ARHistoryFilter header = e.Row as ARHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.ARAcctID = null;
			}
		}
		public virtual void ARHistoryFilter_ARSubID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			ARHistoryFilter header = e.Row as ARHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.ARSubID = null;
			}
		}
		public virtual void ARHistoryFilter_CuryID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			ARHistoryFilter header = e.Row as ARHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.CuryID = null;
			}
		}
		public virtual void ARHistoryFilter_Period_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			ARHistoryFilter header = e.Row as ARHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.Period = null;
			}
		}
		public virtual void ARHistoryFilter_CustomerClassID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			ARHistoryFilter header = e.Row as ARHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.CustomerClassID = null;
			}
		}
		protected virtual void ARHistoryFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			this.Summary.Current.ClearSummary();
		}
		protected virtual void ARHistoryFilter_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			ARHistoryFilter row = e.Row as ARHistoryFilter;
			if (row == null) return;		

			Company company = this.Company.Current;
			bool mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
			PXUIFieldAttribute.SetVisible<ARHistoryFilter.showWithBalanceOnly>(sender, row, true);
			PXUIFieldAttribute.SetVisible<ARHistoryFilter.curyID>(sender, row, mcFeatureInstalled);

			PXUIFieldAttribute.SetVisible<ARHistoryFilter.includeChildAccounts>(sender, row, PXAccess.FeatureInstalled<CS.FeaturesSet.parentChildAccount>());

			bool isCurySelected = string.IsNullOrEmpty(row.CuryID) == false;
			bool isBaseCurySelected = string.IsNullOrEmpty(row.CuryID) == false && (company.BaseCuryID == row.CuryID);
			bool splitByCurrency = (row.SplitByCurrency ?? false);

			PXUIFieldAttribute.SetVisible<ARHistoryFilter.splitByCurrency>(sender, row, mcFeatureInstalled && !isCurySelected);
			PXUIFieldAttribute.SetEnabled<ARHistoryFilter.splitByCurrency>(sender, row, mcFeatureInstalled && !isCurySelected);

			PXCache detailCache = this.History.Cache;
			bool hideCuryColumns = (!mcFeatureInstalled) || (isBaseCurySelected) || (!isCurySelected && !splitByCurrency);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyID>(this.History.Cache, null, mcFeatureInstalled && (isCurySelected || splitByCurrency));
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyBalance>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyPayments>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curySales>(detailCache, null, !hideCuryColumns);			
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyDiscount>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyCrAdjustments>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyDrAdjustments>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyDeposits>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyDepositsBalance>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyBegBalance>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyEndBalance>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyBegRetainedBalance>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyEndRetainedBalance>(History.Cache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyRetainageWithheld>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyRetainageReleased>(detailCache, null, !hideCuryColumns);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.rGOL>(History.Cache, null, mcFeatureInstalled);

			PXUIFieldAttribute.SetVisible<ARHistoryResult.balance>(detailCache, null, false);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.curyBalance>(detailCache, null, false);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.finPeriodID>(detailCache, null);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.begBalance>(detailCache, null);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.endBalance>(detailCache, null);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.begRetainedBalance>(History.Cache, null);
			PXUIFieldAttribute.SetVisible<ARHistoryResult.endRetainedBalance>(History.Cache, null);
		}

        protected virtual void ARHistorySummary_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            var row = (ARHistorySummary)e.Row;
            if (row == null)
            {
                return;
            }

            bool isForeignCurrency = string.IsNullOrEmpty(Filter.Current.CuryID) == false && (Company.Current.BaseCuryID != Filter.Current.CuryID);

            PXUIFieldAttribute.SetVisible<ARHistorySummary.curyBalanceSummary>(sender, row, isForeignCurrency);
            PXUIFieldAttribute.SetVisible<ARHistorySummary.curyDepositsSummary>(sender, row, isForeignCurrency);
			PXUIFieldAttribute.SetVisible<ARHistorySummary.curyBalanceRetainedSummary>(sender, row, isForeignCurrency);
		}
        #endregion

        #region Utility Functions

        public virtual string GetLastActivityPeriod(int? aCustomerID)
		{
			PXSelectBase<CuryARHistory> activitySelect = new PXSelect<CuryARHistory, Where<CuryARHistory.customerID, Equal<Required<CuryARHistory.customerID>>>, OrderBy<Desc<CuryARHistory.finPeriodID>>>(this);
			CuryARHistory result = (CuryARHistory)activitySelect.View.SelectSingle(aCustomerID);
			if (result != null)
				return result.FinPeriodID;
			return null;
		}
		public virtual string GetLastActivityPeriod(int? aCustomerID, bool IncludeChildAccounts)
		{
			if (!IncludeChildAccounts)
				return GetLastActivityPeriod(aCustomerID);

			CuryARHistory result = PXSelectJoin<CuryARHistory,
											InnerJoin<ChildBAccount, On<ChildBAccount.bAccountID, Equal<CuryARHistory.customerID>>>,
											Where<ChildBAccount.consolidatingBAccountID, Equal<Required<CuryARHistory.customerID>>>,
											OrderBy<Desc<CuryARHistory.finPeriodID>>>
											.SelectSingleBound(this, null, aCustomerID);
			if (result != null)
				return result.FinPeriodID;
			return null;
		}
        protected virtual ARHistoryResult CreateResult(PXResult dest, Dictionary<Type,Type> map)
		{
	        ARHistoryResult result = new ARHistoryResult();
			foreach (Type field in map.Keys)
			{
				Type source = map[field];
				Type dac = source.DeclaringType;
				PXCache cache = this.Caches[dac];
				History.Cache.SetValue(result, field.Name, cache.GetValue(PXResult.Unwrap(dest, dac), source.Name));
		}
			return result;
        }
		protected virtual void CopyFrom(ARHistoryResult aDest, CuryARHistoryTran aHistory, bool aIsFinancial)
		{
			if (aIsFinancial)
			{
				aDest.CuryBegBalance = aHistory.CuryFinBegBalance ?? Decimal.Zero;
				aDest.CurySales = aHistory.CuryFinPtdSales ?? Decimal.Zero;
				aDest.CuryPayments = aHistory.CuryFinPtdPayments ?? Decimal.Zero;
				aDest.CuryDiscount = aHistory.CuryFinPtdDiscounts ?? Decimal.Zero;
				aDest.CuryCrAdjustments = aHistory.CuryFinPtdCrAdjustments ?? Decimal.Zero;
				aDest.CuryDrAdjustments = aHistory.CuryFinPtdDrAdjustments ?? Decimal.Zero;
				aDest.CuryDeposits = aHistory.CuryFinPtdDeposits ?? Decimal.Zero;
				aDest.CuryDepositsBalance = -aHistory.CuryFinYtdDeposits ?? Decimal.Zero;
				aDest.CuryFinCharges = aHistory.CuryFinPtdFinCharges ??  Decimal.Zero;
				aDest.CuryRetainageWithheld = aHistory.CuryFinPtdRetainageWithheld ?? Decimal.Zero;
				aDest.CuryRetainageReleased = aHistory.CuryFinPtdRetainageReleased ?? Decimal.Zero;
				aDest.CuryBegRetainedBalance = (aHistory.CuryFinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinYtdRetainageReleased ?? Decimal.Zero) -
					((aHistory.CuryFinPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinPtdRetainageReleased ?? Decimal.Zero));
				aDest.CuryEndRetainedBalance = (aHistory.CuryFinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinYtdRetainageReleased ?? Decimal.Zero);

				aDest.BegBalance = aHistory.FinBegBalance ?? Decimal.Zero;
				aDest.Sales = aHistory.FinPtdSales?? Decimal.Zero;
				aDest.Payments = aHistory.FinPtdPayments ?? Decimal.Zero;
				aDest.Discount = aHistory.FinPtdDiscounts ?? Decimal.Zero;
				aDest.RGOL = -aHistory.FinPtdRGOL ?? Decimal.Zero;
				aDest.CrAdjustments = aHistory.FinPtdCrAdjustments ?? Decimal.Zero;
				aDest.DrAdjustments = aHistory.FinPtdDrAdjustments ?? Decimal.Zero;
				aDest.Deposits = aHistory.FinPtdDeposits ?? Decimal.Zero;
				aDest.DepositsBalance = -aHistory.FinYtdDeposits ?? Decimal.Zero;
				aDest.FinCharges = aHistory.FinPtdFinCharges ??  Decimal.Zero;
				aDest.COGS = aHistory.FinPtdCOGS?? Decimal.Zero;
				aDest.FinPtdRevaluated = aHistory.FinPtdRevalued ?? Decimal.Zero;
				aDest.RetainageWithheld = aHistory.FinPtdRetainageWithheld ?? Decimal.Zero;
				aDest.RetainageReleased = aHistory.FinPtdRetainageReleased ?? Decimal.Zero;
				aDest.BegRetainedBalance = (aHistory.FinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.FinYtdRetainageReleased ?? Decimal.Zero) -
					((aHistory.FinPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.FinPtdRetainageReleased ?? Decimal.Zero));
				aDest.EndRetainedBalance = (aHistory.FinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.FinYtdRetainageReleased ?? Decimal.Zero);
				aDest.CuryID = aHistory.CuryID;
			}
			else
			{
				aDest.CuryBegBalance = aHistory.CuryTranBegBalance ?? Decimal.Zero;
				aDest.CurySales = aHistory.CuryTranPtdSales ?? Decimal.Zero;
				aDest.CuryPayments = aHistory.CuryTranPtdPayments ?? Decimal.Zero;
				aDest.CuryDiscount = aHistory.CuryTranPtdDiscounts ?? Decimal.Zero;
				aDest.CuryCrAdjustments = aHistory.CuryTranPtdCrAdjustments ?? Decimal.Zero;
				aDest.CuryDrAdjustments = aHistory.CuryTranPtdDrAdjustments ?? Decimal.Zero;
				aDest.CuryDeposits = aHistory.CuryTranPtdDeposits ?? Decimal.Zero;
				aDest.CuryDepositsBalance = -aHistory.CuryTranYtdDeposits ?? Decimal.Zero;
				aDest.CuryFinCharges = aHistory.CuryTranPtdFinCharges ?? Decimal.Zero;
				aDest.CuryRetainageWithheld = aHistory.CuryTranPtdRetainageWithheld ?? Decimal.Zero;
				aDest.CuryRetainageReleased = aHistory.CuryTranPtdRetainageReleased ?? Decimal.Zero;
				aDest.CuryBegRetainedBalance = (aHistory.CuryTranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranYtdRetainageReleased ?? Decimal.Zero) -
					((aHistory.CuryTranPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranPtdRetainageReleased ?? Decimal.Zero));
				aDest.CuryEndRetainedBalance = (aHistory.CuryTranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranYtdRetainageReleased ?? Decimal.Zero);

				aDest.BegBalance = aHistory.TranBegBalance ?? Decimal.Zero;
				aDest.Sales = aHistory.TranPtdSales ?? Decimal.Zero;
				aDest.Payments = aHistory.TranPtdPayments ?? Decimal.Zero;
				aDest.Discount = aHistory.TranPtdDiscounts ?? Decimal.Zero;
				aDest.RGOL = -aHistory.TranPtdRGOL ?? Decimal.Zero;
				aDest.CrAdjustments = aHistory.TranPtdCrAdjustments ?? Decimal.Zero;
				aDest.DrAdjustments = aHistory.TranPtdDrAdjustments ?? Decimal.Zero;
				aDest.Deposits = aHistory.TranPtdDeposits ?? Decimal.Zero;
				aDest.DepositsBalance = -aHistory.TranYtdDeposits ?? Decimal.Zero;
				aDest.FinCharges = aHistory.TranPtdFinCharges ?? Decimal.Zero;
				aDest.COGS = aHistory.TranPtdCOGS ?? Decimal.Zero;
				aDest.FinPtdRevaluated = aHistory.FinPtdRevalued ?? Decimal.Zero;
				aDest.RetainageWithheld = aHistory.TranPtdRetainageWithheld ?? Decimal.Zero;
				aDest.RetainageReleased = aHistory.TranPtdRetainageReleased ?? Decimal.Zero;
				aDest.BegRetainedBalance = (aHistory.TranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.TranYtdRetainageReleased ?? Decimal.Zero) -
					((aHistory.TranPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.TranPtdRetainageReleased ?? Decimal.Zero));
				aDest.EndRetainedBalance = (aHistory.TranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.TranYtdRetainageReleased ?? Decimal.Zero);

				aDest.CuryID = aHistory.CuryID;
			}
			aDest.RecalculateEndBalance();
		}
		protected virtual void Aggregate(ARHistoryResult aDest, ARHistoryResult aSrc)
		{
			aDest.CuryBegBalance += aSrc.CuryBegBalance ?? Decimal.Zero;
			aDest.CuryCrAdjustments += aSrc.CuryCrAdjustments ?? Decimal.Zero;
			aDest.CuryDrAdjustments += aSrc.CuryDrAdjustments ?? Decimal.Zero;
			aDest.CuryDiscount += aSrc.CuryDiscount ?? Decimal.Zero;
			aDest.CurySales += aSrc.CurySales ?? Decimal.Zero;
			aDest.CuryPayments += aSrc.CuryPayments ?? Decimal.Zero;
			aDest.CuryFinCharges += aSrc.CuryFinCharges ?? Decimal.Zero;
			aDest.CuryDeposits += aSrc.CuryDeposits ?? Decimal.Zero;
			aDest.CuryDepositsBalance += aSrc.CuryDepositsBalance ?? Decimal.Zero;
			aDest.CuryRetainageWithheld += aSrc.CuryRetainageWithheld ?? Decimal.Zero;
			aDest.CuryRetainageReleased += aSrc.CuryRetainageReleased ?? Decimal.Zero;
			aDest.CuryBegRetainedBalance += aSrc.CuryBegRetainedBalance ?? Decimal.Zero;

			aDest.BegBalance += aSrc.BegBalance ?? Decimal.Zero;
			aDest.CrAdjustments += aSrc.CrAdjustments ?? Decimal.Zero;
			aDest.DrAdjustments += aSrc.DrAdjustments ?? Decimal.Zero;
			aDest.Discount += aSrc.Discount ?? Decimal.Zero;
			aDest.Sales+= aSrc.Sales ?? Decimal.Zero;
			aDest.Payments += aSrc.Payments ?? Decimal.Zero;
			aDest.FinCharges += aSrc.FinCharges ?? Decimal.Zero;
			aDest.RGOL += aSrc.RGOL ?? Decimal.Zero;
			aDest.FinPtdRevaluated += aSrc.FinPtdRevaluated ?? Decimal.Zero;
			aDest.Deposits += aSrc.Deposits ?? Decimal.Zero;
			aDest.DepositsBalance += aSrc.DepositsBalance ?? Decimal.Zero;
			aDest.RetainageWithheld += aSrc.RetainageWithheld ?? Decimal.Zero;
			aDest.RetainageReleased += aSrc.RetainageReleased ?? Decimal.Zero;
			aDest.BegRetainedBalance += aSrc.BegRetainedBalance ?? Decimal.Zero;

			aDest.RecalculateEndBalance();
		}

		protected virtual void Aggregate(ARHistorySummary aDest, ARHistoryResult aSrc)
		{
			aDest.CuryBalanceSummary += aSrc.CuryEndBalance ?? decimal.Zero;
			aDest.BalanceSummary += aSrc.EndBalance ?? decimal.Zero;

			aDest.RevaluedSummary += aSrc.FinPtdRevaluated ?? decimal.Zero;

			aDest.CuryDepositsSummary += aSrc.CuryDepositsBalance ?? decimal.Zero;
			aDest.DepositsSummary += aSrc.DepositsBalance ?? decimal.Zero;

			aDest.BalanceRetainedSummary += aSrc.EndRetainedBalance ?? decimal.Zero;
			aDest.CuryBalanceRetainedSummary += aSrc.CuryEndRetainedBalance ?? decimal.Zero;
		}
        protected virtual void AggregateLatest(ARHistoryResult aDest, ARHistoryResult aSrc)
		{
			if (aSrc.FinPeriodID == aDest.FinPeriodID)
			{
				Aggregate(aDest, aSrc);
			}
			else
			{
				if (string.Compare(aSrc.FinPeriodID, aDest.FinPeriodID) < 0)
				{
					//Just update Beg Balance
					aDest.BegBalance += aSrc.EndBalance ?? Decimal.Zero;
					aDest.DepositsBalance += aSrc.DepositsBalance ?? Decimal.Zero;
					aDest.BegRetainedBalance += aSrc.EndRetainedBalance ?? Decimal.Zero;
					aDest.CuryBegBalance += aSrc.CuryEndBalance ?? Decimal.Zero;
					aDest.CuryDepositsBalance += aSrc.CuryDepositsBalance ?? Decimal.Zero;
					aDest.CuryBegRetainedBalance += aSrc.CuryEndRetainedBalance ?? Decimal.Zero;
				}
				else
				{
					//Invert 
					aDest.BegBalance = (aDest.EndBalance ?? Decimal.Zero) + (aSrc.BegBalance ?? Decimal.Zero);
					aDest.CrAdjustments = aSrc.CrAdjustments ?? Decimal.Zero;
					aDest.DrAdjustments = aSrc.DrAdjustments ?? Decimal.Zero;
					aDest.Discount = aSrc.Discount ?? Decimal.Zero;
					aDest.Sales = aSrc.Sales ?? Decimal.Zero;
					aDest.Payments = aSrc.Payments ?? Decimal.Zero;
					aDest.RGOL = aSrc.RGOL ?? Decimal.Zero;
					aDest.FinPeriodID = aSrc.FinPeriodID;
					aDest.FinPtdRevaluated = aSrc.FinPtdRevaluated ?? Decimal.Zero;
					aDest.FinCharges = aSrc.FinCharges ?? Decimal.Zero;
					aDest.Deposits = aSrc.Deposits ?? Decimal.Zero;
					aDest.DepositsBalance = (aDest.DepositsBalance ?? Decimal.Zero) + (aSrc.DepositsBalance ?? Decimal.Zero);
					aDest.RetainageWithheld = aSrc.RetainageWithheld ?? Decimal.Zero;
					aDest.RetainageReleased = aSrc.RetainageReleased ?? Decimal.Zero;
					aDest.BegRetainedBalance = (aDest.EndRetainedBalance ?? Decimal.Zero) + (aSrc.BegRetainedBalance ?? Decimal.Zero);

					aDest.CuryBegBalance = (aDest.CuryEndBalance ?? Decimal.Zero) + (aSrc.CuryBegBalance ?? Decimal.Zero);
					aDest.CuryCrAdjustments = aSrc.CuryCrAdjustments ?? Decimal.Zero;
					aDest.CuryDrAdjustments = aSrc.CuryDrAdjustments ?? Decimal.Zero;
					aDest.CuryDiscount = aSrc.CuryDiscount ?? Decimal.Zero;
					aDest.CurySales = aSrc.CurySales ?? Decimal.Zero;
					aDest.CuryPayments = aSrc.CuryPayments ?? Decimal.Zero;
					aDest.CuryFinCharges = aSrc.CuryFinCharges ??Decimal.Zero;
					aDest.CuryDeposits = aSrc.CuryDeposits ?? Decimal.Zero;
					aDest.CuryDepositsBalance = (aDest.CuryDepositsBalance ?? Decimal.Zero) + (aSrc.CuryDepositsBalance ?? Decimal.Zero);
					aDest.CuryRetainageWithheld = aSrc.CuryRetainageWithheld ?? Decimal.Zero;
					aDest.CuryRetainageReleased = aSrc.CuryRetainageReleased ?? Decimal.Zero;
					aDest.CuryBegRetainedBalance = (aDest.CuryEndRetainedBalance ?? Decimal.Zero) + (aSrc.CuryBegRetainedBalance ?? Decimal.Zero);
				}
				aDest.RecalculateEndBalance();
			}
		}

		protected virtual bool IsExcludedByZeroBalances(bool? showWithBalanceOnly, ARHistoryResult historyResult)
		{
			return (showWithBalanceOnly ?? false) 
				&& ((historyResult.EndBalance ?? decimal.Zero) == decimal.Zero)
				&& ((historyResult.FinPtdRevaluated ?? decimal.Zero) == decimal.Zero)
				&& ((historyResult.DepositsBalance ?? decimal.Zero) == decimal.Zero)
				&& ((historyResult.CuryEndBalance ?? decimal.Zero) == decimal.Zero)
				&& ((historyResult.CuryDepositsBalance ?? decimal.Zero) == decimal.Zero)
				&& ((historyResult.EndRetainedBalance ?? decimal.Zero) == decimal.Zero)
				&& ((historyResult.CuryEndRetainedBalance ?? decimal.Zero) == decimal.Zero);
		}

        public static void Copy(ARDocumentEnq.ARDocumentFilter filter, ARHistoryFilter histFilter)
		{
			filter.OrganizationID = histFilter.OrganizationID;
			filter.BranchID = histFilter.BranchID;
			filter.OrgBAccountID = histFilter.OrgBAccountID;
			filter.Period = histFilter.Period;
			filter.SubCD = histFilter.SubCD;
			filter.ARAcctID = histFilter.ARAcctID;
			filter.ARSubID = histFilter.ARSubID;
			filter.CuryID = histFilter.CuryID;
			filter.UseMasterCalendar = histFilter.UseMasterCalendar;
			filter.IncludeChildAccounts = histFilter.IncludeChildAccounts;
		}
		public static void Copy(ARHistoryFilter histFilter, ARDocumentEnq.ARDocumentFilter filter)
		{
			histFilter.OrganizationID = filter.OrganizationID;
			histFilter.BranchID = filter.BranchID;
			histFilter.OrgBAccountID = filter.OrgBAccountID;
			histFilter.CustomerID = filter.CustomerID;
			histFilter.Period = filter.Period;
			histFilter.SubCD = filter.SubCD;
			histFilter.ARAcctID = filter.ARAcctID;
			histFilter.ARSubID = filter.ARSubID;
			histFilter.CuryID = filter.CuryID;
			histFilter.UseMasterCalendar = filter.UseMasterCalendar;
			histFilter.IncludeChildAccounts = filter.IncludeChildAccounts;
		}

		#endregion
	}
}
