using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions.Periods;
using PX.Objects.Common.Extensions;
using PX.Objects.Common.GraphExtensions;
using PX.Objects.Common.GraphExtensions.Abstract;
using PX.Objects.GL.FinPeriods.TableDefinition;

namespace PX.Objects.GL.Descriptor
{
    public abstract class PeriodKeyProviderBase<
        TSourcesSpecificationCollection,
        TSourceSpecificationItem,
        TKeyWithSourceValuesCollection, 
        TKeyWithKeyWithSourceValues,
        TKey> : 
            PeriodKeyProviderBase, 
            IPeriodKeyProvider<TSourcesSpecificationCollection, TSourceSpecificationItem, TKeyWithSourceValuesCollection, TKeyWithKeyWithSourceValues, TKey>
            where TSourcesSpecificationCollection: PeriodKeyProviderBase.SourcesSpecificationCollection<TSourceSpecificationItem>, new()
            where TSourceSpecificationItem: PeriodKeyProviderBase.SourceSpecificationItem
            where TKeyWithSourceValuesCollection: PeriodKeyProviderBase.KeyWithSourceValuesCollection<TKeyWithKeyWithSourceValues, TSourceSpecificationItem, TKey>, new()
            where TKeyWithKeyWithSourceValues : PeriodKeyProviderBase.KeyWithSourceValues<TSourceSpecificationItem, TKey>, new()
            where TKey: OrganizationDependedPeriodKey, new()
    {
		public virtual TSourcesSpecificationCollection CachedSourcesSpecification { get; set; }

		public virtual Type UseMasterCalendarSourceType { get; set; }

		public virtual bool UseMasterOrganizationIDByDefault { get; set; }

        public abstract int? MasterValue { get; }

        public PeriodKeyProviderBase(
            TSourcesSpecificationCollection sourcesSpecification = null,
            Type[] sourceSpecificationTypes = null,
			Type useMasterCalendarSourceType = null,
			bool useMasterOrganizationIDByDefault = false)
		{
		    if (sourcesSpecification != null)
		    {
		        List<TSourceSpecificationItem> specificationItemsCopy = new List<TSourceSpecificationItem>(sourcesSpecification.SpecificationItems);

		        sourcesSpecification.SpecificationItems.Clear();

		        foreach (TSourceSpecificationItem specificationItem in specificationItemsCopy)
		        {
		            if (specificationItem.IsAnySourceSpecified)
		            {
		                sourcesSpecification.SpecificationItems.Add(specificationItem);

                        specificationItem.Initialize();
		            }
		        }

		        CachedSourcesSpecification = sourcesSpecification;
		    }
		    else
		    {
		        CachedSourcesSpecification = new TSourcesSpecificationCollection();
            }

            if (sourceSpecificationTypes != null)
			{
				foreach (var sourceSpecificationType in sourceSpecificationTypes)
				{
					TSourceSpecificationItem sourceSpec = (TSourceSpecificationItem)Activator.CreateInstance(sourceSpecificationType);

					sourceSpec.Initialize();

					CachedSourcesSpecification.SpecificationItems.Add(sourceSpec);
				}
			}

		    if (CachedSourcesSpecification.SpecificationItems.Count == 1)
		    {
		        CachedSourcesSpecification.SpecificationItems.First().IsMain = true;
		    }

			CachedSourcesSpecification.MainSpecificationItem = CachedSourcesSpecification.SpecificationItems.SingleOrDefault(s => s.IsMain);

			UseMasterCalendarSourceType = useMasterCalendarSourceType;

			UseMasterOrganizationIDByDefault = useMasterOrganizationIDByDefault;
		}

        public virtual PeriodKeyProviderBase.SourceSpecificationItem GetMainSourceSpecificationItem(PXCache cache, object row)
        {
            return GetSourcesSpecification(cache, row).MainSpecificationItem;
        }

        public virtual TSourcesSpecificationCollection GetSourcesSpecification(PXCache cache, object row)
		{
			return CachedSourcesSpecification;
		}


	    public virtual bool IsKeySourceValuesEquals(PXCache cache, object oldRow, object newRow)
	    {
	        TKeyWithSourceValuesCollection oldSourceValues =
	            BuildKeyCollection(cache.Graph, cache, oldRow, CollectSourceValues); 

	        TKeyWithSourceValuesCollection newSourceValues =
	            BuildKeyCollection(cache.Graph, cache, newRow, CollectSourceValues);

		    if (oldSourceValues.Items.Count != newSourceValues.Items.Count)
			    return false;

	        TSourcesSpecificationCollection specifications = GetSourcesSpecification(cache, oldRow);

		    IEnumerable<object> oldAdditionalFieldsValues =
		        specifications.DependsOnFields.Select(fieldType => cache.GetValue(oldRow, fieldType.Name));

		    IEnumerable<object> additionalFieldsValues =
		        specifications.DependsOnFields.Select(fieldType => cache.GetValue(newRow, fieldType.Name));

	        for (int i = 0; i < oldSourceValues.Items.Count; i++)
	        {
	            if(!oldSourceValues.Items[i].SourcesEqual(newSourceValues.Items[i]))
	                return false;
	        }

	        return oldAdditionalFieldsValues.SequenceEqual(additionalFieldsValues);

	    }

        #region Get Result Key Values
        #endregion


        #region Main Key

        public bool IsKeyDefined(PXGraph graph, PXCache attributeCache, object extRow)
        {
            return GetKeyAsArrayOfObjects(graph, attributeCache, extRow).All(value => value != null);
        }

        public object[] GetKeyAsArrayOfObjects(PXGraph graph, PXCache attributeCache, object extRow)
        {
            return GetKey(graph, attributeCache, extRow).ToListOfObjects(skipPeriodID: true).ToArray();
        }

        public object GetKeyAsObjects(PXGraph graph, PXCache attributeCache, object extRow)
        {
            return GetKey(graph, attributeCache, extRow);
        }

        public virtual IEnumerable<SourceSpecificationItem> GetSourceSpecificationItems(PXCache cache,
            object row)
        {
            return GetSourcesSpecification(cache, row).SpecificationItems;
        }

        public virtual TKey GetKey(PXGraph graph, PXCache attributeCache, object extRow)
        {
            if (UseMasterCalendarSourceType != null &&
                (bool?) BqlHelper.GetCurrentValue(graph, UseMasterCalendarSourceType, extRow) == true)
            {
                return new TKey() {OrganizationID = MasterValue};
            }

            if (!GetSourcesSpecification(attributeCache, extRow).SpecificationItems.Any()
            || GetSourcesSpecification(attributeCache, extRow).SpecificationItems.All(spec => !spec.IsAnySourceSpecified))
                return GetDefaultPeriodKey();

            TKeyWithKeyWithSourceValues rawMainKeyWithSourceValues =
                GetRawMainKeyWithSourceValues(graph, attributeCache, extRow);

            return rawMainKeyWithSourceValues != null
                ? rawMainKeyWithSourceValues.Key
                : GetKeys(graph, attributeCache, extRow).ConsolidatedKey;
        }

        public virtual TKey GetDefaultPeriodKey()
        {
            return new TKey()
            {
                OrganizationID = MasterValue
            };
        }
        
		

		#endregion


		#region OrganizationID & BranchID Values

	    public virtual TKeyWithSourceValuesCollection BuildKeyCollection(
		    PXGraph graph, 
		    PXCache attributeCache, 
		    object extRow,
		    Func<PXGraph, PXCache, object, TSourceSpecificationItem, TKeyWithKeyWithSourceValues> buildItemDelegate,
		    bool skipMain = false)
	    {
	        List<TKeyWithKeyWithSourceValues> items =
	            GetSourcesSpecification(attributeCache, extRow).SpecificationItems
	                .Where(item => item.IsMain != true || !skipMain)
	                .Select(specification => buildItemDelegate(graph, attributeCache, extRow, specification))
	                .ToList();

		    return new TKeyWithSourceValuesCollection()
		    {
			    Items = items,
			    ConsolidatedOrganizationIDs = items.SelectMany(item => item.KeyOrganizationIDs).ToList(),
                ConsolidatedKey = items.FirstOrDefault()?.Key
		    };
	    }

		public virtual TKeyWithSourceValuesCollection GetKeys(PXGraph graph, PXCache attributeCache, object extRow, bool skipMain = false)
		{
			return BuildKeyCollection(graph, attributeCache, extRow, CollectSourceValuesAndPreEvaluateKey, skipMain);
		}

		public virtual TKeyWithSourceValuesCollection GetKeysWithBasisOrganizationIDs(PXGraph graph, PXCache attributeCache, object extRow)
        {
            TKeyWithSourceValuesCollection keyWithSourceValues = GetKeys(graph, attributeCache, extRow);

            var availableOrganizationIDs = PXAccess.GetAvailableOrganizationIDs();

	        if ((keyWithSourceValues.ConsolidatedOrganizationIDs == null
	             || keyWithSourceValues.ConsolidatedOrganizationIDs.All(id => id == null)
	             || !keyWithSourceValues.ConsolidatedOrganizationIDs.Any())
				&& GetKey(graph, attributeCache, extRow).OrganizationID == MasterValue)
	        {
		        keyWithSourceValues.ConsolidatedOrganizationIDs = availableOrganizationIDs.ToList();
	        }
	        else
            {
	            keyWithSourceValues.ConsolidatedOrganizationIDs = keyWithSourceValues.ConsolidatedOrganizationIDs.Intersect(availableOrganizationIDs).ToList();
            }

            return keyWithSourceValues;
        }

		#endregion


		#region Main Source Field

		public virtual TKeyWithKeyWithSourceValues GetRawMainKeyWithSourceValues(PXGraph graph, PXCache attributeCache, object extRow)
        {
            return CollectSourceValuesAndPreEvaluateKey(graph, attributeCache, extRow, GetSourcesSpecification(attributeCache, extRow).MainSpecificationItem);
        }

        public virtual TKeyWithKeyWithSourceValues GetMainSourceOrganizationIDs(PXGraph graph, PXCache attributeCache, object extRow)
        {
            return GetOrganizationIDsValueFromField(graph, attributeCache, extRow, GetSourcesSpecification(attributeCache, extRow).MainSpecificationItem);
        }

        #endregion


        #region OrganizationID Pre-Calculation Services

        protected virtual TKeyWithKeyWithSourceValues CollectSourceValuesAndPreEvaluateKey(
		    PXGraph graph, PXCache attributeCache, object extRow, TSourceSpecificationItem specificationItem)
		{
			if (specificationItem == null)
				return null;

		    return EvaluateRawKey(graph, CollectSourceValues(graph, attributeCache, extRow, specificationItem));
		}

        protected virtual TKeyWithKeyWithSourceValues CollectSourceValues(
            PXGraph graph, PXCache attributeCache, object extRow, TSourceSpecificationItem specificationItem)
        {
            if (specificationItem == null)
                return null;

            return new TKeyWithKeyWithSourceValues()
            {
                SourceOrganizationIDs = GetOrganizationIDsValueFromField(graph, attributeCache, extRow, specificationItem).SourceOrganizationIDs,
                SourceBranchIDs = GetBranchIDsValueFromField(graph, attributeCache, extRow, specificationItem).SourceBranchIDs,
                SpecificationItem = specificationItem,
            };
        }

        protected abstract TKeyWithKeyWithSourceValues EvaluateRawKey(
            PXGraph graph,
            TKeyWithKeyWithSourceValues keyWithKeyWithSourceValues);

        #endregion


        #region Values Extraction

        public virtual TKeyWithKeyWithSourceValues GetIDsValueFromField(
            PXGraph graph,
            PXCache attributeCache,
            object extRow,
            TSourceSpecificationItem sourceSpecification,
            Type fieldType,
            Action<TKeyWithKeyWithSourceValues, List<int?>> setter)
        {
            int? value = null;

            if (fieldType != null)
            {
                PXCache cache = GetSourceCache(graph, attributeCache, fieldType);

                object row = GetSourceRow(cache, extRow);

                value = (int?)BqlHelper.GetOperandValue(cache, row, fieldType);
            }

            var item = new TKeyWithKeyWithSourceValues()
            {
                SpecificationItem = sourceSpecification,
            };

            setter(item, value.SingleToList());

            return item;
        }

        public virtual TKeyWithKeyWithSourceValues GetOrganizationIDsValueFromField(
		    PXGraph graph, 
		    PXCache attributeCache, 
		    object extRow,
		    TSourceSpecificationItem sourceSpecification)
        {
            return GetIDsValueFromField(graph, attributeCache, extRow, sourceSpecification,
                sourceSpecification.OrganizationSourceType, (item, value) => item.SourceOrganizationIDs = value);
		}

        public virtual TKeyWithKeyWithSourceValues GetBranchIDsValueFromField(
		    PXGraph graph, 
		    PXCache attributeCache,
		    object extRow,
		    TSourceSpecificationItem calendarOrganizationIdSourceSpec)
		{
			bool? result = null;
			object branchID = null;

			if (calendarOrganizationIdSourceSpec.BranchSourceType != null || calendarOrganizationIdSourceSpec.BranchSourceFormula != null)
			{
				PXCache cache = GetSourceCache(graph, attributeCache, calendarOrganizationIdSourceSpec.BranchSourceType);

				object row = GetSourceRow(cache, extRow);

				if (calendarOrganizationIdSourceSpec.BranchSourceFormula != null)
				{
					BqlFormula.Verify(cache, row, calendarOrganizationIdSourceSpec.BranchSourceFormula, ref result, ref branchID);
				}
				else
				{
					branchID = BqlHelper.GetOperandValue(cache, row, calendarOrganizationIdSourceSpec.BranchSourceType);
				}
			}

			return new TKeyWithKeyWithSourceValues()
			{
				SpecificationItem = calendarOrganizationIdSourceSpec,
				SourceBranchIDs = ((int?)branchID).SingleToList()
			};
		}

		public virtual List<TKeyWithKeyWithSourceValues> GetBranchIDsValuesFromField(
			PXGraph graph,
			PXCache attributeCache,
			object extRow,
			bool skipMain = false)
		{
			return GetSourcesSpecification(attributeCache, extRow).SpecificationItems
			            .Where(specification => !specification.IsMain || !skipMain)
                        .Select(specification => GetBranchIDsValueFromField(graph, attributeCache, extRow, specification))
						.ToList();
		}

		#endregion


		#region PXCache & CurrentRow Service

		protected virtual PXCache GetSourceCache(PXGraph graph, PXCache attributeCache, Type sourceType)
		{
			if (typeof(IBqlField).IsAssignableFrom(sourceType) && !BqlCommand.GetItemType(sourceType).IsAssignableFrom(attributeCache.GetItemType()))
			{
				return graph.Caches[BqlCommand.GetItemType(sourceType)];
			}

			return attributeCache;
		}

		protected virtual object GetSourceRow(PXCache sourceCache, object extRow)
		{
			if (extRow == null || !sourceCache.GetItemType().IsAssignableFrom(extRow.GetType()))
			{
				return sourceCache.Current;
			}

			return extRow;
		}

		#endregion


	    protected bool IsIDsUndefined(List<int?> values)
	    {
		    return values == null
		           || values.All(id => id == null)
		           || !values.Any();
	    }
	}
}
