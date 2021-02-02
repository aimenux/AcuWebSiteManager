using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.DR
{
	public class ARSalesPriceMaintASC606 : PXGraphExtension<ARSalesPriceMaint>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.aSC606>();
		}

		public override void Initialize()
		{
			PXUIFieldAttribute.SetVisible<ARSalesPrice.isFairValue>(Base.Records.Cache, null, true);
			PXUIFieldAttribute.SetVisible<ARSalesPrice.isProrated>(Base.Records.Cache, null, true);
			PXUIFieldAttribute.SetVisibility<ARSalesPrice.isFairValue>(Base.Records.Cache, null, PXUIVisibility.SelectorVisible);
			PXUIFieldAttribute.SetVisibility<ARSalesPrice.isProrated>(Base.Records.Cache, null, PXUIVisibility.SelectorVisible);
		}

		protected virtual void _(Events.RowSelected<ARSalesPrice> e)
		{
			if (e.Row == null)
				return;

			PXUIFieldAttribute.SetEnabled<ARSalesPrice.isPromotionalPrice>(e.Cache, e.Row, e.Row.IsFairValue != true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.isFairValue>(e.Cache, e.Row, e.Row.IsPromotionalPrice != true);
			PXUIFieldAttribute.SetEnabled<ARSalesPrice.isProrated>(e.Cache, e.Row, e.Row.IsFairValue == true);

		}

		protected virtual void _(Events.FieldUpdated<ARSalesPrice, ARSalesPrice.isFairValue> e)
		{
			if (e.Row == null)
				return;

			if (e.Row.IsPromotionalPrice == false && (bool)e.OldValue == true)
			{
				e.Row.IsProrated = false;
			}
		}
	}
}
