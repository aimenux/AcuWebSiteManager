using System;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.CM;
using PX.Objects.GL.FinPeriods;

namespace PX.Objects.CA
{
	[Serializable]
	//it is used in some reports 
	public partial class FinPeriodFilter : IBqlTable
	{
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }
		[FinPeriodID]
		[PXUIField(DisplayName = "Financial Period")]
		[PXSelector(typeof(MasterFinPeriod.finPeriodID), DescriptionField = typeof(MasterFinPeriod.descr))]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion
	}
}