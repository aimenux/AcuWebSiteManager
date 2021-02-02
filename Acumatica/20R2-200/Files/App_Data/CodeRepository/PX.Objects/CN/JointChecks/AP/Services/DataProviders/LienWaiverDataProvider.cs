using System;
using System.Collections.Generic;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Models;
using PX.Objects.PO;

namespace PX.Objects.CN.JointChecks.AP.Services.DataProviders
{
    public class LienWaiverDataProvider : ILienWaiverDataProvider
    {
        private readonly PXGraph graph;

        public LienWaiverDataProvider(PXGraph graph)
        {
            this.graph = graph;
        }

        public IEnumerable<ComplianceDocument> GetOutstandingCompliancesForPrimaryVendor(int? vendorId,
            IEnumerable<int?> projectIds)
        {
            return GetOutstandingLienWaiversForPrimaryVendors(projectIds)
                .Where(cd => cd.VendorID == vendorId).Distinct(cd => cd.VendorID);
        }

        public bool DoesAnyOutstandingComplianceExistForPrimaryVendor(int? vendorId, IEnumerable<int?> projectIds)
        {
            return GetOutstandingCompliancesForPrimaryVendor(vendorId, projectIds).Any();
        }

        public IEnumerable<ComplianceDocument> GetOutstandingCompliancesForJointVendor(string externalName,
            IEnumerable<int?> projectIds)
        {
            var complianceDocuments = GetOutstandingLienWaiversForJointVendors(projectIds);
            return complianceDocuments.Where(cd =>
                    cd.JointVendorExternalName != null && cd.JointVendorExternalName == externalName)
                .Distinct(cd => cd.JointVendorExternalName);
        }

        public bool DoesAnyOutstandingComplianceExistForJointVendor(string externalName, IEnumerable<int?> projectIds)
        {
            return GetOutstandingCompliancesForJointVendor(externalName, projectIds).Any();
        }

        public IEnumerable<ComplianceDocument> GetOutstandingCompliancesForJointVendor(int? internalId,
            IEnumerable<int?> projectIds)
        {
            var complianceDocuments = GetOutstandingLienWaiversForJointVendors(projectIds);
            return complianceDocuments.Where(cd =>
                    cd.JointVendorInternalId != null && cd.JointVendorInternalId == internalId)
                .Distinct(cd => cd.JointVendorInternalId);
        }

        public bool DoesAnyOutstandingComplianceExistForJointVendor(int? internalId, IEnumerable<int?> projectIds)
        {
            return GetOutstandingCompliancesForJointVendor(internalId, projectIds).Any();
        }

        public IEnumerable<ComplianceDocument> GetOutstandingCompliancesForJointVendor(JointPayee jointPayee,
            List<int?> projectIds)
        {
            return GetOutstandingCompliancesForJointVendor(jointPayee.JointPayeeInternalId, projectIds)
                .Concat(GetOutstandingCompliancesForJointVendor(jointPayee.JointPayeeExternalName, projectIds));
        }

        public bool DoesAnyOutstandingComplianceExistForJointVendor(JointPayee jointPayee, List<int?> projectIds)
        {
            var complianceDocuments = GetOutstandingLienWaiversForJointVendors(projectIds);
            return complianceDocuments.Any(cd =>
                cd.JointVendorInternalId != null && cd.JointVendorInternalId == jointPayee.JointPayeeInternalId
                || cd.JointVendorExternalName != null && cd.JointVendorExternalName == jointPayee.JointPayeeExternalName);
        }

        public bool DoesAnyOutstandingComplianceExist(APRegister payment)
        {
            var adjustments = AdjustmentDataProvider.GetPaymentAdjustments(graph, payment.RefNbr, payment.DocType);
            var projectIds = LienWaiverProjectDataProvider.GetProjectIds(graph, adjustments).ToList();
            return DoesAnyOutstandingComplianceExistForPrimaryVendor(payment.VendorID, projectIds) ||
                PaymentDataProvider.DoesCheckContainPaymentForJointVendors(graph, payment) &&
                DoesAnyOutstandingComplianceExistForJointPayees(payment, projectIds);
        }

        public IEnumerable<ComplianceDocument> GetLienWaivers(Guid? checkNoteId)
        {
            var query = CreateLienWaiversQuery();
            query.Join<InnerJoin<ComplianceDocumentReference,
                On<ComplianceDocumentReference.complianceDocumentReferenceId.IsEqual<ComplianceDocument.apCheckId>>>>();
            query.WhereAnd<Where<ComplianceDocumentReference.refNoteId.IsEqual<P.AsGuid>>>();
            return query.Select<ComplianceDocument>(checkNoteId);
        }

        public IEnumerable<ComplianceDocument> GetNotVoidedLienWaivers(LienWaiverGenerationKey generationKey)
        {
            var query = CreateLienWaiversQuery();
            query.WhereAnd<Where<ComplianceDocument.projectID.IsEqual<P.AsInt>>>();
            query.WhereAnd<Where<ComplianceDocument.vendorID.IsEqual<P.AsInt>>>();
            var parameters = new object[]
            {
                generationKey.ProjectId,
                generationKey.VendorId
            };
            parameters = AppendCommitmentCondition(generationKey.OrderNumber, query, parameters);
            return query.Select<ComplianceDocument>(parameters).Where(lw => lw.IsVoided != true);
        }

        public ComplianceAttribute GetComplianceAttribute(string complianceAttributeValue)
        {
            return SelectFrom<ComplianceAttribute>
                .Where<ComplianceAttribute.value.IsEqual<P.AsString>>.View.Select(graph, complianceAttributeValue);
        }

        private bool DoesAnyOutstandingComplianceExistForJointPayees(APRegister payment, List<int?> projectIds)
        {
            var jointPayeePayments = JointPayeePaymentDataProvider.GetJointPayeePayments(graph, payment)
                .Where(jpp => jpp.JointAmountToPay > 0);
            var jointPayees = JointPayeeDataProvider.GetJointPayees(graph, jointPayeePayments);
            return jointPayees.Any(jp => DoesAnyOutstandingComplianceExistForJointVendor(jp, projectIds));
        }

        private IEnumerable<ComplianceDocument> GetOutstandingLienWaiversForJointVendors(IEnumerable<int?> projectIds)
        {
            var query = GetOutstandingLienWaiversQuery();
            query.WhereAnd<Where<ComplianceDocument.isReceivedFromJointVendor.IsEqual<False>
                .Or<ComplianceDocument.isReceivedFromJointVendor.IsNull>>>();
            return query.Select<ComplianceDocument>(graph.Accessinfo.BusinessDate, projectIds.ToArray());
        }

        private IEnumerable<ComplianceDocument> GetOutstandingLienWaiversForPrimaryVendors(IEnumerable<int?> projectIds)
        {
            var query = GetOutstandingLienWaiversQuery();
            query.WhereAnd<Where<ComplianceDocument.received.IsEqual<False>
                .Or<ComplianceDocument.received.IsNull>>>();
            return query.Select<ComplianceDocument>(graph.Accessinfo.BusinessDate, projectIds.ToArray());
        }

        private PXSelectBase<ComplianceDocument> GetOutstandingLienWaiversQuery()
        {
            var query = CreateLienWaiversQuery();
            query.WhereAnd<Where<ComplianceDocument.throughDate.IsLess<P.AsDateTime>
                .And<ComplianceDocument.projectID.IsIn<P.AsInt>>>>();
            return query;
        }

        private object[] AppendCommitmentCondition(string commitmentNumber, PXSelectBase<ComplianceDocument> query,
            object[] parameters)
        {
            var purchaseOrder =
                CommitmentDataProvider.GetCommitment(graph, commitmentNumber, POOrderType.RegularOrder);
            if (purchaseOrder == null)
            {
                query.WhereAnd<Where<ComplianceDocument.subcontract.IsEqual<P.AsString>>>();
                return parameters.Append(commitmentNumber);
            }
            query.Join<InnerJoin<ComplianceDocumentReference, On<ComplianceDocumentReference.
                complianceDocumentReferenceId.IsEqual<ComplianceDocument.purchaseOrder>>>>();
            query.WhereAnd<Where<ComplianceDocumentReference.refNoteId.IsEqual<P.AsGuid>>>();
            return parameters.Append(purchaseOrder.NoteID);
        }

        private PXSelectBase<ComplianceDocument> CreateLienWaiversQuery()
        {
            return new PXSelectJoin<ComplianceDocument,
                InnerJoin<ComplianceAttributeType,
                    On<ComplianceDocument.documentType.IsEqual<ComplianceAttributeType.complianceAttributeTypeID>>>,
                Where<ComplianceAttributeType.type.IsEqual<ComplianceDocumentType.lienWaiver>>>(graph);
        }
    }
}