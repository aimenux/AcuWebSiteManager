using PX.Data;
using PX.Objects.CN.Compliance.CL.DAC;

namespace PX.Objects.CN.Compliance.CL.Services
{
    internal static class ComplianceDocumentFieldVisibilitySetter
    {
        public static void HideFieldsForProject(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.revenueTaskID>(cache);
            ShowField<ComplianceDocument.costTaskID>(cache);
            ShowField<ComplianceDocument.customerID>(cache);
            ShowField<ComplianceDocument.customerName>(cache);
            ShowField<ComplianceDocument.vendorID>(cache);
            ShowField<ComplianceDocument.vendorName>(cache);
            ShowField<ComplianceDocument.accountID>(cache);
        }

        public static void HideFieldsForCustomer(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.revenueTaskID>(cache);
            ShowField<ComplianceDocument.customerID>(cache);
            ShowField<ComplianceDocument.customerName>(cache);
            ShowField<ComplianceDocument.accountID>(cache);
        }

        public static void HideFieldsForContract(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.customerID>(cache);
            ShowField<ComplianceDocument.accountID>(cache);
        }

        public static void HideFieldsForProjectTask(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.revenueTaskID>(cache);
            ShowField<ComplianceDocument.costTaskID>(cache);
            ShowField<ComplianceDocument.customerID>(cache);
            ShowField<ComplianceDocument.customerName>(cache);
        }

        public static void HideFieldsForVendor(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.costTaskID>(cache);
            ShowField<ComplianceDocument.vendorID>(cache);
            ShowField<ComplianceDocument.vendorName>(cache);
        }

        public static void HideFieldsForCommitments(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.vendorID>(cache);
            ShowField<ComplianceDocument.vendorName>(cache);
            ShowField<ComplianceDocument.purchaseOrder>(cache);
            ShowField<ComplianceDocument.subcontract>(cache);
            ShowField<ComplianceDocument.accountID>(cache);
        }

        public static void HideFieldsForArInvoice(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.revenueTaskID>(cache);
            ShowField<ComplianceDocument.customerID>(cache);
            ShowField<ComplianceDocument.customerName>(cache);
            ShowField<ComplianceDocument.accountID>(cache);
            ShowField<ComplianceDocument.invoiceID>(cache);
            ShowField<ComplianceDocument.invoiceAmount>(cache);
        }

        public static void ConfigureComplianceGridColumnsForApBill(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            PXUIFieldAttribute.SetVisibility<ComplianceDocument.linkToPayment>(cache, null, PXUIVisibility.Visible);
            ShowField<ComplianceDocument.linkToPayment>(cache);
            ShowField<ComplianceDocument.costTaskID>(cache);
            ShowField<ComplianceDocument.vendorID>(cache);
            ShowField<ComplianceDocument.vendorName>(cache);
            ShowField<ComplianceDocument.jointVendorInternalId>(cache);
            ShowField<ComplianceDocument.jointVendorExternalName>(cache);
            ShowField<ComplianceDocument.apCheckId>(cache);
            ShowField<ComplianceDocument.jointAmount>(cache);
            ShowField<ComplianceDocument.lienWaiverAmount>(cache);
            ShowField<ComplianceDocument.billID>(cache);
            ShowField<ComplianceDocument.billAmount>(cache);
            ShowField<ComplianceDocument.accountID>(cache);
        }

        public static void HideFieldsForApPayment(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.costTaskID>(cache);
            ShowField<ComplianceDocument.customerID>(cache);
            ShowField<ComplianceDocument.customerName>(cache);
            ShowField<ComplianceDocument.vendorID>(cache);
            ShowField<ComplianceDocument.vendorName>(cache);
            ShowField<ComplianceDocument.apCheckId>(cache);
            ShowField<ComplianceDocument.checkNumber>(cache);
            ShowField<ComplianceDocument.apPaymentMethodID>(cache);
        }

        public static void HideFieldsForArPayment(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.revenueTaskID>(cache);
            ShowField<ComplianceDocument.customerID>(cache);
            ShowField<ComplianceDocument.customerName>(cache);
            ShowField<ComplianceDocument.arPaymentID>(cache);
            ShowField<ComplianceDocument.arPaymentMethodID>(cache);
        }

        public static void HideFieldsForChangeOrder(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.costTaskID>(cache);
            ShowField<ComplianceDocument.revenueTaskID>(cache);
            ShowField<ComplianceDocument.customerID>(cache);
            ShowField<ComplianceDocument.customerName>(cache);
            ShowField<ComplianceDocument.vendorID>(cache);
            ShowField<ComplianceDocument.vendorName>(cache);
            ShowField<ComplianceDocument.changeOrderNumber>(cache);
        }

        public static void HideFieldsForProjectTransactionsForm(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.revenueTaskID>(cache);
            ShowField<ComplianceDocument.costTaskID>(cache);
            ShowField<ComplianceDocument.customerID>(cache);
            ShowField<ComplianceDocument.customerName>(cache);
            ShowField<ComplianceDocument.vendorID>(cache);
            ShowField<ComplianceDocument.vendorName>(cache);
            ShowField<ComplianceDocument.accountID>(cache);
            ShowField<ComplianceDocument.projectTransactionID>(cache);
        }

        public static void HideFieldsForPurchaseOrder(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.costTaskID>(cache);
            ShowField<ComplianceDocument.vendorID>(cache);
            ShowField<ComplianceDocument.vendorName>(cache);
            ShowField<ComplianceDocument.purchaseOrder>(cache);
            ShowField<ComplianceDocument.purchaseOrderLineItem>(cache);
            ShowField<ComplianceDocument.accountID>(cache);
        }

        public static void HideFieldsForSubcontract(PXCache cache)
        {
            HideAllNotSharedFields(cache);
            ShowField<ComplianceDocument.costTaskID>(cache);
            ShowField<ComplianceDocument.vendorID>(cache);
            ShowField<ComplianceDocument.vendorName>(cache);
            ShowField<ComplianceDocument.subcontract>(cache);
            ShowField<ComplianceDocument.subcontractLineItem>(cache);
            ShowField<ComplianceDocument.accountID>(cache);
        }

        private static void HideAllNotSharedFields(PXCache cache)
        {
            HideField<ComplianceDocument.selected>(cache);
            HideField<ComplianceDocument.linkToPayment>(cache);
            HideField<ComplianceDocument.effectiveDate>(cache);
            HideField<ComplianceDocument.limit>(cache);
            HideField<ComplianceDocument.methodSent>(cache);
            HideField<ComplianceDocument.customerID>(cache);
            HideField<ComplianceDocument.customerName>(cache);
            HideField<ComplianceDocument.vendorID>(cache);
            HideField<ComplianceDocument.vendorName>(cache);
            HideField<ComplianceDocument.purchaseOrder>(cache);
            HideField<ComplianceDocument.purchaseOrderLineItem>(cache);
            HideField<ComplianceDocument.subcontract>(cache);
            HideField<ComplianceDocument.subcontractLineItem>(cache);
            HideField<ComplianceDocument.invoiceID>(cache);
            HideField<ComplianceDocument.invoiceAmount>(cache);
            HideField<ComplianceDocument.billID>(cache);
            HideField<ComplianceDocument.billAmount>(cache);
            HideField<ComplianceDocument.lienWaiverAmount>(cache);
            HideField<ComplianceDocument.sponsorOrganization>(cache);
            HideField<ComplianceDocument.certificateNumber>(cache);
            HideField<ComplianceDocument.insuranceCompany>(cache);
            HideField<ComplianceDocument.policy>(cache);
            HideField<ComplianceDocument.apPaymentMethodID>(cache);
            HideField<ComplianceDocument.arPaymentMethodID>(cache);
            HideField<ComplianceDocument.accountID>(cache);
            HideField<ComplianceDocument.apCheckId>(cache);
            HideField<ComplianceDocument.checkNumber>(cache);
            HideField<ComplianceDocument.arPaymentID>(cache);
            HideField<ComplianceDocument.projectTransactionID>(cache);
            HideField<ComplianceDocument.receiptDate>(cache);
            HideField<ComplianceDocument.dateIssued>(cache);
            HideField<ComplianceDocument.throughDate>(cache);
            HideField<ComplianceDocument.receiveDate>(cache);
            HideField<ComplianceDocument.receivedBy>(cache);
            HideField<ComplianceDocument.sourceType>(cache);
            HideField<ComplianceDocument.isRequiredJointCheck>(cache);
            HideField<ComplianceDocument.jointVendorInternalId>(cache);
            HideField<ComplianceDocument.jointVendorExternalName>(cache);
            HideField<ComplianceDocument.jointAmount>(cache);
            HideField<ComplianceDocument.jointRelease>(cache);
            HideField<ComplianceDocument.jointReleaseReceived>(cache);
            HideField<ComplianceDocument.documentTypeValue>(cache);
            HideField<ComplianceDocument.paymentDate>(cache);
            HideField<ComplianceDocument.costTaskID>(cache);
            HideField<ComplianceDocument.revenueTaskID>(cache);
            HideField<ComplianceDocument.changeOrderNumber>(cache);
            HideField<ComplianceDocument.lienNoticeAmount>(cache);
            HideField<ComplianceDocument.jointLienNoticeAmount>(cache);
            HideField<ComplianceDocument.isReceivedFromJointVendor>(cache);
            HideField<ComplianceDocument.jointReceivedDate>(cache);
            HideField<ComplianceDocument.jointLienWaiverAmount>(cache);
        }

        private static void HideField<TField>(PXCache cache)
            where TField : IBqlField
        {
            PXUIFieldAttribute.SetVisible<TField>(cache, null, false);
        }

        private static void ShowField<TField>(PXCache cache)
            where TField : IBqlField
        {
            PXUIFieldAttribute.SetVisible<TField>(cache, null, true);
        }
    }
}