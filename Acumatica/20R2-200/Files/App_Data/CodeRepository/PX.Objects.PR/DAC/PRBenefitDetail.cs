using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.PM;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRBenefitDetail)]
	[Serializable]
	public class PRBenefitDetail : IBqlTable, IPaycheckDetail
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
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<Current<PRBenefitDetail.batchNbr>>>>))]
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
		[PXParent(typeof(Select<PRPayment, Where<PRPayment.docType, Equal<Current<PRBenefitDetail.paymentDocType>>, And<PRPayment.refNbr, Equal<Current<PRBenefitDetail.paymentRefNbr>>>>>))]
		public string PaymentRefNbr { get; set; }
		#endregion
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[GL.Branch(typeof(Parent<PRPayment.branchID>), IsDetail = false)]
		public int? BranchID { get; set; }
		#endregion
		#region CodeID
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(
			typeof(SearchFor<PRDeductCode.codeID>
				.Where<PRDeductCode.contribType.IsNotEqual<ContributionType.employeeDeduction>
					.And<PRDeductCode.noFinancialTransaction.IsEqual<False>>>),
			SubstituteKey = typeof(PRDeductCode.codeCD),
			DescriptionField = typeof(PRDeductCode.description))]
		[PXCheckUnique(typeof(branchID), typeof(paymentRefNbr), typeof(paymentDocType), typeof(projectID), typeof(projectTaskID), typeof(labourItemID), typeof(earningTypeCD), typeof(costCodeID),
			ErrorMessage = Messages.CantDuplicateBenefitDetail,
			ClearOnDuplicate = false)]
		[PXRestrictor(typeof(
			Where<PRDeductCode.isActive.IsEqual<True>>),
			Messages.DeductCodeInactive)]
		public int? CodeID { get; set; }
		#endregion
		#region IsPayableBenefit
		public abstract class isPayableBenefit : PX.Data.BQL.BqlBool.Field<isPayableBenefit> { }
		[PXBool]
		[PXFormula(typeof(Selector<codeID, PRDeductCode.isPayableBenefit>))]
		[PXUIField(Visible = false)]
		public bool? IsPayableBenefit { get; set; }
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
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.BenefitLaborItem, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public virtual int? LabourItemID { get; set; }
		#endregion
		#region EarningTypeCD
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		[PXDBString(2, IsUnicode = true, InputMask = ">LL")]
		[PXUIField(DisplayName = "Earning Type Code")]
		[PREarningTypeSelector]
		[PXForeignReference(typeof(Field<earningTypeCD>.IsRelatedTo<EPEarningType.typeCD>))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.BenefitEarningType, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public string EarningTypeCD { get; set; }
		#endregion
		#region ExpenseAccountID
		public abstract class expenseAccountID : PX.Data.BQL.BqlInt.Field<expenseAccountID> { }
		[BenExpenseAccount(
			typeof(branchID),
			typeof(codeID),
			typeof(employeeID),
			typeof(PRPayment.payGroupID),
			typeof(earningTypeCD),
			typeof(labourItemID),
			DisplayName = "Expense Account")]
		public virtual Int32? ExpenseAccountID { get; set; }
		#endregion
		#region ExpenseSubID
		public abstract class expenseSubID : PX.Data.BQL.BqlInt.Field<expenseSubID> { }
		[BenExpenseSubAccount(typeof(PRBenefitDetail.expenseAccountID), typeof(PRBenefitDetail.branchID), true,
			DisplayName = "Expense Sub.", Visibility = PXUIVisibility.Visible, Filterable = true)]
		public virtual int? ExpenseSubID { get; set; }
		#endregion
		#region LiabilityAccountID
		public abstract class liabilityAccountID : PX.Data.BQL.BqlInt.Field<liabilityAccountID> { }
		[BenLiabilityAccount(typeof(PRBenefitDetail.branchID), 
			typeof(PRBenefitDetail.codeID), 
			typeof(PRBenefitDetail.employeeID), 
			typeof(PRPayment.payGroupID), 
			typeof(PRBenefitDetail.codeID), 
			typeof(PRBenefitDetail.isPayableBenefit), DisplayName = "Liability Account")]
		public virtual Int32? LiabilityAccountID { get; set; }
		#endregion
		#region LiabilitySubID
		public abstract class liabilitySubID : PX.Data.BQL.BqlInt.Field<liabilitySubID> { }
		[BenLiabilitySubAccount(typeof(PRBenefitDetail.liabilityAccountID), typeof(PRBenefitDetail.branchID), typeof(PRBenefitDetail.isPayableBenefit), true,
			DisplayName = "Liability Sub.", Visibility = PXUIVisibility.Visible, Filterable = true)]
		public virtual int? LiabilitySubID { get; set; }
		#endregion
		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[ProjectBase(DisplayName = "Project")]
		[ProjectDefault]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.BenefitProject, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public int? ProjectID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : PX.Data.BQL.BqlInt.Field<projectTaskID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Task", FieldClass = ProjectAttribute.DimensionName)]
		[PXSelector(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<PRBenefitDetail.projectID>>>>),
			typeof(PMTask.taskCD), typeof(PMTask.description), SubstituteKey = typeof(PMTask.taskCD))]
		[PXUIVisible(typeof(Where<CostAssignmentColumnVisibilityEvaluator.BenefitProject, Equal<True>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.FromCurrent.IsEqual<PayrollType.adjustment>>))]
		public int? ProjectTaskID { get; set; }
		#endregion
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		[CostCode(typeof(liabilityAccountID), typeof(projectTaskID), GL.AccountType.Expense, SkipVerificationForDefault = true, AllowNullValue = true, ReleasedField = typeof(released))]
		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
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

		public int? ParentKeyID { set => CodeID = value; }
	}
}
