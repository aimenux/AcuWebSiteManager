using PX.Data;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PRDirectDepositSplit)]
	public class PRDirectDepositSplit : IBqlTable
	{
		#region DocType
		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXUIField(DisplayName = "Payment Type")]
		[PXDBDefault(typeof(PRPayment.docType))]
		[PXParent(typeof(Select<PRPayment,
			Where<PRPayment.docType, Equal<Current<PRDirectDepositSplit.docType>>, 
				And<PRPayment.refNbr, Equal<Current<PRDirectDepositSplit.refNbr>>>>>))]
		public virtual string DocType { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion

		#region RefNbr
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Payment Reference Nbr.")]
		[PXDBDefault(typeof(PRPayment.refNbr))]
		public virtual string RefNbr { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion

		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		[PXDefault]
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion

		#region BankAcctNbr
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Account Nbr.")]
		[PXDefault]
		public virtual string BankAcctNbr { get; set; }
		public abstract class bankAcctNbr : PX.Data.BQL.BqlString.Field<bankAcctNbr> { }
		#endregion

		#region BankRoutingNbr
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Routing Nbr.")]
		[PXDefault]
		public virtual string BankRoutingNbr { get; set; }
		public abstract class bankRoutingNbr : PX.Data.BQL.BqlString.Field<bankRoutingNbr> { }
		#endregion

		#region BankAcctType
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		[BankAccountType.List]
		public string BankAcctType { get; set; }
		public abstract class bankAcctType : PX.Data.BQL.BqlString.Field<bankAcctType> { }
		#endregion

		#region BankName
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Name")]
		public string BankName { get; set; }
		public abstract class bankName : PX.Data.BQL.BqlString.Field<bankName> { }
		#endregion

		#region Amount
		[PRCurrency]
		[PXUIField(DisplayName = "Amount")]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual Decimal? Amount { get; set; }
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		#endregion

		#region CATranID
		[PXDBLong]
		[PRDirectDepositCashTranID]
		public virtual Int64? CATranID { get; set; }
		public abstract class caTranID : PX.Data.BQL.BqlLong.Field<caTranID> { }
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