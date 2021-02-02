using System.Collections;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.PO;
using PX.Objects.TX;

namespace PX.Objects.CN.Subcontracts.PO.GraphExtensions
{
    public class PoExternalTaxCalcExt : PXGraphExtension<POExternalTaxCalc>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public IEnumerable Items()
        {
            return new PXProcessingJoin<POOrder,
                InnerJoin<TaxZone, On<TaxZone.taxZoneID, Equal<POOrder.taxZoneID>>>,
                Where<TaxZone.isExternal, Equal<True>, And<POOrder.isTaxValid, Equal<False>,
                    And<POOrder.orderType, NotEqual<POOrderType.regularSubcontract>>>>>(Base).Select();
        }
    }
}