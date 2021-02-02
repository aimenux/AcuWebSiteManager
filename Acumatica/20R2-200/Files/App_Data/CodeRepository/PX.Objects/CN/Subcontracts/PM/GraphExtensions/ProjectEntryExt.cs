using System;
using System.Collections;
using PX.Data;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.PM;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.PM.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.PM.GraphExtensions
{
    public class ProjectEntryExt : PXGraphExtension<ProjectEntry>
    {
	    public static bool IsActive()
	    {
		    return PXAccess.FeatureInstalled<FeaturesSet.construction>();
	    }

		public override void Initialize()
        {
            AddSubcontractType();
        }

        [PXOverride]
        public virtual IEnumerable ViewPurchaseOrder(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
        {
            var commitment = Base.PurchaseOrders.Current;
            if (commitment.OrderType == POOrderType.RegularSubcontract)
            {
                RedirectToSubcontractEntry(commitment);
            }
            return baseHandler(adapter);
        }

        private static void RedirectToSubcontractEntry(POOrder commitment)
        {
            var graph = PXGraph.CreateInstance<SubcontractEntry>();
            graph.Document.Current = commitment;
            throw new PXRedirectRequiredException(graph, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private void AddSubcontractType()
        {
            var allowedValues = POOrderType.RegularSubcontract.CreateArray();
            var allowedLabels = Messages.Subcontract.CreateArray();
            PXStringListAttribute.AppendList<POOrder.orderType>(Base.PurchaseOrders.Cache, null, allowedValues,
                allowedLabels);
        }

        public PXAction<PMProject> createSubcontract;
        [PXUIField(DisplayName = Messages.CreateSubcontract, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable CreateSubcontract(PXAdapter adapter)
        {
	        return Base.CreatePOOrderBase<SubcontractEntry>(adapter, Messages.CreateSubcontract);
        }
	}
}