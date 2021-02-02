using PX.Data;
using PX.Objects.CN.Subcontracts.PO.CacheExtensions;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.SC.Views
{
    public class SubcontractSetup : PXSetup<POSetup>
    {
        public SubcontractSetup(PXGraph graph)
            : base(graph)
        {
            graph.Initialized += Initialized;
        }

        private void Initialized(PXGraph graph)
        {
            var baseHandler = graph.Defaults[typeof(POSetup)];
            graph.Defaults[typeof(POSetup)] = () => GetPurchaseOrderSetup(baseHandler);
        }

        private POSetup GetPurchaseOrderSetup(PXGraph.GetDefaultDelegate baseHandler)
        {
            var setup = (POSetup) baseHandler();
            var extension = PXCache<POSetup>.GetExtension<PoSetupExt>(setup);
            return !extension.IsSubcontractSetupSaved.GetValueOrDefault()
                ? throw new PXSetupNotEnteredException<SubcontractsPreferences>(ErrorMessages.SetupNotEntered)
                : setup;
        }

        [PXPrimaryGraph(typeof(SubcontractSetupMaint))]
        [PXCacheName(Messages.SubcontractsPreferencesScreenName)]
        private class SubcontractsPreferences : POSetup
        {
        }
    }
}