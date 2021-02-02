using PX.Common;

namespace PX.Objects.Common
{
	[PXLocalizable]
	public static class Messages
	{
		#region Validation messages
		public const string EntityWithIDDoesNotExist = "{0} with ID '{1}' does not exist";
		public const string NotApprover = "You are not an authorized approver for this document.";
		public const string IncomingApplicationCannotBeReversed = "This application cannot be reversed from {0}. Open {1} {2} to reverse this application.";
		public const string InitialApplicationCannotBeReversed = "This application cannot be reversed because it is a special application created in Migration Mode, which reflects the difference between the original document amount and the migrated balance.";
		public const string ConstantMustBeDeclaredInsideLabelProvider = "The specified constant type '{0}' must be declared inside a class implementing the ILabelProvider interface.";
		public const string TypeMustImplementLabelProvider = "The specified type '{0}' must implement the ILabelProvider interface.";
		public const string LabelProviderMustHaveParameterlessConstructor = "The label provider class '{0}' must have a parameterless constructor.";
		public const string BqlCommandsHaveDifferentParameters = "The BQL commands have different parameters.";
		public const string ApplicationDoesNotCorrespondToDocument = "The specified application does not correspond to the document";
		public const string FieldIsNotOfStringType = "The {0} field is not of string type.";
		public const string FieldDoesNotHaveItemOrCacheLevelAttribute = "The {0} field does not have an item-level or cache-level {1}.";
		public const string FieldDoesNotHaveCacheLevelAttribute = "The {0} field does not have a cache-level {1}.";
		public const string StringListAttributeDoesNotDefineLabelForValue = "The string list attribute does not define a label for value '{0}'.";
		public const string RecordCanNotBeSaved = "The record cannot be saved because at least one error has occurred. Please review the errors.";
		public const string ExecutionDateBoxCannotBeEmptyIfStopOnExecutionDate = "The Execution Date box cannot be empty if the Stop on Execution Date option is selected.";
		public const string DataIntegrityErrorDuringProcessingDefault = "An error occurred during record processing. Your changes cannot be saved. Please copy the error details from Help > Trace, and contact Support service.";
		public const string DataIntegrityErrorDuringProcessingFormat = "An error occurred during record processing: {0} Your changes cannot be saved. Please copy the error details from Help > Trace, and contact Support service.";
		public const string DataIntegrityGLBatchExistsForUnreleasedDocument = "A General Ledger batch already exists for the document that has not been released.";
		public const string DataIntegrityGLBatchNotExistsForReleasedDocument = "A General Ledger batch has not been generated for the released document.";
		public const string DataIntegrityGLBatchSumsNotEqualGLTransSums = "The total of the General Ledger batch is not equal to the sum of its transactions’ amounts.";
		public const string DataIntegrityReleasedDocumentWithUnreleasedApplications = "The document has been released but some of its applications have not been released.";
		public const string DataIntegrityUnreleasedDocumentWithReleasedApplications = "The document has not been released but some of its applications have been released.";
		public const string DataIntegrityDocumentHasNegativeBalance = "The document has obtained a negative balance.";
		public const string DataIntegrityDocumentTotalsHaveLargerPrecisionThanCurrency = "At least one of the totals of the document has a greater precision than the decimal precision specified in the currency settings.";
		public const string FailedToGetCache = "Cache of the {0} type was not found in the graph. Please contact Acumatica support.";
		public const string CannotFindEntityByKeys = "'{0}' '{1}' cannot be found in the system.";
		public const string CannotPerformActionOnDocumentUnreleasedVoidPaymentExists = "The {0} {1} cannot be {2} because an unreleased {3} document exists for this {4}. To proceed, delete or release the {5} {6} document.";
		public const string Error = "Error";
		public const string Warning = "Warning";
		public const string CannotApplyDocumentUnreleasedIncomingApplicationsExist = "The document has an unreleased application from {0} {1}. To create new applications for the document, remove or release the unreleased application from {0} {1}.";
		public const string BalanceCannotExceedDocumentAmount = "The balance cannot exceed the original document amount.";
		public const string CannotVoidPaymentRegularUnreversedApplications = "The payment cannot be voided because it has unreversed regular applications.";
		public const string MigrationModeActivateGLTransactionFromModuleExist = "The General Ledger batches generated in the module exist in the system.";
		public const string MigrationModeDeactivateUnreleasedMigratedDocumentExist = "Migrated documents that have not been released exist in the system.";
		public const string IncorrectMigratedBalance = "Migrated balance cannot be less than zero or greater than the document amount.";
		public const string DocumentHasBeenRefunded = "{0} {1} has been refunded with {2} {3}.";
		public const string ParameterShouldNotNull = "The parameter should not be null.";
		public const string IsNotBqlField = "{0} is not a BqlField.";
		public const string InterBranchFeatureIsDisabled = "Inter-Branch Transactions feature is disabled.";
		public const string InappropriateSiteOnDetailsTab = "One of the warehouses on the Document Details tab belongs to another branch.";

		public const string ShouldBePositive = "'{0}' should be positive.";
		public const string ShouldNotBeNegative = "'{0}' should not be negative.";
		public const string MustHaveValue = "'{0}' must have a value.";
		public const string NonDefaultAvalaraUsageType = "Select the entity usage type other than Default.";

		public const string CannotDecodeBase64ContentExplicit = "An HTML element with the \"img\" tag cannot be parsed because it has the \"src\" attribute with invalid base64 content.";
		public const string CannotDecodeBase64Content = "Base64 content cannot be decoded.";
		public const string DescriptionCannotBeGenerated = "A description of maximum available length ({0}) cannot be correctly generated for the {1} field. The original description is \"{2}\". The extra long generated description is \"{3}\".";
		public const string ResultLimitation = "The number of parameters must be no more than 25.";
		#endregion

		#region Actions

		public const string Actions = "Actions";
		public const string Reports = "Reports";

		#endregion

		#region Translatable strings used in code
		public const string ScheduleID = "Schedule ID";
		public const string NewSchedule = "New Schedule";
		public const string Identifier = "Identifier";
		public const string MethodIsObsoleteRemoveInLaterAcumaticaVersions = "The method is obsolete and will be removed in the later Acumatica versions.";
		public const string ActivateMigrationMode = "Activate Migration Mode";
		public const string MigrationModeBatchNbr = "MIGRATED";
		public const string IncorrectActionSpecified = "Incorrect action has been specified.";
		public const string Current = "Current";
		public const string PastDue = "Past Due";
		public const string BeforeMonth = "Before {0}";
		public const string AfterMonth = "After {0}";
		public const string IntervalDays = "{0} - {1} Days";
		public const string OverDays = "Over {0} Days";
		public const string IntervalDaysPastDue = "{0} - {1} Days Past Due";
		public const string OverDaysPastDue = "Over {0} Days Past Due";
		public const string IntervalDaysOutstanding = "{0} - {1} Days Outstanding";
		public const string OverDaysOutstanding = "Over {0} Days Outstanding";
		public const string AttributeDeprecatedWillRemoveInAcumatica7 = "This attribute has been deprecated and will be removed in Acumatica ERP 2017R2.";
		public const string FieldIsObsoleteRemoveInAcumatica7 = "This field has been deprecated and will be removed in Acumatica ERP 2017R2.";
		public const string FieldIsObsoleteNotUsedAnymore = "This field has been deprecated and is not used anymore.";
		public const string FieldIsObsoleteRemoveInAcumatica8 = "This field has been deprecated and will be removed in Acumatica ERP 2018R2.";
		public const string AttributeDeprecatedWillRemoveInAcumatica8 = "This attribute has been deprecated and will be removed in Acumatica ERP 2018R2.";
		public const string PropertyIsObsoleteRemoveInAcumatica8 = "This property has been deprecated and will be removed in Acumatica ERP 2018R2.";
		public const string MessageIsObsoleteAndWillBeRemoved2018R1 = "This message has been deprecated and will be removed in Acumatica ERP 2018R1.";
        public const string MessageIsObsoleteAndWillBeRemoved2019R1 = "This message has been deprecated and will be removed in Acumatica ERP 2019R1.";
        public const string ClassIsObsoleteRemoveInAcumatica8 = "This class has been deprecated and will be removed in Acumatica ERP 2018R2.";
		public const string MethodIsObsoleteAndWillBeRemoved2019R1 = "This method has been deprecated and will be removed in Acumatica ERP 2019 R1.";
		public const string MethodIsObsoleteAndWillBeRemoved2019R2 = "This method has been deprecated and will be removed in Acumatica ERP 2019 R2.";
		public const string MethodIsObsoleteAndWillBeRemoved2020R1 = "This method has been deprecated and will be removed in Acumatica ERP 2020 R1.";
		public const string MethodIsObsoleteAndWillBeRemoved2020R2 = "This method has been deprecated and will be removed in Acumatica ERP 2020 R2.";
		public const string FieldIsObsoleteAndWillBeRemoved2019R1 = "This field has been deprecated and will be removed in Acumatica ERP 2019R1.";
        public const string FieldIsObsoleteAndWillBeRemoved2019R2 = "This field has been deprecated and will be removed in Acumatica ERP 2019R2.";
        public const string ClassIsObsoleteAndWillBeRemoved2019R2 = "This class has been deprecated and will be removed in Acumatica ERP 2019 R2.";
	    public const string ItemIsObsoleteAndWillBeRemoved2019R2 = "This item has been deprecated and will be removed in Acumatica ERP 2019 R2.";
		public const string ClassIsObsoleteRemoveInAcumatica2019R1 = "This class has been deprecated and will be removed in Acumatica ERP 2019R1.";
		public const string ClassIsObsoleteRemoveInAcumatica2020R1 = "This class has been deprecated and will be removed in Acumatica ERP 2020R1.";
		public const string ViewIsObsoleteAndWillBeRemoved2020R1 = "This view has been deprecated and will be removed in Acumatica ERP 2020R1.";
		public const string FieldIsObsoleteAndWillBeRemoved2020R1 = "This field has been deprecated and will be removed in Acumatica ERP 2020R1.";
		public const string ClassIsObsoleteRemoveInAcumatica2020R2 = "This class has been deprecated and will be removed in Acumatica ERP 2020R2.";
		public const string ItemIsObsoleteAndWillBeRemoved2021R1 = "This item has been deprecated and will be removed in Acumatica ERP 2021 R1.";
		public const string ItemIsObsoleteAndWillBeRemoved2020R2 = "This item has been deprecated and will be removed in Acumatica ERP 2020 R2.";
		public const string PropertySetIsObsoleteAndWillBeRemoved2021R1 = "The setter of this property has been deprecated and will be removed in Acumatica ERP 2021 R1.";
		public const string AttributeIsObsoleteAndWillBeRemoved2020R2 = "This attribute has been deprecated and will be removed in Acumatica ERP 2020 R2.";
		public const string FieldIsObsoleteAndWillBeRemoved2020R2 = "This field has been deprecated and will be removed in Acumatica ERP 2020R2.";
		public const string Customized = "Customized: {0}.";
		public const string ActionReleased = "released";
		public const string ActionWrittenOff = "written off";
		public const string ActionRefunded = "refunded";
		public const string ActionAdjusted = "adjusted";
		public const string SplitByCurrency = "Split by Currency";
		public const string CurrencyPrepaymentBalance = "Currency Prepayment Balance";
		public const string PrepaymentBalance = "Prepayment Balance";
		public const string DocumentDate = "Document Date";
		public const string ExpenseDate = "Expense Date";
		public const string ExtraDataIntegrityValidation = "Extra Data Validation";
		public const string LogErrors = "Log Issues";
		public const string PreventRelease = "Prevent Release";
		public const string WillBeRemovedInAcumatica2019R1 = "Will be removed in Acumatica 2019R1";
		public const string WillBeRemovedInAcumatica2020R2 = "Will be removed in Acumatica 2020R2";
        public const string ObsoletePayrollFieldToRemove = "It is obsolete Payroll field and will be removed in 2019R1";
		public const string UseMasterCalendar = "Use Master Calendar";

		#endregion

		public const string Charges = "Charges";
		public const string NoCharges = "No charges";
	}
}
