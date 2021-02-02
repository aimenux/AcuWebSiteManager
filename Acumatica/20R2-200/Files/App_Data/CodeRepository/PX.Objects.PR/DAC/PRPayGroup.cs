using PX.Data;
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL;
using System;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PRPayGroup)]
	[PXPrimaryGraph(typeof(PRPayGroupMaint))]
	[Serializable]
	public class PRPayGroup : IBqlTable
	{
		#region PayGroupID
		public abstract class payGroupID : BqlString.Field<payGroupID> { }
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Pay Group ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(PRPayGroup.payGroupID))]
		public string PayGroupID { get; set; }
		#endregion
		#region Description
		public abstract class description : BqlString.Field<description> { }
		[PXDBString(60, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Pay Group Name", Visibility = PXUIVisibility.SelectorVisible)]
		public string Description { get; set; }
		#endregion
		#region IsDefault
		public abstract class isDefault : BqlBool.Field<isDefault> { }
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Is Default")]
		public virtual Boolean? IsDefault { get; set; }
		#endregion
		#region EarningsAcctID
		public abstract class earningsAcctID : BqlInt.Field<earningsAcctID> { }
		[Account(DisplayName = "Earnings Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<PRPayGroup.earningsAcctID>.IsRelatedTo<Account.accountID>))]
		[PREarningAccountRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? EarningsAcctID { get; set; }
		#endregion
		#region EarningsSubID
		public abstract class earningsSubID : BqlInt.Field<earningsSubID> { }
		[SubAccount(typeof(PRPayGroup.earningsAcctID), DisplayName = "Earnings Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRPayGroup.earningsSubID>.IsRelatedTo<Sub.subID>))]
		[PREarningSubRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? EarningsSubID { get; set; }
		#endregion
		#region DedLiabilityAcctID
		public abstract class dedLiabilityAcctID : PX.Data.BQL.BqlInt.Field<dedLiabilityAcctID> { }
		[Account(DisplayName = "Deduction Liability Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<PRPayGroup.dedLiabilityAcctID>.IsRelatedTo<Account.accountID>))]
		[PRDedLiabilityAccountRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? DedLiabilityAcctID { get; set; }
		#endregion
		#region DedLiabilitySubID
		public abstract class dedLiabilitySubID : PX.Data.BQL.BqlInt.Field<dedLiabilitySubID> { }
		[SubAccount(typeof(PRPayGroup.dedLiabilityAcctID), DisplayName = "Deduction Liability Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRPayGroup.dedLiabilitySubID>.IsRelatedTo<Sub.subID>))]
		[PRDedLiabilitySubRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? DedLiabilitySubID { get; set; }
		#endregion
		#region BenefitExpenseAcctID
		public abstract class benefitExpenseAcctID : PX.Data.BQL.BqlInt.Field<benefitExpenseAcctID> { }
		[Account(DisplayName = "Benefit Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<PRPayGroup.benefitExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		[PRBenExpenseAccountRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? BenefitExpenseAcctID { get; set; }
		#endregion
		#region BenefitExpenseSubID
		public abstract class benefitExpenseSubID : PX.Data.BQL.BqlInt.Field<benefitExpenseSubID> { }
		[SubAccount(typeof(PRPayGroup.benefitExpenseAcctID), DisplayName = "Benefit Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRPayGroup.benefitExpenseSubID>.IsRelatedTo<Sub.subID>))]
		[PRBenExpenseSubRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? BenefitExpenseSubID { get; set; }
		#endregion
		#region BenefitLiabilityAcctID
		public abstract class benefitLiabilityAcctID : PX.Data.BQL.BqlInt.Field<benefitLiabilityAcctID> { }
		[Account(DisplayName = "Benefit Liability Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<PRPayGroup.benefitLiabilityAcctID>.IsRelatedTo<Account.accountID>))]
		[PRBenLiabilityAccountRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? BenefitLiabilityAcctID { get; set; }
		#endregion
		#region BenefitLiabilitySubID
		public abstract class benefitLiabilitySubID : PX.Data.BQL.BqlInt.Field<benefitLiabilitySubID> { }
		[SubAccount(typeof(PRPayGroup.benefitLiabilityAcctID), DisplayName = "Benefit Liability Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRPayGroup.benefitLiabilitySubID>.IsRelatedTo<Sub.subID>))]
		[PRBenLiabilitySubRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? BenefitLiabilitySubID { get; set; }
		#endregion
		#region TaxExpenseAcctID
		public abstract class taxExpenseAcctID : PX.Data.BQL.BqlInt.Field<taxExpenseAcctID> { }
		[Account(DisplayName = "Tax Expense Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description), AvoidControlAccounts = true)]
		[PXForeignReference(typeof(Field<PRPayGroup.taxExpenseAcctID>.IsRelatedTo<Account.accountID>))]
		[PRTaxExpenseAccountRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? TaxExpenseAcctID { get; set; }
		#endregion
		#region TaxExpenseSubID
		public abstract class taxExpenseSubID : PX.Data.BQL.BqlInt.Field<taxExpenseSubID> { }
		[SubAccount(typeof(PRPayGroup.taxExpenseAcctID), DisplayName = "Tax Expense Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRPayGroup.taxExpenseSubID>.IsRelatedTo<Sub.subID>))]
		[PRTaxExpenseSubRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? TaxExpenseSubID { get; set; }
		#endregion
		#region TaxLiabilityAcctID
		public abstract class taxLiabilityAcctID : PX.Data.BQL.BqlInt.Field<taxLiabilityAcctID> { }
		[Account(DisplayName = "Tax Liability Account", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Account.description))]
		[PXForeignReference(typeof(Field<PRPayGroup.taxLiabilityAcctID>.IsRelatedTo<Account.accountID>))]
		[PRTaxLiabilityAccountRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? TaxLiabilityAcctID { get; set; }
		#endregion
		#region TaxLiabilitySubID
		public abstract class taxLiabilitySubID : PX.Data.BQL.BqlInt.Field<taxLiabilitySubID> { }
		[SubAccount(typeof(PRPayGroup.taxLiabilityAcctID), DisplayName = "Tax Liability Sub.", Visibility = PXUIVisibility.Visible, DescriptionField = typeof(Sub.description))]
		[PXForeignReference(typeof(Field<PRPayGroup.taxLiabilitySubID>.IsRelatedTo<Sub.subID>))]
		[PRTaxLiabilitySubRequired(GLAccountSubSource.PayGroup)]
		public virtual Int32? TaxLiabilitySubID { get; set; }
		#endregion
		#region IsPayGroupIDFilled
		public abstract class isPayGroupIDFilled : BqlBool.Field<isPayGroupIDFilled> { }
		[PXBool]
		[PXUIField(DisplayName = "IsPayGroupIDFilled", Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXFormula(typeof(PRPayGroup.payGroupID.IsNotNull))]
		public virtual bool? IsPayGroupIDFilled { get; set; }
		#endregion
		#region NoteID
		public abstract class noteID : BqlGuid.Field<noteID> { }
		[PXNote]
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
