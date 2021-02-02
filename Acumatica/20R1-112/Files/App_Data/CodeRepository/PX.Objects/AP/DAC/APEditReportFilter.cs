using System;
using PX.Data;
using PX.Objects.Common;
using PX.Objects.CS;

namespace PX.Objects.AP
{
	public class APEditReportFilter : IBqlTable
	{
		#region Prebooked
		public abstract class prebooked : PX.Data.BQL.BqlBool.Field<prebooked> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Include Pre-Released Transactions", FieldClass = "Prebooking")]
		public virtual bool? Prebooked
		{
			get;
			set;
		}
		#endregion
	}
}
