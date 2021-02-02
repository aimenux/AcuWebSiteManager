using System;
using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.PJ.RequestsForInformation.PJ.Descriptor.Attributes.DocumentSelectorProviders
{
    public class ApInvoiceDocumentSelectorProvider : DocumentSelectorProvider
    {
        public ApInvoiceDocumentSelectorProvider(PXGraph graph, string fieldName)
            : base(graph, fieldName)
        {
        }

        public override string DocumentType => RequestForInformationRelationTypeAttribute.ApInvoice;

        protected override Type SelectorType => typeof(APInvoice);

        protected override Type SelectorQuery => typeof(Select<APInvoice>);

        protected override Type[] SelectorFieldTypes =>
            new[]
            {
                typeof(APInvoice.docType),
                typeof(APInvoice.refNbr),
                typeof(APInvoice.docDate),
                typeof(APRegister.finPeriodID),
                typeof(APInvoice.vendorID),
                typeof(APInvoice.invoiceNbr),
                typeof(APInvoice.curyID),
                typeof(APInvoice.curyOrigDocAmt),
                typeof(APInvoice.curyDocBal),
                typeof(APInvoice.status),
                typeof(APInvoice.dueDate)
            };

        public override void NavigateToDocument(Guid? noteId)
        {
            var invoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();
            invoiceEntry.Document.Current = GetInvoice(noteId);
            throw new PXRedirectRequiredException(invoiceEntry, string.Empty)
            {
                Mode = PXBaseRedirectException.WindowMode.NewWindow
            };
        }

        private APInvoice GetInvoice(Guid? noteId)
        {
            var query = new PXSelect<APInvoice,
                Where<APInvoice.noteID, Equal<Required<APInvoice.noteID>>>>(Graph);
            return query.SelectSingle(noteId);
        }
    }
}