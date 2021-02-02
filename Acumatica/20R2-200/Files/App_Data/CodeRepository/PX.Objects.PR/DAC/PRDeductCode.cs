using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.PM;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRDeductCode)]
	[PXPrimaryGraph(typeof(PRDedBenCodeMaint))]
	[Serializable]
	public class PRDeductCode : IBqlTable
	{
		#region CodeID
		public abstract class codeID : PX.Data.BQL.BqlInt.Field<codeID> { }
		[PXDBIdentity]
		[PXUIField(DisplayName = "Code ID")]
		[PXReferentialIntegrityCheck]
		public int? CodeID { get; set; }
		#endregion
		#region CodeCD
		public abstract class codeCD : PX.Data.BQL.BqlString.Field<codeCD> { }
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(PRDeductCode.codeCD))]
		public string CodeCD { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(60, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public string Description { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public bool? IsActive { get; set; }
		#endregion
		#region IsGarnishment
		public abstract class isGarnishment : PX.Data.BQL.BqlBool.Field<isGarnishment> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Garnishment")]
		[PXUIEnabled(typeof(Where<contribType.IsEqual<ContributionType.employeeDeduction>>))]
		public bool? IsGarnishment { get; set; }
		#endregion
		#region AffectsTaxes
		public abstract class affectsTaxes : PX.Data.BQL.BqlBool.Field<affectsTaxes> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Affects tax calculation")]
		[PXUIEnabled(typeof(Where<isWorkersCompensation.IsNotEqual<True>>))]
		public bool? AffectsTaxes { get; set; }
		#endregion
		#region ContribType
		public abstract class contribType : PX.Data.BQL.BqlString.Field<contribType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Contribution Type", Required = true)]
		[ContributionType.List(typeof(isWorkersCompensation), typeof(isPayableBenefit))]
		public string ContribType { get; set; }
		#endregion
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[VendorNonEmployeeActive]
		public int? BAccountID { get; set; }
		#endregion
		#region IncludeType
		public abstract class includeType : PX.Data.BQL.BqlString.Field<includeType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(SubjectToTaxes.None)]
		[PXUIField(DisplayName = "Subject to Taxes")]
		[SubjectToTaxes.List(typeof(isPayableBenefit), typeof(affectsTaxes))]
		[PXUIEnabled(typeof(Where<PRDeductCode.affectsTaxes, Equal<True>>))]
		public string IncludeType { get; set; }
		#endregion
		#region BenefitTypeCD
		public abstract class benefitTypeCD : PX.Data.BQL.BqlInt.Field<benefitTypeCD> { }
		[PXDBInt]
		[PXDefault]
		[PXUIField(DisplayName = "Benefit Type")]
		[PRTypeSelector(typeof(PRBenefit))]
		[PXUIEnabled(typeof(Where<PRDeductCode.includeType, Equal<SubjectToTaxes.perTaxEngine>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.includeType, Equal<SubjectToTaxes.perTaxEngine>>))]
		[PXFormula(typeof(Switch<Case<Where<PRDeductCode.includeType, Equal<SubjectToTaxes.perTaxEngine>>, PRDeductCode.benefitTypeCD>, Null>))]
		public int? BenefitTypeCD { get; set; }
		#endregion
		#region DedCalcType
		public abstract class dedCalcType : PX.Data.BQL.BqlString.Field<dedCalcType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(DedCntCalculationMethod.FixedAmount)]
		[PXUIField(DisplayName = "Calculation Method")]
		[DedCntCalculationMethod.List(typeof(isWorkersCompensation), typeof(affectsTaxes), typeof(isPayableBenefit))]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>>))]
		public string DedCalcType { get; set; }
		#endregion
		#region DedAmount
		public abstract class dedAmount : PX.Data.BQL.BqlDecimal.Field<dedAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXDefault]
		[PXUIField(DisplayName = "Amount")]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PRDeductCode.dedCalcType, IsNotNull,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>>>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PRDeductCode.dedCalcType, IsNotNull,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>>>>))]
		public Decimal? DedAmount { get; set; }
		#endregion
		#region DedPercent
		public abstract class dedPercent : PX.Data.BQL.BqlDecimal.Field<dedPercent> { }
		[PXDBDecimal(MinValue = 0)]
		[PXDefault]
		[PXUIField(DisplayName = "Percent")]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PRDeductCode.dedCalcType, IsNotNull,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PRDeductCode.dedCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PRDeductCode.dedCalcType, IsNotNull,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>>>))]
		public Decimal? DedPercent { get; set; }
		#endregion
		#region DedMaxAmount
		public abstract class dedMaxAmount : PX.Data.BQL.BqlDecimal.Field<dedMaxAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXDefault]
		[PXUIField(DisplayName = "Maximum Amount")]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>,
			And<PRDeductCode.dedMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>,
			And<PRDeductCode.dedMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>))]
		public Decimal? DedMaxAmount { get; set; }
		#endregion
		#region DedMaxFreqType
		public abstract class dedMaxFreqType : PX.Data.BQL.BqlString.Field<dedMaxFreqType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(DeductionMaxFrequencyType.NoMaximum)]
		[PXUIField(DisplayName = "Maximum Frequency")]
		[DeductionMaxFrequencyType.List]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType.IsNotEqual<ContributionType.employerContribution>
			.And<PRDeductCode.isWorkersCompensation.IsEqual<False>>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>>))]
		public string DedMaxFreqType { get; set; }
		#endregion
		#region DedApplicableEarnings
		public abstract class dedApplicableEarnings : PX.Data.BQL.BqlString.Field<dedApplicableEarnings> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Applicable Earnings")]
		[DedBenApplicableEarnings(typeof(dedCalcType))]
		public string DedApplicableEarnings { get; set; }
		#endregion
		#region DedReportType
		public abstract class dedReportType : PX.Data.BQL.BqlInt.Field<dedReportType> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Reporting Type")]
		[PRReportingTypeSelector(typeof(PRBenefit))]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employerContribution>>))]
		public int? DedReportType { get; set; }
		#endregion

		#region CntCalcType
		public abstract class cntCalcType : PX.Data.BQL.BqlString.Field<cntCalcType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(DedCntCalculationMethod.FixedAmount)]
		[PXUIField(DisplayName = "Calculation Method")]
		[DedCntCalculationMethod.List(typeof(isWorkersCompensation), typeof(affectsTaxes), typeof(isPayableBenefit))]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType.IsNotEqual<ContributionType.employeeDeduction>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employeeDeduction>>))]
		public string CntCalcType { get; set; }
		#endregion
		#region CntAmount
		public abstract class cntAmount : PX.Data.BQL.BqlDecimal.Field<cntAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXDefault]
		[PXUIField(DisplayName = "Amount")]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PRDeductCode.cntCalcType, IsNotNull,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>>>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfGross>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfCustom>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.percentOfNet>,
			And<PRDeductCode.cntCalcType, IsNotNull,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>>>>))]
		public Decimal? CntAmount { get; set; }
		#endregion
		#region CntPercent
		public abstract class cntPercent : PX.Data.BQL.BqlDecimal.Field<cntPercent> { }
		[PXDBDecimal(MinValue = 0)]
		[PXDefault]
		[PXUIField(DisplayName = "Percent")]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PRDeductCode.cntCalcType, IsNotNull,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.fixedAmount>,
			And<PRDeductCode.cntCalcType, NotEqual<DedCntCalculationMethod.amountPerHour>,
			And<PRDeductCode.cntCalcType, IsNotNull,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>>>))]
		public Decimal? CntPercent { get; set; }
		#endregion
		#region CntMaxAmount
		public abstract class cntMaxAmount : PX.Data.BQL.BqlDecimal.Field<cntMaxAmount> { }
		[PRCurrency(MinValue = 0)]
		[PXDefault]
		[PXUIField(DisplayName = "Maximum Amount")]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PRDeductCode.cntMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employeeDeduction>,
			And<PRDeductCode.cntMaxFreqType, NotEqual<DeductionMaxFrequencyType.noMaximum>,
			And<PRDeductCode.isWorkersCompensation, Equal<False>>>>))]
		public Decimal? CntMaxAmount { get; set; }
		#endregion
		#region CntMaxFreqType
		public abstract class cntMaxFreqType : PX.Data.BQL.BqlString.Field<cntMaxFreqType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(DeductionMaxFrequencyType.NoMaximum)]
		[PXUIField(DisplayName = "Maximum Frequency")]
		[DeductionMaxFrequencyType.List]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType.IsNotEqual<ContributionType.employeeDeduction>
			.And<PRDeductCode.isWorkersCompensation.IsEqual<False>>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employeeDeduction>>))]
		public string CntMaxFreqType { get; set; }
		#endregion
		#region CntApplicableEarnings
		public abstract class cntApplicableEarnings : PX.Data.BQL.BqlString.Field<cntApplicableEarnings> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Applicable Earnings")]
		[DedBenApplicableEarnings(typeof(cntCalcType))]
		public string CntApplicableEarnings { get; set; }
		#endregion
		#region CntReportType
		public abstract class cntReportType : PX.Data.BQL.BqlInt.Field<cntReportType> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Reporting Type")]
		[PRReportingTypeSelector(typeof(PRBenefit))]
		[PXUIEnabled(typeof(Where<PRDeductCode.contribType, NotEqual<ContributionType.employeeDeduction>>))]
		public int? CntReportType { get; set; }
		#endregion
		#region DedInvDescrType
		public abstract class dedInvDescrType : PX.Data.BQL.BqlString.Field<dedInvDescrType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Invoice Descr Source")]
		[PXUIRequired(typeof(Where<PRDeductCode.bAccountID, IsNotNull>))]
		[InvoiceDescriptionType.DeductionInvoiceDescriptionList(typeof(PRDeductCode.isGarnishment))]
		public string DedInvDescrType { get; set; }
		#endregion
		#region VndInvDescr
		public abstract class vndInvDescr : PX.Data.BQL.BqlString.Field<vndInvDescr> { }
		[PXDBString(60, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Vendor Invoice Description")]
		[PXUIEnabled(typeof(Where<PRDeductCode.dedInvDescrType, Equal<InvoiceDescriptionType.freeFormatEntry>>))]
		[PXUIRequired(typeof(Where<PRDeductCode.dedInvDescrType, Equal<InvoiceDescriptionType.freeFormatEntry>>))]
		public string VndInvDescr { get; set; }
		#endregion
		#region DedLiabilityAcctID
		public abstract class dedLiabilityAcctID : PX.Data.BQL.BqlInt.Field<dedLiabilityAcctID> { }
		[Account(DisplayName = "Deduction Liability Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<PRDeductCode.dedLiabilityAcctID>.IsRelatedTo<Account.accountID>))]
		[PRDedLiabilityAccountRequired(GLAccountSubSource.DeductionCode, typeof(Where<isPayableBenefit.IsNotEqual<True>.And<contribType.IsNotEqual<ContributionType.employerContribution>>>))]
		[PXUIVisible(typeof(Where<isPayableBenefit.IsNotEqual<True>.And<contribType.IsNotEqual<ContributionType.employerContribution>>>))]
		public virtual Int32? DedLiabilityAcctID { get; set; }
		#endregion
		#region DedLiabilitySubID
		public abstract class dedLiabilitySubID : PX.Data.BQL.BqlInt.Field<dedLiabilitySubID> { }
		[SubAccount(typeof(PRDeductCode.dedLiabilityAcctID), DisplayName = "Deduction Liability Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRDeductCode.dedLiabilitySubID>.IsRelatedTo<Sub.subID>))]
		[PRDedLiabilitySubRequired(GLAccountSubSource.DeductionCode, typeof(Where<isPayableBenefit.IsNotEqual<True>.And<contribType.IsNotEqual<ContributionType.employerContribution>>>))]
		[PXUIVisible(typeof(Where<isPayableBenefit.IsNotEqual<True>.And<contribType.IsNotEqual<ContributionType.employerContribution>>>))]
		public virtual Int32? DedLiabilitySubID { get; set; }
		#endregion
		#region BenefitExpenseAcctID
		public abstract class benefitExpenseAcctID : PX.Data.BQL.BqlInt.Field<benefitExpenseAcctID> { }
		[Account(DisplayName = "Benefit Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<PRDeductCode.benefitExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		[PRBenExpenseAccountRequired(
			GLAccountSubSource.DeductionCode,
			typeof(Where<contribType.IsNotEqual<ContributionType.employeeDeduction>
				.And<noFinancialTransaction.IsEqual<False>>>))]
		[PXUIVisible(typeof(Where<contribType.IsNotEqual<ContributionType.employeeDeduction>.And<noFinancialTransaction.IsEqual<False>>>))]
		public virtual Int32? BenefitExpenseAcctID { get; set; }
		#endregion
		#region BenefitExpenseSubID
		public abstract class benefitExpenseSubID : PX.Data.BQL.BqlInt.Field<benefitExpenseSubID> { }
		[SubAccount(typeof(PRDeductCode.benefitExpenseAcctID), DisplayName = "Benefit Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRDeductCode.benefitExpenseSubID>.IsRelatedTo<Sub.subID>))]
		[PRBenExpenseSubRequired(
			GLAccountSubSource.DeductionCode,
			typeof(Where<contribType.IsNotEqual<ContributionType.employeeDeduction>
				.And<noFinancialTransaction.IsEqual<False>>>))]
		[PXUIVisible(typeof(Where<contribType.IsNotEqual<ContributionType.employeeDeduction>.And<noFinancialTransaction.IsEqual<False>>>))]
		public virtual Int32? BenefitExpenseSubID { get; set; }
		#endregion
		#region BenefitLiabilityAcctID
		public abstract class benefitLiabilityAcctID : PX.Data.BQL.BqlInt.Field<benefitLiabilityAcctID> { }
		[Account(DisplayName = "Benefit Liability Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<PRDeductCode.benefitLiabilityAcctID>.IsRelatedTo<Account.accountID>))]
		[PRBenLiabilityAccountRequired(
			GLAccountSubSource.DeductionCode,
			typeof(Where<isPayableBenefit.IsNotEqual<True>
				.And<contribType.IsNotEqual<ContributionType.employeeDeduction>>
				.And<noFinancialTransaction.IsEqual<False>>>))]
		[PXUIVisible(typeof(Where<isPayableBenefit.IsNotEqual<True>
			.And<contribType.IsNotEqual<ContributionType.employeeDeduction>>
			.And<noFinancialTransaction.IsEqual<False>>>))]
		public virtual Int32? BenefitLiabilityAcctID { get; set; }
		#endregion
		#region BenefitLiabilitySubID
		public abstract class benefitLiabilitySubID : PX.Data.BQL.BqlInt.Field<benefitLiabilitySubID> { }
		[SubAccount(typeof(PRDeductCode.benefitLiabilityAcctID), DisplayName = "Benefit Liability Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRDeductCode.benefitLiabilitySubID>.IsRelatedTo<Sub.subID>))]
		[PRBenLiabilitySubRequired(
			GLAccountSubSource.DeductionCode,
			typeof(Where<isPayableBenefit.IsNotEqual<True>
				.And<contribType.IsNotEqual<ContributionType.employeeDeduction>>
				.And<noFinancialTransaction.IsEqual<False>>>))]
		[PXUIVisible(typeof(Where<isPayableBenefit.IsNotEqual<True>
			.And<contribType.IsNotEqual<ContributionType.employeeDeduction>>
			.And<noFinancialTransaction.IsEqual<False>>>))]
		public virtual Int32? BenefitLiabilitySubID { get; set; }
		#endregion
		#region IsWorkersCompensation
		public abstract class isWorkersCompensation : PX.Data.BQL.BqlBool.Field<isWorkersCompensation> { }
		[PXDBBool]
		[PXDefault(false)]
		public bool? IsWorkersCompensation { get; set; }
		#endregion
		#region IsCertifiedProject
		public abstract class isCertifiedProject : PX.Data.BQL.BqlBool.Field<isCertifiedProject> { }
		[PXDBBool]
		[PXDefault(false)]
		public bool? IsCertifiedProject { get; set; }
		#endregion
		#region IsUnion
		public abstract class isUnion : PX.Data.BQL.BqlBool.Field<isUnion> { }
		[PXDBBool]
		[PXDefault(false)]
		public bool? IsUnion { get; set; }
		#endregion
		#region IsPayableBenefit
		public abstract class isPayableBenefit : PX.Data.BQL.BqlBool.Field<isPayableBenefit> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Payable Benefit")]
		public bool? IsPayableBenefit { get; set; }
		#endregion
		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		[PXDBString(50, IsUnicode = true)]
		[State(typeof(countryID))]
		[PXUIField(DisplayName = "State")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIRequired(typeof(isWorkersCompensation.IsEqual<True>))]
		[PXCheckUnique]
		public virtual string State { get; set; }
		#endregion
		#region CertifiedReportType
		public abstract class certifiedReportType : PX.Data.BQL.BqlInt.Field<certifiedReportType> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Certified Reporting Type")]
		[PRReportingTypeSelector(typeof(PRCertifiedBenefit), Payroll.TaxCategory.Employer, false)]
		public virtual int? CertifiedReportType { get; set; }
		#endregion
		#region EarningsIncreasingWageIncludeType
		public abstract class earningsIncreasingWageIncludeType : PX.Data.BQL.BqlString.Field<earningsIncreasingWageIncludeType> { }
		[PXDBString(3)]
		[PXDefault(SubjectToTaxes.None)]
		[PXUIField(DisplayName = "Inclusion Type")]
		[SubjectToTaxes.List(false)]
		[PXUIEnabled(typeof(Where<dedCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>
			.Or<cntCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>>>))]
		public string EarningsIncreasingWageIncludeType { get; set; }
		#endregion
		#region BenefitsIncreasingWageIncludeType
		public abstract class benefitsIncreasingWageIncludeType : PX.Data.BQL.BqlString.Field<benefitsIncreasingWageIncludeType> { }
		[PXDBString(3)]
		[PXDefault(SubjectToTaxes.None)]
		[PXUIField(DisplayName = "Inclusion Type")]
		[SubjectToTaxes.List(false)]
		[PXUIEnabled(typeof(Where<isPayableBenefit.IsEqual<False>
			.And<dedCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>
				.Or<cntCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>>>>))]
		public string BenefitsIncreasingWageIncludeType { get; set; }
		#endregion
		#region TaxesIncreasingWageIncludeType
		public abstract class taxesIncreasingWageIncludeType : PX.Data.BQL.BqlString.Field<taxesIncreasingWageIncludeType> { }
		[PXDBString(3)]
		[PXDefault(SubjectToTaxes.None)]
		[PXUIField(DisplayName = "Inclusion Type")]
		[SubjectToTaxes.List(false)]
		[PXUIEnabled(typeof(Where<isPayableBenefit.IsEqual<False>
			.And<dedCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>
				.Or<cntCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>>>>))]
		public string TaxesIncreasingWageIncludeType { get; set; }
		#endregion
		#region DeductionsDecreasingWageIncludeType
		public abstract class deductionsDecreasingWageIncludeType : PX.Data.BQL.BqlString.Field<deductionsDecreasingWageIncludeType> { }
		[PXDBString(3)]
		[PXDefault(SubjectToTaxes.None)]
		[PXUIField(DisplayName = "Inclusion Type")]
		[SubjectToTaxes.List(false)]
		[PXUIEnabled(typeof(Where<isPayableBenefit.IsEqual<False>
			.And<dedCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>
				.Or<cntCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>>>>))]
		public string DeductionsDecreasingWageIncludeType { get; set; }
		#endregion
		#region TaxesDecreasingWageIncludeType
		public abstract class taxesDecreasingWageIncludeType : PX.Data.BQL.BqlString.Field<taxesDecreasingWageIncludeType> { }
		[PXDBString(3)]
		[PXDefault(SubjectToTaxes.None)]
		[PXUIField(DisplayName = "Inclusion Type")]
		[SubjectToTaxes.List(false)]
		[PXUIEnabled(typeof(Where<isPayableBenefit.IsEqual<False>
			.And<dedCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>
				.Or<cntCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>>>>))]
		public string TaxesDecreasingWageIncludeType { get; set; }
		#endregion
		#region NoFinancialTransaction
		public abstract class noFinancialTransaction : PX.Data.BQL.BqlBool.Field<noFinancialTransaction> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "No Financial Transaction")]
		[PXUIEnabled(typeof(Where<contribType.IsNotEqual<ContributionType.employeeDeduction>
			.And<isWorkersCompensation.IsEqual<False>>
			.And<isPayableBenefit.IsEqual<False>>>))]
		public virtual bool? NoFinancialTransaction { get; set; }
		#endregion

		#region Country
		public abstract class countryID : PX.Data.BQL.BqlString.Field<countryID> { }
		[PXString(2)]
		// TODO: AC-138220, In the Payroll Phase 2 review all the places where the country is set to "US" by the default
		[PXUnboundDefault(typeof(LocationConstants.CountryUS))]
		[PXUIField(Visible = false)]
		public virtual string CountryID { get; set; }
		#endregion
		#region ShowApplicableWageTab
		public abstract class showApplicableWageTab : PX.Data.BQL.BqlBool.Field<showApplicableWageTab> { }
		[PXBool]
		[PXUIField(Visible = false)]
		[PXUnboundDefault(typeof(True.When<dedCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>
			.Or<cntCalcType.IsEqual<DedCntCalculationMethod.percentOfCustom>>>
			.Else<False>))]
		[PXFormula(typeof(Default<dedCalcType, cntCalcType>))]
		public bool? ShowApplicableWageTab { get; set; }
		#endregion
		#region AssociatedSource
		public abstract class associatedSource : PX.Data.BQL.BqlString.Field<associatedSource> { }
		[PXString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Associated With")]
		[DeductCodeSource(typeof(isWorkersCompensation), typeof(isCertifiedProject), typeof(isUnion))]
		public virtual string AssociatedSource { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region Obsolete
		#region WorkCodeID
		public abstract class workCodeID : PX.Data.BQL.BqlString.Field<workCodeID> { }
		[Obsolete]
		[PMWorkCode(FieldClass = null, DisplayName = "Workers' Compensation Code")]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public string WorkCodeID { get; set; }
		#endregion
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
	}
}
