using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CM;
using PX.Objects.Common;
using PX.Objects.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.GL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.FA.DepreciationMethods.Parameters
{
	/// <exclude/>
	public class IncomingCalculationParameters
	{
		public PXGraph Graph;

		public FixedAsset FixedAsset;
		public FADetails Details;
		public FABookBalance BookBalance;
		public FADepreciationMethod Method;
		public List<FAAddition> Additions;

		public int Precision { get; set; }

		public string CalculationMethod => Method?.DepreciationMethod;
		public string AveragingConvention => BookBalance?.AveragingConvention;

		public int? BookID => BookBalance?.BookID;
		public int? AssetID => BookBalance?.AssetID;


		private IFABookPeriodRepository repositoryHelper;
		public IFABookPeriodRepository RepositoryHelper
		{
			get
			{
				repositoryHelper = repositoryHelper ?? Graph.GetService<IFABookPeriodRepository>();
				return repositoryHelper;
			}
		}

		private IFABookPeriodUtils utilsHelper;

		public IFABookPeriodUtils UtilsHelper
		{
			get
			{
				utilsHelper = utilsHelper ?? Graph.GetService<IFABookPeriodUtils>();
				return utilsHelper;
			}
		}

		public IncomingCalculationParameters(PXGraph graph, FABookBalance bookBalance)
		{
			Graph = graph;
			BookBalance = bookBalance;

			FixedAsset = SelectFrom<FixedAsset>
				.Where<FixedAsset.assetID.IsEqual<@P.AsInt>>
				.View
				.Select(Graph, AssetID);

			Details = SelectFrom<FADetails>
				.Where<FADetails.assetID.IsEqual<@P.AsInt>>
				.View
				.Select(Graph, AssetID);

			Method = SelectFrom<FADepreciationMethod>
				.Where<FADepreciationMethod.methodID.IsEqual<@P.AsInt>>
				.View
				.Select(Graph, BookBalance.DepreciationMethodID);

			Precision = (int)SelectFrom<Currency>
				.InnerJoin<Company>
					.On<Company.baseCuryID.IsEqual<Currency.curyID>>
				.View
				.Select(Graph)
				.RowCast<Currency>()
				.FirstOrDefault()
				.DecimalPlaces;

			// TODO: CollectAdditions() must be invoked after AC-156072 implementation
			Additions = CollectAdditionsFromHistory();
		}

		// TODO: Must be removed in scope of AC-156072
		// Will be used CollectAdditions() instead
		private List<FAAddition> CollectAdditionsFromHistory()
		{
			List<FAAddition> additions = SelectFrom<FABookHistory>
				.Where<FABookHistory.assetID.IsEqual<@P.AsInt>
					.And<FABookHistory.bookID.IsEqual<@P.AsInt>>
					.And<FABookHistory.ptdDeprBase.IsNotEqual<decimal0>>>
				.OrderBy<FABookHistory.finPeriodID.Asc>
				.View
				.Select(Graph, AssetID, BookID)
				.RowCast<FABookHistory>()
				.Select(history => new FAAddition(
					(decimal)history.PtdDeprBase,
					history.FinPeriodID,
					CalculateAdditionDate(history.FinPeriodID, BookBalance.DeprFromDate),
					Precision))
				.ToList();

			if(additions.IsEmpty()) // Zero cost fixed asset
			{
				additions.Add(new FAAddition(
					0m,
					BookBalance.DeprFromPeriod,
					CalculateAdditionDate(BookBalance.DeprFromPeriod, BookBalance.DeprFromDate),
					Precision));
			}

			additions.FirstOrDefault()?.MarkOriginal(BookBalance);

			return additions;
		}

		private List<FAAddition> CollectAdditions()
		{
			List<FAAddition> additions = SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.bookID.IsEqual<@P.AsInt>>
					.And<FATran.released.IsEqual<True>>
					.And<FATran.tranType.IsEqual<FATran.tranType.purchasingPlus>
					.Or<FATran.tranType.IsEqual<FATran.tranType.purchasingMinus>
						.And<FATran.origin.IsNotEqual<FARegister.origin.split>>>>>
				.View
				.Select(Graph, AssetID, BookID)
				.RowCast<FATran>()
				.Select(transaction => new FAAddition(
					(transaction.TranType == FATran.tranType.PurchasingMinus ? -1 : 1) * (transaction.TranAmt ?? 0m),
					transaction.TranPeriodID,
					CalculateAdditionDate(transaction.TranPeriodID, transaction.TranDate, BookBalance.DeprFromDate),
					Precision))
				.GroupBy(addition => (addition.Date, addition.PeriodID), (key, group) => new FAAddition(
					group.Sum(addition => addition.Amount),
					key.PeriodID,
					key.Date,
					Precision))
				.ToList();

			additions.Sort((x, y) => x.Date.CompareTo(y.Date));
			additions.FirstOrDefault()?.MarkOriginal(BookBalance);

			foreach(FATran splitReduceTransaction in SelectFrom<FATran>
				.Where<FATran.assetID.IsEqual<@P.AsInt>
					.And<FATran.bookID.IsEqual<@P.AsInt>>
					.And<FATran.released.IsEqual<True>>
					.And<FATran.tranType.IsEqual<FATran.tranType.purchasingMinus>>
					.And<FATran.origin.IsEqual<FARegister.origin.split>>>
				// DO NOT DELETE
				// Temporary commented. Uncomment after AC-149047 fix.
				//
				.AggregateTo<
					GroupBy<FATran.tranPeriodID>,
					Sum<FATran.tranAmt>>
				.View
				.Select(Graph, AssetID, BookID))
			{
				ReduceProportionally(additions, splitReduceTransaction);
			}

			CheckCollectedAdditionsInHistory(additions);

			return additions;
		}

		private DateTime MaxDate(DateTime date1, DateTime date2) => new DateTime(Math.Max(date1.Ticks, date2.Ticks));

		private DateTime CalculateAdditionDate(string tranPeriodID, DateTime? tranDate, DateTime? deprFromDate)
		{
			DateTime additionDate = MaxDate(
				RepositoryHelper.FindFABookPeriodOfDate(tranDate, BookID, AssetID).FinPeriodID == tranPeriodID
					? tranDate.Value
					: RepositoryHelper.FindOrganizationFABookPeriodByID(tranPeriodID, BookID, AssetID).StartDate.Value,
				deprFromDate.Value);

			if (RepositoryHelper.GetFABookPeriodIDOfDate(additionDate, BookID, AssetID) != tranPeriodID)
			{
				throw new PXException(Messages.DeprFromDateNotMatchPeriod, additionDate, FinPeriodIDFormattingAttribute.FormatForError(tranPeriodID), FixedAsset.AssetCD);
			}

			return additionDate;
		}

		// TODO: Must be removed in scope of AC-156072
		private DateTime CalculateAdditionDate(string periodID, DateTime? deprFromDate)
		{
			DateTime additionDate = MaxDate(
				RepositoryHelper.FindOrganizationFABookPeriodByID(periodID, BookID, AssetID).StartDate.Value,
				deprFromDate.Value);

			if (RepositoryHelper.GetFABookPeriodIDOfDate(additionDate, BookID, AssetID) != periodID)
			{
				throw new PXException(Messages.DeprFromDateNotMatchPeriod, additionDate, FinPeriodIDFormattingAttribute.FormatForError(periodID), FixedAsset.AssetCD);
			}

			return additionDate;
		}

		private void CheckCollectedAdditionsInHistory(List<FAAddition> additions)
		{
			HashSet<(string FinPeriodID, decimal Amount)> additionsByPeriod = additions.GroupBy(
				addition => addition.PeriodID,
				(finPeriodID, group) => (finPeriodID, group.Sum(a => a.Amount)))
				.ToHashSet();

			HashSet<(string FinPeriodID, decimal Amount)> additionsFromHistory = SelectFrom<FABookHistory>
				.Where<FABookHistory.assetID.IsEqual<@P.AsInt>
					.And<FABookHistory.bookID.IsEqual<@P.AsInt>>
					.And<FABookHistory.ptdDeprBase.IsNotEqual<decimal0>>>
				.View
				.Select(Graph, AssetID, BookID)
				.RowCast<FABookHistory>()
				.Select(history => (history.FinPeriodID, (decimal)history.PtdDeprBase))
				.ToHashSet();

			if (!additionsByPeriod.SetEquals(additionsFromHistory))
			{
				throw new PXException(
					Messages.FAAdditionsDontMatchHistory,
					FormatAdditions(additionsByPeriod),
					FormatAdditions(additionsFromHistory),
					FixedAsset.AssetCD);
			}
		}

		private string FormatAdditions(IEnumerable<(string FinPeriodID, decimal Amount)> additions)
		{
			return string.Join("; ", additions.Select(addition => $"{addition.FinPeriodID}: {addition.Amount}"));
		}

		private void ReduceProportionally(List<FAAddition> additions, FATran splitReduceTransaction)
		{
			List<FAAddition> affectedAdditions = additions.Where(addition => addition.PeriodID == splitReduceTransaction.TranPeriodID).ToList();
			decimal amountToReduce = affectedAdditions.Sum(addition => addition.Amount);
			decimal reducedSum = 0m;

			foreach(FAAddition addition in affectedAdditions)
			{
				decimal reducedAmount = PXRounder.Round((addition.Amount * splitReduceTransaction.TranAmt / amountToReduce) ?? 0m, Precision);
				reducedSum += reducedAmount;
				addition.Amount -= reducedAmount;
			}
			affectedAdditions.Last().Amount -= (splitReduceTransaction.TranAmt - reducedSum) ?? 0;
		}
	}

	/// <exclude/>
	public class CalculationParameters
	{
		public int AssetID { get; set; }
		public int BookID { get; set; }

		public FABookBalance BookBalance { get; set; }

		public List<FAAddition> Additions
		{
			get;
			set;
		}

		public string MaxDepreciateToPeriodID { get; set; }

		public CalculationParameters(IncomingCalculationParameters incomingData, string maxPeriodID = null)
		{
			if (incomingData.AssetID == null)
			{
				throw new ArgumentNullException(nameof(AssetID));
			}
			if (incomingData.BookID == null)
			{
				throw new ArgumentNullException(nameof(BookID));
			}

			AssetID = incomingData.AssetID.Value;
			BookID = incomingData.BookID.Value;

			BookBalance = incomingData.BookBalance;

			Additions = incomingData.Additions;

			MaxDepreciateToPeriodID = string.IsNullOrEmpty(maxPeriodID) || string.CompareOrdinal(BookBalance.DeprToPeriod, maxPeriodID) <= 0
				? BookBalance.DeprToPeriod
				: maxPeriodID; 
		}
	}
}
