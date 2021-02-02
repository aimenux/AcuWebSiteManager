using System;
using System.Collections;
using PX.Data;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.PO;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class SubcontractLinkAttribute : PXEventSubscriberAttribute
    {
        public override void CacheAttached(PXCache cache)
        {
            var actionName = $"{cache.GetItemType().Name}${_FieldName}$Link";
            cache.Graph.Actions[actionName] = (PXAction) Activator.CreateInstance(
                typeof(PXNamedAction<>).MakeGenericType(GetDacOfPrimaryView(cache)),
                cache.Graph, actionName, (PXButtonDelegate) ViewSubcontract, GetEventSubscriberAttributes());
            cache.Graph.Actions[actionName].SetVisible(false);
        }

        private IEnumerable ViewSubcontract(PXAdapter adapter)
        {
            var cache = adapter.View.Graph.Caches[BqlTable];
            var orderNbr = cache.GetValue(cache.Current, _FieldName);
            var graph = PXGraph.CreateInstance<SubcontractEntry>();
            graph.Document.Current = GetPoOrder(adapter.View.Graph, orderNbr);
            throw new PXRedirectRequiredException(graph, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private POOrder GetPoOrder(PXGraph graph, object orderNbr)
        {
            var query = new PXSelect<POOrder,
                Where<POOrder.orderType, Equal<POOrderType.regularSubcontract>,
                    And<POOrder.orderNbr, Equal<Required<POOrder.orderNbr>>>>>(graph);
            return query.SelectSingle(orderNbr);
        }

        private static PXEventSubscriberAttribute[] GetEventSubscriberAttributes()
        {
            return new PXEventSubscriberAttribute[]
            {
                new PXUIFieldAttribute
                {
                    MapEnableRights = PXCacheRights.Select
                }
            };
        }

        private static Type GetDacOfPrimaryView(PXCache cache)
        {
            return cache.Graph.Views.ContainsKey(cache.Graph.PrimaryView)
                ? cache.Graph.Views[cache.Graph.PrimaryView].GetItemType()
                : cache.BqlTable;
        }
    }
}