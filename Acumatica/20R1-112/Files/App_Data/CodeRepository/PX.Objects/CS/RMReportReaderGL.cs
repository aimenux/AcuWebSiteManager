using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PX.Common;
using PX.Data;
using PX.DbServices.QueryObjectModel;
using PX.CS;
using PX.Reports.ARm;
using PX.Reports.ARm.Data;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.CA.Descriptor;
using Branch = PX.Objects.GL.Branch;
using BAccount = PX.Objects.CR.BAccount;


namespace PX.Objects.CS
{
	public class RMReportReaderGL : PXGraphExtension<RMReportReader>
    {
        #region Report
        public PXSetup<GLSetup> Setup;

        private int? _ytdNetIncomeAccountID;

        private List<CR.BAccount> _bAccounts;
		private Dictionary<int, Ledger> _ledgers;
		private Dictionary<string, Branch> _branches;

        bool _initialized = false;

        private HashSet<Tuple<int, int>> _historyLoaded;
        private HashSet<Tuple<int, int, string>> _historyDrilldownLoaded;
        private HashSet<GLHistoryKeyTuple> _historySegments;
        private Dictionary<GLHistoryKeyTuple, Dictionary<string, GLHistory>> _glhistoryPeriodsByKey;

        private RMReportPeriods<GLHistory> _reportPeriods;

        private RMReportRange<Account> _accountRangeCache;
        private RMReportRange<Sub> _subRangeCache;
        private RMReportRange<Branch> _branchRangeCache;

		private string _accountMask;
	    private string _subMask;

        [PXOverride]
        public void Clear(Action del)
        {
            del();
            _initialized = false;
            _historyLoaded = null;
            _historyDrilldownLoaded = null;
            _accountRangeCache = null;
            _subRangeCache = null;
            _branchRangeCache = null;
        }

        public virtual void GLEnsureInitialized()
        {
            if (!_initialized)
            {
                _initialized = true;
                if (Setup != null && Setup.Current != null)
                {
                    _ytdNetIncomeAccountID = Setup.Current.YtdNetIncAccountID;
                }

                _reportPeriods = new RMReportPeriods<GLHistory>(this.Base);
				_bAccounts = new List<BAccount>();
				_branches = new Dictionary<string, Branch>(StringComparer.OrdinalIgnoreCase);

				foreach (PXResult<BAccount, Branch> bAccountAndBranch in PXSelectJoinOrderBy<BAccount, 
					InnerJoin<Branch, On<BAccount.bAccountID, Equal<Branch.bAccountID>>>, 
					OrderBy<Asc<BAccount.acctCD>>>.Select(Base).ToList())
				{
					BAccount bAccount = bAccountAndBranch;
					Branch branch = bAccountAndBranch;

					_bAccounts.Add(bAccount);
					_branches.Add(RMReportWildcard.NormalizeDsValue(branch.BranchCD), branch);
				}

				_ledgers = PXSelectOrderBy<Ledger, OrderBy<Asc<Ledger.ledgerCD>>>.Select(Base).RowCast<Ledger>().ToDictionary(ledger => ledger.LedgerID ?? 0);

                Base.Caches[typeof(Account)].Clear();
                Base.Caches[typeof(Sub)].Clear();
                Base.Caches[typeof(Branch)].Clear();

                if (Base.Report.Current.ApplyRestrictionGroups == true)
                {
                    PXSelect<Account, Where<Match<Current<AccessInfo.userName>>>,OrderBy<Asc<Account.accountCD>>>.Clear(this.Base);
					var accounts = PXSelect<Account, Where<Match<Current<AccessInfo.userName>>>, OrderBy<Asc<Account.accountCD>>>.Select(this.Base).ToList();

					PXSelect<Sub, Where<Match<Current<AccessInfo.userName>>>, OrderBy<Asc<Sub.subCD>>>.Clear(this.Base);
					var subs = PXSelect<Sub, Where<Match<Current<AccessInfo.userName>>>, OrderBy<Asc<Sub.subCD>>>.Select(this.Base).ToList();

					PXSelect<Branch, Where2<MatchWithBranch<Branch.branchID>, And<Match<Current<AccessInfo.userName>>>>, OrderBy<Asc<Branch.branchCD>>>.Clear(this.Base);
					var branches = PXSelect<Branch, Where2<MatchWithBranch<Branch.branchID>, And<Match<Current<AccessInfo.userName>>>>, OrderBy<Asc<Branch.branchCD>>>.Select(this.Base).ToList();
				}
				else
                {
                    PXSelectOrderBy<Account, OrderBy<Asc<Account.accountCD>>>.Clear(this.Base);
					var accounts = PXSelectOrderBy<Account, OrderBy<Asc<Account.accountCD>>>.Select(this.Base).ToList();

					PXSelectOrderBy<Sub,  OrderBy<Asc<Sub.subCD>>>.Clear(this.Base);
					var subs = PXSelectOrderBy<Sub,OrderBy<Asc<Sub.subCD>>>.Select(this.Base).ToList();

					PXSelectOrderBy<Branch, OrderBy<Asc<Branch.branchCD>>>.Clear(this.Base);
					var branches = PXSelectOrderBy<Branch, OrderBy<Asc<Branch.branchCD>>>.Select(this.Base).ToList();
				}

				_accountRangeCache = new RMReportRange<Account>(Base, GL.AccountAttribute.DimensionName, RMReportConstants.WildcardMode.Fixed, RMReportConstants.BetweenMode.Fixed);
                _subRangeCache = new RMReportRange<Sub>(Base, GL.SubAccountAttribute.DimensionName, RMReportConstants.WildcardMode.Normal, RMReportConstants.BetweenMode.ByChar);
                _branchRangeCache = new RMReportRange<Branch>(Base, GL.BranchAttribute._DimensionName, RMReportConstants.WildcardMode.Fixed, RMReportConstants.BetweenMode.Fixed);

                _historySegments = new HashSet<GLHistoryKeyTuple>();
                _glhistoryPeriodsByKey = new Dictionary<GLHistoryKeyTuple, Dictionary<string, GLHistory>>();
                _historyLoaded = new HashSet<Tuple<int, int>>();
                _historyDrilldownLoaded = new HashSet<Tuple<int, int, string>>();

	            _accountMask = (this.Base.Caches[typeof (Account)].GetStateExt<Account.accountCD>(null) as PXStringState)?.InputMask;
	            _subMask = (this.Base.Caches[typeof (Sub)].GetStateExt<Sub.subCD>(null) as PXStringState)?.InputMask;
			}

        }

        public void NormalizeDataSource(RMDataSourceGL dsGL)
        {
            if (dsGL.StartBranch != null && dsGL.StartBranch.TrimEnd() == "")
            {
                dsGL.StartBranch = null;
            }
            if (dsGL.EndBranch != null && dsGL.EndBranch.TrimEnd() == "")
            {
                dsGL.EndBranch = null;
            }
            if (dsGL.AccountClassID != null && dsGL.AccountClassID.TrimEnd() == "")
            {
                dsGL.AccountClassID = null;
            }
            if (dsGL.StartAccount != null && dsGL.StartAccount.TrimEnd() == "")
            {
                dsGL.StartAccount = null;
            }
            if (dsGL.EndAccount != null && dsGL.EndAccount.TrimEnd() == "")
            {
                dsGL.EndAccount = null;
            }
            if (dsGL.StartSub != null && dsGL.StartSub.TrimEnd() == "")
            {
                dsGL.StartSub = null;
            }
            if (dsGL.EndSub != null && dsGL.EndSub.TrimEnd() == "")
            {
                dsGL.EndSub = null;
            }
            if (dsGL.StartPeriod != null && dsGL.StartPeriod.TrimEnd() == "")
            {
                dsGL.StartPeriod = null;
            }
            if (dsGL.EndPeriod != null && dsGL.EndPeriod.TrimEnd() == "")
            {
                dsGL.EndPeriod = null;
            }
            if (dsGL.StartPeriodOffset == null)
            {
                dsGL.StartPeriodOffset = 0;
            }
            if (dsGL.StartPeriodYearOffset == null)
            {
                dsGL.StartPeriodYearOffset = 0;
            }
            if (dsGL.EndPeriodOffset == null)
            {
                dsGL.EndPeriodOffset = 0;
            }
            if (dsGL.EndPeriodYearOffset == null)
            {
                dsGL.EndPeriodYearOffset = 0;
            }
        }

		private IEnumerable<GLHistory> NormalizeHistory(IEnumerable<PXResult<GLHistoryByPeriod, GLHistory, Account>> history)
		{
			foreach(PXResult<GLHistoryByPeriod, GLHistory, Account> historyItem in history)
			{
				GLHistoryByPeriod currHistory = historyItem;
				GLHistory lastHistory = historyItem;

				if(currHistory.FinPeriodID == currHistory.LastActivityPeriod)
				{
					yield return lastHistory;
				}
				else
				{
					Account account = historyItem;
					GLHistory holeHistory = new GLHistory
					{
						LedgerID = currHistory.LedgerID,
						BranchID = currHistory.BranchID,
						AccountID = currHistory.AccountID,
						SubID = currHistory.SubID,
						FinPeriodID = currHistory.FinPeriodID,
						FinPtdCredit = 0m,
						TranPtdCredit = 0m,
						FinPtdDebit = 0m,
						TranPtdDebit = 0m,
						CuryFinPtdCredit = 0m,
						CuryFinPtdDebit = 0m,
						CuryTranPtdCredit = 0m,
						CuryTranPtdDebit = 0m
					};

					holeHistory.FinBegBalance =
					holeHistory.FinYtdBalance =
						((account.Type == AccountType.Income || account.Type == AccountType.Expense) &&
						(new ReportFunctions()).ArePeriodsInSameYear(currHistory.LastActivityPeriod, currHistory.FinPeriodID))
						|| account.Type == AccountType.Asset || account.Type == AccountType.Liability
						? lastHistory.FinYtdBalance
						: 0m;

					holeHistory.TranBegBalance =
					holeHistory.TranYtdBalance =
							((account.Type == AccountType.Income || account.Type == AccountType.Expense) &&
							 (new ReportFunctions()).ArePeriodsInSameYear(currHistory.LastActivityPeriod, currHistory.FinPeriodID))
							|| account.Type == AccountType.Asset || account.Type == AccountType.Liability
								? lastHistory.TranYtdBalance
								: 0m;

					holeHistory.CuryFinBegBalance =
					holeHistory.CuryFinYtdBalance =
						((account.Type == AccountType.Income || account.Type == AccountType.Expense) &&
						(new ReportFunctions()).ArePeriodsInSameYear(currHistory.LastActivityPeriod, currHistory.FinPeriodID))
						|| account.Type == AccountType.Asset || account.Type == AccountType.Liability
						? lastHistory.CuryFinYtdBalance
						: 0m;

					holeHistory.CuryTranBegBalance =
					holeHistory.CuryTranYtdBalance =
							((account.Type == AccountType.Income || account.Type == AccountType.Expense) &&
							 (new ReportFunctions()).ArePeriodsInSameYear(currHistory.LastActivityPeriod, currHistory.FinPeriodID))
							|| account.Type == AccountType.Asset || account.Type == AccountType.Liability
								? lastHistory.CuryTranYtdBalance
								: 0m;

					yield return holeHistory;
				}
			}
		}

        private void LoadHistory(int ledgerID, int year)
        {
            // We do lazy-loading on a ledger and year basis; this could be changed depending on optimization scenario to have more or less granular loading.

			// 1. Get all GLHistory records for the year
			ProcessGLResultset(PXSelectReadonly<GLHistory,
					Where<GLHistory.ledgerID, Equal<Required<GLHistory.ledgerID>>,
					And<GLHistory.finPeriodID, GreaterEqual<Required<GLHistory.finPeriodID>>,
					And<GLHistory.finPeriodID, Less<Required<GLHistory.finPeriodID>>>>>>.Select(this.Base, new object[] { ledgerID, year.ToString(), (year + 1).ToString() }));

			// 2. For asset and liability accounts, get record for immediately preceding year
			MasterFinPeriod lastPeriodOfPrevYear = _reportPeriods.FinPeriods.Where(p => String.Compare(p.FinPeriodID, year.ToString()) < 0).LastOrDefault();
			if (lastPeriodOfPrevYear != null)
			{
				ProcessGLResultset(PXSelectReadonly2<GLHistory,
					InnerJoin<Account, On<GLHistory.accountID, Equal<Account.accountID>>,
					InnerJoin<GLHistoryByPeriod, On<GLHistory.ledgerID, Equal<GLHistoryByPeriod.ledgerID>,
							And<GLHistory.branchID, Equal<GLHistoryByPeriod.branchID>,
							And<GLHistory.accountID, Equal<GLHistoryByPeriod.accountID>,
							And<GLHistory.subID, Equal<GLHistoryByPeriod.subID>,
							And<GLHistory.finPeriodID, Equal<GLHistoryByPeriod.lastActivityPeriod>>>>>>>>,
					Where2<Where<Account.type, Equal<AccountType.asset>, Or<Account.type, Equal<AccountType.liability>>>,
						And<Where<GLHistoryByPeriod.ledgerID, Equal<Required<GLHistoryByPeriod.ledgerID>>,
						And<GLHistoryByPeriod.finPeriodID, Equal<Required<GLHistoryByPeriod.finPeriodID>>>>>>>.Select(this.Base, new object[] { ledgerID, lastPeriodOfPrevYear.FinPeriodID }));
			}
		}

		private void ProcessGLResultset(PXResultset<GLHistory> resultset)
		{
			foreach (GLHistory hist in resultset)
			{
				var key = new GLHistoryKeyTuple(hist.LedgerID.Value, hist.BranchID.Value, hist.AccountID.Value, hist.SubID.Value);
				Dictionary<string, GLHistory> keyData;
				if (_glhistoryPeriodsByKey.TryGetValue(key, out keyData))
				{
					keyData[hist.FinPeriodID] = hist;
				}
				else
				{
					_glhistoryPeriodsByKey.Add(key, new Dictionary<string, GLHistory>() { { hist.FinPeriodID, hist } });
				}

				_historySegments.Add(new GLHistoryKeyTuple(hist.LedgerID.Value, 0, hist.AccountID.Value, 0));
				_historySegments.Add(new GLHistoryKeyTuple(hist.LedgerID.Value, 0, hist.AccountID.Value, hist.SubID.Value));
			}
		}

		private void LoadDrillDownHistory(int ledgerID, int year, string accountClassID)
        {
            // We do lazy-loading on a ledger and year basis; this could be changed depending on optimization scenario to have more or less granular loading.

			PXSelectBase<GLHistoryByPeriod> glHistorySelect = new PXSelectReadonly2<
					GLHistoryByPeriod,
					LeftJoin<GLHistory,
						On<GLHistoryByPeriod.ledgerID, Equal<GLHistory.ledgerID>,
							And<GLHistoryByPeriod.accountID, Equal<GLHistory.accountID>,
							And<GLHistoryByPeriod.subID, Equal<GLHistory.subID>,
							And<GLHistoryByPeriod.lastActivityPeriod, Equal<GLHistory.finPeriodID>,
							And<GLHistoryByPeriod.branchID, Equal<GLHistory.branchID>
							>>>>>,
					LeftJoin<Account,
						On<GLHistoryByPeriod.accountID, Equal<Account.accountID>>>>,
					Where<GLHistoryByPeriod.ledgerID, Equal<Required<GLHistoryByPeriod.ledgerID>>>,
					OrderBy<Asc<GLHistoryByPeriod.ledgerID,
							Asc<GLHistoryByPeriod.branchID,
							Asc<GLHistoryByPeriod.accountID,
							Asc<GLHistoryByPeriod.subID,
							Asc<GLHistoryByPeriod.finPeriodID>>>>>>
					>(this.Base);

			List<object> glHistorySelectParameters = new List<object>();
			glHistorySelectParameters.Add(ledgerID);

			if (!String.IsNullOrEmpty(accountClassID))
			{
				glHistorySelect.WhereAnd<Where<Account.accountClassID, Equal<Required<Account.accountClassID>>>>();
				glHistorySelectParameters.Add(accountClassID);
			}

            // 1. Get all GLHistory records for the year
			BqlCommand yearHistorySelect = glHistorySelect.View.BqlSelect.WhereAnd<
				Where<GLHistoryByPeriod.finPeriodID, GreaterEqual<Required<GLHistory.finPeriodID>>,
					And<GLHistoryByPeriod.finPeriodID, Less<Required<GLHistory.finPeriodID>>>>>();

			List<object> yearHistoryParameters = new List<object>(glHistorySelectParameters);
			yearHistoryParameters.Add(year.ToString());
			yearHistoryParameters.Add((year + 1).ToString());

			IEnumerable<GLHistory> yearHistory = NormalizeHistory(
				new PXView(Base, true, yearHistorySelect)
				.SelectMulti(yearHistoryParameters.ToArray())
				.Cast<PXResult<GLHistoryByPeriod, GLHistory, Account>>()
			);

			ProcessGLResultset(yearHistory);

            // 2. For asset and liability accounts, get record for immediately preceding year
            MasterFinPeriod lastPeriodOfPrevYear = _reportPeriods.FinPeriods.Where(p => String.Compare(p.FinPeriodID, year.ToString()) < 0).LastOrDefault();
            if (lastPeriodOfPrevYear != null)
            {
				BqlCommand prevYearHistorySelect = glHistorySelect.View.BqlSelect.WhereAnd<
                    Where2<Where<Account.type, Equal<AccountType.asset>, Or<Account.type, Equal<AccountType.liability>>>,
						And<GLHistoryByPeriod.finPeriodID, Equal<Required<GLHistoryByPeriod.finPeriodID>>>>>();

				List<object> prevYearHistoryParameters = new List<object>(glHistorySelectParameters);
				prevYearHistoryParameters.Add(lastPeriodOfPrevYear.FinPeriodID);

				IEnumerable<GLHistory> prevYearHistory = NormalizeHistory(
				new PXView(Base, true, prevYearHistorySelect)
				.SelectMulti(prevYearHistoryParameters.ToArray())
				.Cast<PXResult<GLHistoryByPeriod, GLHistory, Account>>());

				ProcessGLResultset(prevYearHistory);
            }
        }

        private void ProcessGLResultset(IEnumerable<GLHistory> history)
        {
            foreach (GLHistory historyItem in history)
            {
                var key = new GLHistoryKeyTuple(historyItem.LedgerID.Value, historyItem.BranchID.Value, historyItem.AccountID.Value, historyItem.SubID.Value);
                Dictionary<string, GLHistory> keyData;
                if (_glhistoryPeriodsByKey.TryGetValue(key, out keyData))
                {
                    keyData[historyItem.FinPeriodID] = historyItem;
                }
                else
                {
                    _glhistoryPeriodsByKey.Add(key, new Dictionary<string, GLHistory>() { { historyItem.FinPeriodID, historyItem } });
                }

                _historySegments.Add(new GLHistoryKeyTuple(historyItem.LedgerID.Value, 0, historyItem.AccountID.Value, 0));
                _historySegments.Add(new GLHistoryKeyTuple(historyItem.LedgerID.Value, 0, historyItem.AccountID.Value, historyItem.SubID.Value));
            }
        }

        [PXOverride]
        public virtual object GetHistoryValue(ARmDataSet dataSet, bool drilldown, Func<ARmDataSet, bool, object> del)
        {
            string rmType = Base.Report.Current.Type;
            if (rmType == ARmReport.GL)
            {
                if (((!string.IsNullOrEmpty(dataSet[Keys.BookCode] as string ?? "") && (!string.IsNullOrEmpty(dataSet[Keys.StartAccount] as string ?? "") ||
                !string.IsNullOrEmpty(dataSet[Keys.AccountClass] as string ?? "")) && !string.IsNullOrEmpty(dataSet[Keys.StartPeriod] as string ?? "") && (short?)dataSet[Keys.AmountType] != (short?)BalanceType.NotSet)))
                {
                    RMDataSource ds = Base.DataSourceByID.Current;
                    RMDataSourceGL dsGL = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourceGL>(ds);

                    ds.AmountType = (short?)dataSet[Keys.AmountType];
						if (!String.IsNullOrEmpty(dataSet[Keys.BookCode] as string))
						{
							Base.DataSourceByID.SetValueExt<RMDataSourceGL.ledgerID>(ds, dataSet[Keys.BookCode] as string);
						}
                    dsGL.AccountClassID = dataSet[Keys.AccountClass] as string ?? "";
                    dsGL.StartAccount = dataSet[Keys.StartAccount] as string ?? "";
                    dsGL.EndAccount = dataSet[Keys.EndAccount] as string ?? "";
                    dsGL.StartSub = dataSet[Keys.StartSub] as string ?? "";
                    dsGL.EndSub = dataSet[Keys.EndSub] as string ?? "";

						if (!String.IsNullOrEmpty(dataSet[Keys.Organization] as string))
						{
							Base.DataSourceByID.SetValueExt<RMDataSourceGL.organizationID>(ds, dataSet[Keys.Organization] as string);
						}

	                dsGL.UseMasterCalendar = dataSet[Keys.UseMasterCalendar] as bool?;
                    dsGL.StartBranch = dataSet[Keys.StartBranch] as string ?? "";
                    dsGL.EndBranch = dataSet[Keys.EndBranch] as string ?? "";
                    dsGL.EndPeriod = ((dataSet[Keys.EndPeriod] as string ?? "").Length > 2 ? ((dataSet[Keys.EndPeriod] as string ?? "").Substring(2) + "    ").Substring(0, 4) : "    ") + ((dataSet[Keys.EndPeriod] as string ?? "").Length > 2 ? (dataSet[Keys.EndPeriod] as string ?? "").Substring(0, 2) : dataSet[Keys.EndPeriod] as string ?? "");
                    dsGL.EndPeriodOffset = (short?)(int?)dataSet[Keys.EndOffset];
                    dsGL.EndPeriodYearOffset = (short?)(int?)dataSet[Keys.EndYearOffset];
                    dsGL.StartPeriod = ((dataSet[Keys.StartPeriod] as string ?? "").Length > 2 ? ((dataSet[Keys.StartPeriod] as string ?? "").Substring(2) + "    ").Substring(0, 4) : "    ") + ((dataSet[Keys.StartPeriod] as string ?? "").Length > 2 ? (dataSet[Keys.StartPeriod] as string ?? "").Substring(0, 2) : dataSet[Keys.StartPeriod] as string ?? "");
                    dsGL.StartPeriodOffset = (short?)(int?)dataSet[Keys.StartOffset];
                    dsGL.StartPeriodYearOffset = (short?)(int?)dataSet[Keys.StartYearOffset];

                    if (drilldown)
                    {
                        Base.DrilldownNumber++;
                    }

                    List<object[]> splitret = null;

                    if (ds.Expand != ExpandType.Nothing)
                    {
                        splitret = new List<object[]>();
                    }

                    if (dsGL.LedgerID == null || ds.AmountType == null || ds.AmountType == 0)
                    {
                        return 0m;
                    }

                    GLEnsureInitialized();
                    EnsureHistoryLoaded(dsGL, drilldown);
                    NormalizeDataSource(dsGL);

					List<Account> accounts = GetItemsInRange<Account>(dataSet);
	                List<Sub> subs = GetItemsInRange<Sub>(dataSet);
					List<Branch> branchList = GetItemsInRange<Branch>(dataSet);

						if (dsGL.OrganizationID != null)
						{
							branchList = branchList.Where(branch => branch.OrganizationID == dsGL.OrganizationID).ToList();
						}

                    if (ds.Expand == ExpandType.Account)
						foreach (var account in accounts)
                    {
							var dataSetCopy = new ARmDataSet(dataSet);
							dataSetCopy[Keys.StartSub] = dataSetCopy[Keys.EndSub] = account.AccountCD;
							// ReSharper disable once PossibleNullReferenceException
							splitret.Add(new object[] { account.AccountCD, account.Description, 0m, dataSetCopy, null, Mask.Format(_accountMask, account.AccountCD) });
                    }
                    else if (ds.Expand == ExpandType.Sub)
						foreach (var sub in subs)
                    {
							var dataSetCopy = new ARmDataSet(dataSet);
							dataSetCopy[Keys.StartSub] = dataSetCopy[Keys.EndSub] = sub.SubCD;
							// ReSharper disable once PossibleNullReferenceException
							splitret.Add(new object[] { sub.SubCD, sub.Description, 0m, dataSetCopy, null, Mask.Format(_subMask, sub.SubCD) });
                    }

                    if (drilldown && ds.Expand == ExpandType.Account)
                    {
                        SortAccounts(accounts);
                    }
                    return CalculateAndExpandValue(drilldown, ds, dsGL, dataSet, accounts, subs, branchList, splitret);
                }
                else
                {
                    return Decimal.MinValue;
                }
            }
            else
            {
                return del(dataSet, drilldown);
            }
        }

	    private List<T> GetItemsInRange<T>(ARmDataSet dataSet)
	    {
		    return (List<T>)Base.GetItemsInRange(typeof(T), dataSet);
	    }

		[PXOverride]
	    public virtual IEnumerable GetItemsInRange(Type table, ARmDataSet dataSet, Func<Type, ARmDataSet, IEnumerable> del)
	    {
		    if (table == typeof (Account))
		    {
			    return _accountRangeCache.GetItemsInRange(dataSet[Keys.StartAccount] as string, 
					range => range + dataSet[Keys.AccountClass] as string ?? "",
					range => GetAccountsInRange(range, dataSet[Keys.AccountClass] as string ?? ""));
		    }

		    if (table == typeof (Sub))
		    {
			    return _subRangeCache.GetItemsInRange(dataSet[Keys.StartSub] as string, 
					sub => sub.SubCD, 
					(sub, code) => sub.SubCD = code);
		    }

		    if (table == typeof (Branch))
		    {
				string range = (dataSet[Keys.StartBranch] as string)?.ToUpperInvariant();
				return _branchRangeCache.GetItemsInRange(range, 
					branch => branch.BranchCD?.ToUpperInvariant(), 
					(branch, code) =>
					{
						Branch existingBranch;
						_branches.TryGetValue(code, out existingBranch);
						branch.BranchCD = existingBranch?.BranchCD ?? code?.ToUpperInvariant();
					});
		    }

		    if (del != null)
		    {
			    return del(table, dataSet);
		    }

		    throw new NotSupportedException();
	    }

        private HashSet<Account> GetAccountsInRange(string range, string accountClassID)
        {
            var accounts = new HashSet<Account>();
            string[] accountPairs = range.Split(RMReportConstants.RangeUnionChar);

            foreach (string accountPair in accountPairs)
            {
                string startAccount, endAccount;
                RMReportRange<Account>.ParseRangeStartEndPair(accountPair, out startAccount, out endAccount);

                if (!String.IsNullOrEmpty(startAccount))
                {
                    if (String.IsNullOrEmpty(endAccount) || endAccount == startAccount)
                    {
                        string acct = RMReportWildcard.EnsureWildcardForFixed(startAccount, _accountRangeCache.Wildcard);
                        if (acct.Contains(RMReportConstants.DefaultWildcardChar))
                        {
                            accounts.UnionWith(from Account a in _accountRangeCache.Cache.Cached
                                               where RMReportWildcard.IsLike(acct, a.AccountCD) && (String.IsNullOrEmpty(accountClassID) || accountClassID == a.AccountClassID)
                                               select a);
                        }
                        else
                        {
                            _accountRangeCache.Instance.AccountCD = acct;
                            Account a = (Account)_accountRangeCache.Cache.Locate(_accountRangeCache.Instance);
                            if (a != null && (String.IsNullOrEmpty(accountClassID) || accountClassID == a.AccountClassID))
                            {
                                accounts.Add(a);
                            }
                        }
                    }
                    else
                    {
                        accounts.UnionWith(RMReportWildcard.GetBetweenForFixed<Account>(startAccount, endAccount, _accountRangeCache.Wildcard, _accountRangeCache.Cache.Cached, a => a.AccountCD)
                                    .Where(a => String.IsNullOrEmpty(accountClassID) || accountClassID == a.AccountClassID));
                    }
                }
                else
                {
                    accounts.UnionWith(from Account a in _accountRangeCache.Cache.Cached
                                       where (String.IsNullOrEmpty(accountClassID) || accountClassID == a.AccountClassID)
                                       select a);
                }
            }

            return accounts;
        }

        private void EnsureHistoryLoaded(RMDataSourceGL dsGL, bool isDrillDown)
        {
            string per = String.Empty;
            string toper = String.Empty;

            if (dsGL.StartPeriod != null)
            {
                per = RMReportWildcard.EnsureWildcard(dsGL.StartPeriod, _reportPeriods.PerWildcard);
                if (!per.Contains(RMReportConstants.DefaultWildcardChar) && dsGL.StartPeriodOffset != null && dsGL.StartPeriodOffset != 0 || dsGL.StartPeriodYearOffset != 0)
                {
                    per = _reportPeriods.GetFinPeriod(per, dsGL.StartPeriodYearOffset, dsGL.StartPeriodOffset);
                }
                per = per.Replace(RMReportConstants.DefaultWildcardChar, '0');
            }

            if (dsGL.EndPeriod != null)
            {
                toper = RMReportWildcard.EnsureWildcard(dsGL.EndPeriod, _reportPeriods.PerWildcard);
                if (!toper.Contains(RMReportConstants.DefaultWildcardChar) && dsGL.EndPeriodOffset != null && dsGL.EndPeriodOffset != 0 || dsGL.EndPeriodYearOffset != 0)
                {
                    toper = _reportPeriods.GetFinPeriod(toper, dsGL.EndPeriodYearOffset, dsGL.EndPeriodOffset);
                }
                toper = toper.Replace(RMReportConstants.DefaultWildcardChar, '9');
            }

            if (String.IsNullOrEmpty(per))
            {
                per = _reportPeriods.FinPeriods.Select(p => p.FinPeriodID).Min();
            }
            if (String.IsNullOrEmpty(toper))
            {
                toper = _reportPeriods.FinPeriods.Select(p => p.FinPeriodID).Max();
            }

            int startYear = int.Parse(per.Substring(0, 4));
            int endYear = int.Parse(toper.Substring(0, 4));

            for (int year = startYear; year <= endYear; year++)
            {
                var historyKey = new Tuple<int, int>(dsGL.LedgerID.Value, year);
					if (isDrillDown)
					{
                    var drilldownKey = new Tuple<int, int, string>(dsGL.LedgerID.Value, year, dsGL.AccountClassID);
                    if (!_historyLoaded.Contains(historyKey) &&
                        !_historyDrilldownLoaded.Contains(drilldownKey))
                    {
						LoadDrillDownHistory(dsGL.LedgerID.Value, year, dsGL.AccountClassID);
                        _historyDrilldownLoaded.Add(drilldownKey);
                    }
					}
					else
					{
                    if (!_historyLoaded.Contains(historyKey))
                    {
                    LoadHistory(dsGL.LedgerID.Value, year);
                        _historyLoaded.Add(historyKey);
					}
                }
            }
        }

        private static void SortAccounts(List<Account> accounts)
        {
            accounts.Sort((Account a, Account b) =>
            {
                if (a.COAOrder < b.COAOrder)
                {
                    return -1;
                }
                if (a.COAOrder > b.COAOrder)
                {
                    return 1;
                }

                if (a.Type == b.Type)
                {
                    return String.Compare(a.AccountCD, b.AccountCD, StringComparison.OrdinalIgnoreCase);
                }

                return String.Compare(a.Type, b.Type, StringComparison.OrdinalIgnoreCase);
            });
        }

        private List<GLHistory> GetPeriodsToCalculate(Branch branch, Account account, Sub sub, RMDataSource ds, RMDataSourceGL dsGL, out bool takeLast)
        {
            takeLast = false;

            Dictionary<string, GLHistory> periodsForKey = null;
            var key = new GLHistoryKeyTuple(dsGL.LedgerID.Value, branch.BranchID.Value, account.AccountID.Value, sub.SubID.Value);
            if (!_glhistoryPeriodsByKey.TryGetValue(key, out periodsForKey))
            {
                return null;
            }

			Ledger ledger;
			bool isBudgetLedger = _ledgers.TryGetValue(dsGL.LedgerID.Value, out ledger) && ledger?.BalanceType == LedgerBalanceType.Budget;
            bool isPLAccount = (account.Type == AccountType.Expense || account.Type == AccountType.Income);
			bool limitToStartYear = isPLAccount || account.AccountID == _ytdNetIncomeAccountID || isBudgetLedger;

            if (ds.AmountType == BalanceType.BeginningBalance || ds.AmountType == BalanceType.CuryBeginningBalance)
            {
                return _reportPeriods.GetPeriodsForBeginningBalanceAmount(dsGL, periodsForKey, limitToStartYear, out takeLast);
            }
            else if (ds.AmountType == BalanceType.EndingBalance || ds.AmountType == BalanceType.CuryEndingBalance)
            {
                return _reportPeriods.GetPeriodsForEndingBalanceAmount(dsGL, periodsForKey, limitToStartYear);
            }
            else
            {
                return _reportPeriods.GetPeriodsForRegularAmount(dsGL, periodsForKey);
            }
        }

        private object CalculateAndExpandValue(bool drilldown, RMDataSource ds, RMDataSourceGL dsGL, ARmDataSet dataSet, List<Account> accounts, List<Sub> subs, List<Branch> branchList, List<object[]> splitret)
        {
            decimal totalAmount = 0;
            object locker = new object();
            Dictionary<GLHistoryKeyTuple, PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>> drilldownData = null;

	        GLSetup glSetup = null;
            if (drilldown)
            {
                drilldownData = new Dictionary<GLHistoryKeyTuple, PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>>();
	            glSetup = PXSelect<GLSetup>.Select(this.Base);
            }
			bool forceNonParallel = false;
			var options = new ParallelOptions { MaxDegreeOfParallelism = forceNonParallel ? 1 : -1 };
	        Parallel.For(0, accounts.Count, options, account =>
            {
                Account currentAccount = accounts[account];
                if (!_historySegments.Contains(new GLHistoryKeyTuple(dsGL.LedgerID.Value, 0, currentAccount.AccountID.Value, 0))) return;

				Parallel.For(0, subs.Count, options, sub =>
                {
                    Sub currentSub = subs[sub];
                    if (!_historySegments.Contains(new GLHistoryKeyTuple(dsGL.LedgerID.Value, 0, currentAccount.AccountID.Value, currentSub.SubID.Value))) return;

					Parallel.ForEach(branchList, options, currentBranch =>
                    {
                        bool takeLast = false;
                        List<GLHistory> periods = GetPeriodsToCalculate(currentBranch, currentAccount, currentSub, ds, dsGL, out takeLast);
                        if (periods == null) return;

                        foreach (var hist in periods)
                        {
	                        hist.FinFlag = dsGL.UseMasterCalendar != true;
							decimal amount = GetAmountFromGLHistory(ds, currentAccount, hist, takeLast);
                            lock (locker)
                            {
                                totalAmount += amount;
                            }

                            if (drilldown)
                            {
                                var key = new GLHistoryKeyTuple(dsGL.LedgerID.Value, currentBranch.BranchID.Value, currentAccount.AccountID.Value, currentSub.SubID.Value);
                                PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup> drilldownRow;

                                lock (drilldownData)
                                {
                                    if (!drilldownData.TryGetValue(key, out drilldownRow))
                                    {
                                        GLHistoryByPeriod ghp = GetGLHistoryByPeriodRecordForDrilldown(ds, hist);
										drilldownRow = new PXResult<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>(ghp, currentAccount, currentSub, new GLHistory(), glSetup);
	                                    ((GLHistory)drilldownRow).FinFlag = dsGL.UseMasterCalendar != true;	                                    
										drilldownData.Add(key, drilldownRow);
                                    }
                                }

                                lock (drilldownRow)
                                {
	                                hist.FinFlag = dsGL.UseMasterCalendar != true;
									AggregateGLHistoryForDrillDown((GLHistory)drilldownRow, hist);
                                }
                            }

                            if (ds.Expand == ExpandType.Account)
                            {
                                lock (currentAccount)
                                {
                                    splitret[account][2] = (decimal)splitret[account][2] + amount;
									if (splitret[account][4] == null)
                                    {
                                        var dataSetCopy = new ARmDataSet(dataSet);
                                        dataSetCopy[PX.Objects.CS.RMReportReaderGL.Keys.StartAccount] = dataSetCopy[PX.Objects.CS.RMReportReaderGL.Keys.EndAccount] = currentAccount.AccountCD;
                                        splitret[account][3] = dataSetCopy;
                                        splitret[account][4] = _bAccounts.FirstOrDefault(ba => ba.BAccountID == currentBranch.BAccountID).AcctName;
									}
                                }
                            }
                            else if (ds.Expand == ExpandType.Sub)
                            {
                                lock (currentSub)
                                {
                                    splitret[sub][2] = (decimal)splitret[sub][2] + amount;
									if (splitret[sub][4] == null)
                                    {
                                        var dataSetCopy = new ARmDataSet(dataSet);
                                        dataSetCopy[PX.Objects.CS.RMReportReaderGL.Keys.StartSub] = dataSetCopy[PX.Objects.CS.RMReportReaderGL.Keys.EndSub] = currentSub.SubCD;
                                        splitret[sub][3] = dataSetCopy;
                                        splitret[sub][4] = _bAccounts.FirstOrDefault(ba => ba.BAccountID == currentBranch.BAccountID).AcctName;
									}
                                }
                            }
                        }
                    });
                });
            });

            if (drilldown)
            {
                var resultset = new PXResultset<GLHistoryByPeriod, Account, Sub, GLHistory, GLSetup>();
				foreach (var r in 
					from row in drilldownData.Values
					let accCD = ((Account)row[typeof(Account)]).With(_ => _.AccountCD)
					let subCD = ((Sub)row[typeof(Sub)]).With(_ => _.SubCD)
					let finPeriod = ((GLHistoryByPeriod)row[typeof(GLHistoryByPeriod)]).With(_ => _.FinPeriodID)
					orderby accCD, subCD, finPeriod
					select row)
                {
                    resultset.Add(r);
                }

                var hist = (GLHistory)resultset;
                if (hist != null) hist.CuryYtdBalance = totalAmount;
                return resultset;
            }
            else if (ds.Expand != ExpandType.Nothing)
            {
                return splitret;
            }
            else
            {
                return totalAmount;
            }
        }

        private static void AggregateGLHistoryForDrillDown(GLHistory resulthist, GLHistory hist)
        {
            // When drilling down on cells showing total debit, total credit or turnover for a period we need to aggregate GLHistory results and show only one total per account/subaccount.
            resulthist.LedgerID = hist.LedgerID;
            resulthist.BranchID = hist.BranchID;
            resulthist.AccountID = hist.AccountID;
            resulthist.SubID = hist.SubID;
            resulthist.FinPeriodID = hist.FinPeriodID;
            if (resulthist.BegBalance == null)
            {
                resulthist.BegBalance = hist.BegBalance;
                resulthist.PtdDebit = 0m;
                resulthist.PtdCredit = 0m;
            }
            resulthist.PtdDebit += hist.PtdDebit;
            resulthist.PtdCredit += hist.PtdCredit;
            resulthist.YtdBalance = hist.YtdBalance;
        }

        private GLHistoryByPeriod GetGLHistoryByPeriodRecordForDrilldown(RMDataSource ds, GLHistory hist)
        {
            GLHistoryByPeriod ghp = new GLHistoryByPeriod();
            ghp.LedgerID = hist.LedgerID;
            ghp.BranchID = hist.BranchID;
            ghp.AccountID = hist.AccountID;
            ghp.SubID = hist.SubID;
            ghp.FinPeriodID = hist.FinPeriodID;
            ghp.LastActivityPeriod = FinPeriodIDFormattingAttribute.FormatForStoringNoTrim(ds.AmountType.ToString() + RMReportConstants.RangeDelimiterChar.ToString() + Base.DrilldownNumber.ToString());
            return ghp;
        }

        private static decimal GetAmountFromGLHistory(RMDataSource ds, Account account, GLHistory hist, bool takeLast)
        {
            switch (ds.AmountType.Value)
            {
                case BalanceType.Turnover:
                    if (account.Type == AccountType.Asset || account.Type == AccountType.Expense)
                    {
                        return (decimal)(hist.PtdDebit - hist.PtdCredit);
                    }
                    else
                    {
                        return (decimal)(hist.PtdCredit - hist.PtdDebit);
                    }
                case BalanceType.CuryTurnover:
                    if (account.Type == AccountType.Asset || account.Type == AccountType.Expense)
                    {
                        return (decimal)(hist.CuryPtdDebit - hist.CuryPtdCredit);
                    }
                    else
                    {
                        return (decimal)(hist.CuryPtdCredit - hist.CuryPtdDebit);
                    }
                case BalanceType.Credit:
                    return (decimal)hist.PtdCredit;
                case BalanceType.CuryCredit:
                    return (decimal)hist.CuryPtdCredit;
                case BalanceType.Debit:
                    return (decimal)hist.PtdDebit;
                case BalanceType.CuryDebit:
                    return (decimal)hist.CuryPtdDebit;
                case BalanceType.BeginningBalance:
                    if (takeLast)
                    {
                        return (decimal)hist.YtdBalance; // There's no history record for this period, so we use the ending balance of the previous one.
                    }
                    else
                    {
                        return (decimal)hist.BegBalance;
                    }
                case BalanceType.CuryBeginningBalance:
                    if (takeLast)
                    {
                        return (decimal)hist.CuryYtdBalance; // There's no history record for this period, so we use the ending balance of the previous one.
                    }
                    else
                    {
                        return (decimal)hist.CuryBegBalance;
                    }
                case BalanceType.EndingBalance:
                    return (decimal)hist.YtdBalance;
                case BalanceType.CuryEndingBalance:
                    return (decimal)hist.CuryYtdBalance;
                default:
                    System.Diagnostics.Debug.Assert(false, "Unknown amount type: " + ds.AmountType.Value);
                    return 0;
            }
        }

        [PXOverride]
        public string GetUrl(Func<string> del)
        {
            string rmType = Base.Report.Current.Type;
            if (rmType == ARmReport.GL)
            {
                PXSiteMapNode node = PXSiteMap.Provider.FindSiteMapNodeByScreenID("CS600000");
                if (node != null)
                {
                    return PX.Common.PXUrl.TrimUrl(node.Url);
                }
                throw new PXException(ErrorMessages.NotEnoughRightsToAccessObject, "CS600000");
            }
            else
            {
                return del();
            }
        }

		#endregion

		#region IARmDataSource

        // Initializing IDictionary with Keys and their string represantations in static constructor for perfomance
        // (Enum.GetNames(), Enum.TryParse(), Enum.GetValues(), Enum.IsDefined() are very slow because of using Reflection)

        static RMReportReaderGL()
        {
            _keysDictionary = Enum.GetValues(typeof(Keys)).Cast<Keys>().ToDictionary(@e => @e.ToString(), @e => @e);
        }

        private static readonly IDictionary<string, Keys> _keysDictionary;

		public enum Keys
		{
			AmountType,
			StartBranch,
			EndBranch,
			BookCode,
			EndAccount,
			EndSub,
			StartAccount,
			StartSub,
			AccountClass,
			EndOffset,
			EndYearOffset,
			EndPeriod,
			StartOffset,
			StartYearOffset,
			StartPeriod,
			Organization,
			OrganizationName,
			UseMasterCalendar
		}

		[PXOverride]
		public bool IsParameter(ARmDataSet ds, string name, ValueBucket value)
		{
			Keys key;
			if (_keysDictionary.TryGetValue(name, out key))
			{
				if (key == Keys.OrganizationName)
				{
					object org = ds[Keys.Organization];
					if (org != null)
					{
						var obj = PXAccess.GetOrganizations().
							FirstOrDefault(o => string.Compare(o.OrganizationCD, org.ToString(), true) == 0);
						value.Value = obj != null ? obj.OrganizationName : string.Empty;
					}
					return true;
				}
				else value.Value = ds[key];

				bool isStartOfRange = key == Keys.StartAccount || key == Keys.StartBranch || key == Keys.StartSub;
				bool isEndOfRange = key == Keys.EndAccount || key == Keys.EndBranch || key == Keys.EndSub;
				if (isEndOfRange && value.Value == null) // range value is keeped in a corresponding Start field
				{
					if (key == Keys.EndAccount) value.Value = ds[Keys.StartAccount];
					if (key == Keys.EndBranch) value.Value = ds[Keys.StartBranch];
					if (key == Keys.EndSub) value.Value = ds[Keys.StartSub];
				}

				string range = value.Value as string;
				if (range != null)
				{
					if (isStartOfRange || isEndOfRange)
					{
						string start, end;
						RMReportRange.ParseRangeStartEndPair(range, out start, out end);
						value.Value = isStartOfRange ? start : end;
					}
				}
				return true;
			}
			return false;
		}

		[PXOverride]
        public ARmDataSet MergeDataSet(IEnumerable<ARmDataSet> list, string expand, MergingMode mode, Func<IEnumerable<ARmDataSet>, string, MergingMode, ARmDataSet> del)
        {
			ARmDataSet dataSet = del(list, expand, mode);

            foreach (ARmDataSet set in list)
            {
                if (set == null) continue;

                if (string.IsNullOrEmpty(dataSet[Keys.BookCode] as string ?? "")) dataSet[Keys.BookCode] = set[Keys.BookCode] as string ?? "";
                if (!(dataSet[Keys.StartOffset] as int?).HasValue) dataSet[Keys.StartOffset] = (int?)set[Keys.StartOffset];
                if (!(dataSet[Keys.StartYearOffset] as int?).HasValue) dataSet[Keys.StartYearOffset] = (int?)set[Keys.StartYearOffset];
                if (!(dataSet[Keys.EndOffset] as int?).HasValue) dataSet[Keys.EndOffset] = (int?)set[Keys.EndOffset];
                if (!(dataSet[Keys.EndYearOffset] as int?).HasValue) dataSet[Keys.EndYearOffset] = (int?)set[Keys.EndYearOffset];
                if ((dataSet[Keys.AmountType] as short? ?? 0) == (short?)BalanceType.NotSet) dataSet[Keys.AmountType] = set[Keys.AmountType];

                dataSet[Keys.StartPeriod] = Base.MergeMask(dataSet[Keys.StartPeriod] as string ?? "", set[Keys.StartPeriod] as string ?? "");
                dataSet[Keys.EndPeriod] = Base.MergeMask(dataSet[Keys.EndPeriod] as string ?? "", set[Keys.EndPeriod] as string ?? "");

				if (string.IsNullOrEmpty(dataSet[Keys.Organization] as string)) dataSet[Keys.Organization] = set[Keys.Organization] as string ?? "";
	            if (dataSet[Keys.UseMasterCalendar] == null) dataSet[Keys.UseMasterCalendar] = set[Keys.UseMasterCalendar];
				RMReportWildcard.ConcatenateRangeWithDataSet(dataSet, set, Keys.StartBranch, Keys.EndBranch, mode);
				RMReportWildcard.ConcatenateRangeWithDataSet(dataSet, set, Keys.StartAccount, Keys.EndAccount, mode);
				RMReportWildcard.ConcatenateRangeWithDataSet(dataSet, set, Keys.StartSub, Keys.EndSub, mode);
                if (string.IsNullOrEmpty(dataSet[Keys.AccountClass] as string ?? "")) dataSet[Keys.AccountClass] = set[Keys.AccountClass] as string ?? "";
                if (string.IsNullOrEmpty(dataSet.RowDescription)) dataSet.RowDescription = set.RowDescription;
            }

            dataSet.Expand = list.First().Expand;
			if (dataSet.Expand == ExpandType.Account && string.IsNullOrEmpty(dataSet[Keys.StartAccount] as string ?? "") && string.IsNullOrEmpty(dataSet[Keys.AccountClass] as string ?? ""))
				dataSet.Expand = ExpandType.Nothing;

            return dataSet;
        }

        [PXOverride]
        public virtual List<ARmUnit> ExpandUnit(RMDataSource ds, ARmUnit unit, Func<RMDataSource, ARmUnit, List<ARmUnit>> del)
        {
            if (unit.DataSet.Expand == ExpandType.Account)
            {
                GLEnsureInitialized();
                return RMReportUnitExpansion<Account>.ExpandUnit(Base, ds, unit, Keys.StartAccount, Keys.EndAccount, 
		            GetItemsInRange<Account>,
                    account => account.AccountCD, account => account.Description, 
                    (account, wildcard) => { account.AccountCD = wildcard; account.Description = wildcard; });
            }

            if (unit.DataSet.Expand == ExpandType.Sub)
            {
                GLEnsureInitialized();
                return RMReportUnitExpansion<Sub>.ExpandUnit(Base, ds, unit, Keys.StartSub, Keys.EndSub, 
		            GetItemsInRange<Sub>,
                    sub => sub.SubCD, sub => sub.Description,
                    (sub, wildcard) => { sub.SubCD = wildcard; sub.Description = wildcard; });
            }

                return del(ds, unit);
            }

      
		[PXOverride]
		public void FillDataSource(RMDataSource ds, ARmDataSet dst, string rmType, Action<RMDataSource, ARmDataSet, string> del)
		{
			del(ds, dst, rmType);
			FillDataSourceInternal(ds, dst, rmType);
		}

		private void FillDataSourceInternal(RMDataSource ds, ARmDataSet dst, string rmType)
		{
			if (ds != null && ds.DataSourceID != null)
			{
				RMDataSourceGL dsGL = Base.Caches[typeof(RMDataSource)].GetExtension<RMDataSourceGL>(ds);
				dst[Keys.AmountType] = ds.AmountType;
				dst[Keys.StartBranch] = dsGL.StartBranch;
				dst[Keys.EndBranch] = dsGL.EndBranch;

				if (rmType == ARmReport.GL)
				{
					#region LedgerID
					object ledger = Base.DataSourceByID.Cache.GetValueExt(ds, "LedgerID");
					if (ledger != null)
					{
						if (ledger is PXFieldState)
						{
							ledger = ((PXFieldState)ledger).Value;
							if (ledger is string)
							{
								dst[Keys.BookCode] = (string)ledger;
							}
							else if (ledger != null)
							{
								throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, "LedgerID"));
							}
						}
						else if (ledger is string)
						{
							dst[Keys.BookCode] = (string)ledger;
						}
						else
						{
							throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, "LedgerID"));
						}
					}
					#endregion
					#region OrganizationID
					object organization = Base.DataSourceByID.Cache.GetValueExt<RMDataSourceGL.organizationID>(ds);
					string FiledName = string.Empty;
					if (organization != null)
					{
						if (organization is PXFieldState)
						{
							FiledName = ((PXFieldState)organization).Name;
							organization = ((PXFieldState)organization).Value;
							if (organization is string)
							{
								dst[Keys.Organization] = (string)organization;
							}
							else if (organization != null)
							{
								throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, FiledName));
							}
						}
						else if (organization is string)
						{
							dst[Keys.Organization] = (string)organization;
						}
						else
						{
							throw new PXSetPropertyException(PXMessages.LocalizeFormat(ErrorMessages.ElementDoesntExist, FiledName));
						}
					}
					#endregion
					dst[Keys.UseMasterCalendar] = dsGL.UseMasterCalendar;
					dst[Keys.EndAccount] = dsGL.EndAccount;
					dst[Keys.EndSub] = dsGL.EndSub;
					dst[Keys.StartAccount] = dsGL.StartAccount;
					dst[Keys.StartSub] = dsGL.StartSub;
					dst[Keys.AccountClass] = dsGL.AccountClassID;

				}
				dst.Expand = ds.Expand;
				
				dst.RowDescription = ds.RowDescription;
				dst[Keys.EndOffset] = (int?)dsGL.EndPeriodOffset;
				dst[Keys.EndYearOffset] = (int?)dsGL.EndPeriodYearOffset;
				string per = dsGL.EndPeriod;

				if (!String.IsNullOrEmpty(per))
				{
					dst[Keys.EndPeriod] = (per.Length > 4 ? (per.Substring(4) + "  ").Substring(0, 2) : "  ") + (per.Length > 4 ? per.Substring(0, 4) : per);
				}

				dst[Keys.StartOffset] = (int?)dsGL.StartPeriodOffset;
				dst[Keys.StartYearOffset] = (int?)dsGL.StartPeriodYearOffset;
				per = dsGL.StartPeriod;

				if (!String.IsNullOrEmpty(per))
				{
					dst[Keys.StartPeriod] = (per.Length > 4 ? (per.Substring(4) + "  ").Substring(0, 2) : "  ") + (per.Length > 4 ? per.Substring(0, 4) : per);
				}
			}
		}

		[PXOverride]
		public ARmReport GetReport(Func<ARmReport> del)
		{
			ARmReport ar = del();

			int? id = Base.Report.Current.StyleID;
			if (id != null)
			{
				RMStyle st = Base.StyleByID.SelectSingle(id);
				Base.fillStyle(st, ar.Style);
			}

			id = Base.Report.Current.DataSourceID;
			if (id != null)
			{
				RMDataSource ds = Base.DataSourceByID.SelectSingle(id);
				FillDataSourceInternal(ds, ar.DataSet, ar.Type);
			}

			List<ARmReport.ARmReportParameter> aRp = ar.ARmParams;
			PXFieldState state;
			RMReportPM rPM = Base.Report.Cache.GetExtension<RMReportPM>(Base.Report.Current);
			string sViewName = string.Empty;
			string sInputMask = string.Empty;
			string sFieldName = string.Empty;

			int colSpan = 2;
			if (ar.Type == ARmReport.GL)
			{
				RMReportGL rGL = Base.Report.Cache.GetExtension<RMReportGL>(Base.Report.Current);
				//OrganizationID
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourceGL.organizationID>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					sFieldName = state.Name;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.Organization, sFieldName, Messages.GetLocal(Messages.Company), ar.DataSet[Keys.Organization] as string, rGL.RequestOrganizationID ?? false, colSpan, sViewName, sInputMask, aRp);
				//UseMasterCalendar
				if (PXAccess.FeatureInstalled<FeaturesSet.multipleCalendarsSupport>())
				{
					state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourceGL.useMasterCalendar>(null) as PXFieldState;
					if (state != null)
					{
						sFieldName = state.Name;
						if (state.Value == null) state.Value = true;
						Base.CreateParameter(Keys.UseMasterCalendar, sFieldName,
							Messages.GetLocal(Messages.UseMasterCalendar),
							ar.DataSet[Keys.UseMasterCalendar] as bool? ?? (bool) state.Value,
							rGL.RequestUseMasterCalendar ?? false, colSpan, null, null, aRp);
					}
				}

				//StartBranch
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourceGL.startBranch>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					sFieldName = state.Name;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.StartBranch, sFieldName, Messages.GetLocal(Messages.StartBranchTitle), ar.DataSet[Keys.StartBranch] as string, rGL.RequestStartBranch ?? false, colSpan, sViewName, sInputMask, aRp);

				//EndBranch
				state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourceGL.endBranch>(null) as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					sFieldName = state.Name;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.EndBranch, sFieldName, Messages.GetLocal(Messages.EndBranchTitle), ar.DataSet[Keys.EndBranch] as string, rGL.RequestEndBranch ?? false, colSpan, sViewName, sInputMask, aRp);
			}

			bool bSinglePeriod = false;
			bool bRequestEndPeriod = false;
			short? endperreq = rPM.RequestEndPeriod ?? 0;
			if (endperreq > 0)
			{
				bRequestEndPeriod = true;
				if (endperreq == 2)
				{
					bSinglePeriod = true;
				}
			}

			bool bRequestStartPeriod = rPM.RequestStartPeriod ?? false;

			if (bRequestStartPeriod && (ar.DataSet[RMReportReaderGL.Keys.StartPeriod] as string ?? "").TrimEnd() == "")
			{
				try
				{
					ar.DataSet[RMReportReaderGL.Keys.StartPeriod] = (string)((ArmDATA)Base.GetExprContext()).GetDefExt("RowBatch.FinPeriodID");
				}
				catch
				{
				}
			}
			if (bRequestEndPeriod && (ar.DataSet[RMReportReaderGL.Keys.EndPeriod] as string ?? "").TrimEnd() == "")
			{
				try
				{
					ar.DataSet[RMReportReaderGL.Keys.EndPeriod] = (string)((ArmDATA)Base.GetExprContext()).GetDefExt("RowBatch.FinPeriodID");
				}
				catch
				{
				}
			}

			string sViewNameStartEndPeriod = string.Empty;
			string sInputMaskStartEndPeriod = string.Empty;

			state = Base.DataSourceByID.Cache.GetStateExt<RMDataSourceGL.startPeriod>(null) as PXFieldState;
			if (state != null && !String.IsNullOrEmpty(state.ViewName))
			{
				sViewNameStartEndPeriod = state.ViewName;
				if (state is PXStringState)
				{
					sInputMaskStartEndPeriod = ((PXStringState)state).InputMask;
				}
			}

			if (ar.Type == ARmReport.GL)
			{
				RMReportGL rGL = Base.Report.Cache.GetExtension<RMReportGL>(Base.Report.Current);

				//BookCode
				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt(null, "LedgerID") as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				Base.CreateParameter(Keys.BookCode, "BookCode", Messages.GetLocal(Messages.BookCodeTitle), ar.DataSet[Keys.BookCode] as string, rGL.RequestLedgerID ?? false, 2, sViewName, sInputMask, aRp);

				//StartPeriod, EndPeriod 																		
				bool endPeriod = (!bSinglePeriod && bRequestEndPeriod);
				string startLabel = endPeriod ? Messages.StartPeriodTitle : Messages.PeriodTitle;
				//colSpan = endPeriod ? 1 : 2;
				colSpan = 2;
				if (endPeriod)
				{
					Base.CreateParameter(Keys.StartPeriod, "StartPeriod", Messages.GetLocal(startLabel), ar.DataSet[Keys.StartPeriod] as string, bRequestStartPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
					Base.CreateParameter(Keys.EndPeriod, "EndPeriod", Messages.GetLocal(Messages.EndPeriodTitle), ar.DataSet[Keys.EndPeriod] as string, bRequestEndPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
				}
				else
				{
					Base.CreateParameter(new object[] { Keys.StartPeriod, Keys.EndPeriod }, "StartPeriod", Messages.GetLocal(startLabel), ar.DataSet[Keys.StartPeriod] as string, bRequestStartPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
				}

				// StartAccount, EndAccount
				bool bRequestEndAccount = rGL.RequestEndAccount ?? false;
				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt(null, "StartAccount") as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				//colSpan = bRequestEndAccount ? 1 : 2;
				colSpan = 2;
				Base.CreateParameter(Keys.StartAccount, "StartAccount", Messages.GetLocal(Messages.StartAccTitle), ar.DataSet[Keys.StartAccount] as string, rGL.RequestStartAccount ?? false, colSpan, sViewName, sInputMask, aRp);
				Base.CreateParameter(Keys.EndAccount, "EndAccount", Messages.GetLocal(Messages.EndAccTitle), ar.DataSet[Keys.EndAccount] as string, bRequestEndAccount, colSpan, sViewName, sInputMask, aRp);

				// StartSub, EndSub
				bool bRequestEndSub = rGL.RequestEndSub ?? false;
				sViewName = sInputMask = string.Empty;
				state = Base.Report.Cache.GetStateExt(null, "SubCD") as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				//colSpan = bRequestEndSub ? 1 : 2;
				colSpan = 2;
				Base.CreateParameter(Keys.StartSub, "StartSub", Messages.GetLocal(Messages.StartSubTitle), ar.DataSet[Keys.StartSub] as string, rGL.RequestStartSub ?? false, colSpan, sViewName, sInputMask, aRp);
				Base.CreateParameter(Keys.EndSub, "EndSub", Messages.GetLocal(Messages.EndSubTitle), ar.DataSet[Keys.EndSub] as string, bRequestEndSub, colSpan, sViewName, sInputMask, aRp);

				// AccountClass
				bool bRequestAccountClassID = rGL.RequestAccountClassID ?? false;
				sViewName = sInputMask = string.Empty;
				state = Base.DataSourceByID.Cache.GetStateExt(null, "AccountClassID") as PXFieldState;
				if (state != null && !String.IsNullOrEmpty(state.ViewName))
				{
					sViewName = state.ViewName;
					if (state is PXStringState)
					{
						sInputMask = ((PXStringState)state).InputMask;
					}
				}
				//colSpan = bRequestAccountClassID ? 1 : 2;
				colSpan = 2;
				Base.CreateParameter(Keys.AccountClass, "AccountClass", Messages.GetLocal(Messages.AccountClassTitle), ar.DataSet[Keys.AccountClass] as string, bRequestAccountClassID, colSpan, sViewName, sInputMask, aRp);
			}
			else
			{
				// StartPeriod, EndPeriod
				bool endPeriod = (!bSinglePeriod && bRequestEndPeriod);
				string startLabel = endPeriod ? Messages.StartPeriodTitle : Messages.PeriodTitle;
				//colSpan = endPeriod ? 1 : 2;
				colSpan = 2;
				if (endPeriod)
				{
					Base.CreateParameter(Keys.StartPeriod, "StartPeriod", Messages.GetLocal(startLabel), ar.DataSet[Keys.StartPeriod] as string, bRequestStartPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
					Base.CreateParameter(Keys.EndPeriod, "EndPeriod", Messages.GetLocal(Messages.EndPeriodTitle), ar.DataSet[Keys.EndPeriod] as string, bRequestEndPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
				}
				else
				{
					Base.CreateParameter(new object[] { Keys.StartPeriod, Keys.EndPeriod }, "StartPeriod", Messages.GetLocal(startLabel), ar.DataSet[Keys.StartPeriod] as string, bRequestStartPeriod, colSpan, sViewNameStartEndPeriod, sInputMaskStartEndPeriod, aRp);
				}
			}
			return ar;
		}

		#endregion
	}

	public class ArmDATA : PX.Data.Reports.SoapNavigator.DATA
	{
		public ArmDATA()
		{

		}
		public object FormatPeriod(object period)
		{
			if (period is string)
			{
				period = ((string)period).Replace("-", "");
			}
			return ExtToUI("RowBatch.FinPeriodID", period);
		}

		public object FormatPeriod(object period, object shift)
		{
			try
			{
				string periodString = ((string)period).Replace("-", "");
				periodString = String.Format("{0:0000}{1:00}", periodString.Substring(2), periodString.Substring(0, 2));

				string shiftedPeriodString = ShiftPeriod(FinPeriod.organizationID.MasterValue, periodString, shift);

				return FormatPeriod(shiftedPeriodString);
			}
			catch
			{
				return FormatPeriod(period);
			}
		}

		public object FormatPeriod(object period, object shiftYear, object shiftPeriod)
		{
			try
			{
				string periodString = ((string)period).Replace("-", "");
				//shift year
				int year = int.Parse(periodString.Substring(2));
				if (shiftYear != null && (int)shiftYear != 0)
				{
					year += (int)shiftYear;
				}
				periodString = String.Format("{0:0000}{1:00}", year.ToString(), periodString.Substring(0, 2));

				//shift period
				string shiftedPeriodString = ShiftPeriod(FinPeriod.organizationID.MasterValue, periodString, shiftPeriod);

				//Asked to no formatting
				return shiftedPeriodString;
			}
			catch
			{
				return FormatPeriod(period);
			}
		}		

		private string ShiftPeriod(int organizationId, string periodString, object shift)
		{
			int j = Convert.ToInt32(shift);

			PXSelectBase<FinPeriod> cmd = new PXSelect<FinPeriod, 
				Where<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>(_Graph);

			if (j < 0)
				cmd.WhereAnd<Where<FinPeriod.finPeriodID, Less<Required<FinPeriod.finPeriodID>>>>();
			else
				cmd.WhereAnd<Where<FinPeriod.finPeriodID, GreaterEqual<Required<FinPeriod.finPeriodID>>>>();

			cmd.OrderByNew<OrderBy<Asc<FinPeriod.finPeriodID>>>();

			FinPeriod shiftedFinPeriod = cmd.SelectWindowed(j, 1, organizationId, periodString);

			string shiftedPeriodString = String.Format("{0:00}{1:0000}", shiftedFinPeriod.FinPeriodID.Substring(4, 2), shiftedFinPeriod.FinPeriodID.Substring(0, 4));
			return shiftedPeriodString;
		}

		public object FormatYear(object period)
		{
			string per = FormatPeriod(period) as string;
			if (!String.IsNullOrEmpty(per))
			{
				int i = per.IndexOf("-");
				if (i >= 0 && i < per.Length - 1)
				{
					return per.Substring(i + 1);
				}
			}

			return per;
		}
		public object FormatYear(object period, object shift)
		{
			try
			{
				int j = Convert.ToInt32(shift);
				int i = int.Parse((string)FormatYear(period));
				return String.Format("{0:0000}", i + j);
			}
			catch
			{
				return FormatYear(period);
			}
		}

		public object GetNumberOfPeriods(object startPeriod, object endPeriod)
		{
			return GetNumberOfPeriods(FinPeriod.organizationID.MasterValue, startPeriod, endPeriod);
		}

		public object GetNumberOfPeriods(object organizationId, object startPeriod, object endPeriod)
		{
			try
			{
				organizationId = CorrectOrganizationId(organizationId);
				IEnumerable<PXDataRecord> record = PXDatabase.SelectMulti<FinPeriod>(
						new PXDataField<FinPeriod.finPeriodID>(),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, FormatForStoring(startPeriod as string), PXComp.GE),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, FormatForStoring(endPeriod as string), PXComp.LE),
						new PXDataFieldValue<FinPeriod.organizationID>(PXDbType.Int, 4, organizationId),
						new PXDataFieldOrder<FinPeriod.finPeriodID>());
				{
					if (record != null)
						return record.Count<PXDataRecord>() - 1;
				}
			}
			catch
			{ }
			return null;
		}

		public object GetPeriodStartDate(object period)
		{
			return GetPeriodStartDate(FinPeriod.organizationID.MasterValue, period);
		}
		public object GetPeriodStartDate(object organizationId, object period)
		{
			try
			{
				var p = FormatForStoring(period as string);
				organizationId = CorrectOrganizationId(organizationId);
				if (p != null)
				{
					using (PXDataRecord record = PXDatabase.SelectSingle<FinPeriod>(
						new PXDataField<FinPeriod.startDate>(),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, p),
						new PXDataFieldValue<FinPeriod.organizationID>(PXDbType.Int, 4, organizationId)
						))
					{
						if (record != null)
						{
							return record.GetDateTime(0);
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}

		private object CorrectOrganizationId(object organizationId) 
		{
			if (organizationId is string)
			{
				organizationId = PXAccess.GetOrganizationID(((string)organizationId)?.Trim());
			}
			return organizationId;
		}

		public object GetPeriodEndDate(object period)
		{
			return GetPeriodEndDate(FinPeriod.organizationID.MasterValue, period);
		}
		public object GetPeriodEndDate(object organizationId, object period)
		{
			try
			{
				var p = FormatForStoring(period as string);
				organizationId = CorrectOrganizationId(organizationId);
				if (p != null)
				{
					using (PXDataRecord record = PXDatabase.SelectSingle<FinPeriod>(
						new PXDataField<FinPeriod.endDate>(),
						new PXDataField<FinPeriod.startDate>(),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, p),
						new PXDataFieldValue<FinPeriod.organizationID>(PXDbType.Int, 4, organizationId)
						))
					{
						if (record != null)
						{
                            return ((record.GetDateTime(0).HasValue) ? record.GetDateTime(0).Value.AddDays(-1) : record.GetDateTime(0));
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}

		public object GetPeriodDescription(object period)
		{
			return GetPeriodDescription(FinPeriod.organizationID.MasterValue, period);
		}
		public object GetPeriodDescription(object organizationId, object period)
		{
			try
			{
				var p = FormatForStoring(period as string);
				organizationId = CorrectOrganizationId(organizationId);
				if (p != null)
				{
					using (PXDataRecord record = PXDatabase.SelectSingle<FinPeriod>(
						new PXDataField<FinPeriod.descr>(),
						new PXDataFieldValue<FinPeriod.finPeriodID>(PXDbType.Char, 6, p),
						new PXDataFieldValue<FinPeriod.organizationID>(PXDbType.Int, 4, organizationId)
						))
					{
						if (record != null)
						{
							return record.GetString(0);
						}
					}
				}
			}
			catch
			{
			}
			return null;
		}
		private static string FormatForStoring(string period)
		{
			if (string.IsNullOrEmpty(period))
				return null;

			period = period.Replace("-", "");

			if (period.Trim().Length != FinPeriodUtils.PERIOD_LENGTH + FinPeriodUtils.YEAR_LENGTH)
				return period;

			return period.Substring(FinPeriodUtils.PERIOD_LENGTH, FinPeriodUtils.YEAR_LENGTH) + period.Substring(0, FinPeriodUtils.PERIOD_LENGTH);
		}

		public object GetBranchText(object branchCode)
		{
			try
			{
				string code = branchCode as string;
				if (!String.IsNullOrEmpty(code))
				{
					using (PXDataRecord record = PXDatabase.SelectSingle<Branch>(
						Yaql.join<BAccount>(Yaql.eq<BAccount.bAccountID, Branch.bAccountID>()),
						new PXDataField<BAccount.acctName>(),
						new PXDataFieldValue<Branch.branchCD>(code)))
					{
						return record == null ? null : record.GetString(0);
					}
				}
			}
			catch (Exception ex)
			{
				PXTrace.WriteError(ex);
			}
			return null;
		}
	}

    public struct GLHistoryKeyTuple
    {
        public readonly int LedgerID;
        public readonly int BranchID;
        public readonly int AccountID;
        public readonly int SubID;

        public GLHistoryKeyTuple(int ledgerID, int branchID, int accountID, int subID)
        {
            LedgerID = ledgerID;
            BranchID = branchID;
            AccountID = accountID;
            SubID = subID;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + LedgerID.GetHashCode();
                hash = hash * 23 + BranchID.GetHashCode();
                hash = hash * 23 + AccountID.GetHashCode();
                hash = hash * 23 + SubID.GetHashCode();
                return hash;
            }
        }
    }
}
