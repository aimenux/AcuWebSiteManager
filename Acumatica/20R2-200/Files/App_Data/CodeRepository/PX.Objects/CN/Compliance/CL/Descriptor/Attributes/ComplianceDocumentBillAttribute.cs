using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Services;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentBillAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
        {
            if (e.Row is ComplianceDocument row)
            {
                var billAmount = GetBillAmount(row.BillID, sender.Graph);
                sender.SetValue<ComplianceDocument.billAmount>(row, billAmount);
            }
        }

        private static decimal? GetBillAmount(Guid? refNoteId, PXGraph senderGraph)
        {
            if (!refNoteId.HasValue)
            {
                return null;
            }
            var reference = ComplianceDocumentReferenceRetriever.GetComplianceDocumentReference(senderGraph, refNoteId);
            var bill = GetBill(senderGraph, reference);
            return bill?.OrigDocAmt;
        }

        private static APInvoice GetBill(PXGraph senderGraph, ComplianceDocumentReference reference)
        {
            return new PXSelect<APInvoice,
                    Where<APInvoice.docType, Equal<Required<APInvoice.docType>>,
                        And<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>>(senderGraph)
                .SelectSingle(reference.Type, reference.ReferenceNumber);
        }
    }
}
