using System;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CA;
using PX.Objects.CM;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRPayment)]
	[Serializable]
	[PXPrimaryGraph(typeof(PRPayChecksAndAdjustments))]
	public class PRPayment : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected { get; set; }
		#endregion
		#region DocType
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault(PayrollType.Regular)]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		[PayrollType.List]
		[PXFieldDescription]
		public string DocType { get; set; }
		#endregion
		#region RefNbr
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }

		/// <summary>
		/// [key] Reference number of the document.
		/// </summary>
		[PXDBString(15, IsKey = true, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		[PXSelector(typeof(SelectFrom<PRPayment>
			.InnerJoin<EPEmployee>.On<EPEmployee.bAccountID.IsEqual<PRPayment.employeeID>>
			.Where<PRPayment.docType.IsEqual<PRPayment.docType.FromCurrent>>
			.SearchFor<PRPayment.refNbr>),
			typeof(docType), typeof(refNbr), typeof(employeeID), typeof(EPEmployee.acctName), typeof(status), typeof(extRefNbr),
			typeof(payGroupID), typeof(payPeriodID), typeof(startDate), typeof(endDate), typeof(transactionDate))]
		[PayrollType.Numbering]
		[PXFieldDescription]
		public String RefNbr { get; set; }
		#endregion
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Status", Enabled = false, Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault(PaymentStatus.NeedCalculation)]
		[PaymentStatus.List]
		[SetStatus]
		public virtual string Status { get; set; }
		#endregion
		#region Hold
		public abstract class hold : PX.Data.BQL.BqlBool.Field<hold> { }
		/// <summary>
		/// When set to <c>true</c> indicates that the document is on hold and thus cannot be released.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true, typeof(Search<PRSetup.holdEntry>))]
		public virtual Boolean? Hold { get; set; }
		#endregion
		#region Released
		public abstract class released : PX.Data.BQL.BqlBool.Field<released> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Visible = false)]
		public bool? Released { get; set; }
		#endregion
		#region Voided
		public abstract class voided : PX.Data.BQL.BqlBool.Field<voided> { }
		[PXDBBool()]
		[PXUIField(DisplayName = "Voided", Visibility = PXUIVisibility.Visible)]
		[PXDefault(false)]
		public Boolean? Voided { get; set; }
		#endregion
		#region Closed
		public abstract class closed : PX.Data.BQL.BqlBool.Field<closed> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Closed")]
		[PXDefault(false)]
		public virtual Boolean? Closed { get; set; }
		#endregion
		#region LiabilityPartiallyPaid
		public abstract class liabilityPartiallyPaid : PX.Data.BQL.BqlBool.Field<liabilityPartiallyPaid> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Liability Partially Paid")]
		[PXDefault(false)]
		public virtual Boolean? LiabilityPartiallyPaid { get; set; }
		#endregion
		#region Printed
		public abstract class printed : PX.Data.BQL.BqlBool.Field<printed> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Printed")]
		[PXDefault(false)]
		public virtual Boolean? Printed { get; set; }
		#endregion
		#region Calculated
		public abstract class calculated : PX.Data.BQL.BqlBool.Field<calculated> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Calculated")]
		[PXDefault(false)]
		public virtual Boolean? Calculated { get; set; }
		#endregion
		#region HasUpdatedGL
		public abstract class hasUpdatedGL : PX.Data.BQL.BqlBool.Field<hasUpdatedGL> { }
		[PXDBBool]
		[PXUIField(Visible = false)]
		[PXDefault(typeof(PRSetup.updateGL))]
		public virtual bool? HasUpdatedGL { get; set; }
		#endregion
		#region ExtRefNbr
		public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
		[PXDBString(40, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Ref.", Visibility = PXUIVisibility.SelectorVisible)]
		//TODO: Numbering [PaymentRef(typeof(APPayment.cashAccountID), typeof(APPayment.paymentMethodID), typeof(APPayment.stubCntr), typeof(APPayment.updateNextNumber))]
		public virtual string ExtRefNbr { get; set; }
		#endregion
		#region PayGroupID
		public abstract class payGroupID : PX.Data.BQL.BqlString.Field<payGroupID> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Pay Group", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIEnabled(typeof(Where<payBatchNbr.IsNull>))]
		[PXDefault]
		[PXSelector(typeof(Search<PRPayGroup.payGroupID>), DescriptionField = typeof(PRPayGroup.description))]
		public string PayGroupID { get; set; }
		#endregion
		#region IsWeekOrBiWeekPeriod
		public abstract class isWeeklyOrBiWeeklyPeriod : PX.Data.BQL.BqlBool.Field<isWeeklyOrBiWeeklyPeriod> { }
		[PXBool]
		[PXFormula(typeof(Default<payGroupID>))]
		[PXUnboundDefault(typeof(SearchFor<PRPayGroupYearSetup.isWeeklyOrBiWeeklyPeriod>.Where<PRPayGroupYearSetup.payGroupID.IsEqual<payGroupID.FromCurrent>>))]
		[PXUIField(DisplayName = "Is Weekly or Biweekly Period", Visible = false)]
		public bool? IsWeeklyOrBiWeeklyPeriod { get; set; }
		#endregion
		#region PayPeriodID
		public abstract class payPeriodID : PX.Data.BQL.BqlString.Field<payPeriodID> { }
		[PXUIField(DisplayName = "Pay Period", Visibility = PXUIVisibility.SelectorVisible)]
		[PXUIEnabled(typeof(Where<payBatchNbr.IsNull>))]
		[PRPayGroupPeriodID(typeof(payGroupID), typeof(startDate), null, typeof(endDate), typeof(transactionDate), false,
			typeof(Where<docType.IsEqual<PayrollType.special>.And<payBatchNbr.IsNull>>), true)]
		public string PayPeriodID { get; set; }
		#endregion
		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Period Start", Visibility = PXUIVisibility.SelectorVisible)]
		public DateTime? StartDate { get; set; }
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Period End", Visibility = PXUIVisibility.SelectorVisible)]
		public DateTime? EndDate { get; set; }
		#endregion
		#region TransactionDate
		public abstract class transactionDate : PX.Data.BQL.BqlDateTime.Field<transactionDate> { }
		[PXDBDate]
		[PXDefault]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.SelectorVisible)]
		public DateTime? TransactionDate { get; set; }
		#endregion
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[PXUIField(DisplayName = "Posting Period")]
		[PROpenPeriod(typeof(PRPayment.transactionDate))]
		public string FinPeriodID { get; set; }
		#endregion FinPeriodID
		#region BranchID
		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		[PXFormula(typeof(Default<employeeID>))]
		[GL.Branch(typeof(SelectFrom<Branch>.InnerJoin<EPEmployee>.On<Branch.bAccountID.IsEqual<EPEmployee.parentBAccountID>>.
			Where<EPEmployee.bAccountID.IsEqual<employeeID.FromCurrent>>.SearchFor<Branch.branchID>),
			IsDetail = false,
			Visibility = PXUIVisibility.SelectorVisible)]
		public int? BranchID { get; set; }
		#endregion
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }
		[PXDBString(10, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Method")]
		[PXSelector(typeof(SearchFor<PaymentMethod.paymentMethodID>.Where<PRxPaymentMethod.useForPR.IsEqual<True>>), DescriptionField = typeof(PaymentMethod.descr))]
		[PXFormula(typeof(Default<PRPayment.employeeID>))]
		[PXDefault(typeof(SearchFor<PREmployee.paymentMethodID>.Where<PREmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>))]
		public virtual string PaymentMethodID { get; set; }
		#endregion
		#region CashAccountID
		public abstract class cashAccountID : PX.Data.BQL.BqlInt.Field<cashAccountID> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Cash Account")]
		[PXSelector(typeof(Search2<CashAccount.cashAccountID,
			InnerJoin<PaymentMethodAccount, On<PaymentMethodAccount.cashAccountID, Equal<CashAccount.cashAccountID>>>,
			Where<PaymentMethodAccount.paymentMethodID, Equal<Current<PRPayment.paymentMethodID>>, And<PRxPaymentMethodAccount.useForPR, Equal<True>>>>),
			SubstituteKey = typeof(CashAccount.cashAccountCD),
			DescriptionField = typeof(CashAccount.descr))]
		[PXFormula(typeof(Default<PRPayment.paymentMethodID>))]
		[PXDefault(typeof(SearchFor<PREmployee.cashAccountID>.Where<PREmployee.bAccountID.IsEqual<PRPayment.employeeID.FromCurrent>>))]
		[PXUIEnabled(typeof(Where<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>>))]
		public virtual int? CashAccountID { get; set; }
		#endregion
		#region DocDesc
		public abstract class docDesc : PX.Data.BQL.BqlString.Field<docDesc> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string DocDesc { get; set; }
		#endregion
		//TODO : REVIEW 3 Chk fields
		#region ChkReprintType
		public abstract class chkReprintType : PX.Data.BQL.BqlString.Field<chkReprintType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Reprint Reason", Enabled = false)]
		[CheckReprintType.List]
		public virtual string ChkReprintType { get; set; }
		#endregion
		#region ChkVoidType
		public abstract class chkVoidType : PX.Data.BQL.BqlString.Field<chkVoidType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Void Reason", Enabled = false)]
		[CheckVoidType.List]
		public virtual string ChkVoidType { get; set; }
		#endregion
		#region ChkCreateNew
		public abstract class chkCreateNew : PX.Data.BQL.BqlBool.Field<chkCreateNew> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Create New Check from Voided Check")]
		public virtual bool? ChkCreateNew { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[EmployeeActiveInPayGroup]
		[PXDefault]
		[PXUIEnabled(typeof(Where<PRPayment.docType.IsNotEqual<PayrollType.voidCheck>.And<payBatchNbr.IsNull>>))]
		[PXForeignReference(typeof(Field<PRPayment.employeeID>.IsRelatedTo<PREmployee.bAccountID>))]
		public virtual int? EmployeeID { get; set; }
		#endregion
		#region EmpType
		public abstract class empType : PX.Data.BQL.BqlString.Field<empType> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(Visible = false)]
		[PXFormula(typeof(Default<employeeID>))]
		[PXUnboundDefault(typeof(Selector<employeeID, PREmployee.empType>))]
		[EmployeeType.List]
		public virtual string EmpType { get; set; }
		#endregion
		#region SalariedNonExempt
		public abstract class salariedNonExempt : PX.Data.BQL.BqlBool.Field<salariedNonExempt> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Salaried Non-Exempt")]
		[PXUIEnabled(typeof(Where<empType.IsEqual<EmployeeType.salariedNonExempt>>))]
		[PXFormula(typeof(Default<empType>))]
		[PXDefault(typeof(True.When<empType.IsEqual<EmployeeType.salariedNonExempt>.And<docType.IsEqual<PayrollType.regular>>>.Else<False>))]
		public virtual bool? SalariedNonExempt { get; set; }
		#endregion
		#region RegularAmount
		public abstract class regularAmount : PX.Data.BQL.BqlDecimal.Field<regularAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Regular Amount to be paid")]
		public virtual Decimal? RegularAmount { get; set; }
		#endregion
		#region ManualRegularAmount
		public abstract class manualRegularAmount : PX.Data.BQL.BqlBool.Field<manualRegularAmount> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Manual Amount")]
		[PXDefault(false)]
		public virtual bool? ManualRegularAmount { get; set; }
		#endregion
		#region PayBatchNbr
		public abstract class payBatchNbr : PX.Data.BQL.BqlString.Field<payBatchNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Pay Batch Nbr.")]
		[PXDBDefault(typeof(PRBatch.batchNbr), DefaultForUpdate = true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXParent(typeof(Select<PRBatch, Where<PRBatch.batchNbr, Equal<Current<payBatchNbr>>>>))]
		public virtual string PayBatchNbr { get; set; }
		#endregion
		#region BatchNbr
		public abstract class batchNbr : PX.Data.BQL.BqlString.Field<batchNbr> { }
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Nbr.", Visibility = PXUIVisibility.Visible, Enabled = false)]
		[PXSelector(typeof(Search<Batch.batchNbr, Where<Batch.module, Equal<BatchModule.modulePR>>>))]
		public virtual string BatchNbr { get; set; }
		#endregion
		#region TotalEarnings
		public abstract class totalEarnings : PX.Data.BQL.BqlDecimal.Field<totalEarnings> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(Visible = false)]
		public virtual decimal? TotalEarnings { get; set; }
		#endregion
		#region GrossAmount
		public abstract class grossAmount : PX.Data.BQL.BqlDecimal.Field<grossAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Gross Pay", Enabled = false)]
		[PXFormula(typeof(totalEarnings.Add<payableBenefitAmount>))]
		public virtual decimal? GrossAmount { get; set; }
		#endregion
		#region DedAmount
		public abstract class dedAmount : PX.Data.BQL.BqlDecimal.Field<dedAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Deductions", Enabled = false)]
		public virtual decimal? DedAmount { get; set; }
		#endregion
		#region TaxAmount
		public abstract class taxAmount : PX.Data.BQL.BqlDecimal.Field<taxAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Taxes", Enabled = false)]
		public virtual decimal? TaxAmount { get; set; }
		#endregion
		#region NetAmount
		public abstract class netAmount : PX.Data.BQL.BqlDecimal.Field<netAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Net Pay", Enabled = false)]
		[PXFormula(typeof(Sub<Sub<PRPayment.grossAmount, PRPayment.dedAmount>, PRPayment.taxAmount>))]
		public virtual decimal? NetAmount { get; set; }
		#endregion
		//TODO: Review multi-currency support in payroll
		#region CuryInfoID
		public abstract class curyInfoID : PX.Data.BQL.BqlLong.Field<curyInfoID> { }
		[PXDBLong]
		[CurrencyInfo(ModuleCode = BatchModule.PR)]
		public virtual Int64? CuryInfoID { get; set; }
		#endregion
		#region PayableBenefitAmount 
		public abstract class payableBenefitAmount : PX.Data.BQL.BqlDecimal.Field<payableBenefitAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(Visible = false)]
		public virtual decimal? PayableBenefitAmount { get; set; }
		#endregion
		#region BenefitAmount 
		public abstract class benefitAmount : PX.Data.BQL.BqlDecimal.Field<benefitAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Total Benefits", Enabled = false)]
		public virtual decimal? BenefitAmount { get; set; }
		#endregion
		#region EmployerTaxAmount 
		public abstract class employerTaxAmount : PX.Data.BQL.BqlDecimal.Field<employerTaxAmount> { }
		[PRCurrency]
		[PXDefault(TypeCode.Decimal, "0.00")]
		[PXUIField(DisplayName = "Total Employer Tax", Enabled = false)]
		public virtual decimal? EmployerTaxAmount { get; set; }
		#endregion
		#region CATranID
		public abstract class caTranID : PX.Data.BQL.BqlLong.Field<caTranID> { }
		[PXDBLong]
		[PRPaymentCashTranID]
		public virtual Int64? CATranID { get; set; }
		#endregion
		#region OrigDocType
		public abstract class origDocType : PX.Data.BQL.BqlString.Field<origDocType> { }
		/// <summary>
		/// Type of the original (source) document.
		/// </summary>
		[PXDBString(3, IsFixed = true)]
		[PayrollType.List]
		[PXUIField(DisplayName = "Original Doc. Type")]
		public virtual String OrigDocType
		{
			get;
			set;
		}
		#endregion
		#region OrigRefNbr
		public abstract class origRefNbr : PX.Data.BQL.BqlString.Field<origRefNbr> { }
		/// <summary>
		/// Reference number of the original (source) document.
		/// </summary>
		[PXDBString(15, IsUnicode = true, InputMask = "")]
		[PXUIField(DisplayName = "Original Document")]
		public virtual string OrigRefNbr
		{
			get;
			set;
		}
		#endregion
		#region DrCr
		public abstract class drCr : PX.Data.BQL.BqlString.Field<drCr> { }
		/// <summary>
		/// Read-only field indicating whether the document is of debit or credit type.
		/// The value of this field is based solely on the <see cref="DocType"/> field.
		/// </summary>
		[PXString(1, IsFixed = true)]
		public string DrCr
		{
			[PXDependsOnFields(typeof(docType))]
			get
			{
				return PayrollType.DrCr(DocType);
			}
			set
			{
			}
		}
		#endregion
		#region AverageRate
		public abstract class averageRate : PX.Data.BQL.BqlDecimal.Field<averageRate> { }
		[PXDecimal]
		[PXUIField(DisplayName = "Average Rate", Enabled = false)]
		[PXUIVisible(typeof(Where<salariedNonExempt.IsNotEqual<True>>))]
		[PXFormula(typeof(Switch<Case<Where<PRPayment.totalHours.IsGreater<decimal0>>,
			PRPayment.grossAmount.Divide<PRPayment.totalHours>>,
			decimal0>))]
		public virtual decimal? AverageRate { get; set; }
		#endregion
		#region TotalHours
		public abstract class totalHours : PX.Data.BQL.BqlDecimal.Field<totalHours> { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total Hours", Enabled = false)]
		public virtual decimal? TotalHours { get; set; }
		#endregion
		#region ExemptFromOvertimeRules
		public abstract class exemptFromOvertimeRules : PX.Data.BQL.BqlBool.Field<exemptFromOvertimeRules> { }
		[PXBool]
		[PXUIField(DisplayName = "Exempt from Overtime Rules", Visible = false)]
		[PXFormula(typeof(Selector<employeeID, PREmployee.exemptFromOvertimeRules>))]
		public bool? ExemptFromOvertimeRules { get; set; }
		#endregion
		#region ApplyOvertimeRules
		public abstract class applyOvertimeRules : PX.Data.BQL.BqlBool.Field<applyOvertimeRules> { }
		[PXDBBool]
		[PXDefault(typeof(Switch<Case<Where<exemptFromOvertimeRules, Equal<True>, Or<Parent<PRBatch.applyOvertimeRules>, Equal<False>>>, False>, True>))]
		[PXUIField(DisplayName = "Apply Overtime Rules for the Document")]
		[PXUIEnabled(typeof(Where<exemptFromOvertimeRules.IsEqual<False>.And<released.IsNotEqual<True>.And<printed.IsNotEqual<True>>>>))]
		[PXFormula(typeof(Default<exemptFromOvertimeRules, payBatchNbr>))]
		public virtual bool? ApplyOvertimeRules { get; set; }
		#endregion
		#region PaymentDocAndRef
		public abstract class paymentDocAndRef : Data.BQL.BqlString.Field<paymentDocAndRef> { }
		[PXUnboundDefault]
		[DocTypeAndRefNbrDisplayName(typeof(docType), typeof(refNbr))]
		[PXUIField(DisplayName = "Paycheck Ref")]
		public string PaymentDocAndRef { get; set; }
		#endregion
		#region IsPrintChecksPaymentMethod
		public abstract class isPrintChecksPaymentMethod : PX.Data.BQL.BqlBool.Field<isPrintChecksPaymentMethod> { }
		[PXBool]
		[PXUIField(DisplayName = "Print Checks Payment Method", Visible = false)]
		[PXFormula(typeof(Selector<PRPayment.paymentMethodID, PRxPaymentMethod.prPrintChecks>))]
		public virtual Boolean? IsPrintChecksPaymentMethod { get; set; }
		#endregion
		#region NetAmountToWords
		public abstract class netAmountToWords : PX.Data.BQL.BqlString.Field<netAmountToWords> { }
		[ToWords(typeof(PRPayment.netAmount))]
		public virtual string NetAmountToWords { get; set; }
		#endregion
		#region LaborCostSplitType
		public abstract class laborCostSplitType : PX.Data.BQL.BqlString.Field<laborCostSplitType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(Visible = false)]
		[CostAssignmentType.List]
		public virtual string LaborCostSplitType { get; set; }
		#endregion
		#region AutoPayCarryover
		public abstract class autoPayCarryover : PX.Data.BQL.BqlBool.Field<autoPayCarryover> { }
		[PXDBBool]
		[PXUIField(DisplayName = "Pay Carryover if Applicable")]
		[PXDefault(true)]
		public virtual Boolean? AutoPayCarryover { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion
		#region System Columns
		#region CreatedByID
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		#endregion

		#region CreatedByScreenID
		[PXDBCreatedByScreenID()]
		public virtual string CreatedByScreenID { get; set; }
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		#endregion

		#region CreatedDateTime
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		#endregion

		#region LastModifiedByID
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		#endregion

		#region LastModifiedByScreenID
		[PXDBLastModifiedByScreenID()]
		public virtual string LastModifiedByScreenID { get; set; }
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		#endregion

		#region LastModifiedDateTime
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		#endregion
		#endregion System Columns
	}
}
