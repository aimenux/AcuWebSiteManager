using System;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentInvoiceAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row is ComplianceDocument row)
            {
                var invoiceAmount = GetInvoiceAmount(row.InvoiceID, sender.Graph);
                sender.SetValue<ComplianceDocument.invoiceAmount>(row, invoiceAmount);
            }
        }

        private static decimal? GetInvoiceAmount(Guid? refNoteId, PXGraph senderGraph)
        {
            if (!refNoteId.HasValue)
            {
                return null;
            }
            var reference = ComplianceDocumentReferenceRetriever.GetComplianceDocumentReference(senderGraph, refNoteId);
            var invoice = GetInvoice(senderGraph, reference);
            return invoice?.OrigDocAmt;
        }

        private static ARInvoice GetInvoice(PXGraph senderGraph, ComplianceDocumentReference reference)
        {
            return new PXSelect<ARInvoice,
                    Where<ARInvoice.docType, Equal<Required<ARInvoice.docType>>,
                        And<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>>(senderGraph)
                .SelectSingle(reference.Type, reference.ReferenceNumber);
        }
    }
}