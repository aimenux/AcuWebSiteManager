using PX.Data;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Objects.SO
{
	public class SOOrderPriceCalculationScope : Common.PriceCalculationScope<ARSalesPrice, ARSalesPrice.recordID>
	{
		public override bool IsPriceListExist()
		{
			if (IsUpdatedOnly<SOLine.orderQty>())
				return PXAccess.FeatureInstalled<FeaturesSet.supportBreakQty>();

			return base.IsPriceListExist();
		}
	}
}
