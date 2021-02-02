using PX.Data;
using PX.Objects.EP;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PO.Extensions
{
    public static class EpOwnedExtensions
    {
        public static POOrder GetSubcontractEntity(this EPApprovalProcess.EPOwned epOwned, PXGraph graph)
        {
            if (epOwned == null || epOwned.EntityType != typeof(POOrder).FullName)
            {
                return null;
            }
            var purchaseOrder = new PXSelect<POOrder,
                Where<POOrder.noteID, Equal<Required<POOrder.noteID>>>>(graph).SelectSingle(epOwned.RefNoteID);
            return purchaseOrder.OrderType == POOrderType.RegularSubcontract
                ? purchaseOrder
                : null;
        }
    }
}