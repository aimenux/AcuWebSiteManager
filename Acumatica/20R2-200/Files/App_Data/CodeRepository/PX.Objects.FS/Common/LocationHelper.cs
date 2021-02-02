using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class LocationHelper
    {
        public static void OpenCustomerLocation(PXGraph graph, int? soID)
        {
            FSServiceOrder fsServiceOrderRow = PXSelect<FSServiceOrder,
                                               Where<
                                                    FSServiceOrder.sOID, Equal<Required<FSServiceOrder.sOID>>>>
                                               .Select(graph, soID);

            CustomerLocationMaint graphCustomerLocationMaint = PXGraph.CreateInstance<CustomerLocationMaint>();

            BAccount bAccountRow =
                      PXSelect<BAccount,
                      Where<
                          BAccount.bAccountID, Equal<Required<BAccount.bAccountID>>>>
                      .Select(graph, fsServiceOrderRow.CustomerID);

            graphCustomerLocationMaint.Location.Current = graphCustomerLocationMaint.Location.Search<Location.locationID>
                                                            (fsServiceOrderRow.LocationID, bAccountRow.AcctCD);

            throw new PXRedirectRequiredException(graphCustomerLocationMaint, null) { Mode = PXBaseRedirectException.WindowMode.NewWindow };
        }
    }
}
