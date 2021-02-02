using System;
using System.EnterpriseServices;
using PX.Data;
using PX.Common;

namespace PX.Objects.CR
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		#region Validation and Processing Messages
		public const string BAccountIsType = "Business Account is {0}.";
		public const string CannotAttachLeadTo = "Cannot attach Lead to {0}";
		public const string Prefix = "CR Error";
		public const string SelectRecord = "You must select the record first.";
		public const string SelectTemplateTooltip = "Select Template";
		public const string SubmitResponseTooltip = "Submit response";
		public const string CustomerRequired = "Customer is required for this case. Please convert {0} to customer.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CustomerIDRequired = "Customer ID is required.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ContractIDRequired = "Contract ID is required.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ContactIDRequired = "Contact ID is required.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AccountNameIsRequired = "Account Name is required.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string OpportunityIsRequired = "Opportunity ID is required.";
		public const string CampaignIsReferenced = "The campaign cannot be deleted because one or multiple documents refer to this campaign. Remove all the references and try again.";
		public const string AddNewCampaignMembers = "Add new Campaign members.";
		public const string CampaignDeleteMembersQuestion = "Do you want to delete selected Campaign members?";
		public const string ContractExpired = "Contract has expired.";
		public const string ContractIsNotActive = "Contract is not active.";
        public const string OrderTypeIsNotActive = "Order Type '{0}' is not active.";
        public const string ContractBAccountDiffer = "The contact does not belong to the selected business account.";
		public const string ContractInGracePeriod = "Selected Contract is on the grace period. {0} day(s) left before the expiration.";
		public const string CaseClassDetailsIsNotSet = "Cases of the given class cannot be billed. The 'Labor Item' and 'Overtime Class ID' must be specified in the Case Class if you want to bill for the hours. If you want to bill for the 'number of cases' set the Case Count Item in the Contract Template.";
        public const string ProspectNotCustomer = "Prior to creating an invoice or sales order, the opportunity must be associated with a customer account.";
        public const string InvoiceAlreadyCreated = "One or more existing invoices have been found for this opportunity.";
		public const string InvoiceAlreadyCreatedDeleted = "Invoice document was created for this Opportunity but cannot be found with the system. Do you want to recreate it?";
	    public const string InvoiceRequiredCustomerAccount = "A customer account is required for creating an invoice. Would you like to create a business account and then manually convert it to a customer account?";
	    public const string InvoiceRequiredConvertBusinessAccountToCustomerAccount = "A customer account is required for creating an invoice. Would you like to convert the specified business account to a customer account?";
	    public const string SalesOrderRequiredCustomerAccount = "A customer account is required for creating a sales order. Would you like to create a business account and then manually convert it to a customer account?";
	    public const string SalesOrderRequiredConvertBusinessAccountToCustomerAccount = "A customer account is required for creating a sales order. Would you like to convert the specified business account to a customer account?";
	    public const string SalesOrderHasNonInventoryLines = "The opportunity contains one or multiple product lines with no inventory item specified. Click OK if you want to ignore these product lines and proceed with creating a sales order. Click Cancel if you want to review the product lines and specify inventory items where necessary.";
		public const string SalesOrderHasOnlyNonInventoryLines = "The opportunity contains one or multiple product lines with no inventory item specified. If you want to create a sales order, you need to first review the product lines and specify inventory items where necessary because only lines with inventory items can be included in sales orders.";
		public const string InvoiceHasOnlyNonStockLines = "The opportunity contains one or multiple product lines with no non-stock inventory item specified. If you want to create an invoice, you may need to first review the product lines and specify non-stock inventory items where necessary because only lines with non-stock inventory items can be included in invoices.";
		public const string OrderAlreadyCreated = "Sales Order documents already exists for this Opportunity.";
		public const string OrderAlreadyCreatedDeleted = "Sales Order document was created for this Opportunity but cannot be found with the system. Do you want to recreate it?";
		public const string OrderView = "Sales Order document was created for this Opportunity. Do you want to view it?";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CannotCompleteEmail = "Draft email cannot be completed.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CannotDeleteTasksAndEvents = "There are some events or tasks, that cannot be deleted. Are you sure you whant to delete?";
		public const string ConfirmDeleteActivities = "There are some activities, that cannot be deleted. Are you sure you whant to delete?";
		public const string CannotDeleteActivity = "Billable, Time Card and currently email processing Activity can not be deleted.";
		public const string CannotDeleteDefaultLoc = "Default Business Account Location cannot be deleted.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CannotDeleteDefaultContact = "Default Business Account Contact cannot be deleted.";
        public const string CannotReassignDefaultContact = "You cannot assign this contact to a different business account since it is used as a default contact.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CannotCloseClosedCase = "Case already closed.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BAccountChangeWarning = "Do you want to assign this contact to the different Business Account?";
		public const string ArgumentIsNullOrEmpty = "Argument is null or an empty string.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string SelectContactWithBAccount = "You cannot select the Contact that is not assigned to the Business Account.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string SelectContactForCreatedCase = "Please, select Contact for the created Case.";
		public const string BillableTimeCannotBeGreaterThanTimeSpent = "Time Billable cannot be greater than Time Spent.";
		public const string OvertimeBillableCannotBeGreaterThanOvertimeSpent = "Overtime Billable cannot be greater than the Overtime Spent.";
		public const string DurationActivityExceed24Hours = "Activity Duration cannot exceed 24 hours. Please split.";
		public const string FailedToSelectCalenderId = "Calendar with specified Calendar ID cannot be found.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CaseCannotBeDeletedUseStatus = "This Case has one or more Task/Event/Activity record and cannot be deleted. Cancel or Close the Case instead.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ContactCannotBeDeleted = "This Contact has one or more Task/Event/Activity record and cannot be deleted.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadCannotBeDeleted = "This Lead has one or more Task/Event/Activity record and cannot be deleted.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BAccountCannotBeDeleted = "This Customer has one or more Task/Event/Activity record and cannot be deleted.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ServiceCaseCannotBeDeleted = "This service call has one or more Task/Event/Activity record and cannot be deleted.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TaskCannotBeDeleted = "Task cannot be deleted.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string EventCannotBeDeleted = "Event cannot be deleted.";
		public const string MassMailSend = "{0} email messages will be generated. Please confirm to proceed.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string MassMailReSend = "Try to resend selected message?";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string MassMailReSendAll = "There are {0} email messages failed to send. Please confirm to resend.";
		public const string RecipientsNotFound = "At least one recipient must be specified for the Email activity.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BadFilterDataField = "Incorrect Data Field value.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BadFilterCondition = "Incorrect Condition value";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TemplateDublicateError = "Template with this name already exists.";
		public const string AssignNotSetup = "Default Lead Assignment Map is not entered in CR setup.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AssignNotSetupBAccount = "Default Customer Assignment Map is not entered in CR setup";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AssignNotSetupOpportunity = "Default Opportunity Assignment Map is not entered in CR setup";
		public const string AssignNotSetupCase = "Default Case Assignment Map is not entered in CR setup";
		public const string ValidationFailed = "Validation is failed for one or more fields.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ReferenceCycleDetected = "Cyclic reference detected.";
		public const string NumberingIDIsNull = "Numbering ID is not configured in the CR setup.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AttributeIsReferenced = "One or more attributes is referenced and cannot be deleted.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string IncorrectInputData = "Incorrect Input Data.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string SelectBAccountCustomerForCreatedCase = "This Case Class requires the Business Account to be specified.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string EntityParameterIsNotValid = "Entitiy parameter is not valid";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string InheritanceType = "Inheritance type.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TypeMustBeInheritedFrom = "Type '{0}' must be inherited from '{1}' type.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TypeDoesnotImplementDefaultConstructor = "Type '{0}' doesn't implement default constructor.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TypeDoesnotImplementConstructorWithParameters = "Type '{0}' doesn't implement constructor with parameters: ";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string IncorrectParametersForCreatingGenericType = "Incorrect parameters for creating generic type.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string OpportunityCannotBeDeletedUseStatus = "This Opportunity has one or more Task/Event/Activity record and cannot be deleted. Cancel the Opportunity instead.";
		public const string CaseCannotBeFound = "Case cannot be Found";
		public const string ContractCannotBeFound = "Contract cannot be found";
		public const string LaborNotConfigured = "Labor Item cannot be found";
		public const string DueDateLessThanStartDate = "Due Date cannot be less then Start Date";
        public const string EndDateLessThanStartDate = "End Date cannot be earlier then Start Date";
		public const string ReminderDateRequired = "'Reminder at' is required";
		public const string CloseCaseWithHoldActivities = "Case has On-Hold billable activities and cannot be closed.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string OpportunityIsNotCreated = "Opportunity is not created";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string FillAccountAttributes = "Please, Fill the Account Attributes";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string FillOpportunityAttributes = "Please, Fill the Opportunity Attributes";
		public const string ContractActivationDateInFuture = "Contract activation date is in future. This contract can only be used starting from {0}";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CaseAlreadyReleased = "Given Case is already released.";
		public const string AttributeNotValid = "One or more Attributes are not valid.";
		public const string AttributeCannotDeleteActive = "The attribute cannot be removed because it is active. Make it inactive before removing.";
		public const string AttributeDeleteWarning = "This action will delete the attribute from the class and all attribute values from corresponding entities";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadNoMail = "This Contact is requested no email activities. Continue create email?";
		public const string RecordIsReferenced = "This record is referenced and cannot be deleted.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ErrorHeader = "Error";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NoCorrectContacts = "There are no correct contacts for this operation.";
        public const string ErrorFormat = "Specified format is not supported for notifications of this type.";
		public const string DeleteClassNotification = "Unable to delete notification entered for class";
		public const string SiteNotDefined = "The warehouse isn't specified for some stock items in the product list.";
		public const string DefaultLocationCanNotBeNotActive = "Default location can not be inactive.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string DefaultContactCanNotBeNotActive = "Default contact can not be inactive.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string EitherAccountOrContactMustBeSpecified = "Either Business Account or Contact must be specified.";
		public const string OnlyBillByActivity = "Case is configured to bill by Activity. You can release only activities.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string OnlyBillByCase = "Activity assigned to Case. Case is configured to bill by Case. You can release only case";
		public const string CaseCannotDependUponItself = "Case cannot depend upon itself.";
		public const string AssignmentMapIdEmpty = "Assignment Map ID is not specified.";
		public const string AssignmentError = "Unable to find route for assignment process.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CantGenerateInvoice = "Failed to Generated ARInvoice during the instant billing of a case.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string UserInAnotherContact = "User associated with another Contact '{0}'.";
		public const string ContactWithoutEmail = "Contact '{0}' does not have an email address.";
		public const string ContactWithUser = "Contact {0} already associated with another user.";
		public const string DeleteLocalRoles = "All Local Roles of User '{0}' will be deleted.";
		public const string EmptyEmail = "Recipient '{0}': Email is empty.";
		public const string EmailForwardPrefix = "FW: ";
		public const string EmailReplyPrefix = "RE: ";
		public const string InvalidRecipients = "{0} of {1} recipients are invalid in notification'{2}', module {3}.";
		public const string CantDeleteEmployeeContact = "Can not delete Contact of the Employee.";
        public const string OneOrMoreActivitiesAreNotApproved = "One or more activities that require Project Manager's approval is not approved. Case can be released only when all activities are approved.";
		public const string NonContactLoginType = "Incorrect User Type '{0}', linked entity must be 'Contact'";
		public const string NonEmployeeLoginType = "Incorrect User Type '{0}', linked entity must be 'Employee'";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string OpportunityWithIdAlreadyExist = "Opportunity with this ID already exist in the system.";
		public const string InactiveActivityType = "Activity Type '{0}' is not active.";
		public const string InternalActivityType = "Activity Type '{0}' is internal.";
		public const string ExternalActivityType = "Activity Type '{0}' is external. Only portal should create activities of this type.";
		public const string SystemActivityType = "This is a predefined activity type, which cannot be deleted or changed.";
		public const string ActivityTypeUsage = "This Activity Type can't be deleted because it's used.";
		public const string ActivityTypeUsageChanged = "This Activity Type can't be changed because it's used.";
		public const string UnableToFindGraph = "Unable to find primary graph for record.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ContactNotExisted = "This contact does not exist in the system.";
		public const string SomeCaseRequireCustomer = "Warning: Some case classes require customer.";
		public const string ContactNotFound = "Contact not found.";
		public const string DuplicateViewNotFound = "Unable to find view to process duplicate record.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NoDuplicatesWereFound = "No Duplicates were found.";
		public const string AttachToAccountNotFound = "Select the lead or the business account that you want to link with the contact.";
		public const string CanAttachToContactOrBAccount = "A lead can be linked only with a contact or with a business account.";
		public const string LeadHavePossibleDuplicates = "This lead probably has duplicates";
		public const string NoPossibleDuplicates = "No possible duplicates found";
        public const string PleaseSelectEmail = "Please select email before proceed operation";
        public const string CurrentClassHasUnreleasedRelatedCases = "The billing mode could not be changed. There are unreleased cases associated with this class.";


		public const string ContactHavePossibleDuplicates = "This contact probably has duplicates";
        public const string BAccountHavePossibleDuplicates = "This business account probably has duplicates";
		public const string MergeNonProspect = "Only prospect business accounts possible to merge.";
        public const string ContactBAccountOpp = "Contact '{0}' ({1}) has opportunities for another business account.";
        public const string ContactBAccountForOpp = "Contact '{0}' ({1}) has opportunity '{2}' for another business account.";
        public const string ContactBAccountCase = "Contact '{0}' has case '{1}' for another business account.";
        public const string ContactBAccountDiff = "Contact belongs to another business account.";
		public const string LeadBAccountDiff = "The duplicates cannot be merged because they are linked with different business accounts.";
		public const string BAccountAlreadyExists = "Business Account '{0}' already exists.";
		public const string OpportunityAlreadyExists = "Opportunity '{0}' already exists.";
		public const string DuplicatesNotSelected = "Please select duplicates for merge operation.";
		public const string DuplicatesMergeProhibitedDueToDifferentContacts = "The duplicates cannot be merged because they are linked with different contacts.";
		public const string DefAddressNotExists = "Default Address does not exists for '{0}'";		
		public const string ContactInactive = "Contact '{0}' is inactive or closed.";
        public const string BAccountRequiredToCreateCase = "Specify a business account before adding a case.";
		public const string OnlyOwnerSetReadStatus = "Only owner can mark email as '{0}'.";
		public const string SalesAccountIsNotSetForLaborItem = "Sales Account is not configured for the Labour Item.";
		public const string OnlyBAccountMergeSources = "A customer or vendor account cannot be used as a source business account; it can be used only as the target.";
	    public const string CreateCaseException = "Unable to create new case.~ {0}";
        public const string CreateLeadException = "Unable to create new lead.~ {0}";
		public const string IsNotProcessing = "{0} is not processing graph";
		public const string BillableTimeMustBeOtherThanZero = "Billable Time must be other than zero";
		public const string EmailsRuleOnDuplicate = "The rule cannot be deleted because Require Unique Email Address on Contact Validation is selected. To delete the rule, first clear the check box.";
		public const string ContactEmailNotUnique = "A validated lead or contact with the specified email address already exists. Please specify a unique email address.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ContactEmailProcDuplicate = "Unable to validate the record for duplicates: The specified email address is not unique.";
	    public const string ReplaceContactDetails = "Would you like the contact information to be replaced with the new information?";
	    public const string ReplaceContactDetailsAndShippingInfo = "Would you like the contact and shipping information to be replaced with the new information?";
	    public const string ReplaceShippingInfo = "Would you like the shipping information to be replaced with the new information?";
	    public const string ErrorOccurred = "Error occurred: {0}";
	    public const string SystemCannotExecuteWebRequest = "The system cannot execute a web request.";
	    public const string LoginChangedError = "The login cannot be modified.";

        public const string WorkGroupAssigned = "Workgroup '{0}' is used in '{1}' map '{2}'";
        public const string StageIsDisabledInOpportunityClass = "This stage is not active for the selected opportunity class.";
        public const string StageWillBeDeletedFromAllClasses = "This stage will be deleted from all opportunity classes. Are you sure you want to proceed?";
        public const string StageCannotBeDeleted = "The '{0}' stage cannot be deleted because it is specified for at least one opportunity.";
        public const string StageIsActiveInClasses = "This stage cannot be deleted because it is active for the following opportunity class(es): {0}.";
        public const string StageWillBeChangedInEveryClass = "All opportunity classes share the same list of stages. Modifying a stage in this list will affect all opportunity classes.";
        public const string ClassMustHaveActiveStages = "Activate at least one stage for this opportunity class.";

        public const string StageProspect = "Prospect";
        public const string StageNurture = "Nurture";
        public const string StageQualify = "Qualification";
        public const string StageDevelop = "Development";
        public const string StageSolution = "Solution";
        public const string StageProof = "Proof";
        public const string StageClose = "Negotiation";
        public const string StageDeploy = "Won";
		public const string AccessToAddInHasBeenDenied = "Access to the Acumatica add-in has been denied. You may not have sufficient access rights to view the Outlook Add-In (OU201000) form. Contact your system administrator.";
		public const string ValidationRegexError = "The value matches more than one country by regexp. {0} matched: {1}";
		public const string StateNotFound = "State '{0}' cannot be found in the system.";
		[Obsolete("This message is not used anymore")]
		public const string AcumaticaAddinAccessToCRMManagement = "The Acumatica add-in cannot access the customer management functionality of your Acumatica ERP instance. Make sure the Customer Management feature is enabled on the Enable/Disable Features (CS100000) form.";
		public const string CampaignLinkWarning = "The document is linked to a different campaign. This action will affect the performance of the {0} campaign.";
		public const string TaskIsAlreadyLinkedToCampaign = "Task {0} has already been linked to another campaign.";
		public const string QuoteSubmittedReadonly = "One or more settings in the opportunity cannot be modified, because the primary quote has been submitted.";
		public const string TaxAmountExcluded = "The tax total is excluded from the total because the Manual Amount check box is selected.";
		public const string EmptyValuesFromExternalTaxProvider = AP.Messages.EmptyValuesFromExternalTaxProvider;
		public const string ConfirmInvoiceCreation = "Select this check box to confirm that you want to create an invoice for the manual amount specified for the opportunity. To be able to create an invoice for the opportunity products, you need to first clear the Manual Amount check box for the opportunity.";
		public const string ConfirmSalesOrderCreation = "Select this check box to confirm that you want to ignore the manual amount specified for the opportunity and create a sales order for the opportunity products.";

		public const string CannotCreateProjectQuoteBecauseOfCury = "Cannot create a project quote for the opportunity in a non-base currency.";
		public const string CannotCreateProjectQuoteBecauseOfTaxCalcMode = "Cannot create a project quote for the opportunity with a non-default tax calculation mode.";
		public const string ManualAmountWillBeCleared = "The manual amount specified for the opportunity will be cleared.";
		public const string UnsupportedQuoteType = "This type of quote is not supported.";

		[Obsolete]
		public const string OpportunityIsClosed = "The opportunity is closed.";
		public const string OpportunityIsNotActive = "The opportunity is not active.";
		public const string FirstQuoteIsProject = "The quote is the first quote for the opportunity. If you save the changes, the quote will become the primary quote of the opportunity and the product lines of the opportunity will be deleted.";
		public const string OnlyDistributionQuotesAvailable = "Only a sales quote can be selected.";

		public const string AttachmentIsMissing = "This email does not contain attached Invoice documents";
		public const string AttachmentCannotBeSaved = "The attachment cannot be loaded to Acumatica ERP. You can try again or create a bill manually in the system";
		public const string AccountAlreadyExists = "A new business account cannot be created because the specified contact already belongs to a business account. Would you like to link the lead with the business account of the contact?";
		public const string FillReqiredAttributes = "Values should be specified for all required attributes.";
		public const string LastNameOrFullNameReqired = "Last Name or Company Name (or both) should be filled in.";
		public const string ReplaceContactInfoFromContact = "Would you like the contact information to be replaced with the information from the selected contact?";
		public const string ReplaceContactInfoFromAccount = "Would you like the contact information to be replaced with the information from the selected business account?";

		public const string FailedToSendMassEmail = "An error occurred during the mass processing of emails and the email has not been sent.";

		public const string MustBeEmployee = "An expense receipt can be created only for an employee. You are trying to create an expense receipt for the {0} user account, which is not associated with any employee.";

		public const string ExtensionCannotBeFound = "The {0} extension has not been found for the {1} graph.";

		#endregion

		#region Translatable Strings used in the code
		public const string Warning = AP.Messages.Warning;
		public const string Confirmation = GL.Messages.Confirmation;
		public const string AskConfirmation = GL.Messages.Confirmation;
		public const string ConfirmRemoving = "Are you sure you want to remove {0} members?";
        public const string ConfirmAdding = "Are you sure you want to add members to current campaign?";
        public const string ConfirmUpdating = "Are you sure you want to update members in current campaign?";
        public const string New = "<NEW>";
		public const string ActivityClassInfo = "Activity";
		public const string DefaultLocationCD = "MAIN";
		public const string DefaultLocationDescription = "Primary Location";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadAddress = "Lead Address";
		public const string CaseClass = "Case Class";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string DisplayType = "Type"; //TODO: need remove
		public const string ContactType = "Contact";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadType = "Lead";
		public const string CampaignID = "Campaign ID";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ClassSeverity = "Severity Class";
		public const string ContractSLAMapping = "SLA Mapping";
		
		public const string EmailClassInfo = "Email";
		public const string EmailResponseClassInfo = "Email Response";
		public const string EditVendor = "Edit Vendor";
		public const string Company = "Company";
		public const string CompanyName = "Company Name";
		public const string CompanyClass = "Company Class";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NoNameRouter = "Router";
		public const string ViewInvoice = CT.Messages.ViewInvoice;
		public const string ViewCampaignDocuments = "View Campaign Documents";
		public const string LinkMarketingProject = "Link Marketing Project";
		public const string CreateSalesOrder = "Create Sales Order";
	    public const string CreateQuote = "Create Quote";
	    public const string MarkAsPrimary = "Mark As Primary";
        public const string ViewSalesOrder = "View Sales Order";
		public const string ViewCase = "View Case";
		public const string Employee = "Employee";
		public const string Workgroup = "Workgroup";
		public const string Router = "Router";
        public const string InventoryItem = IN.Messages.InventoryItem;
		public const string OpportunityDocDisc = "Opportunity Document Discount";
		public const string Position = "Position";

		public const string CRQuote = "Sales Quote";
		public const string PMQuote = "Project Quote";
		public const string QuoteProducts = "Quote Products";
        public const string QuoteTax = "Quote Tax";
        public const string QuoteDiscount = "Discount Details";
        public const string QuoteContact = "Quote Contact";
        public const string QuoteAddress = "Quote Address";
        public const string QuoteCopy = " - copy";
        public const string QuoteGridProductText = "Subtotal: {0}, Line Discount Subtotal: {1}";

        [Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string SendArticlesByMail = "Send";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string SendArticlesByMailToolTip = "Sends all articles by e-mail";
		public const string AddArticle = "New";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddArticleToolTip = "Initializes a new record";
		public const string ViewArticle = "Details";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ViewArticleToolTip = "Shows current article";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NoEmailParameter = "There is no '((email))' parameter in content of the template";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NoEmailTemplate = "Given template is no published for current and default languages";
		public const string EmptyValueErrorFormat = "'{0}' may not be empty.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string EmailFromFormat = "Email from {0}";
		public const string AccessDenied = "Access denied";
		public const string NoAccessRightsToScreen = "You do not have enough rights to access the screen {0} ({1})";
		public const string NoAccessRightsTo = "You do not have enough rights to access {0}";
		public const string NoItemsFound = "No records matching your query were found.";
		public const string BAccount = "Customer";
		public const string LocationID = "Location ID";
		public const string LeadTimeDays = "Lead Time (Days)";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PrimaryAccountID = "Primary Account ID";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PrimaryContact = "Primary Contact";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PrimaryContract = "Primary Contract";
		public const string CustomerID = "Customer ID";

	    public const string EmailNotificationTemplate = "Notification Template";
	    public const string EmailActivityTemplate = "Activity";
	    public const string KnowledgeBaseArticle = "KB Article";

		public const string CompleteTaskTooltip = "Complete (Ctrl + K)";
		public const string CompleteTaskAndFollowUp = "Complete & Follow-Up";
		public const string CompleteTaskAndFollowUpTooltip = "Complete & Follow-Up (Ctrl + Shift + K)";

		public const string Shortcuts = "Shortcuts";

		public const string CreateAccount = "Create Account";
        public const string CreateContact = "Create Contact";
        public const string CreateOpportunity = "Convert to Opportunity";
        public const string CopyQuote = "Copy Quote";
        public const string RecalculatePrice = "Recalculate Prices";
        public const string SummitForApproval = "Submit Quote";
        public const string EditQuote = "Edit Quote";

        public const string IBqlFieldMustBeNested = "Field '{0}' must be nested in 'PX.Data.IBqlTable' type.";
		public const string IBqlTableMustBeInherited = "Type '{0}' must inherit 'PX.Data.IBqlTable' interface.";

		public const string NeedGraphType = "Type '{0}' must inherit 'PX.Data.PXGraph' type";
		public const string NeedBqlCommandType = "Type '{0}' must inherit 'PX.Data.BqlCommand' type";

		public const string GraphTypesAndConditionsLengthException = "The length of 'graphTypes' must be equal to the length of 'conditions'";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R2")]
		public const string Attention = "Attention";
		public const string JobTitle = "Job Title";

		public const string Unit = "Unit";

		public const string ThereAreManualSubscribers = "There are manual subscribers";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string EmailAccountDoesnotExist = "Account doesn't exist";

		public const string Subject = "Subject";
		public const string Description = "Description";

		public const string CaseEmailDefaultSubject = "[Case #{0}] {1}";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CaseEmailSubjectValidator = "[Case #{0}]";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ActivityStatusListException = "Attribute '{0}' only can be used with '{1}' DAC.";
	    public const string ActivityStatusValidation = "Status is not valid.";

		public const string StartDate = "Start Date";
		public const string DueDate = "Due Date";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CaseNotFound = "The case cannot be found or you doesn't have enough access rights";
		public const string AccountNotFound = "The account cannot be found or you doesn't have enough access rights";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadIsNotSelected = "The Lead is not selected";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadNotFound = "The Lead cannot be found or you doesn't have enough access rights";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AccountAssignmentMapIsNotSet = "Customer Assignment Map is not configured in CRSetup";

        public const string EmailNotificationError = "Email send failed. Email isn't created or email recipient list is empty.";
        public const string EmailNotificationObjectNotFound = "Email send failed. Source notification object not defined to proceed operation.";		
        public const string EmailNotificationSetupNotFound = "Email send failed. Notification Settings '{0}' not found.";

		public const string IncorrectMatching = "Incorrect value";

		public const string StockitemsCanNotBeIncludedIninvoice = "Stock items cannot be included in an AR invoice. You may want to create a sales order instead.";
		public const string NonStockitemsNotSelectedForIninvoice = "Only non-stock items can be included in an AR invoice. You may want to create a sales order instead.";

		public const string RequiredAttributesAreEmpty = "There are empty required attributes: {0}";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NothingToMerge = "No records to merge. Please select at least two records to proceed operation.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string MergeNoConflict = "There are no conflicted values. Do you want to continue and delete {0} records?";

		public const string CRSetupFieldsAreEmpty = "Some fields are empty or have not correct value: {1} in CRSetup";
		public const string DuplicateValidationRulesAreEmpty = "Duplicate Validation rules are not set in CRSetup";
		public const string CaseSearchTitle = "Case: {0} - {2}";
		public const string OpportunitySearchTitle = "Opportunity: {0} - {2}";
		public const string ProjectQuotesSearchTitle = "Project Quote: {0} - {2}";
		public const string SalesQuotesSearchTitle = "Sales Quote: {0} - {2}";
		public const string OutlookPluginHttps = "The Outlook add-in works only if your Acumatica ERP instance is hosted over HTTPS.";
		public const string OutlookFeatureNotInstalled = "The Outlook Integration feature is disabled or not licensed. Please review the feature check box on the Enable/Disable Features (CS100000) form.";
		public const string OutlookSelectEmailRecipient = "Email Recipient";
		public const string DifferentBAccountID = "The selected contact belongs to a business account that differs from the business account associated with the sender contact.";

		public const string Low = "Low";
		public const string Normal = "Normal";
		public const string High = "High";

		public const string Inquiry = "Inquiry";
		public const string ManualAmount = "Manual Amount";

		public const string ExternalTaxVendorNotFound = TX.Messages.ExternalTaxVendorNotFound;
		public const string RecordAlreadyExists = AR.Messages.RecordAlreadyExists;
		public const string FailedRecordCost = "The system failed to record cost transactions to the Project module.";
		public const string FailedGetFromAddressSO = SO.Messages.FailedGetFromAddressSO;
		public const string FailedGetToAddressSO = SO.Messages.FailedGetToAddressSO;
	    public const string FailedGetFromAddressCR = "The system cannot obtain the From address from the document.";
	    public const string FailedGetToAddressCR = "The system cannot obtain the To address from the document.";
        public const string AttributesAlreadyAttached = "Attributes are already attached.";
		public const string CommandNotSpecified = "The Select command is not specified.";
		public const string IncorrectSelectExpression = "The {0} select expression is incorrect.";
		public const string AttributeCanOnlyUsedOnView = "Attribute '{0}' can be used on only the {1} view or its children.";
		public const string FromDescAddr = "From {0} {1}";
		public const string CustomerNotFound = "The customer was not found. A customer is required for the case to be billed.";
		public const string InvalidEmail = "The email address is invalid.";
		public const string IncorrectDataInField = "The data is incorrect in {0}.";
		public const string NoSubject = "<no subject>";
		public const string SenderNameAndEmail = "From {0} {1}";
		public const string GraphHaveNoExt = "The graph does not have defined extension: {0}.";
		public const string CannotCreateAccount = "The account cannot be created because the lead was changed improperly. Please refresh the page and try again.";
		public const string CannotCreateOpportunity = "The opportunity cannot be created because the lead was changed improperly. Please refresh the page and try again.";
		public const string WarningAboutAccountAddressChange = "If you make changes to the address, the changes will be also saved to the address of the business account specified for the related contact. If you want to save the changes only to the current record, select the Override check box.";
		#endregion

		#region Graph Names
		public const string CRAttributeMaint = "Attribute Maintenance";
		public const string ContactMaint = "Contact Maintenance";
		#endregion
				
		#region Cache Names
		public const string CRSetup = "Customer Management Preferences";
		public const string BusinessAccount = "Prospect";
		public const string ParentBusinessAccount = "Parent Prospect";
		public const string BAccountCD = "Business Account";
		public const string BAccountName = "Business Account Name";
		public const string BAccountClass = "Business Account Class";
		public const string ParentAccount = "Parent Business Account";
		public const string ParentAccountID = "Parent Business Account";
		public const string ParentAccountName = "Parent Business Account Name";
		public const string ParentAccountNameShort = "Parent Account Name";
		public const string Class = "Class ID";
		public const string LeadContact = "Lead/Contact";
		public const string Contact = "Contact";
		public const string Primary = "Primary";
		public const string Shipping = "Shipping";
        [Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TasksEvents = "Tasks and Events";
		public const string Activities = "Activities";
		public const string Contracts = "Contracts";
		public const string Orders = "Orders";
		public const string Opportunities = "Opportunities";
		public const string Cases = "Cases";
		public const string Contract = "Contract";
		public const string ContractDescription = "Contract Description";
		public const string Answers = "Answers";
		public const string Articles = "Articles";
		public const string OpportunityProducts = "Opportunity Products";
		public const string OpportunityTax = "Opportunity Tax";
		public const string Address = "Address";
		public const string CampaignMember = "Campaign Members";
		public const string Subscriptions = "Subscriptions";
		public const string MailRecipients = "Mailing List Subscribers";
		public const string FreeItems = "Free Items";
		public const string DiscountDetails = "Discount Details";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CaseActivities = "Case Activities";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Subscribtion = "Subscription";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Unsubscribtion = "Unsubscription";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NewLead = "New Lead";
		public const string Match = "Match";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string MatchAll = "Match All";
		public const string MatchingRecords = "Matching Records";
		public const string Responses = "Responses";
		public const string Relations = "Relations";
		public const string MailList = "Marketing List";
		public const string PreviewSettings = "Preview Settings";
		public const string LocationExtAddress = "Location with Address";
		public const string ContactExtAddress = "Contact with Address";
		public const string ContactNotification = "Contact Notification";
		public const string Location = "Location";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ServiceCall = "Service Call";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ServiceCallItem = "Item";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ServiceCallLabor = "Labor";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ServiceCallMaterial = "Material";
		public const string SelectionCriteria = "Selection Criteria";
		public const string SelectionPreview = "Selection Preview";
		public const string Customer = "Business Account";
		public const string CustomerName = "Customer Name";
		public const string DiscountDetail = "Discount Detail";
		public const string BillingAddress = "Billing Address";
		public const string DestinationAddress = "Destination Address";
		public const string OpportunityClass = "Opportunity Class";
		public const string CustomerClass = "Customer Class";
		public const string CampaignStatus = "Campaign Status";
		public const string LeadClass = "Contact Class";
		public const string MassMail = "Mass Emails";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CaseForNotification = "Case for notification";
		public const string CRActivity = "Activity";
		public const string Criterion = "Criterion";
		public const string MergeMethod = "Method";
		public const string RecordForMerge = "Item For Merge";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string RecordMerged = "The record was merged successfully.";
		public const string MergeDocument = "Merge Document";
		public const string CommunacationClass = "Communication Summary";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string GetMore = "more..";
		public const string ReceiveAll = "Receive All";
		public const string Probability = "Opportunity Probability";
        public const string OpportunityStage = "Opportunity Stage";
        public const string LocationARAccountSub = "Location GL Accounts";
		public const string ContactBAccountID = "Customer ID";
		public const string ContactFullName = "Company Name";
		public const string BAccountAcctCD = "Parent Account ID";
		public const string BAccountType = "Business Account Type";
		public const string SystemEmail = "System Email";
		public const string TimeActivity = "Time Activity";
		public const string DuplicateValidationRules = "Duplicate Validation Rules";
		public const string EmailActivity = "Email Activity";
		public const string Reminder = "Reminder";
		public const string OpportunityDiscount = "Opportunity Discount";
		public const string MassMailMessage = "Mass Mail Message";
		public const string MassMailMembers = "Mass Mail Members";
		public const string MassMailMarketingListMember = "Mass Mail Marketing List Member";
		public const string MassMailCampaignMember = "Mass Mail Campaign Member";
		public const string MarketingListMember = "Marketing List Member";
		public const string TimeReactionBySeverity = "Time Reaction By Severity";
		public const string CaseReference = "Case Reference";
		public const string CaseClassLabor = "Case Class Labor";
		public const string CampaignClass = "Campaign Class";
		public const string ActivityStatistics = "Activity Statistics";
		public const string ActivityRelation = "Activity Relation";
		public const string LeadStatistics = "Lead Statistics";
		public const string CRTax = "CR Tax Detail";


		#endregion

		#region View Names

		public const string Selection = "Selection";
		public const string LeadsAndContacts = "Leads and Contacts";
		public const string Leads = "Leads";
		public const string Contacts = "Contacts";
		public const string MainContact = "Main Contact";
		public const string Locations = "Locations";
		public const string DeliverySettings = "Delivery Settings";
		public const string DeliveryContact = "Shipping Contact";
		public const string DeliveryAddress = "Shipping Address";
		public const string Campaign = "Campaign";
		public const string CampaignStats = "Campaign Statistics";
		public const string CampaignMembers = "Campaign Members";
		public const string BusinessAccounts = "Business Accounts";
		public const string Filter = "Filter";
		public const string OwnerUser = "Owner User";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string MergeProperties = "Merge Properties";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string UpdateProperties = "Update Properties";
		public const string MailLists = "Mail Lists";
		public const string MassMailSummary = "Summary";
		public const string Campaigns = "Campaigns";
		public const string Preview = "Preview";
		public const string View = "View";
		public const string Entity = "Entity";
		public const string EntityFields = "Entity Fields";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string FailedMessages = "Failed Messages";
		public const string SendedMessages = "Sended Messages";
		public const string History = "History";
		public const string MarketingList = "Marketing List Info";
		public const string EquipmentSummary = "Equipment Summary";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CallDetails = "Call Details";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CallDetailsSplit = "Call Details Split";
		public const string Scheduling = "Scheduling";
		public const string Labor = "Labor";
		public const string Materials = "Materials";
		public const string Taxes = "Taxes";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ConversionSummary = "Conversion Summary";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TaskSettings = "Task Information";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string EventSettings = "Event Information";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AccountAnswers = "Account Attributes";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string OpportunityAnswers = "Opportunity Attributes";
		public const string Document = "Document";
		public const string Criteria = "Criteria";
		public const string Methods = "Methods";
		public const string MergeItems = "Items";
		public const string Groups = "Groups";
		public const string Items = "Items";
		public const string Notifications = "Notifications";
		public const string CaseReferences = "Related Cases";
		public const string Attributes = "Attributes";
		public const string CaseClassReaction = "Reaction";
		public const string ImportSettings = "Settings";
        public const string CustomerPaymentMethodDetails = "Customer Payment Method Details";
        public const string OpportunityContact = "Opportunity Contact";
        public const string OpportunityAddress = "Opportunity Address";
        public const string CRContact = "Opportunity Contact";
        public const string CRAddress = "Opportunity Address";
        public const string ShippingContact = "Shipping Contact";
        public const string ShippingAddress = "Shipping Address";
		public const string State = "State";
		public const string CarrierDescription = "Carrier Description";
		public const string NotificationTemplate = "Notification Template";
        public const string OpportunityClassProbabilities = "Opportunity Class Stages";

		#endregion

		#region Combo Values
		// PhoneType
		public const string Business1 = "Business 1";
		public const string Business2 = "Business 2";
		public const string Business3 = "Business 3";
		public const string BusinessAssistant1 = "Business Assistant 1";
		public const string BusinessFax = "Business Fax";
		public const string Home = "Home";
		public const string HomeFax = "Home Fax";
		public const string Cell = "Cell";

		//Title
		public const string Doctor = "Dr.";
		public const string Miss = "Miss";
		public const string Mr = "Mr.";
		public const string Mrs = "Mrs.";
		public const string Ms = "Ms.";
		public const string Prof = "Prof.";

		//LocTypeList
		public const string CompanyLoc = "Company";
		public const string VendorLoc = "Vendor";
		public const string CustomerLoc = "Customer";
		public const string CombinedLoc = "Customer & Vendor";
		public const string EmployeeLoc = "Employee";

		//BAccountType
		public const string VendorType = "Vendor";
		public const string CustomerType = "Customer";
		public const string CombinedType = "Customer & Vendor";
		public const string EmployeeType = "Employee";
		public const string EmpCombinedType = "Customer & Employee";
		public const string ProspectType = "Prospect";

		[Obsolete(Common.Messages.FieldIsObsoleteRemoveInAcumatica8)]
		public const string CompanyType = "Company";

		public const string BranchType = "Branch";
		public const string OrganizationType = "Company";
		public const string OrganizationBranchCombinedType = "Company Without Branches";

		//BAccount.status
		public const string Active = "Active";
		public const string Hold = "On Hold";
		public const string HoldPayments = "Hold Payments";
		public const string Inactive = "Inactive";
		public const string OneTime = "One-Time";
		public const string CreditHold = "Credit Hold";

		//AddressTypes
		public const string BusinessAddress = "Business";
		public const string HomeAddress = "Home";
		public const string OtherAddress = "Other";

		//ContactTypes
		public const string Person = "Contact";
		public const string SalesPerson = "Sales Person";
		public const string BAccountProperty = "Business Account";

		//LeadRating
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2020R1")]
		public const string Hot = "Hot";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2020R1")]
		public const string Warm = "Warm";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2020R1")]
		public const string Cold = "Cold";

		//CSAnswerType
		public const string Lead = "Lead";
		public const string Account = "Account";
		public const string Case = "Case";		
		public const string Opportunity = "Opportunity";		

		// CRMassMailStatus
		public const string Hold_MassMailStatus = "On Hold";
		public const string Prepared_MassMailStatus = "Prepared";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Approved_MassMailStatus = "Approved";
		public const string Sent_MassMailStatus = "Sent";

		// CRMassMailSource
		public const string MailList_MassMailSource = "Marketing Lists";
		public const string Campaign_MassMailSource = "Campaigns";
		public const string LeadContacts_MassMailSource = "Leads/Contacts/Employees";

		//AssignmentMapType
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Lead_AssignmentMapType = Lead;
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BAccount_AssignmentMapType = BAccount;
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Opportunity_AssignmentMapType = Opportunity;
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string Case_AssignmentMapType = Case;
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ExpenseClaim_AssignmentMapType = "Expense Claim";
				
		//RuleType
		public const string AllTrue = "All conditions are true";
		public const string AtleastOneIsTrue = "At least one condition is true";
		public const string AtleastOneIsFalse = "At least one condition is false";

		// CRM Sources
		[Obsolete("This source is not used anymore.")]
		public const string Web = "Web";
		[Obsolete("This source is not used anymore.")]
		public const string PhoneInq = "Phone Inquiry";
		[Obsolete("This source is not used anymore.")]
		public const string Referral = "Referral";
		[Obsolete("This source is not used anymore.")]
		public const string PurchasedList = "Purchased List";
		[Obsolete("This source is not used anymore.")]
		public const string Other = "Other";

        //QuoteApproval 
        public const string Approval = "Approval";

        //QuoteStatus 
        public const string Draft = "Draft";
        public const string Prepared = "Prepared";	    
        public const string Sent = "Sent";
        public const string PendingApproval = "Pending Approval";
        public const string Rejected = "Rejected";        

        //QuoteType
        public const string QuoteTypeDistribution = "Sales Quote";
        public const string QuoteTypeProject = "Project Quote";

        //OpportunityLineType
        public const string OpportunityLineTypeDistribution = "Distribution";
        public const string OpportunityLineTypeScopeOfWork = "Estimation";

        //LeadResolution
        [Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadResolutionAssigned = "Assigned";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadResolutionContacted = "Contacted";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadResolutionConverted = "Converted";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadResolutionNotConverted = "Not Converted";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadResolutionNotContacted = "Not Contacted";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string LeadResolutionDuplicate = "Duplicate";

		//ActivityMajorStatus
		public const string PreprocessFlag = "Preprocess";
		public const string ProcessingFlag = "Processing";
		public const string ProcessedFlag = "Processed";
		public const string FailedFlag = "Failed";
		public const string CanceledFlag = "Canceled";
		public const string CompletedFlag = "Completed";
		public const string DeletedFlag = "Deleted";
		public const string ReleasedFlag = "Released";
		public const string JustCreatedFlag = "Just Created";
		public const string OpenFlag = "Open";

		//CRActivityTypes
		public const string Appointment = "Appointment";
		public const string Email = "Email";
		public const string EmailResponse = "Email Response";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PhoneCall = "Phone Call";
		public const string Note = "Note";
		public const string Chat = "Chat";
		public const string Message = "Message";
		public const string WorkItem = "Work Item";	

		//CRContactMethods
		public const string MethodAny = "Any";
		public const string MethodEmail = "Email";
		public const string MethodMail = "Mail";
		public const string MethodFax = "Fax";
		public const string MethodPhone = "Phone";

		//CRMaritalStatuses
		public const string Single = "Single";
		public const string Married = "Married";
		public const string Divorced = "Divorced";
		public const string Widowed = "Widowed";

		//CRGenders
		public const string Male = "Male";
		public const string Female = "Female";

		//Case Sources
		public const string CaseSourceEmail = "Email";
		public const string CaseSourcePhone = "Phone";
		public const string CaseSourceWeb = "Web";
		public const string CaseSourceChat = "Chat";

		//CRValidationRules
		public const string LeadToContactValidation = "Lead to Contact";
		public const string LeadToAccountValidation = "Lead to Account";
		public const string AccountValidation = "Account";
        public const string AccountContactValidation = "Contact linked to another Business Account";

        //Transformation Rules
        public const string DomainName = "Domain Name";
		public const string None = "None";
		public const string SplitWords = "Split Words";

		//Duplicate Statuses
		public const string Validated = "Validated";
		public const string PossibleDuplicated= "Possible Duplicate";
		public const string NotValidated = "Not Validated";
		public const string Duplicated = "Duplicated";

		//BillingTypeList
		public const string PerCase = "Per Case";
		public const string PerActivity = "Per Activity";
		//Application
		public const string Portal = "Portal";
		public const string Backend = "Back-end";
		#endregion

		#region Aging
		public const string Last30days = "Last 30 Days";
		public const string Last3060days = "30 - 60 Days";
		public const string Last6090days = "60 - 90 Days";
		public const string Over90days = "Over 90 Days";
		#endregion

		// Notification 
		public const string CRNotification = "CR Notification";

        //Campaign Filter
        public const string All = "All";
        public const string NeverSent = "Never Sent";        

        #region Custom Actions
        public const string ViewContact = "Contact Details";
		public const string ViewLocation = "Location Details";
		public const string ViewContract = "Contract Details";
		public const string ViewOrder = "Order Details";

		public const string AddNewLocation = "Add Location";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddNewFakeLocation = " ";
		public const string SetDefault = "Set as Default";
		public const string ViewOnMap = "View on Map";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string RunTemplate = "Run Template";
		public const string SaveTemplate = "Save Template";
		public const string SaveAsTemplate = "Save as Template";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CancelTemplate = "Clear";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string RemoveTemplate = "Remove Template";
		public const string MergeLead = "Merge";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AssignLeads = "Assign";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AssignMap = "Assignment Map";
		public const string Search = "Search";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddNewSubscribers = "Add New Subscribers";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddCampaign = "Add Campaigns";
		public const string AddNewMembers = "Add New Members";
		public const string TrashCurrent = "Move to Trash";		
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TrashCurrentConfirm = "Current record will be moved to trash.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TrashSelectedConfirm = "Selected records will be moved to trash.";
		public const string DeleteSelected = "Delete Selected";
		public const string MultiDeleteTooltip = "Removes selected records";
		public const string ViewVendor = "View Vendor";
		public const string ViewCustomer = "View Customer";
		public const string ViewQuote = "View Quote";
		public const string ConvertToCustomer = "Convert To Customer";
		public const string ConvertToVendor = "Convert To Vendor";
		public const string Merge = "Merge";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string MergeAll = "Merge All";
		public const string Update = "Update";
		public const string AddMembers = "Add Members";
		public const string Add = "Add";
		public const string Remove = "Remove";
		public const string PropertyDisplayName = "Name";
		public const string PropertyValue = "Value";
		public const string Import = "Import";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ReSend = "Resend Message";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ReSendAllFailed = "Resend All Failed";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string OpportunityDetails = "Opportunity Details";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string BusinessAccountDetails = "Business Account Details";
		public const string Details = "Details";
		public const string ViewAllActivities = "View Activities";
		public const string ViewDetails = "View Details";
		public const string ViewDetailsTooltip = "Shows details of current record";
		public const string AddNew = "Add New";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddFakeNew = " ";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddNew2 = "New";
		public const string AddContact = "Add Contact";
		public const string AddNewContact = "New Contact";
		public const string LinkToEntity = "Link to Entity";
		[Obsolete]
		public const string AttachToAccount = "Attach to Account";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddNewFakeContact = " ";
		public const string AddNewLead = "Create Lead";
		public const string AddNewRecordToolTip = "Add New Record";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddNewFakeLead = " ";
		public const string AddNewOpportunity = "Add Opportunity";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddNewFakeOpportunity = " ";
		public const string MergeContact = "Merge Contacts";
		public const string MergeBAccount = "Merge Business Account";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddAll = "Add All";
		public const string Load = "Load";
		public const string Assign = "Apply Assignment Rules";
		public const string ShowCustomer = "Show Customers";
		public const string ShowProspect = "Show Prospects";
		public const string Release = "Release";
		public const string IncludeInactive = "Include Inactive";
		public const string SearchText = "Search";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ShowLeads = "Show Leads";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ShowContacts = "Show Contacts";
		public const string Convert = "Convert";
		public const string ConvertToContact = "Convert to Contact";
		public const string ConvertToOpportunity = "Convert to Opportunity";
		public const string ConvertToBAccount = "Create Business Account";
		public const string MarkAsValidated = "Mark as Validated";
        public const string ValidateAddresses = "Validate Addresses";
        public const string CloseAsDuplicate = "Close as Duplicate";
		public const string CheckForDuplicates = "Check for Duplicates";		
		public const string InsertArticle = "Insert Article";
		public const string InsertArticleToolTip = "Inserts article into the mail body";


		public const string AddNewCase = "Add Case";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string AddNewFakeCase = " ";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CaseDetails = "Case Details";
		public const string AddTask = "Add Task";
		public const string AddEvent = "Add Event";
		public const string AddActivity = "Add Activity";
		public const string AddEmail = "Add Email";
		public const string CreateEmail = "Create Email";
		public const string RegisterActivity = "Register Activity";
		public const string AddTypedActivityFormat = "Add {0}";
		public const string SendMail = "Send Email";

		public const string CompleteActivity = "Complete Activity";
		public const string CancelActivity = "Cancel Activity";
		public const string ViewActivity = "View Activity";
		public const string ViewCompletedActivity = "View Completed Activity";
		public const string CopyFromCompany = "Copy from Company";
		//public const string AssignmentUp = "Up";
		//public const string AssignmentDown = "Down";
        public const string AssignmentUp = " ";
        public const string AssignmentDown = " ";
        public const string ShowContact = "Show Contact";
		public const string Delete = "Delete";
		public const string Send = "Send";
		public const string SendAll = "Send All";
		public const string Reply = "Reply";
		public const string ReplyAll = "Reply All";
		public const string Forward = "Forward";
		public const string LinkTo = "Link To";
		public const string MarkAsRead = "Mark as Read";
		public const string MarkAsUnread = "Mark as Unread";
		public const string Restore = "Restore";
		public const string RestoreFromArchive = "Restore from Archive";
		public const string Archive = "Archive";

		public const string PreviewMessage = "Test Message";
		public const string LoadTemplate = "Load Template";
		public const string CreateInvoice = "Create Invoice";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string SetAsPrimary = "Set as Primary";
		public const string Complete = "Complete";
		public const string Cancel = "Cancel";
		public const string Prev = "Prev";
		public const string Next = "Next";
		public const string EndTimeLTStartTime = "End Time cannot be less than Start Time";

		public const string Actions = "Actions";

		public const string Prepare = "Prepare";
		public const string PrepareAll = "Prepare All";
		public const string Process = "Process";
		public const string ProcessAll = "Process All";

		public const string LastActivity = "Last Activity";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string MassMergeActionName = "Merge";

		public const string OpenActivityOwner = "OpenActivityOwner";
		public const string ConvertToContactOneWord = "ConvertToContact";
		#endregion

		#region Target Entity Type
		public const string APInvoice = "AP Invoice";
		public const string ARInvoice = "AR Invoice";
		public const string CRCustomer = "Customer";
		public const string Vendor = "Vendor";
		public const string Prospect = "Prospect";
		public const string ExpReceipt = "Expense Receipt";
		public const string POrder = "Purchase Order";
		public const string SOrder = "Sales Order";
		#endregion

		public static string GetLocal(string msg)
		{
			return PXLocalizer.Localize(msg, typeof(Messages).ToString());
		}

		public static string[] GetLocal(string[] msgs)
		{
			if (msgs == null)
				return null;

			string[] res = new string[msgs.Length];
			for (int i = 0; i < msgs.Length; i++)
			{
				res[i] = PXLocalizer.Localize(msgs[i], typeof(Messages).ToString());
			}
			return res;
		}

		public static string FormNoAccessRightsMessage(Type graphType)
		{
			PXSiteMapNode sitemap = PXSiteMap.Provider.FindSiteMapNodeUnsecure(graphType);
			if (sitemap != null && sitemap.ScreenID != null)
			{
				return string.Format(GetLocal(Messages.NoAccessRightsToScreen), sitemap.Title, sitemap.ScreenID);
			}
			else
			{
				return string.Format(GetLocal(Messages.NoAccessRightsTo), graphType.ToString());
			}
		}
	}
}
