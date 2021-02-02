using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.Common.DAC;
using PX.Objects.Common.Extensions;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL.Descriptor
{
	public class GenericFinYearSelectorAttribute : PXCustomSelectorAttribute
	{
		public Type SearchType { get; set; }

		public Type SourceType { get; set; }

		public Type BranchSourceType { get; set; }

		public Type BranchSourceFormulaType { get; set; }

		public Type OrganizationSourceType { get; set; }

		public bool TakeBranchForSelectorFromQueryParams { get; set; }

		public bool TakeOrganizationForSelectorFromQueryParams { get; set; }

		public Type UseMasterCalendarSourceType { get; set; }

		public CalendarOrganizationIDProvider CalendarOrganizationIDProvider { get; protected set; }

		public GenericFinYearSelectorAttribute(Type searchType,
											Type sourceType,
											Type branchSourceType = null,
											Type branchSourceFormulaType = null,
											Type organizationSourceType = null,
											bool takeBranchForSelectorFromQueryParams = false,
											bool takeOrganizationForSelectorFromQueryParams = false,
											Type useMasterCalendarSourceType = null,
											bool useMasterOrganizationIDByDefault = false,
											Type[] fieldList = null)
			: base(GetSearchType(
						typeof(Search3<FinYear.year, OrderBy<Desc<FinYear.startDate>>>),
						takeBranchForSelectorFromQueryParams,
						takeOrganizationForSelectorFromQueryParams),
					typeof(MasterFinYear.year))
		{
			if (searchType == null)
			{
				searchType = typeof(Search3<FinYear.year, OrderBy<Desc<FinYear.startDate>>>);
			}

			SearchType = searchType;
			SourceType = sourceType;
			BranchSourceType = branchSourceType;
			BranchSourceFormulaType = branchSourceFormulaType;
			OrganizationSourceType = organizationSourceType;
			TakeBranchForSelectorFromQueryParams = takeBranchForSelectorFromQueryParams;
			TakeOrganizationForSelectorFromQueryParams = takeOrganizationForSelectorFromQueryParams;
			UseMasterCalendarSourceType = useMasterCalendarSourceType;
		    CalendarOrganizationIDProvider = new CalendarOrganizationIDProvider(
		        new PeriodKeyProviderBase.SourcesSpecificationCollection()
		        {
		            SpecificationItems = new PeriodKeyProviderBase.SourceSpecificationItem()
		            {
		                BranchSourceType = branchSourceType,
		                BranchSourceFormulaType = branchSourceFormulaType,
		                OrganizationSourceType = organizationSourceType,
		            }.SingleToList()
		        },
		        useMasterCalendarSourceType: UseMasterCalendarSourceType,
		        useMasterOrganizationIDByDefault: useMasterOrganizationIDByDefault);

			Filterable = true;
		}

		public static Type GetSearchType(Type origSearchType, bool takeBranchForSelectorFromQueryParams, bool takeOrganizationForSelectorFromQueryParams)
		{
			//params will be passed into GetRecords context if they will be parsed from the query
			if (takeBranchForSelectorFromQueryParams || takeOrganizationForSelectorFromQueryParams)
			{
				BqlCommand cmd = BqlCommand.CreateInstance(origSearchType);

				if (takeBranchForSelectorFromQueryParams)
				{
					cmd = cmd.WhereAnd<Where<FinYear.organizationID, Equal<Optional2<QueryParameters.branchID>>>>();
				}

				if (takeOrganizationForSelectorFromQueryParams)
				{
					cmd = cmd.WhereAnd<Where<FinYear.organizationID, Equal<Optional2<QueryParameters.organizationID>>>>();
				}

				return cmd.GetType();
			}

			return origSearchType;
		}

		protected virtual IEnumerable GetRecords()
		{
			PXCache cache = _Graph.Caches[_CacheType];

			object extCurrentRow = PXView.Currents.FirstOrDefault(c => _CacheType.IsAssignableFrom(c.GetType()));

			int? organizationID = TakeBranchForSelectorFromQueryParams || TakeOrganizationForSelectorFromQueryParams
				? CalendarOrganizationIDProvider.GetCalendarOrganizationID(_Graph, PXView.Parameters, TakeBranchForSelectorFromQueryParams, TakeOrganizationForSelectorFromQueryParams)
				: CalendarOrganizationIDProvider.GetCalendarOrganizationID(cache.Graph, cache, extCurrentRow);

			int startRow = PXView.StartRow;
			int totalRows = 0;

			List<object> parameters = new List<object>() { organizationID };

			BqlCommand cmd = BqlCommand.CreateInstance(SearchType);
			cmd = cmd.WhereAnd<Where<FinYear.organizationID, Equal<Required<FinYear.organizationID>>>>();

			PXView view = new PXView(_Graph, PXView.View?.IsReadOnly ?? true, cmd);

			try
			{
				IEnumerable<object> data = view.Select(PXView.Currents,
														parameters.ToArray(),
														PXView.Searches,
														PXView.SortColumns,
														PXView.Descendings,
														PXView.Filters,
														ref startRow,
														PXView.MaximumRows,
														ref totalRows);
				return data;
			}
			finally
			{
				PXView.StartRow = 0;
			}
		}
	}
}
