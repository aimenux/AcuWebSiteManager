using PX.Common;

namespace PX.Objects.CN.Compliance.Descriptor
{
    [PXLocalizable]
    public static class ComplianceMessages
    {
        public const string HasExpiredComplianceDocuments = "Has Expired Compliance Documents";
        public const string ComplianceDocumentIsExpiredMessage = "Compliance document is expired.";
        public const string VendorHasExpiredComplianceDocumentsMessage = "Vendor has expired compliance documents.";
        public const string DocumentHasExpiredComplianceMessage = "Document has expired compliance.";
        public const string CustomerHasExpiredComplianceMessage = "Customer has expired compliance.";
        public const string ExpiredComplianceMessage = "Expired Compliance.";
        public const string AttributeNotFoundMessage = "'Attribute' cannot be found in the system.";
        public const string RequiredFieldMessage = "The field is required.";
        public const string UniqueConstraintMessage = "The value of this field must be unique among all records.";
        public const string OnlyOneVendorIsAllowed = "Joint Vendor could be specified only once.";

        public const string DeleteComplianceAttributeConfirmationDialogBody =
            "This action will delete the attribute from the compliance documents and all attribute values from corresponding records";

        public const string CannotDeleteAttributeMessage =
            "The value can not be deleted. It is used in at least one compliance document.";

        public const string WouldYouLikeToAddVendorClassToExistingProjects =
            "Would you like to add Vendor Class to existing projects for automatic Lien Waiver generation?";

        public const string CostTaskIsNotLinkedToChangeOrder = "Cost Task is not linked to the selected Change Order";
        public const string CostCodeIsNotLinkedToChangeOrder = "Cost Code is not linked to the selected Change Order";

        public const string RevenueTaskIsNotLinkedToChangeOrder =
			"Revenue Task is not linked to the selected Change Order";

        [PXLocalizable]
        public static class LienWaiver
        {
            public const string VendorHasOutstandingLienWaiver = "The vendor has at least one outstanding lien waiver.  ";
            public const string CheckWillBeAssignedOnHoldStatus = "AP Check(s) will be assigned 'On Hold' status.";

            public const string JointPayeeHasOutstandingLienWaiver =
                "The joint payee has at least one outstanding lien waiver.";

            public const string VendorAndJointPayeeHaveOutstandingLienWaiver =
                "The vendor and joint payee have at least one outstanding lien waiver:";

            public const string BillHasOneOrMoreOutstandingLienWaivers =
                "The accounts payable bill has at least one outstanding lien waiver.";

            public const string BillHasOutstandingLienWaiverStopPayment =
                BillHasOneOrMoreOutstandingLienWaivers +
                " Payment is not allowed because the Stop Payment of AP Bill When There Are Outstanding Lien Waivers" +
                " check box is selected on the Compliance Preferences (CL301000) form.";

            public const string ManuallyCreatedLienWaiverIsReferredToApCheck =
                "Manually created Lien Waiver is referred to the AP Check. Generate additional Lien Waiver automatically?";

            public const string WouldYouLikeToVoidAutomaticallyCreatedLienWaiver =
                "Would you like to void automatically created lien waivers referred to the {0}?";

            public const string DocumentTypeOptionVendorAndProjectMustBeSpecified =
                "To process lien waiver Document Type Option, Vendor and Project must be specified.";

            public const string LienWaiverGenerationFailed =
	            "The lien waiver generation has failed. Do one of the following: switch off automatic lien waiver generation on the Compliance Preferences (CL301000) form for a check to be processed or clear the Lien Notice Amount box on the Compliance Management (CL401000) form for the following lien waivers: {0}. Please also contact your Acumatica support provider.";

		}
	}
}