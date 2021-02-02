using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.GL.Attributes;
using PX.Objects.CM;
using PX.Objects.Common;

using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Tools;
using PX.Objects.Common.Utility;
using PX.Objects.GL.FinPeriods.TableDefinition;


namespace PX.Objects.AP
{
	/// <summary>
	/// A projection DAC over <see cref="CuryAPHistory"/> that is intended to close the gaps 
	/// in AP history records. (The gaps in AP history records appear if AP history records do 
	/// not exist for every financial period defined in the system.) That is, the purpose of 
	/// this DAC is to calculate the <see cref="LastActivityPeriod">last activity period</see> 
	/// for every existing <see cref="MasterFinPeriod">financial period</see>, so that inquiries and 
	/// reports that produce information for a given financial period can look at the latest
	/// available <see cref="CuryAPHistory"/> record. For example, this projection is
	/// used in the Vendor Summary (AP401000) inquiry, which corresponds to the
	/// <see cref="APVendorBalanceEnq"/> graph.
	/// </summary>
	[Serializable]
	[PXProjection(typeof(Select5<CuryAPHistory,
		InnerJoin<MasterFinPeriod,
			On<MasterFinPeriod.finPeriodID, GreaterEqual<CuryAPHistory.finPeriodID>>>,
		Aggregate<
		GroupBy<CuryAPHistory.branchID,
		GroupBy<CuryAPHistory.vendorID,
		GroupBy<CuryAPHistory.accountID,
		GroupBy<CuryAPHistory.subID,
		GroupBy<CuryAPHistory.curyID,
		Max<CuryAPHistory.finPeriodID,
		GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>>))]
	[PXPrimaryGraph(
		new Type[] {
			typeof(APDocumentEnq),
			typeof(APVendorBalanceEnq)
		},
		new Type[] {
			typeof(Where<APHistoryByPeriod.vendorID, IsNotNull>),
			typeof(Where<APHistoryByPeriod.vendorID, IsNull>)
		},
		Filters = new Type[] {
			typeof(APDocumentEnq.APDocumentFilter),
			typeof(APVendorBalanceEnq.APHistoryFilter)
		})]
	[PXCacheName(Messages.APHistoryByPeriod)]
	public partial class APHistoryByPeriod : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(IsKey = true, BqlField = typeof(CuryAPHistory.branchID))]
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
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor(IsKey = true, BqlField = typeof(CuryAPHistory.vendorID))]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(IsKey = true, BqlField = typeof(CuryAPHistory.accountID))]
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
		[SubAccount(IsKey = true, BqlField = typeof(CuryAPHistory.subID))]
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
		[PXDBString(5, IsUnicode = true, IsKey=true, InputMask = ">LLLLL", BqlField = typeof(CuryAPHistory.curyID))]
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
		[GL.FinPeriodID(BqlField = typeof(CuryAPHistory.finPeriodID))]
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
	[PXHidden]
	[Serializable]
	[PXProjection(typeof(Select5<APHistory,
		InnerJoin<MasterFinPeriod,
			On<MasterFinPeriod.finPeriodID, GreaterEqual<APHistory.finPeriodID>>>,
		Aggregate<
		GroupBy<APHistory.branchID,
		GroupBy<APHistory.vendorID,
		GroupBy<APHistory.accountID,
		GroupBy<APHistory.subID,
		Max<APHistory.finPeriodID,
		GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>))]
	[PXPrimaryGraph(
		new Type[] {
			typeof(APDocumentEnq),
			typeof(APVendorBalanceEnq)
		},
		new Type[] {
			typeof(Where<APHistoryByPeriod.vendorID, IsNotNull>),
			typeof(Where<APHistoryByPeriod.vendorID, IsNull>)
		},
		Filters = new Type[] {
			typeof(APDocumentEnq.APDocumentFilter),
			typeof(APVendorBalanceEnq.APHistoryFilter)
		})]
	[PXCacheName(Messages.APHistoryByPeriod)]
	public partial class APHistory2ByPeriod : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(IsKey = true, BqlField = typeof(APHistory.branchID))]
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
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor(IsKey = true, BqlField = typeof(APHistory.vendorID))]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(IsKey = true, BqlField = typeof(APHistory.accountID))]
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
		[SubAccount(IsKey = true, BqlField = typeof(APHistory.subID))]
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
		[GL.FinPeriodID(BqlField = typeof(APHistory.finPeriodID))]
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
	/// A projection DAC over <see cref="APHistory"/> that is intended to close the gaps 
	/// in AP history records. (The gaps in AP history records appear if AR history records 
	/// do not exist for every financial period defined in the system.) That is, the purpose 
	/// of this DAC is to calculate the <see cref="LastActivityPeriod">last activity period</see> 
	/// for every existing <see cref="MasterFinPeriod">financial period</see>, so that inquiries and reports 
	/// that produce information for a given financial period can look at the latest available 
	/// <see cref="APHistory"/> record. For example, this projection is used in the Vendor 
	/// Summary (AP401000) form, which corresponds to the <see cref="APVendorBalanceEnq"/> graph.
	/// </summary>
	[Serializable]
	[PXCacheName(Messages.BaseAPHistoryByPeriod)]
	[PXProjection(typeof(Select5<
		APHistory,
		InnerJoin<MasterFinPeriod,
			On<MasterFinPeriod.finPeriodID, GreaterEqual<APHistory.finPeriodID>>>,
		Aggregate<
			GroupBy<APHistory.branchID,
			GroupBy<APHistory.vendorID,
			GroupBy<APHistory.accountID,
			GroupBy<APHistory.subID,
			Max<APHistory.finPeriodID,
			GroupBy<MasterFinPeriod.finPeriodID>>>>>>>>))]
	[PXPrimaryGraph(
		new Type[] {
			typeof(APDocumentEnq),
			typeof(APVendorBalanceEnq)
		},
		new Type[] {
			typeof(Where<BaseAPHistoryByPeriod.vendorID, IsNotNull>),
			typeof(Where<BaseAPHistoryByPeriod.vendorID, IsNull>)
		},
		Filters = new Type[] {
			typeof(APDocumentEnq.APDocumentFilter),
			typeof(APVendorBalanceEnq.APHistoryFilter)
		})]
	public partial class BaseAPHistoryByPeriod : PX.Data.IBqlTable
	{
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[PXDBInt(IsKey = true, BqlField = typeof(APHistory.branchID))]
		[PXSelector(typeof(Branch.branchID), SubstituteKey = typeof(Branch.branchCD))]
        [PXUIField(DisplayName = "Branch")]
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
		#region VendorID
		public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
		protected Int32? _VendorID;
		[Vendor(IsKey = true, BqlField = typeof(APHistory.vendorID), CacheGlobal = true)]
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
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[Account(IsKey = true, BqlField = typeof(APHistory.accountID))]
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
		[SubAccount(IsKey = true, BqlField = typeof(APHistory.subID))]
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
		[GL.FinPeriodID(BqlField = typeof(APHistory.finPeriodID))]
        [PXUIField(DisplayName = "Last Activity Period")]
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
        [PXUIField(DisplayName = "Post Period")]
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
	public class APVendorBalanceEnq : PXGraph<APVendorBalanceEnq>
	{
		#region Internal Types
		[Serializable]
		public partial class APHistoryFilter : IBqlTable
		{
			#region OrganizationID
			public abstract class organizationID : PX.Data.BQL.BqlInt.Field<organizationID> { }

			[Organization(false, Required = false)]
			public int? OrganizationID { get; set; }
			#endregion
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

			[BranchOfOrganization(typeof(APHistoryFilter.organizationID), false)]
			public int? BranchID { get; set; }
			#endregion
			#region OrgBAccountID
			public abstract class orgBAccountID : IBqlField { }

			[OrganizationTree(typeof(organizationID), typeof(branchID), onlyActive: false)]
			public int? OrgBAccountID { get; set; }
			#endregion
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			protected Int32? _AccountID;
			[GL.Account(null, typeof(Search5<Account.accountID,
						InnerJoin<APHistory, On<Account.accountID, Equal<APHistory.accountID>>>,
						Where<Match<Current<AccessInfo.userName>>>,
					   Aggregate<GroupBy<Account.accountID>>>),
				DisplayName = "AP Account", DescriptionField = typeof(GL.Account.description))]
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
			public abstract class subID : PX.Data.BQL.BqlString.Field<subID> { }
			protected String _SubID;
			[PXDBString(30, IsUnicode = true)]
			[PXUIField(DisplayName = "AP Subaccount", Visibility = PXUIVisibility.Invisible, FieldClass = SubAccountAttribute.DimensionName)]
			[PXDimension("SUBACCOUNT", ValidComboRequired = false)]
			public virtual String SubID
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
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL")]
			[PXSelector(typeof(CM.Currency.curyID), CacheGlobal = true)]
			[PXUIField(DisplayName = "Currency")]
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
			#region CashAcctID
			public abstract class cashAcctID : PX.Data.BQL.BqlInt.Field<cashAcctID> { }
			protected int? _CashAcctID;
			[CashAccount]
			public virtual Int32? CashAcctID
			{
				get
				{
					return this._CashAcctID;
				}
				set
				{
					this._CashAcctID = value;
				}
			}
			#endregion
			#region PaymentMethodID
			public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
			protected String _PaymentMethodID;
			[PXDBString(10, IsUnicode = true)]
			[PXUIField(DisplayName = "Payment Method")]
            [PXSelector(typeof(Search<CA.PaymentMethod.paymentMethodID, Where<CA.PaymentMethod.useForAP, Equal<True>>>), DescriptionField = typeof(CA.PaymentMethod.descr))]
			public virtual String PaymentMethodID
			{
				get
				{
					return this._PaymentMethodID;
				}
				set
				{
					this._PaymentMethodID = value;
				}
			}
			#endregion
			#region VendorClassID
			public abstract class vendorClassID : PX.Data.BQL.BqlString.Field<vendorClassID> { }
			protected String _VendorClassID;
			[PXDBString(10, IsUnicode = true)]
			[PXSelector(typeof(VendorClass.vendorClassID), DescriptionField = typeof(VendorClass.descr), CacheGlobal = true)]
			[PXUIField(DisplayName = "Vendor Class")]
			public virtual String VendorClassID
			{
				get
				{
					return this._VendorClassID;
				}
				set
				{
					this._VendorClassID = value;
				}
			}
			#endregion
			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			protected String _Status;
			[PXDBString(1, IsFixed = true)]
			[PXUIField(DisplayName = "Status")]
			[CR.BAccount.status.ListAttribute()]
			public virtual String Status
			{
				get
				{
					return this._Status;
				}
				set
				{
					this._Status = value;
				}
			}
			#endregion
			#region ShowWithBalanceOnly
			public abstract class showWithBalanceOnly : PX.Data.BQL.BqlBool.Field<showWithBalanceOnly> { }
			protected bool? _ShowWithBalanceOnly;
			[PXDBBool()]
			[PXDefault(true)]
			[PXUIField(DisplayName = "Vendors with Balance Only")]
			public virtual bool? ShowWithBalanceOnly
			{
				get
				{
					return this._ShowWithBalanceOnly;
				}
				set
				{
					this._ShowWithBalanceOnly = value;
				}
			}
			#endregion

			#region UseMasterCalendar
			public abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }

			[PXBool]
			[PXUIField(DisplayName = Common.Messages.UseMasterCalendar)]
			[PXUIVisible(typeof(FeatureInstalled<FeaturesSet.multipleCalendarsSupport>))]
			public bool? UseMasterCalendar { get; set; }
			#endregion

			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			protected String _FinPeriodID;
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
				branchSourceType: typeof(APHistoryFilter.branchID),
				organizationSourceType: typeof(APHistoryFilter.organizationID),
				useMasterCalendarSourceType: typeof(APHistoryFilter.useMasterCalendar),
				redefaultOrRevalidateOnOrganizationSourceUpdated: false)]
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
			[PXDefault(true)]
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
			#region SubCD Wildcard
			public abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { };
			[PXDBString(30, IsUnicode = true)]
			public virtual String SubCDWildcard
			{
				get
				{
					return SubCDUtils.CreateSubCDWildcard(this._SubID, SubAccountAttribute.DimensionName);
				}
			}
            #endregion
            
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			protected Int32? _VendorID;
			[PXDBInt()]
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
			
		}

	    public partial class APHistorySummary : IBqlTable
	    {
            #region CuryBalanceSummary
            public abstract class curyBalanceSummary : PX.Data.BQL.BqlDecimal.Field<curyBalanceSummary> { }

            [PXBaseCury(typeof(APHistoryFilter.curyID))]
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

            #region CuryDepositsSummary
            public abstract class curyDepositsSummary : PX.Data.BQL.BqlDecimal.Field<curyDepositsSummary> { }

            [PXCury(typeof(APHistoryFilter.curyID))]
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

			[PXBaseCury(typeof(APHistoryFilter.curyID))]
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
            /// Specifies (if set to <c>true</c>) that the <see cref="APVendorBalanceEnq.history"/> delegate calculated the summary.
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
				this.DepositsSummary = decimal.Zero;
				this.CuryBalanceSummary = decimal.Zero;
				this.CuryDepositsSummary = decimal.Zero;
				this.BalanceRetainedSummary = decimal.Zero;
				this.CuryBalanceRetainedSummary = decimal.Zero;
				this.Calculated = false;
			}
		}

        [Serializable]
		public partial class APHistoryResult : IBqlTable
		{
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			protected Int32? _VendorID;
			[PXDBInt()]
			[PXDefault()]
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
			#region AcctCD
			public abstract class acctCD : PX.Data.BQL.BqlString.Field<acctCD> { }
			protected string _AcctCD;
			[PXDimensionSelector(VendorAttribute.DimensionName, typeof(Vendor.acctCD), typeof(acctCD),
				typeof(Vendor.acctCD), typeof(Vendor.acctName))]
			[PXDBString(30, IsUnicode = true, IsKey = true, InputMask = "")]
			[PXUIField(DisplayName = "Vendor ID", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String AcctCD
			{
				get
				{
					return this._AcctCD;
				}
				set
				{
					this._AcctCD = value;
				}
			}
			#endregion
			#region AcctName
			public abstract class acctName : PX.Data.BQL.BqlString.Field<acctName> { }
			protected String _AcctName;
			[PXDBString(60, IsUnicode = true)]
			[PXUIField(DisplayName = "Vendor Name", Visibility = PXUIVisibility.SelectorVisible)]
			public virtual String AcctName
			{
				get
				{
					return this._AcctName;
				}
				set
				{
					this._AcctName = value;
				}
			}
			#endregion
			#region FinPeriod
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { };
			protected String _FinPeriodID;
			[GL.FinPeriodID()]
			[PXUIField(DisplayName = "Last Activity Period", Visibility = PXUIVisibility.SelectorVisible)]
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
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			protected String _CuryID;
			[PXDBString(5, IsUnicode = true)]
			[PXUIField(DisplayName = "Currency", Visibility = PXUIVisibility.SelectorVisible)]
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
			#region CuryBegBalance
			public abstract class curyBegBalance : PX.Data.BQL.BqlDecimal.Field<curyBegBalance> { }
			protected Decimal? _CuryBegBalance;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Beginning Balance", Visible = false)]
			public virtual Decimal? CuryBegBalance
			{
				get
				{
					return this._CuryBegBalance;
				}
				set
				{
					this._CuryBegBalance = value;
				}
			}
			#endregion
			#region BegBalance
			public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
			protected Decimal? _BegBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Beginning Balance", Visible = false)]
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
			#region CuryEndBalance
			public abstract class curyEndBalance : PX.Data.BQL.BqlDecimal.Field<curyEndBalance> { }
			protected Decimal? _CuryEndBalance;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Ending Balance", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
			public virtual Decimal? CuryEndBalance
			{
				get
				{
					return this._CuryEndBalance;
				}
				set
				{
					this._CuryEndBalance = value;
				}
			}
			#endregion
			#region EndBalance
			public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
			protected Decimal? _EndBalance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Ending Balance", Visible = false, Visibility = PXUIVisibility.SelectorVisible)]
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
			#region CuryBalance
			public abstract class curyBalance : PX.Data.BQL.BqlDecimal.Field<curyBalance> { }

			protected Decimal? _CuryBalance;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Balance", Visible = false)]
			public virtual Decimal? CuryBalance
			{
				get
				{
					return this._CuryBalance;
				}
				set
				{
					this._CuryBalance = value;
				}
			}
			#endregion
			#region Balance
			public abstract class balance : PX.Data.BQL.BqlDecimal.Field<balance> { }

			protected Decimal? _Balance;
			[PXBaseCury()]
			[PXUIField(DisplayName = "Balance", Visible = false)]
			public virtual Decimal? Balance
			{
				get
				{
					return this._Balance;
				}
				set
				{
					this._Balance = value;
				}
			}
			#endregion
			#region CuryPurchases
			public abstract class curyPurchases : PX.Data.BQL.BqlDecimal.Field<curyPurchases> { }
			protected Decimal? _CuryPurchases;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Purchases")]
			public virtual Decimal? CuryPurchases
			{
				get
				{
					return this._CuryPurchases;
				}
				set
				{
					this._CuryPurchases = value;
				}
			}
			#endregion
			#region Purchases
			public abstract class purchases : PX.Data.BQL.BqlDecimal.Field<purchases> { }
			protected Decimal? _Purchases;
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Purchases")]
			public virtual Decimal? Purchases
			{
				get
				{
					return this._Purchases;
				}
				set
				{
					this._Purchases = value;
				}
			}
			#endregion
			#region CuryPayments
			public abstract class curyPayments : PX.Data.BQL.BqlDecimal.Field<curyPayments> { }
			protected Decimal? _CuryPayments;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Payments")]
			public virtual Decimal? CuryPayments
			{
				get
				{
					return this._CuryPayments;
				}
				set
				{
					this._CuryPayments = value;
				}
			}
			#endregion
			#region Payments
			public abstract class payments : PX.Data.BQL.BqlDecimal.Field<payments> { }
			protected Decimal? _Payments;
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Payments")]
			public virtual Decimal? Payments
			{
				get
				{
					return this._Payments;
				}
				set
				{
					this._Payments = value;
				}
			}
			#endregion
			#region CuryDiscount
			public abstract class curyDiscount : PX.Data.BQL.BqlDecimal.Field<curyDiscount> { }
			protected Decimal? _CuryDiscount;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Cash Discount Taken")]
			public virtual Decimal? CuryDiscount
			{
				get
				{
					return this._CuryDiscount;
				}
				set
				{
					this._CuryDiscount = value;
				}
			}
			#endregion
			#region Discount
			public abstract class discount : PX.Data.BQL.BqlDecimal.Field<discount> { }
			protected Decimal? _Discount;
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Cash Discount Taken")]
			public virtual Decimal? Discount
			{
				get
				{
					return this._Discount;
				}
				set
				{
					this._Discount = value;
				}
			}
			#endregion
			#region CuryWhTax
			public abstract class curyWhTax : PX.Data.BQL.BqlDecimal.Field<curyWhTax> { }
			protected Decimal? _CuryWhTax;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Tax Withheld")]
			public virtual Decimal? CuryWhTax
			{
				get
				{
					return this._CuryWhTax;
				}
				set
				{
					this._CuryWhTax = value;
				}
			}
			#endregion
			#region WhTax
			public abstract class whTax : PX.Data.BQL.BqlDecimal.Field<whTax> { }
			protected Decimal? _WhTax;
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Tax Withheld")]
			public virtual Decimal? WhTax
			{
				get
				{
					return this._WhTax;
				}
				set
				{
					this._WhTax = value;
				}
			}
			#endregion
			#region RGOL
			public abstract class rGOL : PX.Data.BQL.BqlDecimal.Field<rGOL> { }
			protected Decimal? _RGOL;
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Realized Gain/Loss")]
			public virtual Decimal? RGOL
			{
				get
				{
					return this._RGOL;
				}
				set
				{
					this._RGOL = value;
				}
			}
			#endregion
			#region CuryCrAdjustments
			public abstract class curyCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyCrAdjustments> { }
			protected Decimal? _CuryCrAdjustments;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Credit Adjustments")]
			public virtual Decimal? CuryCrAdjustments
			{
				get
				{
					return this._CuryCrAdjustments;
				}
				set
				{
					this._CuryCrAdjustments = value;
				}
			}
			#endregion
			#region CrAdjustments
			public abstract class crAdjustments : PX.Data.BQL.BqlDecimal.Field<crAdjustments> { }
			protected Decimal? _CrAdjustments;
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Credit Adjustments")]
			public virtual Decimal? CrAdjustments
			{
				get
				{
					return this._CrAdjustments;
				}
				set
				{
					this._CrAdjustments = value;
				}
			}
			#endregion
			#region CuryDrAdjustments
			public abstract class curyDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyDrAdjustments> { }
			protected Decimal? _CuryDrAdjustments;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Debit Adjustments")]
			public virtual Decimal? CuryDrAdjustments
			{
				get
				{
					return this._CuryDrAdjustments;
				}
				set
				{
					this._CuryDrAdjustments = value;
				}
			}
			#endregion
			#region DrAdjustments
			public abstract class drAdjustments : PX.Data.BQL.BqlDecimal.Field<drAdjustments> { }
			protected Decimal? _DrAdjustments;
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Debit Adjustments")]
			public virtual Decimal? DrAdjustments
			{
				get
				{
					return this._DrAdjustments;
				}
				set
				{
					this._DrAdjustments = value;
				}
			}
			#endregion
			#region CuryDeposits
			public abstract class curyDeposits : PX.Data.BQL.BqlDecimal.Field<curyDeposits> { }
			protected Decimal? _CuryDeposits;
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency PTD Prepayments")]
			public virtual Decimal? CuryDeposits
			{
				get
				{
					return this._CuryDeposits;
				}
				set
				{
					this._CuryDeposits = value;
				}
			}
			#endregion
			#region Deposits
			public abstract class deposits : PX.Data.BQL.BqlDecimal.Field<deposits> { }
			protected Decimal? _Deposits;
			[PXBaseCury()]
			[PXUIField(DisplayName = "PTD Prepayments")]
			public virtual Decimal? Deposits
			{
				get
				{
					return this._Deposits;
				}
				set
				{
					this._Deposits = value;
				}
			}
			#endregion
			#region CuryDepositsBalance
			public abstract class curyDepositsBalance : PX.Data.BQL.BqlDecimal.Field<curyDepositsBalance> { }
			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = Common.Messages.CurrencyPrepaymentBalance)]
			public virtual decimal? CuryDepositsBalance
				{
				get;
				set;
			}
			#endregion
			#region DepositsBalance
			public abstract class depositsBalance : PX.Data.BQL.BqlDecimal.Field<depositsBalance> { }
			[PXBaseCury]
			[PXUIField(DisplayName = Common.Messages.PrepaymentBalance)]
			public virtual decimal? DepositsBalance
			{
				get;
				set;
			}
			#endregion
			#region CuryRetainageWithheld
			public abstract class curyRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyRetainageWithheld> { }
			[PXDBCury(typeof(APHistoryResult.curyID))]
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
			[PXDBCury(typeof(APHistoryResult.curyID))]
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

			[PXDBCury(typeof(APHistoryResult.curyID))]
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

			[PXDBCury(typeof(APHistoryResult.curyID))]
			[PXUIField(DisplayName = "Currency Ending Retained Balance", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? CuryEndRetainedBalance { get; set; }
			#endregion
			#region EndRetainedBalance
			public abstract class endRetainedBalance : PX.Data.BQL.BqlDecimal.Field<endRetainedBalance> { }

			[PXBaseCury()]
			[PXUIField(DisplayName = "Ending Retained Balance", FieldClass = nameof(FeaturesSet.Retainage))]
			public virtual Decimal? EndRetainedBalance { get; set; }
			#endregion
			#region NoteID
			public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
			protected Guid? _NoteID;
			[PXNote()]
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
			#region Converted
			public abstract class converted : PX.Data.BQL.BqlBool.Field<converted> { }
			protected bool? _Converted;
			[PXDBBool()]
			[PXDefault(false)]
			[PXUIField(DisplayName = "Converted to Base Currency", Visible = false, Enabled = false)]
			public virtual bool? Converted
			{
				get
				{
					return this._Converted;
				}
				set
				{
					this._Converted = value;
				}
			}
			#endregion
			public virtual void RecalculateEndBalance()
			{
				const decimal zero = Decimal.Zero;
				this.RecalculateBalance();
				this.CuryEndBalance = (this.CuryBegBalance ?? zero) + (this.CuryBalance ?? zero);
				this.EndBalance = (this.BegBalance ?? zero) + (this.Balance ?? zero);
			}
			public virtual void RecalculateBalance()
			{
				const decimal zero = Decimal.Zero;
				this.Balance = (this.Purchases ?? zero)
							   - (this.Payments ?? zero)
							   - (this.Discount ?? zero)
							   - (this.WhTax ?? zero)
							   + (this.RGOL ?? zero)
							   - (this.DrAdjustments ?? zero)
							   + (this.CrAdjustments ?? zero);
				this.CuryBalance = (this.CuryPurchases ?? zero)
							   - (this.CuryPayments ?? zero)
							   - (this.CuryDiscount ?? zero)
							   - (this.CuryWhTax ?? zero)
							   - (this.CuryDrAdjustments ?? zero)
							   + (this.CuryCrAdjustments ?? zero);
			}
			public virtual void CopyValueToCuryValue(string aBaseCuryID)
			{
				this.CuryBegBalance = this.BegBalance ?? Decimal.Zero;
				this.CuryPurchases = this.Purchases ?? Decimal.Zero;
				this.CuryPayments = this.Payments ?? Decimal.Zero;
				this.CuryDiscount = this.Discount ?? Decimal.Zero;
				this.CuryWhTax = this.WhTax ?? Decimal.Zero;
				this.CuryCrAdjustments = this.CrAdjustments ?? Decimal.Zero;
				this.CuryDrAdjustments = this.DrAdjustments ?? Decimal.Zero;
				this.CuryDeposits = this.Deposits ?? Decimal.Zero;
				this.CuryDepositsBalance = this.DepositsBalance ?? Decimal.Zero;
				this.CuryID = aBaseCuryID;
				this.CuryEndBalance = this.EndBalance ?? Decimal.Zero;
				this.CuryRetainageWithheld = this.RetainageWithheld ?? Decimal.Zero;
				this.CuryRetainageReleased = this.RetainageReleased ?? Decimal.Zero;
				this.CuryBegRetainedBalance = this.BegRetainedBalance ?? Decimal.Zero;
				this.CuryEndRetainedBalance = this.EndRetainedBalance ?? Decimal.Zero;
				this.Converted = true;
			}
		}

		#region Service Types
		/// <summary>
		/// A projection DAC over <see cref="CuryAPHistory"/> that is intended to calculate the 
		/// <see cref="LastActivityPeriod">latest available history period</see> for every 
		/// combination of branch, vendor, account, subaccount, and currency.
		/// </summary>
		[Serializable]
		[PXProjection(typeof(Select4<CuryAPHistory,
			Aggregate<
			GroupBy<CuryAPHistory.branchID,
			GroupBy<CuryAPHistory.vendorID,
			GroupBy<CuryAPHistory.accountID,
			GroupBy<CuryAPHistory.subID,
			GroupBy<CuryAPHistory.curyID,
			Max<CuryAPHistory.finPeriodID
			>>>>>>>>))]
		[PXCacheName(Messages.APLatestHistory)]
		public partial class APLatestHistory : PX.Data.IBqlTable
		{
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			protected Int32? _BranchID;
			[PXDBInt(IsKey = true, BqlField = typeof(CuryAPHistory.branchID))]
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
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			protected Int32? _VendorID;
			[PXDBInt(IsKey = true, BqlField = typeof(CuryAPHistory.vendorID))]
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
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			protected Int32? _AccountID;
			[PXDBInt(IsKey = true, BqlField = typeof(CuryAPHistory.accountID))]
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
			[PXDBInt(IsKey = true, BqlField = typeof(CuryAPHistory.subID))]
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
			[PXDBString(5, IsUnicode = true, InputMask = ">LLLLL", BqlField = typeof(CuryAPHistory.curyID))]
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
			[GL.FinPeriodID(BqlField = typeof(CuryAPHistory.finPeriodID))]
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
		[PXProjection(typeof(Select<CuryAPHistory>))]
		public class CuryAPHistoryTran : IBqlTable
		{
			#region BranchID
			public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
			[PXDBInt(IsKey = true, BqlTable = typeof(CuryAPHistory))]
			public virtual int? BranchID { get; set; }
			#endregion
			#region AccountID
			public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
			[PXDBInt(IsKey = true, BqlTable = typeof(CuryAPHistory))]
			public virtual int? AccountID { get; set; }
			#endregion
			#region SubID
			public abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
			[PXDBInt(IsKey = true, BqlTable = typeof(CuryAPHistory))]
			public virtual int? SubID { get; set; }
			#endregion
			#region VendorID
			public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
			[PXDBInt(IsKey = true, BqlTable = typeof(CuryAPHistory))]
			public virtual int? VendorID { get; set; }
			#endregion
			#region CuryID
			public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
			[PXDBString(IsKey = true, BqlTable = typeof(CuryAPHistory))]
			public virtual string CuryID { get; set; }
			#endregion
			#region FinPeriodID
			public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
			[PXDBString(IsKey = true, BqlTable = typeof(CuryAPHistory))]
			public virtual string FinPeriodID { get; set; }
			#endregion
			#region FinBegBalance
			public abstract class finBegBalance : PX.Data.BQL.BqlDecimal.Field<finBegBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finBegBalance>, CuryAPHistory.finYtdBalance>), typeof(decimal))]
			public virtual decimal? FinBegBalance { get; set; }
			#endregion
			#region FinPtdPurchases
			public abstract class finPtdPurchases : PX.Data.BQL.BqlDecimal.Field<finPtdPurchases> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdPurchases>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdPurchases { get; set; }
			#endregion
			#region FinPtdPayments
			public abstract class finPtdPayments : PX.Data.BQL.BqlDecimal.Field<finPtdPayments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdPayments>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdPayments { get; set; }
			#endregion
			#region FinPtdDrAdjustments
			public abstract class finPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdDrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdDrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdDrAdjustments { get; set; }
			#endregion
			#region FinPtdCrAdjustments
			public abstract class finPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<finPtdCrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdCrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdCrAdjustments { get; set; }
			#endregion
			#region FinPtdDiscTaken
			public abstract class finPtdDiscTaken : PX.Data.BQL.BqlDecimal.Field<finPtdDiscTaken> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdDiscTaken>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdDiscTaken { get; set; }
			#endregion
			#region FinPtdRGOL
			public abstract class finPtdRGOL : PX.Data.BQL.BqlDecimal.Field<finPtdRGOL> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdRGOL>, Zero>>), typeof(decimal))]
			public virtual decimal? FinPtdRGOL { get; set; }
			#endregion
			#region FinYtdBalance
			public abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? FinYtdBalance { get; set; }
			#endregion
			#region FinPtdDeposits
			public abstract class finPtdDeposits : PX.Data.BQL.BqlDecimal.Field<finPtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdDeposits>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdDeposits { get; set; }
			#endregion
			#region FinYtdDeposits
			public abstract class finYtdDeposits : PX.Data.BQL.BqlDecimal.Field<finYtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<CuryAPHistory.finYtdDeposits>), typeof(decimal))]
			public virtual decimal? FinYtdDeposits { get; set; }
			#endregion
			#region TranBegBalance
			public abstract class tranBegBalance : PX.Data.BQL.BqlDecimal.Field<tranBegBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranBegBalance>, CuryAPHistory.tranYtdBalance>), typeof(decimal))]
			public virtual decimal? TranBegBalance { get; set; }
			#endregion
			#region TranPtdPurchases
			public abstract class tranPtdPurchases : PX.Data.BQL.BqlDecimal.Field<tranPtdPurchases> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdPurchases>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdPurchases { get; set; }
			#endregion
			#region TranPtdPayments
			public abstract class tranPtdPayments : PX.Data.BQL.BqlDecimal.Field<tranPtdPayments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdPayments>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdPayments { get; set; }
			#endregion
			#region TranPtdDrAdjustments
			public abstract class tranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdDrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdDrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdDrAdjustments { get; set; }
			#endregion
			#region TranPtdCrAdjustments
			public abstract class tranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<tranPtdCrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdCrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdCrAdjustments { get; set; }
			#endregion
			#region TranPtdDiscTaken
			public abstract class tranPtdDiscTaken : PX.Data.BQL.BqlDecimal.Field<tranPtdDiscTaken> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdDiscTaken>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdDiscTaken { get; set; }
			#endregion
			#region TranPtdRGOL
			public abstract class tranPtdRGOL : PX.Data.BQL.BqlDecimal.Field<tranPtdRGOL> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdRGOL>, Zero>>), typeof(decimal))]
			public virtual decimal? TranPtdRGOL { get; set; }
			#endregion
			#region TranYtdBalance
			public abstract class tranYtdBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdBalance> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? TranYtdBalance { get; set; }
			#endregion
			#region TranPtdDeposits
			public abstract class tranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranPtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdDeposits>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdDeposits { get; set; }
			#endregion
			#region TranYtdDeposits
			public abstract class tranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<tranYtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<CuryAPHistory.tranYtdDeposits>), typeof(decimal))]
			public virtual decimal? TranYtdDeposits { get; set; }
			#endregion
			#region CuryFinBegBalance
			public abstract class curyFinBegBalance : PX.Data.BQL.BqlDecimal.Field<curyFinBegBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinBegBalance>, CuryAPHistory.curyFinYtdBalance>), typeof(decimal))]
			public virtual decimal? CuryFinBegBalance { get; set; }
			#endregion
			#region CuryFinPtdPurchases
			public abstract class curyFinPtdPurchases : PX.Data.BQL.BqlDecimal.Field<curyFinPtdPurchases> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdPurchases>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdPurchases { get; set; }
			#endregion
			#region CuryFinPtdPayments
			public abstract class curyFinPtdPayments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdPayments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdPayments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdPayments { get; set; }
			#endregion
			#region CuryFinPtdDrAdjustments
			public abstract class curyFinPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdDrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdDrAdjustments { get; set; }
			#endregion
			#region CuryFinPtdCrAdjustments
			public abstract class curyFinPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyFinPtdCrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdCrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdCrAdjustments { get; set; }
			#endregion
			#region CuryFinPtdDiscTaken
			public abstract class curyFinPtdDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDiscTaken> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdDiscTaken>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdDiscTaken { get; set; }
			#endregion
			#region CuryFinYtdBalance
			public abstract class curyFinYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalance> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? CuryFinYtdBalance { get; set; }
			#endregion
			#region CuryFinPtdDeposits
			public abstract class curyFinPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinPtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdDeposits>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdDeposits { get; set; }
			#endregion
			#region CuryFinYtdDeposits
			public abstract class curyFinYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyFinYtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<CuryAPHistory.curyFinYtdDeposits>), typeof(decimal))]
			public virtual decimal? CuryFinYtdDeposits { get; set; }
			#endregion
			#region CuryTranBegBalance
			public abstract class curyTranBegBalance : PX.Data.BQL.BqlDecimal.Field<curyTranBegBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranBegBalance>, CuryAPHistory.curyTranYtdBalance>), typeof(decimal))]
			public virtual decimal? CuryTranBegBalance { get; set; }
			#endregion
			#region CuryTranPtdPurchases
			public abstract class curyTranPtdPurchases : PX.Data.BQL.BqlDecimal.Field<curyTranPtdPurchases> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdPurchases>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdPurchases { get; set; }
			#endregion
			#region CuryTranPtdPayments
			public abstract class curyTranPtdPayments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdPayments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdPayments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdPayments { get; set; }
			#endregion
			#region CuryTranPtdDrAdjustments
			public abstract class curyTranPtdDrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdDrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdDrAdjustments { get; set; }
			#endregion
			#region CuryTranPtdCrAdjustments
			public abstract class curyTranPtdCrAdjustments : PX.Data.BQL.BqlDecimal.Field<curyTranPtdCrAdjustments> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdCrAdjustments>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdCrAdjustments { get; set; }
			#endregion
			#region CuryTranPtdDiscTaken
			public abstract class curyTranPtdDiscTaken : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDiscTaken> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdDiscTaken>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdDiscTaken { get; set; }
			#endregion
			#region CuryTranYtdBalance
			public abstract class curyTranYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyTranYtdBalance> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? CuryTranYtdBalance { get; set; }
			#endregion
			#region CuryTranPtdDeposits
			public abstract class curyTranPtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranPtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdDeposits>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdDeposits { get; set; }
			#endregion
			#region CuryTranYtdDeposits
			public abstract class curyTranYtdDeposits : PX.Data.BQL.BqlDecimal.Field<curyTranYtdDeposits> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Minus<CuryAPHistory.curyTranYtdDeposits>), typeof(decimal))]
			public virtual decimal? CuryTranYtdDeposits { get; set; }
			#endregion
			#region DetDeleted
			public abstract class detDeleted : PX.Data.BQL.BqlBool.Field<detDeleted> { }
			[PXDBBool(BqlTable = typeof(CuryAPHistory))]
			public virtual bool? DetDeleted { get; set; }
			#endregion
			#region CuryFinPtdWhTax
			public abstract class curyFinPtdWhTax : PX.Data.BQL.BqlDecimal.Field<curyFinPtdWhTax> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdWhTax>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdWhTax { get; set; }
			#endregion
			#region CuryTranPtdWhTax
			public abstract class curyTranPtdWhTax : PX.Data.BQL.BqlDecimal.Field<curyTranPtdWhTax> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdWhTax>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdWhTax { get; set; }
			#endregion
			#region FinPtdWhTax
			public abstract class finPtdWhTax : PX.Data.BQL.BqlDecimal.Field<finPtdWhTax> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdWhTax>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdWhTax { get; set; }
			#endregion
			#region TranPtdWhTax
			public abstract class tranPtdWhTax : PX.Data.BQL.BqlDecimal.Field<tranPtdWhTax> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdWhTax>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdWhTax { get; set; }
			#endregion
			#region CuryFinPtdRetainageWithheld
			public abstract class curyFinPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyFinPtdRetainageWithheld> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdRetainageWithheld>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdRetainageWithheld { get; set; }
			#endregion
			#region FinPtdRetainageWithheld
			public abstract class finPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageWithheld> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdRetainageWithheld>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdRetainageWithheld { get; set; }
			#endregion
			#region CuryTranPtdRetainageWithheld
			public abstract class curyTranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyTranPtdRetainageWithheld> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdRetainageWithheld>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdRetainageWithheld { get; set; }
			#endregion
			#region TranPtdRetainageWithheld
			public abstract class tranPtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageWithheld> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdRetainageWithheld>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdRetainageWithheld { get; set; }
			#endregion
			#region CuryFinYtdRetainageWithheld
			public abstract class curyFinYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageWithheld> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? CuryFinYtdRetainageWithheld { get; set; }
			#endregion
			#region FinYtdRetainageWithheld
			public abstract class finYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageWithheld> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? FinYtdRetainageWithheld { get; set; }
			#endregion
			#region curyFinRetainedBalance
			public abstract class curyFinBegRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyFinBegRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
				Sub<CuryAPHistory.curyFinYtdRetainageWithheld,
				Add<CuryAPHistory.curyFinYtdRetainageReleased,
				Sub<Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdRetainageWithheld>, Zero>,
				Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdRetainageReleased>, Zero>>>>),
				typeof(decimal))]
			public virtual decimal? CuryFinBegRetainedBalance { get; set; }
			#endregion
			#region FinRetainedBalance
			public abstract class finBegRetainedBalance : PX.Data.BQL.BqlDecimal.Field<finBegRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
				Sub<CuryAPHistory.finYtdRetainageWithheld,
				Add<CuryAPHistory.finYtdRetainageReleased,
				Sub<Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdRetainageWithheld>, Zero>,
				Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdRetainageReleased>, Zero>>>>),
				typeof(decimal))]
			public virtual decimal? FinBegRetainedBalance { get; set; }
			#endregion
			#region curyTranRetainedBalance
			public abstract class curyTranBegRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyTranBegRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
				Sub<CuryAPHistory.curyTranYtdRetainageWithheld,
				Add<CuryAPHistory.curyTranYtdRetainageReleased,
				Sub<Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdRetainageWithheld>, Zero>,
				Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdRetainageReleased>, Zero>>>>),
				typeof(decimal))]
			public virtual decimal? CuryTranBegRetainedBalance { get; set; }
			#endregion
			#region TranRetainedBalance
			public abstract class tranBegRetainedBalance : PX.Data.BQL.BqlDecimal.Field<tranBegRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
				Sub<CuryAPHistory.tranYtdRetainageWithheld,
				Add<CuryAPHistory.tranYtdRetainageReleased,
				Sub<Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdRetainageWithheld>, Zero>,
				Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdRetainageReleased>, Zero>>>>),
				typeof(decimal))]
			public virtual decimal? TranBegRetainedBalance { get; set; }
			#endregion
			#region CuryFinYtdRetainedBalance
			public abstract class curyFinYtdRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
					Sub<CuryAPHistory.curyFinYtdRetainageWithheld,
						CuryAPHistory.curyFinYtdRetainageReleased>),
				typeof(decimal))]
			public virtual decimal? CuryFinYtdRetainedBalance { get; set; }
			#endregion
			#region FinYtdRetainedBalance
			public abstract class finYtdRetainedBalance : PX.Data.BQL.BqlDecimal.Field<finYtdRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
					Sub<CuryAPHistory.finYtdRetainageWithheld,
						CuryAPHistory.finYtdRetainageReleased>),
				typeof(decimal))]
			public virtual decimal? FinYtdRetainedBalance { get; set; }
			#endregion
			#region CuryTranYtdRetainedBalance
			public abstract class curyTranYtdRetainedBalance : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
					Sub<CuryAPHistory.curyTranYtdRetainageWithheld,
						CuryAPHistory.curyTranYtdRetainageReleased>),
				typeof(decimal))]
			public virtual decimal? CuryTranYtdRetainedBalance { get; set; }
			#endregion
			#region FinYtdRetainedBalance
			public abstract class tranYtdRetainedBalance : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainedBalance> { }
			[PXDecimal()]
			[PXDBCalced(typeof(
					Sub<CuryAPHistory.tranYtdRetainageWithheld,
						CuryAPHistory.tranYtdRetainageReleased>),
				typeof(decimal))]
			public virtual decimal? TranYtdRetainedBalance { get; set; }
			#endregion
			#region CuryTranYtdRetainageWithheld
			public abstract class curyTranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageWithheld> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? CuryTranYtdRetainageWithheld { get; set; }
			#endregion
			#region TranYtdRetainageWithheld
			public abstract class tranYtdRetainageWithheld : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageWithheld> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? TranYtdRetainageWithheld { get; set; }
			#endregion
			#region CuryFinPtdRetainageReleased
			public abstract class curyFinPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyFinPtdRetainageReleased> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyFinPtdRetainageReleased>, Zero>), typeof(decimal))]
			public virtual decimal? CuryFinPtdRetainageReleased { get; set; }
			#endregion
			#region FinPtdRetainageReleased
			public abstract class finPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finPtdRetainageReleased> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdRetainageReleased>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdRetainageReleased { get; set; }
			#endregion
			#region CuryTranPtdRetainageReleased
			public abstract class curyTranPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyTranPtdRetainageReleased> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.curyTranPtdRetainageReleased>, Zero>), typeof(decimal))]
			public virtual decimal? CuryTranPtdRetainageReleased { get; set; }
			#endregion
			#region TranPtdRetainageReleased
			public abstract class tranPtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranPtdRetainageReleased> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.tranPtdRetainageReleased>, Zero>), typeof(decimal))]
			public virtual decimal? TranPtdRetainageReleased { get; set; }
			#endregion
			#region CuryFinYtdRetainageReleased
			public abstract class curyFinYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyFinYtdRetainageReleased> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? CuryFinYtdRetainageReleased { get; set; }
			#endregion
			#region FinYtdRetainageReleased
			public abstract class finYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<finYtdRetainageReleased> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? FinYtdRetainageReleased { get; set; }
			#endregion
			#region CuryTranYtdRetainageReleased
			public abstract class curyTranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<curyTranYtdRetainageReleased> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? CuryTranYtdRetainageReleased { get; set; }
			#endregion
			#region TranYtdRetainageReleased
			public abstract class tranYtdRetainageReleased : PX.Data.BQL.BqlDecimal.Field<tranYtdRetainageReleased> { }
			[PXDBDecimal(4, BqlTable = typeof(CuryAPHistory))]
			public virtual decimal? TranYtdRetainageReleased { get; set; }
			#endregion
			#region FinPtdRevalued
			public abstract class finPtdRevalued : PX.Data.BQL.BqlDecimal.Field<finPtdRevalued> { }
			[PXDecimal()]
			[PXDBCalced(typeof(Switch<Case<Where<CuryAPHistory.finPeriodID, Equal<CurrentValue<APHistoryFilter.finPeriodID>>>, CuryAPHistory.finPtdRevalued>, Zero>), typeof(decimal))]
			public virtual decimal? FinPtdRevalued { get; set; }
			#endregion
		}

		private sealed class decimalZero : PX.Data.BQL.BqlDecimal.Constant<decimalZero>
		{
			public decimalZero()
				: base(Decimal.Zero)
			{
			}
		}
		#endregion
		#endregion

		#region Views/Selects

		public PXFilter<APHistoryFilter> Filter;
		public PXCancel<APHistoryFilter> Cancel;
		[PXFilterable]
		public PXSelect<APHistoryResult> History;
        [PXVirtualDAC]
        public PXFilter<APHistorySummary> Summary;
        public PXSetup<APSetup> APSetup;
		public PXSetup<Company> Company;

		protected virtual IEnumerable history()
		{
			return RetrieveHistoryForPeriod(Filter.Current);
		}

        protected virtual IEnumerable summary()
        {
			if (Summary.Current.Calculated == false)
			{
				var summary = Summary.Cache.CreateInstance() as APHistorySummary;
				summary.ClearSummary();
				Summary.Insert(summary);
				RetrieveHistoryForPeriod(Filter.Current, summary);
				Summary.Update(summary);
			}

			yield return Summary.Current;
		}
		#endregion

		#region Ctor+overrides
		public APVendorBalanceEnq()
		{
			APSetup setup = this.APSetup.Current;
			Company company = this.Company.Current;
			this.History.Cache.AllowDelete = false;
			this.History.Cache.AllowInsert = false;
			this.History.Cache.AllowUpdate = false;
			this.reports.MenuAutoOpen = true;
			this.reports.AddMenuAction(this.aPBalanceByVendorReport);
			this.reports.AddMenuAction(this.vendorHistoryReport);
			this.reports.AddMenuAction(this.aPAgedPastDueReport);
			this.reports.AddMenuAction(this.aPAgedOutstandingReport);
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
		[Obsolete(Common.Messages.WillBeRemovedInAcumatica2019R1)]
		public PXAction<APHistoryFilter> viewDetails;
		public PXAction<APHistoryFilter> previousPeriod;
		public PXAction<APHistoryFilter> nextPeriod;
		public PXAction<APHistoryFilter> reports;
		public PXAction<APHistoryFilter> aPBalanceByVendorReport;
		public PXAction<APHistoryFilter> vendorHistoryReport;
		public PXAction<APHistoryFilter> aPAgedPastDueReport;
		public PXAction<APHistoryFilter> aPAgedOutstandingReport;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }


		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = false)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			if (this.History.Current != null && this.Filter.Current != null)
			{
				APHistoryResult res = this.History.Current;
				APHistoryFilter currentFilter = this.Filter.Current;
				APDocumentEnq graph = PXGraph.CreateInstance<APDocumentEnq>();
				APDocumentEnq.APDocumentFilter filter = graph.Filter.Current;
				Copy(filter, currentFilter);
				filter.VendorID = res.VendorID;
				filter.BalanceSummary = null;
				graph.Filter.Update(filter);
				filter = graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Vendor Details");
			}
			return Filter.Select();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable PreviousPeriod(PXAdapter adapter)
		{
			APHistoryFilter filter = Filter.Current as APHistoryFilter;

			filter.UseMasterCalendar = (filter.OrganizationID == null && filter.BranchID == null);
			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);
			FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.FinPeriodID, looped: true);
			filter.FinPeriodID = prevPeriod != null ? prevPeriod.FinPeriodID : null;
			this.Summary.Current.ClearSummary();
			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable NextPeriod(PXAdapter adapter)
		{
			APHistoryFilter filter = Filter.Current as APHistoryFilter;

			filter.UseMasterCalendar = (filter.OrganizationID == null && filter.BranchID == null);
			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);
			FinPeriod nextPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.FinPeriodID, looped: true);
			filter.FinPeriodID = nextPeriod != null ? nextPeriod.FinPeriodID : null;
			this.Summary.Current.ClearSummary();
			return adapter.Get();
		}

		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.ReportsFolder)]
		protected virtual IEnumerable Reports(PXAdapter adapter)
		{
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.APBalanceByVendorReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable APBalanceByVendorReport(PXAdapter adapter)
		{
			APHistoryFilter filter = Filter.Current;
			APHistoryResult history = History.Current;
			if (filter != null && history != null)
			{
				Dictionary<string, string> parameters = GetBasicReportParameters(filter, history);
				if (!string.IsNullOrEmpty(filter.FinPeriodID))
				{
					parameters["PeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.FinPeriodID);
				}
				parameters["UseMasterCalendar"] = filter.UseMasterCalendar == true ? true.ToString() : false.ToString();
				throw new PXReportRequiredException(parameters, "AP632500", Messages.APBalanceByVendorReport);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.VendorHistoryReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable VendorHistoryReport(PXAdapter adapter)
		{
			APHistoryFilter filter = Filter.Current;
			APHistoryResult history = History.Current;
			if (filter != null && history != null)
			{
				Dictionary<string, string> parameters = GetBasicReportParameters(filter, history);
				if (!string.IsNullOrEmpty(filter.FinPeriodID))
				{
					parameters["FromPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.FinPeriodID);
					parameters["ToPeriodID"] = FinPeriodIDFormattingAttribute.FormatForDisplay(filter.FinPeriodID);
				}
				throw new PXReportRequiredException(parameters, "AP652000", Messages.VendorHistoryReport);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.APAgedPastDueReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable APAgedPastDueReport(PXAdapter adapter)
		{
			APHistoryFilter filter = Filter.Current;
			APHistoryResult history = History.Current;
			if (filter != null && history != null)
			{
				Dictionary<string, string> parameters = GetBasicReportParameters(filter, history);
				throw new PXReportRequiredException(parameters, "AP631000", Messages.APAgedPastDueReport);
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.APAgedOutstandingReport, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		public virtual IEnumerable APAgedOutstandingReport(PXAdapter adapter)
		{
			APHistoryFilter filter = Filter.Current;
			APHistoryResult history = History.Current;
			if (filter != null && history != null)
			{
				Dictionary<string, string> parameters = GetBasicReportParameters(filter, history);
				throw new PXReportRequiredException(parameters, "AP631500", Messages.APAgedOutstandingReport);
			}
			return adapter.Get();
		}

		private Dictionary<string, string> GetBasicReportParameters(APHistoryFilter filter, APHistoryResult currentRow)
		{
			BAccountR bAccount = Data.BQL.Fluent.SelectFrom<BAccountR>
				.Where<BAccountR.bAccountID.IsEqual<Data.BQL.@P.AsInt>>
				.View
				.Select(this, filter.OrgBAccountID);

			return new Dictionary<string, string>
			{
				{ "VendorID" , VendorMaint.FindByID(this, currentRow.VendorID)?.AcctCD },
				{ "OrgBAccountID", bAccount.AcctCD },
			};
		}

		#endregion

		#region Event Handlers
		public virtual void APHistoryFilter_SubID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		public virtual void APHistoryFilter_AccountID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.AccountID = null;
			}
		}

		public virtual void APHistoryFilter_CashAcctID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.CashAcctID = null;
			}
		}

		public virtual void APHistoryFilter_CuryID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.CuryID = null;
			}
		}

		public virtual void APHistoryFilter_FinPeriodID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.FinPeriodID = null;
			}
		}

		public virtual void APHistoryFilter_VendorClassID_ExceptionHandling(PXCache cache, PXExceptionHandlingEventArgs e)
		{
			APHistoryFilter header = e.Row as APHistoryFilter;
			if (header != null)
			{
				e.Cancel = true;
				header.VendorClassID = null;
			}
		}

		public virtual void APHistoryFilter_CuryID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
		{
			cache.SetDefaultExt<APHistoryFilter.splitByCurrency>(e.Row);
		}
		protected virtual void APHistoryFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			this.Summary.Current.ClearSummary();
		}
		public virtual void APHistoryFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			APHistoryFilter row = (APHistoryFilter)e.Row;
			if (row != null)
			{
				Company company = this.Company.Current;
				bool mcFeatureInstalled = PXAccess.FeatureInstalled<FeaturesSet.multicurrency>();
				PXUIFieldAttribute.SetVisible<APHistoryFilter.showWithBalanceOnly>(cache, row, true);
				PXUIFieldAttribute.SetVisible<APHistoryFilter.curyID>(cache, row, mcFeatureInstalled);

				bool isCurySelected = string.IsNullOrEmpty(row.CuryID) == false;
				bool isBaseCurySelected = string.IsNullOrEmpty(row.CuryID) == false && (company.BaseCuryID == row.CuryID);
				bool splitByCurrency = (row.SplitByCurrency ?? false);

				PXUIFieldAttribute.SetVisible<APHistoryFilter.splitByCurrency>(cache, row, mcFeatureInstalled && !isCurySelected);
				PXUIFieldAttribute.SetEnabled<APHistoryFilter.splitByCurrency>(cache, row, mcFeatureInstalled && !isCurySelected);

				PXCache detailCache = this.History.Cache;
				bool hideCuryColumns = (!mcFeatureInstalled) || (isBaseCurySelected) || (!isCurySelected && !splitByCurrency);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyID>(this.History.Cache, null, mcFeatureInstalled && (isCurySelected || splitByCurrency));
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyBalance>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyPayments>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyPurchases>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyWhTax>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyDiscount>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyCrAdjustments>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyDrAdjustments>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyDeposits>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyDepositsBalance>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyBegBalance>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyEndBalance>(History.Cache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyBegRetainedBalance>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyEndRetainedBalance>(History.Cache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.rGOL>(History.Cache, null, mcFeatureInstalled);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyRetainageWithheld>(detailCache, null, !hideCuryColumns);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyRetainageReleased>(detailCache, null, !hideCuryColumns);

				PXUIFieldAttribute.SetVisible<APHistoryResult.balance>(detailCache, null, false);
				PXUIFieldAttribute.SetVisible<APHistoryResult.curyBalance>(detailCache, null, false);
				PXUIFieldAttribute.SetVisible<APHistoryResult.finPeriodID>(History.Cache, null);
				PXUIFieldAttribute.SetVisible<APHistoryResult.begBalance>(History.Cache, null);
				PXUIFieldAttribute.SetVisible<APHistoryResult.endBalance>(History.Cache, null);
				PXUIFieldAttribute.SetVisible<APHistoryResult.begRetainedBalance>(History.Cache, null);
				PXUIFieldAttribute.SetVisible<APHistoryResult.endRetainedBalance>(History.Cache, null);
			}
		}

	    public virtual void APHistorySummary_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
	    {
            var row = (APHistorySummary)e.Row;
	        if (row == null)
	        {
	            return;
	        }

            bool isForeignCurrency = string.IsNullOrEmpty(Filter.Current.CuryID) == false && (Company.Current.BaseCuryID != Filter.Current.CuryID);

            PXUIFieldAttribute.SetVisible<APHistorySummary.curyBalanceSummary>(cache, Summary.Current, isForeignCurrency);
            PXUIFieldAttribute.SetVisible<APHistorySummary.curyDepositsSummary>(cache, Summary.Current, isForeignCurrency);
			PXUIFieldAttribute.SetVisible<APHistorySummary.curyBalanceRetainedSummary>(cache, Summary.Current, isForeignCurrency);
		}
        #endregion

        protected virtual IEnumerable RetrieveHistoryForPeriod(APHistoryFilter header, APHistorySummary summary = null)
		{
			bool isCurySelected = string.IsNullOrEmpty(header.CuryID) == false;
			bool splitByCurrency = header.SplitByCurrency ?? false;
			bool useFinancial = (header.UseMasterCalendar != true);

			#region Specific Fiscal Period
			Type having = null;
			if (header.ShowWithBalanceOnly == true && summary == null)
			{
				if (useFinancial)
					having = typeof(Having<CuryAPHistoryTran.curyFinYtdBalance.Summarized.IsNotEqual<Zero>
						.Or<CuryAPHistoryTran.finYtdBalance.Summarized.IsNotEqual<Zero>>
						.Or<CuryAPHistoryTran.finPtdRevalued.Summarized.IsNotEqual<Zero>>
						.Or<CuryAPHistoryTran.curyFinYtdDeposits.Summarized.IsNotEqual<Zero>>
						.Or<CuryAPHistoryTran.curyFinYtdRetainedBalance.Summarized.IsNotEqual<Zero>>>);
				else
					having = typeof(Having<CuryAPHistoryTran.curyTranYtdBalance.Summarized.IsNotEqual<Zero>
						.Or<CuryAPHistoryTran.tranYtdBalance.Summarized.IsNotEqual<Zero>>
						.Or<CuryAPHistoryTran.finPtdRevalued.Summarized.IsNotEqual<Zero>>
						.Or<CuryAPHistoryTran.curyTranYtdDeposits.Summarized.IsNotEqual<Zero>>
						.Or<CuryAPHistoryTran.curyTranYtdRetainedBalance.Summarized.IsNotEqual<Zero>>>);
			}

			List<Type> typesList = new List<Type>
			{
				typeof(Select5<,,,,>), typeof(CA.Light.Vendor),
				typeof(InnerJoin<APHistoryByPeriod, On<APHistoryByPeriod.vendorID, Equal<CA.Light.Vendor.bAccountID>,
							And<Match<CA.Light.Vendor, Current<AccessInfo.userName>>>>,
						LeftJoin<CuryAPHistoryTran, 
							 On<APHistoryByPeriod.accountID, Equal<CuryAPHistoryTran.accountID>,
							And<APHistoryByPeriod.branchID, Equal<CuryAPHistoryTran.branchID>,
							And<APHistoryByPeriod.vendorID, Equal<CuryAPHistoryTran.vendorID>,
							And<APHistoryByPeriod.subID, Equal<CuryAPHistoryTran.subID>,
							And<APHistoryByPeriod.curyID, Equal<CuryAPHistoryTran.curyID>,
							And<APHistoryByPeriod.lastActivityPeriod, Equal<CuryAPHistoryTran.finPeriodID>>>>>>>>>),
				typeof(Where<APHistoryByPeriod.finPeriodID, Equal<Current<APHistoryFilter.finPeriodID>>>),
				having == null ? typeof(Aggregate<>) : typeof(Aggregate<,>)
			};

			typesList.AddRange(BqlHelper.GetDecimalFieldsAggregate<CuryAPHistoryTran>(this));



			Type groupBy = null;
			Type orderBy = null;
			if (summary != null)
			{
				groupBy = typeof(GroupBy<APHistoryByPeriod.curyID>);
				orderBy = typeof(OrderBy<Asc<APHistoryByPeriod.curyID>>);
			}
			else
			{
				if (splitByCurrency == false)
				{
					groupBy = typeof(GroupBy<CA.Light.Vendor.acctCD>);
					orderBy = typeof(OrderBy<Asc<CA.Light.Vendor.acctCD>>);
				}
				else
				{
					groupBy = typeof(GroupBy<CA.Light.Vendor.acctCD, GroupBy<APHistoryByPeriod.curyID>>);
					orderBy = typeof(OrderBy<Asc<CA.Light.Vendor.acctCD, Asc<APHistoryByPeriod.curyID>>>);
				}
			}
			typesList.Add(groupBy);
			if (having != null)
				typesList.Add(having);
			typesList.Add(orderBy);

			Type select = BqlCommand.Compose(typesList.ToArray());
			if (!SubCDUtils.IsSubCDEmpty(header.SubID))
			{
				select = BqlCommand.AppendJoin(select, BqlCommand.Compose(typeof(LeftJoin<Sub, On<APHistoryByPeriod.subID, Equal<Sub.subID>>>)));
			}
			BqlCommand cmd = BqlCommand.CreateInstance(select);
			PXView view = new PXView(this, false, cmd);

			if (isCurySelected)
			{
				view.WhereAnd<Where<APHistoryByPeriod.curyID, Equal<Current<APHistoryFilter.curyID>>>>();
			}

			if (header.OrgBAccountID != null)
			{
				view.WhereAnd<Where<APHistoryByPeriod.branchID, Inside<Current<APHistoryFilter.orgBAccountID>>>>(); //MatchWithOrg
			}

			if (header.AccountID != null)
			{
				view.WhereAnd<Where<APHistoryByPeriod.accountID, Equal<Current<APHistoryFilter.accountID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(header.SubID))
			{
				view.WhereAnd<Where<Sub.subCD, Like<Current<APHistoryFilter.subCDWildcard>>>>();
			}

			if (header.VendorClassID != null)
			{
				view.WhereAnd<Where<CA.Light.Vendor.vendorClassID, Equal<Current<APHistoryFilter.vendorClassID>>>>();
			}

			if (header.VendorID != null)
			{
				view.WhereAnd<Where<CA.Light.Vendor.bAccountID, Equal<Current<APHistoryFilter.vendorID>>>>();
			}

			if (header.Status != null)
			{
				view.WhereAnd<Where<CA.Light.Vendor.status, Equal<Current<APHistoryFilter.status>>>>();
			}

			int startRow = PXView.StartRow;
			int totalRows = 0;

			var mapper = new PXResultMapper(this, useFinancial ? mapFin : mapTran, typeof(APHistoryResult));
			mapper.ExtFilters.Add(typeof(CuryAPHistoryTran));			
			
			var records = summary == null
				? view.Select(null, null, PXView.Searches, mapper.SortColumns, PXView.Descendings, mapper.Filters,
					ref startRow, PXView.MaximumRows, ref totalRows)
				: view.SelectMulti();

			PXDelegateResult list = mapper.CreateDelegateResult(summary == null);

			foreach (PXResult<CA.Light.Vendor> record in records)
			{
				APHistoryResult res = mapper.CreateResult(record) as APHistoryResult;
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
			{typeof(APHistoryResult.acctCD), typeof(CA.Light.Vendor.acctCD)},
			{typeof(APHistoryResult.acctName), typeof(CA.Light.Vendor.acctName)},
			{typeof(APHistoryResult.vendorID), typeof(CA.Light.Vendor.bAccountID)},
			{typeof(APHistoryResult.noteID), typeof(CA.Light.Vendor.noteID)},

			{typeof(APHistoryResult.curyID), typeof(APHistoryByPeriod.curyID)},
			{typeof(APHistoryResult.finPeriodID), typeof(CuryAPHistoryTran.finPeriodID)},

			{typeof(APHistoryResult.curyBegBalance), typeof(CuryAPHistoryTran.curyFinBegBalance)},
			{typeof(APHistoryResult.curyEndBalance), typeof(CuryAPHistoryTran.curyFinYtdBalance)},
			{typeof(APHistoryResult.curyPurchases), typeof(CuryAPHistoryTran.curyFinPtdPurchases)},
			{typeof(APHistoryResult.curyPayments), typeof(CuryAPHistoryTran.curyFinPtdPayments)},
			{typeof(APHistoryResult.curyDiscount), typeof(CuryAPHistoryTran.curyFinPtdDiscTaken)},
			{typeof(APHistoryResult.curyWhTax), typeof(CuryAPHistoryTran.curyFinPtdWhTax)},
			{typeof(APHistoryResult.curyCrAdjustments), typeof(CuryAPHistoryTran.curyFinPtdCrAdjustments)},
			{typeof(APHistoryResult.curyDrAdjustments), typeof(CuryAPHistoryTran.curyFinPtdDrAdjustments)},
			{typeof(APHistoryResult.curyDeposits), typeof(CuryAPHistoryTran.curyFinPtdDeposits)},
	        //aDest.CuryDepositsBalance = -aHistory.CuryFinYtdDeposits ?? Decimal.Zero;
	        {typeof(APHistoryResult.curyDepositsBalance), typeof(CuryAPHistoryTran.curyFinYtdDeposits)},
			{typeof(APHistoryResult.curyRetainageWithheld), typeof(CuryAPHistoryTran.curyFinPtdRetainageWithheld)},
			{typeof(APHistoryResult.curyRetainageReleased), typeof(CuryAPHistoryTran.curyFinPtdRetainageReleased)},
	        //aDest.CuryBegRetainedBalance = (aHistory.CuryFinYtdRetainageWithheld ?? Decimal.Zero)
	        //- (aHistory.CuryFinYtdRetainageReleased ?? Decimal.Zero)
	        //-	((aHistory.CuryFinPtdRetainageWithheld ?? Decimal.Zero)
	        //- (aHistory.CuryFinPtdRetainageReleased ?? Decimal.Zero));
	        {typeof(APHistoryResult.curyBegRetainedBalance), typeof(CuryAPHistoryTran.curyFinBegRetainedBalance)},
	        //aDest.CuryEndRetainedBalance = (aHistory.CuryFinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinYtdRetainageReleased ?? Decimal.Zero);
	        {typeof(APHistoryResult.curyEndRetainedBalance), typeof(CuryAPHistoryTran.curyFinYtdRetainedBalance)},

			{typeof(APHistoryResult.begBalance), typeof(CuryAPHistoryTran.finBegBalance)},
			{typeof(APHistoryResult.endBalance), typeof(CuryAPHistoryTran.finYtdBalance)},
			{typeof(APHistoryResult.purchases), typeof(CuryAPHistoryTran.finPtdPurchases)},
			{typeof(APHistoryResult.payments), typeof(CuryAPHistoryTran.finPtdPayments)},
			{typeof(APHistoryResult.discount), typeof(CuryAPHistoryTran.finPtdDiscTaken)},
			{typeof(APHistoryResult.whTax), typeof(CuryAPHistoryTran.finPtdWhTax)},
			{typeof(APHistoryResult.rGOL), typeof(CuryAPHistoryTran.finPtdRGOL)},

			{typeof(APHistoryResult.crAdjustments), typeof(CuryAPHistoryTran.finPtdCrAdjustments)},
			{typeof(APHistoryResult.drAdjustments), typeof(CuryAPHistoryTran.finPtdDrAdjustments)},
			{typeof(APHistoryResult.deposits), typeof(CuryAPHistoryTran.finPtdDeposits)},
			//aDest.DepositsBalance = -aHistory.FinYtdDeposits ?? Decimal.Zero;
	        {typeof(APHistoryResult.depositsBalance), typeof(CuryAPHistoryTran.finYtdDeposits)},
			{typeof(APHistoryResult.retainageWithheld), typeof(CuryAPHistoryTran.finPtdRetainageWithheld)},
			{typeof(APHistoryResult.retainageReleased), typeof(CuryAPHistoryTran.finPtdRetainageReleased)},
	        //aDest.BegRetainedBalance = (aHistory.FinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.FinYtdRetainageReleased ?? Decimal.Zero) -
	        //	((aHistory.FinPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.FinPtdRetainageReleased ?? Decimal.Zero));
	        {typeof(APHistoryResult.begRetainedBalance), typeof(CuryAPHistoryTran.finBegRetainedBalance)},
	        //aDest.EndRetainedBalance = (aHistory.FinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.FinYtdRetainageReleased ?? Decimal.Zero);
	        {typeof(APHistoryResult.endRetainedBalance), typeof(CuryAPHistoryTran.finYtdRetainedBalance)}
		};
        protected readonly Dictionary<Type, Type> mapTran = new Dictionary<Type, Type>
		{
			{typeof(APHistoryResult.acctCD), typeof(CA.Light.Vendor.acctCD)},
			{typeof(APHistoryResult.acctName), typeof(CA.Light.Vendor.acctName)},
			{typeof(APHistoryResult.vendorID), typeof(CA.Light.Vendor.bAccountID)},
			{typeof(APHistoryResult.noteID), typeof(CA.Light.Vendor.noteID)},

			{typeof(APHistoryResult.curyID), typeof(APHistoryByPeriod.curyID)},
			{typeof(APHistoryResult.finPeriodID), typeof(CuryAPHistoryTran.finPeriodID)},

			{typeof(APHistoryResult.curyBegBalance), typeof(CuryAPHistoryTran.curyTranBegBalance)},
			{typeof(APHistoryResult.curyEndBalance), typeof(CuryAPHistoryTran.curyTranYtdBalance)},
			{typeof(APHistoryResult.curyPurchases), typeof(CuryAPHistoryTran.curyTranPtdPurchases)},
			{typeof(APHistoryResult.curyPayments), typeof(CuryAPHistoryTran.curyTranPtdPayments)},
			{typeof(APHistoryResult.curyDiscount), typeof(CuryAPHistoryTran.curyTranPtdDiscTaken)},
			{typeof(APHistoryResult.curyWhTax), typeof(CuryAPHistoryTran.curyTranPtdWhTax)},
			{typeof(APHistoryResult.curyCrAdjustments), typeof(CuryAPHistoryTran.curyTranPtdCrAdjustments)},
			{typeof(APHistoryResult.curyDrAdjustments), typeof(CuryAPHistoryTran.curyTranPtdDrAdjustments)},
			{typeof(APHistoryResult.curyDeposits), typeof(CuryAPHistoryTran.curyTranPtdDeposits)},
	        //aDest.CuryDepositsBalance = -aHistory.CuryTranYtdDeposits ?? Decimal.Zero;
	        {typeof(APHistoryResult.curyDepositsBalance), typeof(CuryAPHistoryTran.curyTranYtdDeposits)},
			{typeof(APHistoryResult.curyRetainageWithheld), typeof(CuryAPHistoryTran.curyTranPtdRetainageWithheld)},
			{typeof(APHistoryResult.curyRetainageReleased), typeof(CuryAPHistoryTran.curyTranPtdRetainageReleased)},
	        //aDest.CuryBegRetainedBalance = (aHistory.CuryTranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranYtdRetainageReleased ?? Decimal.Zero) -
	        //	((aHistory.CuryTranPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranPtdRetainageReleased ?? Decimal.Zero));
	        {typeof(APHistoryResult.curyBegRetainedBalance), typeof(CuryAPHistoryTran.curyTranBegRetainedBalance)},
	        //aDest.CuryEndRetainedBalance = (aHistory.CuryTranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranYtdRetainageReleased ?? Decimal.Zero);
	        {typeof(APHistoryResult.curyEndRetainedBalance), typeof(CuryAPHistoryTran.curyTranYtdRetainedBalance)},

			{typeof(APHistoryResult.begBalance), typeof(CuryAPHistoryTran.tranBegBalance)},
			{typeof(APHistoryResult.endBalance), typeof(CuryAPHistoryTran.tranYtdBalance)},
			{typeof(APHistoryResult.purchases), typeof(CuryAPHistoryTran.tranPtdPurchases)},
			{typeof(APHistoryResult.payments), typeof(CuryAPHistoryTran.tranPtdPayments)},
			{typeof(APHistoryResult.discount), typeof(CuryAPHistoryTran.tranPtdDiscTaken)},
			{typeof(APHistoryResult.whTax), typeof(CuryAPHistoryTran.tranPtdWhTax)},
			//aDest.RGOL = -aHistory.FinPtdRGOL ?? Decimal.Zero;
			{typeof(APHistoryResult.rGOL), typeof(CuryAPHistoryTran.tranPtdRGOL)},
			{typeof(APHistoryResult.crAdjustments), typeof(CuryAPHistoryTran.tranPtdCrAdjustments)},
			{typeof(APHistoryResult.drAdjustments), typeof(CuryAPHistoryTran.tranPtdDrAdjustments)},
			{typeof(APHistoryResult.deposits), typeof(CuryAPHistoryTran.tranPtdDeposits)},
	        //aDest.DepositsBalance = -aHistory.tranYtdDeposits ?? Decimal.Zero;
	        {typeof(APHistoryResult.depositsBalance), typeof(CuryAPHistoryTran.tranYtdDeposits)},
			{typeof(APHistoryResult.retainageWithheld), typeof(CuryAPHistoryTran.tranPtdRetainageWithheld)},
			{typeof(APHistoryResult.retainageReleased), typeof(CuryAPHistoryTran.tranPtdRetainageReleased)},
	        //aDest.BegRetainedBalance = (aHistory.tranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.tranYtdRetainageReleased ?? Decimal.Zero) -
	        //	((aHistory.tranPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.tranPtdRetainageReleased ?? Decimal.Zero));
	        {typeof(APHistoryResult.begRetainedBalance), typeof(CuryAPHistoryTran.tranBegRetainedBalance)},
	        //aDest.EndRetainedBalance = (aHistory.tranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.tranYtdRetainageReleased ?? Decimal.Zero);
	        {typeof(APHistoryResult.endRetainedBalance), typeof(CuryAPHistoryTran.tranYtdRetainedBalance)}
		};

		#region Utility Functions

		public virtual string GetLastActivityPeriod(int? aVendorID)
		{
			PXSelectBase<CuryAPHistory> activitySelect = new PXSelect<CuryAPHistory, Where<CuryAPHistory.vendorID, Equal<Required<CuryAPHistory.vendorID>>>, OrderBy<Desc<CuryAPHistory.finPeriodID>>>(this);
			CuryAPHistory result = (CuryAPHistory)activitySelect.View.SelectSingle(aVendorID);
			if (result != null)
				return result.FinPeriodID;
			return null;
		}
		protected virtual void CopyFrom(APHistoryResult aDest, CA.Light.Vendor aVendor)
		{
			aDest.AcctCD = aVendor.AcctCD;
			aDest.AcctName = aVendor.AcctName;
			aDest.CuryID = aVendor.CuryID;
			aDest.VendorID = aVendor.BAccountID;
			aDest.NoteID = aVendor.NoteID;
		}
		protected virtual void CopyFrom(APHistoryResult aDest, CuryAPHistoryTran aHistory, bool aIsFinancial)
		{
			if (aIsFinancial)
			{
				aDest.CuryBegBalance = aHistory.CuryFinBegBalance ?? Decimal.Zero;
				aDest.CuryPurchases = aHistory.CuryFinPtdPurchases ?? Decimal.Zero;
				aDest.CuryPayments = aHistory.CuryFinPtdPayments ?? Decimal.Zero;
				aDest.CuryDiscount = aHistory.CuryFinPtdDiscTaken ?? Decimal.Zero;
				aDest.CuryWhTax = aHistory.CuryFinPtdWhTax ?? Decimal.Zero;
				aDest.CuryCrAdjustments = aHistory.CuryFinPtdCrAdjustments ?? Decimal.Zero;
				aDest.CuryDrAdjustments = aHistory.CuryFinPtdDrAdjustments ?? Decimal.Zero;
				aDest.CuryDeposits = aHistory.CuryFinPtdDeposits ?? Decimal.Zero;
				aDest.CuryDepositsBalance = -aHistory.CuryFinYtdDeposits ?? Decimal.Zero;
				aDest.CuryRetainageWithheld = aHistory.CuryFinPtdRetainageWithheld ?? Decimal.Zero;
				aDest.CuryRetainageReleased = aHistory.CuryFinPtdRetainageReleased ?? Decimal.Zero;
				aDest.CuryBegRetainedBalance = (aHistory.CuryFinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinYtdRetainageReleased ?? Decimal.Zero) -
					((aHistory.CuryFinPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinPtdRetainageReleased ?? Decimal.Zero));
				aDest.CuryEndRetainedBalance = (aHistory.CuryFinYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryFinYtdRetainageReleased ?? Decimal.Zero);

				aDest.BegBalance = aHistory.FinBegBalance ?? Decimal.Zero;
				aDest.Purchases = aHistory.FinPtdPurchases ?? Decimal.Zero;
				aDest.Payments = aHistory.FinPtdPayments ?? Decimal.Zero;
				aDest.Discount = aHistory.FinPtdDiscTaken ?? Decimal.Zero;
				aDest.WhTax = aHistory.FinPtdWhTax ?? Decimal.Zero;
				aDest.RGOL = -aHistory.FinPtdRGOL ?? Decimal.Zero;
				aDest.CrAdjustments = aHistory.FinPtdCrAdjustments ?? Decimal.Zero;
				aDest.DrAdjustments = aHistory.FinPtdDrAdjustments ?? Decimal.Zero;
				aDest.Deposits = aHistory.FinPtdDeposits ?? Decimal.Zero;
				aDest.DepositsBalance = -aHistory.FinYtdDeposits ?? Decimal.Zero;
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
				aDest.CuryPurchases = aHistory.CuryTranPtdPurchases ?? Decimal.Zero;
				aDest.CuryPayments = aHistory.CuryTranPtdPayments ?? Decimal.Zero;
				aDest.CuryDiscount = aHistory.CuryTranPtdDiscTaken ?? Decimal.Zero;
				aDest.CuryWhTax = aHistory.CuryTranPtdWhTax ?? Decimal.Zero;
				aDest.CuryCrAdjustments = aHistory.CuryTranPtdCrAdjustments ?? Decimal.Zero;
				aDest.CuryDrAdjustments = aHistory.CuryTranPtdDrAdjustments ?? Decimal.Zero;
				aDest.CuryDeposits = aHistory.CuryTranPtdDeposits ?? Decimal.Zero;
				aDest.CuryDepositsBalance = -aHistory.CuryTranYtdDeposits ?? Decimal.Zero;
				aDest.CuryRetainageWithheld = aHistory.CuryTranPtdRetainageWithheld ?? Decimal.Zero;
				aDest.CuryRetainageReleased = aHistory.CuryTranPtdRetainageReleased ?? Decimal.Zero;
				aDest.CuryBegRetainedBalance = (aHistory.CuryTranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranYtdRetainageReleased ?? Decimal.Zero) -
					((aHistory.CuryTranPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranPtdRetainageReleased ?? Decimal.Zero));
				aDest.CuryEndRetainedBalance = (aHistory.CuryTranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.CuryTranYtdRetainageReleased ?? Decimal.Zero);

				aDest.BegBalance = aHistory.TranBegBalance ?? Decimal.Zero;
				aDest.Purchases = aHistory.TranPtdPurchases ?? Decimal.Zero;
				aDest.Payments = aHistory.TranPtdPayments ?? Decimal.Zero;
				aDest.Discount = aHistory.TranPtdDiscTaken ?? Decimal.Zero;
				aDest.WhTax = aHistory.TranPtdWhTax ?? Decimal.Zero;
				aDest.RGOL = -aHistory.TranPtdRGOL ?? Decimal.Zero;
				aDest.CrAdjustments = aHistory.TranPtdCrAdjustments ?? Decimal.Zero;
				aDest.DrAdjustments = aHistory.TranPtdDrAdjustments ?? Decimal.Zero;
				aDest.Deposits = aHistory.TranPtdDeposits ?? Decimal.Zero;
				aDest.DepositsBalance = -aHistory.TranYtdDeposits ?? Decimal.Zero;
				aDest.RetainageWithheld = aHistory.TranPtdRetainageWithheld ?? Decimal.Zero;
				aDest.RetainageReleased = aHistory.TranPtdRetainageReleased ?? Decimal.Zero;
				aDest.BegRetainedBalance = (aHistory.TranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.TranYtdRetainageReleased ?? Decimal.Zero) -
					((aHistory.TranPtdRetainageWithheld ?? Decimal.Zero) - (aHistory.TranPtdRetainageReleased ?? Decimal.Zero));
				aDest.EndRetainedBalance = (aHistory.TranYtdRetainageWithheld ?? Decimal.Zero) - (aHistory.TranYtdRetainageReleased ?? Decimal.Zero);
				aDest.CuryID = aHistory.CuryID;
			}
			aDest.RecalculateEndBalance();
		}

		protected virtual void Aggregate(APHistoryResult aDest, APHistoryResult aSrc)
		{
			aDest.CuryBegBalance += aSrc.CuryBegBalance ?? Decimal.Zero;
			aDest.CuryCrAdjustments += aSrc.CuryCrAdjustments ?? Decimal.Zero;
			aDest.CuryDrAdjustments += aSrc.CuryDrAdjustments ?? Decimal.Zero;
			aDest.CuryDiscount += aSrc.CuryDiscount ?? Decimal.Zero;
			aDest.CuryWhTax += aSrc.CuryWhTax ?? Decimal.Zero;
			aDest.CuryPurchases += aSrc.CuryPurchases ?? Decimal.Zero;
			aDest.CuryPayments += aSrc.CuryPayments ?? Decimal.Zero;
			aDest.CuryDeposits += aSrc.CuryDeposits ?? Decimal.Zero;
			aDest.CuryDepositsBalance += aSrc.CuryDepositsBalance ?? Decimal.Zero;
			aDest.CuryRetainageWithheld += aSrc.CuryRetainageWithheld ?? Decimal.Zero;
			aDest.CuryRetainageReleased += aSrc.CuryRetainageReleased ?? Decimal.Zero;
			aDest.CuryBegRetainedBalance += aSrc.CuryBegRetainedBalance ?? Decimal.Zero;

			aDest.BegBalance += aSrc.BegBalance ?? Decimal.Zero;
			aDest.CrAdjustments += aSrc.CrAdjustments ?? Decimal.Zero;
			aDest.DrAdjustments += aSrc.DrAdjustments ?? Decimal.Zero;
			aDest.Discount += aSrc.Discount ?? Decimal.Zero;
			aDest.WhTax += aSrc.WhTax ?? Decimal.Zero;
			aDest.Purchases += aSrc.Purchases ?? Decimal.Zero;
			aDest.Payments += aSrc.Payments ?? Decimal.Zero;
			aDest.RGOL += aSrc.RGOL ?? Decimal.Zero;
			aDest.Deposits += aSrc.Deposits ?? Decimal.Zero;
			aDest.DepositsBalance += aSrc.DepositsBalance ?? Decimal.Zero;
			aDest.RetainageWithheld += aSrc.RetainageWithheld ?? Decimal.Zero;
			aDest.RetainageReleased += aSrc.RetainageReleased ?? Decimal.Zero;
			aDest.BegRetainedBalance += aSrc.BegRetainedBalance ?? Decimal.Zero;
			aDest.RecalculateEndBalance();
		}

		protected virtual void Aggregate(APHistorySummary aDest, APHistoryResult aSrc)
		{
			aDest.CuryBalanceSummary += aSrc.CuryEndBalance ?? decimal.Zero;
			aDest.CuryDepositsSummary += aSrc.CuryDepositsBalance ?? decimal.Zero;
			aDest.BalanceSummary += aSrc.EndBalance ?? decimal.Zero;
			aDest.DepositsSummary += aSrc.DepositsBalance ?? decimal.Zero;
			aDest.BalanceRetainedSummary += aSrc.EndRetainedBalance ?? decimal.Zero;
			aDest.CuryBalanceRetainedSummary += aSrc.CuryEndRetainedBalance ?? decimal.Zero;
		}
		protected virtual void AggregateLatest(APHistoryResult aDest, APHistoryResult aSrc)
		{
			if (aSrc.FinPeriodID == aDest.FinPeriodID)
			{
				Aggregate(aDest, aSrc);
			}
			else
			{
				if (string.Compare(aSrc.FinPeriodID,aDest.FinPeriodID)<0)
				{
					//Just update Beg Balance
					aDest.BegBalance += aSrc.EndBalance?? Decimal.Zero;
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
					aDest.WhTax = aSrc.WhTax ?? Decimal.Zero;
					aDest.Purchases = aSrc.Purchases ?? Decimal.Zero;
					aDest.Payments = aSrc.Payments ?? Decimal.Zero;
					aDest.RGOL = aSrc.RGOL ?? Decimal.Zero;
					aDest.FinPeriodID = aSrc.FinPeriodID;
					aDest.Deposits = aSrc.Deposits;
					aDest.DepositsBalance = (aDest.DepositsBalance?? Decimal.Zero) + (aSrc.DepositsBalance??Decimal.Zero);
					aDest.RetainageWithheld = aSrc.RetainageWithheld ?? Decimal.Zero;
					aDest.RetainageReleased = aSrc.RetainageReleased ?? Decimal.Zero;
					aDest.BegRetainedBalance = (aDest.EndRetainedBalance ?? Decimal.Zero) + (aSrc.BegRetainedBalance ?? Decimal.Zero);

					aDest.CuryBegBalance = (aDest.CuryEndBalance ?? Decimal.Zero) + (aSrc.CuryBegBalance ?? Decimal.Zero);
					aDest.CuryCrAdjustments = aSrc.CuryCrAdjustments ?? Decimal.Zero;
					aDest.CuryDrAdjustments = aSrc.CuryDrAdjustments ?? Decimal.Zero;
					aDest.CuryDiscount = aSrc.CuryDiscount ?? Decimal.Zero;
					aDest.CuryWhTax = aSrc.CuryWhTax ?? Decimal.Zero;
					aDest.CuryPurchases = aSrc.CuryPurchases ?? Decimal.Zero;
					aDest.CuryPayments = aSrc.CuryPayments ?? Decimal.Zero;
					aDest.CuryDeposits = aSrc.CuryDeposits;
					aDest.CuryDepositsBalance = (aDest.CuryDepositsBalance ?? Decimal.Zero) + (aSrc.CuryDepositsBalance ?? Decimal.Zero);
					aDest.CuryRetainageWithheld = aSrc.CuryRetainageWithheld ?? Decimal.Zero;
					aDest.CuryRetainageReleased = aSrc.CuryRetainageReleased ?? Decimal.Zero;
					aDest.CuryBegRetainedBalance = (aDest.CuryEndRetainedBalance ?? Decimal.Zero) + (aSrc.CuryBegRetainedBalance ?? Decimal.Zero);
				}
				aDest.RecalculateEndBalance();
			}
		}

		protected virtual bool IsExcludedByZeroBalances(bool? showWithBalanceOnly, APHistoryResult historyResult)
		{
			return (showWithBalanceOnly ?? false)
				&& ((historyResult.EndBalance ?? decimal.Zero) == decimal.Zero)
				&& (historyResult.DepositsBalance ?? decimal.Zero) == decimal.Zero
				&& (historyResult.CuryEndBalance ?? decimal.Zero) == decimal.Zero
				&& (historyResult.CuryDepositsBalance ?? decimal.Zero) == decimal.Zero
				&& (historyResult.EndRetainedBalance ?? decimal.Zero) == decimal.Zero
				&& (historyResult.CuryEndRetainedBalance ?? decimal.Zero) == decimal.Zero;
		}
		public static void Copy(APDocumentEnq.APDocumentFilter filter, APHistoryFilter histFilter)
		{
			filter.OrganizationID = histFilter.OrganizationID;
			filter.BranchID = histFilter.BranchID;
			filter.OrgBAccountID = histFilter.OrgBAccountID;
			filter.FinPeriodID = histFilter.FinPeriodID;
			filter.AccountID = histFilter.AccountID;
			filter.SubCD = histFilter.SubID;
			filter.CuryID = histFilter.CuryID;
			filter.UseMasterCalendar = histFilter.UseMasterCalendar;
		}
		public static void Copy(APHistoryFilter histFilter, APDocumentEnq.APDocumentFilter filter)
		{
			histFilter.OrganizationID = filter.OrganizationID;
			histFilter.BranchID = filter.BranchID;
			histFilter.OrgBAccountID = filter.OrgBAccountID;
			histFilter.VendorID = filter.VendorID;
			histFilter.FinPeriodID = filter.FinPeriodID;
			histFilter.AccountID = filter.AccountID;
			histFilter.SubID = filter.SubCD;
			histFilter.CuryID = filter.CuryID;
			histFilter.UseMasterCalendar = filter.UseMasterCalendar;
		}

		#endregion
    }
}
