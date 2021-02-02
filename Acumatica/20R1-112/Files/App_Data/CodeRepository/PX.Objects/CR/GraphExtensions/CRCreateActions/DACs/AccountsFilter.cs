using System;
using PX.Data;

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[Serializable]
	[PXHidden]
	public class AccountsFilter : IBqlTable, IClassIdFilter
	{
		#region BAccountID

		public abstract class bAccountID : PX.Data.BQL.BqlString.Field<bAccountID> { }

		[PXDefault]
		[PXDBString]
		[PXDimension(BAccountAttribute.DimensionName)]
		[PXUIField(DisplayName = "Business Account ID", Required = true)]
		public virtual string BAccountID { get; set; }

		#endregion

		#region AccountName

		public abstract class accountName : PX.Data.BQL.BqlString.Field<accountName> { }

		[PXDefault]
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Business Account Name", Required = true)]
		public virtual string AccountName { get; set; }

		#endregion

		#region AccountClass

		public abstract class accountClass : PX.Data.BQL.BqlString.Field<accountClass> { }

		[PXDBString(10, IsUnicode = true, InputMask = ">aaaaaaaaaa")]
		[PXUIField(DisplayName = "Business Account Class")]
		[PXSelector(typeof(CRCustomerClass.cRCustomerClassID))]
		public virtual string AccountClass { get; set; }

		string IClassIdFilter.ClassID => AccountClass;

		#endregion

		#region LinkContactToAccount

		public abstract class linkContactToAccount : PX.Data.BQL.BqlBool.Field<linkContactToAccount> { }

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Link Contact to Account", Visible = false, Enabled = false)]
		public virtual bool? LinkContactToAccount { get; set; }

		#endregion
	}
}