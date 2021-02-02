using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PR
{
	public sealed class PRxINPostClass : PXCacheExtension<INPostClass>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region EarningsAcctID
		public abstract class earningsAcctID : PX.Data.BQL.BqlInt.Field<earningsAcctID> { }
		[Account(DisplayName = "Earnings Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<PRxINPostClass.earningsAcctID>.IsRelatedTo<Account.accountID>))]
		[PXReferentialIntegrityCheck]
		public int? EarningsAcctID { get; set; }
		#endregion

		#region EarningsSubID
		public abstract class earningsSubID : PX.Data.BQL.BqlInt.Field<earningsSubID> { }
		[SubAccount(typeof(PRxINPostClass.earningsAcctID), DisplayName = "Earnings Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRxINPostClass.earningsSubID>.IsRelatedTo<Sub.subID>))]
		public int? EarningsSubID { get; set; }
		#endregion

		#region BenefitExpenseAcctID
		public abstract class benefitExpenseAcctID : PX.Data.BQL.BqlInt.Field<benefitExpenseAcctID> { }
		[Account(DisplayName = "Benefit Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<PRxINPostClass.benefitExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		public int? BenefitExpenseAcctID { get; set; }
		#endregion

		#region BenefitExpenseSubID
		public abstract class benefitExpenseSubID : PX.Data.BQL.BqlInt.Field<benefitExpenseSubID> { }
		[SubAccount(typeof(PRxINPostClass.benefitExpenseAcctID), DisplayName = "Benefit Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRxINPostClass.benefitExpenseSubID>.IsRelatedTo<Sub.subID>))]
		public int? BenefitExpenseSubID { get; set; }
		#endregion

		#region TaxExpenseAcctID
		public abstract class taxExpenseAcctID : PX.Data.BQL.BqlInt.Field<taxExpenseAcctID> { }
		[Account(DisplayName = "Tax Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<PRxINPostClass.taxExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		public int? TaxExpenseAcctID { get; set; }
		#endregion

		#region TaxExpenseSubID
		public abstract class taxExpenseSubID : PX.Data.BQL.BqlInt.Field<taxExpenseSubID> { }
		[SubAccount(typeof(PRxINPostClass.taxExpenseAcctID), DisplayName = "Tax Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRxINPostClass.taxExpenseSubID>.IsRelatedTo<Sub.subID>))]
		public int? TaxExpenseSubID { get; set; }
		#endregion
	}
}
