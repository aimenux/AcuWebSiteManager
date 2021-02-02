using System;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.CN.Common.Descriptor;
using PX.Objects.CN.Compliance.CL.DAC;
using PX.Objects.CN.Compliance.Descriptor;
using PX.Objects.CN.JointChecks.AP.DAC;
using PX.Objects.CN.JointChecks.AP.GraphExtensions.InvoiceEntry;
using PX.Objects.CN.JointChecks.AP.Services.DataProviders;

namespace PX.Objects.CN.JointChecks.AP.Services.LienWaiverValidationServices
{
    public class LienWaiverWarningDialogService
    {
        private readonly LienWaiverSetup lienWaiverSetup;
        private readonly LienWaiverDataProvider lienWaiverDataProvider;
        private readonly APInvoiceEntry graph;
        private readonly PXSelect<JointPayeePayment> jointPayeePayments;

        public LienWaiverWarningDialogService(APInvoiceEntry graph, ApInvoiceEntryPayBillExtension extension)
        {
            this.graph = graph;
            lienWaiverDataProvider = new LienWaiverDataProvider(graph);
            jointPayeePayments = extension.JointPayeePayments;
            lienWaiverSetup = extension.LienWaiverSetup.Current;
        }

        public void ShowWarningIfNeeded()
        {
            if (lienWaiverSetup.ShouldStopPayments == true ||
                lienWaiverSetup.ShouldWarnOnPayment == true)
            {
                var jointPayees = graph.GetExtension<ApInvoiceEntryExt>().JointPayees.SelectMain();
                var doesOutstandingComplianceExistForPrimaryVendor =
                    DoesAnyOutstandingComplianceExistForPrimaryVendor();
                var doesOutstandingComplianceExistForJointCheck =
                    jointPayees.Any(DoesAnyOutstandingComplianceExistForJointCheck);
                ShowWarningIfNeeded(doesOutstandingComplianceExistForPrimaryVendor,
                    doesOutstandingComplianceExistForJointCheck);
            }
        }

        private bool DoesAnyOutstandingComplianceExistForPrimaryVendor()
        {
            var projectIds = LienWaiverProjectDataProvider.GetProjectIds(graph);
            return lienWaiverDataProvider.DoesAnyOutstandingComplianceExistForPrimaryVendor(
                graph.Document.Current.VendorID, projectIds);
        }

        private bool DoesAnyOutstandingComplianceExistForJointCheck(JointPayee jointPayee)
        {
            var projectIds = LienWaiverProjectDataProvider.GetProjectIds(graph, jointPayee).ToList();
            return lienWaiverDataProvider.DoesAnyOutstandingComplianceExistForJointVendor(jointPayee, projectIds);
        }

        private void ShowWarningIfNeeded(bool doesOutstandingComplianceExistForPrimaryVendor,
            bool doesOutstandingComplianceExistForJointCheck)
        {
            if (lienWaiverSetup.ShouldStopPayments == true && (doesOutstandingComplianceExistForPrimaryVendor ||
                doesOutstandingComplianceExistForJointCheck))
            {
                ShowWarning(graph.APSetup.Current.HoldEntry == true
                    ? ComplianceMessages.LienWaiver.BillHasOutstandingLienWaiverStopPayment
                    : string.Concat(ComplianceMessages.LienWaiver.BillHasOutstandingLienWaiverStopPayment,
                        Environment.NewLine, Environment.NewLine,
                        ComplianceMessages.LienWaiver.CheckWillBeAssignedOnHoldStatus));
            }
            else if (lienWaiverSetup.ShouldWarnOnPayment == true)
            {
                if (doesOutstandingComplianceExistForPrimaryVendor && doesOutstandingComplianceExistForJointCheck)
                {
                    ShowWarning(string.Concat(ComplianceMessages.LienWaiver.VendorHasOutstandingLienWaiver,
                        Environment.NewLine,
                        ComplianceMessages.LienWaiver.JointPayeeHasOutstandingLienWaiver));
                }
                else if (doesOutstandingComplianceExistForPrimaryVendor)
                {
                    ShowWarning(ComplianceMessages.LienWaiver.VendorHasOutstandingLienWaiver);
                }
                else if (doesOutstandingComplianceExistForJointCheck)
                {
                    ShowWarning(ComplianceMessages.LienWaiver.JointPayeeHasOutstandingLienWaiver);
                }
            }
        }

        private void ShowWarning(string message)
        {
            jointPayeePayments.Ask(Common.Descriptor.SharedMessages.Warning, message, MessageButtons.OK, MessageIcon.Warning);
        }
    }
}