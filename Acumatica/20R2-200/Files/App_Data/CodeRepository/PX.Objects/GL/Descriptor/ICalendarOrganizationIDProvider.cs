using System;
using System.Collections.Generic;
using PX.Data;
using PX.Objects.Common.Abstractions.Periods;

namespace PX.Objects.GL.Descriptor
{
    public interface IPeriodKeyProvider<out TKey,
        out TSourcesSpecificationCollection> : IPeriodKeyProviderBase
        where TKey : OrganizationDependedPeriodKey, new()
        where TSourcesSpecificationCollection : PeriodKeyProviderBase.SourcesSpecificationCollectionBase
    {
        TKey GetKey(PXGraph graph, PXCache attributeCache, object extRow);
        TSourcesSpecificationCollection GetSourcesSpecification(PXCache cache, object row);
    }

    public interface IPeriodKeyProviderBase
    {
        object[] GetKeyAsArrayOfObjects(PXGraph graph, PXCache attributeCache, object extRow);

        bool IsKeyDefined(PXGraph graph, PXCache attributeCache, object extRow);

        IEnumerable<PeriodKeyProviderBase.SourceSpecificationItem> GetSourceSpecificationItems(PXCache cache, object row);

        PeriodKeyProviderBase.SourceSpecificationItem GetMainSourceSpecificationItem(PXCache cache, object row);

        bool IsKeySourceValuesEquals(PXCache cache, object oldRow, object newRow);
    }

    public interface IPeriodKeyProvider<
        TSourcesSpecificationCollection, 
        TSourceSpecificationItem, 
        TKeyWithSourceValuesCollection, 
        TKeyWithSourceValuesCollectionItem, 
        TKey>:
        IPeriodKeyProvider<TKey, TSourcesSpecificationCollection>
        where TSourcesSpecificationCollection : PeriodKeyProviderBase.SourcesSpecificationCollection<TSourceSpecificationItem>
        where TSourceSpecificationItem : PeriodKeyProviderBase.SourceSpecificationItem
        where TKeyWithSourceValuesCollection : PeriodKeyProviderBase.KeyWithSourceValuesCollection<TKeyWithSourceValuesCollectionItem, TSourceSpecificationItem, TKey>
        where TKeyWithSourceValuesCollectionItem : PeriodKeyProviderBase.KeyWithSourceValues<TSourceSpecificationItem, TKey>
        where TKey: OrganizationDependedPeriodKey, new()
    {
        Type UseMasterCalendarSourceType { get; set; }
		bool UseMasterOrganizationIDByDefault { get; set; }
		

	    TKeyWithSourceValuesCollection BuildKeyCollection(
			PXGraph graph, 
			PXCache attributeCache, 
			object extRow,
			Func<PXGraph, PXCache, object, TSourceSpecificationItem, TKeyWithSourceValuesCollectionItem> buildItemDelegate,
			bool skipMain = false);

	    TKeyWithSourceValuesCollection GetKeys(PXGraph graph, PXCache attributeCache, object extRow, bool skipMain = false);
	    TKeyWithSourceValuesCollection GetKeysWithBasisOrganizationIDs(PXGraph graph, PXCache attributeCache, object extRow);
        TKeyWithSourceValuesCollectionItem GetRawMainKeyWithSourceValues(PXGraph graph, PXCache attributeCache, object extRow);
        TKeyWithSourceValuesCollectionItem GetMainSourceOrganizationIDs(PXGraph graph, PXCache attributeCache, object extRow);
        TKeyWithSourceValuesCollectionItem GetOrganizationIDsValueFromField(
			PXGraph graph,
			PXCache attributeCache,
			object extRow,
			TSourceSpecificationItem sourceSpecification);

        TKeyWithSourceValuesCollectionItem GetBranchIDsValueFromField(
			PXGraph graph,
			PXCache attributeCache,
			object extRow,
			TSourceSpecificationItem sourceSpecification);

		List<TKeyWithSourceValuesCollectionItem> GetBranchIDsValuesFromField(
			PXGraph graph,
			PXCache attributeCache,
			object extRow,
			bool skipMain = false);
	}
}