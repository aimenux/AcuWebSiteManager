using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    // TODO: AC-137974 Delete this graph
    public class CustomerMaintBridge : PXGraph<CustomerMaintBridge, FSCustomer>
    {
        #region Selects
        public PXSelect<FSCustomer, 
               Where2<
                   Match<Current<AccessInfo.userName>>, 
               And<
                   Where<
                       FSCustomer.type, Equal<BAccountType.customerType>, 
                       Or<FSCustomer.type, Equal<BAccountType.combinedType>>>>>> Customers;
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<FSCustomer> e)
        {
            if (e.Row == null)
            {
                return;
            }

            FSCustomer fsCustomerRow = e.Row;

            var graph = PXGraph.CreateInstance<CustomerMaint>();

            if (fsCustomerRow.BAccountID >= 0)
            {
                graph.BAccount.Current = graph.BAccount.Search<Customer.bAccountID>(fsCustomerRow.BAccountID);
            }

            throw new PXRedirectRequiredException(graph, null) { Mode = PXBaseRedirectException.WindowMode.Same };
        }
        #endregion
    }
}
