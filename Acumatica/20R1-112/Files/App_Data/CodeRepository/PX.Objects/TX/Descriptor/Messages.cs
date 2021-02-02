using System;

using PX.Common;
using PX.Data;

namespace PX.Objects.TX
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		/* Add your messages here as follows (see line below):
		 * public const string YourMessage = "Your message here."; */

		#region Validation and Processing Messages
		public const string DateBelongsToNonExistingPeriod = "The date belongs to the nonexistent period.";
		public const string DateBelongsToClosedPeriod = "The date belongs to the closed period.";
		public const string DeductiblePPDTaxProhibited = "The combination of a partially deductible VAT and a cash discount that reduces taxable amount on early payment is not supported.";
		public const string TaxAgencyStatusIs = "The tax agency status is '{0}'.";
		public const string TaxPeriodStatusIs = "The tax period status is '{0}'.";
		public const string CannotProcessW = "The record cannot be processed because the Tax Doc. Nbr box is empty. To proceed, fill in the box.";
        public const string Prefix = "TX Error";
		public const string Document_Status_Invalid = "Document Status is invalid for processing.";
		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R2")]
		public const string Only_Prepared_CanBe_Adjusted = "You can only adjust Tax Reports with Prepared status.";
		public const string OnlyPreparedOrOpenCanBeAdjusted = "You can adjust a tax report for a tax period with the Open or Prepared status only.";
		public const string DocumentOutOfBalance = "Document is out of balance.";

		public const string TaxAlreadyInList = "This tax is already included into the list.";
		public const string NetTaxMustBeTax = "Net Tax line must have 'Tax' type.";

		public const string UseTaxExcludedFromTotals = "Use Tax is excluded from Tax Total.";
		public const string NoLinesMatchTax = "No lines match the tax.";

		public const string TaxableCategoryIDIsNotSet = "Taxable Category ID is not entered in the Import Settings. Please correct this and try again.";
		public const string TaxAgencyWithoutTran = "Tax Agency has no tax transactions.";

		public const string CannotPrepareReportPreviousOpen = "Cannot prepare tax report for open period when previous period isn't closed.";
		public const string CannotPrepareReportExistPrepared = "Cannot prepare tax report for period when previous prepared period isn't closed.";
		public const string CannotCloseReportForNotPreparedPeriod = "Cannot close tax report for Closed or Open period.";
		
		public const string SortOrderNumbersMustBeUnique = "The report line number is in use.";
        public const string TaxBoxNumbersMustBeUnique = "Tax box numbers must be unique.";
		public const string CannotAdjustTaxForClosedOrPreparedPeriod = "Cannot adjust tax for Closed or Prepared period '{0}'.";
		public const string InvalidTaxConfiguration = "Tax configuration is invalid. A document cannot contain both Reduce Taxable Amount and Reduce Taxable Amount On Early Payment taxes.";
		public const string CannotPostRetainedTaxExpenseToItemAccounts = "Partially deductible VATs with the cleared Use Tax Expense Account check box on the Taxes (TX205000) form are not supported in Accounts Payable bills with retainage.";
		public const string OriginalDocumentAlreadyContainsTaxRecord = "Original document already contains tax record.";

		public const string PerUnitTaxesNotSupportedOperation = "This operation is not supported for per-unit taxes.";
		public const string CannotPostPerUnitTaxesToItemAccountsForRetainedDocuments = "Per-unit taxes are not supported for documents with retainage.";

		public const string NoAdjustmentToReportedTaxPeriodWillBeMade = "There is no transactions to adjust in the selected reporting period.";

		public const string DeductibleVATWithOutputReportingGroupError = "A partially deductible VAT cannot be included in the output reporting group.";
		public const string WithholdingTaxWithInputReportingGroupError = "A withholding tax cannot be included in the input reporting group.";


	    public const string TheseTwoOptionsCantBeCombined = "These two options can't be combined.";
	    public const string ThisOptionCanOnlyBeUsedWithTaxTypeVAT = "This option can only be used with tax type VAT.";

        public const string TheseTwoOptionsShouldBeCombined = "These two options should be combined.";
		public const string CannotPrepareReportDefineRoundingAccounts = "The tax report cannot be prepared because the accounts to be used for recording rounding amounts are not specified on the General Ledger Preferences (GL102000) form. To proceed, specify the Rounding Gain/Loss Account (and subaccounts, if applicable).";
		public const string TaxReportRateNotFound = "Tax report preparing failed. There is no currency rate to convert tax from currency '{0}' to report currency '{1}' for date '{2}'";
		public const string FailedToGetTaxes = "Failed to get taxes from the external tax provider. Check Trace Log for details.";
		public const string FailedToApplyTaxes = "The tax amount calculated by the external tax provider cannot be applied to the document.";
		public const string FailedToCancelTaxes = "Failed to cancel the tax application in the external tax provider during the rollback. Check details in the Trace Log.";
	    
		public const string OneOrMoreTaxTransactionsFromPreviousPeriodsWillBeReported = "One or more tax transactions from the previous periods will be reported into the current period.";
        public const string EffectiveTaxNotFound = "Can't find effective rate for '{0}' (type '{1}')";
        public const string InactiveTaxCategory = "Tax Category '{0}' is inactive";
        public const string BucketContainsOnlyAggregateLines = "Tax reporting group {0} contains only aggregate amount lines! Please review your tax reporting setup.";


		public const string ClaimableAndPayableAccountsAreTheSame = "Tax Claimable and Tax Payable accounts and subaccounts for Tax {0} are the same. It's impossible to enter this Tax via GL in this configuration.";
		public const string TaxRateNotSpecified = "The {0} tax rate is not specified in the settings of the selected tax.";
		public const string NoTaxRevForTaxType = "Tax {0} has no configuration for {1} tax type, but there are tax transactions of this type.";
		public const string CheckTaxConfig = "Please check tax configuration.";
		public const string TaxReportCannotBeReleased = "The tax report cannot be released and the tax period cannot be closed because unreleased tax adjustments exist in the selected tax period.";
		public const string TheTaxReportSettingsCannotBeModified = "The tax report settings cannot be modified because there is a prepared tax report in the system for the selected tax agency. You need to release or void the prepared tax report before modifying the settings.";
		public const string SelectedDateBelongsToTheTaxPeriodThatIsGreaterThanTheSpecifiedOne = "Selected date belongs to the tax period that is greater than the specified one.";
		public const string CannotClosePeriod = "Cannot close tax period.";
		public const string ReportingPeriodDoesNotExistForTheTaxAgency = "Reporting period '{0}' does not exist for the tax agency '{1}'.";
		public const string TheReportingGroupAlreadyContainsTheReportLine = "The reporting group '{0}' already contains the report line '{1}'.";
		public const string ThereAreUnreportedTrans = "There are unreported transactions for tax(es) {0} in branch {1}. Please void this tax period and prepare it again.";
		public const string WrongTaxConfig = "There are transactions for tax(es) {0} that could not be included into report.";
		public const string RecalculateExtCost = "Do you want the system to recalculate the amount(s) in the '{0}' column?";

		public const string TheTaxZonesReloadSummaryBeginning = "The following changes have been made when tax zones were reloaded:";
		public const string TheTaxZonesReloadSummaryDeletedLinesFormat = "The tax zone details for the following lines have been deleted: {0}.";
		public const string TheTaxZonesReloadSummaryCreatedLinesFormat = "The tax zone details for the following lines have been added: {0}.";
		public const string TheTaxZonesReloadSummaryManyLinesSuffixFormat = "and {0} more lines";
		public const string TheTaxZonesReloadSummaryNoChangesOnReload = "No changes have been made to the reporting lines; the reporting lines reflect current configuration.";

		[Obsolete("This message is not used anymore and will be removed in Acumatica 2018R1")]
		public const string NoNewZonesLoaded = "No new Tax Zones found. All required Report Lines are already created.";
		public const string NoLinesByTaxZone = "This Tax Report doesn't have any Report Line that requires splitting by Tax Zones.";

		public const string NoReverseInManualVAT = "Reverse VAT cannot be used for manual tax entry.";
		public const string NoDeductibleInManualVAT = "Deductible VAT cannot be used for manual tax entry.";		
		public const string TaxPayableAccountNotSpecified = "The Tax Payable account should be specified for tax agency '{0}'.";
		public const string TaxPayableSubNotSpecified = "The Tax Payable account should be specified for tax agency '{0}'.";
		public const string ExternalTaxVendorNotFound = "Tax Vendor is required but not found for the External TaxZone.";
		public const string TaxReportHasUnprocessedSVAT = "In the tax period, there are payment applications for which pending VAT amount has not been recognized yet. To recognize pending VAT, use the Recognize Output VAT (TX503000) form and the Recognize Input VAT (TX503500) form.";
		public const string MultInstallmentTermsWithSVAT = "The document cannot be processed because VAT of the Pending type (recognized by payments) cannot be applied to documents with the multiple installment credit terms specified.";
		public const string CannotReleaseTaxReportNoFinancialPeriodForPeriodEndDateAndCompany = "The tax report cannot be released, and the tax period cannot be closed because the financial period related to the {0} end date and {2} tax period is not defined for the {1} company. To proceed, create and activate the necessary financial periods on the Company Financial Calendar (GL201010) form.";
		public const string FinancialPeriodInactiveOrLockedDocumentsWillBePostedToFirstOpenPeriod = "The financial period corresponding to the {0} end date of the {2} tax period is inactive or locked in the {1} company.";
		public const string FinancialPeriodClosedInAPDocumentsWillBePostedToFirstOpenPeriod = "The financial period corresponding to the {0} end date of the {2} tax period is closed in the Accounts Payable module in the {1} company. The generated documents will be posted to the first available open period.";
		public const string TaxReportCannotBeReleasedMigrationModeNetTax = "The tax report cannot be released and the tax period cannot be closed because Net Tax lines are configured for the tax agency and migration mode is activated in the Accounts Payable module.";
		public const string TheBranchCanBeSpecifiedOnlyForCompaniesForWhichFileTaxesByBranchesOptionIsEnabled = "A branch can be specified only for companies for which the File Taxes by Branches check box is selected on the Companies (CS101500) form.";
		public const string RatesForCurrencyWereNotProvided = "Rates for the {0} currency have not been specified on the Currency Rates (CM301000) form. To proceed, specify the rates.";
		public const string RateForCurrenciesAndDateNotFound = "No exchange rate from {0} to {1} has been found for the date {2}. Specify the rate on the Currency Rates (CM301000) form.";
		public const string TaxYearStartDateDoesNotMatch = "The Financial Period type of tax period cannot be selected because the tax year start date does not match the financial year start date.";

		[Obsolete(Common.Messages.FieldIsObsoleteNotUsedAnymore)]
		public const string GrossModeIsNotAvailableForDocument = "The Gross tax calculation mode cannot be used in a document where a tax with the {0} type is specified.";
		public const string NetModeIsNotAvailableForWithholdingType = "The Net tax calculation mode cannot be used in a document where a tax with the {0} type is specified.";
		public const string GrossModeIsNotAvailableForUseType = "The Gross tax calculation mode cannot be used in a document where a tax with the {0} type is specified.";

		public const string ValueCannotBeFoundInSystem = "The value cannot be found in the system.";
		public const string TaxRoundingGainLossAccountsRequired = "Tax rounding gain and loss accounts cannot be empty. Specify these accounts on the Tax Preferences (TX103000) form.";
		#endregion

		#region Translatable Strings used in the code
		public const string CurrentBatchRecord = "Current batch record";
		public const string TranDesc1 = "{0}";
		public const string TranDesc2 = "{0},{1}";
		public const string DefaultInputGroup = "Default Input Group";
		public const string DefaultOutputGroup = "Default Output Group";
		public const string TaxAmount = "Tax Amount";
		public const string TaxableAmount = "Taxable Amount";
        public const string TaxOutDateTooEarly = "Entered date is before some of tax revisions' starting dates";
		public const string GLEntry = "GL Entry";
		public const string ReversingGLEntry = "Reversing GL Entry";
		public const string ExternalTaxProviderNotConfigured = "External tax provider is not configured.";
		public const string DocumentTaxCalculationModeNotEnabled = "Document Tax Calculation mode is not enabled!";
		public const string MethodMustBeOverridden = "Method must be overridden in module-specific tax attributes!";
		public const string AreSureToShortenTaxYear = "Are you sure you want to shorten the tax year?";
		public const string SubsequentTaxYearsWillBeDeleted = "Because the end date of the tax year has been changed, the subsequent tax years will be deleted. Please review and modify the next tax year settings if required.";
		public const string ExternalTaxProviderConnectSuccessAskHeader = "Setup";
		public const string ExternalTaxProviderConnectSuccessAskMsg = "The connection to external tax provider was successful. The version of the service is {0}.";
		public const string UnexpectedCall = "Unexpected call";
		public const string InvalidModule = "Invalid module {0}: Only AP, AR, or CA is expected.";
		public const string ProcessCaptionPost = "Post";
		public const string ProcessCaptionPostAll = "Post All";

		public const string DocumentInclusiveTaxDiscrepancy = "Tax rounding difference";
		public const string NotSupportedPerUnitTaxCalculationLevelErrorMsg = "The calculation level {0} for the per-unit tax is not supported.";
		public const string MissingInventoryAndLineUomForPerUnitTaxErrorMsgFormat =
			"The {0} per-unit tax cannot be calculated for the document with the {1} tax zone if inventory item and UOM are not specified for the line with the {2} tax category.";

		public const string MissingUomConversionForPerUnitTaxErrorMsgFormat =
			"The {0} per-unit tax cannot be calculated for the document with the {1} tax zone and the line with the {2} tax category." +
			" Conversion rule to the {3} tax UOM is missing.";

		public const string CombinationOfExclusivePerUnitTaxAndInclusveTaxIsForbiddenErrorMsg =
			"This combination of inclusive taxes and exclusive per-unit taxes is forbidden. Please review and modify your tax settings on the Taxes (TX205000) form.";

		public const string PerUnitTaxCannotBeCalculatedInNonBaseCurrencyErrorMsg = "Calculation of per-unit taxes is not supported for documents in a non-base currency.";
		public const string PerUnitTaxCannotBeCalculatedForRetainedDocumentsErrorMsg = "Calculation of per-unit taxes is not supported for retained documents.";
		public const string PerUnitTaxCannotBeInsertedManuallyErrorMsg = "Per-unit taxes cannot be inserted manually.";
		public const string PerUnitTaxCannotBeDeletedManuallyErrorMsg = "Per-unit taxes cannot be deleted manually.";
		#endregion

		#region Graph Names
		public const string ReportTax = "Tax Preparation Process";
		public const string ReportTaxReview = "Tax Filing Process";
		public const string ReportTaxProcess = "Tax Report Creator";
		public const string ReportTaxDetail = "Tax Report Details Inquiry";
		public const string SalesTaxMaint = "Taxes Maintenance";
		public const string TaxAdjustmentEntry = "Tax Adjustment Entry";
		public const string TaxBucketMaint = "Tax Reporting Groups Maintenance";
		public const string TaxCategoryMaint = "Tax Categories Maintenance";
		public const string TaxReportMaint = "Tax Report Setting Maintenance";
		public const string TaxZoneMaint = "Tax Zones Maintenance";
		public const string TaxExplorer = "Tax Explorer";
		public const string TaxImport = "Tax Import";
		public const string TaxImportSettings = "Tax Import Settings";
		public const string TaxImportDataMaint = "Tax Import Data Maintenance";
		public const string TaxImportZipDataMaint = "Tax Import Zip Data Maintenance";
		public const string ExternalTaxProviderMaint = "External Tax Provider Configuration";
		#endregion

		#region Cache Names
		public const string TXSetupMaint = "Tax Preferences";

		public const string TaxZone = "Tax Zone";
		public const string TaxAdjustment = "Tax Adjustment";
		public const string TaxCategory = "Tax Category";
		public const string Tax = "Tax";
		public const string VendorMaster = "Tax Agency";

		public const string TaxPeriod = "Tax Period";
		public const string TaxTransaction = "Tax Transaction";
		public const string TaxReportLine = "Tax Report Line";
		public const string TaxReportSummary = "Tax Report Summary";
		public const string TaxDetailReport = "Tax Report Detail";
		public const string TaxBucket = "Tax Group";
		public const string TaxBucketLine = "Tax Group Line";
		public const string TaxCategoryDet = "Tax Category Detail";
		public const string TaxDetailReportCurrency = "Tax Detail Report Currency";
		public const string TaxHistory = "Tax History";
		public const string TaxHistorySum = "Tax History Sum";
		public const string TaxRev = "Tax Revision";
		public const string TaxTranReport = "Tax Transaction for Report";
		public const string TaxYear = "Tax Year";
		public const string TaxZoneDet = "Tax Zone Detail";
		public const string TaxZoneZip = "Tax Zone Zip Code";
		public const string SVATConversionHist = "SVAT Conversion History";
		public const string TaxPlugin = "Tax Plug-in";
		public const string TaxPluginDetail = "Tax Plug-in Details";
		public const string TaxPluginMapping = "Tax Plug-in Mapping";
		public const string FailedToCreateTaxPlugin = "Failed to create the tax plug-in. Please check that Plug-In(Type) is valid Type Name {0}.";
		public const string FailedToFindTaxPlugin = "Failed to find the tax plug-in with the specified ID {0}.";
		public const string ConnectionTaxAskSuccessHeader = "Settings";
		public const string ConnectionTaxAskSuccess = "The connection to the tax provider was successful.";
		public const string TestFailed = "The test has failed. Details: {0}.";
		public const string TaxZoneFK = "Tax plug-in cannot be deleted because one or more tax zones depends on this tax plug-in.";

		#endregion

		#region Combo Values
		// TaxType
		public const string Output = "Output";
		public const string Input = "Input";

		#region Tax Bucket Type
		public const string Purchase = "Purchase";
		#endregion

		#region Tax Period Type
		public const string HalfMonth = "Half a Month";
		public const string Month = "Month";
		public const string TwoMonths = "Two Months";
		public const string Quarter = "Quarter";
		public const string HalfYear = "Half a Year";
		public const string Year = "Year";
		public const string FinancialPeriod = "Financial Period";

		#endregion

		#region Tax Period Status
		public const string Prepared = "Prepared";
		public const string Open = "Open";
		public const string Closed = "Closed";
		#endregion

		#region Adjustment Status
		public const string AdjHold = "On Hold";
		public const string AdjBalanced = "Balanced";
		public const string AdjReleased = "Released";
		#endregion

		#region Adjustment Types
		public const string AdjustOutput = "Adjust Output";
		public const string AdjustInput = "Adjust Input";
		#endregion

		#region VAT Invoice Types
		public const string InputVAT = "Input VAT";
		public const string OutputVAT = "Output VAT";
		#endregion

		#region Tax Type
		public const string Sales = "Sales";
		public const string Use = "Use";
		public const string VAT = "VAT";
		public const string Withholding = "Withholding";
		public const string PerUnit = "Per-Unit/Specific";
		#endregion
		//CSTaxTermsDiscount
		public const string DiscountToTaxableAmount = "Reduces Taxable Amount";
		public const string DiscountToPromtPayment = "Reduces Taxable Amount on Early Payment";
		public const string DiscountToTotalAmount = "Does Not Affect Taxable Amount";

		#region SVAT Tax Reversal Methods
		public const string OnPayments = "On Payments";
		public const string OnDocuments = "On Documents";
		#endregion

		#region Tax Entry Ref. Nbr.
		public const string DocumentRefNbr = "Document Ref. Nbr.";
		public const string PaymentRefNbr = "Payment Ref. Nbr.";
		public const string TaxInvoiceNbr = "Tax Invoice Nbr.";
		public const string ManuallyEntered = "Manually Entered";
		#endregion

		#region Tax Report Line
		public const string TaxAmountLine = "Tax Amount";
		public const string TaxableAmountLine = "Taxable Amount";
		public const string ExemptedAmountLine = "Exempted Amount";
		#endregion

		#region TaxCalcRule
		public const string CalcRuleDocInclusive = "Inclusive Document-Level";
		public const string CalcRuleExtract = "Inclusive Line-Level";
		public const string CalcRuleItemAmt = "Exclusive Line-Level";
		public const string CalcRuleItemAmtPlusTaxAmt = "Compound Line-Level";
		public const string CalcRuleDocAmt = "Exclusive Document-Level";
		public const string CalcRuleDocAmtPlusTaxAmt = "Compound Document-Level";
		#endregion
		#endregion

		#region Custom Actions
		public const string NewVendor = "New Vendor";
		public const string EditVendor = "Edit Vendor";
		public const string ReviewBatch = "Review Batch";
		public const string Release = "Release";
		public const string Reverse = "Reverse";
		public const string Document = "Document";
		public const string AdjustTax = "Adjust Tax";
		public const string NewAdjustment = "New Adjustment";
		public const string VoidReport = "Void Report";
		public const string ClosePeriod = "Close Period";
		public const string ViewDocuments = "View Documents";
		public const string Report = "Report";
		public const string PrepareTaxReport = "Prepare Tax Report";
		public const string ReleaseTaxReport = "Release Tax Report";
		public const string ViewTaxPeriods = "View Tax Periods";
		public const string Review = "Review";
		public const string TestConnection = "Test Connection";
	    public const string ViewGroupDetails = "Group Details";
		public const string CreateReportLinesForNewTaxZones = "Reload Tax Zones";
		public const string TaxSummary = "Tax Summary";
		public const string TaxDetails = "Tax Details";
		#endregion

		public const string ExternalTaxProvider = "External Tax Provider";
	    public const string ExternalTaxProviderUrlIsMissing = "URL is missing.";
		public const string ExternalTaxProviderIsNotActive = "External tax provider is not activated.";
		public const string ExternalTaxProviderBranchToCompanyCodeMappingIsMissing = "The Branch to Company Code mapping is not specified in the configuration of the external tax provider.";
		public const string Custom = "Custom";
		public const string ConnectionToExternalTaxProviderFailed = "Connection to external tax provider failed.";
		public const string FailedToDeleteFromExternalTaxProvider = "Failed to delete taxes in the external tax provider. Check details in the Trace Log.";
		public const string ExternalTaxProviderTaxId = "External Tax Provider {0}";
		public const string ExternalTaxProviderTaxFor = "External Tax Provider {0} tax for {1}";

		#region Avalara Customer Usage
		public const string FederalGovt = "Federal Government";
		public const string StateLocalGovt = "State/Local Govt.";
		public const string TribalGovt = "Tribal Government";
		public const string ForeignDiplomat = "Foreign Diplomat";
		public const string CharitableOrg = "Charitable Organization";
		public const string Religious = "Religious";
		public const string Resale = "Resale";
		public const string AgriculturalProd = "Agricultural Production";
		public const string IndustrialProd = "Industrial Prod/Mfg.";
		public const string DirectPayPermit = "Direct Pay Permit";
		public const string DirectMail = "Direct Mail";
		public const string Other = "Other";
		public const string Education = "Education";
		public const string LocalGovt = "Local Government";
		public const string ComAquaculture = "Commercial Aquaculture";
		public const string ComFishery = "Commercial Fishery";
		public const string NonResident = "Non-resident";
		public const string Default = "Default";
		#endregion

		public const string CantUseManualVAT = "Selected tax zone is configured for manual VAT entry mode and is not available for this document. ";

		#region Field Labels
		public const string NetTaxAmount = "Net Tax Amount";

		public const string TaxRateLabelForPerUnitTax = "Tax Amount";
		public const string TaxRateDefaultLabel = "Tax Rate";
		#endregion

		#region Per-unit tax post options
		public const string PostPerUnitTaxToLineAccount = "Line Account";
		public const string PostPerUnitTaxToTaxAccount = "Provisional Account";
		#endregion
	}
}
