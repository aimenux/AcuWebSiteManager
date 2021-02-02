using System;
using PX.Common;

namespace PX.Objects.CA
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		// Add your messages here as follows (see line below):
		// public const string YourMessage = "Your message here.";
		#region Validation and Processing Messages
		public const string Prefix = "CA Error";
		public const string CATranNotSaved = "An error occurred while saving CATran for the table '{0}'";
		public const string DocumentOutOfBalance = AP.Messages.DocumentOutOfBalance;
		public const string DocumentStatusInvalid = AP.Messages.Document_Status_Invalid;
		public const string DuplicatedPaymentTypeDetail = "Record already exists.";
		public const string DuplicatedPaymentMethodDetail = "Record already exists.";
		public const string CashAccountExists = "This ID is already used for another Cash Account record.";
		public const string ProcessingCenterInactive = "The processing center is deactivated.";
		public const string CashAccountInactive = "The cash account {0} is deactivated on the Cash Accounts (CA202000) form.";
		public const string CashAccount_MayBeCreatedFromDenominatedAccountOnly = "Only a denominated GL account can be converted to the Cash type.";
		public const string CashAccountNotReconcile = "The {0} cash account does not require reconciliation. Verify if the Requires Reconciliation check box is selected on the Cash Accounts (CA202000) form.";
		public const string ReleasedDocCanNotBeDel = "Released document cannot be deleted.";
		public const string TransferDocCanNotBeDel = "This transaction cannot be deleted. Use Cash Transfer Entry screen.";
		public const string TransferOutCAAreEquals = "The source cash account must be different from the destination cash account.";
		public const string TransferInCAAreEquals = "The destination cash account must be different from the source cash account.";
		public const string TransferCAAreEquals = "Destination Cash Account must be different from the source.";
		public const string GLTranExistForThisCashAcct = "One or more transactions recorded on the selected General Ledger account are not tracked in the Cash Management module. To synchronize the balances of the specified accounts, validate the balance of the cash account on the Validate Account Balances (CA.50.30.00) form.";
		public const string HoldExpenses = "The transfer {0} cannot be released. At least one expense associated with the transfer is on hold or pending approval. First clear the On Hold check box for an expense document and then release the transfer.";
		public const string HoldExpense = "The expense is on hold and cannot be released. Review the expense and clear the On Hold check box.";
		public const string CantEditDisbReceipt = "You cannot change the Type for the Entry Type if one or more transactions was entered already.";
		public const string CantEditModule = "You cannot change the Module for the Entry Type if one or more transactions was entered already.";
		public const string DuplicatedKeyForRow = "Record with this ID already exists.";
		public const string ERR_CashAccountHasTransactions_DeleteForbidden = "This Cash Account cannot be deleted as one or more transaction already exists.";
		public const string ERR_IncorrectFormatOfPTInstanceExpiryDate = "Incorrect date format provided.";
		public const string ERR_RequiredValueNotEnterd = "This field is required.";
		public const string ValueIsNotValid = "Provided value does not pass validation rules defined for this field.";
		public const string ProcessingCenterIsAlreadyAssignedToTheCard = "This Processing Center is already assigned to the Payment Method";
		public const string IncompatiblePluginForCardProcessing = "The plug-in you selected may be incompatible. Please consult with the provider of this plug-in.";
		public const string ProcessExpiredCardWarning = "Expired credit card payment methods will be deactivated, the sensitive information stored within them will be deleted from the system.";
		public const string PaymentMethodConverterWarning = "This operation cannot be reverted. Before converting the payment method, ensure that the {1} processing center is configured to connect to the same external processing center with the same credentials as the {0} processing center. Are you sure you want to convert the customer payment profiles from the {0} processing center to the {1} processing center?";
		public const string UseAcceptPaymentFormWarning = "The check box was selected automatically because this processing center allows accepting payments from new cards. Clear this check box if new cards should be registered on the Customer Payment Methods (AR303010) form only.";
		public const string DefaultProcessingCenterConfirmation = "Make this processing center default?";
		public const string PaymentMethodIsAlreadyAssignedToTheProcessingCenter = "This Payment Method is already assigned to the Processing Center";
		public const string RowIsDuplicated = "Row is duplicated";
		public const string RequiresReconNumbering = "Requires Reconciliation Numbering";
		public const string EntryTypeIDDoesNotExist = "This Entry Type ID does not exist";
		public const string TransactionNotComplete = "Transaction Not Complete";
		public const string TransactionNotFound = "Cash Transaction Not Found";
		public const string OneOrMoreItemsAreNotReleased = "One or more items are not released";
		public const string OneOrMoreItemsAreNotPosted = "One or more items are not posted";
		public const string OneOrMoreItemsAreNotReleasedAndStatementCannotBeCompleted = "One or more items are not released and statement cannot be completed";
		public const string DocNotFound = "Document Not Found";
		public const string APDocumentsCanNotBeReleasedFromCAModule = "AP Documents Can Not Be Released From CA Module";
		public const string ARDocumentsCanNotBeReleasedFromCAModule = "AR Documents Can Not Be Released From CA Module";
		public const string NotAllDocumentsAllowedForClearingAccount = "A document of this type cannot be recorded to this account. The account is selected as a clearing account on the Cash Accounts (CA202000) form.";
		public const string ThisDocTypeNotAvailableForRelease = "The document of this type cannot be released in the Cash Management module. Release the document in the module from which the document has originated.";
		public const string OriginalDocAlreadyReleased = "Original document has already been released";
		public const string CanNotVoidStatement = "There are newer non-voided statements.";
		public const string CanNotCreateStatement = "Can not create statement - current statement is not reconciled.";
		public const string CashAccounNotReconcile = "Cash account does not require reconciliation";
		public const string ReconciledDocCanNotBeNotCleared = "Reconciled document can not be not cleared";
		public const string ProcessingCenterIDIsRequiredForImport = "Processing CenterID is required for this operation";
		public const string ProcessingObjectTypeIsNotSpecified = "Type of the object for the Credit Card processing is not specified";
		public const string InstanceOfTheProcessingTypeCanNotBeCreated = "Instance of the Type {0} can't be created";
		public const string PaymentMethodAccountIsInUseAndCantBeDeleted = "This Cash Account is used  in one or more Customer Payment Methods and can not be deleted";
		public const string PaymentMethodIsInUseAndCantBeDeleted = "This Payment Method is used in one or more Customer Payment Methods and can not be deleted";
		public const string CashAccountMayNotBeMadeClearingAccount = "A cash account that has one or more clearing accounts cannot be defined as a clearing account.";
		public const string DontHaveAppoveRights = "You don't have access rights to approve document.";
		public const string DontHaveRejectRights = "You don't have access rights to reject document.";
		public const string CABatchExportProviderIsNotConfigured = "The batch cannot be exported. An export scenario is not specified for the payment method.";
		public const string ReleasedDocumentMayNotBeAddedToCABatch = "This document is released and can not be added to the batch";
		public const string ReleasedDocumentMayNotBeDeletedFromCABatch = "This document is released and can not be deleted from the batch";
		public const string CABatchDefaultExportFilenameTemplate = "{0}-{1}-{2:yyyyMMdd}{3:00000}.txt";  //Do not translate this message, only change it if required
		public const string CABatchStatusIsNotValidForProcessing = "Document status is not valid for processing";
		public const string CABatchContainsUnreleasedPaymentsAndCannotBeReleased = "This  batch contains unreleased payments. It can'not be released until all the payments are released successfully";
		public const string DateSeqNumberIsOutOfRange = "Date Sequence Number is out of range";
		public const string DocumentOnHoldCanNotBeReleased = "Statement on Hold can't be released";
		public const string DocumentIsUnbalancedItCanNotBeReleased = "Statement is not balanced";
		public const string StatementCanNotBeReleasedSomeDetailsMatchedDeletedDocument = "Statement can not be released - same of the details matched deleted document";
		public const string StatementCanNotBeReleasedThereAreUnmatchedDetails = "Statement can not be released - same of the details are not matched";
		public const string PaymentMethodIsRequiredToCreateDocument = "Filling the Payment Method box is mandatory for creating a payment.";
		public const string EntryTypeIsRequiredToCreateCADocument = "Filling the Entry Type box is mandatory for creating a payment.";
		public const string PayeeLocationIsRequiredToCreateDocument = "Filling the Location box is mandatory for creating a payment.";
		public const string PayeeIsRequiredToCreateDocument = "Filling the Business Account box is mandatory for creating a payment.";
		public const string DocumentIsAlreadyCreatedForThisDetail = "Document is already created";
		public const string StatementEndDateMustBeGreaterThenStartDate = "End Balance Date should be greater then Start Balance Date";
		public const string StatementEndBalanceDateIsRequired = "End Balance Date is required";
		public const string StatementStartBalanceDateIsRequired = "Start Balance Date is required";
		public const string StatementIsOutOfBalanceThereAreUnmatchedDetails = "Statement is out of balance - there are unmatched details";
		public const string StatementIsOutOfBalanceEndBalanceDoesNotMatchDetailsTotal = "Statement is not balanced - end balance does not match details total";
		public const string StatementDetailIsAlreadyMatched = "This detail is already matched with another CA transaction";
		public const string CashAccountWithExtRefNbrIsNotFoundInTheSystem = "Account with Ext Ref Number {0} is not found in the system";
		public const string CashAccountHasCurrencyDifferentFromOneInStatement = "Account {0} has currency {1} different from one specified in the statement. Statement can not be imported. Please, check correctness of the cash account's Ext Ref Nbr and other settings";
		public const string CashInTransitAccountCanNotBeDenominated = "Cash-In-Transit Account can not be Cash Account or denominated one.";
		public const string CashAccountCanNotBeTransit = "Cash-in-Transit account cannot be Cash Account.";
		public const string TransactionWithFitIdHasAlreadyBeenImported = "Transaction with FITID {0} is found in the existing Statement: {1} for the Account: {2}-'{3}'. Most likely, this file has already been imported";
		public const string FITIDisEmpty = "The file does not comply with the standard format: FITID is empty. You will be able to upload the file if you select the Allow Empty FITID check box on the CA Preferences form.";
		public const string OFXImportErrorAccountInfoIsIncorrect = "Account information in the file is invalid or has an unsupported format";
		public const string OFXParsingErrorTransactionValueHasInvalidFormat = "The Value {0} for the Field {1} in the transaction {2} has invalid format: {3}";
		public const string OFXParsingErrorValueHasInvalidFormat = "The Field {0} has invalid format: {1}";
		public const string OFXUnsupportedEncodingDetected = "Unsupported Encoding {0} or Charset (1) detected in the header";
		public const string UnsavedDataInThisScreenWillBeLostConfirmation = "Unsaved data in this screen will be lost. Continue?";
		public const string ImportConfirmationTitle = "Confirmation";
		public const string ViewResultingDocument = "View Resulting Document";
		public const string DuplicatedPaymentMethodForCashAccount = "Payment method '{0}' is already added to this Cash Account";
		public const string PaymentMethodCannotBeUsedInAR = "The {0} payment method cannot be used in AR. Use the Payment Methods (CA204000) form to modify the payment method settings.";
		public const string PaymentMethodCannotBeUsedInAP = "The {0} payment method cannot be used in AP. Use the Payment Methods (CA204000) form to modify the payment method settings.";
		public const string DuplicatedCashAccountForPaymentMethod = "Cash Account '{0}' is already added to this Payment method";
		public const string APPaymentApplicationInvoiceIsNotReleased = "Invoice with number {0} is not released. Application can be made only to the released invoices";
		public const string APPaymentApplicationInvoiceIsClosed = "Invoice with number {0} is closed.";
		public const string APPaymentApplicationInvoiceDateIsGreaterThenPaymentDate = "Invoice with the number {0} is found, but it's date is greater then date of the transaction. It can not  be used for the payment application";
		public const string APPaymentApplicationInvoiceUnrealeasedApplicationExist = "There are unreleased applications to the Invoice number {0}. It can not be used for this payment application.";
		public const string APPaymentApplicationInvoiceIsPartOfPrepaymentOrDebitAdjustment = "Invoice with the number {0} is found, but there it's used in prepayment or debit adjustment. It can not be used for this payment application.";
		public const string APPaymentApplicationInvoiceIsNotFound = "Invoice number {0} match neither internal nor external Invoice numbers registered in the system. You need to enter this invoice before the application.";
		public const string ARPaymentApplicationInvoiceIsNotReleased = "Invoice with number {0} is not released. Application can be made only to the released invoices";
		public const string ARPaymentApplicationInvoiceIsClosed = "Invoice with number {0} is closed.";
		public const string ARPaymentApplicationInvoiceDateIsGreaterThenPaymentDate = "Invoice with the number {0} is found, but it's date is greater then date of the transaction. It can not  be used for the payment application";
		public const string ARPaymentApplicationInvoiceUnrealeasedApplicationExist = "There are unreleased applications to the Invoice number {0}. It can not be used for this payment application.";
		public const string ARPaymentApplicationInvoiceIsNotFound = "Invoice number {0} match neither internal nor external Invoice numbers registered in the system. You need to enter this invoice before the application.";
		public const string CAFinPeriodRequiredForSheduledTransactionIsNotDefinedInTheSystem = "A scheduled document {0} {1} {2} assigned to the Schedule {3} needs a financial period, but it's not defined in the system";
		public const string FinPeriodsAreNotDefinedForDateRangeProvided = "Financial Periods are not defined for the date range provided. Scheduled documents may not be included correctly";
		public const string CurrencyRateIsNotDefined = "The currency rate is not defined. Specify it on the Currency Rates (CM301000) form.";
		public const string CurrencyRateIsRequiredToConvertFromCuryToBaseCuryForAccount = "A currency rate for conversion from Currency {0} to Base Currency {1} is not found for account {2}";
		public const string CurrencyRateTypeInNotDefinedInCashAccountAndDafaultIsNotConfiguredInCMSetup = "A currency rate type is not defined to the Cash ccount {0} and no default is provided for CA Module in CM Setup";
		public const string RowMatchesCATranWithDifferentExtRefNbr = "This row is matched with the document having not exactly matching Ext. Ref. Nbr.";
		public const string MatchingCATransIsNotValid = "Matching Transaction is not valid. Probably, it has been deleted";
		public const string RowsWithSuspiciousMatchingWereDetected = "There were detected {0} rows, having not exact matching";
		public const string CABatchContainsVoidedPaymentsAndConnotBeReleased = "This Batch Payments contains voided documents. You must remove them to be able to release the Batch";
		public const string CAEntryTypeUsedForPaymentReclassificationMustHaveCashAccount = "Entry Type which is used for Payment Reclassification must have a Cash Account as Offset Account";
		public const string EntryTypeCashAccountIsNotConfiguredToUseWithAnyPaymentMethod = "This Cash Account is not configured for usage with any Payment Method. Please, check the configuration of the Payment Methods before using the Payments Reclassifications";
		public const string OffsetAccountForThisEntryTypeMustBeInSameCurrency = "Offset account must be a Cash Account in the same currency as current Cash Account";
		public const string OffsetAccountMayNotBeTheSameAsCurrentAccount = "Offset account may not be the same as current Cash Account";
		public const string NoActivePaymentMethodIsConfigueredForCashAccountInModule = "There is no active Payment Method which may be used with account '{0}' to create documents for Module '{1}'. Please, check the configuration for the Cash Account '{0}'.";
		public const string EntryTypeRequiresCashAccountButNoOneIsConfigured = "This Entry Type requires to set a Cash Account with currency {0} as an Offset Account. Currently, there is no such a Cash Account defined in the system";
		public const string UploadFileHasUnrecognizedBankStatementFormat = "This file format is not supported for the bank statement import. You must create an import scenario for this file extention prior uploading it.";
		public const string SetOffsetAccountInSameCurrency = "Default offset account currency is different from the currency of the current Cash Account. You must override Reclassification account.";
		public const string SetOffsetAccountDifferFromCurrentAccount = "Default offset account can not be the same as current Cash Account. You must override Reclassification account.";
		public const string SpecifyLastRefNbr = "To use the {0} - Suggest Next Number option you must specify the {0} Last Reference Number.";

		public const string StatementServiceReaderCreationError = "A Statement Reader Service of a type {0} is failed to create";
		public const string CashAccountMustBeSelectedToImportStatement = "You need to select a Cash Account, for which a statement will be imported";
		public const string StatementImportServiceMustBeConfiguredForTheCashAccount = "You have to configure Statement Import Service for the selected Cash Account. Please, check account settings in the 'Cash Accounts' interface";
		public const string StatementImportServiceMustBeConfiguredInTheCASetup = "You have to configure Statement Import Service. Please, check 'Bank Statement Settings' section in the 'Cash Management Preferences'";
		public const string ImportedStatementIsMadeForAnotherAccount = "The Statement in the file selected is created for another account: {0}-'{1}'. Please, select a correct file";
		public const string CashAccountExist = "Cash account for this account, sub account and branch already exist";
		public const string SomeRemittanceSettingsForCashAccountAreNotValid = "Some Remittance Settings for this Payment Method have invalid values. Please Check.";
		public const string WrongSubIdForCashAccount = "Wrong sub account for this account";
		public const string DocumentMustByAppliedInFullBeforeItMayBeCreated = "To be able to create the payment, you need to apply documents whose total amount must be equal to the payment amount.";
		public const string TranCuryNotMatchAcctCury = "Transaction's Currency does not Match CashAccount's Currency";
		public const string CryptoSettingsChanged = "Encryption settings were changed during last system update. To finalize changes please press save button manually.";
		public const string ShouldContainBQLField = "Parameter should cointain a BqlField";
		public const string CouldNotInsertPMDetail = "Converter was not able to setup Payment Method Detail due to ID conflict. Please contact support for help.";
		public const string CouldNotInsertCATran = "Attempt to rewrite existing CATran was detected. Please contact support for help.";
		public const string NoProcCenterSetAsDefault = "No processing center was set as default";
		public const string CCPaymentProfileIDNotSetUp = "Payment Profile ID in 'Settings for Use in AR' has to be set up before tokenized processing centers can be used";
		public const string NoCashAccountForBranchAndSub = "There is no Cash Account matching these Branch and Subaccount";
		public const string CashAccountNotMatchBranch = "This Cash Account does not match selected Branch";
		public const string CATranNotFound = "The transaction cannot be processed due to absence of the matched document. Clear the transaction match by clicking the Unmatch button and repeat the matching process.";
		public const string MatchNotFound = "Match for transaction '{0}' was not found";
		public const string DeatsilProcess = "Bank transaction was processed";
		public const string ErrorsInProcessing = "Not all records have been processed, please review";
		public const string AmountDiscrepancy = "Payment details amount {0} differs from bank transaction amount {1}. To be able to create the payment, you need to add details whose total amount is equal to the bank transaction amount.";
		public const string MatchToInvoiceAmountDiscrepancy = "The total amount of the selected invoices {0} is not equal to the bank transaction amount {1}. Select invoices with the total amount equal to the bank transaction amount.";

		public const string NoDocumentSelected = "No document selected. Please select one or more documents to process.";

		public const string BankRuleTooLoose = "Tran. Code, Description or Amount Criteria must be specified for the rule.";
		public const string BankRuleEntryTypeDoesntSuitCashAccount = "Entry Type does not suit the selected Cash Account.";
		public const string BankRuleOnlyCADocumentsSupported = "Only Cash Management documents can be created according to rules.";
		public const string BankRuleFailedToApply = "Failed to apply a rule due to data validation.";
		public const string BankRuleInUseCantDelete = "Cannot delete the Rule. There are Transactions associated with this Rule.";
		public const string HideTranMsg = "This will undo all changes to this transaction, hide it from feed and mark it as processed. Proceed?";
		public const string UnmatchTranMsg = "The system will roll back all changes to the selected bank transaction and make it available for processing on the Process Bank Transactions (CA306000) form. The link to the matched document will be deleted, but the document will stay marked as cleared if it is included into a reconciliation statement. If the document was created to match the transaction, it should be handled manually, for instance, voided or matched. Proceed?";
		public const string UnmatchAllTranMsg = "The system will roll back all changes to the matched bank transactions and make them available for processing on the Process Bank Transactions (CA306000) form. The links to the matched documents will be deleted, but the documents will stay marked as cleared if they are included into a reconciliation statement. If the documents were created to match the transactions, they should be handled manually, for instance, voided or matched. Proceed?";
		public const string AnotherOptionChosen = "Another option is already chosen";
		public const string TransactionMatchedToExistingDocument = "Transaction is matched to an existing document.";
	    public const string TransactionMatchedToExistingExpenseReceipt = "The transaction is already matched to an expense receipt.";
        public const string TransactionWillPayInvoice = "A new payment will be created for this transaction based on the invoice details.";
		public const string TransactionWillCreateNewDocument = "New payment will be created for this transaction.";
		public const string TransactionWillCreateNewDocumentBasedOnRuleDefined = "New payment will be created for this transaction basing on the defined rule.";
		public const string TRansactionNotMatched = "Transaction is not matched.";
		public const string WrongInvoiceType = "Wrong Invoice Type!";
		public const string UnknownModule = "Unknown module!";
		public const string CouldNotAddApplication = "Could not add application of '{0}' invoice. Possibly it is already used in another application";
		public const string ApplicationremovedByUser = "Application of '{0}' invoice was removed by user.";
		public const string ErrorInMatchTable = "Error in CABankTranMatch table for CABankTran '{0}'";
		public const string InvoiceNotFound = "Invoice No. '{0}' has not been found.";
		public const string CannotDeleteTranHeader = "This statement cannot be deleted because it contains transactions that has already been matched or processed.";
		public const string CannotDeleteTran = "This transaction cannot be deleted because it has already been matched or processed.";
		public const string CannotDeleteRecon = "The operation cannot be performed because the reconciliation statement has been already released or voided.";
		public const string PaymentAlreadyAppliedToThisDocument = "The payment is already applied to this document.";
		public const string CanApplyPrepaymentToDrAdjOrPrepayment = "Can't apply Prepayment to Debit Adjustment or Prepayment. Please remove the Debit Adjustments and Prepayments from the list of applications or apply the entire payment amount so that a Check is created.";
		public const string UnableToProcessWithoutDetails = "This document has no details and can not be processed.";
		public const string EndBalanceDoesNotMatch = "Ending balance does not match the calculated balance.";
		public const string BalanceDateDoesNotMatch = "Start date does not match the end date of the previous statement ({0:d}).";
		public const string EarlyInDate = "The date in Receipt Date is earlier than in Transfer Date.";
		public const string EarlyExpenseDate = "The date of the expense is earlier than the transfer date.";
		public const string ReversingTransferOfTransferNbr = "The reversing transfer of the {0} transfer";
		public const string BegBalanceDoesNotMatch = "Beginning balance does not match the ending balance of previous statement ({0:F2}).";
		public const string NotExactAmount = "Please note that the application amount is different from the invoice total.";
        public const string CannotProceedFundTransfer = "A fund transfer with {0} amount cannot be processed. To proceed, enter an amount greater than zero.";
        public const string VoidedDepositCannotBeReleasedDueToInvalidStatusOfIncludedPayment = "The voided deposit cannot be released due to an invalid status of at least one included payment.";


        public const string ErrorsProcessingEmptyLines = "Cannot release this document because it has no lines.";

		public const string CantTransferNegativeAmount = "Cannot transfer negative amount.";
		public const string CannotEditTaxAmtWOCASetup = "Tax Amount cannot be edited if \"Validate Tax Totals on Entry\" is not selected on the CA Preferences form";
		public const string ConfirmVoid = "Are you sure you want to void the statement?";
		public const string ExpDateRetrievalFailed = "Failed to retrieve the expiration date from the processing center.";
		public const string CannotDeleteClearingAccount = "The cash account cannot be deleted. This cash account is assigned to another cash account as clearing. Remove links to other cash account and then proceed with the cash account deletion.";
		public const string CannotDeletePaymentMethodAccount = "The combination of Payment Method, Cash Account '{0}, {1}' cannot be deleted because it is already used in payments.";
		public const string CATranHasExcessGLTran = "Cannot release documents. Please contact support.";
		public const string MultipleCABankTransMatchedToSingleCATran = "Multiple bank transactions have been matched to the same cash transaction in the system. Please contact the support service.";
		public const string DocumentIsAlreadyMatched = "This document has been already matched. Refresh the page to update the data.";
		public const string ValueValidationNotValid = "The ValidationField value is not valid.";
		public const string ValueMaskNotValid = "The MaskField value is not valid.";
		public const string OutOfProcessed = "The system processed {0} out of {1}. {2} are searchable. Error: {3}.";
		public const string ProcessCannotBeCompleted = "The process cannot be completed. Please contact support service.";
		public const string CannotReleasePendingApprovalDocument = "A document with the Pending Approval status cannot be released. The document has to be approved by a responsible person before it can be released.";
		public const string CustomerNotDefined = "Customer not defined!";
		public const string PaymentMethodAccountCannotBeFound = "The payment method for the cash account cannot be found in the system.";
		public const string PaymentMethodCannotBeFound = "The {0} payment method cannot be found in the system.";
		public const string PaymentMethodNotDefined = "Payment Method not defined!";
		public const string CorpCardCashAccountToBeLinkedToOnePaymentMethod = "The cash account configured for corporate cards should have a single associated payment method.";
		public const string ClearingAccountNotAllowed = "The cash account configured for corporate cards cannot be set up as a clearing account.";
		public const string PaymentAndAdditionalProcessingSettingsHaveWrongValues = "For a cash account configured for corporate cards, you can select only a payment method with the following settings specified on the Settings for Use in AP tab of the Payment Methods (CA204000) form: the Require Unique Payment Ref. check box is cleared and the Not Required option is selected in the Additional Processing section.";
		public const string PaymentAndAdditionalProcessingSettingsHaveWrongValuesPaymentSide = "The payment method has an associated cash account configured for corporate cards and should have the following settings specified on the Settings for Use in AP tab: the Require Unique Payment Ref. check box is cleared and the Not Required option is selected in the Additional Processing section.";
		public const string CashAccountLinkOrMethodCannotBeDeleted = "You cannot delete the associated cash account because it is configured for corporate cards. If you need to change the payment method for this cash account, please use the Cash Accounts (CA202000) form.";
		public const string Completed = "Completed";
		public const string CorpCardIsInactive = "The corporate card is inactive.";
		public const string RecordWithPaymentCCPIDExists = "The customer payment method cannot be created because a record with the specified payment profile ID already exists."; 
		public const string RelodCardDataDialogMsg = "Data will be reloaded. Continue loading?";
		public const string LoadCardCompleted = "Loading of the credit card has been completed.";
		public const string FinancialPeriodClosedInCA = "The {0} financial period of the {1} company is closed in Cash Management.";
		public const string FinPeriodCanNotBeSpecifiedCashAccountIsEmpty = "The financial period cannot be specified because the cash account has not been selected in the Cash Account box.";
		public const string OnlyNonControlAccountCanBeCashAccount = "Only a non-control account can be selected as a cash account.";
		public const string CashAccountCanNotBeUsedAsControl = "Cash accounts cannot be used as control accounts.";
		public const string ChargeAlreadyExists = "The charge with the same Entry Type and Payment Method already exists.";
		#endregion

		#region Translatable Strings used in the code
		public const string CardNumber = "Card Number";
		public const string ExpirationDate = "Expiration Date";
		public const string NameOnCard = "Name on the Card";
		public const string CCVCode = "Card Verification Code";
		public const string CCPID = "Payment Profile ID";
		public const string ReportID = "Report ID";
		public const string ReportName = "Report Name";
		public const string Day = "Day";
		public const string Week = "Week";
		public const string Month = "Month";
		public const string Period = "Financial Period";
		public const string ViewExpense = "View Expense";
		public const string Release = PM.Messages.Release;
		public const string ReleaseAll = PM.Messages.ReleaseAll;
		public const string Void = "Void";
		public const string AddARPayment = "Add Payment";
		public const string ViewBatch = "View Batch";
		public const string ViewDetails = "View Details";
		public const string Reports = "Reports";
		public const string CashTransactions = "Cash Transactions";
		public const string Approval = "Approval";
		public const string Approved = "Approved";
		public const string Export = "Export";
		public const string MatchSelected = "Match Selected";
		public const string ImportStatement = "Import";
		public const string ViewDocument = "View Document";
		public const string ViewMatchedDocument = "View Matched Document";
		public const string ClearMatch = "Unmatch";
		public const string ClearMatchAll = "Unmatch All";
		public const string CreateAllDocuments = "Create All";
		public const string CreateDocument = "Create Document";
		public const string UploadFile = "Upload File";
		public const string MatchSettings = "Match Settings";
		public const string CashForecastTran = "Cash Forecast Transactions";
		public const string CashForecastReport = "Cash Flow Forecast Report";
		public const string ViewAsReport = "View As a Report";
		public const string ViewAsTabReport = "View As a Tab Report";
		public const string DateFormat = "yyyy-MM-dd";
		public const string AccountDescription = "Account Description";
		public const string ClearAllIndexes = "Clear all indexes";
		public const string ClearAllIndexesTip = "Clears all indexes in the system.";
		public const string IndexCustomArticles = "Index Custom Articles";
		public const string HideTran = "Hide";
		public const string UnhideTran = "Unhide Transaction";
		public const string ProcessMatched = "Process Matched Lines";
		public const string ClearAllMatches = "Unmatch All";
		public const string AutoMatch = "Auto-Match";
		public const string CreateRule = "Create Rule";
		public const string ClearRule = "Clear Rule";
		public const string ViewPayment = "View Payment";
		public const string ViewInvoice = "View Invoice";
		public const string MatchModeNone = "None";
		public const string MatchModeEqual = "Equal";
		public const string MatchModeBetween = "Between";
		public const string SearchTitleBatchPayment = "Batch Payment: {0} - {2}";
		public const string SearchTitleCATransfer = "CA Transfer: {0}";
		public const string Matched = "Matched to Payment";
		public const string InvoiceMatched = "Matched to Invoice";
		public const string MatchedToEReceipt = "Matched to Expense Receipt";
		public const string Created = "Created";
		public const string Hidden = "Hidden";
		public const string Offset = "Offset";
		public const string FailedGetFrom = "The system failed to get the From address for the document.";
		public const string FailedGetTo = "The system failed to get the To address for the document.";
		public const string NotVoidedUnreleased = "An unreleased document can't be voided.";
		public const string ExternalTaxVendorNotFound = TX.Messages.ExternalTaxVendorNotFound;
		public const string MultiCurDepositNotSupported = "A deposit of multiple currencies is not supported yet.";
		public const string ProcessingCenterNotSelected = "Processing Center is not selected.";
		public const string ProcessingPluginNotSelected = "Processing plug-in is not selected.";
		public const string ProcessingCenterAimPluginNotAllowed = "The Authorize.Net AIM plug-in cannot be used for new processing center configurations. Use another plug-in.";
		public const string DiscontinuedProcCenter = "The processing center uses the discontinued plug-in. Set up the processing center to use the Authorize.Net API plug-in.";
		public const string NotSupportedProcCenter = "The processing center uses an unsupported plug-in.";
		public const string CredentialsAccepted = "The credentials were accepted by the processing center.";
		public const string Result = "Result";
		public const string ResetDetailsToDefault = "Changing the Plug-In Type will reset the details to default values. Continue?";
		public const string NotSetProcessingCenter = "The new processing center is not set in the Proc. Center ID box.";
		public const string AskConfirmation = AP.Messages.AskConfirmation;
		public const string PaymentMethodDetailsWillReset = "The details for the payment method will be reset. Continue?";
		public const string EmptyValuesFromExternalTaxProvider = AP.Messages.EmptyValuesFromExternalTaxProvider; 
		public const string Validate = "Validate";
		public const string ValidateAll = "Validate All";
		public const string ValueMInstanceIdError = @"The value of the parameter ""pMInstanceID"" cannot be represented as an integer.";
		public const string CanNotRedirectToDocumentThisType = "The system cannot redirect the user to a document of this type.";
		public const string UnknownDetailType = "The {0} detail type is unknown.";
		public const string CABankTransactionsRuleName = "CA Bank Transactions Rule";
		public const string PaymentProfileID = "Payment Profile ID";
		public const string NoMatchingDocFound = "No matching documents were found.";
		public const string NoRelevantDocFound = "Found documents are not relevant and cannot be auto-matched.";
		public const string RatioInRelevanceCalculation = "The weights of the Ref. Nbr., Doc. Date, and Amount in the relevance calculation are {0:F2}%, {1:F2}%, and {2:F2}%, respectively.";
		#endregion

		#region Graphs Names
		public const string CABalValidate = "Cash Account Validation Process";
		public const string CAReconEntry = "Reconciliation Statement Entry";
		public const string CAReconEnq = "Reconciliation Statement Summary Entry";
		public const string CASetup = "Cash Management Preferences";
		public const string CASetupMaint = "Setup Cash Management";
		public const string CashAccountMaint = "Cash Account Maintenance";
		public const string CashTransferEntry = "Cash Transfer Entry";
		public const string CATranEnq = "Cash Transactions Summary Entry";
		public const string CATranEntry = "Cash Transaction Details Entry";
		public const string CATrxRelease = "Cash Transactions Release Process";
		public const string EntryTypeMaint = "Entry Types Maintenance";
		public const string PaymentTypeMaint = "Payment Types Maintenance";
		public const string PaymentMethodMaint = "Payment Methods Maintenance";
		public const string CCProcessingCenter = "Processing Center";
		public const string CAReleaseProcess = "Cash Transactions Release";
		public const string CADepositEntry = "Payment Deposits";
		public const string CABatchEntry = "Payment Batch Entry";
		public const string CashForecastEntry = "Cash Flow Forecast Enquiry";

		#endregion

		#region DAC Names
		public const string CATran = "CA Transaction";
		public const string CATran2 = "CA Transaction (alias)";
		public const string CABatch = "CA Batch";
		public const string CABatchDetail = "CA Batch Details";
		public const string PaymentMethodDetail = "Payment Method Detail";
		public const string CARecon = "Reconciliation Statement";
		public const string PaymentMethod = "Payment Method";
		public const string CashAccount = "Cash Account";
		public const string PaymentType = "Payment Type";
		public const string CashFlowForecast = "Cash Flow Forecast Record";
		public const string CashFlowForecast2 = "Cash Flow Forecast Record";
		public const string BankTranHeader = "Bank Statement";
		public const string BankTransaction = "Bank Transaction";
		public const string BankTranMatch = "Bank Transaction Match";
		public const string BankTranAdjustment = "Bank Transaction Adjustment";
		public const string ClearingAccount = "Clearing Account";
		public const string CADailySummary = "CA Daily Summary";
		public const string CAReconByPeriod = "Reconciliation by Period";
		public const string CASplit = "CA Transaction Details";
		public const string CADepositDetail = "CA Deposit Detail";
		public const string CABankTranDetail = "CA Bank Transaction Detail";
		public const string CADepositCharge = "CA Deposit Charge";
		public const string CAEntryType = "CA Entry Type";
		public const string CASetupApproval = "CA Approval Preferences";
		public const string CashAccountCheck = "Cash Account Check";
		public const string CashAccountETDetail = "Entry Type for Cash Account";
		public const string CashAccountPaymentMethodDetail = "Details of Payment Method for Cash Account";
		public const string CATax = "CA Tax Detail";
		public const string CATaxTran = "CA Tax Transaction";
		public const string CCProcessingCenterDetail = "Credit Card Processing Center Detail";
		public const string CCProcessingCenterPmntMethod = "Payment Method for Credit Card Processing Center";
		public const string PaymentMethodAccount = "Payment Method for Cash Account";
		public const string PMInstance = "Payment Method Instance";
		public const string CorporateCard = "Corporate Card";
		#endregion

		#region Tran Type
		public const string CATransferOut = "Transfer Out";
		public const string CATransferIn = "Transfer In";
		public const string CATransferExp = "Expense Entry";
		public const string CATransfer = "Transfer";
		public const string CAAdjustment = "Cash Entry";
		public const string GLEntry = "GL Entry";
		public const string CADeposit = "CA Deposit";
		public const string CAVoidDeposit = "CA Void Deposit";
		public const string Statement = "Bank Statement Import";
		public const string PaymentImport = "Payments Import";

		public const string ARAPInvoice = "AR Invoice or AP Bill";
		public const string ARAPPrepayment = "Prepayment";
		public const string ARAPRefund = "Refund";
        public const string ARAPVoidRefund = "Voided Refund";
        #endregion

        #region CA Transfer & CA Deposit Status
        public const string Balanced = "Balanced";
		public const string Hold = "On Hold";
		public const string Released = "Released";
		public const string Pending = "Pending Approval";
		public const string Rejected = "Rejected";
		public const string Voided = "Voided";
		public const string Exported = "Exported";

		#endregion

		#region Dr Cr Type
		public const string CADebit = "Receipt";
		public const string CACredit = "Disbursement";
		#endregion

		#region CA Modules
		public const string ModuleAP = "AP";
		public const string ModuleAR = "AR";
		public const string ModuleCA = "CA";
		#endregion

		#region CA Reconcilation
		public const string ReconDateNotAvailable = "Reconciliation date must be later than the date of the previous released Reconciliation Statement";
		public const string PrevStatementNotReconciled = "Previous Statement Not Reconciled";
		public const string LastDateToLoadNotAvailable = "The date is earlier than the reconciliation date. Note, that the list of documents will be filtered by the specified date.";
		public const string HoldDocCanNotAddToReconciliation = "Document on hold cannot be added to reconciliation.";
		public const string NotReleasedDocCanNotAddToReconciliation = "Unreleased document cannot be added to Reconciliation.";
		public const string HoldDocCanNotBeRelease = "Document on hold cannot be released";
		public const string ClearedDateNotAvailable = "Clear Date NOT Available;";
		public const string OrigDocCanNotBeFound = "Orig. Document Can Not Be Found";
		public const string ThisCATranOrigDocTypeNotDefined = "This CATran Orig. Document Type Not Defined";
		public const string VoidPendingStatus = "Void Pending";
		public const string VoidedTransactionHavingNotReleasedVoidCannotBeAddedToReconciliation = "This transaction has a voiding transaction which is not released. It may not be added to the reconciliation";
		public const string TransactionsWithVoidPendingStatusMayNotBeAddedToReconciliation = "Transactions having a 'Void Pending' status may not be added to the reconciliation";
		public const string AbsenceCashTransitAccount = "The documents cannot be released due to absence of a cash-in-transit account. Specify the account in the Cash-In-Transit box on the Cash Management Preferences (CA101000) form.";
		public const string ReconDateNotSet = "The number of the next reconciliation statement cannot be generated, because the reconciliation date is not set.";
		#endregion

		#region OFXFileReader
		public const string ContentIsNotAValidOFXFile = "Provided content is not recognized as a valid OFX format";
		public const string UnknownFormatOfTheOFXHeader = "Unrecognized format of the message header";
		public const string OFXDocumentHasUnclosedTag = "Document has invalid format - tag at position {0} is missing closing bracket (>)";


		#endregion
		#region
		public const string PeriodHasUnreleasedDocs = "Period has Unreleased Documents";
		public const string PeriodHasHoldDocs = "Period has Hold Documents";
		#endregion
		
		public const string RefNbr = "Reference Nbr.";
		public const string Type = "Type";
	}
}
