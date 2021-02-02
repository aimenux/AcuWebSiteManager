using PX.Data;
using System;

namespace PX.Objects.PR.Standalone
{
	[PXCacheName("Payroll Earning Type")]
	[Serializable]
	public partial class PREarningType : IBqlTable
	{
		#region TypeCD
		public abstract class typeCD : PX.Data.BQL.BqlString.Field<typeCD> { }
		[PXDefault]
		[PXDBString(2, IsUnicode = false, IsKey = true, InputMask = ">LL", IsFixed = true)]
		[PXUIField(DisplayName = "Code", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string TypeCD { get; set; }
		#endregion
	}
}
