using System;
using PX.Data;
using PX.Objects.EP;

namespace PX.Objects.PR
{
	[PXCacheName(Messages.PREmployeeDirectDeposit)]
	[Serializable]
	public class PREmployeeDirectDeposit : IBqlTable, ISortOrder
	{
		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		[PXDBInt(IsKey = true)]
		[PXDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<PREmployeeDirectDeposit.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		#endregion
		#region LineNbr
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Line Nbr.", Visible = false)]
		[PXDefault]
		[PXLineNbr(typeof(PREmployee))]
		public virtual int? LineNbr { get; set; }
		public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion
		#region BankAcctNbr
		public abstract class bankAcctNbr : PX.Data.BQL.BqlString.Field<bankAcctNbr> { }
		[PXDBString(30, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Account Number")]
		public string BankAcctNbr { get; set; }
		#endregion
		#region BankRoutingNbr
		public abstract class bankRoutingNbr : PX.Data.BQL.BqlString.Field<bankRoutingNbr> { }
		[PXDBString(30, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Routing Number")]
		[PXDefault]
		public string BankRoutingNbr { get; set; }
		#endregion
		#region BankAcctType
		public abstract class bankAcctType : PX.Data.BQL.BqlString.Field<bankAcctType> { }
		[PXDBString(3, IsFixed = true)]
		[PXDefault(BankAccountType.Checking)]
		[PXUIField(DisplayName = "Type")]
		[BankAccountType.List]
		public string BankAcctType { get; set; }
		#endregion
		#region BankName
		public abstract class bankName : PX.Data.BQL.BqlString.Field<bankName> { }
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank Name")]
		[PXDefault]
		public string BankName { get; set; }
		#endregion
		#region Amount
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		[PRCurrency(MinValue = 0)]
		[PXUIField(DisplayName = "Amount")]
		public decimal? Amount { get; set; }
		#endregion
		#region Percent
		public abstract class percent : PX.Data.BQL.BqlDecimal.Field<percent> { }
		[PXDBDecimal(MinValue = 0, MaxValue = 100)]
		[PXUIField(DisplayName = "Percent")]
		public decimal? Percent { get; set; }
		#endregion
		#region GetsRemainder
		public abstract class getsRemainder : PX.Data.BQL.BqlBool.Field<getsRemainder> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Gets Remainder")]
		public bool? GetsRemainder { get; set; }
		#endregion
		#region SortOrder
		[PXDBInt(MinValue = 1)]
		[PXUIField(DisplayName = "Sequence")]
		[PXDefault]
		[PXCheckUnique(typeof(PREmployeeDirectDeposit.bAccountID))]
		public virtual int? SortOrder { get; set; }
		public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }
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
