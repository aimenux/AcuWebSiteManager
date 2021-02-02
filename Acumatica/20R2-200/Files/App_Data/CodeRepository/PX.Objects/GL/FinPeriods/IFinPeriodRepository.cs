using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL.FinPeriods
{
	public interface IFinPeriodRepository
	{
		int? GetCalendarOrganizationID(int? organizationID, int? branchID, bool? useMasterCalendar);
		int? GetCalendarOrganizationID(int? branchID, bool? useMasterCalendar);

		FinPeriod FindMaxFinPeriodWithEndDataBelongToInterval(DateTime? startDate, DateTime? endDate, int? organizationID);

		/// <summary>
		/// Returns PeriodID from the given date.
		/// </summary>
		string GetPeriodIDFromDate(DateTime? date, int? organizationID);

		/// <summary>
		/// Returns FinPeriod record from the given date.
		/// Throws the <see cref="FinancialPeriodNotDefinedForDateException"/> exception if period does not exists.
		/// </summary>
		FinPeriod GetFinPeriodByDate(DateTime? date, int? organizationID);
		/// <summary>
		/// Returns FinPeriod record from the given date.
		/// Does not throw any exceptions and returns null if period does not exists.
		/// </summary>
		/// <param name="date">The date between start and end date of the searching period.</param>
		/// <param name="organizationID">The identifier of the organization searching for.</param>
		/// <returns><see cref="FinPeriod"/></returns>
		FinPeriod FindFinPeriodByDate(DateTime? date, int? organizationID);
		string GetOffsetPeriodId(string finPeriodID, int offset, int? organizationID);
		FinPeriod GetOffsetPeriod(string finPeriodID, int offset, int? organizationID);
		string FindOffsetPeriodId(string finPeriodID, int offset, int? organizationID);
		FinPeriod FindOffsetPeriod(string finPeriodID, int offset, int? organizationID);

		/// <summary>
		/// Returns Next Period from the given.
		/// </summary>
		string NextPeriod(string finPeriodID, int? organizationID);

		/// <summary>
		/// Returns Start date for the given Period
		/// </summary>
		DateTime PeriodStartDate(string finPeriodID, int? organizationID);

		/// <summary>
		/// Returns End date for the given period
		/// </summary>
		DateTime PeriodEndDate(string finPeriodID, int? organizationID);

		IEnumerable<FinPeriod> GetFinPeriodsInInterval(DateTime? fromDate, DateTime? tillDate, int? organizationID);
		IEnumerable<FinPeriod> GetAdjustmentFinPeriods(string finYear, int? organizationID);
		FinPeriod FindLastYearNotAdjustmentPeriod(string finYear, int? organizationID);
		FinPeriod FindLastFinancialPeriodOfYear(string finYear, int? organizationID);

		OrganizationFinPeriod FindLastNonAdjustmentOrganizationFinPeriodOfYear(int? organizationID, string finYear, bool clearQueryCache = false);

		/// <summary>
		/// Returns a minimal set of financial periods that contain a given date interval
		/// within them, excluding any adjustment periods.
		/// </summary>
		/// <param name="Graph">The Graph which will be used when performing a select DB query.</param>
		/// <param name="startDate">The starting date of the date interval.</param>
		/// <param name="endDate">The ending date of the date interval.</param>
		IEnumerable<FinPeriod> PeriodsBetweenInclusive(DateTime startDate, DateTime endDate, int? organizationID);

		void CheckIsDateWithinPeriod(string finPeriodID, int? organizationID, DateTime date, string errorMessage);
		void CheckIsDateWithinPeriod(string finPeriodID, int? organizationID, DateTime date, string errorMessage, PXErrorLevel level);
		bool IsDateWithinPeriod(string finPeriodID, DateTime date, int? organizationID);
		bool PeriodExists(string finPeriodID, int? organizationID);

		/// <summary>
		/// Gets the ID of the financial period with the same <see cref="PX.Objects.GL.Obsolete.FinPeriod.PeriodNbr"/> 
		/// as the one specified, but residing in the previous financial year. If no such financial 
		/// period exists, an exception is thrown.
		/// </summary>
		string GetSamePeriodInPreviousYear(string finPeriodID, int? organizationID);

		FinPeriod GetByID(string finPeriodID, int? organizationID);
		FinPeriod FindByID(int? organizationID, string finPeriodID);
		FinPeriod FindPrevPeriod(int? organizationID, string finPeriodID, bool looped = false);
		FinPeriod FindNextPeriod(int? organizationID, string finPeriodID, bool looped = false);

		FinYear FindFirstYear(int? organizationID, bool clearQueryCache = false);
		FinYear FindLastYear(int? organizationID, bool clearQueryCache = false);

		FinPeriod FindLastPeriod(int? organizationID, bool clearQueryCache = false);
		FinPeriod FindFirstPeriod(int? organizationID, bool clearQueryCache = false);

		OrganizationFinPeriod FindFirstOpenFinPeriod(string fromFinPeriodID, int? organizationID, Type fieldModuleClosed = null);

	    ProcessingResult<FinPeriod> GetFinPeriodByMasterPeriodID(int? organizationID, string masterFinPeriodID);
	    string FindFinPeriodIDByMasterPeriodID(int? organizationID, string masterFinPeriodID, bool readAllAndCacheToPXContext = false);
		string GetFinPeriodByBranchAndMasterPeriodID(int? branchId, string masterFinPeriod);

        ProcessingResult FinPeriodsForMasterExist(string masterFinPeriodID, int?[] organizationIDs);
		MasterFinYear FindMasterFinYearByID(string year, bool clearQueryCache = false);
		MasterFinPeriod FindMasterFinPeriodByID(string finPeriodID, bool clearQueryCache = false);
		OrganizationFinYear FindOrganizationFinYearByID(int? organizationID, string year, bool clearQueryCache = false);
		OrganizationFinPeriod FindOrganizationFinPeriodByID(int? organizationID, string finPeriodID, bool clearQueryCache = false);

		MasterFinPeriod FindNextNonAdjustmentMasterFinPeriod(string prevFinPeriodID, bool clearQueryCache = false);

	    FinPeriod GetMappedPeriod(int? organizationID1, string finPeriodID1, int? organizationID2);

		OrganizationFinYear FindNearestOrganizationFinYear(int? organizationID, string yearNumber, bool clearQueryCache = false, bool mergeCache = false);
	}
}