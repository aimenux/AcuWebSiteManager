using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.EP;
using PX.Objects.GL;
using PX.Payroll.Data;
using System;
using System.Collections;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PREarningType)]
	[PXPrimaryGraph(typeof(PREarningTypeMaint))]
	[Serializable]
	[PXTable(IsOptional = true)] //ToDo: AC-142439 Ensure PXForeignReference attribute works correctly with PXCacheExtension DACs.
	public sealed class PREarningType : PXCacheExtension<EPEarningType>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region RegularTypeCD
		public abstract class regularTypeCD : BqlString.Field<regularTypeCD> { }
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXDBString(2, IsUnicode = true, InputMask = ">LL", IsFixed = true)]
		[PXUIField(DisplayName = "Regular Time Type Code")]
		[RegularTypeCDSelector(typeof(EPEarningType.typeCD), DescriptionField = typeof(EPEarningType.description))]
		[PXUIVisible(typeof(
			Where<EPEarningType.isOvertime.IsEqual<True>
				.Or<PREarningType.isPTO.IsEqual<True>>>))]
		[PXFormula(typeof(
			Switch<Case<
				Where<EPEarningType.isOvertime.IsNotEqual<True>
					.Or<PREarningType.isPTO.IsNotEqual<True>>>,
				Null>>))]
		[PXReferentialIntegrityCheck]
		public string RegularTypeCD { get; set; }
		#endregion
		#region WageTypeCD
		public abstract class wageTypeCD : BqlInt.Field<wageTypeCD> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Wage Type", Required = true)]
		[PRTypeSelector(typeof(PRWage), PRWage.DefaultWageType)]
		public int? WageTypeCD { get; set; }
		#endregion
		#region IsWCCCalculation
		public abstract class isWCCCalculation : BqlBool.Field<isWCCCalculation> { }
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Contributes to WCC Calculation")]
		public bool? IsWCCCalculation { get; set; }
		#endregion
		#region IsAmountBased
		public abstract class isAmountBased : BqlBool.Field<isAmountBased> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Amount Based", Visible = false)]
		public bool? IsAmountBased { get; set; }
		#endregion
		#region IsPiecework
		public abstract class isPiecework : BqlBool.Field<isPiecework> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Piecework", Visible = false)]
		public bool? IsPiecework { get; set; }
		#endregion
		#region IsTimeOff
		public abstract class isPTO : BqlBool.Field<isPTO> { }
		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Time Off", Visible = false)]
		public bool? IsPTO { get; set; }
		#endregion
		#region EarningTypeCategory
		public abstract class earningTypeCategory : BqlString.Field<earningTypeCategory> { }
		[PXString(3, IsUnicode = false, InputMask = ">LLL", IsFixed = true)]
		[EarningTypeCategory.List]
		[PXUIField(DisplayName = "Earning Type Category")]
		public string EarningTypeCategory { get; set; }
		#endregion
		#region IncludeType
		public abstract class includeType : BqlString.Field<includeType> { }
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Subject to Taxes", Required = true)]
		[PXDefault(SubjectToTaxes.PerTaxEngine, PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[SubjectToTaxes.List]
		public string IncludeType { get; set; }
		#endregion
		#region ReportType
		public abstract class reportType : BqlInt.Field<reportType> { }
		[PXDBInt]
		[PXUIField(DisplayName = "Reporting Type", Required = true)]
		[PRReportingTypeSelector(typeof(PRWage))]
		public int? ReportType { get; set; }
		#endregion
		#region EarningsAcctID
		public abstract class earningsAcctID : BqlInt.Field<earningsAcctID> { }
		[Account(DisplayName = "Earnings Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PREarningAccountRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of account if in use.  Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.earningsAcctID>.IsRelatedTo<Account.accountID>), BqlTable = typeof(PREarningType))]
		public int? EarningsAcctID { get; set; }
		#endregion
		#region EarningsSubID
		public abstract class earningsSubID : BqlInt.Field<earningsSubID> { }
		[SubAccount(typeof(PREarningType.earningsAcctID), DisplayName = "Earnings Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PREarningSubRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of subaccount if in use. Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.earningsSubID>.IsRelatedTo<Sub.subID>), BqlTable = typeof(PREarningType))]
		public int? EarningsSubID { get; set; }
		#endregion
		#region AccruePTO
		public abstract class accruePTO : BqlBool.Field<accruePTO> { }
		[PXDBBool]
		[PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Accrue Time Off")]
		public bool? AccruePTO { get; set; }
		#endregion
		#region BenefitExpenseAcctID
		public abstract class benefitExpenseAcctID : BqlInt.Field<benefitExpenseAcctID> { }
		[Account(DisplayName = "Benefit Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PRBenExpenseAccountRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of account if in use.  Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.benefitExpenseAcctID>.IsRelatedTo<Account.accountID>), BqlTable = typeof(PREarningType))]
		public int? BenefitExpenseAcctID { get; set; }
		#endregion
		#region BenefitExpenseSubID
		public abstract class benefitExpenseSubID : BqlInt.Field<benefitExpenseSubID> { }
		[SubAccount(typeof(PREarningType.benefitExpenseAcctID), DisplayName = "Benefit Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PRBenExpenseSubRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of subaccount if in use. Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.benefitExpenseSubID>.IsRelatedTo<Sub.subID>), BqlTable = typeof(PREarningType))]
		public int? BenefitExpenseSubID { get; set; }
		#endregion
		#region TaxExpenseAcctID
		public abstract class taxExpenseAcctID : BqlInt.Field<taxExpenseAcctID> { }
		[Account(DisplayName = "Tax Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PRTaxExpenseAccountRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of account if in use.  Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.taxExpenseAcctID>.IsRelatedTo<Account.accountID>), BqlTable = typeof(PREarningType))]
		public int? TaxExpenseAcctID { get; set; }
		#endregion
		#region TaxExpenseSubID
		public abstract class taxExpenseSubID : BqlInt.Field<taxExpenseSubID> { }
		[SubAccount(typeof(PREarningType.taxExpenseAcctID), DisplayName = "Tax Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PRTaxExpenseSubRequired(GLAccountSubSource.EarningType)]
		//TODO AC-142439 : Make PXForeignReference work to prevent deletion of subaccount if in use. Other idea could be to have a standalone dac with the attribute instead of using this extension
		//[PXForeignReference(typeof(Field<PREarningType.taxExpenseSubID>.IsRelatedTo<Sub.subID>), BqlTable = typeof(PREarningType))]
		public int? TaxExpenseSubID { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : BqlGuid.Field<noteID> { }
		[PXNote]
		public Guid? NoteID { get; set; }
		#endregion
	}

	public class RegularTypeCDSelectorAttribute : PXCustomSelectorAttribute
	{
		public RegularTypeCDSelectorAttribute(Type fieldType) : base(fieldType) { }

		protected virtual IEnumerable GetRecords()
		{
			foreach (EPEarningType epEarningType in SelectFrom<EPEarningType>
				.Where<EPEarningType.isActive.IsEqual<True>
					.And<EPEarningType.typeCD.IsNotEqual<EPEarningType.typeCD.FromCurrent>>
					.And<EPEarningType.isOvertime.IsNotEqual<True>>>.View.Select(_Graph))
			{
				PREarningType earningTypeExt = _Graph.Caches[typeof(EPEarningType)].GetExtension<PREarningType>(epEarningType);
				string salaryEarningType = _Graph.Caches[typeof(PREarningType)].GetValueExt<PREarningType.earningTypeCategory>(epEarningType) as string;
				if (earningTypeExt == null || salaryEarningType == EarningTypeCategory.Salary)
				{
					yield return epEarningType;
				}
			}
		}
	}
}
