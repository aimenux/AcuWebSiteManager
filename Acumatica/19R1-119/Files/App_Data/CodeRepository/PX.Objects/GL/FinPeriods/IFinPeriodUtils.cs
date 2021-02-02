using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.GL.DAC.Abstract;

namespace PX.Objects.GL.FinPeriods
{
	public interface IFinPeriodUtils
	{
		bool AllowPostToUnlockedPeriodAnyway { get; set; }

		void VerifyAndSetFirstOpenedFinPeriod<TFinPeriodField, TBranchField>(PXCache rowCache, object row, PXSelectBase<OrganizationFinPeriod> finPeriodView, Type fieldModuleClosed = null)
			where TFinPeriodField : class, IBqlField
			where TBranchField : class, IBqlField;


		void ValidateFinPeriod(IEnumerable<IAccountable> records, Type fieldModuleClosed = null);

		void ValidateFinPeriod<T>(IEnumerable<T> records, Func<T, string> getFinPeriodID, Func<T, int?[]> getBranchID, Type fieldModuleClosed = null);

		ProcessingResult CanPostToPeriod(IFinPeriod finPeriod, Type fieldModuleClosed = null);

		bool CanPostToClosedPeriod();

		OrganizationFinPeriod GetOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(string orgFinPeriodID, int? branchID)
			where TClosedInSubledgerField: IBqlField;

		OrganizationFinPeriod GetNearestOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(string orgFinPeriodID, int? branchID, Func<bool> additionalCondition = null)
			where TClosedInSubledgerField : IBqlField;

		OrganizationFinPeriod GetNearestOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(IPeriod orgFinPeriod)
			where TClosedInSubledgerField : IBqlField;

		OrganizationFinPeriod GetOpenOrganizationFinPeriodInFA(string orgFinPeriodID, int? assetID);

		string ComposeFinPeriodID(string yearNumber, string periodNumber);

		(string yearNumber, string periodNumber) ParseFinPeriodID(string finPeriodID);

		void CheckParametersOfCalendarGeneration(int? organizationID, int fromYear, int toYear);

		(int firstYear, int lastYear) GetFirstLastYearForGeneration(int? organizationID, int fromYear, int toYear, bool clearQueryCache = false);

		void CopyPeriods<TDAC, TFinPeriodID, TMasterFinPeriodID>(PXCache cache, TDAC src, TDAC dest)
			where TDAC : class, IBqlTable, new()
			where TFinPeriodID : class, IBqlField
			where TMasterFinPeriodID : class, IBqlField;
	}
}
