using PX.Common;

namespace PX.Objects.PR
{
	[PXLocalizable(Messages.Prefix)]
	public static class Messages
	{
		public const string Prefix = "PR Error";

		#region GLAccountSource List
		public const string BenefitAndDeductionCode = "Benefit & Deduction Code";
		public const string Branch = "Branch";
		public const string CompanyDefault = "Company Default";
		public const string Department = "Department";
		public const string EarningType = "Earning Type";
		public const string EmployeeClass = "Employee Class";
		public const string JobCode = "Job Code";
		public const string Location = "Location";
		public const string Position = "Position";
		public const string ShiftCode = "Shift Code";
		public const string TaxCode = "Tax Code";
		public const string Union = "Union";
		#endregion

		#region Jurisdiction List
		public const string Federal = "Federal";
		public const string State = "State";
		public const string Local = "Local";
		public const string Municipal = "Municipal";
		public const string SchoolDistrict = "School Disrict";
		public const string UnknownJurisdiction = "<Unknown>";
		#endregion

		#region Tax Category List
		public const string EmployerTax = "Employer Tax";
		public const string EmployeeWithholding = "Employee Withholding";
		public const string UnknownTaxCategory = "<Unknown>";
		#endregion

		#region Wage Base Inclusion List
		public const string AllWages = "All Wages";
		public const string StateSpecific = "State Specific";
		public const string LocationSpecific = "Location Specific";
		#endregion

		#region Subject To Taxes List
		public const string All = "All";
		public const string AllButList = "All But List Below";
		public const string NoneButList = "None But List Below";
		#endregion

		#region Contribution Subject To Taxes List
		public const string AllCSTT = "All";
		public const string NoneCSTT = "None";
		public const string PerTaxEngine = "Per Tax Engine";
		public const string FromListCSTT = "From List Below";
		#endregion

		#region Deduction/Contribution Calculation Method List
		public const string FixedAmount = "Fixed Amount";
		public const string PercentOfGross = "Percent of Gross";
		public const string PercentOfNet = "Percent of Net";
		public const string AmountPerHour = "Amount Per Hour";
		public const string PercentOfCustom = "Percent of Custom";
		#endregion

		#region Unit Type List
		public const string Hour = "Hour";
		public const string Year = "Year";
		public const string Misc = "Miscellaneous";
		#endregion

		#region Invoice Description Type List
		public const string Code = "Code";
		public const string CodeName = "Code Name";
		public const string CodeAndCodeName = "Code + Code Name";
		public const string EmployeeGarnishDescription = "Employee Garnish Description";
		public const string EmployeeGarnishDescriptionPlusPaymentDate = "Employee Garnish Descr + Payment Date";
		public const string PaymentDate = "Payment Date";
		public const string PaymentDateAndCode = "Payment Date + Code";
		public const string PaymentDateAndCodeName = "Payment Date + Code Name";
		public const string FreeFormatEntry = "<Free Format Entry>";
		#endregion

		#region Deduction Max Frequency Type List
		public const string NoMaximum = "No Maximum";
		public const string PerPayPeriod = "Per Pay Period";
		public const string PerCalendarYear = "Per Calendar Year";
		#endregion

		#region Contribution Type List
		public const string EmployeeDeduction = "Employee Deduction";
		public const string EmployerContribution = "Employer Contribution";
		public const string BothDeductionAndContribution = "Both Deduction & Contribution";
		#endregion

		#region Income Calculation Type List
		public const string SpecificList = "Specific List";
		public const string Formula = "Formula";
		#endregion

		#region Deduction Split Type List
		public const string Even = "Even";
		public const string ProRata = "Pro-Rata";
		#endregion

		#region Employee Type List
		public const string SalariedExempt = "Salaried-Exempt";
		public const string SalariedNonExempt = "Salaried Non-Exempt";
		public const string Hourly = "Hourly";
		public const string Other = "Other – piecework, commission-only, etc.";
		#endregion

		#region Bank Account Type List
		public const string Checking = "Checking";
		public const string Savings = "Savings";
		#endregion

		#region Bank Account Status Type List
		public const string Active = "Active";
		public const string Inactive = "Inactive";
		#endregion

		#region Batch Status Type List
		public const string Open = "Open";
		public const string Created = "Created";
		public const string Released = "Released";
		#endregion

		#region Pay Periods
		public const string Weekly = "Weekly";
		public const string Biweekly = "Biweekly";
		public const string Semimonthly = "Semimonthly";
		public const string Monthly = "Monthly";
		public const string BiMonthly = "Bimonthly";
		public const string Quarterly = "Quarterly";

		public const string ShiftedPeriodDescrFormat = "Shifted {0} from {1}";
		#endregion

		#region PRPayment Status List
		public const string OpenPRST = "Open";
		public const string Calculated = "Calculated";
		public const string NeedCalculation = "Needs Calculation";
		public const string CheckPrintedOrPaid = "Check Printed/Paid";
		public const string DirectDepositPrinted = "Direct Deposit Printed";
		public const string DirectDepositExported = "Direct Deposit Exported";
		public const string ChecksAndDDsPrinted = "Checks & DDs Printed";
		public const string ReleasedPRST = "Released";
		#endregion

		#region Payment Type List
		public const string Regular = "Regular";
		public const string Special = "Special";
		public const string Adjustment = "Adjustment";
		public const string VoidCheck = "Void Check";
		#endregion

		#region Payment Status Type List
		public const string OpenPST = "Open";
		public const string CalculatedPST = "Calculated";
		public const string RequiresRecalculationPST = "Requires Recalculation";
		public const string CheckPrintedPST = "Check Printed";
		public const string DirectDepositPrintedPST = "Direct Deposit Printed";
		public const string DirectDepositExportedPST = "Direct Deposit Exported";
		public const string ReleasedPST = "Released";
		public const string Void = "Void";
		public const string Voided = "Voided";
		public const string ReprintedCheck = "Reprinted Check";
		public const string Hold = "Hold";
		public const string LiabilityPartiallyPaid = "Liability Partially Paid";
		public const string Closed = "Closed";
		public const string PendingPrintOrPayment = "Pending Print/Payment";
		#endregion

		#region Check Reprint Type List
		public const string PrinterIssue = "Printer Issue";
		public const string Lost = "Lost";
		public const string Damaged = "Damaged";
		public const string Stolen = "Stolen";
		public const string Corrected = "Corrected";
		#endregion

		#region Check Void Type List
		public const string PrinterIssueCVT = "Printer Issue";
		public const string LostCVT = "Lost";
		public const string DamagedCVT = "Damaged";
		public const string StolenCVT = "Stolen";
		public const string ErrorCorrection = "Error Correction";
		#endregion

		#region Required Calculation Level List
		public const string CalculateAll = "Calculate All";
		public const string CalculateTaxesAndNetPay = "Calculate Taxes and Net Pay";
		public const string CalculateNetPayOnly = "Calculate Net Pay Only";
		public const string NoCalculationRequired = "No Calculation Required";
		#endregion

		#region Earning Detail Source Type List

		public const string TimeActivity = "Time Activity";
		public const string QuickPay = "Quick Pay";
		public const string SalesCommission = "Sales Commission";

		#endregion

		#region Batch Status Type List
		public const string Balanced = "Balanced";
		#endregion

		#region Carryover Type List
		public const string Partial = "Partial";
		public const string Total = "Total";
		public const string PaidOnTimeLimit = "Paid After a Period of Time";
		#endregion Carryover Type List

		#region Overtime Rule Type List
		public const string DailyRule = "Daily";
		public const string WeeklyRule = "Weekly";
		#endregion

		#region Earning Type List
		public const string Salary = "Salary";
		public const string Overtime = "Overtime";
		public const string Piecework = "Piecework";
		public const string AmountBased = "Amount Based";
		public const string TimeOff = "Time Off";
		#endregion

		#region Overtime Rule Warnings
		public const string WeeklyOvertimeRulesApplyToWeeklyPeriods = "Weekly Overtime Rules can be applied to Weekly and Biweekly pay periods only.";
		public const string InconsistentBaseEarningDetailRecord = "Inconsistent Base Earning Detail record (ID: {0}) for current Overtime Earning Detail record (ID: {1}).";
		#endregion

		#region Error Messages
		public const string SegmentNotSupportedInMaskForAccountType = "Segment {0} can't be used for masks of this account type. Allowed values are: {1}";
		public const string AddressNotRecognized = "The tax calculation service does not recognize the address specified for {0}. Make sure that the address is complete and valid.";
		public const string ValueBlankAndRequiredAndNotOverridable = "The Value box cannot be empty for a setting that has the Required check box selected and the Allow Employee Override check box cleared.";
		public const string ValueBlankAndRequired = "The Value box cannot be empty for a setting that has the Required check box selected.";
		public const string TaxSettingNotFound = "The {0} setting is not configured for the parent entity.";
		public const string StartDateMustBeLessOrEqualToTheEndDate = "Start date must be less or equal to the end date.";
		public const string NoTaxCodeExistForTaxType = "No tax code is setup for tax type {0}.";
		public const string AtLeastOneRemainderDD = "At least one active Direct Deposit must have 'Gets Remainder' checked";
		public const string AccountNotAPayrollEmployee = "This account is not a payroll employee.";
		public const string DeductionCodeIsInUseAndCantBeDeleted = "This deduction code is in use and can't be deleted.";
		public const string EmployeeClassIsInUseAndCantBeDeleted = "This employee class is in use and can't be deleted.";
		public const string IncomeCodeIsInUseAndCantBeDeleted = "This income code is in use and can't be deleted.";
		public const string PayGroupIsInUseAndCantBeDeleted = "This pay group is in use and can't be deleted.";
		public const string TaxTypeIsNotImplemented = "Tax Type with with ID {0} is not implemented.";
		public const string CantGetPrimaryView = IN.Messages.CantGetPrimaryView;
		public const string Document_Status_Invalid = AP.Messages.Document_Status_Invalid;
		public const string CannotReleaseInInitializationMode = "Cannot release PR transaction to GL in Initialization Mode.";
		public const string PayGroupPeriodsDefined = "You cannot delete the Payroll Year Setup because one or more periods for the pay group exist in the system.";
		public const string DeleteGeneratedPeriods = "Delete Pay Group Periods";
		public const string CalendarMonthlyTransactionsDateOutOfRange = "Transactions Start Date must occur during the first month of the year.";
		public const string CalendarBiMonthlyTransactionsDateOutOfRange = "Transactions Start Date must occur during the two first month of the year.";
		public const string CalendarQuarterTransactionsDateOutOfRange = "Transactions Start Date must occur during the first quarter of the year.";
		public const string CalendarCustomTransactionsDateOutOfRange = "Transactions Start Date must occur before {0:D}.";
		public const string CalendarTransactionsDateOtherYear = "Transactions Start Date must occur during the payroll year.";
		public const string CalendarSemiMonthlySecondPeriodsOutOfRange = "Second Period must start 10 to 20 after the first one.";
		public const string CalendarSemiMonthlySecondTransactionsOutOfRange = "Second Transaction Date must be 10 to 20 after the first one.";
		public const string NonZeroYTDAmounts = "This row contain non-zero YTD amounts and can not be deleted.";
		public const string DuplicateDeductionCode = "You can not add duplicate deduction codes.";
		public const string AccountCantBeEmpty = "Account can not be empty.";
		public const string SubaccountCantBeEmpty = "Subaccount can not be empty.";
		public const string DedCalculationErrorFormat = "Deduction amount must be set for {0} deduction.";
		public const string CntCalculationErrorFormat = "Contribution amount must be set for {0} contribution.";
		public const string DuplicateEmployeeDeduct = "Duplicate Deduction Definition. This line overlaps with another Deduction (Start Date: {0}, End Date: {1})";
		public const string InconsistentDeductDate = "Deduction End Date can't be before Deduction Start Date (Start Date: {0}, End Date: {1})";
		public const string DuplicateEmployeeEarning = "Duplicate Earning Definition. This line overlaps with another Earning (Start Date: {0}, End Date: {1})";
		public const string InconsistentEarningDate = "Earning End Date can't be before Earning Start Date (Start Date: {0}, End Date: {1})";
		public const string InvalidNegative = "Value cannot be negative.";
		public const string DeductionMaxLimitExceededWarn = "{0} maximum for deduction has been {1}.";
		public const string BenefitMaxLimitExceededWarn = "{0} maximum for contribution has been {1}.";
		public const string Reached = "reached";
		public const string Exceeded = "exceeded";
		public const string DeductionCausesNetPayBelowMin = "Deduction amount causes net pay to go below Net Pay Minimum.";
		public const string DeductionAdjustedForNetPayMin = "Calculated deduction amount has been adjusted to respect Net Pay Minimum.";
		public const string GarnishmentCausesPercentOfNetAboveMax = "Overridden garnishment amount causes total garnishment amount to go above Maximum Percent of Net Pay for all Garnishments.";
		public const string GarnishmentAdjustedForPercentOfNetMax = "Calculated garnishment amount has been adjusted to respect Maximum Percent of Net Pay for all Garnishments.";
		public const string CalculationEngineError = "An error occurred during the calculation of deductions, benefits and taxes. Could not find payment {0}.";
		public const string DeductionAdjustedByTaxEngine = "Deduction amount has been adjusted by tax engine.";
		public const string BenefitAdjustedByTaxEngine = "Benefit amount has been adjusted by tax engine.";
		public const string DuplicateEarningType = "Earning type is already used with another bank.";
		public const string CantUseFieldAsState = "Can't use field {0} as a filter for state.";
		public const string CantBeEmpty = "{0} can not be empty.";
		public const string PostingValueNotFound = "{0} value wasn't found automatically and can not be empty. Verify your posting settings in Payroll Preferences or set it manually.";
		public const string InvalidPayPeriod = "Invalid pay period number";
		public const string PTOUsedExceedsAvailable = "PTO used exceeds available amount.";
		public const string CantFindAatrixEndpoint = "Can't find Aatrix endpoint for requested operation.";
		public const string AatrixOperationTimeout = "Aatrix operation timed out (timeout = {0} ms).";
		public const string AatrixReportProcessingError = "Error while trying to run the Aatrix report.";
		public const string AatrixReportEinMissing = "EIN not set.";
		public const string AatrixReportAatrixVendorIDMissing = "Aatrix Vendor ID not set.";
		public const string AatrixReportFormNameMissing = "Form name not selected.";
		public const string AatrixReportCompanyNameMissing = "Company Name not set.";
		public const string AatrixReportFirstNameMissing = "First Name not set for employee {0}.";
		public const string AatrixReportLastNameMissing = "Last Name not set for employee {0}.";
		public const string AatrixReportInvalidSsn = "Social security number not valid for employee {0} ({1}).";
		public const string AatrixReportSsnNotSet = "Social security number not set for employee {0}.";
		public const string AatrixReportMissingAufInfo = "Can't generate AUF file. Required information in unavailable.";
		public const string UnsupportedTaxJurisdictionLevel = "Unsupported tax jurisdiction level {0}.";
		public const string TaxCodeNotSetUp = "Tax code {0} was calculated but not set-up in the system.";
		public const string CantParseYear = "Can't parse year from string \"{0}\"";
		public const string UnknownOfferOfCoverageCode = "Unknown Offer of Coverage \"{0}\"";
		public const string AcaEinMissing = "Tax registration number missing. It can be set on the Branches or Companies screen.";
		public const string VoidCheckExists = "A void check with this line already exists. Either delete or release that void check.";
		public const string CannotDeactivatePieceworkEarningType = "Cannot deactivate \"Piecework as an Earning Type\". The \"{0}\" Earning Type is already selected as a Piecework Type.";
		public const string CannotDeactivatePieceworkEarningTypeEmployee = "Cannot deactivate \"Piecework as an Earning Type\". The \"{0}\" Unit of Pay is assigned to employee: {1}.";
		public const string CannotDeactivatePieceworkEarningTypePayment = "Cannot deactivate \"Piecework as an Earning Type\". The \"{0}\" Unit of Pay is assigned to payment: {1}.";
		public const string CannotChangeFieldStateEarningType = "The {0} setting cannot be modified because the {1} earning type is assigned as a regular time type for the {2} earning type.";
		public const string CannotChangeFieldStateEmployee = "The {0} setting cannot be modified because the {1} earning type is assigned to the {2} employee.";
		public const string CannotChangeFieldStatePayment = "The {0} setting cannot be modified because the {1} earning type is used in the {2} payment.";
		public const string CannotChangeFieldStatePayrollBatch = "The {0} setting cannot be modified because the {1} earning type is used in the {2} payroll batch.";
		public const string CannotChangeFieldStateSetup = "The {0} setting cannot be modified because the {1} earning type is used in the {2} payroll setting.";
		public const string CannotChangeFieldStateOvertime = "The {0} setting cannot be modified because the {1} earning type is used in the {2} overtime rule.";
		public const string CannotChangeFieldStatePTOBanks = "The {0} setting cannot be modified because the {1} earning type is used in the {2} PTO bank.";
		public const string EarningTypeIsNotActive = "'{0}' Earning Type is not active. Choose another Earning Type.";
		public const string RequiredCheckNumber = "PayCheck Nbr. can not be empty.";
		public const string CantCalculateNegative = "Cannot perform Calculate action on a pay check with negative earning details, deductions or benefits.";
		public const string EarningDetailsWithInactiveEarningTypes = "Paycheck contains Earning Details with inactive Earning Type: '{0}'.";
		public const string DuplicateExceptionDate = "Cannot insert duplicate Transaction Exception Date";
		public const string CantCreatePayGroupYear = "Cannot create payroll year {0} for pay group {1}.";
		public const string CantCreatePayGroupPeriodNumber = "Cannot generate payroll period number for pay group {0}.";
		public const string TotalOver100Pct = "Total is over 100%.";
		public const string NoBankAccountForDirectDeposit = "Bank account information must be set up on the employee to use a Direct Deposit Payment Method.";
		public const string BatchContainsVoidedPayments = "This batch contains voided payments and can't be released.";
		public const string OneBasedDayOfWeekIncorrectValue = "One-based numbering is used for the days of the week but the range of the day numbers is wrong, which may indicate data corruption. Contact your system administrator.";
		public const string CantReleasePaymentWithoutGrossPay = "Payment can't be released without a gross pay.";
		public const string ProjectTaskIsNotActive = "The payment cannot be released because the '{0}' task of the '{1}' project is not active.";
		public const string IncorrectPeriodDate = "Date should be in the range between {0} and {1}.";
		public const string YearNotSetUp = "Payroll year {0} has not been set up.";
		public const string CantFindTaxDefinitionAssembly = "The tax definition assembly is not found. You can update tax definitions by clicking Update Taxes on the Tax Maintenance form.";
		public const string InvalidTaxDefinitionAssembly = "The tax definition assembly is not valid. You can update tax definitions by clicking Update Taxes on the Tax Maintenance form.";
		public const string PayrollCalendarNotSetUp = "Payroll calendar has not been set up for pay group {0}.";
		public const string SameBillRecordError = "A record for the same bill couldn't be processed.";
		public const string VendorRequired = "Vendor is required.";
		public const string DeleteEmployeePayrollSettings = "Can't delete employee with configured payroll settings. Please delete the record on Employee Payroll Settings (PR203000).";
		public const string AttributeKeysInvalid = "{0} doesn't have valid keys defined. Verify the following attribute parameters : {1}, {2}. Check if both key arrays are not empty and have the same number of keys.";
		public const string InactivePREmployee = "Employee '{0}' is not active in Employee Payroll Settings (PR203000).";
		public const string OlderPaymentIsUnreleased = "A payment with an earlier transaction date exists for employee {0} and is not released. Release or delete payment {1} to continue.";
		public const string CantFindPostingPeriod = "Cannot find an open posting period for pay period {0}.";
		public const string FieldCantBeNull = "Field cannot be empty.";
		public const string CantAssignCostToLaborItemAndEarningType = "Cannot assign payroll cost to both Labor Item and Earning Type";
		public const string InactiveEPEmployee = "Employee '{0}' is not marked as Active on the Employees (EP203000) form.";
		public const string CantDuplicateDeductionDetail = "Cannot duplicate deduction detail record.";
		public const string CantDuplicateBenefitDetail = "Cannot duplicate benefit detail record.";
		public const string CantDuplicateTaxDetail = "Cannot duplicate tax detail record.";
		public const string DeductionDetailSumDoesntMatchProject = "The sum of deduction details for {0} must equal the sum of the certified project packages of {1:0.00}.";
		public const string DeductionDetailSumDoesntMatchUnion = "The sum of deduction details for {0} must equal the sum of the union packages of {1:0.00}.";
		public const string DeductionDetailSumDoesntMatchWC = "The sum of deduction details for {0} must equal the sum of the workers' compensation packages of {1:0.00}.";
		public const string BenefitDetailSumDoesntMatchProject = "The sum of benefit details for {0} must equal the sum of the certified project packages of {1:0.00} and the fringe rate amount in benefit of {2:0.00}.";
		public const string BenefitDetailSumDoesntMatchUnion = "The sum of benefit details for {0} must equal the sum of the union packages of {1:0.00}.";
		public const string BenefitDetailSumDoesntMatchWC = "The sum of benefit details for {0} must equal the sum of the workers' compensation packages of {1:0.00}.";
		public const string BenefitSummarySumDoesntMatchProject = "The benefit summary for {0} must equal the sum of the certified project packages of {1:0.00} and the fringe rate amount in benefit of {2:0.00}.";
		public const string BenefitSummarySumDoesntMatchUnion = "The benefit summary for {0} must equal the sum of the union packages of {1:0.00}.";
		public const string TaxDetailSumDoesntMatch = "The sum of tax details for {0} must equal the sum of the tax splits of {1:0.00}.";
		public const string TaxMustBeInSummary = "Tax {0} is not an employer tax or is not present in tax summary.";
		public const string EarningTypeAndLaborItemInAcctSub = "Earning Type and Labor Item cannot be both present in the account and subaccount settings for '{0}' and '{1}'.";
		public const string AdjustmentDetailsWillBeDeleted = "Changing this setting will delete {0} Details for one or more adjustment paychecks. Details will need to be recreated manually or paychecks will have to be Calculated. See trace for details.";
		public const string AdjustmentListWithDeletedDetails = "List of Adjustment paychecks with {0} Details deleted:";
		public const string BankNotFound = "Could not get the PTO bank history because the source bank couldn't be found.";
		public const string InvalidBankStartDate = "Could not get the PTO bank history because the bank start date is missing.";
		public const string InactivePTOBank = "The {0} PTO bank is not marked as Active on the PTO Banks (PR204000) form.";
		public const string CantContactWebservice = "The system cannot connect to the tax calculation service because there is no connection to the Internet or because the service is temporarily unavailable.";
		public const string PeriodsNotGenerated = "No calendar periods have been found. Click Create Periods to generate calendar periods before saving this calendar.";
		public const string DeductCodeInUse = "This deduction and benefit code cannot be edited because it is already in use.";
		public const string PercentOfNetInCertifiedProject = "The Percent of Net calculation method is not supported for certified projects.";
		public const string PercentOfNetInUnion = "The Percent of Net calculation method is not supported for union locals.";
		public const string LocationNotSetInEmployee = "Work location {0} is not assigned to employee {1}.";
		public const string LocationNotSetInEmployeeClass = "Work location {0} is not assigned to employee class {1}.";
		public const string CantAssignTaxesToEmployee = "Because of missing settings, an error occurred while taxes were being assigned to employees. Click Import Taxes on the Taxes tab of the Employee Payroll Settings (PR203000) form to see which settings need to be updated.";
		public const string RequiredTaxSettingNullInCalculate = "Required tax settings are missing for the employee. Make sure that all required settings are specified for the employee on the Tax Settings and Taxes tabs of the Employee Payroll Settings (PR203000) form.";
		public const string TaxUpdateNeeded = "New tax information is available. To update tax definitions, click Update Taxes on the Tax Maintenance form.";
		public const string EmployeeClassHasNoWorkLocation = "No work location is specified for the employee class.";
		public const string EmployeeHasNoTaxes = "Tax settings need to be specified on the Employee Payroll Settings (PR203000) form.";
		public const string CancelPrintedCheckWarning = "This action will put this check back to its pre-print status.";
		public const string RemoveFromDirectDepositWarning = "This action will put this check back to its pre-print status and remove it from its direct deposit batch.";
		public const string CantUseSimultaneously = "The Allow Negative Balance and Can Only Disburse from Carryover check boxes cannot be both selected.";
		public const string NotEnoughLastYearCarryover = "There are not enough hours left from last year's carryover of PTO Bank '{0}'.";
		public const string NotEnoughPTOAvailable = "A negative balance is not allowed. Hours associated with earning code '{0}' can't exceed available hours in PTO Bank '{1}'.";
		public const string CarryoverPaidWithThisEarningLine = "This earning line was created to pay remaining carryover hours.";
		public const string NegativePTOBalanceError = "Can't disallow negative balances because the following employees already have a negative balance {0}.";
		public const string DeductsWillBeRemoved = " This action will also delete deductions (summaries and details), certified project packages, union packages and workers' compensation packages for which the associated deduction and benefit code's source doesn't match.";
		public const string PressOK = " Click OK to proceed.";
		#endregion Error Messages

		#region Payroll Batches Messages
		public const string PayrollBatchReCreateTitle = "Re-Create Batch";
		public const string PayrollBatchReCreateConfirm = "This will delete all current Transaction Detail records. Do you want to continue?";
		#endregion

		#region Miscellaneous Strings
		public const string Add = "Add";
		public const string None = "None";
		public const string NotSetUp = "<Not Set Up>";
		public const string NotImplemented = "<Not Implemented>";
		public const string Remove = "Remove";
		public const string AddTaxCode = "Add Tax Code";
		public const string SemiMonthlyFirstHalfDescr = "First Half - {0:MMMM}";
		public const string SemiMonthlySecondHalfDescr = "Second Half - {0:MMMM}";
		public const string Benefit = "Benefit";
		public const string Deduction = "Deduction";
		public const string Tax = "Tax";
		public const string PayCheckReport = "Check";
		public const string CreatePREmployeeLabel = "Create Payroll Employee";
		public const string EditPREmployeeLabel = "Edit Payroll Employee";
		public const string LaborItem = "Labor Item";
		public const string Others = "others";
		#endregion

		#region PXCacheName Values
		public const string PRAcaAggregateGroupMember = "ACA Aggregate Group Member";
		public const string PRAcaCompanyYearlyInformation = "ACA Company Yearly Information";
		public const string PRAcaCompanyMonthlyInformation = "ACA Company Monthly Information";
		public const string PRAcaDeductCode = "ACA Deduction Code";
		public const string PRAcaDeductCoverageInfo = "ACA Deduction Code Coverage Information";
		public const string PRAcaEmployeeMonthlyInformation = "ACA Employee Monthly Information";
		public const string PRAcaUpdateEmployeeFilter = "ACA Update Employee Filter";
		public const string PRAcaUpdateCompanyMonthFilter = "ACA Update Company Month Filter";
		public const string PRAttribute = "Attribute";
		public const string PRAttributeDetail = "Attribute Detail";
		public const string PRBatch = "Batch";
		public const string PRBatchDeduct = "Batch Deduction";
		public const string PRBatchEmployee = "Batch Employee";
		public const string PRBenefitDetail = "Benefit Detail";
		public const string PRBenefitType = "Benefit Type";
		public const string PRCompanyTaxAttribute = "Company Tax Setting";
		public const string PRDeductCode = "Deduction Code";
		public const string PRDeductCodeBeforeTaxes = "Deduction Code Before Taxes";
		public const string PRDeductCodeSubjectToTaxes = "Deduction Code Subject To Taxes";
		public const string PRDeductionDetails = "Deduction Details";
		public const string PRDeductionSummary = "Deduction Summary";
		public const string PREarningType = "Earning Type";
		public const string PREarningTypeDetail = "Earning Type Detail";
		public const string PREmployee = "Employee";
		public const string PREmployeeAttribute = "Employee Setting";
		public const string PREmployeeClass = "Employee Class";
		public const string PREmployeeClassPTOBank = "Employee Class PTO Bank";
		public const string PREmployeeDirectDeposit = "Employee Direct Deposit";
		public const string PREmployeeDeduct = "Employee Deduct";
		public const string PREmployeeEarning = "Employee Earning";
		public const string PREmployeePTOBank = "Employee PTO Bank";
		public const string PREmployeePTOHistory = "Employee PTO History";
		public const string PREmployeeTax = "Employee Tax";
		public const string PREmployeeTaxAttribute = "Employee Tax Setting";
		public const string PRGovernmentReport = "Government Report";
		public const string PRGovernmentReportingFilter = "Government Reporting Filter";
		public const string PRLocation = "Location";
		public const string PRPayGroup = "Pay Group";
		public const string PRPayGroupYear = "Pay Group Year";
		public const string PayCheckDetail = "Pay Check Detail";
		public const string PayCheckDetailFilter = "Pay Check Detail Filter";
		public const string PRPayGroupCalendar = "Pay Group Calendar";
		public const string PRPayGroupPeriod = "Pay Periods";
		public const string PRPayment = "Payment";
		public const string PRPaymentEarning = "Payment Earning";
		public const string PRPaymentPTOBank = "Payment PTO Bank";
		public const string PRPayPeriodCreationDialog = "Pay Period Creation Dialog";
		public const string PREarningDetail = "Earning Detail";
		public const string PROvertimeRule = "Overtime Rule";
		public const string PRBatchOvertimeRule = "Batch Overtime Rule";
		public const string PRPaymentOvertimeRule = "Payment Overtime Rule";
		public const string PRPaymentTax = "Payment Tax";
		public const string PRPaymentTaxSplit = "Tax Splits";
		public const string PRPTOBank = "PTO Bank";
		public const string PRPeriodTaxes = "Period Taxes";
		public const string PRSetup = "Payroll Preferences";
		public const string PRTaxCode = "Tax Code";
		public const string PRTaxCodeAttribute = "Tax Code Setting";
		public const string PRTaxDetail = "Tax Detail";
		public const string PRTaxType = "Tax Type";
		public const string PRTransactionDateException = "Transaction Date Exception";
		public const string PRYtdEarnings = "YTD Earnings";
		public const string PRYtdDeductions = "YTD Deductions";
		public const string PRYtdTaxes = "YTD Taxes";
		public const string PTOBanksFilter = "PTO Banks Filter";
		public const string PRDeductionAndBenefitUnionPackage = "Deductions And Benefits Union Package";
		public const string PRDeductionAndBenefitProjectPackage = "Deductions And Benefits Project Package";
		public const string PrintChecksFilter = "Print Checks Filter";
		public const string PayStubFilter = "Pay Stub Filter";
		public const string PRProjectFringeBenefitRate = "Project Fringe Benefit Rate";
		public const string PRProjectFringeBenefitRateReducingDeduct = "Project Fringe Benefit Rate Reducing Deduction";
		public const string PRProjectFringeBenefitRateReducingEarning = "Project Fringe Benefit Rate Reducing Earning";
		public const string PRDirectDepositSplit = "Direct Deposit Split";
		public const string PRBatchDetail = "Batch Detail";
		public const string PRWorkCompensationBenefitRate = "Work Compensation Benefit Rate";
		public const string PRPaymentWCPremium = "Payment Work Compensation Premium";
		public const string EmploymentHistory = "Employment History";
		public const string CreateEditPREmployeeFilter = "Create/Edit Payroll Employee Filter";
		public const string PRPaymentUnionPackageDeduct = "Payment Union Package Deduction";
		public const string PRPaymentProjectPackageDeduct = "Payment Project Package Deduction";
		public const string PREmployeeClassWorkLocation = "Employee Class Work Location";
		public const string PREmployeeWorkLocation = "Employee Work Location";
		public const string PRPaymentFringeBenefit = "Payment Fringe Benefit";
		public const string PRPaymentFringeBenefitDecreasingRate = "Payment Fringe Benefit Decreasing Rate";
		public const string PRPaymentFringeEarningDecreasingRate = "Payment Fringe Earning Decreasing Rate";
		public const string PRDeductCodeEarningIncreasingWage = "Deduct Code Earning Increasing Wage";
		public const string PRDeductCodeBenefitIncreasingWage = "Deduct Code Benefit Increasing Wage";
		public const string PRDeductCodeTaxIncreasingWage = "Deduct Code Tax Increasing Wage";
		public const string PRDeductCodeDeductionDecreasingWage = "Deduct Code Deduction Decreasing Wage";
		public const string PRDeductCodeTaxDecreasingWage = "Deduct Code Tax Decreasing Wage";
		#endregion

		#region PRPayBatchEntry Error Messages
		public const string EmployeeIDCannotBeNull = "EmpoyeeID is not specified";
		public const string BatchStartDateCannotBeNull = "Batch Start Date is not specified";
		public const string BatchEndDateCannotBeNull = "Batch End Date is not specified";
		public const string ActivityOnHold = "Activity on Hold";
		public const string ActivityPendingApproval = "Activity Pending Approval";
		public const string ActivityNotReleased = "Activity Not Released";
		public const string EmployeeWasNotEmployed = "Employee was not employed during the Payroll Batch period";
		public const string ActivityWhenNotEmployed = "Activity when employee was not employed";
		public const string RegularHoursTypeIsNotSetUp = "The Regular Hours earning type is not set up for the quick pay process.";
		public const string HolidaysTypeIsNotSetUp = "The Holiday earning type is not set up for the quick pay process.";
		public const string RegularAndHolidaysTypesAreNotSetUp = "The Regular Hours and Holiday earning types are not set up for the quick pay process.";
		public const string CommissionTypeIsNotSetUp = "Commission Earning Type is not set up";
		public const string IncorrectNumberOfPayPeriods = "Incorrect number of Pay Periods";
		public const string EmployeeAlreadyAddedToThisBatch = "The Employee has already been added to this Payroll Batch.";
		public const string EmployeeAlreadyAddedToBatch = "The Employee has already been added to the Regular Payroll Batch ({0}) with the same Pay Period. Would you like to view the existing Payroll Batch or continue editing the current Paycheck? If you save the current Paycheck you would not be able to release the existing Payroll Batch with the current Employee.";
		public const string EmployeeAlreadyAddedToAnotherBatch = "The Employee has already been added to another Regular Payroll Batch ({0}) with the same Pay Period.";
		public const string EmployeeAlreadyAddedToPaycheck = "The Regular Paycheck ({0}) with the same Pay Period has already been created for the Employee.";
		public const string EmployeeAlreadyAddedToPaycheckBatchRelease = "It is impossible to release the Payroll Batch since the Regular Paycheck ({0}) with the same Pay Period has already been created for {1}. To release the Payroll Batch please delete the Document Detail record for this Employee first.";
		public const string EmployeeAlreadyAddedToAnotherPaycheck = "Another Regular Paycheck ({0}) with the same Pay Period has already been created for the Employee. Would you like to view the existing Paycheck or continue editing the current one?";
		public const string EmployeeEarningDetailsCreationFailed = "Creating Earning Details for Employee ({0}) failed with error: {1}";
		public const string EarningDetailsCreationFailedForSomeEmployees = "There were errors during Earning Details creation for some Employees. Please check the trace for more details.";
		#endregion

		#region PRPayRate Warning and Error Messages
		public const string EmptyPayRate = "Pay Rate is empty";
		public const string ZeroPayRate = "Pay Rate is zero";
		public const string NegativePayRate = "Pay Rate is negative";
		public const string EarningTypeNotFound = "'{0}' Earning Type was not found in the {1} table.";
		#endregion

		#region RegularAmount Warning and Error Messages
		public const string IncorrectNumberOfPayPeriodsInPayGroup = "Incorrect number of pay periods in the '{0}' pay group.";
		public const string RegularHoursTypeIsNotSetUpInPayrollPreferences = "Regular Hours earning type is not set up in the Payroll Preferences (PR101000).";
		public const string SuitableEmployeeEarningNotFound = "Suitable employee earning is not found.";
		#endregion

		#region Deduction Warnings
		public const string DeductCodeInactive = "Deduction code is not active.";
		public const string PaymentDeductInactivated = "Deduction has been de-activated in the following pay checks: {0}.";
		public const string CantFindProjectPackageDeduct = "No certified project deduction and benefit package has been found for this combination of Project ID, Labor Item ID, and Deduction and Benefit Code.";
		public const string CantFindUnionPackageDeduct = "No union local deduction and benefit package has been found for this combination of Union Local ID, Labor Item ID, and Deduction and Benefit Code.";
		#endregion Deduction Warnings

		#region PRCreateLiabilitiesAPBill Messages
		public const string PayrollLiabilities = "Payroll Liabilities";
		#endregion PRCreateLiabilitiesAPBill Messages

		#region Government Reporting Period
		public const string Annual = "Annual";
		public const string DateRange = "Date Range";

		public const string Quarter = "Quarter";
		public const string Month = "Month";
		public const string DateFrom = "Date From";
		public const string DateTo = "Date To";
		public const string InconsistentDateRange = "Date To can't be before Date From.";
		#endregion

		#region PaymentMethod

		public const string PrintChecks = "Print Checks";
		public const string CreateBatchPayment = "Create Batch Payments";

		#endregion PaymentMethod
		
		#region ACA Reporting
		public const string QualifyingOfferMethod = "Qualifying Offer Method";
		public const string NinetyEightPctOfferMethod = "98% Offer Method";
		public const string FullTime = "Full Time";
		public const string PartTime = "Part Time";
		public const string Employee = "Employee";
		public const string Spouse = "Spouse";
		public const string Children = "Children";
		public const string MeetsEssentialCoverageAndValue = "Meets Minimum Essential Coverage and provides Minimum Value";
		public const string MeetsEssentialCoverage = "Meets Minimum Essential Coverage but does not provide Minimum Value";
		public const string SelfInsured = "Self-Insured";
		public const string NoneOfTheAbove = "None of the Above";
		#endregion

		#region Payment Deduction Sources
		public const string EmployeeSettingsSource = "Employee Settings";
		public const string CertifiedProjectSource = "Certified Project";
		public const string UnionSource = "Union";
		public const string WorkCodeSource = "Work Class Compensation";
		#endregion
		
		#region Document Release Actions
		public const string PutOnHoldAction = "Put on Hold";
		public const string RemoveFromHoldAction = "Remove from Hold";
		public const string Calculate = "Calculate";
		public const string Recalculate = "Recalculate";
		public const string Release = "Release";
		#endregion
		
		#region Transaction Date Exception Behaviors
		public const string TransactionDayBefore = "Paid first business day before exception";
		public const string TransactionDayAfter = "Paid first business day after exception";
		public const string PeriodWillBeShifted = "Period will be shifted to year {0}.";
		public const string CantChangePeriodTransDate = "Cannot change period transaction date. Pay period is already being used by payment: {0}.";
		#endregion

		#region Direct Deposit Messages

		public const string ViewPRDocument = "View Payment Document";
		public const string PrintPayStubs = "Print Pay Stubs";

		#endregion Direct Deposit Messages

		#region Project Cost Assignment Types
		public const string NoCostAssigned = "No Cost Assigned";
		public const string WageCostAssigned = "Wage Costs Assigned";
		public const string WageLaborBurdenAssigned = "Wage Costs and Labor Burden Assigned";
		#endregion

		#region Tax setting messages
		public const string NewTaxSetting = "Review and, if needed, update the new tax setting.";
		#endregion

		#region GL Tran descriptions
		public const string DefaultPaymentDescriptionFormat = "Paycheck for {0} - {1}";
		public const string EarningDescriptionFormat = "Earning {0} - {1}";
		public const string DeductionLiabilityFormat = "Deduction Liability for {0}";
		public const string BenefitExpenseFormat = "Benefit Expense for {0}";
		public const string BenefitLiabilityFormat = "Benefit Liability for {0}";
		public const string TaxExpenseFormat = "Tax Expense for {0}";
		public const string TaxLiabilityFormat = "Tax Liability for {0}";
		#endregion

		#region Months
		public const string January = "January";
		public const string February = "February";
		public const string March = "March";
		public const string April = "April";
		public const string May = "May";
		public const string June = "June";
		public const string July = "July";
		public const string August = "August";
		public const string September = "September";
		public const string October = "October";
		public const string November = "November";
		public const string December = "December";
		#endregion Months

		#region Deduction/Benefit Applicable Earnings
		public const string TotalEarnings = "Total Earnings";
		public const string RegularEarnings = "Regular Earnings";
		public const string RegularAndOTEarnings = "Regular and OT Earnings";
		public const string StraightTimeEarnings = "Straight Time Earnings";
		#endregion

		#region Project/Tasks warnings

		public const string ProjectStatusWarning = "Project status is '{0}'.";
		public const string TaskStatusWarning = "Task status is '{0}'.";

		#endregion

		public const string WorkersCompensationFormat = "Workers Compensation - {0}";
		public const string AskSkipCarryoverPayments = "Do you want to skip all carryover payment for this pay check?";
		public const string EarningTypeInactive = "The earning type is not active.";
	}
}
