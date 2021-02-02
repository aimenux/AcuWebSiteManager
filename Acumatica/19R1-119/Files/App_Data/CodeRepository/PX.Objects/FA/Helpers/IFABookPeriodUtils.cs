using System;

namespace PX.Objects.FA
{
	public interface IFABookPeriodUtils
	{
		int? PeriodMinusPeriod(string finPeriodID1, string finPeriodID2, int? bookID, int? assetID);
		string PeriodPlusPeriodsCount(string finPeriodID, int counter, int? bookID, int? assetID);
		string GetNextFABookPeriodID(string finPeriodID, int? bookID, int? assetID);
		DateTime GetFABookPeriodStartDate(string finPeriodID, int? bookID, int? assetID);
		DateTime GetFABookPeriodEndDate(string finPeriodID, int? bookID, int? assetID);
	}
}
