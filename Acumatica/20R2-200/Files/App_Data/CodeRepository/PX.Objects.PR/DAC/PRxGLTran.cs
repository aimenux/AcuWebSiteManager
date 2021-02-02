using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL;

namespace PX.Objects.PR
{
	public sealed class PRxGLTran : PXCacheExtension<GLTran>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region EarningTypeCD
		public abstract class earningTypeCD : PX.Data.BQL.BqlString.Field<earningTypeCD> { }
		[PXDBString(2, IsFixed = true, IsUnicode = true)]
		public string EarningTypeCD { get; set; }
		#endregion
		#region PayrollWorkLocationID
		public abstract class payrollWorkLocationID : PX.Data.BQL.BqlInt.Field<payrollWorkLocationID> { }
		[PXDBInt]
		public int? PayrollWorkLocationID { get; set; }
		#endregion
	}
}
