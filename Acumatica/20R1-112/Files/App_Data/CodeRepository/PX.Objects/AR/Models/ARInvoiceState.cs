namespace PX.Objects.AR
{
	/// <exclude/>
	public class ARInvoiceState
	{
		public bool RetainageApply;
		public bool IsRetainageDocument;
		public bool IsDocumentReleased;
		public bool IsDocumentInvoice;
		public bool IsDocumentCreditMemo;
		public bool IsDocumentDebitMemo;
		public bool IsRetainageCreditMemo;
		public bool RetainTaxes;
		public bool IsDocumentOnHold;
		public bool IsDocumentOnCreditHold;
		public bool IsDocumentScheduled;
		public bool IsDocumentVoided;
		public bool IsDocumentRejected;
		public bool InvoiceUnreleased;
		public bool IsRetainageApplyInvoice;
		public bool IsRetainageApplyDebitAdjustment;
		public bool IsDocumentRejectedOrPendingApproval;
		public bool IsDocumentApprovedBalanced;
		public bool IsUnreleasedWO;
		public bool IsUnreleasedPPD;
		public bool IsMigratedDocument;
		public bool IsUnreleasedMigratedDocument;
		public bool IsReleasedMigratedDocument;
		public bool IsMigrationMode;
		public bool IsCancellationDocument;
		public bool IsCorrectionDocument;
		public bool IsRegularBalancedDocument;

		public bool CuryEnabled;
		public bool ShouldDisableHeader;
		public bool AllowDeleteDocument;
		public bool DocumentHoldEnabled;
		public bool DocumentDateEnabled;
		public bool DocumentDescrEnabled;
		public bool EditCustomerEnabled;
		public bool AddressValidationEnabled;
		public bool IsTaxZoneIDEnabled;
		public bool IsAvalaraCustomerUsageTypeEnabled;
		public bool ApplyFinChargeVisible;
		public bool ApplyFinChargeEnable;
		public bool ShowCashDiscountInfo;
		public bool IsAssignmentEnabled;
		public bool BalanceBaseCalc;
		public bool AllowDeleteTransactions;
		public bool AllowInsertTransactions;
		public bool AllowUpdateTransactions;
		public bool AllowDeleteTaxes;
		public bool AllowInsertTaxes;
		public bool AllowUpdateTaxes;
		public bool AllowDeleteDiscounts;
		public bool AllowInsertDiscounts;
		public bool AllowUpdateDiscounts;
	}
}
