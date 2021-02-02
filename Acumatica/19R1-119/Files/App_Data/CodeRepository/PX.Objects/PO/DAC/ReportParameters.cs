using PX.Data;

namespace PX.Objects.PO.DAC
{
	public class ReportParameters: IBqlTable
	{
		#region FinPeriodID
		public abstract class finPeriodID : PX.Data.BQL.BqlString.Field<finPeriodID> { }

		[POOpenPeriod(typeof(AccessInfo.businessDate), null)]
		public string FinPeriodID { get; set; }
		#endregion
	}
}
