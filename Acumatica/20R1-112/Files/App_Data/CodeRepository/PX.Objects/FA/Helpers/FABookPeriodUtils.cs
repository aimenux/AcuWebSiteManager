using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.FinPeriods;
using System;
using System.Linq;
using PX.Objects.Common.Extensions;
using PX.Objects.Common;

namespace PX.Objects.FA
{
	class FABookPeriodUtils : IFABookPeriodUtils
	{
		protected readonly PXGraph Graph;
		protected IFABookPeriodRepository FABookPeriodRepositoryHelper;

		public FABookPeriodUtils(PXGraph graph)
		{
			Graph = graph;
			FABookPeriodRepositoryHelper = Graph.GetService<IFABookPeriodRepository>();
		}

		public int? PeriodMinusPeriod(string finPeriodID1, string finPeriodID2, int? bookID, int? assetID)
		{
			FABookPeriodRepository.CheckNotNullOrEmptyStringContract(finPeriodID1, nameof(finPeriodID1));
			FABookPeriodRepository.CheckNotNullOrEmptyStringContract(finPeriodID2, nameof(finPeriodID2));
			FABookPeriodRepository.CheckNotNullIDContract(bookID, nameof(bookID));
			FABookPeriodRepository.CheckNotNullIDContract(assetID, nameof(assetID));

			int count = PXSelect<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<Where<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>,
						Or<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>>>>>>>
				.Select(
					Graph, 
					bookID, 
					FABookPeriodRepositoryHelper.GetFABookPeriodOrganizationID(bookID, assetID), 
					finPeriodID1, 
					finPeriodID2)
				.Count;
			if (count < 2 && string.Equals(finPeriodID1, finPeriodID2) == false)
			{
				throw new PXException(Messages.NoCalendarDefined);
			}

			PXResult res = PXSelectGroupBy<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<FABookPeriod.finPeriodID, LessEqual<Required<FABookPeriod.finPeriodID>>,
					And<FABookPeriod.finPeriodID, Greater<Required<FABookPeriod.finPeriodID>>,
					And<FABookPeriod.endDate, Greater<FABookPeriod.startDate>>>>>>,
				Aggregate<
					GroupBy<FABookPeriod.bookID,
					GroupBy<FABookPeriod.organizationID,
					Count>>>>
				.Select(
					Graph,
					bookID,
					FABookPeriodRepositoryHelper.GetFABookPeriodOrganizationID(bookID, assetID),
					finPeriodID1,
					finPeriodID2);
			return res != null ? res.RowCount : null;
		}

		public string PeriodPlusPeriodsCount(string finPeriodID, int counter, int? bookID, int? assetID)
		{
			FABookPeriodRepository.CheckNotNullOrEmptyStringContract(finPeriodID, nameof(finPeriodID));
			FABookPeriodRepository.CheckNotNullIDContract(bookID, nameof(bookID));
			FABookPeriodRepository.CheckNotNullIDContract(assetID, nameof(assetID));

			IYearSetup setup = FABookPeriodRepositoryHelper.FindFABookYearSetup(bookID);
			IPeriodSetup periodsInYear = FABookPeriodRepositoryHelper.FindFABookPeriodSetup(bookID).LastOrDefault();

			int organizationID = FABookPeriodRepositoryHelper.GetFABookPeriodOrganizationID(bookID, assetID);

			if (setup != null && FiscalPeriodSetupCreator.IsFixedLengthPeriod(setup.FPType) &&
				periodsInYear != null && periodsInYear.PeriodNbr != null)
			{
				return FinPeriodUtils.OffsetPeriod(finPeriodID, counter, Convert.ToInt32(periodsInYear.PeriodNbr));
			}
			else if (counter > 0)
			{
				PXResultset<FABookPeriod> res = PXSelect<
					FABookPeriod,
					Where<FABookPeriod.finPeriodID, Greater<Required<FABookPeriod.finPeriodID>>,
						And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>,
						And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
						And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>>>>>,
					OrderBy<
						Asc<FABookPeriod.finPeriodID>>>
					.SelectWindowed(Graph, 0, counter, finPeriodID, bookID, organizationID);

				if (res.Count < counter)
				{
					throw new PXFABookPeriodException();
				}

				return ((FABookPeriod)res[res.Count - 1]).FinPeriodID;
			}
			else if (counter < 0)
			{
				PXResultset<FABookPeriod> res = PXSelect<
					FABookPeriod,
					Where<FABookPeriod.finPeriodID, Less<Required<FABookPeriod.finPeriodID>>,
						And<FABookPeriod.startDate, NotEqual<FABookPeriod.endDate>,
						And<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
						And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>>>>>,
					OrderBy<
						Desc<FABookPeriod.finPeriodID>>>
					.SelectWindowed(Graph, 0, -counter, finPeriodID, bookID, organizationID);

				if (res.Count < -counter)
				{
					throw new PXFABookPeriodException();
				}

				return ((FABookPeriod)res[res.Count - 1]).FinPeriodID;
			}
			else
			{
				return finPeriodID;
			}
		}

		public string GetNextFABookPeriodID(string finPeriodID, int? bookID, int? assetID)
		{
			return PeriodPlusPeriodsCount(finPeriodID, 1, bookID, assetID);
		}

		public DateTime GetFABookPeriodStartDate(string finPeriodID, int? bookID, int? assetID)
		{
			FABookPeriodRepository.CheckNotNullOrEmptyStringContract(finPeriodID, nameof(finPeriodID));
			FABookPeriodRepository.CheckNotNullIDContract(bookID, nameof(bookID));
			FABookPeriodRepository.CheckNotNullIDContract(assetID, nameof(assetID));

			FABookPeriod period = PXSelect<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>>>>>
				.Select(Graph, bookID, FABookPeriodRepositoryHelper.GetFABookPeriodOrganizationID(bookID, assetID), finPeriodID);

			if (period?.StartDate == null)
			{
				throw new PXFABookPeriodException();
			}

			return (DateTime)period.StartDate;
		}

		public DateTime GetFABookPeriodEndDate(string finPeriodID, int? bookID, int? assetID)
		{
			FABookPeriod period = PXSelect<
				FABookPeriod,
				Where<FABookPeriod.bookID, Equal<Required<FABookPeriod.bookID>>,
					And<FABookPeriod.organizationID, Equal<Required<FABookPeriod.organizationID>>,
					And<FABookPeriod.finPeriodID, Equal<Required<FABookPeriod.finPeriodID>>>>>>
				.Select(Graph, bookID, FABookPeriodRepositoryHelper.GetFABookPeriodOrganizationID(bookID, assetID), finPeriodID);

			if (period?.EndDate == null)
			{
				throw new PXFABookPeriodException();
			}

			return ((DateTime)period.EndDate).AddDays(-1);
		}

		public virtual OrganizationFinPeriod GetNearestOpenOrganizationMappedFABookPeriodInSubledger<TClosedInSubledgerField>(int? bookID, int? sourceBranchID, string sourcefinPeriodID, int? targetBranchID)
			where TClosedInSubledgerField : IBqlField
		{
			if (!FABookPeriodRepositoryHelper.IsPostingFABook(bookID))
				return null;

			IFinPeriodUtils finPeriodUtils = Graph.GetService<IFinPeriodUtils>();
			IFinPeriodRepository finPeriodRepository = Graph.GetService<IFinPeriodRepository>();
			IFABookPeriodRepository faBookPeriodRepository = Graph.GetService<IFABookPeriodRepository>();

			int? sourceOrganizationID = PXAccess.GetParentOrganizationID(sourceBranchID);
			int? targetOrganizationID = PXAccess.GetParentOrganizationID(targetBranchID);

			// Mapped book period - first way:
			// FABookPeriod of sourceBranchID -> master book Period -> FABookPeriod of targetBranchID
			//
			ProcessingResult<FABookPeriod> targetFABookPeriod = faBookPeriodRepository.FindMappedFABookPeriod(
				bookID,
				sourceOrganizationID,
				sourcefinPeriodID,
				targetOrganizationID);

			// Mapped book period - second way: 
			// finPeriodID of sourceBranchID -> masterFinPeriod -> FinPeriodID of targetBranchID -> FABookPeriod of targetBranchID
			//
			if (targetFABookPeriod.Result == null)
			{
				ProcessingResult<FABookPeriod> targetFABookPeriodSecondWay = faBookPeriodRepository.FindMappedFABookPeriodUsingFinPeriod(
					bookID,
					sourceOrganizationID,
					sourcefinPeriodID,
					targetOrganizationID);

				targetFABookPeriodSecondWay.RaiseIfHasError();

				targetFABookPeriod = targetFABookPeriodSecondWay;
			}

			OrganizationFinPeriod period = finPeriodUtils.GetNearestOpenOrganizationFinPeriodInSubledger<TClosedInSubledgerField>(targetFABookPeriod.ThisOrRaiseIfHasError().Result);

			return period;
		}
	}
}
