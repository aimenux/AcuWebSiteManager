using PX.Data;
using PX.Data.BQL.Fluent;
using System;

namespace PX.Objects.PR
{
	[Serializable]
	[PXCacheName(Messages.PREmployeePTOHistory)]
	[Obsolete]
	public class PREmployeePTOHistory : IBqlTable
	{
		#region RecordID
		[PXDBIdentity(IsKey = true)]
		[PXUIField(DisplayName = "Record ID", Visibility = PXUIVisibility.Invisible)]
		public virtual int? RecordID { get; set; }
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		#endregion

		#region DocType
		[PXDBString(3, IsFixed = true)]
		[PXUIField(DisplayName = "Type")]
		[PayrollType.List]
		public string DocType { get; set; }
		public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
		#endregion

		#region RefNbr
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Reference Nbr.")]
		public String RefNbr { get; set; }
		public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
		#endregion

		#region BAccountID
		[PXDBInt]
		[PXUIField(DisplayName = "Employee")]
		public virtual int? BAccountID { get; set; }
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		#endregion

		#region BankID
		[PXDBString(3, IsUnicode = true)]
		[PXUIField(DisplayName = "Bank")]
		public virtual string BankID { get; set; }
		public abstract class bankID : PX.Data.BQL.BqlString.Field<bankID> { }
		#endregion

		#region Type
		[PXDBString(1, IsUnicode = true)]
		[PXUIField(DisplayName = "Type")]
		[PTOHistoryType.List]
		public virtual string Type { get; set; }
		public abstract class type : PX.Data.BQL.BqlString.Field<type> { }
		#endregion

		#region Date
		[PXDBDate]
		[PXUIField(DisplayName = "Date")]
		public virtual DateTime? Date { get; set; }
		public abstract class date : PX.Data.BQL.BqlDateTime.Field<date> { }
		#endregion

		#region Amount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Amount")]
		public virtual Decimal? Amount { get; set; }
		public abstract class amount : PX.Data.BQL.BqlDecimal.Field<amount> { }
		#endregion

		#region IsFrontLoading
		[PXDBBool]
		[PXUIField(DisplayName = "Is Front Loading")]
		[PXDefault(false)]
		public virtual bool? IsFrontLoading { get; set; }
		public abstract class isFrontLoading : PX.Data.BQL.BqlBool.Field<isFrontLoading> { }
		#endregion

		#region IsCarryover
		[PXDBBool]
		[PXUIField(DisplayName = "Is Carryover")]
		[PXDefault(false)]
		public virtual bool? IsCarryover { get; set; }
		public abstract class isCarryover : PX.Data.BQL.BqlBool.Field<isCarryover> { }
		#endregion
	}
}