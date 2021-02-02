using System;
using PX.Common;

namespace PX.Objects.CT
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		#region Validation and Processing Messages
		public const string Prefix = "CT Error";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string RenewalDateIsNotsetup = "Renewal Date must be specified for the Renewable and Expiring contract types. Please specify the renewal date and run process again.";
		public const string ItemNotUnique = "Item with this type and Inventory ID already exists for the selected contract.";
		public const string DuplicateItem = "Duplicate Item Code {0}.";
		public const string DuplicateRecurringItem = "The contract cannot be activated because it contains duplicate recurring items {0}.";
		public const string QtyError = "Included Quantity must be within the Min and Max limits.";
		public const string QtyErrorWithParameters = "Included Quantity must be within the {0} and {1} limits.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CustomerRequiredForStatementCycle = "Statement-Based Billing Schedule is available when Customer ID is specified for the contract.";
		public const string StatementCycleIsNull = "Selected Customer do not have valid Statement Cycle assigned and cannot be configred for the Statement-Based billing. Please configure the Customer Statement Cycle or select different contract template.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string TypeNotValidForGivenContract = "This Type is not valid for the selected Contract.";
		public const string ContractAlreadyRenewed = "This Contract has already been renewed. An Expiring Contract can be renewed only once.  See {0}.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ContractLocationIsRequired = "Location for the given Contract is not Setup. Location is required for Billing procedure.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CustomerLocationIsRequired = "Default Location for the given Customer is not Setup. Location is required for Billing procedure.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string VirtualContractConstraint = "Virtual Contract (Contract without a Customer) cannot have Setup, Renewal, Re-Installment or Billing Items. Either remove these items from Contract or assign a Customer.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string VirtualContractType = "Virtual Contract (Contract without a Customer) must have a Unlimited Type only";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CRSetupIsNotConfigured = "CRSetup is not configured";
		public const string CancelledContarctCannotBeActivated = "Contract once Cancelled can not be Activated.";
		public const string ActiveContractCannotBeActivated = "Contract is already Active.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string DraftContractsCannotBeBilled = "Draft Contract cannot be Billed.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CancelledContractCannotBeBilled = "Contract once Cancelled can not be Billed.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NoNextBillDateContractCannotBeBilled = "Contract can not be Billed if Next Billing Date is empty.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string DraftContractsCannotBeRenewed = "Draft Contract cannot be Renewed.";
		public const string ContractRefError = "This record cannot be deleted. One or more contracts are referencing this document.";
		public const string VirtualContractCannotBeTerminated = "This type of Contract cannot be Terminated.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string RenewFeeNotCollected = "Contract cannot be renewed since the fee for the previos renewal has not been collected. Please run Contract Billing first to generate the Invoice for previous renewal.";
		public const string ItemNotPrice = "Item has no price in this Currency";
		public const string SpecificItemNotPrice = "{0} has no price in this Currency";
		public const string SpecificItemNotSpecificPrice = "{0} has no {1} in this Currency";
		public const string ItemsUOMConflict = "All Non-Stock items used to define a Contract Item must share same UOM. The Base Unit of current item differs from others.";
		public const string CustomerCuryNotMatchWithContractCury = "Customer Currency does not match with Contract Currency and Currency Overriding is not allowed for the Customer.";
		public const string InvoiceExistPostGivendate = "Invoice exists past the effective date.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string ItemWithoutPrice = "Item has no price in this Currency.";
		public const string ActivationDateError = "Activation Date of the contract cannot be earlier than the Start Date of the contract";
		public const string ContractIsNotDeposit = "Contract Item is not Deposit";
		public const string ContractDoesNotMatchDeposit = "Contract Item does not match with Current Item on Deposit";
		public const string CustomerDoesNotHaveParentAccount = "Customer does not have Parent Account";
		public const string CannotCalculateValue = "Extra Usage Total cannot be calculated.";
		public const string CannotUndoAction = "Last action can not be undone.";
		public const string CannotUndoActionDueToReleasedDocument = "The last action cannot be undone as the document has been released.";
        public const string UnreasedActivityExists = "During the last billing of the {0} contract all cases and activities must be already released. There exists at least one open or unreleased case or activity for this contract. The list of open cases or activities is recorded in the Trace.";
		public const string ItemOnDemandRecurringItem = "For contracts with billing on demand, items cannot have any recurring settings.";
		public const string ItemHasAnotherCuryID = "The contract item '{0}' cannot be added because its currency ({1}) does not match the {3} currency ({2})";
		public const string TemplateIsNotStarted = "Template is not effective yet.";
		public const string TemplateIsExpired = "Template is expired.";
		public const string TemplateIsNotActivated = "Template {0} is not activated.";
		public const string DepositBalanceIsBelowTheRetainingAmountThreshold = "The deposit has been fully used.";
		public const string RenewableContractContainsDepositItem = "Renewable contract can not contain deposit items.";
		public const string DepositItemGreaterThanOne = "A contract can not contain more than one deposit item.";
		public const string CannotRenewContract = "Cannot automatically renew contract when Auto Numbering is off. Please use Renew Contract action on Customer Contracts screen.";
		public const string DublicateContractCD = "Contract with this Contract ID '{0}' already exists. Please enter another Contract ID.";
		public const string ContractExpirationPrevNextBilling = "Expiration Date of the contract cannot be earlier than the Next Billing Date '{0}'.";
		public const string ContractCompletedExpiredCantSetUp = "Contract is Completed/Expired and cannot be Set up.";
		public const string ContractAlreadySetup = "Contract is already Set up.";
		public const string ContractTerminatedCantSetUp = "Contract is Terminated and cannot be Set up.";
		public const string ActivationDateTooLate = "Activation Date of a contract cannot be greater than its Expiration Date";
		public const string UpdateActivationDateTooEarlyOrTooLate = "Update can only be activated if the effective date is between the Last Billing Date and Next Billing Date";
		public const string ContractCompletedExpiredCantActivate = "Contract is Completed/Expired and cannot be Activated.";
		public const string ContractTerminatedCantActivate = "Contract is Terminated and cannot be Activated.";
		public const string ContractMustBeActive = "Contract must be Active.";
		public const string ContractTerminatedCantBill = "Contract is Terminated and cannot be Billed.";
		public const string ContractCompletedExpiredCantBill = "Contract is Completed/Expired and cannot be Billed.";
		public const string BillingDateMustBeSet = "Billing Date must be Set";
		public const string BillingDateGreaterThanExpiration = "Billing Date is greater than Expiration Date";
		public const string BillingDateLessThanScheduleStartDate = "Billing Date is less than Billing Schedule Start Date";
		public const string ExpireDateMissingCantRenew = "Contract Expire date must be not null for a Contract that is being Renewed.";
		public const string TerminatedContractCannotBeTerminated = "Contract is already Terminated";
		public const string TerminationDateTooEarly = "Termination date of a Contract cannot be earlier than the Last Billing Date of the Contract";
		public const string TerminationDateTooLate = "Termination date of a Contract cannot be later than the Next Billing Date of the Contract";
		public const string CantDeleteRenewingContract = "Cannot to delete Renewing Contract";
		public const string ContractCompletedExpiredCantUpgrade = "Contract is Completed/Expired and cannot be Upgraded.";
		public const string ContractTerminatedCantUpgrade = "Contract is Terminated and cannot be Upgraded.";
		public const string QuantityUnderDeposit = "Under Deposit contract item do not use {0}.";
		public const string OriginalContractIsNotFullyBilledInActivate = "The contract cannot be activated because the original contract '{0}' has unbilled usage or included fee. Run billing for the original contract first.";
		public const string OriginalContractIsNotFullyBilledInSetupAndActivate = "The contract cannot be activated because the original contract '{0}' has unbilled usage or included fee. Use Set Up Contract action or run billing for the original contract first.";
		public const string BillContractBeforeRenewal = "Contract must be billed before renewal.";
		public const string ContractExpirationDateCantBeCalculated = "Can't calculate Expiration Date for Contract '{0}'.";
		public const string ExpiraionDateWillBeRecalculated = "The contract expiration date will be recalculated based on the specified {0}.";
		public const string ContractIDAlreadyUsed = "This contract ID is already used by an existing contract or contract template.";
		public const string CannotUseKit = "A kit cannot be used on the [ScreenName] ([ScreenID]) form.";
		public const string ContractInventoryItemCantBeStock = "The {0} stock item cannot be used on the Contract Items (CT201000) form.";
		public const string ContractInventoryItemCantBeUnknown = "The {0} item cannot be used on the Contract Items (CT201000) form.";
		#endregion

		#region Translatable Strings used in the code
		public const string ViewInvoice = "View Invoice";
		public const string ViewUsage = "Contract Usage";
		public const string All = "All";
		public const string ViewContract = "View Contract";
		public const string ViewContractTemplate = "View Contract Template";
		public const string Renew = "Renew";
		public const string Bill = "Run Contract Billing";
		public const string Terminate = "Terminate Contract";
		public const string SetupContract = "Set Up Contract";
		public const string ActivateContract = "Activate Contract";
		public const string SetupAndActivateContract = "Set Up and Activate Contract";
		public const string Upgrade = "Upgrade Contract";
		public const string ActivateUpgrade = "Activate Upgrade";
		public const string UndoBilling = "Undo Last Action";
		public const string UndoBillingTooltip = "Undo last billing. Deletes the ARInvoice created during the last billing and shifts the Billing date of the contract back a period.";
		public const string ContractExpired = "Contract Expired";
		public const string ContractRenewed = "Contract Renewed";
		public const string PerCase = "Per-Case Billing";
		public const string PerItem = "Per-Activity Billing";
		public const string ContractSearchTitle = "Contract: {0} - {2}";


		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PrefixReinstallment = "Reinstallment";
		public const string PrefixOverused = "Overused";
		public const string PrefixIncluded = "Included";
		public const string PrefixPrepaid = "Prepaid";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PrefixRefundFor = "Refund for";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PrefixAggregatedUsage = "Aggregated Usage"; 
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PrefixAggregatedUsageIncluded = "Aggregated Usage Included";
		public const string Correction = "Correction";
		public const string PrefixIncludedUsage = "Included Usage";
		public const string PrefixPrepaidUsage = "Prepaid Usage";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string PrepaidInPortion = "Prepaid (in proportions for {0} days): {1}";
		public const string BillingFor = "Billing for [{0}]: {1}.";
		public const string ActivatingContract = "Contract Activation {0}: {1}.";
		public const string SettingUpContract = "Contract Setup {0}: {1}.";
		public const string UpgradingContract = "Contract Upgrade {0}: {1}.";
		public const string BillingContract = "Contract Billing {0}: {1}.";
		public const string RenewingContract = "Contract Renew {0}: {1}.";
		public const string TerminatingContract = "Contract Termination {0}: {1}.";

		public const string ActionInvoiceActivatingContract = "Contract Activation";
		public const string ActionInvoiceSettingUpContract = "Contract Setup";
		public const string ActionInvoiceUpgradingContract = "Contract Upgrade";
		public const string ActionInvoiceBillingContract = "Contract Billing";
		public const string ActionInvoiceRenewingContract = "Contract Renewal";
		public const string ActionInvoiceTerminatingContract = "Contract Termination";

		public const string Labels_DaysBeforeExpiration = "Days Before Expiration";
		public const string Labels_Days = "Days";
		public const string Labels_Min = "Min";

		public const string SetupDate = "Setup Date";
		public const string ActivationDate = "Activation Date";

		public const string Summary = "Summary";
		public const string Detail = "Detail";

		public const string ActivateRenew = "Activate/Renew: {0}";
		public const string SetupUpgrade = "Setup upgrade: {0}";
		public const string SetupDescription = "Setup: {0}";
		public const string UpgradeActivation = "Upgrade activation: {0}";

		public const string ActionItemActivateRenew = "Activate/Renew";
		public const string ActionItemSetupUpgrade = "Setup Upgrade";
		public const string ActionItemSetupDescription = Setup;
		public const string ActionItemUpgradeActivation = "Upgrade Activation";

		public const string Update = "Update";
		public const string UpdateAll = "Update All";
		public const string InvalidScheduleType = "The schedule type is invalid: {0}.";

		public const string SetupPriceMessage = "Setup Price";
		public const string RecurringPriceMessage = "Recurring Price";
		public const string RenewalPriceMessage = "Renewal Price";
		public const string SetupItem = "Setup Item";
		public const string RenewalItem = "Renewal Item";
		public const string RecurringItem = "Recurring Item";
		public const string ExtraUsagePrice = "Extra Usage Price";
		public const string ListOfUnreleasedBillableCases = "List of unreleased billable cases:";
		public const string ListOfUnreleasedBillableActivities = "List of unreleased billable activities:";
		public const string CaseActivityFormat = "Case: {0} Activity: {1}{2}";
		#endregion

		#region Graph Names
		public const string ContractBilling = "Contract Billing Process";
		public const string ContractMaint = "Contract Entry";
		public const string TemplateMaint = "Contract Template Maintenance";
		public const string UsageMaint = "Contract Usage Entry";
		public const string ExpiringContractsEng = "Expiring Contracts Inquiry";
		public const string RenewContracts = "Renew Contracts";
		#endregion

		#region DAC Names
		public const string CTContract = "Contract";
		public const string ContractTemplate = "Contract Template";
		public const string ContractDetail = "Contract Detail";
		public const string ContractUsage = "Contract Usage";
		public const string ContractBillingSchedule = "Contract Billing Schedule";
		public const string ContractBillingTrace = "Contract Billing Trace";
		public const string ContractItem = "Contract Item";
		public const string ContractRenewalHistory = "Contract Renewal History";
		public const string ContractSLAMapping = "Contract SLA Mapping";
		#endregion

		#region Combo Values

		#region Contract Item Type
		public const string Setup = "Setup";
		public const string Renewal = "Renewal";
		public const string Billing = "Billing";
		public const string UsagePrice = "Usage";
		public const string Reinstallment = "Re-Installment";
		#endregion

		public const string Renewable = "Renewable";
		public const string Expiring = "Expiring";
		public const string Unlimited = "Unlimited";

		public const string Draft = "Draft";

		public const string InApproval = "Pending Approval";
		public const string Active = "Active";
		public const string Expired = "Expired";
		public const string Canceled = "Canceled";
		public const string Completed = "Completed";
		public const string InUpgrade = "Pending Upgrade";
		public const string PendingActivation = "Pending Activation";
		public const string Used = "Used";
		public const string Included = "Included";

		#region DurationType

		public const string Annual = "Year";
		public const string SemiAnnual = "Half a Year";
		public const string Quarterly = "Quarter";
		public const string Monthly = "Month";
		public const string Custom = "Custom (days)";
		public const string Weekly = "Week";
		public const string OnDemand = "On Demand";
		
		#endregion


		#region ResetUsage

		public const string Never = "Never";
		public const string OnBilling = "On Billing";
		public const string OnRenewal = "On Renewal";

		#endregion
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CaseCreated = "Case Created";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string CaseClosed = "Case Closed";

		public const string StatementBased = "Statement-Based";
		public const string Contract = "Contract";
		public const string Inventory = "Inventory";


		public const string Case = "Case";
		public const string Task = "Task";
		public const string Activity = "Activity";
		public const string None = "None";

		public const string ItemPrice = "Use Item Price";
		public const string PercentOfItemPrice = "Percent of Item Price";
		public const string EnterManually = "Enter Manually";
		public const string PercentOfBasePrice = "Percent of Setup Price";

		public const string Prepay = "Prepaid";
		public const string Usage = "Postpaid";
		public const string Deposit = "Deposit";

		public const string ParentAccount = "Parent Account";
		public const string CustomerAccount = "Customer Account";
		public const string SpecificAccount = "Specific Account";
		#region AttributeEntity
		public const string AttributeEntity_Contract = "Contract";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string GroupTypes_Contract = "Contract";
		#endregion

		#endregion
	}

	[PXLocalizable]
	public class ActionMessages
	{
		public const string Create = "Create";
		public const string Activate = "Activate";
		public const string Bill = "Bill";
		public const string Renew = "Renew";
		public const string Terminate = "Terminate";
		public const string Upgrade = "Upgrade";
		public const string Setup = "Set Up";
		public const string SetupAndActivate = "Set Up and Activate";
	}
}
