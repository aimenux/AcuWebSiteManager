using System;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.Common;
using PX.Objects.Common.Abstractions;

namespace PX.Objects.CN.Compliance.CL.Services
{
    public static class ComplianceDocumentReferenceRetriever
    {
        public static ComplianceDocumentReference GetComplianceDocumentReference(PXGraph graph, Guid? referenceId)
        {
            return new PXSelect<ComplianceDocumentReference,
                    Where<ComplianceDocumentReference.complianceDocumentReferenceId,
                        Equal<Required<ComplianceDocumentReference.complianceDocumentReferenceId>>>>(graph)
                .SelectSingle(referenceId);
        }

        public static ComplianceDocumentReference GetComplianceDocumentReference(PXGraph graph,
            IDocumentKey documentKey)
        {
            return new PXSelect<ComplianceDocumentReference,
                    Where<ComplianceDocumentReference.type,
                        Equal<Required<ComplianceDocumentReference.type>>,
                        And<ComplianceDocumentReference.referenceNumber,
                            Equal<Required<ComplianceDocumentReference.referenceNumber>>>>>(graph)
                .SelectSingle(documentKey.DocType, documentKey.RefNbr);
        }

        public static ComplianceDocumentReference GetComplianceDocumentReference(PXGraph graph,
            IDocumentAdjustment adjust)
        {
            return new PXSelect<ComplianceDocumentReference,
                    Where<ComplianceDocumentReference.type,
                        Equal<Required<ComplianceDocumentReference.type>>,
                        And<ComplianceDocumentReference.referenceNumber,
                            Equal<Required<ComplianceDocumentReference.referenceNumber>>>>>(graph)
                .SelectSingle(adjust.AdjgDocType, adjust.AdjgRefNbr);
        }

        public static ComplianceDocumentReference GetComplianceDocumentReference(PXGraph graph, APAdjust adjustment)
        {
            return new PXSelect<ComplianceDocumentReference,
                    Where<ComplianceDocumentReference.type,
                        Equal<Required<ComplianceDocumentReference.type>>,
                        And<ComplianceDocumentReference.referenceNumber,
                            Equal<Required<ComplianceDocumentReference.referenceNumber>>>>>(graph)
                .SelectSingle(adjustment.DisplayDocType, adjustment.DisplayRefNbr);
        }

        public static ComplianceDocumentReference GetComplianceDocumentReference(PXGraph graph, APTran transaction)
        {
            return new PXSelect<ComplianceDocumentReference,
                    Where<ComplianceDocumentReference.type, Equal<Required<ComplianceDocumentReference.type>>,
                        And<ComplianceDocumentReference.referenceNumber,
                            Equal<Required<ComplianceDocumentReference.referenceNumber>>>>>(graph)
                .SelectSingle(transaction.POOrderType, transaction.PONbr);
        }

        public static Guid? GetComplianceDocumentReferenceId(PXGraph graph, IDocumentKey documentKey)
        {
            return GetComplianceDocumentReference(graph, documentKey)?.ComplianceDocumentReferenceId;
        }

        public static Guid? GetComplianceDocumentReferenceId(PXGraph graph, IDocumentAdjustment arAdjust)
        {
            return GetComplianceDocumentReference(graph, arAdjust)?.ComplianceDocumentReferenceId;
        }

        public static Guid? GetComplianceDocumentReferenceId(PXGraph graph, APInvoice apInvoice)
        {
            return GetComplianceDocumentReference(graph, apInvoice)?.ComplianceDocumentReferenceId;
        }

        public static Guid? GetComplianceDocumentReferenceId(PXGraph graph, APTran transaction)
        {
            return GetComplianceDocumentReference(graph, transaction)?.ComplianceDocumentReferenceId;
        }

        public static Guid? GetComplianceDocumentReferenceId(PXGraph graph, APAdjust adjustment)
        {
            return GetComplianceDocumentReference(graph, adjustment)?.ComplianceDocumentReferenceId;
        }
    }
}