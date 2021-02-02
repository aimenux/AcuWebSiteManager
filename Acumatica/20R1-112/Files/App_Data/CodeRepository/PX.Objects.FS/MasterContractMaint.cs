using PX.Data;
using PX.Objects.AR;
using PX.Objects.CR;

namespace PX.Objects.FS
{
    public class MasterContractMaint : PXGraph<MasterContractMaint, FSMasterContract>
    {
        public MasterContractMaint()
        {
            PXUIFieldAttribute.SetDisplayName<BAccount.acctName>(BAccountRecords.Cache, TX.CustomTextFields.CUSTOMER_NAME);
            PXUIFieldAttribute.SetRequired<BAccount.acctName>(BAccountRecords.Cache, false);
        }

        #region Selects
        public PXSelect<BAccount> BAccountRecords;

        public PXSelectJoin<FSMasterContract,
               LeftJoin<BAccount,
                    On<BAccount.bAccountID, Equal<FSMasterContract.customerID>>,
               LeftJoin<Customer,
                    On<Customer.bAccountID, Equal<FSMasterContract.customerID>>>>,
                Where<Customer.bAccountID, IsNull,
                    Or<Match<Customer, Current<AccessInfo.userName>>>>>
               MasterContracts;
        #endregion
    }
}
