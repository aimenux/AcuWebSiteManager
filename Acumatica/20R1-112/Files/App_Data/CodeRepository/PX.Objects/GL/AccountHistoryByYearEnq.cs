using System;
using PX.Data;
using System.Collections;
using System.Linq;
using PX.Objects.GL.Descriptor;
using PX.Objects.GL.FinPeriods;
using PX.Objects.Common.Tools;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL
{
	[Serializable]
	public partial class AccountByYearFilter : GLHistoryEnqFilter
	{
		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		public new abstract class ledgerID : PX.Data.BQL.BqlInt.Field<ledgerID> { }
		public new abstract class accountID : PX.Data.BQL.BqlInt.Field<accountID> { }
		public new abstract class subID : PX.Data.BQL.BqlInt.Field<subID> { }
		public new abstract class subCD : PX.Data.BQL.BqlString.Field<subCD> { }
		public new abstract class subCDWildcard : PX.Data.BQL.BqlString.Field<subCDWildcard> { }
		public new abstract class begFinPeriod : PX.Data.BQL.BqlString.Field<begFinPeriod> { }
		public new abstract class showCuryDetail : PX.Data.BQL.BqlBool.Field<showCuryDetail> { }
		public new abstract class useMasterCalendar : PX.Data.BQL.BqlBool.Field<useMasterCalendar> { }

		public override string BegFinPeriod => _FinYear != null ? FirstPeriodOfYear(_FinYear) : null;

		#region FinYear
		public abstract class finYear : PX.Data.BQL.BqlString.Field<finYear> { }
		protected string _FinYear;
		[PXDBString(4)]
		[PXDefault]
		[PXUIField(DisplayName = "Financial Year", Visibility = PXUIVisibility.Visible)]
		[GenericFinYearSelector(null,
			typeof(AccessInfo.businessDate),
			branchSourceType: typeof(AccountByYearFilter.branchID),
			organizationSourceType: typeof(AccountByYearFilter.organizationID),
			useMasterCalendarSourceType: typeof(AccountByYearFilter.useMasterCalendar))]
		public virtual string FinYear
		{
			get { return _FinYear; }
			set { _FinYear = value; }
		}
		#endregion
	}

	[TableAndChartDashboardType]
	public class AccountHistoryByYearEnq : PXGraph<AccountHistoryByYearEnq>
	{
		#region Type Override events
		[AccountAny]
		protected virtual void GLHistoryEnquiryResult_AccountID_CacheAttached(PXCache sender)
		{
		}

		[FinPeriodID(IsKey = true)]
		[PXUIField(DisplayName = "Period", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 0)]
		protected virtual void GLHistoryEnquiryResult_LastActivityPeriod_CacheAttached(PXCache sender)
		{
		}

		#endregion

		public PXCancel<AccountByYearFilter> Cancel;
		public PXAction<AccountByYearFilter> PreviousPeriod;
		public PXAction<AccountByYearFilter> NextPeriod;
		public PXFilter<AccountByYearFilter> Filter;
		public PXAction<AccountByYearFilter> accountDetails;
		public PXAction<AccountByYearFilter> accountBySub;
		[PXFilterable]
		public PXSelectOrderBy<GLHistoryEnquiryResult, OrderBy<Asc<GLHistoryEnquiryResult.lastActivityPeriod>>> EnqResult;
		public PXSetup<GLSetup> glsetup;
		public PXSelect<Account, Where<Account.accountID, Equal<Current<AccountByYearFilter.accountID>>>> AccountInfo;

		[InjectDependency]
		public IFinPeriodRepository FinPeriodRepository { get; set; }

		public FinYear fiscalyear
		{
			get
			{
				int? calendarOrganizationID =
					FinPeriodRepository.GetCalendarOrganizationID(Filter.Current.OrganizationID,
																	Filter.Current.BranchID,
																	Filter.Current.UseMasterCalendar);

				return PXSelect<
					FinYear,
					Where<FinYear.year, Equal<Required<FinYear.year>>,
						And<FinYear.organizationID, Equal<Required<FinYear.organizationID>>>>>
					.Select(this, Filter.Current.FinYear, calendarOrganizationID);
			}
		}

		private AccountByYearFilter CurrentFilter => Filter.Current;

		public AccountHistoryByYearEnq()
		{
			GLSetup setup = glsetup.Current;
			EnqResult.Cache.AllowInsert = false;
			EnqResult.Cache.AllowDelete = false;
			EnqResult.Cache.AllowUpdate = false;

		}
		#region Button Delegates
		[PXUIField(DisplayName = ActionsMessages.Previous, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXPreviousButton]
		public virtual IEnumerable previousperiod(PXAdapter adapter)
		{
			AccountByYearFilter filter = CurrentFilter;
			int? calendarOrganizationID =
					FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID,
																	filter.BranchID,
																	filter.UseMasterCalendar);
			FinYear nextperiod = PXSelect<
				FinYear,
				Where<FinYear.year, Less<Required<AccountByYearFilter.finYear>>,
					And<FinYear.organizationID, Equal<Required<AccountByYearFilter.organizationID>>>>,
				OrderBy<
					Desc<FinYear.year>>>
				.Select(this, filter.FinYear, calendarOrganizationID)
			?? PXSelect<
				FinYear,
				Where<FinYear.organizationID, Equal<Required<AccountByYearFilter.organizationID>>>,
				OrderBy<
					Desc<FinYear.year>>>
				.Select(this, calendarOrganizationID);

			filter.FinYear = nextperiod?.Year;
			return adapter.Get();
		}

		[PXUIField(DisplayName = ActionsMessages.Next, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXNextButton]
		public virtual IEnumerable nextperiod(PXAdapter adapter)
		{
			AccountByYearFilter filter = CurrentFilter;
			int? calendarOrganizationID =
					FinPeriodRepository.GetCalendarOrganizationID(filter.OrganizationID,
																	filter.BranchID,
																	filter.UseMasterCalendar);
			FinYear nextperiod = PXSelect<
				FinYear,
				Where<FinYear.year, Greater<Required<AccountByYearFilter.finYear>>,
					And<FinYear.organizationID, Equal<Required<AccountByYearFilter.organizationID>>>>,
				OrderBy<
					Asc<FinYear.year>>>
				.Select(this, filter.FinYear, calendarOrganizationID)
			?? PXSelect<
				FinYear,
				Where<FinYear.organizationID, Equal<Required<AccountByYearFilter.organizationID>>>,
				OrderBy<
					Asc<FinYear.year>>>
				.Select(this, calendarOrganizationID);

			filter.FinYear = nextperiod?.Year;
			return adapter.Get();
		}

		[PXUIField(DisplayName = Messages.ViewAccountDetails)]
		[PXButton]
		protected virtual IEnumerable AccountDetails(PXAdapter adapter)
		{
			if (EnqResult.Current != null)
			{
				if (EnqResult.Current.AccountID == glsetup.Current.YtdNetIncAccountID)
					throw new PXException(Messages.DetailsReportIsNotAllowedForYTDNetIncome);
				AccountByPeriodEnq graph = CreateInstance<AccountByPeriodEnq>();
				AccountByPeriodFilter filter = PXCache<AccountByPeriodFilter>.CreateCopy(graph.Filter.Current);
				filter.OrgBAccountID = Filter.Current.OrgBAccountID;
				filter.OrganizationID = Filter.Current.OrganizationID;
				filter.BranchID = Filter.Current.BranchID;
				filter.LedgerID = Filter.Current.LedgerID;
				filter.AccountID = Filter.Current.AccountID;
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
			if (EnqResult.Current != null)
			{
				AccountHistoryBySubEnq graph = CreateInstance<AccountHistoryBySubEnq>();
				GLHistoryEnqFilter filter = PXCache<GLHistoryEnqFilter>.CreateCopy(graph.Filter.Current);
				filter.AccountID = Filter.Current.AccountID;
				filter.LedgerID = Filter.Current.LedgerID;
				filter.OrgBAccountID = Filter.Current.OrgBAccountID;
				filter.OrganizationID = Filter.Current.OrganizationID;
				filter.BranchID = Filter.Current.BranchID;
				filter.SubCD = Filter.Current.SubCD;
				filter.FinPeriodID = EnqResult.Current.LastActivityPeriod;
				filter.ShowCuryDetail = Filter.Current.ShowCuryDetail;
				graph.Filter.Update(filter);
				throw new PXRedirectRequiredException(graph, "Account by Subaccount");
			}
			return adapter.Get();
		}
		#endregion

		protected virtual IEnumerable enqResult()
		{
			AccountByYearFilter filter = CurrentFilter;
			bool showCurrency = filter.ShowCuryDetail.HasValue && filter.ShowCuryDetail.Value;

			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyID>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyPtdCreditTotal>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyPtdDebitTotal>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyBegBalance>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.curyEndBalance>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.signCuryBegBalance>(EnqResult.Cache, null, showCurrency);
			PXUIFieldAttribute.SetVisible<GLHistoryEnquiryResult.signCuryEndBalance>(EnqResult.Cache, null, showCurrency);

			if (filter.AccountID == null || filter.LedgerID == null || filter.FinYear == null) yield break; //Prevent code from accessing database;

			using (new PXReadBranchRestrictedScope(null, null, restrictByAccessRights: false, requireAccessForAllSpecified: false))
			{
			PXSelectBase<GLHistoryByPeriod> cmd = new PXSelectJoinGroupBy<GLHistoryByPeriod,
				LeftJoin<Account, On<GLHistoryByPeriod.accountID, Equal<Account.accountID>,
					And<Match<Account, Current<AccessInfo.userName>>>>,
				LeftJoin<MasterFinPeriod, On<GLHistoryByPeriod.finPeriodID, Equal<MasterFinPeriod.finPeriodID>>,
				LeftJoin<Sub, On<GLHistoryByPeriod.subID, Equal<Sub.subID>,
					And<Match<Sub, Current<AccessInfo.userName>>>>,
				LeftJoin<GLHistory, On<GLHistoryByPeriod.accountID, Equal<GLHistory.accountID>,
					And<GLHistoryByPeriod.subID, Equal<GLHistory.subID>,
					And<GLHistoryByPeriod.branchID, Equal<GLHistory.branchID>,
					And<GLHistoryByPeriod.ledgerID, Equal<GLHistory.ledgerID>,
					And<GLHistoryByPeriod.finPeriodID, Equal<GLHistory.finPeriodID>>>>>>,
				LeftJoin<AH, On<GLHistoryByPeriod.ledgerID, Equal<AH.ledgerID>,
					And<GLHistoryByPeriod.branchID, Equal<AH.branchID>,
					And<GLHistoryByPeriod.accountID, Equal<AH.accountID>,
					And<GLHistoryByPeriod.subID, Equal<AH.subID>,
					And<GLHistoryByPeriod.lastActivityPeriod, Equal<AH.finPeriodID>>>>>>>>>>>,
				Where<GLHistoryByPeriod.ledgerID, Equal<Current<AccountByYearFilter.ledgerID>>,
					And<MasterFinPeriod.finYear, Equal<Current<AccountByYearFilter.finYear>>,
					And<GLHistoryByPeriod.accountID, Equal<Current<AccountByYearFilter.accountID>>,
					And<Where2<
						Where<Account.accountID, NotEqual<Current<GLSetup.ytdNetIncAccountID>>, And<Where<Account.type, Equal<AccountType.asset>,
							Or<Account.type, Equal<AccountType.liability>>>>>,
						Or<Where<GLHistoryByPeriod.lastActivityPeriod, GreaterEqual<Required<GLHistoryByPeriod.lastActivityPeriod>>,
							And<Where<Account.type, Equal<AccountType.expense>,
							Or<Account.type, Equal<AccountType.income>,
							Or<Account.accountID, Equal<Current<GLSetup.ytdNetIncAccountID>>>>>>>>>>>>>,
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
					GroupBy<GLHistoryByPeriod.ledgerID,
					GroupBy<GLHistoryByPeriod.accountID,
					GroupBy<GLHistoryByPeriod.finPeriodID>>>>>>>>>>>>>>>>>>>>>>>>>(this);

			if (filter.SubID != null)
			{
				cmd.WhereAnd<Where<GLHistoryByPeriod.subID, Equal<Current<AccountByYearFilter.subID>>>>();
			}

			int[] branchIDs = null;

			if (filter.BranchID != null)
			{
				cmd.WhereAnd<Where<GLHistoryByPeriod.branchID, Equal<Current<AccountByYearFilter.branchID>>>>();
			}
			else if (filter.OrganizationID != null)
			{
				branchIDs = PXAccess.GetChildBranchIDs(filter.OrganizationID, false);

				cmd.WhereAnd<Where<GLHistoryByPeriod.branchID, In<Required<GLHistoryByPeriod.branchID>>,
				  And<MatchWithBranch<GLHistoryByPeriod.branchID>>>>();
			}

			if (!SubCDUtils.IsSubCDEmpty(filter.SubCD))
			{
				cmd.WhereAnd<Where<Sub.subCD, Like<Current<AccountByYearFilter.subCDWildcard>>>>();
			}

			foreach (PXResult<GLHistoryByPeriod, Account, MasterFinPeriod, Sub, GLHistory, AH> it in cmd.Select(filter.BegFinPeriod, branchIDs))
			{
				GLHistoryByPeriod baseview = it;
				Account acct = it;
				GLHistory ah = it;
				AH ah1 = it;
				ah.FinFlag = filter.UseMasterCalendar != true;
				ah1.FinFlag = filter.UseMasterCalendar != true;
				GLHistoryEnquiryResult item = new GLHistoryEnquiryResult
				{
					AccountID = baseview.AccountID,
					AccountCD = acct.AccountCD,
					LedgerID = baseview.LedgerID,
					LastActivityPeriod = baseview.FinPeriodID,
					PtdCreditTotal = ah.PtdCredit ?? 0m,
					PtdDebitTotal = ah.PtdDebit ?? 0m,
					CuryID = ah1.CuryID,
					Type = acct.Type,
					EndBalance = ah1.YtdBalance ?? 0m
				};
				if (!string.IsNullOrEmpty(ah1.CuryID))
				{
					item.CuryEndBalance = ah1.CuryYtdBalance ?? 0m; 
					item.CuryPtdCreditTotal = ah.CuryPtdCredit ?? 0m;
					item.CuryPtdDebitTotal = ah.CuryPtdDebit ?? 0m;
				}
				else
				{
					item.CuryEndBalance = null;
					item.CuryPtdCreditTotal = null;
					item.CuryPtdDebitTotal = null;
				}
				item.recalculate(true); // End balance is considered as correct digit - so we need to calculate begBalance base on ending one
				item.recalculateSignAmount(glsetup.Current?.TrialBalanceSign == GLSetup.trialBalanceSign.Reversed);
				yield return item;
			}
		}
		}
		public override bool IsDirty => false;

		#region Events
		protected virtual void AccountByYearFilter_AccountID_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			if (e.NewValue != null && !(e.NewValue is string))
			{
				Account acct = PXSelect<Account>.Search<Account.accountID>(this, e.NewValue);
				if (acct != null)
				{
					e.NewValue = acct.AccountCD;
				}
			}
		}
		protected virtual void AccountByYearFilter_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			AccountByYearFilter filter = e.Row as AccountByYearFilter;
			if (filter != null)
			{
				if (!string.IsNullOrEmpty(filter.FinPeriodID))
				{
					filter.FinYear = FinPeriodUtils.FiscalYear(filter.FinPeriodID); //Fill year from finPeriodID
				}

				if (string.IsNullOrEmpty(filter.FinYear))
				{
					DateTime businessDate = Accessinfo.BusinessDate.Value;
					filter.FinYear = businessDate.Year.ToString(FiscalPeriodSetupCreator.CS_YEAR_FORMAT);
				}
			}
		}
		protected virtual void AccountByYearFilter_RowUpdated(PXCache cache, PXRowUpdatedEventArgs e)
		{
			EnqResult.Select();
		}
		protected virtual void AccountByYearFilter_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			GLHistoryEnqFilter row = e.Row as GLHistoryEnqFilter;
			if (row == null) return;
			if (row?.AccountID != null)
			{
				Account acctDef = AccountInfo.Current == null || row.AccountID != AccountInfo.Current.AccountID ? AccountInfo.Select() : AccountInfo.Current;
				bool isDenominated = !string.IsNullOrEmpty(acctDef.CuryID);
				PXUIFieldAttribute.SetEnabled<AccountByYearFilter.showCuryDetail>(cache, e.Row, isDenominated);
				if(!isDenominated)
				{
					row.ShowCuryDetail = false;
				}
			}
		}
		protected virtual void AccountByYearFilter_SubCD_FieldVerifying(PXCache cache, PXFieldVerifyingEventArgs e)
		{
			e.Cancel = true;
		}
		protected virtual void AccountByYearFilter_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<AccountByYearFilter.ledgerID>(e.Row);
		}
		protected virtual void AccountByYearFilter_OrganizationID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<AccountByYearFilter.ledgerID>(e.Row);
		}

		#endregion
	}
}
