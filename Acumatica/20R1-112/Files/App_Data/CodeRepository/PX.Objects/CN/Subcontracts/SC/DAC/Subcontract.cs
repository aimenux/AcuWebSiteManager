using PX.Data;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.CN.Subcontracts.SC.DAC
{
    [PXCacheName(Messages.Subcontract.CacheName)]
    [PXPrimaryGraph(typeof(SubcontractEntry))]
    public class Subcontract : POOrder
    {
    }
}
