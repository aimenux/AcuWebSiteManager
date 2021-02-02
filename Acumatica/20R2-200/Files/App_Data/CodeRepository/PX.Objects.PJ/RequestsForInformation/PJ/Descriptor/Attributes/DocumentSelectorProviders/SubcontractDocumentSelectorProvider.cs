using System;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.PO;
using ScMessages = PX.Objects.CN.Subcontracts.SC.Descriptor.Messages;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class SubcontractDocumentSelectorProvider : DocumentSelectorProvider
    {
        public SubcontractDocumentSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.Subcontract;

        protected override Type SelectorType => typeof(POOrder);

        protected override Type SelectorQuery =>
            typeof(Select<POOrder, Where<POOrder.orderType, Equal<POOrderType.regularSubcontract>>>);

        protected override Type[] SelectorFieldTypes =>
            new[]
            {
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
            var subcontractEntry = PXGraph.CreateInstance<SubcontractEntry>();
            subcontractEntry.Document.Current = GetSubcontract(noteId);
            throw new PXRedirectRequiredException(subcontractEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        protected override string GetDocumentDescription(Guid? noteId)
        {
            return noteId != null
                ? GetSubcontractNumber(noteId)
                : null;
        }

        protected override string[] GetSelectorHeaderNames()
        {
            return SelectorFieldTypes.Select(GetFieldHeader).ToArray();
        }

        private string GetFieldHeader(Type fieldType)
        {
            return fieldType == typeof(POOrder.orderNbr)
                ? ScMessages.Subcontract.SubcontractNumber
                : GetDisplayName(fieldType);
        }

        private string GetDisplayName(Type fieldType)
        {
            var fieldName = Cache.GetField(fieldType);
            return PXUIFieldAttribute.GetDisplayName(Cache, fieldName);
        }

        private string GetSubcontractNumber(Guid? noteId)
        {
            return new PXSelect<POOrder, Where<POOrder.noteID, Equal<Required<POOrder.noteID>>>>(Graph)
                .SelectSingle(noteId).OrderNbr;
        }

        private POOrder GetSubcontract(Guid? noteId)
        {
            return new PXSelect<POOrder,
                Where<POOrder.noteID, Equal<Required<POOrder.noteID>>>>(Graph).SelectSingle(noteId);
        }
    }
}