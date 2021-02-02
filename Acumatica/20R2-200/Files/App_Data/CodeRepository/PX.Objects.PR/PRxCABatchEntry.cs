using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CA;
using PX.Objects.CS;
using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.PR
{
	public class PRxCABatchEntry : PXGraphExtension<CABatchEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRemoveBaseAttribute(typeof(PXSelectorAttribute))]
		[PXSelector(typeof(SearchFor<PaymentMethod.paymentMethodID>
			.Where<PaymentMethod.aPCreateBatchPayment.IsEqual<True>
				.Or<PRxPaymentMethod.prCreateBatchPayment.IsEqual<True>>>))]
		public virtual void _(Events.CacheAttached<CABatch.paymentMethodID> e) { }
	}
}