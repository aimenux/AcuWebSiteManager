using PX.Data;
using PX.Objects.GL.FinPeriods;
using System.Collections;
using PX.Objects.Common.Extensions;

namespace PX.Objects.GL.GraphBaseExtensions
{
	public class GenerateCalendarExtensionBase<FinPeriodMaintenanceGraph, PrimaryFinYear> : PXGraphExtension<FinPeriodMaintenanceGraph>
		where FinPeriodMaintenanceGraph : PXGraph, IFinPeriodMaintenanceGraph, new()
		where PrimaryFinYear : class, IBqlTable, IFinYear, new()
	{
		public PXFilter<FinPeriodGenerateParameters> GenerateParams;

		public PXAction<PrimaryFinYear> GenerateYears;
		[PXButton]
		[PXUIField(DisplayName = "Generate Calendar", MapEnableRights = PXCacheRights.Select)]
		public virtual IEnumerable generateYears(PXAdapter adapter)
		{
			IFinPeriodRepository finPeriodRepository = Base.GetService<IFinPeriodRepository>();
			IFinPeriodUtils finPeriodUtils = Base.GetService<IFinPeriodUtils>();
			PrimaryFinYear primaryYear = (PrimaryFinYear)Base.Caches<PrimaryFinYear>().Current;

			if(primaryYear == null)
			{
				throw new PXException(Messages.NeedToCreateFirstCalendarYear);
			}

			int? firstExistingYear = int.TryParse(finPeriodRepository.FindFirstYear(primaryYear.OrganizationID ?? 0, clearQueryCache: true)?.Year, out int parsedFirstExistingYear)
				? parsedFirstExistingYear
				: (int?)null;
			int? lastExistingYear = int.TryParse(finPeriodRepository.FindLastYear(primaryYear.OrganizationID ?? 0, clearQueryCache: true)?.Year, out int parsedLastExistingYear)
				? parsedLastExistingYear
				: (int?)null;

			bool generateCalendar = true;
			if (!Base.IsContractBasedAPI)
			{
				generateCalendar=GenerateParams.AskExtFullyValid((graph, viewName) =>
				{
					FinPeriodGenerateParameters parameters = GenerateParams.Current;
					parameters.OrganizationID = primaryYear.OrganizationID;
					parameters.FromYear =
					parameters.ToYear = lastExistingYear == null ? primaryYear.Year : (lastExistingYear + 1).ToString();
					parameters.FirstFinYear = firstExistingYear?.ToString();
					parameters.LastFinYear = lastExistingYear?.ToString();
				},
					DialogAnswerType.Positive);
			}

			if (generateCalendar)
			{
				int fromYear = int.Parse(GenerateParams.Current.FromYear);
				int toYear = int.Parse(GenerateParams.Current.ToYear);

				IFinPeriodMaintenanceGraph processingGraph = Base.Clone();
				PXLongOperation.StartOperation(
					Base,
					delegate ()
					{
						finPeriodUtils.CheckParametersOfCalendarGeneration(primaryYear.OrganizationID, fromYear, toYear);
						processingGraph.GenerateCalendar(primaryYear.OrganizationID, fromYear, toYear);
					});

				if (Base.IsContractBasedAPI)
					PXLongOperation.WaitCompletion(Base.UID);
			}
			return adapter.Get();
		}
	}
}