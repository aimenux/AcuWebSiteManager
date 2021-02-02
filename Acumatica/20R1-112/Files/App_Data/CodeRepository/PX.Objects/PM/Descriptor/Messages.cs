using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.CarrierService;
using PX.Data;
using PX.Common;

namespace PX.Objects.PM
{
	[PXLocalizable(Messages.Prefix)]
	public static partial class Messages
	{
		#region Validation and Processing Messages
		public const string Prefix = "PM Error";
		public const string Account_FK = "Account Group cannot be deleted. One or more Accounts are mapped to this Account Group.";
		public const string AccountDiactivate_FK = "Account Group cannot be deactivated. One or more Accounts are mapped to this Account Group.";
		public const string ProjectStatus_FK = "Account Group cannot be deleted. Project Status table contains one or more references to the given Account Group.";
		public const string OnlyPlannedCanbeDeleted = "The task cannot be deleted. You can delete a task with only the Planning or Canceled status.";
		public const string StartEndDateInvalid = "Planned Start Date for the given Task should be before the Planned End Date.";
		public const string UncompletedTasksExist = "Project can only be Completed if all Tasks are completed. {0} Task(s) are still incomplete.";
		public const string ProjectIsCanceled = "Project is Canceled and cannot be used for data entry.";
		public const string ProjectIsCompleted = "Project is Completed and cannot be used for data entry.";
		public const string ProjectIsSuspended = "Project is Suspended and cannot be used for data entry.";
		public const string ProjectTaskIsCompleted = "Task is Completed and cannot be used for data entry.";
		public const string ProjectTaskIsCompletedDetailed = "Task is Completed and cannot be used for data entry. Project:'{0}' Task:'{1}'";
		public const string ProjectTaskIsCanceled = "Task is Canceled and cannot be used for data entry.";
		public const string HasRollupData = "Cannot delete Balance record since it already has rollup data associated with it.";
		public const string HasTranData = "Cannot delete Task since it already has at least one Transaction associated with it.";
		public const string HasActivityData = "Cannot delete Task since it already has at least on Activity associated with it.";
		public const string HasTimeCardItemData = "Cannot delete Task since it already has at least one Time Card Item Record associated with it.";
		public const string ValidationFailed = "One or more rows failed to validate. Please correct and try again.";
		public const string NoAccountGroup = "Record is associated with Project whereas Account '{0}' is not associated with any Account Group";
		public const string InactiveProjectsCannotBeBilled = "Inactive Project cannot be billed.";
		public const string CancelledProjectsCannotBeBilled = "The project is canceled and cannot be billed.";
		public const string NoNextBillDateProjectCannotBeBilled = "Project can not be Billed if Next Billing Date is empty.";
		public const string NoCustomer = "This Project has no Customer associated with it and thus cannot be billed.";
		public const string BillingIDIsNotDefined = "Billing Rule must be defined for the task for Auto Budget to work.";
		public const string FailedToEmulateExpenses = "Failed to emulate Expenses when running Auto Budget. Probably there is no Expense Account Group in the Budget.";
		public const string FailedToCalcDescFormula = "Failed to calculate Description formula in the allocation rule:{0}, Step{1}. Formula:{2} Error:{3}";
		public const string FailedToCalcAmtFormula = "Failed to calculate Amount formula in the allocation rule:{0}, Step{1}. Formula:{2} Error:{3}";
		public const string FailedToCalcQtyFormula = "Failed to calculate Quantity formula in the allocation rule:{0}, Step{1}. Formula:{2} Error:{3}";
		public const string FailedToCalcBillQtyFormula = "Failed to calculate Billable Quantity formula in the allocation rule:{0}, Step{1}. Formula:{2} Error:{3}";
		public const string FailedToCalcDescFormula_Billing = "Failed to calculate the line description using the {3} step of the {0} billing rule. Line Description Formula: {1} Error: {2}";
		public const string FailedToCalcAmtFormula_Billing = "Failed to calculate the amount using the {3} step of the {0} billing rule. Line Amount Formula: {1} Error: {2}";
		public const string FailedToCalcQtyFormula_Billing = "Failed to calculate the quantity using the {3} step of the {0} billing rule. Line Quantity Formula: {1} Error: {2}";
		public const string FailedToCalcInvDescFormula_Billing = "Failed to calculate the invoice description using the {3} step of the {0} billing rule. Invoice Description Formula: {1} Error: {2}";
		public const string ResultNotReturned = "No result has been returned. Please check Trace for details.";
		public const string PeriodsOverlap = "Overlapping time intervals are not allowed";
		public const string Activities = "Activities";
		public const string RangeOverlapItself = "Range for the summary step should not refer to itself.";
		public const string RangeOverlapFuture = "Range for the summary step should not refer future steps.";
		public const string ReversalExists = "Reversal for the given allocation already exist. Allocation can be reversed only once. RefNbr of the reversal document is {0}.";
		public const string TaskAlreadyExists = "Task with this ID already exists in the Project.";
		public const string AllocationStepFailed = "Failed to Process Step: {0} during Allocation for Task: {1}. Check Trace for details.";
		public const string DebitProjectNotFound = "Step '{0}': Debit Project was not found in the system.";
		public const string CreditProjectNotFound = "Step '{0}': Credit Project was not found in the system.";
		public const string DebitTaskNotFound = "The {0} step of the {1} allocation rule cannot assign the debit task. The {2} task has not been found for the {3} project.";
		public const string CreditTaskNotFound = "The {0} step of the {1} allocation rule cannot assign the credit task. The {2} task has not been found for the {3} project.";
		public const string AccountGroupInBillingRuleNotFound = "The {0} billing rule has the {1} account group that has not been found in the system.";
		public const string AccountGroupInAllocationStepFromNotFound = "The {0} account group specified as the From setting of the {1} step of the {2} allocation rule has not been found in the system.";
		public const string AccountGroupInAllocationStepToNotFound = "The {0} account group specified as the To setting of the {1} step of the {2} allocation rule has not been found in the system.";
		public const string ProjectInTaskNotFound = "Task '{0}' has invalid Project associated with it. Project with the ID '{1}' was not found in the system.";
		public const string TaskNotFound = "Task with the given id was not found in the system. ProjectID='{0}' TaskID='{1}'";
		public const string ProjectNotFound = "Project with the given id was not found in the system. ProjectID='{0}'";
		public const string AccountNotFound = "Account with the given id was not found in the system. AccountID='{0}'";
		public const string AutoAllocationFailed = "Auto-allocation of Project Transactions failed.";
		public const string AutoReleaseFailed = "Auto-release of allocated Project Transactions failed. Please try to release this document manually.";
		public const string AutoReleaseARFailed = "Auto-release of ARInvoice document created during billing failed. Please try to release this document manually.";
		public const string AutoReleaseOfReversalFailed = "During Billing ARInvoice was created successfully. PM Reversal document was created successfully. Auto-release of PM Reversal document failed. Please try to release this document manually.";
		public const string BillingRuleAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the billing rule, but the account has not been specified for the {0} billing rule.";
		public const string BillingRuleAccountIsNotConfiguredForBillingRecurent = "Recurring billing for the {0} task and the {1} item has been configured to use the account from the recurring item, but the account has not been specified for the recurring item.";
		public const string AccountGroupAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the account group, but the default account has not been specified for the {1} account group.";
		public const string ProjectAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the project, but the default account has not been specified for the {1} project.";
		public const string ProjectAccountIsNotConfiguredForBillingRecurent = "The {0} recurrent item has been configured to use the account from the project, but the default account has not been specified for the {1} project.";
		public const string TaskAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to get its account from the task, but the default account has not been configured for the {1} task of the {2} project.";
		public const string TaskAccountIsNotConfiguredForBillingRecurent = "Recurring billing for the {0} task and the {1} item has been configured to use the account from the task, but the default account has not been specified for the {0} task of the {2} project.";
		public const string InventoryAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the inventory item, but the sales account has not been specified for the {1} inventory item.";
		public const string InventoryAccountIsNotConfiguredForBillingProjectFallback = "The {0} billing rule has been configured to use the sales account from the inventory item. In case the empty item code is specified for a project budget line, the default account of the project is used instead of the sales account of the inventory item, but no default account has been specified for the {1} project.";
		public const string InventoryAccountIsNotConfiguredForBillingRecurent = "Recurring billing for the {0} task and the {1} item has been configured to use the account from the inventory item, but the sales account has not been specified for the item.";
		public const string CustomerAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the customer, but the sales account has not been specified for the {1} customer.";
		public const string CustomerAccountIsNotConfiguredForBillingRecurent = "The {0} recurring item has been configured to use the account from the customer, but the sales account has not been specified for the {1} customer.";
		public const string EmployeeAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the employee, but the sales account has not been specified for the {1} employee.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2020R2")]
		public const string DefaultAccountIsNotConfiguredForBilling = "The {0} billing rule has been configured to use the sales account from the inventory item. In case the empty item code is specified for a project budget line, the default account of the project is used instead of the sales account of the inventory item, but no default account has been specified for the {1} project.";
		public const string SubAccountCannotBeComposed = "Billing Rule '{0}' will not be able the compose the subaccount since account was not determined.";
		public const string EmployeeNotInProjectList = "Project is configured to restrict employees that are not in the Project's Employee list. Given Employee is not assigned to the Project.";
		public const string RateTypeNotDefinedForStep = "Rate Type is not defined for step {0}";
		public const string RateTypeNotDefinedForBilling = "The rate type is not defined for the '{1}' step of the '{0}' billing rule.";
		public const string RateNotDefinedForStep = "The @Rate is not defined for the {1} step of the {0} billing rule. Check Trace for details.";
		public const string RateNotDefinedForStepAllocation = "The @Rate is not defined for the {1} step of the {0} allocation rule. Check Trace for details.";
		public const string InactiveWorkCode = "The {0} workers compensation code is not marked as Active on the Work Class Compensation Codes form.";
		public const string InactiveTask = "Project Task '{0}' is inactive.";
		public const string CompletedTask = "Project Task '{0}' is completed.";
		public const string TaskInvisibleInModule = "Project Task '{0}' is invisible in {1} module.";
		public const string InvisibleTask = "Project Task '{0}' is invisible.";
		public const string InactiveContract = "Given Project/Contract '{0}' is inactive";
		public const string CompleteContract = "Given Project/Contract '{0}' is completed";
		public const string TemplateContract = "Given Project/Contract '{0}' is a template";
		public const string InactiveUnion = "The {0} union local is inactive.";
		public const string ProjectInvisibleInModule = "The '{0}' project is invisible in the module.";
		public const string CancelledContract = "The {0} project or contract is canceled.";
		public const string DebitAccountGroupIsRequired = "Allocation Rule Step {0} is not defined correctly. Debit Account Group is required.";
		public const string AtleastOneAccountGroupIsRequired = "Allocation Rule Step {0} is not defined correctly. At least either Debit or Credit Account Group is required.";
		public const string DebitAccountEqualCreditAccount = "Debit Account matches Credit Account.";
		public const string DebitAccountGroupEqualCreditAccountGroup = "Debit Account Group matches Credit Account Group.";
		public const string AccountGroupIsRequired = "Failed to Release PM Transaction '{0}': Account Group is required.";
		public const string InvalidAllocationRule = "Allocation Step '{0}' is not valid. When applied to transactions in Task '{1}' failed to set Account Group. Please correct your Allocation rules and try again.";
		public const string PostToGLFailed = "Failed to Automatically Post GLBatch created during release of PM document.";
		public const string UnitConversionNotDefinedForItemOnBudgetUpdate = "Failed to Convert from {0} to {1} when updating the Budget for the Project. Unit conversion is not defined for {2}";
		public const string SourceSubNotSpecified = "Allocation rule is configured to use the source subaccount of transaction that is being allocated but the Subaccount is not set for the original transaction. Please correct your allocation step. Allocation Rule:{0} Step:{1}";
		public const string StepSubNotSpecified = "Allocation rule is configured to use the subaccount of allocation step but the subaccount is not set up. Please correct your allocation step. Allocation Rule:{0} Step:{1}";
		public const string OtherSourceIsEmpty = "Allocation rule is configured to take Debit Account from the source transaction and use it as a Credit Account of allocated transaction but the Debit Account is not set for the source transaction. Rule:{0} Step:{1} Transaction Description:{2}";
		public const string ProjectIsNullAfterAllocation = "In Step {0} Transaction that is processed has a null ProjectID. Please check the allocation rules in the preceding steps.";
		public const string TaskIsNullAfterAllocation = "In Step {0} Transaction that is processed has a null TaskID. Please check the allocation rules in the preceding steps.";
		public const string AccountGroupIsNullAfterAllocation = "In Step {0} Transaction that is processed has a null AllocationID. Please check the allocation rules in the preceding steps.";
		public const string StepSubMaskSpecified = "Subaccount Mask is not set in allocation step. Please correct your allocation step. Allocation Rule:{0} Step:{1}";
		public const string RateTableIsInvalid = "One or more validations failed for the given Rate Table sequence. Combinations of entities within sequence must be unique. The following combinations are not unique:";
		public const string ProjectIdIsNotSpecifiedForActivity = "ProjectID is not specified for the activity {0}:{1}";
		public const string ProjectRefError = "This record cannot be deleted. One or more projects are referencing this document.";
		public const string ValueMustBeGreaterThanZero = "The value must be greater than zero";
		public const string LocationNotFound = "Failed to create an allocation transaction. The location specified for the task is not valid. Check the following error for more details. {0}";
		public const string GenericFieldErrorOnAllocation = "Failed to create an allocation transaction. Check the following error for more details. {0}";
		public const string CommitmentsAutoCannotBeModified = "Commitment records with type 'Auto' cannot be deleted or modified.";
		public const string OtherUomUsedInTransaction = "Cannot set the UOM on the budget line. There already exists one or more transactions with a different UOM.";
		public const string UomNotDefinedForBudget = "The value of the Actual Qty. will not be updated if no UOM is defined.";
		public const string PrepaidAmountDecreased = "The Prepaid Amount can only be decreased from the auto assigned value.";
		public const string PrepaimentLessThanInvoiced = "The Prepaid Amount can not be decreased less than the already invoiced amount.";
		public const string ProjectExpired = "The project is expired.";
		public const string ProjectTaskExpired = "The project task is expired.";
		public const string TaskIsCompleted = "The project task is completed.";
		public const string PrepaymentAmointExceedsRevisedAmount = "The Prepaid Amount exceeds the uninvoiced balance.";
		public const string NoProgressiveRule = "The billing rule of the task contains only Time and Material steps. The Completed (%) and Pending Invoice Amount columns are not used for billing.";
		public const string BudgetNotDefinedForCostCode = "The budget is not defined for the cost code. Either define the budget or select a different cost code.";
		public const string UnreleasedProforma = "Pro Forma documents should be released in the sequence they were created. You cannot release the document until you release the following documents that precede the current one: {0}.";
		public const string EmailSendFailed = "Cannot send the email. Mailing settings are not configured on the Project Preferences (PM101000) form. Please check Trace for details.";
		public const string GroupedAllocationsBillLater = "The selected option is not available when a line represents a group of allocated transactions.";
		public const string DefaultAccountInProjectDoesnotMatchAccountGroup = "The '{0}' billing rule is configured to get its Sales Account from the Project but the selected '{1}' account does not match the '{2}' account group of the revenue budget.";
		public const string DefaultAccountInTaskDoesnotMatchAccountGroup = "The '{0}' billing rule is configured to get its Sales Account from the Task but the selected '{1}' account does not match the '{2}' account group of the revenue budget.";
		public const string SalesAccountInItemDoesnotMatchAccountGroup = "The '{0}' billing rule is configured to get its Sales Account from the Inventory Item but the selected '{1}' account does not match the '{2}' account group of the revenue budget.";
		public const string NonProjectCodeIsInvalid = "Non-Project is not a valid option.";
		public const string Overlimit = "The validation of the Max Limit Amount value has failed. Do one of the following: adjust the amounts of the document, adjust the limits of the budget, or select Ignore in the Validate T&M Revenue Budget Limits box on the Project Preferences (PM101000) form.";
		public const string OverlimitHint = "The validation of the Max Limit Amount has failed.";
		public const string UnreleasedPreviousInvoice = "You cannot release the pro forma invoice until you release the {1} {0} on the Invoices and Memos (AR301000) form.";
		public const string UnreleasedProformaOrInvoice = "All existing pro forma and Accounts Receivable invoices of the project have to be released before changing this setting.";
		public const string RevenueAccountIsNotMappedToAccountGroup = "Revenue Account {0} is not mapped to Account Group.";
		public const string SalesAccountIsNotMappedToAccountGroup = "The {0} billing step of the {1} billing rule failed. The {2} sales account, which is taken from the {3}, is not included in any account group.";
		public const string AccountIsNotAssociatedWithAccountGroup = "In the cost PM transaction emulated during the auto-budget process, the {0} debit account is not associated with the {1} account group.";
		public const string DefaultAccountIsNotConfigured = "Default Account is required but not configured for Account group: {0}";
		public const string ReservedForProject = "Item reserved for Project Module to represent N/A item.";
		public const string NoBillingRule = "The invoice cannot be created because no billing rule is specified for the task.";
		public const string InclusiveTaxNotSupported = "Inclusive taxes are not supported. ";
		public const string EmptyValuesFromExternalTaxProvider = AP.Messages.EmptyValuesFromExternalTaxProvider;
		public const string NewCommitmentIsNegative = "The value of a new commitment line cannot be negative.";
		public const string CommitmentCannotbeDecreased = "The negative change cannot be applied because the value of the resulting document line cannot be negative or less than the received or billed value.";
		public const string DuplicateChangeOrderNumber = "The project already has the {0} change order with this number.";
		public const string FailedGetFromAddress = "The system has failed to obtain the From address from the pro forma invoice.";
		public const string FailedGetToAddress = "The system has failed to obtain the To address from the pro forma invoice.";
		public const string CommitmentExistForThisProject_Enable = "Before you enable the change order workflow for the project, please make sure that all the related purchase order lines of the project have one of the following statuses: Completed, Closed, or Canceled.";
		public const string CommitmentExistForThisProject_Cancel = "Before canceling change order workflow for the project, please make sure that the project has no related non-canceled purchase order lines.";
		public const string ChangeOrderExistsForThisProject = "Before canceling change order workflow for the project, please make sure that the project has no related change orders.";
		public const string ProjectCommintmentsLocked = "To be able to create original purchase order commitments for this project, perform the Unlock Commitments action for the project on the Projects (PM301000) form.";
		public const string CostCodeNotInBudget = "The {0} cost code is not present in the project budget with the combination of the {1} project task and the {2} account group.";
		public const string CannotDeleteDefaultCostCode = "This is a system record and cannot be deleted.";
		public const string CannotModifyCostCode = "Cost code number cannot be updated directly. Use the Change ID action.";
		public const string ChangeOrderContainsRevenueBudget = "The change order class you are about to select does not support project revenue budget modification. Before disabling revenue budget modification for the change order, please make sure there are no change order lines affecting project revenue budget.";
		public const string ChangeOrderContainsCostBudget = "The change order class you are about to select does not support project cost budget modification. Before disabling cost budget modification for the change order, please make sure there are no change order lines affecting project cost budget.";
		public const string ChangeOrderContainsDetails = "The change order class you are about to select does not support project commitments modification. Before disabling commitments modification for the change order, please make sure there are no change order lines affecting project commitments.";
		public const string ClassContainsRevenueBudget = "Before disabling revenue budget modification for the change order class, please make sure there are no change orders belonging to this class that affect project revenue budget.";
		public const string ClassContainsCostBudget = "Before disabling cost budget modification for the change order class, please make sure there are no change orders belonging to this class that affect project cost budget.";
		public const string ClassContainsDetails = "Before disabling commitments modification for the change order class, please make sure there are no change orders belonging to this class that affect project commitments.";
		public const string ClassContainsCRs = "The change order class cannot be modified because it is already used in multiple entities.";
		public const string RetainageNotApplicable = "Invoice Total is negative and the credit memo will be created from the pro forma. Retainage Total has to be zero if Invoice Total is negative.";
		public const string TaskReferencesRequiredAttributes = "The project tasks cannot be activated because required attributes of the tasks have no values. Please, use the Project Tasks (PM302000) form to fill in required attribute values.";
		public const string AtleastOneTaskWasNotActivated = "At least one task could not be activated. Please, review the list of errors.";
		public const string DuplicateProjectCD = "A project with the given Project ID already exists.";
		public const string DuplicateTemplateCD = "A template with the given Template ID already exists.";
		public const string QuoteConversionFailed = "The quote cannot be converted.";
		public const string InactiveAccountGroup = "The {0} account group is inactive.";
		public const string OpportunityBAccount = "The opportunity business account is not equal to the quote account of the project.";
		public const string ClosedQuoteCannotBeDeleted = "Closed quote cannot be deleted.";
		public const string CannotDeleteUsedTask = "Cannot delete a project task that is already in use on the Estimation tab.";
		[Obsolete]
		public const string ClosedOpportunityCannotBeSelected = "The project quote cannot be linked to an opportunity with the Won or Lost status.";
		public const string QuoteCannotBeLinkedToNotActiveOpportunity = "The project quote cannot be linked to an opportunity that is not active.";
		public const string QuoteBAccountIsNotACustomer = "You cannot convert the project quote to a project because the type of the business account of the project quote is not Customer. Select a business account of the Customer type to proceed.";
		public const string MissingExpenseAccountGroup = "The extended cost is non-zero and the cost account group is empty. The line cannot be converted to the project budget. You need to either specify the cost account group or set the extended cost to zero to be able to convert the quote to a project.";
		public const string MissingRevenueAccountGroup = "The amount is non-zero and the revenue account group is empty. The line is not printed in the quote and cannot be converted to the project budget. You need to either specify the revenue account group or set the amount to zero to be able to convert the quote to a project.";
		public const string LicenseCostBudgetAndRevenueBudget  = "The total number of lines on the Cost Budget and Revenue Budget tabs has exceeded the limit set for the current license. Please reduce the number of lines to be able to save the document.";
		public const string LicenseCommitments = "The number of lines on the Commitments tab has exceeded the limit set for the current license. Please reduce the number of lines to be able to save the document.";
		public const string LicenseProgressBillingAndTimeAndMaterial = "The total number of lines on the Progress Billing and Time and Material tabs has exceeded the limit set for the current license. Please reduce the number of lines to be able to save the document.";
		public const string LicenseTasks = "The total number of lines on the Tasks tab has exceeded the limit set for the current license. Please reduce the number of lines to be able to save the document.";
        public const string QuoteIsClosed = "The quote cannot be marked as the primary quote of the {0} opportunity because the opportunity is linked to the closed {1} project quote.";
		public const string BudgetLineCannotBeDeleted = "The line cannot be deleted because the project budget is locked. Please unlock the project budget and try again.";
		public const string LockedBudgetLineCannotBeUpdated = "Original budgeted values of the selected project budget line will not be updated because the project budget is locked.";
		public const string ChangeOrderWorkflowLineCannotBeUpdated = "Revised budgeted values of the selected project budget line have not been updated because the change order workflow is enabled for the project.";
		public const string ProjectCuryCannotBeChanged = "The project currency cannot be changed because the project already has transactions.";
		public const string BillingCuryCannotBeChanged = "Another billing currency is not supported because the project currency {0} differs from the base currency {1}.";
		public const string FxTranToProjectNotFound = "The conversion rate from the transaction currency {0} to the project currency {1} cannot be found for the {2} rate type and {3:d} date.";
		public const string BillingCurrencyCannotBeChanged = "An invoice can be created for the {0} project only in the billing currency of the project.";
		public const string CurrencyRateIsNotDefined = "Please define a conversion rate from the {0} to {1} currency within the {2} currency rate type and the {3:d} effective date on the Currency Rates (CM301000) form.";
		public const string ConversionRateNotDefinedForCommitment = "The commitment cannot be created because the conversion rate from the purchase order currency {0} to the project currency {1} cannot be found for the {2} rate type and {3:d} date.";
		public const string FinPeriodForDateNotFound = "The financial period that corresponds to the date of at least one activity of a time card does not exist. Please, generate needed financial periods on the Master Financial Calendar (GL201000) form.";
		public const string OrderIsNotApproved = "The purchase order is pending approval. Please, approve the order first.";
		public const string OrderIsOnHold = "The purchase order is on hold. You need to first clear the On Hold check box for the purchase order.";
		public const string ActiveMigrationMode = "The operation is not available because the migration mode is enabled for accounts receivable.";
		public const string NoPermissionForInactiveTasks = "You have no permission to use project tasks with the Completed, Canceled, or In Planning status for data entry.";
		public const string ChnageOrderInvalidDate = "The financial period for the selected {0} date is not defined in the system. The change date is used for balance calculation and must belong to an existing financial period of the master calendar.";
		public const string ProformaDeletingRestriction = "You cannot delete the document with lines associated with the branches to which your user has no sufficient access rights.";
		public const string ProformaLineDeletingRestriction = "You cannot delete the line linked to a project transaction associated with the branch to which your user has no sufficient access rights.";
		public const string ChangeRetainageMode = "To enable the creation of pro forma invoices, the retainage mode must be changed first.";
		public const string CreateProformaRequired = "To select the Contract Cap mode, the creation of pro forma invoices must be enabled first.";
		public const string DuplicateID = "Duplicate ID";
		public const string EffectiveDateShouldBeGreater = "Effective date should be greater than {0}";
		public const string EffectiveDateShouldBeLess = "Effective date should be less than {0}";
		public const string NeedUpdate = "Need update instead of insert";
		public const string ExcludedFromBillingAsCreditMemoWrittenOff = "Written-Off with Credit Memo {0}";
		public const string ExcludedFromBillingAsCreditMemoResult = "Result of Credit Memo {0}";
		public const string ExcludedFromBillingAsARInvoiceResult = "Result of AR Invoice {0}";
		public const string ExcludedFromBillingAsReversal = "Reversal of Tran. ID {0}";
		public const string ExcludedFromBillingAsReversed = "Reversed";
		public const string ExcludedFromBillingAsWIPReversed = "WIP Reversed";
		public const string ExcludedFromBillingAsBillableWithCase = "Billable with Case {0}";
		public const string CannotDeleteAccountMappedToAG = "You cannot delete the account associated with an account group. Delete the {0} account from the {1} account group on the Account Groups (PM201000) form first.";
		#endregion

		#region Translatable Strings used in the code
		public const string ViewTask = "View Task";
		public const string ViewProject = "View Project";
		public const string ViewExternalCommitment = "View External Commitment";
		public const string NewTask = "New Task";
		public const string OffBalance = "Off-Balance";
		public const string SetupNotConfigured = "Project Management Setup is not configured.";
		public const string ViewBalance = "View Details";
		public const string ViewCommitments = "View Commitment Details";
		public const string ViewRates = "View Rates";
		public const string NonProjectDescription = "Non-Project Code.";
		public const string FullDetail = "Full Detail";
		public const string ProcAllocate = "Allocate";
		public const string ProcAllocateAll = "Allocate All";
		public const string ProcBill = "Bill";
		public const string ProcBillAll = "Bill All";
		public const string Release = "Release";
		public const string ReleaseAll = "Release All";
		public const string Approve = "Approve";
		public const string ApproveAll = "Approve All";
		public const string ViewTransactions = "View Transactions";
		public const string PrjDescription = "Project Description";
		public const string Bill = "Run Project Billing";
		public const string BillTip = "Runs billing for the Next Billing Date";
		public const string Allocate = "Allocate";
		public const string AddTasks = "Add Tasks";
		public const string ActivateTasks = "Activate Tasks";
		public const string CompleteTasks = "Complete Tasks";
		public const string AddCommonTasks = "Add Common Tasks";
		public const string CreateTemplate = "Create Template";
		public const string AutoBudget = "Auto-Budget Time and Material Revenue";
		public const string AutoBudgetTip = "Creates projected budget based on the expenses and Allocation Rules";
		public const string Actions = "Actions";
		public const string Filter = "Filter";
		public const string Reject = "Reject";
		public const string Assign = "Assign";
		public const string Reverse = "Reverse";
		public const string ApprovalDate = "Approval Date";
		public const string ReverseAllocation = "Reverse Allocation";
		public const string ReverseAllocationTip = "Reverses Released Allocation";
		public const string ViewAllocationSource = "View Allocation Source";
		public const string ViewBatch = "View Batch";
		public const string ProjectSearchTitle = "Project: {0} - {2}";
		public const string AccountGroup = "Account Group";
		public const string RateCode = "Rate Code";
		public const string Task = "Task";
		public const string TaskTotal = "Task Total";
		public const string Item = "Item";
		public const string FailedEmulateBilling = "The billing emulation cannot be run because of incorrect configuration. The sales account in the invoice is not mapped to any account group.";
		public const string InvalidScheduleType = "The schedule type is invalid.";
		public const string ArgumentIsNullOrEmpty = CR.Messages.ArgumentIsNullOrEmpty;
		public const string AllocationForProject = "Allocation for {0}";
		public const string AllocationReversalOnARInvoiceGeneration = "Allocation Reversal on AR Invoice Generation";
		public const string AllocationReversalOnARInvoiceRelease = "Allocation Reversal on AR Invoice Release";
		public const string ProjectAttributeNotSupport = "ProjectAttribute does not support the given module.";
		public const string ProjectTaskAttributeNotSupport = "ProjectTaskAttribute does not support the given module.";
		public const string FailedSelectProjectTask = "The system failed to select a project task with the given keys: ProjectID={0}, TaskID={1}.";
		public const string TaskIdEmptyError = "Task ID cannot be empty.";
		public const string TaskCannotBeSaved = "The {0} task cannot be saved. Please save the task with the Planned status first and then change the status to Active.";
		public const string CreateCommitment = "Create External Commitment";
		public const string ViewProforma = "View Pro Forma";
		public const string ViewPMTrans = "View Project Transactions";
		public const string Prepayment = "Prepayment";
		public const string Total = "Total:";
		public const string PMTax = "PM Tax Detail";
		public const string PMTaxTran = "PM Tax";
		public const string ViewChangeOrder = "View Change Order";
		public const string ChangeOrderPrefix = "Change Order #{0}";
		public const string ViewPurchaseOrder = "View Purchase Order";
		public const string NotAvailable = @"N/A";
		public const string RetaiangeChangedDialogHeader = "Default Retainage (%) Changed";
		public const string RetaiangeChangedDialogQuestion = "Update Retainage (%) in the revenue budget lines?";
		public const string RetaiangeChangedCustomerDialogQuestion = "Changing Customer will update the default project Retainage (%) from {0:f} to {1:f}. Would you also like to update Retainage (%) in the revenue budget lines?";
		public const string ReleaseRetainage = "Release Retainage";
		public const string PendingInvoiceAmountTotal = "Pending Invoice Amount Total: {0:n}";
		public const string QuoteNumberingIDIsNull = "The quote numbering sequence is not specified in the preferences of the Projects module.";
		public const string RunAllocation = "Run Allocation";
		public const string ConvertToProject = "Convert to Project";
		public const string TemplateChangedDialogHeader = "Quote Template Changed";
		public const string ValidateBalance = "Validate Project Balance";
		public const string TemplateChangedDialogQuestion = "Replace quote settings with the settings of the project template? The project tasks, attributes, and project manager will be replaced.";
		public const string AssetTotals = "Asset Totals";
		public const string LiabilityTotals = "Liability Totals";
		public const string IncomeTotals = "Income Totals";
		public const string ExpenseTotals = "Expense Totals";
		public const string OffBalanceTotals = "Off-Balance Totals";
		public const string SubmitProjectForApproval = "Submit Project for Approval";
		public const string Aggregated = "Aggregated: ";
		public const string UpdateQuoteByTemplateDialogHeader = "Update Quote by Template";
		public const string ViewBase = "View Base";
		public const string ViewCury = "View Cury";
		public const string CreatePurchaseOrder = "Create Purchase Order";
		public const string NewKey = "<NEW>";
		public const string RevenueBudgetFilter = "Revenue Budget Filter";
		public const string CostBudgetFilter = "Cost Budget Filter";
		public const string ProjectBillingRecord = "Project Billing Record";
		public const string ProjectUnionLocals = "Project Union Locals";
		public const string ProjectBalances = "Project Balances";
		#endregion

		#region Graph Names
		public const string RegisterEntry = "Register Entry";
		public const string ProjectEntry = "Project Entry";
		public const string ProjectTaskEntry = "Project Task Entry";
		public const string ProjectBalanceEntry = "Project Balance Entry";
		public const string ProjectBalanceByPeriodEntry = "Project Balance By Period Entry";
		public const string CommitmentEntry = "Commitment Entry";
		public const string ProjectAttributeGroupMaint = "Project Attribute Maintenance";
		public const string AccountGroupMaint = "Account Group Maintenance";
		public const string EquipmentMaint = "Equipment Maintenance";
		public const string RateDefinitionMaint = "Rate Definition Maintenance";
		public const string RateMaint = "Rate Maintenance";
		public const string BillingMaint = "Billing Maintenance";
		public const string RegisterRelease = "Register Release";
		public const string AllocationProcess = "Allocation Process By Task";
		public const string Process = "Process";
		public const string ProcessAll = "Process All";
		public const string AllocationProcessByProject = "Allocation Process By Project";
		public const string BillingProcess = "Billing Process";
		public const string ReverseUnbilledProcess = "Reverse Unbilled Process";
		public const string TransactionInquiry = "Transactions Inquiry";
		public const string PMSetup = "Project Management Setup";
		public const string PMSetupMaint = "Project Preferences";
		public const string TaskInquiry = "Tasks Inquiry";
		public const string TemplateMaint = "Project Template Maintenance";
		public const string TemplateTaskMaint = "Project Task Template Maintenance";
		public const string TemplateGlobalTaskListMaint = "Common Task List Template Maintenance";
		public const string TemplateGlobalTaskMaint = "Project Task Template Maintenance";
		public const string PMAllocator = "Project Allocator";
		public const string PMAddress = "PM Address";
		public const string PMContact = "PM Contact";
		public const string PMProFormaAddress = "PM ProForma Address";
		public const string PMProFormaContact = "PM ProForma Contact";
		#endregion

		#region View Names
		public const string Selection = "Selection";
		public const string ProjectAnswers = "Project Answers";
		public const string TaskAnswers = "Task Answers";
		public const string AccountGroupAnswers = "Account Group Answers";
		public const string EquipmentAnswers = "Equipment Answers";
		public const string QuoteAnswers = "Quote Answers";
		public const string PMTasks = "Tasks";
		public const string Approval = "Approval";
		public const string Commitments = "Commitments";
		public const string Budget = "Budget";
		public const string BudgetProduction = "Budget Production";
		public const string UnionLocals = "Union Locals";
		public const string Estimates = "Estimates";
        public const string RecurringItem = "Recurring Items";
		public const string ChangeRequest = "Change Request";
		public const string ChangeRequestLine = "Change Request Line";
		public const string Markup = "Markup";
		public const string ContractTotal = "Contract Total";
		#endregion

		#region DAC Names
		public const string Project = "Project";
		//public const string PMDocument = "PM Document";
		public const string ProjectTask = "Project Task";
		public const string PMTaskRate = "PM Task Rate";
		public const string PMProjectRate = "PM Project Rate";
		public const string PMItemRate = "PM Item Rate";
		public const string PMEmployeeRate = "PM Item Employee";
		public const string PMRate = "PM Rate";
		public const string PMRateType = "PM Rate Type";
		public const string PMRateTable = "PM Rate Table";
	    public const string PMProjectTemplate = "Project Template";
		public const string PMRateDefinition = "PM Rate Lookup Rule";
		public const string PMRateSequence = "PM Rate Lookup Rule Sequence";
		//public const string PMBillingRule = "PM Billing Rule";
		//public const string PMBilling = "PM Billing";
		public const string PMAccountTask = "PM Account Task";
		public const string PMAccountGroupRate = "PM Account Group Rate";
		//public const string PMAllocation = "PM Allocation";
		public const string PMAllocationDetail = "PM Allocation Rule";
		public const string PMAllocationSourceTran = "PM Allocation Source Transaction";
		public const string PMAllocationAuditTran = "PM Allocation Audit Transaction";
        public const string SelectedTask = "Tasks for Addition";
        public const string PMAccountGroupName = "Project Account Group Name";
        public const string PMTran = "Project Transaction";
        public const string PMProjectStatus = "Project Status";
        public const string PMHistory = "Project History";
		public const string Employee = "Employee";
		public const string Inventory = "Inventory";
		public const string CostCode = "Cost Code";
		public const string Proforma = "Pro Forma Invoice";
		public const string ProformaLine = "Pro Forma Line";
		public const string PMRegister = "Project Register";
		public const string BillingRule = "Billing Rule";
		public const string BillingRuleStep = "Billing Rule Step";
		public const string AllocationRule = "Allocation Rule";
		public const string Commitment = "Commitment Record";
		public const string ChangeOrderClass = "Change Order Class";
		public const string ChangeOrder = "Change Order";
		public const string ChangeOrderLine = "Change Order Line";
		public const string PMQuoteTask = "PM Quote Task";
		public const string PMLaborCostRate = "Labor Cost Rates";
		public const string PMForecast = "PM Budget Forecast";
		public const string PMForecastDetail = "PM Budget Forecast Detail";
		public const string PMForecastHistory = "PM Budget Forecast History";
		public const string PMProjectBalance = "Project Balance";
		#endregion

		#region Combo Values
		public const string NotStarted = "NotStarted";
		public const string Active = "Active";
		public const string Canceled = "Canceled";
		public const string Completed = "Completed";
		public const string InPlanning = "In Planning";
		public const string OnHold = "On Hold";
		public const string Suspend = "Suspended";
		public const string PendingApproval = "Pending Approval";
		public const string Open = "Open";
		public const string Closed = "Closed";
		public const string Rejected = "Rejected";

		public const string Hold = "Hold";
		public const string Balanced = "Balanced";
		public const string Released = "Released";
		public const string Reversed = "Reversed";

		public const string GroupTypes_Project = "Project";
		public const string GroupTypes_Task = "Task";
		public const string GroupTypes_AccountGroup = "Account Group";
		public const string GroupTypes_Equipment = "Equipment";


		public const string None = "None";
		public const string All = "All";

		public const string Origin_Source = "Use Source";
		public const string Origin_Change = "Replace";
		public const string Origin_FromAccount = "From Account";
		public const string Origin_None = "None";
		public const string Origin_DebitSource = "Debit Source";
		public const string Origin_CreditSource = "Credit Source";
        public const string Origin_Branch = "Specific Branch";

		public const string PMMethod_Transaction = "Allocate Transactions";
		public const string PMMethod_Budget = "Allocate Budget";

		public const string PMRestrict_AllProjects = "All Projects";
		public const string PMRestrict_CustomerProjects = "Customer Projects";

		public const string PMProForma_Select = "<SELECT>";
		public const string PMProForma_Release = "Release";

		public const string PMBillingType_Transaction = "Time and Material";
		public const string PMBillingType_Budget = "Progress Billing";

		public const string PMSelectOption_Transaction = "Not Allocated Transactions";
		public const string PMSelectOption_Step = "From Previous Allocation Steps";

		public const string MaskSource = "Source";
		public const string AllocationStep = "Allocation Step";
		public const string ProjectDefault = "Project Default";
		public const string TaskDefault = "Task Default";

		public const string OnBilling = "By Billing Period";
		public const string OnTaskCompletion = "On Task Completion";
		public const string OnProjectCompetion = "On Project Completion";

		public const string AccountSource_None = "None";
		public const string AccountSource_SourceTransaction = "Source Transaction"; 
		public const string AccountSource_BillingRule = "Billing Rule";
		public const string AccountSource_Project = "Project";
		public const string AccountSource_ProjectAccrual = "Project Accrual";
		public const string AccountSource_Task = "Task";
		public const string AccountSource_Task_Accrual = "Task Accrual";
		public const string AccountSource_InventoryItem = "Inventory Item";
		public const string AccountSource_LaborItem = "Labor Item";
		public const string AccountSource_LaborItem_Accrual = "Labor Item Accrual";
		public const string AccountSource_Customer = "Customer";
		public const string AccountSource_Resource = "Resource";
		public const string AccountSource_Employee = "Employee";
		public const string AccountSource_Branch = "Branch";
		public const string AccountSource_CurrentBranch = "Current Branch";
		public const string AccountSource_RecurentBillingItem = "Recurring Item";
		public const string AccountSource_AccountGroup = "Account Group";

		public const string Allocation = "Allocation";
		public const string Timecard = "Time Card";
		public const string Case = "Case";
		public const string ExpenseClaim = "Expense Claim";
		public const string EquipmentTimecard = "Equipment Time Card";
		public const string AllocationReversal = "Allocation Reversal";
		public const string Reversal = "Reversal";
		public const string Reversing = "Reversing";
		public const string ARInvoice = "Invoice";
		public const string CreditMemo = "Credit Memo";
		public const string DebitMemo = "Debit Memo";
		public const string APBill = "Bill";
		public const string CreditAdjustment = "Credit Adjustment";
		public const string DebitAdjustment = "Debit Adjustment";
		public const string UnbilledRemainder = "Unbilled Remainder";

		public const string UnbilledRemainderReversal = "Unbilled Remainder Reversal";
		public const string ProformaBilling = "Pro Forma Billing";
		public const string WipReversal = "WIP Reversal";
        public const string ServiceOrder = "Service Order";
        public const string Appointment = "Appointment";

		public const string RegularPaycheck = "Regular Paycheck";
		public const string SpecialPaycheck = "Special Paycheck";
		public const string AdjustmentPaycheck = "Adjustment Paycheck";
		public const string VoidPaycheck = "Void Paycheck";

		public const string PMReverse_OnARInvoiceRelease = "On AR Invoice Release";
		public const string PMReverse_OnARInvoiceGeneration = "On AR Invoice Generation";
		public const string PMReverse_Never = "Never";

		public const string PMNoRateOption_SetOne = "Set @Rate to 1";
		public const string PMNoRateOption_SetZero = "Set @Rate to 0";
		public const string PMNoRateOption_RaiseError = "Raise Error";
		public const string PMNoRateOption_NoAllocate = "Do not allocate";
		public const string PMNoRateOption_NoBill = "Do not bill";

		public const string PMDateSource_Transaction = "Original Transaction";
		public const string PMDateSource_Allocation = "Allocation Date";

		public const string Included = "Include Trans. created on billing date";
		public const string Excluded = "Exclude Trans. created on billing date";

		public const string Manual = "Manual";
		public const string ByQuantity = "Budgeted Quantity";
		public const string ByAmount = "Budgeted Amount";

		public const string CommitmentType_Internal = "Internal";
		public const string CommitmentType_External = "External";

		public const string TaskType_Combined = "Combined";
		public const string TaskType_Expense = "Cost";
		public const string TaskType_Revenue = "Revenue";

		public const string Progressive = "Progressive";
		public const string Transaction = "Transactions";

		public const string Option_BillNow = "Bill";
		public const string Option_WriteOffRemainder = "Write Off Remainder";
		public const string Option_HoldRemainder = "Hold Remainder";
		public const string Option_Writeoff = "Write Off";

		public const string BudgetLevel_Item = "Task and Item";
		public const string BudgetLevel_CostCode = "Task and Cost Code";
        public const string BudgetLevel_Detail = "Task, Item, and Cost Code";


        public const string Validation_Error = "Validate";
		public const string Validation_Warning = "Ignore";

		public const string BudgetUpdate_Detailed = "Detailed";
		public const string BudgetUpdate_Summary = "Summary";

		public const string ChangeOrderLine_Update = "Update";
		public const string ChangeOrderLine_NewDocument = "New Document";
		public const string ChangeOrderLine_NewLine = "New Line";
		public const string ChangeOrderLine_Reopen = "Reopen";

		public const string CostRateType_All = "All";
		public const string CostRateType_Employee = "Employee";
		public const string CostRateType_Union = "Union Wage";
		public const string CostRateType_Certified = "Prevailing Wage";
		public const string CostRateType_Project = "Project";
		public const string CostRateType_Item = "Labor Item";

		public const string BillingFormat_Summary = "Summary";
		public const string BillingFormat_Detail = "Detail";
		public const string BillingFormat_Progress = "Progress Billing";

		public const string ProjectMustBeSpecified = "The Project must be specified.";
		public const string DateFromMustBeSpecified = "The date From must be specified.";
		public const string DateToMustBeSpecified = "The date To must be specified.";
		public const string DocumentTypeMustBeSpecified = "The Document Type must be specified.";
		public const string Percentage = "%";
		public const string FlatFee = "Flat Fee";
		public const string Cumulative = "Cumulative %";
		
		public const string BudgetControlCalculate = "Calculate";
		public const string BudgetControlCalculateTip = "Calculate budget overruns";

		public const string BudgetControlOption_Nothing = "Do Not Control";
		public const string BudgetControlOption_Warn = "Show a Warning";

		public const string BudgetControlDocumentType_PurchaseOrder = "Purchase Order";
		public const string BudgetControlDocumentType_Subcontract = "Subcontract";
		public const string BudgetControlDocumentType_APBill = "AP Bill";
		public const string BudgetControlDocumentType_ChangeOrder = "Change Order";

		public const string BudgetControlWarning = "Budgeted: {0:F2}, Consumed: {1:F2}, Available: {2:F2}, Document: {3:F2}, Remaining: {4:F2}";
		public const string BudgetControlNotFoundWarning = "No budget is found. Budgeted: {0:F2}, Document: {1:F2}, Remaining: {2:F2}";
		public const string BudgetControlDocumentWarning = "The project budget is exceeded. For details, check warnings in the document lines.";

		public const string Retainage_Normal = "Standard";
		public const string Retainage_Contract = "Contract Cap";
		public const string Retainage_Line = "Contract Item Cap";

		#endregion


		#region Field Display Names

		public const string CreditAccountGroup = "Credit Account Group";
		public const string FinPTDAmount = "Financial PTD Amount";
		public const string FinPTDQuantity = "Financial PTD Quantity";
		public const string BudgetPTDAmount = "Budget PTD Amount";
		public const string BudgetPTDQuantity = "Budget PTD Quantity";
		public const string RevisedPTDAmount = "Revised PTD Amount";
		public const string RevisedPTDQuantity = "Revised PTD Quantity";
		public const string AccountedCampaign = "Accounted Campaign";
		#endregion

		public static string GetLocal(string message)
		{
			return PXLocalizer.Localize(message, typeof(Message).FullName);
		}
	}

	[PXLocalizable(Warnings.Prefix)]
	public static class Warnings
	{
		public const string Prefix = "PM Warning";

		public const string AccountIsUsed = "This account is already added to the '{0}' account group. By clicking Save, you will move the account to the currently selected account group.";
		public const string StartDateOverlow = "Start Date for the given Task falls outside the Project Start and End date range.";
		public const string EndDateOverlow = "End Date for the given Task falls outside the Project Start and End date range.";
		public const string ProjectIsCompleted = "Project is Completed. It will not be available for data entry.";
		public const string ProjectIsNotActive = "Project is Not Active. Please Activate Project.";
		public const string NothingToAllocate = "Transactions were not created during the allocation.";
		public const string NothingToBill = "Invoice was not created during the billing. Nothing to bill.";
		public const string ProjectCustomerDontMatchTheDocument = "Customer on the Document doesn't match the Customer on the Project or Contract.";
	}
}
