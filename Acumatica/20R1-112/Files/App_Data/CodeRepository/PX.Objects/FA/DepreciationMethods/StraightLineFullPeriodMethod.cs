using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.FA.DepreciationMethods.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA.DepreciationMethods
{
	/// <exclude/>
	public class StraightLineFullPeriodMethod : StraightLineMethodBase
	{
		protected override string[] ApplicableAveragingConventions { get; } = { FAAveragingConvention.FullPeriod };

		protected override ICollection<FADepreciationScheduleItem> Calculate()
		{
			SortedDictionary<string, FADepreciationScheduleItem> depreciationSchedule = new SortedDictionary<string, FADepreciationScheduleItem>();
			foreach (FAAddition addition in CalculationParameters.Additions)
			{
				SLMethodAdditionParameters additionParameters = CalculateAdditionParameters(CalculationParameters, addition);

				List<FABookPeriod> periods = SelectFrom<FABookPeriod>
					.Where<FABookPeriod.finPeriodID.IsGreaterEqual<@P.AsString>
						.And<FABookPeriod.finPeriodID.IsLessEqual<@P.AsString>>
						.And<FABookPeriod.organizationID.IsEqual<@P.AsInt>>
						.And<FABookPeriod.bookID.IsEqual<@P.AsInt>>
						.And<FABookPeriod.startDate.IsNotEqual<FABookPeriod.endDate>>>
					.OrderBy<FABookPeriod.finPeriodID.Asc>
					.View
					.Select(
						IncomingParameters.Graph,
						additionParameters.DepreciateFromPeriodID,
						additionParameters.DepreciateToPeriodID,
						IncomingParameters.RepositoryHelper.GetFABookPeriodOrganizationID(CalculationParameters.BookID, CalculationParameters.AssetID),
						CalculationParameters.BookID
						)
					.RowCast<FABookPeriod>()
					.Where(period => !additionParameters.SuspendedPeriodIDs.Contains(period.FinPeriodID))
					.ToList();

				decimal periodDepreciationAmount = additionParameters.DepreciationBasis / periods.Count;
				foreach (FABookPeriod period in periods)
				{
					if (string.CompareOrdinal(period.FinPeriodID, CalculationParameters.MaxDepreciateToPeriodID) > 0) break;

					if (depreciationSchedule.TryGetValue(period.FinPeriodID, out FADepreciationScheduleItem scheduleItem))
					{
						scheduleItem.DepreciationAmount += periodDepreciationAmount;
					}
					else
					{
						depreciationSchedule[period.FinPeriodID] = new FADepreciationScheduleItem
						{
							FinPeriodID = period.FinPeriodID,
							DepreciationAmount = periodDepreciationAmount
						};
					}
				}
			}

			return depreciationSchedule.Values;
		}

		public static DateTime AddUsefulLifeToDate(DateTime date, decimal usefulLife)
		{
			int wholeYearsCount = (int)usefulLife;
			double restFraction = (double)(usefulLife - wholeYearsCount);

			DateTime plusYearDate = date.AddYears(1);
			int fractionalDaysCount = (int)((plusYearDate - date).TotalDays * restFraction);

			return date.AddYears(wholeYearsCount).AddDays(fractionalDaysCount - 1);
		}

		private SLMethodAdditionParameters CalculateAdditionParameters(
			CalculationParameters calculationData,
			FAAddition addition)
		{
			FABookBalance bookBalance = calculationData.BookBalance;

			#region Parameters Contracts
			if (bookBalance == null)
			{
				throw new ArgumentNullException(nameof(calculationData.BookBalance));
			}
			if (bookBalance.DeprFromDate == null)
			{
				throw new ArgumentNullException(nameof(bookBalance.DeprFromDate));
			}
			if (bookBalance.UsefulLife == null)
			{
				throw new ArgumentNullException(nameof(bookBalance.UsefulLife));
			}
			#endregion

			int assetID = calculationData.AssetID;
			int bookID = calculationData.BookID;

			SLMethodAdditionParameters parameters = new SLMethodAdditionParameters
			{
				DepreciationBasis = addition.DepreciationBasis,
				PlacedInServiceDate = (DateTime)bookBalance.DeprFromDate
			};

			addition.CalculatedAdditionParameters = parameters;

			FABookPeriod additionPeriod = IncomingParameters.RepositoryHelper.FindFABookPeriodOfDate(
				addition.IsOriginal
					? parameters.PlacedInServiceDate
					: addition.Date,
				bookID,
				assetID);
			parameters.DepreciateFromDate = (DateTime)additionPeriod?.StartDate.Value;
			parameters.DepreciateFromPeriodID = additionPeriod.FinPeriodID;

			DateTime depreciateToDate = AddUsefulLifeToDate(parameters.DepreciateFromDate, bookBalance.UsefulLife.Value);
			parameters.DepreciateToPeriodID = IncomingParameters.RepositoryHelper.GetFABookPeriodIDOfDate(depreciateToDate, bookID, assetID);

			// Consider the suspended periods
			parameters.SuspendedPeriodIDs = SelectFrom<FABookHistory>
				.Where<FABookHistory.assetID.IsEqual<@P.AsInt>
					.And<FABookHistory.bookID.IsEqual<@P.AsInt>>
					.And<FABookHistory.finPeriodID.IsGreaterEqual<@P.AsString>>
					.And<FABookHistory.finPeriodID.IsLessEqual<@P.AsString>>
					.And<FABookHistory.suspended.IsEqual<True>>>
				.OrderBy<FABookHistory.finPeriodID.Desc>
				.View
				.Select(
					IncomingParameters.Graph,
					calculationData.AssetID,
					calculationData.BookID,
					parameters.DepreciateFromPeriodID,
					parameters.DepreciateToPeriodID)
				.RowCast<FABookHistory>()
				.Select(history => history.FinPeriodID)
				.ToHashSet();

			parameters.DepreciateToPeriodID = IncomingParameters.UtilsHelper.PeriodPlusPeriodsCount(
				parameters.DepreciateToPeriodID,
				parameters.SuspendedPeriodIDs.Count,
				bookID,
				assetID);

			return parameters;
		}
	}
}
