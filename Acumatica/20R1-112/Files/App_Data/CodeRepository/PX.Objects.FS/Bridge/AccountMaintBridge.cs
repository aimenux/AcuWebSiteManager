using PX.Data;
using PX.Objects.AP;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.EP;
using System;

namespace PX.Objects.FS
{
    //TODO: AC-169956 Remove AccountMaintBridge
    [Obsolete("Remove in major release")]
    public class AccountMaintBridge : PXGraph<AccountMaintBridge, BAccountSelectorBase>
    {
        #region Selects
        public PXSelect<BAccountSelectorBase,
               Where2<
                   Match<Current<AccessInfo.userName>>,
                   And<
                       Where<
                           BAccount.type, Equal<BAccountType.customerType>,
                           Or<BAccount.type, Equal<BAccountType.prospectType>,
                           Or<BAccount.type, Equal<BAccountType.combinedType>,
                           Or<BAccount.type, Equal<BAccountType.vendorType>>>>>>>> BAccount;
        #endregion

        #region Event Handlers
        protected virtual void _(Events.RowSelected<BAccountSelectorBase> e)
        {
            if (e.Row == null)
            {
                return;
            }

            BAccountSelectorBase bAccountSelectorBaseRow = e.Row;

            if (bAccountSelectorBaseRow.BAccountID <= 0)
            {
                var graphCustomer = PXGraph.CreateInstance<CustomerMaint>();
                throw new PXRedirectRequiredException(graphCustomer, null) { Mode = PXBaseRedirectException.WindowMode.Same };
            }

            switch (bAccountSelectorBaseRow.Type)
            {
                case BAccountType.CustomerType:
                case BAccountType.CombinedType:

                    var graphCustomer = PXGraph.CreateInstance<CustomerMaint>();
                    graphCustomer.BAccount.Current = graphCustomer.BAccount.Search<Customer.bAccountID>(bAccountSelectorBaseRow.BAccountID);
                    throw new PXRedirectRequiredException(graphCustomer, null) { Mode = PXBaseRedirectException.WindowMode.Same };

                case BAccountType.VendorType:

                    var graphVendor = PXGraph.CreateInstance<VendorMaint>();
                    graphVendor.BAccount.Current = graphVendor.BAccount.Search<Vendor.bAccountID>(bAccountSelectorBaseRow.BAccountID);
                    throw new PXRedirectRequiredException(graphVendor, null) { Mode = PXBaseRedirectException.WindowMode.Same };

                case BAccountType.EmployeeType:
                case BAccountType.EmpCombinedType:

                    var graphEmployee = PXGraph.CreateInstance<EmployeeMaint>();
                    graphEmployee.Employee.Current = graphEmployee.Employee.Search<EPEmployee.bAccountID>(bAccountSelectorBaseRow.BAccountID);
                    throw new PXRedirectRequiredException(graphEmployee, null) { Mode = PXBaseRedirectException.WindowMode.Same };

                default:

                    var graphAccount = PXGraph.CreateInstance<BusinessAccountMaint>();
                    graphAccount.BAccount.Current = graphAccount.BAccount.Search<BAccount.bAccountID>(bAccountSelectorBaseRow.BAccountID);
                    throw new PXRedirectRequiredException(graphAccount, null) { Mode = PXBaseRedirectException.WindowMode.Same };
            }
        }
        #endregion
    }
}
