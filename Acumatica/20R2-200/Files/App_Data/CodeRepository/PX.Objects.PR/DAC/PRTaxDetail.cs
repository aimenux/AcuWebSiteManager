using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRTaxDetail)]
	[Serializable]
	public class PRTaxDetail : IBqlTable, IPaycheckDetail
	{
		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[Employee]
		[PXDBDefault(typeof(PRPayment.employeeID))]
		public int? EmployeeID { get; set; }
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number")]
		[PXDBDefault(typeof(PRBatch.batchNbr), DefaultForUpdate = true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<Current<PRTaxDetail.batchNbr>>>>))]
		public string BatchNbr { get; set; }
		#endregion
		#region PaymentDocType
		public abstract class paymentDocType : PX.Data.BQL.BqlString.Field<paymentDocType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Payment Doc. Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		public string PaymentDocType { get; set; }
		#endregion
		#region PaymentRefNbr
		public abstract class paymentRefNbr : PX.Data.BQL.BqlString.Field<paymentRefNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref. Number")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<PRTaxDetail.paymentDocType>>, And<PRPayment.refNbr, Equal<Current<PRTaxDetail.paymentRefNbr>>>>>))]
		public string PaymentRefNbr { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[GL.Branch(typeof(Parent<PRPayment.branchID>), IsDetail = false)]
		public int? BranchID { get; set; }
		#endregion
		#region TaxID
		public abstract class taxID : PX.Data.BQL.BqlInt.Field<taxID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Tax", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(PRTaxCode.taxID), DescriptionField = typeof(PRTaxCode.description), SubstituteKey = typeof(PRTaxCode.taxCD))]
		[PXParent(typeof(Select<PRPaymentTax,
						Where<PRPaymentTax.docType, Equal<Current<PRTaxDetail.paymentDocType>>,
							And<PRPaymentTax.refNbr, Equal<Current<PRTaxDetail.paymentRefNbr>>,
							And<PRPaymentTax.taxID, Equal<Current<PRTaxDetail.taxID>>>>>>))]
		[PXCheckUnique(typeof(branchID), typeof(paymentRefNbr), typeof(paymentDocType), typeof(projectID), typeof(projectTaskID), typeof(labourItemID), typeof(earningTypeCD), typeof(costCodeID),
			ErrorMessage = Messages.CantDuplicateTaxDetail)]
		[TaxIDRestrictor]
		public int? TaxID { get; set; }
		#endregion
		#region TaxCategory
		public abstract class taxCategory : PX.Data.BQL.BqlString.Field<taxCategory> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Tax Category", Enabled = false)]
		[TaxCategory.List]
		[PXFormula(typeof(Selector<PRTaxDetail.taxID, PRTaxCode.taxCategory>))]
		public string TaxCategory { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PRCurrency]
		[PXUIField(DisplayName = "Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public virtual Decimal? Amount { get; set; }
		#endregion
		#region LabourItemID
		public abstract class labourItemID : PX.Data.BQL.BqlInt.Field<labourItemID> { }
		[PMLaborItem(typeof(projectID), null, null)]
		[PXForeignReference(typeof(Field<labourItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.TaxLaborItem, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>
			.And<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>>))]
		public virtual int? LabourItemID { get; set; }
		#endregion
		#region EarningTypeCD
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		[PXDBString(2, IsUnicode = true, InputMask = ">LL")]
		[PXUIField(DisplayName = "Earning Type Code")]
		[PREarningTypeSelector]
		[PXForeignReference(typeof(Field<earningTypeCD>.IsRelatedTo<EPEarningType.typeCD>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.TaxEarningType, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>
			.And<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>>))]
		public string EarningTypeCD { get; set; }
		#endregion
		#region ExpenseAccountID
		public abstract class expenseAccountID : PX.Data.BQL.BqlInt.Field<expenseAccountID> { }
		[TaxExpenseAccount(
			typeof(branchID),
			typeof(taxID),
			typeof(employeeID),
			typeof(PRPayment.payGroupID),
			typeof(taxCategory),
			typeof(earningTypeCD),
			typeof(labourItemID),
			DisplayName = "Expense Account")]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>>))]
		public virtual Int32? ExpenseAccountID { get; set; }
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		[TaxExpenseSubAccount(typeof(PRTaxDetail.expenseAccountID), typeof(PRTaxDetail.branchID), typeof(PRTaxDetail.taxCategory), true,
			DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, Filterable = true)]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>>))]
		public virtual int? ExpenseSubID { get; set; }
		#endregion
		#region LiabilityAccountID
		public abstract class liabilityAccountID : PX.Data.BQL.BqlInt.Field<liabilityAccountID> { }
		[TaxLiabilityAccount(typeof(PRTaxDetail.branchID),
		   typeof(PRTaxDetail.taxID),
		   typeof(PRTaxDetail.employeeID),
		   typeof(PRPayment.payGroupID),
		   typeof(PRTaxDetail.taxID), DisplayName = "Liability Account")]
		public virtual Int32? LiabilityAccountID { get; set; }
		#endregion
		#region LiabilitySubID
		public abstract class liabilitySubID : PX.Data.BQL.BqlInt.Field<liabilitySubID> { }
		[TaxLiabilitySubAccount(typeof(PRTaxDetail.liabilityAccountID), typeof(PRTaxDetail.branchID), true,
			DisplayName = "Liability Sub.", Visibility = PXUIVisibility.Visible, Filterable = true)]
		public virtual int? LiabilitySubID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[ProjectBase(DisplayName = "Project")]
		[TaxDetailProjectDefault(typeof(PRTaxDetail.taxCategory), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>
			.And<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.TaxProject, Equal<True>>))]
		public int? ProjectID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Task", FieldClass = ProjectAttribute.DimensionName)]
		[PXSelector(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<PRTaxDetail.projectID>>>>),
			typeof(PMTask.taskCD), typeof(PMTask.description), SubstituteKey = typeof(PMTask.taskCD))]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>
			.And<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.TaxProject, Equal<True>>))]
		public int? ProjectTaskID { get; set; }
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[CostCode(typeof(liabilityAccountID), typeof(projectTaskID), GL.AccountType.Expense, SkipVerificationForDefault = true, AllowNullValue = true, ReleasedField = typeof(released))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		[PXUIEnabled(typeof(Where<PRTaxDetail.taxCategory.IsEqual<TaxCategory.employerTax>>))]
		public virtual Int32? CostCodeID { get; set; }
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		/// <summary>
		/// Indicates whether the line is released or not.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released")]
		public virtual Boolean? Released { get; set; }
		#endregion
		#region APInvoiceDocType
		public abstract class apInvoiceDocType : PX.Data.BQL.BqlString.Field<apInvoiceDocType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		public string APInvoiceDocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class apInvoiceRefNbr : PX.Data.BQL.BqlString.Field<apInvoiceRefNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public string APInvoiceRefNbr { get; set; }
		#endregion
		#region LiabilityPaid
		public abstract class liabilityPaid : PX.Data.BQL.BqlBool.Field<liabilityPaid> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Liability Paid")]
		public virtual Boolean? LiabilityPaid { get; set; }
		#endregion
		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion

		#region AmountErrorMessage
		public abstract class amountErrorMessage : PX.Data.BQL.BqlString.Field<amountErrorMessage> { }
		[PXString]
		public virtual string AmountErrorMessage { get; set; }
		#endregion

		public int? ParentKeyID { set => TaxID = value; }
	}

	public class TaxIDRestrictorAttribute : PXRestrictorAttribute
	{
		public TaxIDRestrictorAttribute() : base(
			typeof(Where<PRTaxCode.taxID.IsInSubselect<SearchFor<PRPaymentTax.taxID>
				.Where<PRPaymentTax.docType.IsEqual<PRPayment.docType.FromCurrent>
					.And<PRPaymentTax.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>>>),
			Messages.TaxMustBeInSummary,
			typeof(PRTaxCode.taxCD))
		{ }

		public override void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if (!e.ExternalCall)
			{
				return;
			}

			PRTaxCode taxCode = GetItem(sender, this, e.Row, e.NewValue) as PRTaxCode;

			if (!SelectFrom<PRPaymentTax>
				.Where<PRPaymentTax.docType.IsEqual<PRPayment.docType.FromCurrent>
					.And<PRPaymentTax.refNbr.IsEqual<PRPayment.refNbr.FromCurrent>>>.View.Select(sender.Graph).FirstTableItems
				.Any(x => x.TaxID == taxCode.TaxID))
			{
				if (_SubstituteKey != null)
				{
					object errorValue = e.NewValue;
					sender.RaiseFieldSelecting(_FieldName, e.Row, ref errorValue, false);
					PXFieldState state = errorValue as PXFieldState;
					e.NewValue = state != null ? state.Value : errorValue;
				}

				throw new PXSetPropertyException(Messages.TaxMustBeInSummary, taxCode.TaxCD);
			}
		}
	}
}
