using System.Collections;
using PX.Data;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Helpers;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.SC.Graphs
{
    public class PrintSubcontract : POPrintOrder
    {
        public PXAction<POPrintOrderFilter> ViewSubcontractDetails;

        public PrintSubcontract()
        {
            FeaturesSetHelper.CheckConstructionFeature();
        }

        [PXCustomizeBaseAttribute(typeof(PXUIFieldAttribute),
            Constants.AttributeProperties.DisplayName, Messages.Subcontract.SubcontractNumber)]
        public virtual void POPrintOrderOwned_OrderNbr_CacheAttached(PXCache cache)
        {
        }

        [PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXEditDetailButton]
        public override IEnumerable Details(PXAdapter adapter)
        {
            if (Records.Current != null && Filter.Current != null)
            {
                OpenSubcontractDetails();
            }
            return adapter.Get();
        }

        [PXUIField(MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Select, Enabled = false)]
        [PXButton]
        public virtual void viewSubcontractDetails()
        {
            if (Records.Current != null)
            {
                OpenSubcontractDetails();
            }
        }

        private void OpenSubcontractDetails()
        {
            var graph = CreateInstance<SubcontractEntry>();
            graph.Document.Current = Records.Current;
            throw new PXRedirectRequiredException(graph, true, Messages.ViewSubcontract)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }
    }
}