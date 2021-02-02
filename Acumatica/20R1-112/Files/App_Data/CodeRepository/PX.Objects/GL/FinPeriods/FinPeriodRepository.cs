using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Objects.Common.Extensions;
using PX.Objects.GL.FinPeriods.TableDefinition;
using PX.Objects.GL.Exceptions;
using PX.Objects.Common;

namespace PX.Objects.GL.FinPeriods
{
	public class FinPeriodRepository : IFinPeriodRepository
	{
        public class FinPeriodKeysByMasterAndOrganizationIDCachedCollection
        {
            public Dictionary<int, string> OrganizationFinPeriodByOrgAndMaster { get; set; }

            public void Load()
            {
                PXGraph graph = PXGraph.CreateInstance<PXGraph>();

                PXSelectBase<FinPeriod> getPeriodCmd = new PXSelectReadonly<FinPeriod,
                                        Where<FinPeriod.organizationID, NotEqual<FinPeriod.organizationID.masterValue>>>(graph);

                using (new PXFieldScope(getPeriodCmd.View,
                    typeof(FinPeriod.organizationID),
                    typeof(FinPeriod.finPeriodID),
                    typeof(FinPeriod.masterFinPeriodID)))
                {
                    OrganizationFinPeriodByOrgAndMaster = getPeriodCmd.Select().AsEnumerable()
                        .Select(period => ((FinPeriod) period))
                        .ToDictionary(period => CalcCollectionKey(period.OrganizationID, period.MasterFinPeriodID), period => period.FinPeriodID);
                }
            }

            public virtual string GetPeriodByMaster(int? organizationID, string masterFinPeriodID)
            {
                if (organizationID == null || masterFinPeriodID == null)
                    return null;

                string finPeriodID;

                if (OrganizationFinPeriodByOrgAndMaster.TryGetValue(CalcCollectionKey(organizationID, masterFinPeriodID), out finPeriodID))
                {
                    return finPeriodID;
                }

                return null;
            }

            public virtual int CalcCollectionKey(int? organizationID, string masterFinPeriodID)
            {
                return organizationID.Value * 10000 + Convert.ToInt32(masterFinPeriodID);
            }
        }

	    public static FinPeriodKeysByMasterAndOrganizationIDCachedCollection FinPeriodKeysByMasterAndOrganizationID
	    {
	        get
	        {
	            var collection = PXContext.GetSlot<FinPeriodKeysByMasterAndOrganizationIDCachedCollection>();

                if (collection == null)
	            {
	                collection = new FinPeriodKeysByMasterAndOrganizationIDCachedCollection();

	                collection.Load();

	                PXContext.SetSlot<FinPeriodKeysByMasterAndOrganizationIDCachedCollection>(collection);
	            }

	            return collection;
	        }
	    }

        protected readonly PXGraph Graph;

		public FinPeriodRepository(PXGraph graph)
		{
			Graph = graph;
		}

		public int? GetCalendarOrganizationID(int? organizationID, int? branchID, bool? useMasterCalendar)
		{
		    if (useMasterCalendar == true || (organizationID == null && branchID == null))
		    {
		        return FinPeriod.organizationID.MasterValue;
		    }
		    else
			{
				if (branchID != null)
					return PXAccess.GetParentOrganizationID(branchID);

				return organizationID;
			}
		}

		public int? GetCalendarOrganizationID(int? branchID, bool? useMasterCalendar)
		{
			if (useMasterCalendar == true || branchID == null)
			{
				return FinPeriod.organizationID.MasterValue;
			}
			else
			{
				return PXAccess.GetParentOrganizationID(branchID);
			}
		}

		public FinPeriod FindMaxFinPeriodWithEndDataBelongToInterval(DateTime? startDate, DateTime? endDate, int? organizationID)
		{
			return PXSelect<FinPeriod,
					Where<FinPeriod.finDate, GreaterEqual<Required<FinPeriod.finDate>>,
						And<FinPeriod.finDate, Less<Required<FinPeriod.finDate>>,
						And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>,
					OrderBy<Desc<FinPeriod.finPeriodID>>>
				.Select(Graph, startDate, endDate, organizationID);
		}

		/// <summary>
		/// Returns PeriodID from the given date.
		/// </summary>
		public string GetPeriodIDFromDate(DateTime? date, int? organizationID)
		{
			FinPeriod period = GetFinPeriodByDate(date, organizationID);

			return period.FinPeriodID;
		}

		public FinPeriod GetFinPeriodByDate(DateTime? date, int? organizationID)
		{
			FinPeriod finPeriod = FindFinPeriodByDate(date, organizationID);

			if (finPeriod == null)
			{
				throw new FinancialPeriodNotDefinedForDateException(date);
			}

			return finPeriod;
		}

		public FinPeriod FindFinPeriodByDate(DateTime? date, int? organizationID)
		{
			return PXSelect<
					FinPeriod,
					Where<
						FinPeriod.startDate, LessEqual<Required<FinPeriod.startDate>>,
						And<FinPeriod.endDate, Greater<Required<FinPeriod.endDate>>,
							And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>>
				.Select(Graph, date, date, organizationID);
		}

		public string GetOffsetPeriodId(string finPeriodID, int offset, int? organizationID)
		{
			string offsetPeriod = FindOffsetPeriodId(finPeriodID, offset, organizationID);

			if (offsetPeriod == null)
			{
				throw new FinancialPeriodOffsetNotFoundException(finPeriodID, offset);
			}

			return offsetPeriod;
		}

		public FinPeriod GetOffsetPeriod(string finPeriodID, int offset, int? organizationID)
		{
			FinPeriod offsetPeriod = FindOffsetPeriod(finPeriodID, offset, organizationID);

			if (offsetPeriod == null)
			{
				throw new FinancialPeriodOffsetNotFoundException(finPeriodID, offset);
			}

			return offsetPeriod;
		}

		public string FindOffsetPeriodId(string finPeriodID, int offset, int? organizationID)
		{
			FinYearSetup setup = PXSelect<FinYearSetup>.Select(Graph);

			//TODO: Need to refactor, duplicates the part of function FABookPeriodIDAttribute.GetBookPeriodsInYear
			FinPeriodSetup periodsInYear = PXSelectGroupBy<FinPeriodSetup, Where<FinPeriodSetup.endDate, Greater<FinPeriodSetup.startDate>>,
				Aggregate<Max<FinPeriodSetup.periodNbr>>>.Select(Graph);
			if (setup != null && FiscalPeriodSetupCreator.IsFixedLengthPeriod(setup.FPType) &&
				periodsInYear != null && periodsInYear.PeriodNbr != null)
			{
				return FinPeriodUtils.OffsetPeriod(finPeriodID, offset, Convert.ToInt32(periodsInYear.PeriodNbr));
			}
			else if (offset > 0)
			{
				PXResultset<FinPeriod> res = PXSelect<
						FinPeriod,
						Where<
							FinPeriod.finPeriodID, Greater<Required<FinPeriod.finPeriodID>>,
							And<FinPeriod.startDate, NotEqual<FinPeriod.endDate>,
							And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>,
						OrderBy<
							Asc<FinPeriod.finPeriodID>>>
					.SelectWindowed(Graph, 0, offset, finPeriodID, organizationID);

				if (res.Count < offset)
				{
					return null;
				}

				return ((FinPeriod)res[res.Count - 1]).FinPeriodID;
			}
			else if (offset < 0)
			{
				PXResultset<FinPeriod> res = PXSelect<
						FinPeriod,
						Where<
							FinPeriod.finPeriodID, Less<Required<FinPeriod.finPeriodID>>,
							And<FinPeriod.startDate, NotEqual<FinPeriod.endDate>,
							And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>,
						OrderBy<
							Desc<FinPeriod.finPeriodID>>>
					.SelectWindowed(Graph, 0, -offset, finPeriodID, organizationID);

				if (res.Count < -offset)
				{
					return null;
				}

				return ((FinPeriod)res[res.Count - 1]).FinPeriodID;
			}
			else
			{
				return finPeriodID;
			}
		}

		public FinPeriod FindOffsetPeriod(string finPeriodID, int offset, int? organizationID)
		{
			FinYearSetup setup = PXSelect<FinYearSetup>.Select(Graph);

			//TODO: Need to refactor, duplicates the part of function FABookPeriodIDAttribute.GetBookPeriodsInYear
			FinPeriodSetup periodsInYear = PXSelectGroupBy<FinPeriodSetup, Where<FinPeriodSetup.endDate, Greater<FinPeriodSetup.startDate>>,
				Aggregate<Max<FinPeriodSetup.periodNbr>>>.Select(Graph);
			if (setup != null && FiscalPeriodSetupCreator.IsFixedLengthPeriod(setup.FPType) &&
				periodsInYear != null && periodsInYear.PeriodNbr != null)
			{
				string offsetFinPeriodID = FinPeriodUtils.OffsetPeriod(finPeriodID, offset, Convert.ToInt32(periodsInYear.PeriodNbr));
				return FindByID(organizationID, offsetFinPeriodID);
			}
			else if (offset > 0)
			{
				PXResultset<FinPeriod> res = PXSelect<
						FinPeriod,
						Where<
							FinPeriod.finPeriodID, Greater<Required<FinPeriod.finPeriodID>>,
							And<FinPeriod.startDate, NotEqual<FinPeriod.endDate>,
							And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>,
						OrderBy<
							Asc<FinPeriod.finPeriodID>>>
					.SelectWindowed(Graph, 0, offset, finPeriodID, organizationID);

				if (res.Count < offset)
				{
					return null;
				}

				return (FinPeriod)res[res.Count - 1];
			}
			else if (offset < 0)
			{
				PXResultset<FinPeriod> res = PXSelect<
						FinPeriod,
						Where<
							FinPeriod.finPeriodID, Less<Required<FinPeriod.finPeriodID>>,
							And<FinPeriod.startDate, NotEqual<FinPeriod.endDate>,
							And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>,
						OrderBy<
							Desc<FinPeriod.finPeriodID>>>
					.SelectWindowed(Graph, 0, -offset, finPeriodID, organizationID);

				if (res.Count < -offset)
				{
					return null;
				}

				return (FinPeriod)res[res.Count - 1];
			}
			else
			{
				return FindByID(organizationID, finPeriodID);
			}
		}

		/// <summary>
		/// Returns Next Period from the given.
		/// </summary>
		public string NextPeriod(string finPeriodID, int? organizationID)
		{
			return GetOffsetPeriodId(finPeriodID, 1, organizationID);
		}

		/// <summary>
		/// Returns Start date for the given Period
		/// </summary>
		public DateTime PeriodStartDate(string finPeriodID, int? organizationID)
		{
			FinPeriod financialPeriod = PXSelect<
					FinPeriod,
					Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>,
						And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>
				.Select(Graph, finPeriodID, organizationID);

			if (financialPeriod == null)
			{
				throw new FinancialPeriodWithIdNotFoundException(finPeriodID);
			}

			return (DateTime)financialPeriod.StartDate;
		}

		/// <summary>
		/// Returns End date for the given period
		/// </summary>
		public DateTime PeriodEndDate(string finPeriodID, int? organizationID)
		{
			FinPeriod financialPeriod = PXSelect<
					FinPeriod,
					Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>,
						And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>
				.Select(Graph, finPeriodID, organizationID);

			if (financialPeriod == null)
			{
				throw new FinancialPeriodWithIdNotFoundException(finPeriodID);
			}

			return financialPeriod.EndDate.Value.AddDays(-1);
		}

		public IEnumerable<FinPeriod> GetFinPeriodsInInterval(DateTime? fromDate, DateTime? tillDate, int? organizationID)
		{
			return PXSelect<FinPeriod,
							Where<FinPeriod.startDate, GreaterEqual<Required<FinPeriod.startDate>>,
								And<FinPeriod.endDate, LessEqual<Required<FinPeriod.endDate>>,
								And<FinPeriod.startDate, NotEqual<FinPeriod.endDate>,
								And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>>>
							.Select(Graph, fromDate, tillDate, organizationID)
							.RowCast<FinPeriod>();
		}

		public IEnumerable<FinPeriod> GetAdjustmentFinPeriods(string finYear, int? organizationID)
		{
			return PXSelect<FinPeriod,
							Where<FinPeriod.finYear, Equal<Required<FinPeriod.finYear>>,
								And<FinPeriod.startDate, Equal<FinPeriod.endDate>,
								And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>>
							.Select(Graph, finYear, organizationID)
							.RowCast<FinPeriod>();
		}

		public FinPeriod FindLastYearNotAdjustmentPeriod(string finYear, int? organizationID)
		{
			return (FinPeriod)PXSelect<FinPeriod,
					Where<FinPeriod.finYear, Equal<Required<FinPeriod.finYear>>,
						And<FinPeriod.startDate, NotEqual<FinPeriod.endDate>,
						And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>,
					OrderBy<Desc<FinPeriod.finPeriodID>>>
				.SelectWindowed(Graph, 0, 1, finYear, organizationID);
		}

		public FinPeriod FindLastFinancialPeriodOfYear(string finYear, int? organizationID)
			=> PXSelect<
					FinPeriod,
					Where<FinPeriod.finYear, Equal<Required<FinPeriod.finYear>>,
						And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>,
					OrderBy<Desc<FinPeriod.finPeriodID>>>
					.SelectWindowed(Graph, 0, 1, finYear, organizationID);


		public OrganizationFinPeriod FindLastNonAdjustmentOrganizationFinPeriodOfYear(int? organizationID, string finYear, bool clearQueryCache = false)
		{
			PXSelectBase<OrganizationFinPeriod> select = new PXSelectReadonly<
				OrganizationFinPeriod,
				Where<OrganizationFinPeriod.finYear, Equal<Required<OrganizationFinPeriod.finYear>>,
					And<OrganizationFinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>,
					And<OrganizationFinPeriod.startDate, NotEqual<OrganizationFinPeriod.endDate>>>>,
				OrderBy<
					Desc<OrganizationFinPeriod.finPeriodID>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(finYear, organizationID) as OrganizationFinPeriod ;
		}

		/// <summary>
		/// Returns a minimal set of financial periods that contain a given date interval
		/// within them, excluding any adjustment periods.
		/// </summary>
		/// <param name="Graph">The Graph which will be used when performing a select DB query.</param>
		/// <param name="startDate">The starting date of the date interval.</param>
		/// <param name="endDate">The ending date of the date interval.</param>
		public IEnumerable<FinPeriod> PeriodsBetweenInclusive(DateTime startDate, DateTime endDate, int? organizationID)
		{
			if (startDate > endDate)
			{
				throw new PXArgumentException(nameof(startDate));
			}

			return PXSelect<
					FinPeriod,
					Where<
						FinPeriod.endDate, Greater<Required<FinPeriod.endDate>>,
						And<FinPeriod.startDate, LessEqual<Required<FinPeriod.startDate>>,
						And<FinPeriod.startDate, NotEqual<FinPeriod.endDate>,
						And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>>>
						.Select(Graph, startDate, endDate, organizationID)
						.RowCast<FinPeriod>();
		}

		public void CheckIsDateWithinPeriod(string finPeriodID, int? organizationID, DateTime date, string errorMessage, PXErrorLevel errorLevel)
		{
			if (!IsDateWithinPeriod(finPeriodID, date, organizationID))
			{
				throw new PXSetPropertyException(errorMessage, errorLevel);
			}
		}
		public void CheckIsDateWithinPeriod(string finPeriodID, int? organizationID, DateTime date, string errorMessage)
		{
			CheckIsDateWithinPeriod(finPeriodID, organizationID, date, errorMessage, PXErrorLevel.Error);
		}

		public bool IsDateWithinPeriod(string finPeriodID, DateTime date, int? organizationID)
		{
			FinPeriod finPeriod = GetByID(finPeriodID, organizationID);

			return date >= finPeriod.StartDate && date < finPeriod.EndDate;
		}

		public bool PeriodExists(string finPeriodID, int? organizationID)
		{
			FinPeriod finPeriod = FindByID(organizationID, finPeriodID);

			return finPeriod != null;
		}

		/// <summary>
		/// Gets the ID of the financial period with the same <see cref="FinPeriod.PeriodNbr"/> 
		/// as the one specified, but residing in the previous financial year. If no such financial 
		/// period exists, an exception is thrown.
		/// </summary>
		public string GetSamePeriodInPreviousYear(string finPeriodID, int? organizationID)
		{
			string yearPart = finPeriodID.Substring(0, 4);
			string periodNumber = finPeriodID.Substring(4, 2);

			string previousYear = (int.Parse(yearPart) - 1).ToString();
			string suggestedPeriodID = string.Concat(previousYear, periodNumber);

			if (!PeriodExists(suggestedPeriodID, organizationID))
			{
				throw new FinancialPeriodWithIdNotFoundException(suggestedPeriodID);
			}

			return suggestedPeriodID;
		}

		public FinPeriod GetByID(string finPeriodID, int? organizationID)
		{
			FinPeriod finPeriod = FindByID(organizationID, finPeriodID);

			if (finPeriod == null)
			{
				throw new PXException(Common.Messages.EntityWithIDDoesNotExist,
										EntityHelper.GetFriendlyEntityName(typeof(FinPeriod)),
										finPeriodID);
			}

			return finPeriod;
		}

		public FinPeriod FindByID(int? organizationID, string finPeriodID)
		{
			return PXSelect<FinPeriod,
						Where<FinPeriod.finPeriodID, Equal<Required<FinPeriod.finPeriodID>>,
							And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>>
						.Select(Graph, finPeriodID, organizationID);
		}

		public FinPeriod FindPrevPeriod(int? organizationID, string finPeriodID, bool looped = false)
		{
			FinPeriod nextperiod = null;
			if (!string.IsNullOrEmpty(finPeriodID))
			{
				nextperiod =
						PXSelect<FinPeriod,
							Where<FinPeriod.finPeriodID, Less<Required<FinPeriod.finPeriodID>>,
								And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>,
							OrderBy<Desc<FinPeriod.finPeriodID>>>
							.SelectWindowed(Graph, 0, 1, finPeriodID, organizationID);
			}
			if (looped && nextperiod == null)
			{
				nextperiod = FindFirstPeriod(organizationID);

			}
			return nextperiod;
		}

		public FinPeriod FindNextPeriod(int? organizationID, string finPeriodID, bool looped = false)
		{
			FinPeriod nextperiod = null;
			if (!string.IsNullOrEmpty(finPeriodID))
			{
				nextperiod =
						PXSelect<FinPeriod,
							Where<FinPeriod.finPeriodID, Greater<Required<FinPeriod.finPeriodID>>,
								And<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>,
							OrderBy<Asc<FinPeriod.finPeriodID>>>
							.SelectWindowed(Graph, 0, 1, finPeriodID, organizationID);
			}
			if (looped && nextperiod == null)
			{
				nextperiod = FindLastPeriod(organizationID);
			}
			return nextperiod;
		}

		public FinPeriod FindFirstPeriod(int? organizationID, bool clearQueryCache = false)
		{
			PXSelectBase<FinPeriod> select = new PXSelectReadonly<FinPeriod,
				Where<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>,
				OrderBy<Asc<FinPeriod.finPeriodID>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(organizationID) as FinPeriod;
		}

		public FinPeriod FindLastPeriod(int? organizationID, bool clearQueryCache = false)
		{
			PXSelectBase<FinPeriod> select = new PXSelectReadonly<FinPeriod,
				Where<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>,
				OrderBy<Desc<FinPeriod.finPeriodID>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(organizationID) as FinPeriod;
		}

		public FinYear FindFirstYear(int? organizationID, bool clearQueryCache = false)
		{
			PXSelectBase<FinYear> select = new PXSelectReadonly<
				FinYear,
				Where<FinYear.organizationID, Equal<Required<FinYear.organizationID>>>,
				OrderBy<
					Asc<FinYear.year>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(organizationID) as FinYear;
		}

		public FinYear FindLastYear(int? organizationID, bool clearQueryCache = false)
		{
			PXSelectBase<FinYear> select = new PXSelectReadonly<
				FinYear,
				Where<FinYear.organizationID, Equal<Required<FinYear.organizationID>>>,
				OrderBy<
					Desc<FinYear.year>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(organizationID) as FinYear;
		}

		public OrganizationFinPeriod FindFirstOpenFinPeriod(string fromFinPeriodID, int? organizationID, Type fieldModuleClosed = null)
		{
			bool canPostToClosedPeriod = PXContext.GetSlot<bool>("FinPeriodUtils.AllowPostToUnlockedPeriod") == true
						|| (((GLSetup)PXSelect<GLSetup>.Select(Graph)).RestrictAccessToClosedPeriods != true
						|| !string.IsNullOrEmpty(PredefinedRoles.FinancialSupervisor)
							&& System.Web.Security.Roles.IsUserInRole(PXAccess.GetUserName(), PredefinedRoles.FinancialSupervisor));

			BqlCommand select =
				BqlCommand.CreateInstance(typeof(Select<OrganizationFinPeriod,
					Where<OrganizationFinPeriod.finPeriodID, GreaterEqual<Required<OrganizationFinPeriod.finPeriodID>>,
						And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>,
					OrderBy<Asc<OrganizationFinPeriod.finPeriodID>>>));

			if (fieldModuleClosed != null)
			{
				select = select.WhereAnd(BqlCommand.Compose(typeof(Where<,>), fieldModuleClosed, typeof(NotEqual<True>)));
			}
			if (canPostToClosedPeriod == true)
			{
				select = select.WhereAnd(typeof(Where<OrganizationFinPeriod.status, Equal<FinPeriod.status.open>, Or<OrganizationFinPeriod.status, Equal<FinPeriod.status.closed>>>));
			}
			else
			{
				select = select.WhereAnd(typeof(Where<OrganizationFinPeriod.status, Equal<FinPeriod.status.open>>));
			}

			return (OrganizationFinPeriod)(new PXView(Graph, false, select).SelectSingle(fromFinPeriodID, organizationID));
		}

		public MasterFinYear FindMasterFinYearByID(string year, bool clearQueryCache = false)
		{
			PXSelectBase<MasterFinYear> select = new PXSelectReadonly<
				MasterFinYear,
				Where<MasterFinYear.year, Equal<Required<MasterFinYear.year>>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(year) as MasterFinYear;
		}

		public MasterFinPeriod FindMasterFinPeriodByID(string finPeriodID, bool clearQueryCache = false)
		{
			PXSelectBase<MasterFinPeriod> select = new PXSelectReadonly<
				MasterFinPeriod,
				Where<MasterFinPeriod.finPeriodID, Equal<Required<MasterFinPeriod.finPeriodID>>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(finPeriodID) as MasterFinPeriod;
		}
		public OrganizationFinYear FindOrganizationFinYearByID(int? organizationID, string year, bool clearQueryCache = false)
		{
			PXSelectBase<OrganizationFinYear> select = new PXSelectReadonly<
				OrganizationFinYear,
				Where<OrganizationFinYear.year, Equal<Required<OrganizationFinYear.year>>,
					And<OrganizationFinYear.organizationID, Equal<Required<OrganizationFinYear.organizationID>>>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(year, organizationID) as OrganizationFinYear;
		}

		public OrganizationFinPeriod FindOrganizationFinPeriodByID(int? organizationID, string finPeriodID, bool clearQueryCache = false)
		{
			PXSelectBase<OrganizationFinPeriod> select = new PXSelectReadonly<
				OrganizationFinPeriod,
				Where<OrganizationFinPeriod.finPeriodID, Equal<Required<OrganizationFinPeriod.finPeriodID>>,
					And<OrganizationFinPeriod.organizationID, Equal<Required<OrganizationFinPeriod.organizationID>>>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(finPeriodID, organizationID) as OrganizationFinPeriod;
		}

		public MasterFinPeriod FindNextNonAdjustmentMasterFinPeriod(string prevFinPeriodID, bool clearQueryCache = false)
		{
			PXSelectBase<MasterFinPeriod> select = new PXSelectReadonly<
				MasterFinPeriod,
				Where<MasterFinPeriod.finPeriodID, Greater<Required<MasterFinPeriod.finPeriodID>>,
					And<MasterFinPeriod.startDate, NotEqual<MasterFinPeriod.endDate>>>,
				OrderBy<
					Asc<MasterFinPeriod.finPeriodID>>>(Graph);
			if (clearQueryCache)
			{
				select.View.Clear();
			}
			return select.View.SelectSingle(prevFinPeriodID) as MasterFinPeriod;
		}

	    public FinPeriod GetMappedPeriod(int? organizationID1, string finPeriodID1, int? organizationID2)
	    {
	        FinPeriod oldFinPeriod = FindByID(organizationID1, finPeriodID1);

	        return GetFinPeriodByMasterPeriodID(organizationID2, oldFinPeriod?.MasterFinPeriodID).Result;
	    }

	    public virtual ProcessingResult<FinPeriod> GetFinPeriodByMasterPeriodID(int? organizationID, string masterFinPeriodID)
	    {
	        FinPeriod period = PXSelect<FinPeriod,
	                                Where<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>,
	                                    And<FinPeriod.masterFinPeriodID, Equal<Required<FinPeriod.masterFinPeriodID>>>>>
	                                .Select(Graph, organizationID, masterFinPeriodID);

	        var result = ProcessingResult<FinPeriod>.CreateSuccess(period);

            if (period == null)
	        {
	            string errorMessage = PXMessages.LocalizeFormatNoPrefix(
	                Messages.RelatedFinPeriodsForMasterDoesNotExistForCompany,
	                PeriodIDAttribute.FormatForError(masterFinPeriodID),
	                PXAccess.GetOrganizationCD(organizationID));

	            result.AddErrorMessage(errorMessage);
            }

	        return result;
	    }

	    public virtual string FindFinPeriodIDByMasterPeriodID(int? organizationID, string masterFinPeriodID, bool readAllAndCacheToPXContext = false)
	    {
	        if (readAllAndCacheToPXContext)
	        {
	            return FinPeriodKeysByMasterAndOrganizationID.GetPeriodByMaster(organizationID, masterFinPeriodID);
	        }
	        else
	        {
	            ProcessingResult<FinPeriod> result = GetFinPeriodByMasterPeriodID(organizationID, masterFinPeriodID);

	            if (result.IsSuccess)
	            {
	                return result.Result.FinPeriodID;
	            }

	            return null;
	        }
	    }

		public virtual string GetFinPeriodByBranchAndMasterPeriodID(int? branchId, string masterFinPeriod)
		{
			if (branchId == default(int?))
			{
				throw new ArgumentNullException($"{nameof(branchId)} cannot be null");
			}
			if (masterFinPeriod == null)
			{
				throw new ArgumentNullException($"{nameof(masterFinPeriod)} cannot be null");
			}

			var orgId = PXAccess.GetParentOrganizationID(branchId);
			var getFinPeriodProcess = GetFinPeriodByMasterPeriodID(orgId, masterFinPeriod);
			if (getFinPeriodProcess.IsSuccess)
			{
				return getFinPeriodProcess.Result.FinPeriodID;
			}
			else
			{
				throw new PXException(getFinPeriodProcess.GeneralMessage);
			}
		}

        public ProcessingResult FinPeriodsForMasterExist(string masterFinPeriodID, int?[] organizationIDs)
	    {
	        List<FinPeriod> finPeriods =
	            PXSelect<FinPeriod,
	                    Where<FinPeriod.masterFinPeriodID, Equal<Required<FinPeriod.masterFinPeriodID>>,
	                        And<FinPeriod.organizationID, In<Required<FinPeriod.organizationID>>>>>
	                .Select(Graph, masterFinPeriodID, organizationIDs)
	                .RowCast<FinPeriod>()
	                .ToList();

	        ProcessingResult validationResult = new ProcessingResult();

	        if (finPeriods.Count != organizationIDs.Length)
	        {
	            IEnumerable<int?> unexistingForOrganizationIDs =
	                organizationIDs.Except(finPeriods.Select(period => period.OrganizationID));

	            validationResult.AddMessage(
	                PXErrorLevel.Error,
	                Messages.RelatedFinPeriodsForMasterDoesNotExistForCompanies,
	                FinPeriodIDFormattingAttribute.FormatForError(masterFinPeriodID),
	                unexistingForOrganizationIDs.Select(PXAccess.GetOrganizationCD).OrderBy(v => v).ToArray().JoinIntoStringForMessageNoQuotes(20));
	        }

	        return validationResult;
	    }

		public OrganizationFinYear FindNearestOrganizationFinYear(int? organizationID, string yearNumber, bool clearQueryCache = false, bool mergeCache = false)
		{
			BqlCommand selectCommand = new Select<
				OrganizationFinYear,
				Where<OrganizationFinYear.organizationID, Equal<Required<OrganizationFinYear.organizationID>>,
					And<OrganizationFinYear.year, Equal<Required<OrganizationFinYear.year>>>>>();

			if (selectCommand
				.CreateView(Graph, clearQueryCache, mergeCache)
				.SelectSingle(organizationID, yearNumber)
					is OrganizationFinYear sameYear)
			{
				return sameYear;
			}

			selectCommand = new Select<
				OrganizationFinYear,
				Where<OrganizationFinYear.organizationID, Equal<Required<OrganizationFinYear.organizationID>>,
					And<OrganizationFinYear.year, Less<Required<OrganizationFinYear.year>>>>,
				OrderBy<
					Desc<OrganizationFinYear.year>>>();

			if (selectCommand
				.CreateView(Graph, clearQueryCache, mergeCache)
				.SelectSingle(organizationID, yearNumber)
					is OrganizationFinYear earlierYear)
			{
				return earlierYear;
			}

			selectCommand = new Select<
				OrganizationFinYear,
				Where<OrganizationFinYear.organizationID, Equal<Required<OrganizationFinYear.organizationID>>,
					And<OrganizationFinYear.year, Greater<Required<OrganizationFinYear.year>>>>,
				OrderBy<
					Asc<OrganizationFinYear.year>>>();

			if (selectCommand
				.CreateView(Graph, clearQueryCache, mergeCache)
				.SelectSingle(organizationID, yearNumber)
					is OrganizationFinYear laterYear)
			{
				return laterYear;
			}

			return null;
		}
	}
}
