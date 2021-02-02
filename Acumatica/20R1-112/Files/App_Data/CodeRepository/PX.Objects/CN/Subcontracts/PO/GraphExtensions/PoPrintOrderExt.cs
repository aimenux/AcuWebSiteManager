using System.Collections;
using System.Linq;
using PX.Data;
using PX.Objects.CN.Subcontracts.SC.Graphs;
using PX.Objects.CS;
using PX.Objects.PO;

namespace PX.Objects.CN.Subcontracts.PO.GraphExtensions
{
    public class PoPrintOrderExt : PXGraphExtension<POPrintOrder>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public IEnumerable records(PXAdapter adapter)
        {
            return Base.records(adapter).OfType<PXResult>().Where(IsApplicable);
        }

        private bool IsApplicable(PXResult result)
        {
            var order = result.GetItem<POPrintOrder.POPrintOrderOwned>();
            var isSubcontract = order.OrderType == POOrderType.RegularSubcontract;
            return Base is PrintSubcontract
                ? isSubcontract
                : !isSubcontract;
        }
    }
}