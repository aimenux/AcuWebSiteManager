using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.CL.Descriptor.Attributes.ComplianceDocumentRefNote;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.ChecksAndPaymentsServices
{
    public class ComplianceDocumentsService
    {
        private readonly APPaymentEntry graph;

        public ComplianceDocumentsService(APPaymentEntry graph)
        {
            this.graph = graph;
        }

        public void UpdateComplianceDocumentsIfRequired(APAdjust adjustment)
        {
            if (IsCheck())
            {
                UpdateComplianceDocuments(adjustment);
            }
        }

        private void UpdateComplianceDocuments(APAdjust adjustment)
        {
            var complianceDocuments = GetRelatedComplianceDocuments(adjustment.AdjdRefNbr, adjustment.AdjdDocType);
            graph.Caches<JointPayeePayment>().Persist(PXDBOperation.Insert);
            foreach (var complianceDocument in complianceDocuments)
            {
                UpdateComplianceDocument(adjustment, complianceDocument);
            }
        }

        private decimal? GetTotalJointAmountToPay(IEnumerable<PXResult<JointPayeePayment>> jointPayeePayments)
        {
            return jointPayeePayments.Sum(x => x.GetItem<JointPayeePayment>().JointAmountToPay);
        }

        private List<PXResult<JointPayeePayment>> GetJointPayeePaymentsAndJointPayees(APAdjust adjustment)
        {
            return adjustment.AdjdDocType == APDocType.Invoice
                ? JointPayeePaymentDataProvider.GetJointPayeePaymentsAndJointPayees(graph, adjustment)
                    .Where(x => x.GetItem<JointPayeePayment>().JointAmountToPay > 0).ToList()
                : new List<PXResult<JointPayeePayment>>();
        }

        private bool IsCheck()
        {
            return graph.Document.Current?.DocType == APDocType.Check;
        }

        private void UpdateComplianceDocument(APAdjust adjustment, ComplianceDocument complianceDocument)
        {
            UpdateComplianceDocumentData(adjustment, complianceDocument);
            complianceDocument.LinkToPayment = false;
            PersistComplianceDocument(complianceDocument);
        }

        private void UpdateComplianceDocumentData(APAdjust adjustment, ComplianceDocument complianceDocument)
        {
            var payment = graph.Document.Current;
			ComplianceDocumentRefNoteAttribute.SetComplianceDocumentReference<ComplianceDocument.apCheckId>(
				graph.Caches<ComplianceDocument>(), complianceDocument, payment.DocType, payment.RefNbr, payment.NoteID);

			UpdateComplianceDocumentAmountsIfRequired(adjustment, complianceDocument);
        }

        private void UpdateComplianceDocumentAmountsIfRequired(APAdjust adjustment, ComplianceDocument complianceDocument)
        {
            if (complianceDocument.DocumentType == GetLienWaiverDocumentTypeId())
            {
                UpdateComplianceDocumentAmounts(adjustment, complianceDocument);
            }
        }

        private void UpdateComplianceDocumentAmounts(APAdjust adjustment, ComplianceDocument complianceDocument)
        {
            complianceDocument.LienWaiverAmount = adjustment.CuryAdjgAmt;
            var jointPayeePayments = GetJointPayeePaymentsAndJointPayees(adjustment);
            if (jointPayeePayments.Any())
            {
                complianceDocument.JointAmount = GetTotalJointAmountToPay(jointPayeePayments);
            }
        }

        private void PersistComplianceDocument(ComplianceDocument complianceDocument)
        {
            var complianceDocumentCache = graph.Caches<ComplianceDocument>();
            complianceDocumentCache.Update(complianceDocument);
            complianceDocumentCache.PersistUpdated(complianceDocument);
        }

        private IEnumerable<ComplianceDocument> GetRelatedComplianceDocuments(
            string adjustmentReferenceNumber, string adjustmentDocumentType)
        {
            var query = new PXSelectJoin<ComplianceDocument,
                InnerJoin<ComplianceDocumentReference,
                    On<ComplianceDocumentReference.complianceDocumentReferenceId, Equal<ComplianceDocument.billID>>>,
                Where<ComplianceDocumentReference.type, Equal<Required<APInvoice.docType>>,
                    And<ComplianceDocumentReference.referenceNumber, Equal<Required<APInvoice.refNbr>>,
                        And<ComplianceDocument.linkToPayment, Equal<True>>>>>(graph);
            return query.Select(adjustmentDocumentType, adjustmentReferenceNumber).FirstTableItems.ToList();
        }

        private int? GetLienWaiverDocumentTypeId()
        {
            var query = new PXSelect<ComplianceAttributeType,
                Where<ComplianceAttributeType.type, Equal<ComplianceDocumentType.lienWaiver>>>(graph);
            return query.SelectSingle()?.ComplianceAttributeTypeID;
        }
    }
}