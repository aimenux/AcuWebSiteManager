using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Common.Services.DataProviders;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverValidationServices
{
    public class LienWaiverWarningMessageService
    {
        private readonly LienWaiverDataProvider lienWaiverDataProvider;
        private readonly PXGraph graph;
        private readonly IProjectDataProvider projectDataProvider;

        public LienWaiverWarningMessageService(PXGraph graph, IProjectDataProvider projectDataProvider)
        {
            lienWaiverDataProvider = new LienWaiverDataProvider(graph);
            this.projectDataProvider = projectDataProvider;
            this.graph = graph;
        }

        public string CreateVendorWarningMessage(IEnumerable<ComplianceDocument> outstandingCompliancesForPrimaryVendor)
        {
            return outstandingCompliancesForPrimaryVendor
                .Select(CreateVendorMessageLine)
                .Aggregate(string.Empty, string.Concat);
        }

        public string CreateWarningMessage(IEnumerable<ComplianceDocument> outstandingCompliancesForPrimaryVendor)
        {
            var message = string.Concat(ComplianceMessages.LienWaiver.VendorHasOutstandingLienWaiver, Environment.NewLine);
            return outstandingCompliancesForPrimaryVendor
                .Select(CreateMessageLine)
                .Aggregate(message, string.Concat);
        }

        public string CreateWarningMessage(JointPayee jointPayee, string message,
            IEnumerable<int?> projectIds)
        {
            var outstandingCompliancesForJointCheck =
                lienWaiverDataProvider.GetOutstandingCompliancesForJointVendor(jointPayee, projectIds.ToList());
            var vendorName = GetVendorName(jointPayee.JointPayeeInternalId);
            return outstandingCompliancesForJointCheck
                .Select(compliance => CreateMessageLine(jointPayee, compliance, vendorName))
                .Aggregate(message, string.Concat);
        }

        private string GetVendorName(int? vendorId)
        {
            var vendor = VendorDataProvider.GetVendor(graph, vendorId);
            return vendor == null
                ? string.Empty
                : $"{vendor.AcctCD}-{vendor.AcctName}";
        }

        private string CreateMessageLine(JointPayee jointPayee, ComplianceDocument compliance,
            string vendorName)
        {
            var project = projectDataProvider.GetProject(graph, compliance.ProjectID);
            var messageLine =
                $"{jointPayee.JointPayeeExternalName ?? vendorName}, {project.ContractCD}-{project.Description}";
            return AppendCommitments(compliance, messageLine);
        }

        private string CreateMessageLine(ComplianceDocument compliance)
        {
            var project = projectDataProvider.GetProject(graph, compliance.ProjectID);
            var messageLine = $"{project.ContractCD}-{project.Description}";
            return AppendCommitments(compliance, messageLine);
        }

        private string CreateVendorMessageLine(ComplianceDocument compliance)
        {
            var project = projectDataProvider.GetProject(graph, compliance.ProjectID);
            var vendorName = GetVendorName(compliance.VendorID);
            var messageLine = $"{vendorName}, {project.ContractCD}-{project.Description}";
            return AppendCommitments(compliance, messageLine);
        }

        private string AppendCommitments(ComplianceDocument compliance, string messageLine)
        {
            var complianceDocumentReference = GetPurchaseOrderComplianceReference(compliance.PurchaseOrder);
            messageLine = AddCommitmentIdIfExists(complianceDocumentReference?.ReferenceNumber, messageLine);
            messageLine = AddCommitmentIdIfExists(compliance.Subcontract, messageLine);
            return string.Concat(messageLine, Constants.WarningMessageSymbols.NewLine);
        }

        private static string AddCommitmentIdIfExists(string commitmentId, string messageLine)
        {
            return string.IsNullOrEmpty(commitmentId)
                ? messageLine
                : string.Concat(messageLine, $", {commitmentId}");
        }

        private ComplianceDocumentReference GetPurchaseOrderComplianceReference(Guid? purchaseOrderReference)
        {
            return SelectFrom<ComplianceDocumentReference>
                .Where<ComplianceDocumentReference.complianceDocumentReferenceId.IsEqual<P.AsGuid>>.View
                .Select(graph, purchaseOrderReference).FirstTableItems.SingleOrDefault();
        }
    }
}