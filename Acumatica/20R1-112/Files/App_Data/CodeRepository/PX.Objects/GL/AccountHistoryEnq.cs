using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using PX.Common;

using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.CS;

namespace PX.Objects.GL
{
	#region Additional DAC Classes
	//Derived class, used for creation of table's alias
	[Serializable]
	[PXHidden]
	public partial class AH : GLHistory
	{
		#region LedgerID
		public new abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		#endregion
		#region BranchID
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
		#region AccountID
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		#endregion
		#region SubID
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		#endregion
		#region FinPeriod
		public new abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		#endregion
		#region FinYtdBalance
		public new abstract class finYtdBalance : PX.Data.BQL.BqlDecimal.Field<finYtdBalance> { }
		#endregion
		#region TranYtdBalance
		public new abstract class tranYtdBalance : PX.Data.IBqlField { }
		#endregion
		#region CuryFinYtdBalance
		public new abstract class curyFinYtdBalance : PX.Data.BQL.BqlDecimal.Field<curyFinYtdBalance> { }
		#endregion
		#region CuryTranYtdBalance
		public new abstract class curyTranYtdBalance : PX.Data.IBqlField { }
		#endregion
		#region CuryID
		public new abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		#endregion
	};
	//Resultset
	[PXCacheName(Messages.GLHistoryEnquiryResult)]
	[Serializable]
	public partial class GLHistoryEnquiryResult : PX.Data.IBqlTable, ISignedBalances
	{
		public GLHistoryEnquiryResult() { }
		#region LedgerID
		public abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		protected Int32? _LedgerID;
		[PXDBInt(IsKey = true)]
		public virtual Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}
		#endregion
		#region AccountID
		public abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		protected Int32? _AccountID;
		[PXDBInt]
		[PXDefault()]
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
		#region AccountCD
		public abstract class accountCD : PX.Data.BQL.BqlString.Field<accountCD> { }
		
		[PXUIField(DisplayName = "Account", Visibility = PXUIVisibility.Visible)]
		[PXDBString(10, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDimensionSelectorAttribute(AccountAttribute.DimensionName, typeof(Account.accountCD), typeof(accountCD),
			typeof(Account.accountCD), typeof(Account.accountClassID), typeof(Account.type), typeof(Account.description))]
		public virtual string AccountCD { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		protected Int32? _BranchID;
		[Branch(IsKey = true)]
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
		#region Type
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		protected string _Type;
		[PXDBString(1)]
		[PXDefault(AccountType.Income)] //For designer
		[AccountType.List]
		[PXUIField(DisplayName = "Type")]
		public virtual string Type
		{
			get
			{
				return this._Type;
			}
			set
			{
				this._Type = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region LastActivityPeriod
		public abstract class lastActivityPeriod : PX.Data.BQL.BqlString.Field<lastActivityPeriod> { }
		protected String _LastActivityPeriod;
		[FinPeriodID(IsKey = true)]
		[PXUIField(DisplayName = "Last Activity", Visible = false)]
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
		#region BegBalance
		public abstract class begBalance : PX.Data.BQL.BqlDecimal.Field<begBalance> { }
		protected Decimal? _BegBalance;
		[PXDBBaseCury(typeof(GLHistoryEnquiryResult.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beg. Balance")]
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
		#region PtdDebitTotal
		public abstract class ptdDebitTotal : PX.Data.BQL.BqlDecimal.Field<ptdDebitTotal> { }
		protected Decimal? _PtdDebitTotal;
		[PXDBBaseCury(typeof(GLHistoryEnquiryResult.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Total")]
		public virtual Decimal? PtdDebitTotal
		{
			get
			{
				return this._PtdDebitTotal;
			}
			set
			{
				this._PtdDebitTotal = value;
			}
		}
		#endregion
		#region PtdCreditTotal
		public abstract class ptdCreditTotal : PX.Data.BQL.BqlDecimal.Field<ptdCreditTotal> { }
		protected Decimal? _PtdCreditTotal;
		[PXDBBaseCury(typeof(GLHistoryEnquiryResult.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Total")]
		public virtual Decimal? PtdCreditTotal
		{
			get
			{
				return this._PtdCreditTotal;
			}
			set
			{
				this._PtdCreditTotal = value;
			}
		}
		#endregion
		#region EndBalance
		public abstract class endBalance : PX.Data.BQL.BqlDecimal.Field<endBalance> { }
		protected Decimal? _EndBalance;
		[PXDBBaseCury(typeof(GLHistoryEnquiryResult.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance")]

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
		#region SignBegBalance
		public abstract class signBegBalance : PX.Data.BQL.BqlDecimal.Field<signBegBalance> { }
		protected Decimal? _SignBegBalance;
		[PXDBBaseCury(typeof(GLHistoryEnquiryResult.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Beg. Balance")]
		public virtual Decimal? SignBegBalance
		{
			get
			{
				return this._SignBegBalance;
			}
			set
			{
				this._SignBegBalance = value;
			}
		}
		#endregion
		#region SignEndBalance
		public abstract class signEndBalance : PX.Data.BQL.BqlDecimal.Field<signEndBalance> { }
		protected Decimal? _SignEndBalance;
		[PXBaseCury(typeof(GLHistoryEnquiryResult.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Ending Balance")]

		public virtual Decimal? SignEndBalance
		{
			get
			{
				return this._SignEndBalance;
			}
			set
			{
				this._SignEndBalance = value;
			}
		}
		#endregion

		#region CuryID
		public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;
		[PXDBString(5, IsUnicode = true)]
		[PXUIField(DisplayName = "Currency ID")]
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
		[PXDBCury(typeof(GLHistoryEnquiryResult.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Beg. Balance")]
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
		#region CuryPtdDebitTotal
		public abstract class curyPtdDebitTotal : PX.Data.BQL.BqlDecimal.Field<curyPtdDebitTotal> { }
		protected Decimal? _CuryPtdDebitTotal;
		[PXDBCury(typeof(GLHistoryEnquiryResult.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Debit Total")]
		public virtual Decimal? CuryPtdDebitTotal
		{
			get
			{
				return this._CuryPtdDebitTotal;
			}
			set
			{
				this._CuryPtdDebitTotal = value;
			}
		}
		#endregion
		#region CuryPtdCreditTotal
		public abstract class curyPtdCreditTotal : PX.Data.BQL.BqlDecimal.Field<curyPtdCreditTotal> { }
		protected Decimal? _CuryPtdCreditTotal;
		[PXDBCury(typeof(GLHistoryEnquiryResult.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Credit Total")]
		public virtual Decimal? CuryPtdCreditTotal
		{
			get
			{
				return this._CuryPtdCreditTotal;
			}
			set
			{
				this._CuryPtdCreditTotal = value;
			}
		}
		#endregion
		#region CuryEndBalance
		public abstract class curyEndBalance : PX.Data.BQL.BqlDecimal.Field<curyEndBalance> { }
		protected Decimal? _CuryEndBalance;
		[PXDBCury(typeof(GLHistoryEnquiryResult.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Ending Balance")]

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
		#region SignCuryBegBalance
		public abstract class signCuryBegBalance : PX.Data.BQL.BqlDecimal.Field<signCuryBegBalance> { }
		protected Decimal? _SignCuryBegBalance;
		[PXDBCury(typeof(GLHistoryEnquiryResult.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Beg. Balance")]
		public virtual Decimal? SignCuryBegBalance
		{
			get
			{
				return this._SignCuryBegBalance;
			}
			set
			{
				this._SignCuryBegBalance = value;
			}
		}
		#endregion
		#region SignCuryEndBalance
		public abstract class signCuryEndBalance : PX.Data.BQL.BqlDecimal.Field<signCuryEndBalance> { }
		protected Decimal? _SignCuryEndBalance;
		[PXDBCury(typeof(GLHistoryEnquiryResult.curyID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Curr. Ending Balance")]

		public virtual Decimal? SignCuryEndBalance
		{
			get
			{
				return this._SignCuryEndBalance;
			}
			set
			{
				this._SignCuryEndBalance = value;
			}
		}
		#endregion
		#region CalculatedFields
		#region PtdSaldo
		public abstract class ptdSaldo : PX.Data.BQL.BqlDecimal.Field<ptdSaldo> { }
		[PXDBCury(typeof(GLHistoryEnquiryResult.curyID))]
		[PXUIField(DisplayName = "Ptd. Total", Visible = false)]
		public Decimal? PtdSaldo
		{
			[PXDependsOnFields(typeof(type), typeof(ptdDebitTotal), typeof(ptdCreditTotal))]
			get { return AccountRules.CalcSaldo(this._Type, this._PtdDebitTotal ?? 0m, this._PtdCreditTotal ?? 0m); }
		}
		#endregion
		#region CuryPtdSaldo
		public abstract class curyPtdSaldo : PX.Data.BQL.BqlDecimal.Field<curyPtdSaldo> { }
		[PXDBCury(typeof(GLHistoryEnquiryResult.curyID))]
		[PXUIField(DisplayName = "Cury. Ptd. Total", Visible = false)]
		public decimal? CuryPtdSaldo
		{
			[PXDependsOnFields(typeof(type), typeof(curyPtdDebitTotal), typeof(curyPtdCreditTotal))]
			get { return AccountRules.CalcSaldo(this._Type, this._CuryPtdDebitTotal ?? 0m, this._CuryPtdCreditTotal ?? 0m); }
		}
		#endregion
		public virtual void recalculate(bool reversive)
		{
			if (reversive)
			{
				this._BegBalance = this._EndBalance - this.PtdSaldo;
				this._CuryBegBalance = this._CuryEndBalance - this.CuryPtdSaldo;
			}
			else
			{
				this._EndBalance = this._BegBalance + this.PtdSaldo;
				this._CuryEndBalance = this._CuryBegBalance + this.CuryPtdSaldo;
			}
		}

		public static void recalculateSignAmount(ISignedBalances item, bool uplaySignReverse)
		{
			if (uplaySignReverse && (item.Type == AccountType.Income || item.Type == AccountType.Liability))
			{
				item.SignBegBalance = -item.BegBalance;
				item.SignEndBalance = -item.EndBalance;
				item.SignCuryBegBalance = -item.CuryBegBalance;
				item.SignCuryEndBalance = -item.CuryEndBalance;
			}
			else
			{
				item.SignBegBalance = item.BegBalance;
				item.SignEndBalance = item.EndBalance;
				item.SignCuryBegBalance = item.CuryBegBalance;
				item.SignCuryEndBalance = item.CuryEndBalance;
			}
		}

		public virtual void recalculateSignAmount(bool uplaySignReverse)
		{
			recalculateSignAmount(this, uplaySignReverse);
		}
		#endregion
		#region ConsolAccountID
		public abstract class consolAccountCD : PX.Data.BQL.BqlString.Field<consolAccountCD> { }

		protected String _ConsolAccountCD;
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Consolidation Account", Visibility = PXUIVisibility.Visible)]
		[PXSelector(typeof(GLConsolAccount.accountCD), DescriptionField = typeof(GLConsolAccount.description))]
		public virtual String ConsolAccountCD
		{
			get
			{
				return this._ConsolAccountCD;
			}
			set
			{
				this._ConsolAccountCD = value;
			}
		}
		#endregion
		#region AccountClassID
		public abstract class accountClassID : PX.Data.BQL.BqlString.Field<accountClassID> { }
		protected string _AccountClassID;
		[PXDBString(20, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Class")]
		[PXSelector(typeof(AccountClass.accountClassID), DescriptionField = typeof(AccountClass.descr))]
		public virtual string AccountClassID
		{
			get
			{
				return this._AccountClassID;
			}
			set
			{
				this._AccountClassID = value;
			}
		}
		#endregion
		#region SubCD
		public abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }
		protected String _SubCD;
		[PXDBString(30, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Subaccount")]
		[PXDimension("SUBACCOUNT")]
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
	}
	#endregion
	public interface ISignedBalances
	{
		string Type { get; }
		decimal? BegBalance { get; }
		decimal? EndBalance { get; }
		decimal? CuryBegBalance { get; }
		decimal? CuryEndBalance { get; }
		decimal? SignBegBalance { get; set; }
		decimal? SignEndBalance { get; set; }
		decimal? SignCuryBegBalance { get; set; }
		decimal? SignCuryEndBalance { get; set; }
	}

	[PX.Objects.GL.TableAndChartDashboardType]
	public class AccountHistoryEnq : PXGraph<AccountHistoryEnq>
	{
		public PXCancel<GLHistoryEnqFilter> Cancel;
		public PXAction<GLHistoryEnqFilter> PreviousPeriod;
		public PXAction<GLHistoryEnqFilter> NextPeriod;
		public PXFilter<GLHistoryEnqFilter> Filter;
		public PXAction<GLHistoryEnqFilter> accountDetails;
		public PXAction<GLHistoryEnqFilter> accountBySub;
		public PXAction<GLHistoryEnqFilter> accountByPeriod;
		[PXFilterable]
		public PXSelectOrderBy<GLHistoryEnquiryResult, OrderBy<Asc<GLHistoryEnquiryResult.accountCD>>> EnqResult;
		public PXSetup<GLSetup> glsetup;

		private GLHistoryEnqFilter CurrentFilter
		{
			get { return this.Filter.Current as GLHistoryEnqFilter; }
		}

		public AccountHistoryEnq()
		{
			GLSetup setup = glsetup.Current;
			//SWUIFieldAttribute.SetEnabled(EnqResult.Cache, null, false);
			EnqResult.Cache.AllowInsert = false;
			EnqResult.Cache.AllowDelete = false;
			EnqResult.Cache.AllowUpdate = false;
		}

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }


		#region Buttons
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable previousperiod(PXAdapter adapter)
		{
			GLHistoryEnqFilter filter = Filter.Current as GLHistoryEnqFilter;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod prevPeriod = FinPeriodRepository.FindPrevPeriod(calendarOrganizationID, filter.FinPeriodID, looped: true);

			filter.FinPeriodID = prevPeriod != null ? prevPeriod.FinPeriodID : null;

			return adapter.Get();
		}

		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable nextperiod(PXAdapter adapter)
		{
			GLHistoryEnqFilter filter = Filter.Current as GLHistoryEnqFilter;

			int? calendarOrganizationID = FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID, filter.BranchID, filter.UseMasterCalendar);

			FinPeriod nextPeriod = FinPeriodRepository.FindNextPeriod(calendarOrganizationID, filter.FinPeriodID, looped: true);

			filter.FinPeriodID = nextPeriod != null ? nextPeriod.FinPeriodID : null;
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewAccountDetails)]
		[PXButton]
		protected virtual IEnumerable AccountDetails(PXAdapter adapter)
		{
			if (this.EnqResult.Current != null)
			{
				if (this.EnqResult.Current.AccountID == this.glsetup.Current.YtdNetIncAccountID)
					throw new PXException(Messages.DetailsReportIsNotAllowedForYTDNetIncome);

				AccountByPeriodEnq graph = CreateInstance<AccountByPeriodEnq>();
				AccountByPeriodFilter filter = PXCache<AccountByPeriodFilter>.CreateCopy(graph.Filter.Current);
				filter.OrgBAccountID = Filter.Current.OrgBAccountID;
				filter.OrganizationID = Filter.Current.OrganizationID;
				filter.BranchID = EnqResult.Current.BranchID;
				filter.LedgerID = Filter.Current.LedgerID;
				filter.AccountID = EnqResult.Current.AccountID;
				filter.SubID = Filter.Current.SubCD;
				filter.StartPeriodID = EnqResult.Current.LastActivityPeriod;
				filter.EndPeriodID = filter.StartPeriodID;
				filter.ShowCuryDetail = Filter.Current.ShowCuryDetail;
				graph.Filter.Update(filter);
				graph.Filter.Select(); // to calculate totals
				throw new PXRedirectRequiredException(graph, "Account Details");
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewAccountBySub)]
		[PXButton]
		protected virtual IEnumerable AccountBySub(PXAdapter adapter)
		{
			if (this.EnqResult.Current != null)
			{
				AccountHistoryBySubEnq graph = PXGraph.CreateInstance<AccountHistoryBySubEnq>();
				GLHistoryEnqFilter filter = PXCache<GLHistoryEnqFilter>.CreateCopy(graph.Filter.Current);
				filter.OrgBAccountID = Filter.Current.OrgBAccountID;
				filter.OrganizationID = Filter.Current.OrganizationID;
				filter.BranchID = EnqResult.Current.BranchID;
				filter.LedgerID = Filter.Current.LedgerID;
				filter.AccountID = EnqResult.Current.AccountID;
				filter.SubCD = Filter.Current.SubCD;
				filter.FinPeriodID = EnqResult.Current.LastActivityPeriod;
				filter.ShowCuryDetail = Filter.Current.ShowCuryDetail;
				graph.Filter.Update(filter);
				throw new PXRedirectRequiredException(graph, "Account by Subaccount");
			}
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewAccountByPeriod)]
		[PXButton]
		protected virtual IEnumerable AccountByPeriod(PXAdapter adapter)
		{
			if (this.EnqResult.Current != null)
			{
				AccountHistoryByYearEnq graph = PXGraph.CreateInstance<AccountHistoryByYearEnq>();
				AccountByYearFilter filter = PXCache<AccountByYearFilter>.CreateCopy(graph.Filter.Current);
				filter.OrgBAccountID = Filter.Current.OrgBAccountID;
				filter.OrganizationID = Filter.Current.OrganizationID;
				filter.BranchID = EnqResult.Current.BranchID;
				filter.LedgerID = Filter.Current.LedgerID;
				filter.AccountID = EnqResult.Current.AccountID;
				filter.SubCD = Filter.Current.SubCD;
				filter.FinPeriodID = EnqResult.Current.LastActivityPeriod;

				OrganizationFinPeriod fp = PXSelect<
					OrganizationFinPeriod, 
					Where<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>,
					And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>
					.Select(this, filter.FinPeriodID, PXAccess.GetParentOrganizationID(filter.BranchID));
				if (fp != null)
					filter.FinYear = fp.FinYear;

				filter.ShowCuryDetail = this.Filter.Current.ShowCuryDetail;
				graph.Filter.Update(filter);
				throw new PXRedirectRequiredException(graph, "Account By Period");
			}
			return adapter.Get();
		}
		#endregion

		protected virtual IEnumerable enqResult()
		{
			GLHistoryEnqFilter filter = (GLHistoryEnqFilter)this.Filter.Current;
			bool showCurrency = filter.ShowCuryDetail.HasValue && filter.ShowCuryDetail.Value;
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyID>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyPtdCreditTotal>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyPtdDebitTotal>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyBegBalance>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyEndBalance>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.signCuryBegBalance>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.signCuryEndBalance>(EnqResult.Cache, null, showCurrency);
			if (filter.LedgerID == null || filter.FinPeriodID == null)
			{
				yield break; //Prevent code from accessing database;
			}
			#region cmd
			PXSelectBase<GLHistoryByPeriod> cmd = new PXSelectJoinGroupBy<GLHistoryByPeriod,
								InnerJoin<Account,
										On<GLHistoryByPeriod.accountID, Equal<Account.accountID>, And<Match<Account, Current<AccessInfo.userName>>>>,
								InnerJoin<Sub,
										On<GLHistoryByPeriod.subID, Equal<Sub.subID>, And<Match<Sub, Current<AccessInfo.userName>>>>,
								LeftJoin<GLHistory, On<GLHistoryByPeriod.accountID, Equal<GLHistory.accountID>,
										And<GLHistoryByPeriod.ledgerID, Equal<GLHistory.ledgerID>,
										And<GLHistoryByPeriod.branchID, Equal<GLHistory.branchID>,
										And<GLHistoryByPeriod.subID, Equal<GLHistory.subID>,
										And<GLHistoryByPeriod.finPeriodID, Equal<GLHistory.finPeriodID>>>>>>,
								LeftJoin<AH, On<GLHistoryByPeriod.ledgerID, Equal<AH.ledgerID>,
										And<GLHistoryByPeriod.branchID, Equal<AH.branchID>,
										And<GLHistoryByPeriod.accountID, Equal<AH.accountID>,
										And<GLHistoryByPeriod.subID, Equal<AH.subID>,
										And<GLHistoryByPeriod.lastActivityPeriod, Equal<AH.finPeriodID>>>>>>>>>>,
								Where<GLHistoryByPeriod.ledgerID, Equal<Current<GLHistoryEnqFilter.ledgerID>>,
										And<GLHistoryByPeriod.finPeriodID, Equal<Current<GLHistoryEnqFilter.finPeriodID>>,
										And<
											Where2<
												Where<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>, And<Where<Account.type, Equal<AccountType.asset>,
													Or<Account.type, Equal<AccountType.liability>>>>>,
											Or<Where<GLHistoryByPeriod.lastActivityPeriod, GreaterEqual<Required<GLHistoryByPeriod.lastActivityPeriod>>,
												And<Where<Account.type, Equal<AccountType.expense>,
												Or<Account.type, Equal<AccountType.income>,
												Or<Account.accountID, Equal<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>>>>,
								Aggregate<
										Sum<AH.finYtdBalance,
										Sum<AH.tranYtdBalance,
										Sum<AH.curyFinYtdBalance,
										Sum<AH.curyTranYtdBalance,
										Sum<GLHistory.finPtdDebit,
										Sum<GLHistory.tranPtdDebit,
										Sum<GLHistory.finPtdCredit,
										Sum<GLHistory.tranPtdCredit,
										Sum<GLHistory.finBegBalance,
										Sum<GLHistory.tranBegBalance,
										Sum<GLHistory.finYtdBalance,
										Sum<GLHistory.tranYtdBalance,
										Sum<GLHistory.curyFinBegBalance,
										Sum<GLHistory.curyTranBegBalance,
										Sum<GLHistory.curyFinYtdBalance,
										Sum<GLHistory.curyTranYtdBalance,
										Sum<GLHistory.curyFinPtdCredit,
										Sum<GLHistory.curyTranPtdCredit,
										Sum<GLHistory.curyFinPtdDebit,
										Sum<GLHistory.curyTranPtdDebit,
										GroupBy<GLHistoryByPeriod.branchID,
										GroupBy<GLHistoryByPeriod.ledgerID,
										GroupBy<GLHistoryByPeriod.accountID,
										GroupBy<GLHistoryByPeriod.finPeriodID
								 >>>>>>>>>>>>>>>>>>>>>>>>>>(this);

			if (filter.LedgerID != null)
			{
				Ledger ledger = (Ledger)PXSelectorAttribute.Select<GLHistoryEnqFilter.ledgerID>(Filter.Cache, filter);
				if (ledger?.BalanceType == LedgerBalanceType.Budget)
				{
					// we shouldn't select history from the previous years for the budget ledgers
					cmd.WhereAnd<Where<Substring<GLHistoryByPeriod.finPeriodID, int1, int4>, Equal<Substring<GLHistoryByPeriod.lastActivityPeriod, int1, int4>>>>();
				}
			}
			if (filter.AccountID != null)
			{
				cmd.WhereAnd<Where<GLHistoryByPeriod.accountID, Equal<Current<GLHistoryEnqFilter.accountID>>>>();
			}
			if (filter.AccountClassID != null)
			{
				cmd.WhereAnd<Where<Account.accountClassID, Equal<Current<GLHistoryEnqFilter.accountClassID>>>>();
			}

			if (filter.SubID != null)
			{
				cmd.WhereAnd<Where<GLHistoryByPeriod.subID, Equal<Current<GLHistoryEnqFilter.subID>>>>();
			}

			int[] branchIDs = null;

			if (filter.BranchID != null)
			{
				cmd.WhereAnd<Where<GLHistoryByPeriod.branchID, Equal<Current<GLHistoryEnqFilter.branchID>>>>();
			}
			else if (filter.OrganizationID != null)
			{
				branchIDs = PXAccess.GetChildBranchIDs(filter.OrganizationID, false);

				cmd.WhereAnd<Where<GLHistoryByPeriod.branchID, In<Required<GLHistoryEnqFilter.branchID>>,
				  And<MatchWithBranch<GLHistoryByPeriod.branchID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(filter.SubCD))
			{
				cmd.WhereAnd<Where<Sub.subCD, Like<Current<GLHistoryEnqFilter.subCDWildcard>>>>();
			}
			//cmd.WhereAnd<Where<Match<Current<AccessInfo.userName>>>>();
			#endregion
			string yearBegPeriod = filter.BegFinPeriod;
			GLSetup glSetup = glsetup.Current;
			bool reverseSign = (glSetup != null) && (glSetup.TrialBalanceSign == GLSetup.trialBalanceSign.Reversed);
			foreach (PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, AH> it in cmd.Select(yearBegPeriod, branchIDs))
			{
				GLHistoryByPeriod baseview = (GLHistoryByPeriod)it;
				Account acct = (Account)it;
				GLHistory ah = (GLHistory)it;
				AH ah1 = (AH)it;
				ah.FinFlag = filter.UseMasterCalendar != true;
				ah1.FinFlag = filter.UseMasterCalendar != true;

				if (reverseSign && acct.AccountID == glSetup.YtdNetIncAccountID) continue;

				GLHistoryEnquiryResult item = new GLHistoryEnquiryResult();				
				item.BranchID = baseview.BranchID;
				item.LedgerID = baseview.LedgerID;
				item.AccountID = baseview.AccountID;
				item.AccountCD = acct.AccountCD;
				item.Type = acct.Type;
				item.Description = acct.Description;
				item.LastActivityPeriod = baseview.LastActivityPeriod;
				item.PtdCreditTotal = ah.PtdCredit ?? 0m;
				item.PtdDebitTotal = ah.PtdDebit ?? 0m;
				item.EndBalance = ah1.YtdBalance ?? 0m;
				item.ConsolAccountCD = acct.GLConsolAccountCD;
				item.AccountClassID = acct.AccountClassID;
				if (!(string.IsNullOrEmpty(ah.CuryID) && string.IsNullOrEmpty(ah1.CuryID)))
				{
					item.CuryEndBalance = ah1.CuryYtdBalance ?? 0m;
					item.CuryPtdCreditTotal = ah.CuryPtdCredit ?? 0m;
					item.CuryPtdDebitTotal = ah.CuryPtdDebit ?? 0m;
					item.CuryID = string.IsNullOrEmpty(ah.CuryID) ? ah1.CuryID : ah.CuryID;
				}
				else
				{
					item.CuryEndBalance = null;
					item.CuryPtdCreditTotal = null;
					item.CuryPtdDebitTotal = null;
					item.CuryID = null;
				}

				item.recalculate(true); // End balance is considered as correct digit - so we need to calculate begBalance base on ending one
				item.recalculateSignAmount(reverseSign);
				yield return item;
			}
		}
		public override bool IsDirty
		{
			get
			{
				return false;
			}
		}

		#region Actions

		public PXAction<GLHistoryEnqFilter> viewDetails;
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXEditDetailButton]
		public virtual IEnumerable ViewDetails(PXAdapter adapter)
		{
			if (this.EnqResult.Current != null && this.Filter.Current != null)
			{
				GLHistoryEnquiryResult res = this.EnqResult.Current;
				GLHistoryEnqFilter currentFilter = this.Filter.Current;

				AccountByPeriodEnq graph = PXGraph.CreateInstance<AccountByPeriodEnq>();
				AccountByPeriodFilter filter = graph.Filter.Current;
				Copy(filter, currentFilter);

				filter.AccountID = res.AccountID;
				graph.Filter.Update(filter);
				filter = graph.Filter.Select();
				throw new PXRedirectRequiredException(graph, "Account Details");
			}
			return Filter.Select();
		}

		public static void Copy(AccountByPeriodFilter filter, GLHistoryEnqFilter histFilter)
		{
			filter.OrganizationID = histFilter.OrganizationID;
			filter.BranchID = histFilter.BranchID;
			filter.StartPeriodID = histFilter.FinPeriodID;
			filter.EndPeriodID = histFilter.FinPeriodID;
			filter.LedgerID = histFilter.LedgerID;
			filter.AccountID = histFilter.AccountID;
			filter.SubID = histFilter.SubCD;
			filter.StartPeriodID = histFilter.BegFinPeriod;
			filter.ShowCuryDetail = histFilter.ShowCuryDetail;
			filter.UseMasterCalendar = histFilter.UseMasterCalendar;
		}

		#endregion

		#region Events
		protected virtual void GLHistoryEnqFilter_SubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void GLHistoryEnqFilter_AccountClassID_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}

		protected virtual void GLHistoryEnqFilter_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<GLHistoryEnqFilter.ledgerID>(e.Row);
		}

		protected virtual void GLHistoryEnqFilter_OrganizationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<GLHistoryEnqFilter.ledgerID>(e.Row);
		}

		protected virtual void GLHistoryEnqFilter_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			EnqResult.Select();
		}
		#endregion
	}

	public class SubCDUtils
	{
		public static bool IsSubCDEmpty(string aSub)
		{
			if (aSub == null || aSub == "") return true;
			return new Regex("^[_\\?]*$").IsMatch(aSub);
		}

		public static string CreateSubCDWildcard(string aSub, string dimensionID)
		{
			ISqlDialect sql = PXDatabase.Provider.SqlDialect;
			if (IsSubCDEmpty(aSub)) return sql.WildcardAnything;
			if (aSub[aSub.Length - 1] != '?'
				&& aSub[aSub.Length - 1] != ' '
				&& aSub.Length == PXDimensionAttribute.GetLength(dimensionID))
			{
				aSub = aSub + "?";
			}
			return Regex.Replace(Regex.Replace(aSub, "[ \\?]+$", sql.WildcardAnything), "[ \\?]", sql.WildcardAnySingle);
		}
		public const string CS_SUB_CD_FILL_CHAR = " ";
		public const string CS_SUB_CD_WILDCARD = "?";
	}

}
