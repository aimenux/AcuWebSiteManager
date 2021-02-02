using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Objects.IN;

namespace PX.Objects.PR
{
	public sealed class PRxInventoryItem : PXCacheExtension<InventoryItem>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region EarningsAcctID
		public abstract class earningsAcctID : PX.Data.BQL.BqlInt.Field<earningsAcctID> { }
		[Account(DisplayName = "Earnings Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<PRxINPostClass.earningsAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.earningsAcctID>.IsRelatedTo<Account.accountID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PXReferentialIntegrityCheck]
		[PREarningAccountRequired(GLAccountSubSource.LaborItem)]
		public int? EarningsAcctID { get; set; }
		#endregion

		#region EarningsSubID
		public abstract class earningsSubID : PX.Data.BQL.BqlInt.Field<earningsSubID> { }
		[SubAccount(typeof(PRxInventoryItem.earningsAcctID), DisplayName = "Earnings Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<PRxINPostClass.earningsSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.earningsSubID>.IsRelatedTo<Sub.subID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PREarningSubRequired(GLAccountSubSource.LaborItem)]
		public int? EarningsSubID { get; set; }
		#endregion

		#region BenefitExpenseAcctID
		public abstract class benefitExpenseAcctID : PX.Data.BQL.BqlInt.Field<benefitExpenseAcctID> { }
		[Account(DisplayName = "Benefit Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<PRxINPostClass.benefitExpenseAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.benefitExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRBenExpenseAccountRequired(GLAccountSubSource.LaborItem)]
		public int? BenefitExpenseAcctID { get; set; }
		#endregion

		#region BenefitExpenseSubID
		public abstract class benefitExpenseSubID : PX.Data.BQL.BqlInt.Field<benefitExpenseSubID> { }
		[SubAccount(typeof(PRxInventoryItem.benefitExpenseAcctID), DisplayName = "Benefit Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<PRxINPostClass.benefitExpenseSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.benefitExpenseSubID>.IsRelatedTo<Sub.subID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRBenExpenseSubRequired(GLAccountSubSource.LaborItem)]
		public int? BenefitExpenseSubID { get; set; }
		#endregion

		#region TaxExpenseAcctID
		public abstract class taxExpenseAcctID : PX.Data.BQL.BqlInt.Field<taxExpenseAcctID> { }
		[Account(DisplayName = "Tax Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXDefault(typeof(Search<PRxINPostClass.taxExpenseAcctID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.taxExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRTaxExpenseAccountRequired(GLAccountSubSource.LaborItem)]
		public int? TaxExpenseAcctID { get; set; }
		#endregion

		#region TaxExpenseSubID
		public abstract class taxExpenseSubID : PX.Data.BQL.BqlInt.Field<taxExpenseSubID> { }
		[SubAccount(typeof(PRxInventoryItem.taxExpenseAcctID), DisplayName = "Tax Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXDefault(typeof(Search<PRxINPostClass.taxExpenseSubID, Where<INPostClass.postClassID, Equal<Current<InventoryItem.postClassID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[PXFormula(typeof(Default<InventoryItem.postClassID>))]
		[PXForeignReference(typeof(Field<PRxInventoryItem.taxExpenseSubID>.IsRelatedTo<Sub.subID>))]
		[PXUIVisible(typeof(Where<InventoryItem.itemType.FromCurrent.IsEqual<INItemTypes.laborItem>>))]
		[PRTaxExpenseSubRequired(GLAccountSubSource.LaborItem)]
		public int? TaxExpenseSubID { get; set; }
		#endregion
	}
}
