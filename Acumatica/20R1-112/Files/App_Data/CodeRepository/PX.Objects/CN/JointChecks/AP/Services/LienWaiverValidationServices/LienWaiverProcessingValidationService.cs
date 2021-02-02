using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverValidationServices
{
    public class LienWaiverProcessingValidationService
    {
        private readonly LienWaiverDataProvider lienWaiverDataProvider;
        private readonly PXGraph graph;
        private string exceptionMessage;

        public LienWaiverProcessingValidationService(PXGraph graph)
        {
            lienWaiverDataProvider = new LienWaiverDataProvider(graph);
            this.graph = graph;
        }

        public void ValidateLienWaivers(APRegister document, bool? shouldStopPayments)
        {
            if (shouldStopPayments == true)
            {
                exceptionMessage = ComplianceMessages.LienWaiver.BillHasOutstandingLienWaiverStopPayment;
                CheckPaymentForOutstandingLienWaivers(document);
            }
        }

        public void ValidateLienWaiversOnManualStatusChange(APRegister document, bool? shouldStopPayments)
        {
            if (shouldStopPayments == true)
            {
                exceptionMessage = string.Concat(ComplianceMessages.LienWaiver.BillHasOutstandingLienWaiverStopPayment,
                    Environment.NewLine, ComplianceMessages.LienWaiver.CheckWillBeAssignedOnHoldStatus);
                CheckPaymentForOutstandingLienWaivers(document);
            }
        }

        private void CheckPaymentForOutstandingLienWaivers(APRegister document)
        {
            var adjustments = AdjustmentDataProvider.GetPaymentAdjustments(graph, document.RefNbr, document.DocType).ToList();
            var projectIds = LienWaiverProjectDataProvider.GetProjectIds(graph, adjustments).ToList();
            CheckPrimaryVendorForOutstandingLienWaivers(document, projectIds);
            adjustments.ForEach(adjust => CheckJointVendorsForOutstandingLienWaivers(adjust, projectIds));
        }

        private void CheckPrimaryVendorForOutstandingLienWaivers(APRegister document, IEnumerable<int?> projectIds)
        {
            if (lienWaiverDataProvider.DoesAnyOutstandingComplianceExistForPrimaryVendor(document.VendorID, projectIds))
            {
                throw new PXException(exceptionMessage);
            }
        }

        private void CheckJointVendorsForOutstandingLienWaivers(APAdjust adjustment, List<int?> projectIds)
        {
            var jointPayeePayments = JointPayeePaymentDataProvider.GetJointPayeePayments(graph, adjustment)
                .Where(jpp => jpp.JointAmountToPay > 0);
            var jointPayees =
                JointPayeeDataProvider.GetJointPayees(graph, jointPayeePayments);
            if (jointPayees.Any(jp =>
                lienWaiverDataProvider.DoesAnyOutstandingComplianceExistForJointVendor(jp, projectIds)))
            {
                throw new PXException(exceptionMessage);
            }
        }
    }
}