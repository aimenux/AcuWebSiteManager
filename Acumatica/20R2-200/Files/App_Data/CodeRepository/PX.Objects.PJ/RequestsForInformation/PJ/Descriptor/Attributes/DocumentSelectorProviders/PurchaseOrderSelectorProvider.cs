using System;
using PX.Data;
using PX.Objects.PO;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class PurchaseOrderSelectorProvider : DocumentSelectorProvider
    {
        public PurchaseOrderSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.PurchaseOrder;

        protected override Type SelectorType => typeof(POOrder);

        protected override Type SelectorQuery =>
            typeof(Select<POOrder, Where<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>);

        protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(POOrder.orderType),
                typeof(POOrder.orderNbr),
                typeof(POOrder.vendorRefNbr),
                typeof(POOrder.orderDate),
                typeof(POOrder.status),
                typeof(POOrder.vendorID),
                typeof(POOrder.curyID),
                typeof(POOrder.orderTotal),
                typeof(POOrder.sOOrderNbr)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var purchaseOrderEntry = PXGraph.CreateInstance<POOrderEntry>();
            purchaseOrderEntry.Document.Current = GetPurchaseOrder(noteId);
            throw new PXRedirectRequiredException(purchaseOrderEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private POOrder GetPurchaseOrder(Guid? noteId)
        {
            var query = new PXSelect<POOrder,
                Where<POOrder.noteID, Equal<Required<POOrder.noteID>>>>(Graph);
            return query.SelectSingle(noteId);
        }
    }
}