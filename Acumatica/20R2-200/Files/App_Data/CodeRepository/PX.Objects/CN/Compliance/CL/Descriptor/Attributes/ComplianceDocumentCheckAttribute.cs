using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Descriptor.Attributes
{
    public class ComplianceDocumentCheckAttribute : PXEventSubscriberAttribute, IPXFieldUpdatedSubscriber
    {
        public void FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs args)
        {
            if (args.Row is ComplianceDocument document)
            {
                UpdateCheckNumber(cache, document);
            }
        }

        private void UpdateCheckNumber(PXCache cache, ComplianceDocument document)
        {
            var checkNumber = document.ApCheckID.HasValue
                ? GetPayment(document.ApCheckID, cache.Graph)?.ExtRefNbr
                : null;
            cache.SetValue<ComplianceDocument.checkNumber>(document, checkNumber);
        }

        private APPayment GetPayment(Guid? checkId, PXGraph graph)
        {
            var reference = GetComplianceDocumentReference(checkId, graph);
            return new PXSelect<APPayment, Where<APPayment.refNbr, Equal<Required<APPayment.refNbr>>,
                    And<APPayment.docType, Equal<Required<APPayment.docType>>>>>(graph)
                .SelectSingle(reference.ReferenceNumber, reference.Type);
        }

        private ComplianceDocumentReference GetComplianceDocumentReference(Guid? checkId, PXGraph graph)
        {
            return new PXSelect<ComplianceDocumentReference, Where<
                ComplianceDocumentReference.complianceDocumentReferenceId,
                Equal<Required<ComplianceDocumentReference.complianceDocumentReferenceId>>>>(
                graph).SelectSingle(checkId);
        }
    }
}
