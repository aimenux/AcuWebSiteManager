using System;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class ArInvoiceDocumentSelectorProvider : DocumentSelectorProvider
    {
        public ArInvoiceDocumentSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.ArInvoice;

        protected override Type SelectorType => typeof(ARInvoice);

        protected override Type SelectorQuery => typeof(Select<ARInvoice>);

        protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(ARInvoice.docType),
                typeof(ARInvoice.refNbr),
                typeof(ARInvoice.docDate),
                typeof(ARRegister.finPeriodID),
                typeof(ARInvoice.customerID),
                typeof(ARInvoice.curyID),
                typeof(ARInvoice.curyOrigDocAmt),
                typeof(ARInvoice.curyDocBal),
                typeof(ARInvoice.status),
                typeof(ARInvoice.dueDate)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
            invoiceEntry.Document.Current = GetInvoice(noteId);
            throw new PXRedirectRequiredException(invoiceEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private ARInvoice GetInvoice(Guid? noteId)
        {
            var query = new PXSelect<ARInvoice,
                Where<ARInvoice.noteID, Equal<Required<ARInvoice.noteID>>>>(Graph);
            return query.SelectSingle(noteId);
        }
    }
}