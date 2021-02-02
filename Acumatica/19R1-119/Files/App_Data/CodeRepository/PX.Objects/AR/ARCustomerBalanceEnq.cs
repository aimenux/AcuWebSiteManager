using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.Common;

using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Tools;
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

			[PXDefault()]
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

			[PXDBString(5, IsUnicode = true)]
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
		public sealed class ARH : CuryARHistory
		{
			#region BranchID
			public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			#endregion
			#region AccountID
			public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			#endregion
			#region SubID
			public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
			#endregion
			#region FinPeriodID
			public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			#endregion
			#region CustomerID
			public new abstract class customerID : PX.Data.BQL.BqlInt.Field<customerID> { }
			#endregion
			#region CuryID
			public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			#endregion
			#region FinBegBalance
			public new abstract class finBegBalance : PX.Data.BQL.BqlDecimal.Field<finBegBalance> { }
			#endregion
			#region FinYtdBalance
			public new abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
			#endregion

			#region TranBegBalance
			public new abstract class tranBegBalance : PX.Data.BQL.BqlDecimal.Field<tranBegBalance> { }
			#endregion
			#region TranYtdBalance
			public new abstract class tranYtdBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdBalance> { }
			#endregion

			#region CuryFinBegBalance
			public new abstract class curyFinBegBalance : PX.Data.BQL.BqlDecimal.Field<curyFinBegBalance> { }
			#endregion
			#region CuryFinYtdBalance
			public new abstract class curyFinYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalance> { }
			#endregion

			#region CuryTranBegBalance
			public new abstract class curyTranBegBalance : PX.Data.BQL.BqlDecimal.Field<curyTranBegBalance> { }
			#endregion
			#region CuryTranYtdBalance
			public new abstract class curyTranYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyTranYtdBalance> { }
			#endregion

			#region CuryTranPtdDeposits
			public new abstract class curyTranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDeposits> { }
			#endregion
			#region CuryTranYtdDeposits
			public new abstract class curyTranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranYtdDeposits> { }
			#endregion
			#region TranPtdDeposits
			public new abstract class tranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranPtdDeposits> { }
			#endregion
			#region TranYtdDeposits
			public new abstract class tranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranYtdDeposits> { }
			#endregion

			#region CuryFinPtdDeposits
			public new abstract class curyFinPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDeposits> { }
			#endregion
			#region CuryFinYtdDeposits
			public new abstract class curyFinYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinYtdDeposits> { }
			#endregion
			#region FinPtdDeposits
			public new abstract class finPtdDeposits : PX.Data.BQL.BqlDecimal.Field<finPtdDeposits> { }
			#endregion
			#region FinYtdDeposits
			public new abstract class finYtdDeposits : PX.Data.BQL.BqlDecimal.Field<finYtdDeposits> { }
			#endregion

			#region FinPtdRevaluated
			public abstract class finPtdRevaluated : PX.Data.IBqlField { }
			#endregion

			#region FinYtdRetainageWithheld
			public new abstract class finYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageWithheld> { }
			#endregion
			#region FinYtdRetainageReleased
			public new abstract class finYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageReleased> { }
			#endregion
			#region TranYtdRetainageWithheld
			public new abstract class tranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageWithheld> { }
			#endregion
			#region TranYtdRetainageReleased
			public new abstract class tranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageReleased> { }
			#endregion
			#region CuryFinYtdRetainageWithheld
			public new abstract class curyFinYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageWithheld> { }
			#endregion
			#region CuryFinYtdRetainageReleased
			public new abstract class curyFinYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageReleased> { }
			#endregion
			#region CuryTranYtdRetainageWithheld
			public new abstract class curyTranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageWithheld> { }
			#endregion
			#region CuryTranYtdRetainageReleased
			public new abstract class curyTranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageReleased> { }
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
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
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

			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
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
            Summary.Cache.Clear();
            ARHistoryFilter header = Filter.Current;
	        ARHistoryResult[] empty = null;
	        if (header == null)
	        {
	            return empty;
	        }

	        Dictionary<KeyValuePair<int, string>, ARHistoryResult> result;
	        if (header.Period == null)
	        {
	            RetrieveHistory(header, out result);
	        }
	        else
	        {
	            RetrieveHistoryForPeriod(header, out result);
	        }

            Summary.Update(Summary.Current);


            bool anyDoc = result.Count > 0;
	        this.viewDetails.SetEnabled(anyDoc);

            return result.Values;
	    }

        protected virtual IEnumerable summary()
        {
            if (Summary.Current.Calculated == false)
            {
                var summary = Summary.Cache.CreateInstance() as ARHistorySummary;
                summary.ClearSummary();
                Summary.Insert(summary);
                History.Select();
            }

            yield return Summary.Current;
        }

        protected virtual void RetrieveHistory(ARHistoryFilter header, out Dictionary<KeyValuePair<int, string>, ARHistoryResult> result) 
		{
			result = new Dictionary<KeyValuePair<int, string>, ARHistoryResult>();
			bool isCurySelected = string.IsNullOrEmpty(header.CuryID) == false;
			bool splitByCurrency = header.SplitByCurrency ?? false;
			bool useFinancial = (header.UseMasterCalendar != true);
			
			#region FiscalPeriodUndefined

			List<Type> typesList = new List<Type>
			{
				typeof(Select5<,,>), typeof(ARLatestHistory),
				typeof(LeftJoin<CA.Light.Customer, On<ARLatestHistory.customerID, Equal<CA.Light.Customer.bAccountID>,
						And<Match<CA.Light.Customer, Current<AccessInfo.userName>>>>,
					LeftJoin<Sub, On<ARLatestHistory.subID, Equal<Sub.subID>>,
					LeftJoin<CuryARHistory, On<ARLatestHistory.accountID, Equal<CuryARHistory.accountID>,
						And<ARLatestHistory.branchID, Equal<CuryARHistory.branchID>,
						And<ARLatestHistory.customerID, Equal<CuryARHistory.customerID>,
						And<ARLatestHistory.subID, Equal<CuryARHistory.subID>,
						And<ARLatestHistory.curyID, Equal<CuryARHistory.curyID>,
						And<ARLatestHistory.lastActivityPeriod, Equal<CuryARHistory.finPeriodID>>>>>>>>>>),
				typeof(Aggregate<>)
			};
				
			typesList.AddRange(BqlHelper.GetDecimalFieldsAggregate<CuryARHistory>(this));

			Type typeCustomer = header.IncludeChildAccounts == true
				? typeof(CA.Light.Customer.consolidatingBAccountID)
				: typeof(CA.Light.Customer.bAccountID);

			typesList.AddRange(
				new Type[]
				{
				typeof(GroupBy<,>), typeof(ARLatestHistory.lastActivityPeriod),
				typeof(GroupBy<,>), typeof(ARLatestHistory.curyID),
				typeof(GroupBy<>), typeCustomer
				});

			Type select = BqlCommand.Compose(typesList.ToArray());
            select = BqlCommand.AppendJoin(select, BqlCommand.Compose(typeof(LeftJoin<,>),
                typeof(CA.Light.CustomerMaster), typeof(On<,>), typeof(CA.Light.CustomerMaster.bAccountID), typeof(Equal<>), typeCustomer));

            BqlCommand cmd = BqlCommand.CreateInstance(select);
			PXView view = new PXView(this, false, cmd);

            view.WhereAnd<Where<CA.Light.Customer.bAccountID, IsNotNull>>();
            view.WhereAnd<Where<CA.Light.CustomerMaster.bAccountID, IsNotNull>>();

			int[] branchIDs = null;

			if (header.BranchID != null)
			{
				view.WhereAnd<Where<ARLatestHistory.branchID, Equal<Current<ARHistoryFilter.branchID>>>>();
			}
			else if (header.OrganizationID != null)
			{
				branchIDs = PXAccess.GetChildBranchIDs(header.OrganizationID, false);

				view.WhereAnd<Where<ARLatestHistory.branchID, In<Required<ARLatestHistory.branchID>>,
				  And<MatchWithBranch<ARLatestHistory.branchID>>>>();
			}

			if (header.ARAcctID != null)
			{
				view.WhereAnd<Where<ARLatestHistory.accountID, Equal<Current<ARHistoryFilter.aRAcctID>>>>();
			}

			if (header.ARSubID != null)
			{
				view.WhereAnd<Where<ARLatestHistory.subID, Equal<Current<ARHistoryFilter.aRSubID>>>>();
			}

			if (isCurySelected)
			{
				view.WhereAnd<Where<ARLatestHistory.curyID, Equal<Current<ARHistoryFilter.curyID>>>>();
			}

			AppendCommonWhereFilters(view, header);

            var summary = Summary.Cache.CreateInstance() as ARHistorySummary;
            summary.ClearSummary();

        foreach (PXResult<ARLatestHistory, CA.Light.Customer, Sub, CuryARHistory, CA.Light.CustomerMaster> record in view.SelectMulti(branchIDs))
			{
                CA.Light.CustomerMaster customer = record;
				CuryARHistory history = record;
				ARHistoryResult res = new ARHistoryResult();
				CopyFrom(res, customer);
				CopyFrom(res, history, useFinancial);
				res.FinPeriodID = history.FinPeriodID;
				string keyCuryID = (isCurySelected || splitByCurrency) ? history.CuryID : this.Company.Current.BaseCuryID;

				if (!isCurySelected && !splitByCurrency)
				{
					res.CopyValueToCuryValue(this.Company.Current.BaseCuryID);
					res.RecalculateEndBalance();
				}

                if (IsExcludedByZeroBalances(header.ShowWithBalanceOnly, res))
                {
                    continue;
                }

                KeyValuePair<int, string> key = new KeyValuePair<int, string>(customer.BAccountID.Value, keyCuryID);
				if (result.ContainsKey(key))
				{
					AggregateLatest(result[key], res);
				}
				else
				{
					result[key] = res;
				}

                Aggregate(summary, res);
			}

            summary.Calculated = true;
            Summary.Update(summary);
            #endregion
        }

		protected virtual void RetrieveHistoryForPeriod(ARHistoryFilter header, out Dictionary<KeyValuePair<int, string>, ARHistoryResult> result) 
		{
			result = new Dictionary<KeyValuePair<int, string>, ARHistoryResult>();
			bool isCurySelected = string.IsNullOrEmpty(header.CuryID) == false;
			bool splitByCurrency = header.SplitByCurrency ?? false;			
			bool useFinancial = (header.UseMasterCalendar != true);
			
			#region Specific Fiscal Period

			List<Type> typesList = new List<Type>
			{
				typeof(Select5<,,,>), typeof(ARHistoryByPeriod),
				typeof(LeftJoin<CA.Light.Customer, On<ARHistoryByPeriod.customerID, Equal<CA.Light.Customer.bAccountID>,
						And<Match<CA.Light.Customer, Current<AccessInfo.userName>>>>,
					LeftJoin<Sub, On<ARHistoryByPeriod.subID, Equal<Sub.subID>>,
					LeftJoin<CuryARHistory, On<ARHistoryByPeriod.accountID, Equal<CuryARHistory.accountID>,
						And<ARHistoryByPeriod.branchID, Equal<CuryARHistory.branchID>,
						And<ARHistoryByPeriod.customerID, Equal<CuryARHistory.customerID>,
						And<ARHistoryByPeriod.subID, Equal<CuryARHistory.subID>,
						And<ARHistoryByPeriod.curyID, Equal<CuryARHistory.curyID>,
						And<ARHistoryByPeriod.finPeriodID, Equal<CuryARHistory.finPeriodID>>>>>>>,
					LeftJoin<ARH, On<ARHistoryByPeriod.accountID, Equal<ARH.accountID>,
						And<ARHistoryByPeriod.branchID, Equal<ARH.branchID>,
						And<ARHistoryByPeriod.customerID, Equal<ARH.customerID>,
						And<ARHistoryByPeriod.subID, Equal<ARH.subID>,
						And<ARHistoryByPeriod.curyID, Equal<ARH.curyID>,
						And<ARHistoryByPeriod.lastActivityPeriod, Equal<ARH.finPeriodID>>>>>>>>>>>),
				typeof(Where<ARHistoryByPeriod.finPeriodID, Equal<Current<ARHistoryFilter.period>>>),
				typeof(Aggregate<>)
			};

			typesList.AddRange(BqlHelper.GetDecimalFieldsAggregate<CuryARHistory>(this));
			typesList.AddRange(BqlHelper.GetDecimalFieldsAggregate<ARH>(this));
			
			Type typeCustomer = header.IncludeChildAccounts == true
				? typeof(CA.Light.Customer.consolidatingBAccountID)
				: typeof(CA.Light.Customer.bAccountID);

			typesList.AddRange(
				new Type[]
				{
				typeof(GroupBy<,>), typeof(ARHistoryByPeriod.lastActivityPeriod),
				typeof(GroupBy<,>), typeof(ARHistoryByPeriod.finPeriodID),
				typeof(GroupBy<,>), typeof(ARHistoryByPeriod.curyID),
				typeof(GroupBy<>), typeCustomer
				});

			Type select = BqlCommand.Compose(typesList.ToArray());
            select = BqlCommand.AppendJoin(select, BqlCommand.Compose(typeof(LeftJoin<,>),
                typeof(CA.Light.CustomerMaster), typeof(On<,>), typeof(CA.Light.CustomerMaster.bAccountID), typeof(Equal<>), typeCustomer));

            BqlCommand cmd = BqlCommand.CreateInstance(select);
			PXView view = new PXView(this, false, cmd);

            view.WhereAnd<Where<CA.Light.Customer.bAccountID, IsNotNull>>();
            view.WhereAnd<Where<CA.Light.CustomerMaster.bAccountID, IsNotNull>>();

            if (isCurySelected)
			{
				view.WhereAnd<Where<ARHistoryByPeriod.curyID, Equal<Current<ARHistoryFilter.curyID>>>>();
			}

			int[] branchIDs = null;

			if (header.BranchID != null)
			{
				view.WhereAnd<Where<ARHistoryByPeriod.branchID, Equal<Current<ARHistoryFilter.branchID>>>>();
			}
			else if (header.OrganizationID != null)
			{
				branchIDs = PXAccess.GetChildBranchIDs(header.OrganizationID, false);

				view.WhereAnd<Where<ARHistoryByPeriod.branchID, In<Required<ARHistoryFilter.branchID>>,
				  And<MatchWithBranch<ARHistoryByPeriod.branchID>>>>();
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

            var summary = Summary.Cache.CreateInstance() as ARHistorySummary;
            summary.ClearSummary();

         foreach (PXResult<ARHistoryByPeriod, CA.Light.Customer, Sub, CuryARHistory, ARH, CA.Light.CustomerMaster> record in view.SelectMulti(branchIDs))
			{
                CA.Light.CustomerMaster customer = record;
				CuryARHistory history = record;
				ARH lastActivity = record;
				ARHistoryByPeriod hstByPeriod = record;
				ARHistoryResult res = new ARHistoryResult();
				CopyFrom(res, customer);
				CopyFrom(res, history, useFinancial);

				res.FinPeriodID = lastActivity.FinPeriodID;
				if (string.IsNullOrEmpty(res.CuryID))
				{
					res.CuryID = hstByPeriod.CuryID;
				}

				string keyCuryID = (isCurySelected || splitByCurrency) ? hstByPeriod.CuryID : this.Company.Current.BaseCuryID;
				KeyValuePair<int, string> key = new KeyValuePair<int, string>(customer.BAccountID.Value, keyCuryID);

				if ((history.FinPeriodID == null) || (history.FinPeriodID != lastActivity.FinPeriodID))
				{
					if (useFinancial)
					{
						res.EndBalance = res.BegBalance = lastActivity.FinYtdBalance ?? Decimal.Zero;
						res.CuryEndBalance = res.CuryBegBalance = lastActivity.CuryFinYtdBalance ?? Decimal.Zero;
						res.DepositsBalance = -lastActivity.FinYtdDeposits ?? Decimal.Zero;
						res.CuryDepositsBalance = -lastActivity.CuryFinYtdDeposits ?? Decimal.Zero;
						res.EndRetainedBalance = res.BegRetainedBalance = (lastActivity.FinYtdRetainageWithheld ?? Decimal.Zero) - (lastActivity.FinYtdRetainageReleased ?? Decimal.Zero);
						res.CuryEndRetainedBalance = res.CuryBegRetainedBalance = (lastActivity.CuryFinYtdRetainageWithheld ?? Decimal.Zero) - (lastActivity.CuryFinYtdRetainageReleased ?? Decimal.Zero);
					}
					else
					{
						res.EndBalance = res.BegBalance = lastActivity.TranYtdBalance ?? Decimal.Zero;
						res.CuryEndBalance = res.CuryBegBalance = lastActivity.CuryTranYtdBalance ?? Decimal.Zero;
						res.CuryDepositsBalance = -lastActivity.CuryTranYtdDeposits ?? Decimal.Zero;
						res.DepositsBalance = -lastActivity.TranYtdDeposits ?? Decimal.Zero;
						res.EndRetainedBalance = res.BegRetainedBalance = (lastActivity.TranYtdRetainageWithheld ?? Decimal.Zero) - (lastActivity.TranYtdRetainageReleased ?? Decimal.Zero);
						res.CuryEndRetainedBalance = res.CuryBegRetainedBalance = (lastActivity.CuryTranYtdRetainageWithheld ?? Decimal.Zero) - (lastActivity.CuryTranYtdRetainageReleased ?? Decimal.Zero);
					}
				}

				if ((!isCurySelected) && splitByCurrency == false)
				{
					res.CopyValueToCuryValue(this.Company.Current.BaseCuryID);
				}

                if (IsExcludedByZeroBalances(header.ShowWithBalanceOnly, res))
                {
                    continue;
                }

                if (result.ContainsKey(key))
				{
					var resultRowToAgregate = result[key];

					if (string.CompareOrdinal(resultRowToAgregate.FinPeriodID, res.FinPeriodID) < 0)
					{
						resultRowToAgregate.FinPeriodID = res.FinPeriodID;
					}
					
					Aggregate(resultRowToAgregate, res);
				}
				else
				{
					result[key] = res;
				}

                Aggregate(summary, res);
            }

            summary.Calculated = true;
            Summary.Update(summary);
		    #endregion
        }

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

			PXUIFieldAttribute.SetRequired<ARHistoryFilter.branchID>(sender, false);

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

        protected virtual string GetLastActivityPeriod(int? aCustomerID)
		{
			PXSelectBase<CuryARHistory> activitySelect = new PXSelect<CuryARHistory, Where<CuryARHistory.customerID, Equal<Required<CuryARHistory.customerID>>>, OrderBy<Desc<CuryARHistory.finPeriodID>>>(this);
			CuryARHistory result = (CuryARHistory)activitySelect.View.SelectSingle(aCustomerID);
			if (result != null)
				return result.FinPeriodID;
			return null;
		}

		protected virtual void CopyFrom(ARHistoryResult aDest, CA.Light.Customer aCustomer)
		{
			aDest.AcctCD = aCustomer.AcctCD;
			aDest.AcctName = aCustomer.AcctName;
			aDest.CuryID = aCustomer.CuryID;
			aDest.CustomerID = aCustomer.BAccountID;
			aDest.NoteID = aCustomer.NoteID;
		}

		protected virtual void CopyFrom(ARHistoryResult aDest, CuryARHistory aHistory, bool aIsFinancial)
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
		protected virtual void CopyFrom(ARHistoryResult aDest, CuryARHistory aHistory, bool aUseCurrency, bool aIsFinancial)
		{
			if (aIsFinancial)
			{
				if (aUseCurrency)
				{
					aDest.Sales = aHistory.CuryFinPtdSales ?? 0m;
					aDest.Payments = aHistory.CuryFinPtdPayments ?? 0m;
					aDest.Discount = aHistory.CuryFinPtdDiscounts ?? 0m;
					aDest.RGOL = 0m;
					aDest.CrAdjustments = aHistory.CuryFinPtdCrAdjustments ?? 0m;
					aDest.DrAdjustments = aHistory.CuryFinPtdDrAdjustments ?? 0m;
					aDest.BegBalance = aHistory.CuryFinBegBalance ?? 0m;
					aDest.CuryID = aHistory.CuryID;					
					aDest.FinPtdRevaluated = Decimal.Zero;
					aDest.Deposits = aHistory.CuryFinPtdDeposits ?? Decimal.Zero;
					aDest.DepositsBalance = -aHistory.CuryFinYtdDeposits ?? Decimal.Zero;
					aDest.RetainageWithheld = aHistory.CuryFinPtdRetainageWithheld ?? Decimal.Zero;
					aDest.RetainageReleased = aHistory.CuryFinPtdRetainageReleased ?? Decimal.Zero;
				}
				else
				{
					aDest.Sales = aHistory.FinPtdSales ?? 0m;
					aDest.Payments = aHistory.FinPtdPayments ?? 0m;
					aDest.Discount = aHistory.FinPtdDiscounts ?? 0m;
					aDest.RGOL = -aHistory.FinPtdRGOL ?? 0m;
					aDest.CrAdjustments = aHistory.FinPtdCrAdjustments ?? 0m;
					aDest.DrAdjustments = aHistory.FinPtdDrAdjustments ?? 0m;
					aDest.BegBalance = aHistory.FinBegBalance ?? 0m;
					aDest.COGS = aHistory.FinPtdCOGS?? 0m;
					aDest.FinCharges = aHistory.FinPtdFinCharges ?? 0m;
					aDest.FinPtdRevaluated = aHistory.FinPtdRevalued ?? Decimal.Zero;
					aDest.Deposits = aHistory.FinPtdDeposits ?? Decimal.Zero;
					aDest.DepositsBalance = -aHistory.FinYtdDeposits ?? Decimal.Zero;
					aDest.RetainageWithheld = aHistory.FinPtdRetainageWithheld ?? Decimal.Zero;
					aDest.RetainageReleased = aHistory.FinPtdRetainageReleased ?? Decimal.Zero;
				}
			}
			else
			{
				if (aUseCurrency)
				{
					aDest.Sales = aHistory.CuryTranPtdSales ?? 0m;
					aDest.Payments = aHistory.CuryTranPtdPayments ?? 0m;
					aDest.Discount = aHistory.CuryTranPtdDiscounts ?? 0m;
					aDest.RGOL = 0m;
					aDest.CrAdjustments = aHistory.CuryTranPtdCrAdjustments ?? 0m;
					aDest.DrAdjustments = aHistory.CuryTranPtdDrAdjustments ?? 0m;
					aDest.BegBalance = aHistory.CuryTranBegBalance ?? 0m;
					aDest.CuryID = aHistory.CuryID;
					aDest.FinPtdRevaluated = Decimal.Zero;
					aDest.Deposits = aHistory.CuryTranPtdDeposits ?? Decimal.Zero;
					aDest.DepositsBalance = -aHistory.CuryTranYtdDeposits ?? Decimal.Zero;
					aDest.RetainageWithheld = aHistory.CuryTranPtdRetainageWithheld ?? Decimal.Zero;
					aDest.RetainageReleased = aHistory.CuryTranPtdRetainageReleased ?? Decimal.Zero;
				}
				else
				{
					aDest.Sales = aHistory.TranPtdSales ?? 0m;
					aDest.Payments = aHistory.TranPtdPayments ?? 0m;
					aDest.Discount = aHistory.TranPtdDiscounts ?? 0m;
					aDest.RGOL = -aHistory.TranPtdRGOL ?? 0m;
					aDest.CrAdjustments = aHistory.TranPtdCrAdjustments ?? 0m;
					aDest.DrAdjustments = aHistory.TranPtdDrAdjustments ?? 0m;
					aDest.BegBalance = aHistory.TranBegBalance ?? 0m;
					aDest.COGS = aHistory.TranPtdCOGS ?? 0m;
					aDest.FinCharges = aHistory.TranPtdFinCharges ?? 0m;
					aDest.FinPtdRevaluated = aHistory.FinPtdRevalued ?? Decimal.Zero;
					aDest.Deposits = aHistory.TranPtdDeposits ?? Decimal.Zero;
					aDest.DepositsBalance = -aHistory.TranYtdDeposits ?? Decimal.Zero;
					aDest.RetainageWithheld = aHistory.TranPtdRetainageWithheld ?? Decimal.Zero;
					aDest.RetainageReleased = aHistory.TranPtdRetainageReleased ?? Decimal.Zero;
				}
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
