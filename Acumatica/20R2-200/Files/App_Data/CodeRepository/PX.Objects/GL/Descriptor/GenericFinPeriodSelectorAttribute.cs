using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Objects.Common.DAC;
using PX.Objects.GL.FinPeriods;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL.Descriptor
{
	public class GenericFinPeriodSelectorAttribute: PXCustomSelectorAttribute
	{
		public Type OrigSearchType { get; set; }

		public Func<ICalendarOrganizationIDProvider> GetCalendarOrganizationIDProvider { get; set; }

		public bool TakeBranchForSelectorFromQueryParams { get; set; }

		public bool TakeOrganizationForSelectorFromQueryParams { get; set; }

		public bool MasterPeriodBasedOnOrganizationPeriods { get; set; }

	    public GenericFinPeriodSelectorAttribute(
	        Type searchType,
	        Func<ICalendarOrganizationIDProvider> getCalendarOrganizationIDProvider,
	        bool takeBranchForSelectorFromQueryParams = false,
	        bool takeOrganizationForSelectorFromQueryParams = false,
	        bool masterPeriodBasedOnOrganizationPeriods = true,
	        FinPeriodSelectorAttribute.SelectionModesWithRestrictions selectionModeWithRestrictions = FinPeriodSelectorAttribute.SelectionModesWithRestrictions.Undefined,
	        Type[] fieldList = null)
	        : base(GetSearchType(searchType, takeBranchForSelectorFromQueryParams, takeOrganizationForSelectorFromQueryParams),
	                fieldList)
	    {
	        OrigSearchType = searchType;
	        GetCalendarOrganizationIDProvider = getCalendarOrganizationIDProvider;
	        TakeBranchForSelectorFromQueryParams = takeBranchForSelectorFromQueryParams;
	        TakeOrganizationForSelectorFromQueryParams = takeOrganizationForSelectorFromQueryParams;
	        MasterPeriodBasedOnOrganizationPeriods = masterPeriodBasedOnOrganizationPeriods;
	    }

	    public static Type GetSearchType(Type origSearchType, bool takeBranchForSelectorFromQueryParams, bool takeOrganizationForSelectorFromQueryParams)
		{
			//params will be passed into GetRecords context if they will be parsed from the query
			if (takeBranchForSelectorFromQueryParams || takeOrganizationForSelectorFromQueryParams)
			{
				BqlCommand cmd = BqlCommand.CreateInstance(origSearchType);

				if (takeBranchForSelectorFromQueryParams)
				{
					cmd = cmd.WhereAnd<Where<FinPeriod.organizationID, Equal<Optional2<QueryParameters.branchID>>>>();
				}

				if (takeOrganizationForSelectorFromQueryParams)
				{
					cmd = cmd.WhereAnd<Where<FinPeriod.organizationID, Equal<Optional2<QueryParameters.organizationID>>>>();
				}

				return cmd.GetType();
			}

			return origSearchType;
		}

		protected virtual FinPeriod BuildFinPeriod(int? organizationID, object record)
		{
			if (organizationID == FinPeriod.organizationID.MasterValue
			    && MasterPeriodBasedOnOrganizationPeriods)
			{
				MasterFinPeriod baseFinPeriod = (record as PXResult).GetItem<MasterFinPeriod>();

				return new FinPeriod()
				{
					FinPeriodID = baseFinPeriod.FinPeriodID,
					StartDateUI = baseFinPeriod.StartDateUI,
					EndDateUI = baseFinPeriod.EndDateUI,
					Descr = baseFinPeriod.Descr,
					NoteID = baseFinPeriod.NoteID
				};
			}
			else
			{
				PXResult resultRecord = record as PXResult;

				FinPeriod orgFinPeriod = resultRecord != null
													? resultRecord.GetItem<FinPeriod>()
													: (FinPeriod) record;

				return new FinPeriod
				{
					FinPeriodID = orgFinPeriod.FinPeriodID,
					StartDateUI = orgFinPeriod.StartDateUI,
					EndDateUI = orgFinPeriod.EndDateUI,
					Descr = orgFinPeriod.Descr,
					NoteID = orgFinPeriod.NoteID
				};
			}
		}

		protected virtual IEnumerable GetRecords()
		{
			PXCache cache = _Graph.Caches[_CacheType];

			object extCurrentRow = PXView.Currents.FirstOrDefault(c => _CacheType.IsAssignableFrom(c.GetType()));

			int? calendarOrganizationID = TakeBranchForSelectorFromQueryParams || TakeOrganizationForSelectorFromQueryParams
				? GetCalendarOrganizationIDProvider().GetCalendarOrganizationID(_Graph, PXView.Parameters, TakeBranchForSelectorFromQueryParams, TakeOrganizationForSelectorFromQueryParams)
				: GetCalendarOrganizationIDProvider().GetCalendarOrganizationID(cache.Graph, cache, extCurrentRow);

			calendarOrganizationID = calendarOrganizationID ?? FinPeriod.organizationID.MasterValue;

			int startRow = PXView.StartRow;
			int totalRows = 0;
			
			List<object> parameters = new List<object>();

			BqlCommand cmd = GetCommand(cache, extCurrentRow, parameters, calendarOrganizationID);

			PXView view = new PXView(_Graph, PXView.View?.IsReadOnly ?? true, cmd);

			try
			{
				List<PXFilterRow> adjustedFilters = new List<PXFilterRow>();
				foreach (PXFilterRow f in PXView.Filters)
				{
					// Attention: Conscious technomagic
					// Depending on the need to use the master calendar, we either group by field FinPeriod.masterFinPeriodID or not. 
					// When using the master calendar, we also must use filtering by the FinPeriod.masterFinPeriodID field instead of the default FinPeriod.finPeriodID field.
					if (calendarOrganizationID == FinPeriod.organizationID.MasterValue && 
						MasterPeriodBasedOnOrganizationPeriods && 
						string.Equals(f.DataField, nameof(FinPeriod.finPeriodID), StringComparison.OrdinalIgnoreCase))
					{
						f.DataField = nameof(FinPeriod.masterFinPeriodID);
					}
					adjustedFilters.Add(f);
				}

				IEnumerable<object> data = view.Select(PXView.Currents, parameters.ToArray(), PXView.Searches, PXView.SortColumns, PXView.Descendings, adjustedFilters.ToArray(), ref startRow, PXView.MaximumRows, ref totalRows);

				return data.Select(record => BuildFinPeriod(calendarOrganizationID, record)).ToArray();
			}
			finally
			{
				PXView.StartRow = 0;
			}
		}

		protected virtual BqlCommand GetCommand(PXCache cache, object extRow, List<object> parameters, int? calendarOrganizationID)
		{
			BqlCommand cmd = BqlCommand.CreateInstance(OrigSearchType);

			if (calendarOrganizationID == FinPeriod.organizationID.MasterValue
			    && MasterPeriodBasedOnOrganizationPeriods)
			{				
				
				cmd = BqlCommand.AppendJoin<LeftJoin<MasterFinPeriod,
													On<FinPeriod.masterFinPeriodID, Equal<MasterFinPeriod.finPeriodID>>>>(cmd)
															.AggregateNew<Aggregate<GroupBy<FinPeriod.masterFinPeriodID,
																					GroupBy<MasterFinPeriod.startDate,
																					GroupBy<MasterFinPeriod.endDate,
																					GroupBy<MasterFinPeriod.noteID>>>>>>();

				int?[] basisOrganizationIDs = GetCalendarOrganizationIDProvider().GetKeysWithBasisOrganizationIDs(cache.Graph, cache, extRow).ConsolidatedOrganizationIDs.ToArray();

				if (basisOrganizationIDs.Any())
				{
					cmd = cmd.WhereAnd<Where<FinPeriod.organizationID, In<Required<FinPeriod.organizationID>>>>();

					parameters.Add(basisOrganizationIDs);
				}
				else
				{
					cmd = cmd.WhereAnd<Where<FinPeriod.organizationID, NotEqual<Required<FinPeriod.organizationID>>>>();

					parameters.Add(calendarOrganizationID);
				}
			}
			else
			{
				cmd = cmd.WhereAnd<Where<FinPeriod.organizationID, Equal<Required<FinPeriod.organizationID>>>>();
				parameters.Add(calendarOrganizationID);
			}

			return cmd;
		}
	}
}
