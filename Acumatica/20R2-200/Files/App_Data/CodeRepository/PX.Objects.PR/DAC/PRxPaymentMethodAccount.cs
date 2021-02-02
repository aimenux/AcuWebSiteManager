using PX.Data;
using PX.Objects.CA;
using PX.Objects.CS;

namespace PX.Objects.PR
{
	public class PRxPaymentMethodAccount : PXCacheExtension<PaymentMethodAccount>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region UseForPR
		public abstract class useForPR : PX.Data.BQL.BqlBool.Field<useForPR> { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Use in PR")]
		[PXUIVisible(typeof(Where<Parent<PRxPaymentMethod.useForPR>, Equal<True>>))]
		public virtual bool? UseForPR { get; set; }
		#endregion
	}
}